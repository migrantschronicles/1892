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
//  Some modifications by Kronnect
using UnityEngine;
using System;
using System.Collections.Generic;

namespace WPM.PathFinding
{
	public class PathFinderFast : IPathFinder
	{
		internal struct PathFinderNodeFast
		{
			public int F;
			// f = gone + heuristic
			public int G;
			public int PIndex;
			public byte Status;
		}

		// Heap variables are initializated to default, but I like to do it anyway
		private Dictionary<int,byte> mGrid = null;
		private Cell[] mCells = null;
		private PriorityQueueB<int> mOpen = null;
		private List<PathFinderNode> mClose = new List<PathFinderNode> ();
		private HeuristicFormula mFormula = HeuristicFormula.SphericalDistance;
		private int mHEstimate = 1;
		private int mSearchLimit = 2000;
		private int mSearchMaxCost = 2000;
		private PathFinderNodeFast[] mCalcGrid = null;
		private byte mOpenNodeValue = 1;
		private byte mCloseNodeValue = 2;
		private HiddenCellsFilterMode mHiddenCellsFilter = HiddenCellsFilterMode.OnlyUseVisibleCells;
		private OnCellCross mOnCellCross = null;

		//Promoted local variables to member variables to avoid recreation between calls
		private int mH = 0;
		private int mLocation = 0;
		private int mNewLocation = 0;
		private int mCloseNodeCounter = 0;
		private bool mFound = false;
		private int mEndLocation = 0;
		private int mNewG = 0;

		public PathFinderFast (Dictionary<int,byte> grid, Cell[] Cells)
		{
			if (grid == null || Cells == null)
				throw new Exception ("Grid or Cells cannot be null");
			mGrid = grid;
			mCells = Cells;
			mCalcGrid = new PathFinderNodeFast[mCells.Length];
			mOpen = new PriorityQueueB<int> (new ComparePFNodeMatrix (mCalcGrid));
		}

		public void SetCalcMatrix (Dictionary<int,byte> grid, Cell[] Cells)
		{
			if (grid == null)
				throw new Exception ("Grid cannot be null");
			mGrid = grid;
			mCells = Cells;

			if (mCalcGrid == null || mCalcGrid.Length != mCells.Length) {
				mCalcGrid = new PathFinderNodeFast[mCells.Length];
			}
			mOpen.Clear ();

			Array.Clear (mCalcGrid, 0, mCalcGrid.Length);
			ComparePFNodeMatrix comparer = (ComparePFNodeMatrix)mOpen.comparer;
			comparer.SetMatrix (mCalcGrid);
		}

		public HeuristicFormula Formula {
			get { return mFormula; }
			set { mFormula = value; }
		}

		public int HeuristicEstimate {
			get { return mHEstimate; }
			set { mHEstimate = value; }
		}

		public int SearchLimit {
			get { return mSearchLimit; }
			set { mSearchLimit = value; }
		}

		public int SearchMaxCost {
			get { return mSearchMaxCost; }
			set { mSearchMaxCost = value; }
		}

		public HiddenCellsFilterMode HiddenCellsFilter {
            get { return mHiddenCellsFilter; }
            set { mHiddenCellsFilter = value; }
        }

		public OnCellCross OnCellCross {
			get { return mOnCellCross; }
			set { mOnCellCross = value; }
		}

		public List<PathFinderNode> FindPath (int CellStartIndex, int CellEndIndex)
		{
			mFound = false;
			mCloseNodeCounter = 0;
			if (mOpenNodeValue > 250) {
				mOpenNodeValue = 1;
				mCloseNodeValue = 2;
			} else {
				mOpenNodeValue += 2;
				mCloseNodeValue += 2;
			}
			mOpen.Clear ();
			mClose.Clear ();

			mLocation = CellStartIndex;
			mEndLocation = CellEndIndex;
			mCalcGrid [mLocation].G = 0;
			mCalcGrid [mLocation].F = mHEstimate;
			mCalcGrid [mLocation].PIndex = CellStartIndex;
			mCalcGrid [mLocation].Status = mOpenNodeValue;

			Vector3 p0 = mCells[1].sphereCenter;
			Vector3 p1 = mCells[1].neighbours[0].sphereCenter;
			float distCellToCell = Vector3.Angle(p0, p1) / 180f;
			float distPerDegree = 0;
			// Compute spherical distance
			if (mFormula == HeuristicFormula.SphericalDistance) {
				distPerDegree = 1f / (distCellToCell * 180f);
			}

			mOpen.Push (mLocation);
			while (mOpen.Count > 0) {
				mLocation = mOpen.Pop ();

				//Is it in closed list? means this node was already processed
				if (mCalcGrid [mLocation].Status == mCloseNodeValue)
					continue;

				if (mLocation == mEndLocation) {
					mCalcGrid [mLocation].Status = mCloseNodeValue;
					mFound = true;
					break;
				}

				if (mCloseNodeCounter > mSearchLimit) {
					return null;
				}

				//Lets calculate each successors
				int maxi = mCells [mLocation].neighbours.Length;
				for (int i = 0; i < maxi; i++) {
					mNewLocation = mCells [mLocation].neighboursIndices [i];
                    if (mHiddenCellsFilter == HiddenCellsFilterMode.OnlyUseVisibleCells && !mCells[mNewLocation].visible) continue;
					if (mHiddenCellsFilter == HiddenCellsFilterMode.OnlyUseHiddenCells && mCells[mNewLocation].visible) continue;

					int gridValue = mGrid [mNewLocation] > 0 ? 1 : 0;
					if (gridValue == 0)
						continue;

					gridValue += mCells[mLocation].GetNeighbourCost(i);

					// Check custom validator
					if (mOnCellCross != null) {
						gridValue += mOnCellCross (mNewLocation);
					}

					mNewG = mCalcGrid [mLocation].G + gridValue;

					if (mNewG > mSearchMaxCost)
						continue;

					//Is it open or closed?
					if (mCalcGrid [mNewLocation].Status == mOpenNodeValue || mCalcGrid [mNewLocation].Status == mCloseNodeValue) {
						// The current node has less code than the previous? then skip this node
						if (mCalcGrid [mNewLocation].G <= mNewG)
							continue;
					}

					mCalcGrid [mNewLocation].PIndex = mLocation;
					mCalcGrid [mNewLocation].G = mNewG;

					int dist = 1;
					switch (mFormula) {
					case HeuristicFormula.SphericalDistance:
						dist += (int)(Vector3.Angle (mCells [mEndLocation].sphereCenter, mCells [mNewLocation].sphereCenter) * distPerDegree); //  1000f);
						break;
					case HeuristicFormula.Euclidean:
						dist += (int)(Vector3.Distance (mCells [mEndLocation].sphereCenter, mCells [mNewLocation].sphereCenter) / distCellToCell);
						break;
					case HeuristicFormula.EuclideanNoSQR:
						dist += (int)(Vector3.SqrMagnitude (mCells [mEndLocation].sphereCenter - mCells [mNewLocation].sphereCenter) / distCellToCell);
						break;
					}
					mH = dist; //mHEstimate * dist; 
					mCalcGrid [mNewLocation].F = mNewG + mH;
					mOpen.Push (mNewLocation);
					mCalcGrid [mNewLocation].Status = mOpenNodeValue;
				}

				mCloseNodeCounter++;
				mCalcGrid [mLocation].Status = mCloseNodeValue;
			}

			//mCompletedTime = HighResolutionTime.GetTime();
			if (mFound) {
				mClose.Clear ();
				int pos = CellEndIndex;

				PathFinderNodeFast fNodeTmp = mCalcGrid [CellEndIndex];
				PathFinderNode fNode;
				fNode.F = fNodeTmp.F;
				fNode.G = fNodeTmp.G;
				fNode.H = 0;
				fNode.PIndex = fNodeTmp.PIndex;
				fNode.index = CellEndIndex;

				while (fNode.index != fNode.PIndex) {
					mClose.Add (fNode);
					pos = fNode.PIndex;
					fNodeTmp = mCalcGrid [pos];
					fNode.F = fNodeTmp.F;
					fNode.G = fNodeTmp.G;
					fNode.H = 0;
					fNode.PIndex = fNodeTmp.PIndex;
					fNode.index = pos;
				} 

				mClose.Add (fNode);

				return mClose;
			}
			return null;
		}

		internal class ComparePFNodeMatrix : IComparer<int>
		{
			protected PathFinderNodeFast[] mMatrix;

			public ComparePFNodeMatrix (PathFinderNodeFast[] matrix)
			{
				mMatrix = matrix;
			}

			public int Compare (int a, int b)
			{
				if (mMatrix [a].F > mMatrix [b].F)
					return 1;
				else if (mMatrix [a].F < mMatrix [b].F)
					return -1;
				return 0;
			}

			public void SetMatrix (PathFinderNodeFast[] matrix)
			{
				mMatrix = matrix;
			}
		}
	}
}
