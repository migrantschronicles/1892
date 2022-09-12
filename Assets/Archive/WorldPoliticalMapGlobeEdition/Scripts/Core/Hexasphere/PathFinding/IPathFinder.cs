using System;
using System.Collections.Generic;
using System.Text;


namespace WPM.PathFinding {

	public delegate int OnCellCross(int location);

	interface IPathFinder {

		HeuristicFormula Formula {
			get;
			set;
		}

		int HeuristicEstimate {
			get;
			set;
		}

		int SearchLimit {
			get;
			set;
		}

		int SearchMaxCost {
			get;
			set;
		}

		HiddenCellsFilterMode HiddenCellsFilter {
			get;
			set;
        }

		List<PathFinderNode> FindPath (int tileStartIndex, int tileEndIndex);
		void SetCalcMatrix(Dictionary<int,byte> grid, Cell[] tiles);

		OnCellCross OnCellCross { get; set; }

	}
}
