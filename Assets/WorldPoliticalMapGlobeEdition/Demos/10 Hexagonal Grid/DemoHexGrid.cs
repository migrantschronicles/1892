using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
    public class DemoHexGrid : MonoBehaviour {

        public Texture2D roadTexture;

        enum SELECTION_MODE {
            NONE = 0,
            CUSTOM_PATH = 1,
            CUSTOM_COST = 2
        }

        WorldMapGlobe map;
        GUIStyle labelStyle, labelStyleShadow, buttonStyle, sliderStyle, sliderThumbStyle;
        SELECTION_MODE selectionMode = SELECTION_MODE.NONE;
        int selectionState;
        // 0 = selecting first cell, 1 = selecting second cell
        int firstCell;
        // the cell index of the first selected cell when setting edge cost between two neighbour cells

        void Start() {

            // UI Setup - non-important, only for this demo
            labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = Color.white;
            labelStyleShadow = new GUIStyle(labelStyle);
            labelStyleShadow.normal.textColor = Color.black;
            buttonStyle = new GUIStyle(labelStyle);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.normal.background = Texture2D.whiteTexture;
            buttonStyle.normal.textColor = Color.white;
            sliderStyle = new GUIStyle();
            sliderStyle.normal.background = Texture2D.whiteTexture;
            sliderStyle.fixedHeight = 4.0f;
            sliderThumbStyle = new GUIStyle();
            sliderThumbStyle.normal.background = Resources.Load<Texture2D>("thumb");
            sliderThumbStyle.overflow = new RectOffset(0, 0, 8, 0);
            sliderThumbStyle.fixedWidth = 20.0f;
            sliderThumbStyle.fixedHeight = 12.0f;

            // setup GUI resizer - only for the demo
            GUIResizer.Init(800, 500);

            // Get map instance to Globe API methods
            map = WorldMapGlobe.instance;

            // Setup grid events
            map.OnCellEnter += (int cellIndex) => Debug.Log("Entered cell: " + cellIndex);
            map.OnCellExit += (int cellIndex) => Debug.Log("Exited cell: " + cellIndex);
            map.OnCellClick += HandleOnCellClick;


        }

        void HandleOnCellClick(int cellIndex) {
            Debug.Log("Clicked cell: " + cellIndex);

            switch (selectionMode) {
                case SELECTION_MODE.CUSTOM_PATH:
                    if (selectionState == 0) {
                        firstCell = cellIndex;
                        map.SetCellColor(firstCell, Color.green, true);
                        selectionState = 1;
                    } else {
                        DrawPath(firstCell, cellIndex);
                        selectionState = 0;
                    }
                    break;
                case SELECTION_MODE.CUSTOM_COST:
                    if (selectionState == 0) {
                        firstCell = cellIndex;
                        map.SetCellColor(firstCell, Color.green, true);
                        selectionState = 1;
                    } else {
                        // Assign crossing cost between previously selected cell and clicked cell
                        map.SetCellNeighbourCost(firstCell, cellIndex, 1000, true);
                        // Draw a line
                        Vector3[] points = map.GetCellSharedEdge(firstCell, cellIndex);
                        if (points != null) {
                            map.ClearCells(true, false, false);
                            map.AddLine(points[0], points[1], Color.yellow, 0, 0, 0.002f, 0);
                            // Switch selection mode back to select first cell
                        }
                        selectionState = 0;
                    }
                    break;
            }

        }

        void OnGUI() {

            // Do autoresizing of GUI layer
            GUIResizer.AutoResize();

            // Slider for the divisions the travel progress
            GUI.Label(new Rect(10, 10, 90, 30), "Divisions:", labelStyle);
            GUI.backgroundColor = Color.white;
            map.hexaGridDivisions = (int)GUI.HorizontalSlider(new Rect(100, 20, 200, 35), map.hexaGridDivisions, 15, 200, sliderStyle, sliderThumbStyle);

            // Mask?
            map.hexaGridUseMask = GUI.Toggle(new Rect(10, 40, 150, 30), map.hexaGridUseMask, "Toggle Water Mask");

            // Path find
            if (GUI.Button(new Rect(10, 80, 120, 30), "  Random Path")) {
                RandomPathFind();
            }

            // Path find
            if (selectionMode != SELECTION_MODE.CUSTOM_PATH) {
                if (GUI.Button(new Rect(10, 120, 120, 30), "  Custom Path")) {
                    selectionMode = SELECTION_MODE.CUSTOM_PATH;
                    selectionState = 0;
                }
            } else {
                if (GUI.Button(new Rect(10, 120, 120, 30), "  Cancel")) {
                    selectionMode = SELECTION_MODE.NONE;
                }
            }

            // Path find
            if (GUI.Button(new Rect(10, 160, 120, 30), "  Paint Country")) {
                PaintCountry();
            }

            // Neighbour costs
            if (selectionMode != SELECTION_MODE.CUSTOM_COST) {
                if (GUI.Button(new Rect(10, 200, 120, 30), "  Set Edge Cost")) {
                    selectionMode = SELECTION_MODE.CUSTOM_COST;
                    selectionState = 0;
                }
            } else {
                if (GUI.Button(new Rect(10, 200, 120, 30), "  Cancel")) {
                    selectionMode = SELECTION_MODE.NONE;
                }
            }

            if (GUI.Button(new Rect(10, 240, 120, 30), "  Build Road")) {
                BuildRoad();
            }


            if (map.lastHighlightedCellIndex >= 0) {
                GUI.Label(new Rect(10, 280, 120, 30), "Current cell: " + map.lastHighlightedCellIndex);
            }

            switch (selectionMode) {
                case SELECTION_MODE.CUSTOM_PATH:
                    if (selectionState == 0) {
                        GUI.Label(new Rect(10, 300, 200, 30), "Click to select first cell.");
                    } else {
                        GUI.Label(new Rect(10, 300, 200, 30), "Click to select second cell.");
                    }
                    break;
                case SELECTION_MODE.CUSTOM_COST:
                    if (selectionState == 0) {
                        GUI.Label(new Rect(10, 300, 200, 30), "Click to a cell.");
                    } else {
                        GUI.Label(new Rect(10, 300, 200, 30), "Click to select adjacent cell.");
                    }
                    break;
            }
        }


        /// <summary>
        /// Computes the shortest path between two random cells of the hexagonal grid
        /// </summary>
        void RandomPathFind() {

            int startCellIndex = -1, endCellIndex = -1;

            for (int k = 0; k < 1000; k++) {  // This loop ensure enough tries while avoiding infinite search
                startCellIndex = GetRandomVisibleCellIndex();
                endCellIndex = GetRandomVisibleCellIndex();
                if (DrawPath(startCellIndex, endCellIndex))
                    break;
            }

            // Navigate to show the path
            map.FlyToCell(startCellIndex, 2f);

        }

        /// <summary>
        /// Draws a path between startCellIndex and endCellIndex
        /// </summary>
        /// <returns><c>true</c>, if path was found and drawn, <c>false</c> otherwise.</returns>
        /// <param name="startCellIndex">Start cell index.</param>
        /// <param name="endCellIndex">End cell index.</param>
        bool DrawPath(int startCellIndex, int endCellIndex) {

            List<int> cellIndices = map.FindPath(startCellIndex, endCellIndex);
            map.ClearCells(true, false, false);
            if (cellIndices == null)
                return false;   // no path found

            // Color starting cell, end cell and path
            map.SetCellColor(cellIndices, Color.gray, true);
            map.SetCellColor(startCellIndex, Color.green, true);
            map.SetCellColor(endCellIndex, Color.red, true);

            return true;
        }

        /// <summary>
        /// Looks for a random cell which is visible.
        /// </summary>
        int GetRandomVisibleCellIndex() {
            Vector3 pos = Random.onUnitSphere * 0.5f;
            int cellIndex = map.GetCellIndex(pos);
            return cellIndex;
        }


        /// <summary>
        /// Colorized all cells belonging to a country
        /// </summary>
        void PaintCountry() {
            Country country = map.GetCountry("Algeria");
            map.SetCellColor(country, new Color(1, 1, 0, 0.5f));
            map.FlyToCountry(country);
        }



        int GetRandomEuropeanCity() {
            int v = Random.Range(1, map.cities.Count);
            int tries = v;
            int cityIndex = 0;
            int cityCount = map.cities.Count;
            while (tries > 0) {
                cityIndex++;
                if (cityIndex >= cityCount) {
                    cityIndex = 0;
                }
                int countryIndex = map.cities[cityIndex].countryIndex;
                if (map.countries[countryIndex].continent.Equals("Europe")) {
                    tries--;
                }
            }
            return cityIndex;
        }

        void BuildRoad() {


            // Get a path between both cities
            List<int> cellIndices = null;
            do {
                // Find 2 random cities in Europe
                int city1Index = GetRandomEuropeanCity();
                int city2Index = GetRandomEuropeanCity();
                // Underline cell indices?
                int cell1Index = map.GetCellIndex(map.GetCity(city1Index).latlon);
                int cell2Index = map.GetCellIndex(map.GetCity(city2Index).latlon);
                if (cell1Index == cell2Index)
                    continue;
                // Get the path
                cellIndices = map.FindPath(cell1Index, cell2Index, 0, true);
            } while (cellIndices == null);

            // Highlight road cells
            StartCoroutine(IlluminateRoadCells(cellIndices));

            // Get lat/lon coordinates for the path
            int positionsCount = cellIndices.Count;
            Vector2[] latLons = new Vector2[positionsCount];
            for (int k = 0; k < positionsCount; k++) {
                latLons[k] = map.cells[cellIndices[k]].latlonCenter;
            }

            // Build a road along the map coordinates
            LineMarkerAnimator lma = map.AddLine(latLons, Color.white, 0.1f);
            lma.lineMaterial.mainTexture = roadTexture;
            lma.lineMaterial.mainTextureScale = new Vector2(2f, 1f);

            // Go to there!
            map.FlyToLocation(latLons[0].x, latLons[0].y, 1f, 0.4f);
        }

        IEnumerator IlluminateRoadCells(List<int> cellIndices) {
            WaitForEndOfFrame nextFrame = new WaitForEndOfFrame();
            for (int k = 0; k < cellIndices.Count; k++) {
                map.SetCellColor(cellIndices[k], new Color(1, 1, 0, 0.5f));
                yield return nextFrame;
            }
            for (int k = 0; k < cellIndices.Count; k++) {
                map.ClearCell(cellIndices[k]);
                yield return nextFrame;
            }

        }
    }
}