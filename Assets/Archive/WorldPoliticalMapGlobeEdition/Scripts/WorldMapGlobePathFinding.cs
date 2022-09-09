using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using WPM.PathFinding;

namespace WPM {
    public partial class WorldMapGlobe : MonoBehaviour {

        #region Path Finding properties

        /// <summary>
        /// Fired when path finding algorithmn evaluates a cell. Return the increased cost for cell.
        /// </summary>
        public event PathFindingEvent OnPathFindingCrossCell;

        [SerializeField]
        HeuristicFormula
            _pathFindingHeuristicFormula = HeuristicFormula.SphericalDistance;

        /// <summary>
        /// The path finding heuristic formula to estimate distance from current position to destination
        /// </summary>
        public PathFinding.HeuristicFormula pathFindingHeuristicFormula {
            get { return _pathFindingHeuristicFormula; }
            set {
                if (value != _pathFindingHeuristicFormula) {
                    _pathFindingHeuristicFormula = value;
                }
            }
        }

        [SerializeField]
        int
            _pathFindingSearchLimit = 2000;

        /// <summary>
        /// The maximum path length.
        /// </summary>
        public int pathFindingSearchLimit {
            get { return _pathFindingSearchLimit; }
            set {
                if (value != _pathFindingSearchLimit) {
                    _pathFindingSearchLimit = value;
                }
            }
        }

        [SerializeField]
        int
            _pathFindingSearchMaxCost = 2000;

        /// <summary>
        /// The maximum cost for any path.
        /// </summary>
        public int pathFindingSearchMaxCost {
            get { return _pathFindingSearchMaxCost; }
            set {
                if (value != _pathFindingSearchMaxCost) {
                    _pathFindingSearchMaxCost = value;
                    isDirty = true;
                }
            }
        }


        #endregion

        #region Public API

        /// <summary>
        /// Returns an optimal path from startPosition to endPosition with options.
        /// </summary>
        /// <returns>The route consisting of a list of cell indexes.</returns>
        /// <param name="startPosition">Start position in spherical coordinates</param>
        /// <param name="endPosition">End position in spherical coordinates</param>
        /// <param name="maxSearchCost">Maximum search cost for the path finding algorithm. A value of 0 will use the global default defined by pathFindingMaxCost</param>
        /// <param name="hiddenCells">Specifies if hidden cells can be included</param>
        /// <param name="searchMaxCost">Maximum crossing cost of path</param>
        public List<int> FindPath(Vector3 startPosition, Vector3 endPosition, int searchLimit = 0, bool includeStartingCell = false, HiddenCellsFilterMode hiddenCells = HiddenCellsFilterMode.OnlyUseVisibleCells, int searchMaxCost = 0) {

            int cellStartIndex = GetCellIndex(startPosition);
            int cellEndIndex = GetCellIndex(endPosition);
            return FindPath(cellStartIndex, cellEndIndex, searchLimit, includeStartingCell, hiddenCells, searchMaxCost);
        }

        /// <summary>
        /// Returns an optimal path from startPosition to endPosition with options.
        /// </summary>
        /// <returns>The route consisting of a list of cell indexes.</returns>
        /// <param name="cellIndexStart">Index of starting cell</param>
        /// <param name="cellIndexEnd">Index of destination cell</param>
        /// <param name="maxSearchCost">Maximum search cost for the path finding algorithm. A value of 0 will use the global default defined by pathFindingMaxCost</param>
        /// <param name="hiddenCells">Specifies if hidden cells can be included</param>
        /// <param name="searchMaxCost">Maximum crossing cost of path</param>
        public List<int> FindPath(int cellIndexStart, int cellIndexEnd, int searchLimit = 0, bool includeStartingCell = false, HiddenCellsFilterMode hiddenCells = HiddenCellsFilterMode.OnlyUseVisibleCells, int searchMaxCost = 0) {

            if (shouldGenerateGrid) {
                GenerateGrid();
            }
            if (cellIndexStart < 0 || cells == null || cellIndexStart >= cells.Length || cellIndexEnd < 0 || cellIndexEnd >= cells.Length)
                return null;

            int startingPoint = cellIndexStart;
            int endingPoint = cellIndexEnd;
            List<int> routePoints = null;

            // Minimum distance for routing?
            if (startingPoint != endingPoint) {
                ComputeRouteMatrix();
                finder.Formula = _pathFindingHeuristicFormula;
                finder.SearchLimit = searchLimit == 0 ? _pathFindingSearchLimit : searchLimit;
                finder.HiddenCellsFilter = hiddenCells;
                finder.SearchMaxCost = searchMaxCost == 0 ? _pathFindingSearchMaxCost : searchMaxCost;
                if (OnPathFindingCrossCell != null) {
                    finder.OnCellCross = FindRoutePositionValidator;
                } else {
                    finder.OnCellCross = null;
                }
                List<PathFinderNode> route = finder.FindPath(startingPoint, endingPoint);
                if (route != null) {
                    int count = route.Count;
                    routePoints = new List<int>(count);
                    int last = includeStartingCell ? count - 1 : count - 2;
                    for (int r = last; r >= 0; r--) {
                        routePoints.Add(route[r].index);
                    }
                    if (count == 0 || routePoints[routePoints.Count - 1] != cellIndexEnd) {
                        routePoints.Add(cellIndexEnd);
                    }
                } else {
                    return null;    // no route available
                }
            }
            return routePoints;
        }

        /// <summary>
        /// Sets if path finding can cross this cell.
        /// </summary>
        public bool SetCellCanCross(int cellIndex, bool canCross) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            cells[cellIndex].canCross = canCross;
            return true;
        }

        /// <summary>
        /// Returns whether path finding can cross this cell.
        /// </summary>
        public bool GetCellCanCross(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            return cells[cellIndex].canCross;
        }

        /// <summary>
        /// Sets the cost for crossing a given cell side. Note that the cost is only assigned in one direction (from this cell to the outside).
        /// </summary>
        /// <returns><c>true</c>, if neighbour cost was set, <c>false</c> otherwise.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="cellNeighbourIndex">Cell neighbour index.</param>
        /// <param name="cost">Cost.</param>
        /// <param name="bothSides">If set to true, cost is assigned in either direction.</param>
        public bool SetCellNeighbourCost(int cellIndex, int cellNeighbourIndex, int cost, bool bothSides = true) {
            if (cells == null || cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            Cell cell = cells[cellIndex];
            int neighbourIndex = GetCellNeighbourIndex(cellIndex, cellNeighbourIndex);
            if (neighbourIndex < 0)
                return false;
            cell.neighboursCosts[neighbourIndex] = cost;
            if (bothSides) {
                cell = cells[cellNeighbourIndex];
                neighbourIndex = GetCellNeighbourIndex(cellNeighbourIndex, cellIndex);
                if (neighbourIndex < 0)
                    return false;
                cell.neighboursCosts[neighbourIndex] = cost;
            }
            return true;
        }

        /// <summary>
        /// Sets the cost for crossing any cell side. Note that the cost is only assigned in one direction (from this cell to the outside).
        /// </summary>
        public void SetCellNeighboursCost(int cellIndex, int cost, bool bothSides = true) {
            if (cells == null || cellIndex < 0 || cellIndex >= cells.Length)
                return;
            Cell cell = cells[cellIndex];
            int neighbourCount = cell.neighboursIndices.Length;
            for (int k = 0; k < neighbourCount; k++) {
                SetCellNeighbourCost(cellIndex, cell.neighboursIndices[k], cost, bothSides);
            }
        }


        /// <summary>
        /// Gets the cost for crossing a given cell side
        /// </summary>
        public int GetCellNeighbourCost(int cellIndex, int neighbourIndex) {
            if (cells == null || cellIndex < 0 || cellIndex >= cells.Length)
                return -1;
            Cell cell = cells[cellIndex];
            if (neighbourIndex < 0 || neighbourIndex >= cell.neighboursCosts.Length) {
                return -1;
            }
            return cell.neighboursCosts[neighbourIndex];
        }

        #endregion

    }

}