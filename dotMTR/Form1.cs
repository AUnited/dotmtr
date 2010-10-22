/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Configuration;
using Amib.Threading;
using ZedGraph;
using Microsoft.Test.CommandLineParsing;
using MyQueues;


namespace dotMTR
{
	public partial class Form1 : Form
	{
		/// <summary>
		/// Application configuration
		/// </summary>
		int _MinTTL = Properties.Settings.Default.MinTTL;
		public int MinTTL
		{
			get { return _MinTTL; }
			set { _MinTTL = value; }
		}

		int _MaxTTL = Properties.Settings.Default.MaxTTL;
		public int MaxTTL
		{
			get { return _MaxTTL; }
			set { _MaxTTL = value; }
		}

		int _TraceInterval = Properties.Settings.Default.TraceInterval;
		public int TraceInterval
		{
			get { return _TraceInterval; }
			set { _TraceInterval = value; }
		}

		int _PingInterval = Properties.Settings.Default.PingInterval;
		public int PingInterval
		{
			get { return _PingInterval; }
			set { _PingInterval = value; }
		}

		int _PingTimout = Properties.Settings.Default.PingTimeout;
		public int PingTimeout
		{
			get { return _PingTimout; }
			set { _PingTimout = value; }
		}

		Color _ProgressColor = Properties.Settings.Default.ProgressColor;
		public Color ProgressColor
		{
			get { return _ProgressColor; }
			set { _ProgressColor = value; }
		}

		Color _UpProgressColor = Properties.Settings.Default.UpProgressColor;
		public Color UpProgressColor
		{
			get { return _UpProgressColor; }
			set { _UpProgressColor = value; }
		}

		Color _DownProgressColor = Properties.Settings.Default.DownProgressColor;
		public Color DownProgressColor
		{
			get { return _DownProgressColor; }
			set { _DownProgressColor = value; }
		}

		/// <summary>
		/// Delegates
		/// </summary>
		delegate void UpdateMTRDelegate();
		delegate bool HopCheckDelegate(IPAddress _mtrHop, IPAddress _hop);


		/// <summary>
		/// Runtime processing variables
		/// </summary>
		string formTitle;
		bool traceActive = false;
		MouseEventArgs lastMouseDown;
		MouseEventArgs lastMouseUp;
		DateTime lastMouseDownTime;
		DateTime lastMouseUpTime;
		Dictionary<int, DotHop> mtrHops = new Dictionary<int, DotHop>();
		SmartThreadPool tracePool = new SmartThreadPool();
		BlockingQueue traceQueue = new MyQueues.BlockingQueue(1024);
		GridLayout gl;
		ColorMap cm;

		/// <summary>
		/// Defines the main form
		/// </summary>
		/// <param name="_args"></param>
		public Form1(string[] _args)
		{
			// Process command line args
			CmdLineArgs clArgs = new CmdLineArgs(_args);

			//traceQueue.Enqueued += new MyQueues.BlockingQueue.EnqueuedEventHandler(UpdateStats);

			InitializeComponent();
			InitializeValues();

			this.formTitle = this.Text;
			this.comboBox2.Text = MaxTTL.ToString();
			this.ActiveControl = comboBox1;

			// Populate trace host combobox
			if (clArgs.traceDest != null && clArgs.traceDest.Length > 0)
			{
				comboBox1.Text = clArgs.traceDest;
			}
		}

		/// <summary>
		/// Initialize default values
		/// </summary>
		private void InitializeValues()
		{
			this.progressIndicator1.CircleColor = ProgressColor;

			Dictionary<string, int> columnMap = new Dictionary<string, int>();
			columnMap.Add("Hop", 20);
			columnMap.Add("TTL", 5);
			columnMap.Add("Result", 10);
			columnMap.Add("Sent", 7);
			columnMap.Add("Received", 7);
			columnMap.Add("Packet Loss", 11);
			columnMap.Add("Last", 10);
			columnMap.Add("Best", 10);
			columnMap.Add("Worst", 10);
			columnMap.Add("Average", 10);

			this.gl = new GridLayout(columnMap);
			this.cm = new ColorMap(MaxTTL);

			this.mtrHops = new Dictionary<int, DotHop>();

			this.gl.Layout(ref grid1);
			DrawTraceGraph(ref zedGraphControl1);
		}

		/// <summary>
		/// The main MTR start method
		/// </summary>
		/// <param name="_destIP"></param>
		/// <param name="_maxTTL"></param>
		void StartMTR(IPAddress _destIP, int _maxTTL)
		{
			// Start the thread pool
			tracePool.Start();

			// Perpetually trace as long as traceActive is true
			while (traceActive)
			{
				// Add the trace result to the trace queue
				traceQueue.Enqueue(DoDotTrace(_destIP, _maxTTL));

				// Update trace statistics
				UpdateMTR();

				// Sleep for the duration of the traceInterval
				System.Threading.Thread.Sleep(TraceInterval);
			}
		}

		/// <summary>
		/// The main MTR start method (overloaded)
		/// </summary>
		/// <param name="_destIP"></param>
		public void StartMTR(IPAddress _destIP)
		{
			StartMTR(_destIP, this.MaxTTL);
		}

		/// <summary>
		/// The main MTR stop method
		/// </summary>
		void StopMTR()
		{
			// Discontinue thread pool processing
			tracePool.Cancel(true);
			tracePool.WaitForIdle();
			GC.Collect();
		}

		/// <summary>
		/// The main Trace method
		/// </summary>
		/// <param name="_destIP"></param>
		/// <param name="_maxTTL"></param>
		/// <returns></returns>
		List<DotHop> DoDotTrace(IPAddress _destIP, int _maxTTL)
		{
			// Create a DotTrace return object
			List<DotHop> dt = new List<DotHop>();

			// Create a SmartThreadPool
			SmartThreadPool hopPool = new SmartThreadPool();

			// Create a work item result list
			List<IWorkItemResult> hopWIResults = new List<IWorkItemResult>();

			// Queue a work item for each hop
			for (int i = 1; _maxTTL >= i; i++)
			{
				IWorkItemResult wir = hopPool.QueueWorkItem(new WorkItemCallback(DoDotHopCallback), new DotHop(_destIP, i));

				hopWIResults.Add(wir);

				// Sleep for the duration of the pingInterval
				System.Threading.Thread.Sleep(PingInterval);
			}

			// Wait for tasks to complete
			hopPool.WaitForIdle();

			// Shut down the hop pool
			hopPool.Shutdown();
			hopPool.Dispose();

			// Collate the results
			for (int i = 0; hopWIResults.Count > i; i++)
			{
				dt.Add((DotHop)hopWIResults[i].Result);
				if (dt[i].IsLastHop) goto LASTHOP;
			}

			LASTHOP:
			return dt;
		}

		/// <summary>
		/// Callback method for receiving the trace results from the thread pool
		/// </summary>
		/// <param name="_dh"></param>
		/// <returns></returns>
		Object DoDotHopCallback(Object _dh)
		{
			DotHop dh = (DotHop)_dh;

			return DoDotHop(dh.destIP, dh.ttl);
		}

		/// <summary>
		/// The main hop method
		/// </summary>
		/// <param name="_destIP"></param>
		/// <param name="_ttl"></param>
		/// <returns></returns>
		DotHop DoDotHop(IPAddress _destIP, int _ttl)
		{
			DotHop dh = new DotHop(_destIP, _ttl);
			byte[] pingPayload = System.Text.Encoding.UTF8.GetBytes(Properties.Settings.Default.PingPayload);

			using (Ping pingSender = new Ping())
			{
				try
				{
					// Create a stopwatch to time and timeout round trips
					Stopwatch stopWatch = new Stopwatch();
					stopWatch.Start();

					// Create PingOptions object with the requisite parameters
					PingOptions pingOptions = new PingOptions() { DontFragment = true, Ttl = _ttl };
					PingReply pingReply = pingSender.Send(_destIP, this.PingTimeout, pingPayload, pingOptions);

					// Stop our timer
					stopWatch.Stop();

					// Return a null triptime for timeout instances
					if (pingReply.Status == IPStatus.TimedOut)
					{
						dh.tripTime = null;
					}

					// Otherwise return the elapsed triptime
					else
					{
						dh.tripTime = stopWatch.Elapsed.TotalMilliseconds;
					}

					// Timestamp the trip and return the sender and status
					dh.timeStamp = DateTime.Now;
					dh.hopIP = pingReply.Address;
					dh.status = pingReply.Status;
				}

				catch (Exception ex)
				{
					// TODO - canceling via threadpool results in an exception?
					throw ex;
				}

				return dh;
			}
		}

		/// <summary>
		/// Hides / unhides detail and graph panes
		/// </summary>
		private void ShowHideTraceDetailGraph()
		{
			if (traceDetailToolStripMenuItem.Checked) { splitContainer1.Panel1Collapsed = false; }
			else { splitContainer1.Panel1Collapsed = true; }

			if (traceGraphToolStripMenuItem.Checked) { splitContainer1.Panel2Collapsed = false; }
			else { splitContainer1.Panel2Collapsed = true; }

			if (traceDetailToolStripMenuItem.Checked || traceGraphToolStripMenuItem.Checked)
			{
				splitContainer1.Visible = true;
			}

			else
			{
				splitContainer1.Visible = false;
			}
		}

		/// <summary>
		/// Starts the MTR running
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button1_Click(object sender, EventArgs e)
		{
			// Cleanup
			StopMTR();

			// Save our trace parameters for later
			if (comboBox1.Text.Length > 0) comboBox1.Items.Add(comboBox1.Text);
			if (comboBox2.Text.Length > 0) comboBox2.Items.Add(comboBox2.Text);

			// Check TTL input for sanity
			try
			{
				try
				{
					int _maxTTL = Convert.ToInt32(comboBox2.Text);
					if (_maxTTL >= 1 && _maxTTL <= 128) MaxTTL = _maxTTL;
					else throw new Exception("Max TTL must be between 1 and 128.");
				}

				catch (Exception ex)
				{
					throw new Exception("Max TTL must be an integer (" + ex.Message + ")");
				}

				InitializeValues();
				traceActive = true;

				// Resolve the host name and queue the work
				tracePool.QueueWorkItem(StartMTR, Dns.GetHostEntry(comboBox1.Text).AddressList[0], Convert.ToInt32(comboBox2.Text));

				// Set various control states as appropriate
				button1.Enabled = false;
				button2.Enabled = true;
				comboBox1.Enabled = false;
				comboBox2.Enabled = false;
				progressIndicator1.Start();

				this.Text = formTitle + " - Tracing route to host: " + comboBox1.Text;
			}

			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message);
			}
		}

		/// <summary>
		/// Stops the MTR
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button2_Click(object sender, EventArgs e)
		{
			StopMTR();

			// Set various control states as appropriate
			comboBox1.Enabled = true;
			comboBox2.Enabled = true;
			button1.Enabled = true;
			button2.Enabled = false;
			progressIndicator1.Stop();
			this.Text = formTitle;
		}

		/// <summary>
		/// Quits the program
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		/// <summary>
		/// Watch for enter key being pressed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if ((Keys)e.KeyChar == Keys.Enter) button1_Click(this, EventArgs.Empty);
		}

		/// <summary>
		/// Watch for enter key being pressed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void comboBox2_KeyPress(object sender, KeyPressEventArgs e)
		{
			if ((Keys)e.KeyChar == Keys.Enter) button1_Click(this, EventArgs.Empty);
		}

		/// <summary>
		/// Displayes an about form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.Show();
		}

		/// <summary>
		/// Catch resize events and rescale the grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void grid1_Resize(object sender, EventArgs e)
		{
			this.gl.Layout(ref this.grid1);
		}

		/// <summary>
		/// Show or hide the detail panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void traceDetailToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
		{
			ShowHideTraceDetailGraph();
		}

		/// <summary>
		/// Show or hide the graph panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void traceGraphToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
		{
			ShowHideTraceDetailGraph();
		}

		/// <summary>
		/// Watch for mouse clicks to be able to determine when to show text objects
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool zedGraphControl1_MouseDownEvent(ZedGraphControl sender, MouseEventArgs e)
		{
			this.lastMouseDown = e;
			this.lastMouseDownTime = DateTime.Now;
			return default(bool);
		}

		/// <summary>
		/// Watch for mouse clicks to be able to determine when to show text objects
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool zedGraphControl1_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
		{
			this.lastMouseUp = e;
			this.lastMouseUpTime = DateTime.Now;

			TimeSpan delta = new TimeSpan((this.lastMouseUpTime - this.lastMouseDownTime).Ticks);

			if ((e.Button == MouseButtons.Left) && (delta < new TimeSpan(0, 0, 0, 0, 1000)))
			{
				PointF clickPF = new PointF(e.X, e.Y);
				CurveItem nearCurve;
				int pointIndex;

				int index;
				if (sender.GraphPane.GraphObjList.FindPoint(clickPF, sender.GraphPane, Graphics.FromHwnd(sender.Handle), sender.GraphPane.CalcScaleFactor(), out index))
				{
					sender.GraphPane.GraphObjList.RemoveAt(index);
					sender.Refresh();
				}

				else if (sender.GraphPane.FindNearestPoint(clickPF, out nearCurve, out pointIndex))
				{
					try
					{
						TextObj textObj = (TextObj)sender.GraphPane.GraphObjList[nearCurve.Points[pointIndex].GetHashCode().ToString()];

						if (textObj != null)
						{
							sender.GraphPane.GraphObjList.Remove(textObj);
						}

						else
						{
							string hopText = nearCurve.Points[pointIndex].Tag.ToString();
							double positionX = nearCurve.Points[pointIndex].X;
							double positionY = nearCurve.Points[pointIndex].Y;
							//double offsetX = 2.0;
							//double offsetY = 2.0;

							TextObj hopObj = new TextObj(
									hopText,
									positionX,
									positionY,
									CoordType.AxisXYScale,
									AlignH.Left,
									AlignV.Bottom
									);

							hopObj.Tag = nearCurve.Points[pointIndex].GetHashCode().ToString();
							hopObj.ZOrder = ZOrder.A_InFront;
							hopObj.FontSpec.Border.IsVisible = true;
							hopObj.FontSpec.Fill.IsVisible = true;
							hopObj.Text = hopText.ToString();
							hopObj.IsClippedToChartRect = true;

							sender.GraphPane.GraphObjList.Add(hopObj);
						}
					}

					catch (Exception ex)
					{
						// Ignore
					}

					sender.Refresh();
				}
			}

			return default(bool);
		}

		/// <summary>
		/// Customize the graph's context menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="menuStrip"></param>
		/// <param name="mousePt"></param>
		/// <param name="objState"></param>
		private void zedGraphControl1_ContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt, ZedGraphControl.ContextMenuObjectState objState)
		{
			string menuText;

			for (int i = 0; sender.ContextMenuStrip.Items.Count > i; i++)
			{
				switch (sender.ContextMenuStrip.Items[i].Tag.ToString())
				{
					case ("set_default"):
						menuText = "Show All Available Data (Pause Updating)";
						sender.ContextMenuStrip.Items[i].Text = menuText;
						break;

					case ("unzoom"):
						menuText = "Revert to Previous View";
						sender.ContextMenuStrip.Items[i].Text = menuText;
						break;

					case ("show_val"):
						menuText = "Show / Hide Tooltips";
						sender.ContextMenuStrip.Items[i].Text = menuText;
						break;

					case ("undo_all"):
						menuText = "Reset to Default View (Resume Updating)";
						sender.ContextMenuStrip.Items[i].Text = menuText;
						break;
				}
			}
		}

		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{

		}

		private void zedGraphControl1_Paint(object sender, PaintEventArgs e)
		{

		}

		//private void zedGraphControl1_MouseMove(object sender, MouseEventArgs e)
		//{
		//    PointF lastPF = new PointF(e.X, e.Y);
		//    CurveItem nearCurve;
		//    int pointIndex;

		//    if (sender.GraphPane.FindNearestPoint(lastPF, out nearCurve, out pointIndex))
		//    {
		//    }
		//}

		/// <summary>
		/// Change the cursor to give visual feedback that the point can be clicked on 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="pane"></param>
		/// <param name="curve"></param>
		/// <param name="iPt"></param>
		/// <returns></returns>
		private string zedGraphControl1_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
		{
			sender.Cursor = Cursors.Hand;

			return curve[iPt].Tag.ToString();
		}

		/// <summary>
		/// Stop the MTR process when the form closes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			StopMTR();
		}
	}
}
