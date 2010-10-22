/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Amib.Threading;
using System.Net.NetworkInformation;

namespace dotMTR
{
	public partial class Form1 : Form
	{
		private void UpdateMTR()
		{
			Object traceQueuePeek = new Object();

			if (this.InvokeRequired && (this.Enabled == true))
			{
				UpdateMTRDelegate DoUpdateMTR = new UpdateMTRDelegate(UpdateMTR);
				this.Invoke(DoUpdateMTR);
				return;
			}

			while (traceQueue.TryPeek(out traceQueuePeek))
			{
				List<DotHop> dt = (List<DotHop>)traceQueue.Dequeue(100);

				if (dt.Last().status == IPStatus.Success)
				{
					this.progressIndicator1.CircleColor = UpProgressColor;
				}

				else
				{
					this.progressIndicator1.CircleColor = DownProgressColor;
				}

				for (int i = 0; dt.Count > i; i++)
				{
					DotHop dh = dt[i];

					if (!mtrHops.ContainsKey(dh.hop)) mtrHops.Add(dh.hop, new DotHop());

					mtrHops[dh.hop].statuss.Add(dh.status);
					mtrHops[dh.hop].tripTimes.Add(dh.tripTime);
					mtrHops[dh.hop].timeStamps.Add(dh.timeStamp);

					// TODO - Does an absence of a value affect the statistics?
					if (dh.tripTime.HasValue && (int)dh.tripTime >= 0) mtrHops[dh.ttl].tripTimes.Add(dh.tripTime);
				}

				UpdateTraceDetail(ref this.grid1, dt);
				UpdateTraceGraph(ref zedGraphControl1, dt);
			}
		}
	}
}
