//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//
//  Modifications by Kronnect


using System;
using System.Collections.Generic;

namespace WPM.PathFinding {

    public struct PathFinderNode
    {
        public int     F;
        public int     G;
        public int     H;  // f = gone + heuristic
		public int     index;
        public int     PIndex;
    }

    public enum PathFinderNodeType
    {
        Start   = 1,
        End     = 2,
        Open    = 4,
        Close   = 8,
        Current = 16,
        Path    = 32
    }

    public enum HeuristicFormula
    {
        SphericalDistance   = 1,
        Euclidean           = 2,
        EuclideanNoSQR      = 3
    }

    public enum HiddenCellsFilterMode {
        OnlyUseVisibleCells = 0,
        OnlyUseHiddenCells = 1,
        UseAllCells = 2
    }


    internal class ComparePFNode : IComparer<PathFinderNode>
	{
		public int Compare(PathFinderNode x, PathFinderNode y)
		{
			if (x.F > y.F)
				return 1;
			else if (x.F < y.F)
				return -1;
			return 0;
		}
	}
}
