/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using SourceGrid;


namespace dotMTR
{
	public class ColLayout
	{
		private string _HeaderText;
		public string Text
		{
			get{return _HeaderText;}
			set{_HeaderText = value;}
		}

		private int _WidthPct;
		public int WidthPct
		{
			get { return _WidthPct; }
			set {
				if ((int)value < 1 || (int)value > 100)
				{
					throw new Exception("Value must be between 1 and 100");
				}

				else
				{
					_WidthPct = value;
				}
			}
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ColLayout(String _HeaderText, int _WidthPct)
		{
			this.Text = _HeaderText;
			this.WidthPct = _WidthPct;
		}
	}

	public class GridLayout
	{
		private int _HeaderRow = 0;
		public int HeaderRow
		{
			get
			{
				return _HeaderRow;
			}
		}

		private List<ColLayout> _Columns = new List<ColLayout>();
		public List<ColLayout> Columns
		{
			get
			{
				return _Columns;
			}
		}

		/// <summary>
		/// Default constructor (not very useful)
		/// </summary>
		public GridLayout()
		{
		}

		/// <summary>
		/// Main constructor
		/// </summary>
		/// <param name="_colDefs"></param>
		public GridLayout(Dictionary<String, int> _colDefs)
		{
			foreach (var colDef in _colDefs)
			{
				_Columns.Add(new ColLayout(colDef.Key, colDef.Value));
			}
		}

		/// <summary>
		/// Layout method
		/// </summary>
		/// <param name="_sg"></param>
		public void Layout(ref SourceGrid.Grid _sg)
        {
			for (int i = 0; this.Columns.Count > i; i++)
			{
				_sg[this.HeaderRow, i] = new SourceGrid.Cells.ColumnHeader(this.Columns[i].Text);
			}

			RescaleX(ref _sg);
        }

		/// <summary>
		/// Rescale X (column)
		/// </summary>
		/// <param name="_sg"></param>
		public void RescaleX(ref SourceGrid.Grid _sg)
		{
			for (int i = 0; this.Columns.Count > i; i++)
			{
				_sg.Columns[i].Width = this.CalcColWidth(_sg, this.Columns[i].WidthPct);
			}
		}

		/// <summary>
		/// Calculate column width given a percentage
		/// </summary>
		/// <param name="_sg"></param>
		/// <param name="_widthPct"></param>
		/// <returns></returns>
		private int CalcColWidth(SourceGrid.Grid _sg, int _widthPct)
		{
			int availWidth = _sg.Parent.Width - _sg.Parent.Margin.Right - _sg.Margin.Right;

			return Convert.ToInt32((Convert.ToDouble(_widthPct) / Convert.ToDouble(100)) * Convert.ToDouble(availWidth));
		}
	}
}
