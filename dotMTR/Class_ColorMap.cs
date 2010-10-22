/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace dotMTR
{
	/// <summary>
	/// ColorMap class
	/// </summary>
	public class ColorMap
	{
		private Dictionary<Color, Color> _colorPairs = new Dictionary<Color, Color>();
		private Dictionary<int, Color> _colorMap = new Dictionary<int, Color>();
		private Dictionary<int, SourceGrid.Cells.Views.Cell> _cellViews = new Dictionary<int, SourceGrid.Cells.Views.Cell>();

		/// <summary>
		/// Colors property
		/// </summary>
		public Dictionary<int, Color> Colors
		{
			get
			{
				return _colorMap;
			}
		}
		
		/// <summary>
		/// CellViews property
		/// </summary>
		public Dictionary<int, SourceGrid.Cells.Views.Cell> CellViews
		{
			get
			{
				return _cellViews;
			}
		}

		/// <summary>
		/// Main constructor
		/// </summary>
		/// <param name="_count"></param>
		public ColorMap(int _count)
		{
			this._colorPairs.Add(Color.Lime, Color.Blue);
			this._colorPairs.Add(Color.Blue, Color.Red);
			this._colorPairs.Add(Color.Red, Color.Yellow);

			var colorList = new List<System.Drawing.Color>();

			foreach (var colorPair in _colorPairs)
			{
				// TODO
				Color hopColorMin = colorPair.Key;
				Color hopColorMax = colorPair.Value;
				int rMin = hopColorMin.R;
				int rMax = hopColorMax.R;
				int gMin = hopColorMin.G;
				int gMax = hopColorMax.G;
				int bMin = hopColorMin.B;
				int bMax = hopColorMax.B;

				for (int i = 0; i < _count; i++)
				{
					var rAverage = rMin + (int)((rMax - rMin) * i / _count);
					var bAverage = bMin + (int)((bMax - bMin) * i / _count);
					var gAverage = gMin + (int)((gMax - gMin) * i / _count);

					colorList.Add(System.Drawing.Color.FromArgb(128, rAverage, gAverage, bAverage));
				}
			}

			for (int i = 0; _count > i; i++)
			{
				Color c = colorList[(i * colorList.Count) / _count];

				_colorMap.Add((i + 1), c);

				SourceGrid.Cells.Views.Cell cellView = new SourceGrid.Cells.Views.Cell();
				cellView.BackColor = c;

				_cellViews.Add((i + 1), cellView);
			}
		}
	}
}
