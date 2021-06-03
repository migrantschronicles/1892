using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WPM.PathFinding;

namespace WPM
{
	public partial class WorldMapGlobe : MonoBehaviour
	{
		Dictionary<int,byte> routeMatrix;
		IPathFinder finder;
		bool needRefreshRouteMatrix;

		void ComputeRouteMatrix ()
		{
			// prepare matrix
			if (routeMatrix == null) {
				needRefreshRouteMatrix = true;
				routeMatrix = new Dictionary<int,byte> ();
			}

			if (!needRefreshRouteMatrix || cells == null)
				return;

			routeMatrix.Clear ();
			// Compute route
			for (int j = 0; j < cells.Length; j++) {
				if (cells [j].canCross) {	// set navigation bit
					routeMatrix [j] = 1;
				} else {		// clear navigation bit
					routeMatrix [j] = 0;
				}
			}

			if (finder == null) {
				finder = new PathFinderFast (routeMatrix, cells);
			} else {
				finder.SetCalcMatrix (routeMatrix, cells);
			}
		}

		/// <summary>
		/// Used by FindRoute method to satisfy custom positions check
		/// </summary>
		int FindRoutePositionValidator (int CellIndex)
		{
			int cost = 1;
			if (OnPathFindingCrossCell != null) {
				cost = OnPathFindingCrossCell (CellIndex);
			}
			return cost;
		}

	}

}