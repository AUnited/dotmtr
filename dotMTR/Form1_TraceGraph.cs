/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Net.NetworkInformation;
using ZedGraph;

namespace dotMTR
{
	public partial class Form1 : Form
	{
		private static int _GraphSpanSeconds = Properties.Settings.Default.GraphSpanSeconds;
		public  static int GraphSpanSeconds
		{
			get { return _GraphSpanSeconds; }
			set { _GraphSpanSeconds = value; }
		}

		private double _GraphXAxisMin = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, GraphSpanSeconds, 0)).ToOADate();
		public  double GraphXAxisMin
		{
			get { return _GraphXAxisMin; }
			set { _GraphXAxisMin = value; }
		}

		private static double _GraphXAxisMax = DateTime.Now.ToOADate();
		public static double GraphXAxisMax
		{
			get	{ return DateTime.Now.ToOADate(); }
			set { _GraphXAxisMax = value; }
		}

		private double _GraphYAxisMin = Properties.Settings.Default.GraphYAxisMin;
		public double GraphYAxisMin
		{
			get { return _GraphYAxisMin; }
			set { _GraphYAxisMin = value; }
		}

		private double _GraphYAxisMax = Properties.Settings.Default.GraphYAxisMax;
		public double GraphYAxisMax
		{
			get { return _GraphYAxisMax; }
			set { _GraphYAxisMax = value; }
		}

		delegate void UpdateTraceGraphDelegate(ref ZedGraph.ZedGraphControl _zgc, List<DotHop> _dh);

		private void DrawTraceGraph(ref ZedGraphControl _zgc)
		{
			while (_zgc.GraphPane.CurveList.Count > 0)
			{
				_zgc.GraphPane.CurveList.Remove(_zgc.GraphPane.CurveList.Last());
			}

			while (_zgc.GraphPane.GraphObjList.Count > 0)
			{
				_zgc.GraphPane.GraphObjList.Remove(_zgc.GraphPane.GraphObjList.Last());
			}

			//zgc.GraphPane.Margin.All = 2;
			_zgc.GraphPane.IsFontsScaled = false;
			_zgc.GraphPane.Title.Text = "Trace Graph";
			_zgc.GraphPane.Title.IsVisible = false;
			_zgc.GraphPane.Legend.IsVisible = false;
			_zgc.GraphPane.Chart.Fill = new Fill(Color.LightGray, Color.WhiteSmoke);
			//_zgc.GraphPane.Fill = new Fill(Color.WhiteSmoke, Color.LightGray);
			_zgc.GraphPane.Fill = new Fill(Color.WhiteSmoke);

			_zgc.GraphPane.XAxis.Title.Text = "Time";
			_zgc.GraphPane.XAxis.Title.IsVisible = false;
			_zgc.GraphPane.XAxis.Type = AxisType.Date;
			_zgc.GraphPane.XAxis.Scale.MajorUnit = DateUnit.Second;
			_zgc.GraphPane.XAxis.Scale.MinorUnit = DateUnit.Second;
			_zgc.GraphPane.XAxis.Scale.MajorStep = 1.0;
			_zgc.GraphPane.XAxis.Scale.MinorStep = 1.0;
			_zgc.GraphPane.XAxis.Scale.Min = GraphXAxisMin;
			_zgc.GraphPane.XAxis.Scale.Max = GraphXAxisMax;
			_zgc.GraphPane.XAxis.Scale.MinAuto = false;
			_zgc.GraphPane.XAxis.Scale.MaxAuto = false;
			_zgc.GraphPane.XAxis.Scale.MinGrace = 0;
			_zgc.GraphPane.XAxis.Scale.MaxGrace = 0;
			_zgc.GraphPane.XAxis.IsVisible = false;

			_zgc.GraphPane.YAxis.Title.Text = "Round Trip Time (milliseconds)";
			_zgc.GraphPane.YAxis.Title.IsVisible = true;
			_zgc.GraphPane.YAxis.Type = AxisType.Linear;
			_zgc.GraphPane.YAxis.MajorGrid.IsVisible = true;
			//zgc.GraphPane.YAxis.MinorGrid.IsVisible = true;
			//_zgc.GraphPane.YAxis.Scale.MajorUnit = DateUnit.Millisecond;
			//_zgc.GraphPane.YAxis.Scale.MinorUnit = DateUnit.Millisecond;
			//_zgc.GraphPane.YAxis.Scale.MajorStep = 10.0;
			//_zgc.GraphPane.YAxis.Scale.MajorStepAuto = true;
			//zgc.GraphPane.YAxis.Scale.MinAuto = false;
			//zgc.GraphPane.YAxis.Scale.MinGrace = 0;
			//zgc.GraphPane.YAxis.Scale.Min = new TimeSpan(0, 0, 0, 0, 0).TotalMilliseconds;
			//zgc.GraphPane.YAxis.Scale.MaxAuto = false;
			//zgc.GraphPane.YAxis.Scale.MaxGrace = 0;
			//zgc.GraphPane.YAxis.Scale.Max = graphYAxisMax;

			_zgc.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);

			RescaleGraphXAxis(ref _zgc);
			RescaleGraphYAxis(ref _zgc);
		}

		string XAxis_ScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
		{
			return DateTime.FromOADate(val).ToLongTimeString();
		}

		private void RescaleGraphXAxis(ref ZedGraphControl _zgc)
		{
			List<PointPair> maxPairs = new List<PointPair>();

			if (!_zgc.GraphPane.IsZoomed)
			{
				if (_zgc.GraphPane.CurveList.Count > 0)
				{
					double maxX = 0;

					for (int i = 0 ; _zgc.GraphPane.CurveList.Count > i; i++)
					{
						double _maxX;
						double _bogus;

						_zgc.GraphPane.CurveList[i].GetRange(out _bogus, out _maxX, out _bogus, out _bogus, true, false, _zgc.GraphPane);

						if (_maxX > maxX) maxX = _maxX;
					}

					//if (maxX > 0)
					//{
					//    _zgc.GraphPane.XAxis.Scale.Min = graphXAxisMin;
					//    _zgc.GraphPane.XAxis.Scale.Max = maxX;
					//}

					//else
					//{
					//    _zgc.GraphPane.XAxis.Scale.Min = graphXAxisMin;
					//    _zgc.GraphPane.XAxis.Scale.Max = graphXAxisMax;
					//}

					// HACK
					_zgc.GraphPane.XAxis.Scale.Min = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, _GraphSpanSeconds, 0)).ToOADate();
					_zgc.GraphPane.XAxis.Scale.Max = DateTime.Now.ToOADate();
				}

				_zgc.AxisChange();
				_zgc.Refresh();
			}
		}

		private void RescaleGraphYAxis(ref ZedGraphControl _zgc)
		{
			_zgc.AxisChange();
			_zgc.Refresh();
		}

		void UpdateTraceGraph(ref ZedGraphControl _zgc, List<DotHop> _dt)
		{
			if (this.InvokeRequired && this != null)
			{
				UpdateTraceGraphDelegate DoUpdateTraceGraph = new UpdateTraceGraphDelegate(UpdateTraceGraph);
				this.Invoke(DoUpdateTraceGraph);
				return;
			}

			for (int i = 0; _dt.Count > i; i++)
			{
				DotHop dh = _dt[i];

				// Create the hop curve if it doesn't already exist
				if (!(_zgc.GraphPane.CurveList.IndexOf(dh.hop.ToString()) > 0))
				{
					LineItem lastCurve = _zgc.GraphPane.AddCurve(
						dh.hop.ToString(),
						new RollingPointPairList(2048),
						this.cm.Colors[dh.hop],
						SymbolType.Circle
						);

					lastCurve.Line.IsSmooth = true;
					lastCurve.Line.SmoothTension = 0.3F;
					lastCurve.Line.Width = 2.0F;
					lastCurve.Line.IsAntiAlias = true;
					lastCurve.Symbol.Border = new Border(this.cm.Colors[dh.hop], 2.0F);
					lastCurve.Symbol.Fill = new Fill(Color.White);
					lastCurve.Symbol.Size = 6;
					lastCurve.Symbol.IsAntiAlias = true;
				}

				if (dh.tripTime.HasValue)
				{
					int index = _zgc.GraphPane.CurveList.IndexOf(dh.hop.ToString());

					StringBuilder hopText = new StringBuilder();
					hopText.Append("Hop - " + dh.hop.ToString());
					hopText.Append(System.Environment.NewLine);
					hopText.Append("IP - " + dh.hopIPStr);
					hopText.Append(System.Environment.NewLine);
					hopText.Append("Trip - " + dh.tripTimeStr);
					hopText.Append(System.Environment.NewLine);
					hopText.Append("Time - " + dh.timeStamp.ToLongTimeString());

					PointPair hopPair = new PointPair(dh.timeStamp.ToOADate(), (double)dh.last, hopText.ToString());
					_zgc.GraphPane.CurveList[index].AddPoint(hopPair);
				}

			}

			// TODO - Clean up curve list?
			//while (_zgc.GraphPane.CurveList.Count > _dt.Count)
			//{
			//    _zgc.GraphPane.CurveList.Remove(
			//        _zgc.GraphPane.CurveList[_zgc.GraphPane.CurveList.Count - 1]
			//        );
			//}

			RescaleGraphXAxis(ref _zgc);
		}
	}
}
