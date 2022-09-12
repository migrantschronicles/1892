using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

    public enum GRID_STYLE {
        Wireframe = 0,
        Shaded = 1,
        ShadedWireframe = 2
    }

    public partial class WorldMapGlobe : MonoBehaviour {

        #region Hexagonal grid properties

        [SerializeField]
        bool
            _showHexagonalGrid = false;

        /// <summary>
        /// Enables or disables hexagonal grid on Globe.
        /// </summary>
        public bool showHexagonalGrid {
            get { return _showHexagonalGrid; }
            set {
                if (_showHexagonalGrid != value) {
                    _showHexagonalGrid = value;
                    if (!_showHexagonalGrid) {
                        if (gridCellsRoot != null)
                            gridCellsRoot.gameObject.SetActive(_showHexagonalGrid);
                        if (gridWireframeRoot != null)
                            gridWireframeRoot.gameObject.SetActive(_showHexagonalGrid);
                    } else {
                        GenerateGrid();
                    }
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _hexaGridUseMask = false;

        /// <summary>
        /// Enables or disables hexagonal grid masking.
        /// </summary>
        public bool hexaGridUseMask {
            get { return _hexaGridUseMask; }
            set {
                if (_hexaGridUseMask != value) {
                    _hexaGridUseMask = value;
                    shouldGenerateGrid = true;
                    UpdateGridMaterialProperties();
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        Texture2D _hexaGridMask;

        public Texture2D hexaGridMask {
            get { return _hexaGridMask; }
            set {
                if (_hexaGridMask != value) {
                    _hexaGridMask = value;
                    isDirty = true;
                    LoadGridMask();
                    if (_showHexagonalGrid) {
                        GenerateGrid();
                    }
                }
            }
        }


        [Range(15, 200)]
        [SerializeField]
        int
            _hexaGridDivisions = 15;

        public int hexaGridDivisions {
            get { return _hexaGridDivisions; }
            set {
                if (_hexaGridDivisions != value) {
                    _hexaGridDivisions = Mathf.Max(15, value);  // less divisions will increase clipping with the sphere.
                    shouldGenerateGrid = true;
                    UpdateGridMaterialProperties();
                    isDirty = true;
                }
            }
        }

        [Range(0, 255)]
        [SerializeField]
        int
            _hexaGridMaskThreshold = 11;

        public int hexaGridMaskThreshold {
            get { return _hexaGridMaskThreshold; }
            set {
                if (_hexaGridMaskThreshold != value) {
                    _hexaGridMaskThreshold = Mathf.Clamp(value, 0, 255);
                    shouldGenerateGrid = true;
                    UpdateGridMaterialProperties();
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        Vector3
            _hexaGridRotationShift;

        /// <summary>
        /// Applies an internal rotation to the positions of the grid cells. Does not change globe rotation.
        /// </summary>
        public Vector3 hexaGridRotationShift {
            get { return _hexaGridRotationShift; }
            set {
                if (_hexaGridRotationShift != value) {
                    _hexaGridRotationShift = value;
                    shouldGenerateGrid = true;
                    UpdateGridMaterialProperties();
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        Color
            _hexaGridColor = Color.white;

        public Color hexaGridColor {
            get { return _hexaGridColor; }
            set {
                if (_hexaGridColor != value) {
                    _hexaGridColor = value;
                    UpdateGridMaterialProperties();
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Fired when path finding algorithmn evaluates a cell. Return the increased cost for cell.
        /// </summary>
        public event GridCellEvent OnCellClick;

        /// <summary>
        /// Fired when cursor enters a cell
        /// </summary>
        public event GridCellEvent OnCellEnter;

        /// <summary>
        /// Fired when cursor exits a cell
        /// </summary>
        public event GridCellEvent OnCellExit;

        [SerializeField]
        Color
            _hexaGridHighlightColor = new Color(0, 0.25f, 1f, 0.8f);

        public Color hexaGridHighlightColor {
            get { return _hexaGridHighlightColor; }
            set {
                if (_hexaGridHighlightColor != value) {
                    _hexaGridHighlightColor = value;
                    UpdateGridMaterialProperties();
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        [Range(0.1f, 5f)]
        float
            _hexaGridHighlightSpeed = 1f;

        public float hexaGridHighlightSpeed {
            get { return _hexaGridHighlightSpeed; }
            set {
                if (_hexaGridHighlightSpeed != value) {
                    _hexaGridHighlightSpeed = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _hexaGridHighlightEnabled = true;

        public bool hexaGridHighlightEnabled {
            get { return _hexaGridHighlightEnabled; }
            set {
                if (_hexaGridHighlightEnabled != value) {
                    _hexaGridHighlightEnabled = value;
                    if (!_hexaGridHighlightEnabled) {
                        HideHighlightedCell();
                    }
                    isDirty = true;
                }
            }
        }

        public int lastHighlightedCellIndex = -1;
        public Cell lastHighlightedCell;

        #endregion

        #region Public API

        /// <summary>
        /// Gets the cell under the local sphere position
        /// </summary>
        public int GetCellIndex(Vector3 spherePosition) {
            if (cells == null)
                return -1;
            return GetCellAtLocalPosition(spherePosition, false);
        }

        /// <summary>
        /// Gets the cell under the latlon position
        /// </summary>
        public int GetCellIndex(Vector2 latlon) {
            if (cells == null)
                return -1;
            Vector3 spherePosition = Conversion.GetSpherePointFromLatLon(latlon);
            return GetCellAtLocalPosition(spherePosition, false);
        }

        /// <summary>
        /// Gets the cell index of the cell nearest to a given world position.
        /// </summary>
        /// <returns>The cell at world position.</returns>
        public int GetCellAtWorldPos(Vector3 worldPosition) {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            return GetCellIndex(localPosition);
        }

        /// <summary>
        /// Array of generated cells.
        /// </summary>
        public Cell[] cells;

        /// <summary>
        /// Returns the index of the cell in the cells list
        /// </summary>
        public int GetCellIndex(Cell cell) {
            if (cells == null)
                return -1;
            return cell.index;
        }


        /// <summary>
        /// Sets the cell material.
        /// </summary>
        /// <returns><c>true</c>, if cell material was set, <c>false</c> otherwise.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="mat">Material to be used.</param>
        /// <param name="temporary">If set to <c>true</c> the material is not saved anywhere and will be restored to default cell material when cell gets unselected.</param>
        public bool SetCellMaterial(int cellIndex, Material mat, bool temporary = false) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            Cell cell = cells[cellIndex];
            if (cell.renderer == null) {
                GenerateCellMesh(cellIndex, mat);
            } else {
                cell.renderer.sharedMaterial = mat;
                cell.renderer.enabled = true;
            }
            if (mat != hexaGridHighlightMaterial) {
                if (temporary) {
                    cell.tempMat = mat;
                } else {
                    cell.customMat = mat;
                }
            }
            if (hexaGridHighlightMaterial != null && cell == lastHighlightedCell) {
                if (cell.renderer != null)
                    cell.renderer.sharedMaterial = hexaGridHighlightMaterial;
                if (cell.tempMat != null) {
                    hexaGridHighlightMaterial.SetColor("_Color2", cell.tempMat.color);
                    hexaGridHighlightMaterial.mainTexture = cell.tempMat.mainTexture;
                } else if (cell.customMat != null) {
                    hexaGridHighlightMaterial.SetColor("_Color2", cell.customMat.color);
                    hexaGridHighlightMaterial.mainTexture = cell.customMat.mainTexture;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the color of the cell.
        /// </summary>
        /// <returns><c>true</c>, if cell color was set, <c>false</c> otherwise.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="color">Color.</param>
        /// <param name="temporary">If set to <c>true</c> th_gridDivisionsored temporarily and returns to default color when it gets unselected.</param>
        public bool SetCellColor(int cellIndex, Color color, bool temporary = false) {
            Material mat = GetCachedMaterial(color);
            return SetCellMaterial(cellIndex, mat, temporary);
        }


        /// <summary>
        /// Sets the color of a list of cells.
        /// </summary>
        /// <returns><c>true</c>, if cell color was set, <c>false</c> otherwise.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="color">Color.</param>
        /// <param name="temporary">If set to <c>true</c> the cell is colored temporarily and returns to default color when it gets unselected.</param>
        public void SetCellColor(List<int> cellIndices, Color color, bool temporary = false) {
            if (cellIndices == null)
                return;
            Material mat = GetCachedMaterial(color);
            int tc = cellIndices.Count;
            for (int k = 0; k < tc; k++) {
                int cellIndex = cellIndices[k];
                SetCellMaterial(cellIndex, mat, temporary);
            }
        }


        /// <summary>
        /// Sets the color of all cells contained in a country main region.
        /// </summary>
        /// <param name="Country">Country object.</param>
        /// <param name="color">Color.</param>
        /// <param name="temporary">If set to <c>true</c> th_gridDivisionsored temporarily and returns to default color when it gets unselected.</param>
        public void SetCellColor(Country country, Color color, bool temporary = false) {
            if (country == null)
                return;
            Region region = country.mainRegion;
            for (int k = 0; k < cells.Length; k++) {
                Vector2 p = Conversion.GetLatLonFromSpherePoint(cells[k].sphereCenter);
                if (region.Contains(p)) {
                    SetCellColor(k, color, temporary);
                }
            }
        }

        /// <summary>
        /// Sets the texture of the cell.
        /// </summary>
        /// <returns><c>true</c>, if cell color was set, <c>false</c> otherwise.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="texture">Color.</param>
        /// <param name="temporary">If set to <c>true</c> the cell is colored temporarily and returns to default color when it gets unselected.</param>
        public bool SetCellTexture(int cellIndex, Texture2D texture, bool temporary = false) {
            return SetCellTexture(cellIndex, texture, Color.white, temporary);
        }

        /// <summary>
        /// Sets the texture and tint color of the cell.
        /// </summary>
        /// <returns><c>true</c>, if cell color was set, <c>false</c> otherwise.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="texture">Color.</param>
        /// <param name="tint">Optional tint color.</param>
        /// <param name="temporary">If set to <c>true</c> the cell is colored temporarily and returns to default color when it gets unselected.</param>
        public bool SetCellTexture(int cellIndex, Texture2D texture, Color tint, bool temporary = false) {
            Material mat = GetCachedMaterial(tint, texture);
            return SetCellMaterial(cellIndex, mat, temporary);
        }

        /// <summary>
        /// Returns current cell color.
        /// </summary>
        public Color GetCellColor(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return Color.white;
            Cell cell = cells[cellIndex];
            if (cell.tempMat != null)
                return cell.tempMat.color;
            if (cell.customMat != null)
                return cell.customMat.color;
            return Color.white;
        }

        /// <summary>
        /// Gets the cell neighbours.
        /// </summary>
        /// <param name="cellIndex">Cell index.</param>
        public Cell[] GetCellNeighbours(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return null;
            return cells[cellIndex].neighbours;
        }

        /// <summary>
        /// Gets the cell neighbours indices.
        /// </summary>
        /// <param name="cellIndex">Cell index.</param>
        public int[] GetCellNeighboursIndices(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return null;
            return cells[cellIndex].neighboursIndices;
        }

        /// <summary>
        /// Given a cell, returns the neighbour index of a second cell. If that cell is not a neighbour, this function returns -1
        /// </summary>
        /// <returns>The cell neighbour index.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="cellNeighbourIndex">Cell neighbour index.</param>
        public int GetCellNeighbourIndex(int cellIndex, int cellNeighbourIndex) {
            if (cells == null || cellIndex < 0 || cellIndex >= cells.Length || cellNeighbourIndex < 0 || cellNeighbourIndex >= cells.Length)
                return -1;
            Cell cell = cells[cellIndex];
            int neighbourCount = cell.neighboursIndices.Length;
            for (int k = 0; k < neighbourCount; k++) {
                if (cellNeighbourIndex == cell.neighboursIndices[k]) {
                    return k;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the points of the edge line which is shared by two cells
        /// </summary>
        /// <returns>The cell shared edge.</returns>
        /// <param name="cellIndex">Cell index.</param>
        /// <param name="cellNeighbourIndex">Cell neighbour index.</param>
        public Vector3[] GetCellSharedEdge(int cellIndex, int cellNeighbourIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length || cellNeighbourIndex < 0 || cellNeighbourIndex >= cells.Length)
                return null;
            Cell cell1 = cells[cellIndex];
            Cell cell2 = cells[cellNeighbourIndex];

            Vector3[] edge = new Vector3[2];
            int vertexIndex = 0;
            for (int k = 0; k < cell1.vertexPoints.Length; k++) {
                for (int j = 0; j < cell2.vertexPoints.Length; j++) {
                    if (cell1.vertexPoints[k] == cell2.vertexPoints[j]) {
                        edge[vertexIndex++] = cell1.vertices[k];
                        if (vertexIndex > 1)
                            break;
                    }
                }
            }
            return edge;
        }

        /// <summary>
        /// Hide a given cell
        /// </summary>
        public void ClearCell(int cellIndex, bool clearTemporaryColor = false, bool clearAllColors = true, bool clearObstacles = true) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            Cell cell = cells[cellIndex];
            Renderer cellRenderer = cell.renderer;
            cell.tempMat = null;
            if (cellRenderer != null) {
                cellRenderer.enabled = false;
            }
            if (clearAllColors) {
                cell.customMat = null;
            }
            if (clearObstacles) {
                cell.canCross = true;
            }
        }


        /// <summary>
        /// Hide all cells
        /// </summary>
        public void ClearCells(bool clearTemporaryColors = false, bool clearAllColors = true, bool clearObstacles = true) {
            for (int k = 0; k < cells.Length; k++) {
                ClearCell(k, clearTemporaryColors, clearAllColors, clearObstacles);
            }
            ResetHighlightMaterial();
        }

        /// <summary>
        /// Destroys a colored cell
        /// </summary>
        public void DestroyCell(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            cells[cellIndex].tempMat = null;
            cells[cellIndex].customMat = null;
            if (cells[cellIndex].renderer != null) {
                DestroyImmediate(cells[cellIndex].renderer.gameObject);
                cells[cellIndex].renderer = null;
            }
        }

        /// <summary>
        /// Toggles cell visibility
        /// </summary>
        public bool ToggleCell(int cellIndex, bool visible) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            Cell cell = cells[cellIndex];
            if (cell.renderer != null) {
                cell.renderer.enabled = visible;
                return true;
            }
            if (cell.visible != visible) {
                cell.visible = visible;
                shouldGenerateGrid = true;
            }
            return false;
        }

        /// <summary>
        /// Hides a colored cell
        /// </summary>
        public bool HideCell(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            if (cells[cellIndex].renderer != null) {
                cells[cellIndex].renderer.enabled = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Shows a colored cell
        /// </summary>
        public bool ShowCell(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            if (cells[cellIndex].renderer != null) {
                cells[cellIndex].renderer.enabled = true;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns true if cell is visible
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        public bool IsCellVisible(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return false;
            return cells[cellIndex].visible;
        }

        /// <summary>
        /// Returns the center of the cell in world space coordinates.
        /// </summary>
        public Vector3 GetWorldSpaceCellCenter(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return Vector3.zero;
            Cell cell = cells[cellIndex];
            Vector3 cellTop = transform.TransformPoint(cell.sphereCenter);
            return cellTop;
        }


        /// <summary>
        /// Starts navigating to target cell by index in the cells collection with specified duration using NavigationTime property for duration.
        /// </summary>
        public void FlyToCell(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            FlyToLocation(cells[cellIndex].sphereCenter, _navigationTime, 0, _navigationBounceIntensity);
        }


        /// <summary>
        /// Starts navigating to target cell by index in the cells collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// </summary>
        public void FlyToCell(int cellIndex, float duration) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            FlyToLocation(cells[cellIndex].sphereCenter, duration, 0, _navigationBounceIntensity);
        }

        /// <summary>
        /// Starts navigating to target cell by index in the cells collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// Set zoomLevel to a value from 0 to 1 for the destination zoom level.
        /// </summary>
        public void FlyToCell(int cellIndex, float duration, float zoomLevel) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            FlyToLocation(cells[cellIndex].sphereCenter, duration, zoomLevel, _navigationBounceIntensity);
        }

        /// <summary>
        /// Starts navigating to target cell by index in the cells collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// Set zoomLevel to a value from 0 to 1 for the destination zoom level.
        /// Set bounceIntensity to a value from 0 to 1 for a bouncing effect between current position and destination
        /// </summary>
        public void FlyToCell(int cellIndex, float duration, float zoomLevel, float bounceIntensity) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            FlyToLocation(cells[cellIndex].sphereCenter, duration, zoomLevel, _navigationBounceIntensity);
        }




        /// <summary>
        /// Get all cell indices whose center is inside a given region
        /// </summary>
        public List<int> GetCells(Region region, int enclosedVertexCount = 3) {
            List<int> results = new List<int>();
            for (int k = 0; k < cells.Length; k++) {
                if (region.Contains(cells[k].latlonCenter)) {
                    results.Add(k);
                    continue;
                }
                Cell cell = cells[k];
                int vertexCount = cell.latlon.Length;
                int count = 0;
                for (int v = 0; v < vertexCount; v++) {
                    if (region.Contains(cell.latlon[v])) {
                        count++;
                        if (count >= enclosedVertexCount) {
                            results.Add(k);
                            break;
                        }
                    }
                }
            }
            return results;
        }


        #endregion

    }

}