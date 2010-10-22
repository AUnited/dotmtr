/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Drawing;


namespace dotMTR
{
	public partial class Form1 : Form
	{
		private delegate void UpdateTraceDetailDelegate(ref SourceGrid.Grid _sg, List<DotHop> _dh);

		void UpdateTraceDetail(ref SourceGrid.Grid _sg, List<DotHop> _dt)
		{
			if (this.InvokeRequired && this != null)
			{
				UpdateTraceDetailDelegate DoUpdateTraceDetail = new UpdateTraceDetailDelegate(UpdateTraceDetail);
				this.Invoke(DoUpdateTraceDetail);
				return;
			}

			while ((_sg.Rows.Count - 1) > _dt.Count)
			{
				_sg.Rows.Remove(_sg.RowsCount - 1);
			}

			for (int i = 0; _dt.Count > i; i++)
			{
				DotHop dh = _dt[i];

				if (_sg.Rows[dh.hop] == null) _sg.Rows.Insert(dh.hop);


				Double lossRate = (double)(mtrHops[dh.hop].sent - mtrHops[dh.hop].recvd) / mtrHops[dh.hop].sent;
				Double last = new Double();
				Double best = new Double();
				Double worst = new Double();
				Double average = new Double();


				if (dh.hopIP != null) _sg[dh.hop, 0] = new SourceGrid.Cells.Cell(dh.hopIP.ToString());
				if (dh.hopIP == null) _sg[dh.hop, 0] = new SourceGrid.Cells.Cell("*");

				_sg[dh.hop, 1] = new SourceGrid.Cells.Cell(dh.ttl.ToString());
				_sg[dh.hop, 2] = new SourceGrid.Cells.Cell(dh.status.ToString());
				_sg[dh.hop, 3] = new SourceGrid.Cells.Cell(mtrHops[dh.hop].sent.ToString());
				_sg[dh.hop, 4] = new SourceGrid.Cells.Cell(mtrHops[dh.hop].recvd.ToString());
				_sg[dh.hop, 5] = new SourceGrid.Cells.Cell(lossRate.ToString("0.00%"));

				if (mtrHops[dh.hop].tripTime.HasValue)
				{
					last = (double)mtrHops[dh.hop].tripTime;
					_sg[dh.hop, 6] = new SourceGrid.Cells.Cell(last.ToString("0.00ms"));
				}

				else
				{
					_sg[dh.hop, 6] = new SourceGrid.Cells.Cell("*");
				}

				if (mtrHops[dh.hop].best.HasValue)
				{
					best = (double)mtrHops[dh.hop].best;
					_sg[dh.hop, 7] = new SourceGrid.Cells.Cell(best.ToString("0.00ms"));
				}

				else
				{
					_sg[dh.hop, 7] = new SourceGrid.Cells.Cell("*");
				}

				if (mtrHops[dh.hop].worst.HasValue)
				{
					worst = (double)mtrHops[dh.hop].worst;
					_sg[dh.hop, 8] = new SourceGrid.Cells.Cell(worst.ToString("0.00ms"));
				}

				else
				{
					_sg[dh.hop, 8] = new SourceGrid.Cells.Cell("*");
				}

				if (mtrHops[dh.hop].average.HasValue)
				{
					average = (double)mtrHops[dh.hop].average;
					_sg[dh.hop, 9] = new SourceGrid.Cells.Cell(average.ToString("0.00ms"));
				}

				else
				{
					_sg[dh.hop, 9] = new SourceGrid.Cells.Cell("*");
				}


				_sg[dh.hop, 1].View = this.cm.CellViews[dh.hop];


				if (dh.IsLastHop) goto LASTHOP;
			}

			LASTHOP:
			_sg.Refresh();
		}
	}
}
