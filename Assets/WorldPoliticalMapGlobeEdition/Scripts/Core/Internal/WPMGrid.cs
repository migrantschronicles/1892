//#define TRACE_PERFORMANCE
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
    public delegate Point GetCachedPointDelegate(Point point);

    public delegate void GridCellEvent(int cellIndex);

    public delegate int PathFindingEvent(int cellIndex);

    public partial class WorldMapGlobe : MonoBehaviour {
        Material gridMatNonAlpha, gridMatAlpha;
        Material cellColoredMatNonAlpha, cellTexturedMatNonAlpha;
        Material cellColoredMatAlpha, cellTexturedMatAlpha;
        Material hexaGridHighlightMaterial;
        int lastHitCellIndex;
        Transform gridCellsRoot, gridWireframeRoot;
        int[] hexagonIndices = new int[] {
            0, 1, 5,
            1, 2, 5,
            4, 5, 2,
            3, 4, 2
        };
        Vector2[] hexagonUVs = new Vector2[] {
            new Vector2 (0, 0.5f),
            new Vector2 (0.25f, 0),
            new Vector2 (0.75f, 0),
            new Vector2 (1f, 0.5f),
            new Vector2 (0.75f, 1f),
            new Vector2 (0.25f, 1f)
        };
        int[] pentagonIndices = new int[] {
            0, 1, 4,
            1, 2, 4,
            3, 4, 2
        };
        Vector2[] pentagonUVs = new Vector2[] {
            new Vector2 (0, 0.75f),
            new Vector2 (0.25f, 0),
            new Vector2 (0.75f, 0),
            new Vector2 (1f, 0.75f),
            new Vector2 (0.5f, 1f),
        };
        Dictionary<Color, Material> colorCacheNonAlpha = new Dictionary<Color, Material>();
        Dictionary<Color, Material> textureCacheNonAlpha = new Dictionary<Color, Material>();
        Dictionary<Color, Material> colorCacheAlpha = new Dictionary<Color, Material>();
        Dictionary<Color, Material> textureCacheAlpha = new Dictionary<Color, Material>();
        Color32[] gridColors;
        int gridMaskWidthMinusOne, gridMaskHeightMinusOne;
        Material lastCachedMaterialWithNoTexture, lastCachedMaterialWithTexture;

        Material gridMat { get { return _hexaGridColor.a < 1f ? gridMatAlpha : gridMatNonAlpha; } }

        #region Gameloop events

        void InitGridSystem() {
            gridMatAlpha = Instantiate(Resources.Load<Material>("Materials/HexaGridMatNoExtrusionAlpha")) as Material;
            gridMatAlpha.hideFlags = HideFlags.DontSave;
            gridMatNonAlpha = Instantiate(Resources.Load<Material>("Materials/HexaGridMatNoExtrusion")) as Material;
            gridMatNonAlpha.hideFlags = HideFlags.DontSave;
            cellColoredMatNonAlpha = Instantiate(Resources.Load<Material>("Materials/HexaCellsMat")) as Material;
            cellColoredMatNonAlpha.hideFlags = HideFlags.DontSave;
            cellColoredMatAlpha = Instantiate(Resources.Load<Material>("Materials/HexaCellsMatAlpha")) as Material;
            cellColoredMatAlpha.hideFlags = HideFlags.DontSave;
            cellTexturedMatNonAlpha = Instantiate(Resources.Load<Material>("Materials/HexaCellsTexturedMat")) as Material;
            cellTexturedMatNonAlpha.hideFlags = HideFlags.DontSave;
            cellTexturedMatAlpha = Instantiate(Resources.Load<Material>("Materials/HexaCellsTexturedMatAlpha")) as Material;
            cellTexturedMatAlpha.hideFlags = HideFlags.DontSave;
            lastCachedMaterialWithNoTexture = Instantiate(cellColoredMatAlpha);
            lastCachedMaterialWithNoTexture.hideFlags = HideFlags.DontSave;
            lastCachedMaterialWithTexture = Instantiate(cellTexturedMatAlpha);
            lastCachedMaterialWithTexture.hideFlags = HideFlags.DontSave;

            if (hexaGridHighlightMaterial == null) {
                hexaGridHighlightMaterial = Instantiate(Resources.Load<Material>("Materials/HexaCellHighlightMat")) as Material;
                hexaGridHighlightMaterial.hideFlags = HideFlags.DontSave;
            }
            LoadGridMask();
            UpdateGridMaterialProperties();
        }

        void DestroyGridSystem() {
            if (gridMatAlpha != null)
                DestroyImmediate(gridMatAlpha);
            if (gridMatNonAlpha != null)
                DestroyImmediate(gridMatNonAlpha);
            if (cellColoredMatAlpha != null)
                DestroyImmediate(cellColoredMatAlpha);
            if (cellColoredMatNonAlpha != null)
                DestroyImmediate(cellColoredMatNonAlpha);
            if (cellTexturedMatAlpha != null)
                DestroyImmediate(cellTexturedMatAlpha);
            if (cellTexturedMatNonAlpha != null)
                DestroyImmediate(cellTexturedMatNonAlpha);
        }

        void UpdateHexagonalGrid() {
            if (hexaGridHighlightMaterial != null && lastHighlightedCellIndex >= 0) {
                hexaGridHighlightMaterial.SetFloat("_ColorShift", Mathf.PingPong(Time.time * _hexaGridHighlightSpeed, 1f));
            }
            if (mouseIsOver) {
                CheckHexGridUserInteraction();
            }
        }

        #endregion

        #region Interaction


        void CheckHexGridUserInteraction() {
            if (_hexaGridHighlightEnabled || OnCellEnter != null) {
                Vector3 localPosition = transform.InverseTransformPoint(sphereCurrentHitPos);
                int cellIndex = GetCellAtLocalPosition(localPosition);
                if (cellIndex >= 0 && cellIndex != lastHighlightedCellIndex) {
                    if (OnCellEnter != null) {
                        OnCellEnter(cellIndex);
                    }
                    if (lastHighlightedCell != null)
                        HideHighlightedCell();
                    lastHighlightedCell = cells[cellIndex];
                    lastHighlightedCellIndex = cellIndex;
                    if (_hexaGridHighlightEnabled) {
                        SetCellMaterial(lastHighlightedCellIndex, hexaGridHighlightMaterial);
                    }
                } else if (cellIndex < 0 && lastHighlightedCellIndex >= 0) {
                    HideHighlightedCell();
                }
            }

            if (lastHighlightedCellIndex >= 0 && !hasDragged && leftMouseButtonRelease && OnCellClick != null) {
                OnCellClick(lastHighlightedCellIndex);
            }
        }

        #endregion

        #region Hexasphere builder

        // internal fields
        const string HEXASPHERE_WIREFRAME = "WireFrame";
        const string HEXASPHERE_cellSROOT = "CellsRoot";
        const int HEXASPHERE_MAX_PARTS = 100;
        const int MAX_VERTEX_COUNT_PER_CHUNK = 65500;
        const int VERTEX_ARRAY_SIZE = 65530;
        Dictionary<Point, Point> points = new Dictionary<Point, Point>();
        Dictionary<Point, int> verticesIdx = new Dictionary<Point, int>();
        List<Vector3>[] verticesWire = new List<Vector3>[HEXASPHERE_MAX_PARTS];
        List<int>[] indicesWire = new List<int>[HEXASPHERE_MAX_PARTS];
        const float phi = 1.61803399f;
        float radius = 0.5f;
        Mesh[] wiredMeshes = new Mesh[HEXASPHERE_MAX_PARTS];
        bool shouldGenerateGrid;

        Point GetCachedPoint(Point point) {
            Point thePoint;
            if (points.TryGetValue(point, out thePoint)) {
                return thePoint;
            } else {
                points[point] = point;
                return point;
            }
        }

        /// <summary>
        /// Updates shader properties and generate hexasphere geometry if divisions or style has changed
        /// </summary>
        public void UpdateGridMaterialProperties() {
            if (!_showHexagonalGrid)
                return;

            if (hexaGridHighlightMaterial != null) {
                hexaGridHighlightMaterial.color = _hexaGridHighlightColor;
            }

            bool opaqueOld = gridMatAlpha.color.a >= 1f;
            bool opaqueNew = _hexaGridColor.a >= 1f;
            if (opaqueOld != opaqueNew)
                shouldGenerateGrid = true;

            if (gridMatAlpha != null) {
                gridMatAlpha.color = _hexaGridColor;
            }
            if (gridMatNonAlpha != null) {
                gridMatNonAlpha.color = _hexaGridColor;
            }

            if (shouldGenerateGrid) {
                GenerateGrid();
            }
        }


        /// <summary>
        /// Generate the hexasphere geometry.
        /// </summary>
        public void GenerateGrid() {
#if TRACE_PERFORMANCE
			DateTime dt = DateTime.Now;
#endif

            shouldGenerateGrid = false;

            Point[] corners = new Point[] {
                new Point (1, phi, 0),
                new Point (-1, phi, 0),
                new Point (1, -phi, 0),
                new Point (-1, -phi, 0),
                new Point (0, 1, phi),
                new Point (0, -1, phi),
                new Point (0, 1, -phi),
                new Point (0, -1, -phi),
                new Point (phi, 0, 1),
                new Point (-phi, 0, 1),
                new Point (phi, 0, -1),
                new Point (-phi, 0, -1)
            };

            if (_hexaGridRotationShift != Vector3.zero) {
                Quaternion q = Quaternion.Euler(_hexaGridRotationShift);
                for (int k = 0; k < corners.Length; k++) {
                    Point c = corners[k];
                    Vector3 v = (Vector3)c;
                    v = q * v;
                    c.x = v.x;
                    c.y = v.y;
                    c.z = v.z;
                }
            }

            Triangle[] triangles = new Triangle[] {
                new Triangle (corners [0], corners [1], corners [4], false),
                new Triangle (corners [1], corners [9], corners [4], false),
                new Triangle (corners [4], corners [9], corners [5], false),
                new Triangle (corners [5], corners [9], corners [3], false),
                new Triangle (corners [2], corners [3], corners [7], false),
                new Triangle (corners [3], corners [2], corners [5], false),
                new Triangle (corners [7], corners [10], corners [2], false),
                new Triangle (corners [0], corners [8], corners [10], false),
                new Triangle (corners [0], corners [4], corners [8], false),
                new Triangle (corners [8], corners [2], corners [10], false),
                new Triangle (corners [8], corners [4], corners [5], false),
                new Triangle (corners [8], corners [5], corners [2], false),
                new Triangle (corners [1], corners [0], corners [6], false),
                new Triangle (corners [11], corners [1], corners [6], false),
                new Triangle (corners [3], corners [9], corners [11], false),
                new Triangle (corners [6], corners [10], corners [7], false),
                new Triangle (corners [3], corners [11], corners [7], false),
                new Triangle (corners [11], corners [6], corners [7], false),
                new Triangle (corners [6], corners [0], corners [10], false),
                new Triangle (corners [9], corners [1], corners [11], false)
            };


            DestroyCachedCells(false);
            lastHighlightedCellIndex = -1;
            lastHighlightedCell = null;

            points.Clear();

            for (int i = 0; i < corners.Length; i++) {
                points[corners[i]] = corners[i];
            }

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 1 " + DateTime.Now);
#endif

            List<Point> bottom = new List<Point>();
            int triCount = triangles.Length;
            for (int f = 0; f < triCount; f++) {
                List<Point> prev = null;
                Point point0 = triangles[f].points[0];
                bottom.Clear();
                bottom.Add(point0);
                List<Point> left = point0.Subdivide(triangles[f].points[1], _hexaGridDivisions, GetCachedPoint);
                List<Point> right = point0.Subdivide(triangles[f].points[2], _hexaGridDivisions, GetCachedPoint);
                for (int i = 1; i <= _hexaGridDivisions; i++) {
                    prev = bottom;
                    bottom = left[i].Subdivide(right[i], i, GetCachedPoint);
                    new Triangle(prev[0], bottom[0], bottom[1]);
                    for (int j = 1; j < i; j++) {
                        new Triangle(prev[j], bottom[j], bottom[j + 1]);
                        new Triangle(prev[j - 1], prev[j], bottom[j]);
                    }
                }
            }

#if TRACE_PERFORMANCE
		Debug.Log ("Stage 2 " + DateTime.Now);
#endif
            int meshPointsCount = points.Values.Count;

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 2.1 " + DateTime.Now);
#endif

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 2.2 " + DateTime.Now);
#endif
            int p = 0;
            Point.flag = 0;
            cells = new Cell[meshPointsCount];
            foreach (Point point in points.Values) {
                cells[p] = new Cell(point, p);
                p++;
            }
#if TRACE_PERFORMANCE
			Debug.Log ("Stage 3 " + DateTime.Now);
#endif

            // Destroy placeholders
            Transform t = gameObject.transform.Find(HEXASPHERE_WIREFRAME);
            if (t != null)
                DestroyImmediate(t.gameObject);
            t = gameObject.transform.Find(HEXASPHERE_cellSROOT);
            if (t != null)
                DestroyImmediate(t.gameObject);

            // Check grid mask
            bool useGridMask = _hexaGridUseMask && gridColors != null;
            if (useGridMask) {
                for (int k = 0; k < cells.Length; k++) {
                    // Mask
                    Cell cell = cells[k];
                    // Convert cell center to texture coordinates
                    int colorIndex = Conversion.ConvertToTextureColorIndex(cell.sphereCenter, gridMaskWidthMinusOne, gridMaskHeightMinusOne);
                    cell.visible = (gridColors[colorIndex].b <= _hexaGridMaskThreshold);
                }
            }

            // Create meshes
            BuildWireFrame();


#if TRACE_PERFORMANCE
			Debug.Log ("Stage 3.1 " + DateTime.Now);
#endif

            needRefreshRouteMatrix = true;

#if TRACE_PERFORMANCE
			Debug.Log ("Stage 4 " + DateTime.Now);
			Debug.Log ("Time = " + (DateTime.Now - dt).TotalSeconds + " s.");
#endif
        }

        List<T> CheckList<T>(ref List<T> l) {
            if (l == null) {
                l = new List<T>(VERTEX_ARRAY_SIZE);
            } else {
                l.Clear();
            }
            return l;
        }

        void LoadGridMask() {
            if (_hexaGridMask == null) {
                _hexaGridMask = Resources.Load<Texture2D>("Textures/EarthMask");
                if (_hexaGridMask != null)
                    isDirty = true;
            }
            if (_hexaGridMask != null) {
                gridColors = _hexaGridMask.GetPixels32();
                gridMaskWidthMinusOne = _hexaGridMask.width - 1;
                gridMaskHeightMinusOne = _hexaGridMask.height - 1;
                shouldGenerateGrid = true;
            } else {
                gridColors = null;
            }
        }

        void BuildWireFrame() {

            if (!_showHexagonalGrid) return;

            int chunkIndex = 0;
            List<Vector3> vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
            List<int> indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);

            int pos;
            int verticesCount = -1;
            verticesIdx.Clear();
            int cellCount = cells.Length;
            for (int k = 0; k < cellCount; k++) {
                // Mask
                Cell cell = cells[k];
                if (!cell.visible)
                    continue;
                if (verticesCount > MAX_VERTEX_COUNT_PER_CHUNK) {
                    chunkIndex++;
                    vertexChunk = CheckList<Vector3>(ref verticesWire[chunkIndex]);
                    indicesChunk = CheckList<int>(ref indicesWire[chunkIndex]);
                    verticesIdx.Clear();
                    verticesCount = -1;
                }
                int pos0 = 0;
                Point[] cellVertices = cell.vertexPoints;
                int cellVerticesCount = cellVertices.Length;
                for (int b = 0; b < cellVerticesCount; b++) {
                    Point point = cellVertices[b];
                    if (!verticesIdx.TryGetValue(point, out pos)) {
                        vertexChunk.Add(point.projectedVector3);
                        verticesCount++;
                        pos = verticesCount;
                        verticesIdx[point] = pos;
                    }
                    indicesChunk.Add(pos);
                    if (b == 0) {
                        pos0 = pos;
                    } else {
                        indicesChunk.Add(pos);
                    }
                }
                indicesChunk.Add(pos0);
            }

            gridWireframeRoot = CreateGOandParent(gameObject.transform, HEXASPHERE_WIREFRAME).transform;
            for (int k = 0; k <= chunkIndex; k++) {
                GameObject go = CreateGOandParent(gridWireframeRoot, "Wire");
                MeshFilter mf = go.AddComponent<MeshFilter>();
                wiredMeshes[k] = new Mesh();
                wiredMeshes[k].hideFlags = HideFlags.DontSave;
                wiredMeshes[k].SetVertices(verticesWire[k]);
                wiredMeshes[k].SetIndices(indicesWire[k].ToArray(), MeshTopology.Lines, 0);
                mf.sharedMesh = wiredMeshes[k];
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                mr.sharedMaterial = gridMat;
            }

        }

        GameObject CreateGOandParent(Transform parent, string name) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Misc.Vector3zero;
            go.transform.localScale = Misc.Vector3one;
            go.transform.localRotation = Misc.QuaternionZero;
            return go;
        }

        #endregion

        #region Cell functions

        void GenerateCellMesh(int cellIndex, Material mat) {
            if (gridCellsRoot == null) {
                gridCellsRoot = CreateGOandParent(gameObject.transform, HEXASPHERE_cellSROOT).transform;
            }
            GameObject go = CreateGOandParent(gridCellsRoot, "Cell");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;
            Cell cell = cells[cellIndex];
            mesh.vertices = cell.vertices;
            int cellVerticesCount = cell.vertices.Length;
            if (cellVerticesCount == 6) {
                mesh.SetIndices(hexagonIndices, MeshTopology.Triangles, 0);
                mesh.uv = hexagonUVs;
            } else {
                mesh.SetIndices(pentagonIndices, MeshTopology.Triangles, 0);
                mesh.uv = pentagonUVs;
            }
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            cell.renderer = mr;
        }

        Material GetCachedMaterial(Color color, Texture2D texture = null) {
            Material mat;
            if (texture == null) {
                if (color == lastCachedMaterialWithNoTexture.color) {
                    return lastCachedMaterialWithNoTexture;
                }
                if (color.a < 1f) {
                    if (colorCacheAlpha.TryGetValue(color, out mat)) {
                        return mat;
                    }
                    mat = Instantiate(cellColoredMatAlpha) as Material;
                    colorCacheAlpha[color] = mat;
                } else {
                    if (colorCacheNonAlpha.TryGetValue(color, out mat)) {
                        return mat;
                    }
                    mat = Instantiate(cellColoredMatNonAlpha) as Material;
                    colorCacheNonAlpha[color] = mat;
                }
                lastCachedMaterialWithNoTexture = mat;
            } else {
                if (color == lastCachedMaterialWithTexture.color) {
                    return lastCachedMaterialWithTexture;
                }
                if (color.a < 1f) {
                    if (textureCacheAlpha.TryGetValue(color, out mat)) {
                        return mat;
                    }
                    mat = Instantiate(cellTexturedMatAlpha) as Material;
                    textureCacheAlpha[color] = mat;
                } else {
                    if (textureCacheNonAlpha.TryGetValue(color, out mat)) {
                        return mat;
                    }
                    mat = Instantiate(cellTexturedMatNonAlpha) as Material;
                    textureCacheNonAlpha[color] = mat;
                }
                mat.mainTexture = texture;
                lastCachedMaterialWithTexture = mat;
            }
            mat.hideFlags = HideFlags.DontSave;
            mat.color = color;
            return mat;
        }

        void HideHighlightedCell() {
            if (lastHitCellIndex >= 0 && OnCellExit != null)
                OnCellExit(lastHitCellIndex);
            if (lastHighlightedCellIndex >= 0 && lastHighlightedCell != null && lastHighlightedCell.renderer != null && lastHighlightedCell.renderer.sharedMaterial == hexaGridHighlightMaterial) {
                if (lastHighlightedCell.tempMat != null) {
                    CellRestoreTemporaryMaterial(lastHighlightedCellIndex);
                } else if (lastHighlightedCell.customMat != null) {
                    CellRestoreMaterial(lastHighlightedCellIndex);
                } else {
                    HideCell(lastHighlightedCellIndex);
                }
            }
            ResetHighlightMaterial();
            lastHighlightedCell = null;
            lastHighlightedCellIndex = -1;
        }

        void CellRestoreTemporaryMaterial(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            Cell cell = cells[cellIndex];
            if (cell.tempMat != null) {
                cell.renderer.sharedMaterial = cell.tempMat;
            }
        }

        void CellRestoreMaterial(int cellIndex) {
            if (cellIndex < 0 || cellIndex >= cells.Length)
                return;
            Cell cell = cells[cellIndex];
            if (cell.customMat != null) {
                cell.renderer.sharedMaterial = cell.customMat;
            }
        }

        void ResetHighlightMaterial() {
            if (hexaGridHighlightMaterial != null) {
                Color co = hexaGridHighlightMaterial.color;
                co.a = 0.2f;
                hexaGridHighlightMaterial.SetColor("_Color2", co);
                hexaGridHighlightMaterial.mainTexture = null;
            }
        }

        void RefreshHighlightedCell() {
            if (lastHighlightedCellIndex < 0 || lastHighlightedCellIndex >= cells.Length)
                return;
            SetCellMaterial(lastHighlightedCellIndex, hexaGridHighlightMaterial, true);
        }

        void DestroyCachedCells(bool preserveMaterials) {
            if (cells == null)
                return;

            HideHighlightedCell();
            for (int k = 0; k < cells.Length; k++) {
                Cell cell = cells[k];
                if (cell.renderer != null) {
                    DestroyImmediate(cell.renderer.gameObject);
                    cell.renderer = null;
                    if (!preserveMaterials) {
                        cell.customMat = null;
                        cell.tempMat = null;
                    }
                }
            }

        }

        Cell GetNearestCellToPosition(Cell[] cells, Vector3 localPosition, out float distance) {
            distance = float.MaxValue;
            Cell nearest = null;
            for (int k = 0; k < cells.Length; k++) {
                Cell cell = cells[k];
                Vector3 center = cell.sphereCenter;
                // unwrapped SqrMagnitude for performance considerations
                float dist = (center.x - localPosition.x) * (center.x - localPosition.x) + (center.y - localPosition.y) * (center.y - localPosition.y) + (center.z - localPosition.z) * (center.z - localPosition.z);
                if (dist < distance) {
                    nearest = cell;
                    distance = dist;
                }
            }
            return nearest;
        }

        int GetCellAtLocalPosition(Vector3 localPosition, bool reuseLastHitCell = true) {
            // If this the same cell? Heuristic: any neighour will be farther
            if (reuseLastHitCell && lastHitCellIndex >= 0 && lastHitCellIndex < cells.Length) {
                Cell lastHitCell = cells[lastHitCellIndex];
                if (lastHitCell != null && lastHitCell.visible) {
                    float dist = Vector3.SqrMagnitude(lastHitCell.sphereCenter - localPosition);
                    bool valid = true;
                    for (int k = 0; k < lastHitCell.neighbours.Length; k++) {
                        Cell otherCell = lastHitCell.neighbours[k];
                        float otherDist = Vector3.SqrMagnitude(otherCell.sphereCenter - localPosition);
                        if (otherDist < dist) {
                            valid = false;
                            break;
                        }
                    }
                    if (valid) {
                        return lastHitCellIndex;
                    }
                }
            } else {
                lastHitCellIndex = 0;
            }

            // follow the shortest path to the minimum distance
            Cell nearest = cells[lastHitCellIndex];
            float cellDist =
                (nearest.sphereCenter.x - localPosition.x) * (nearest.sphereCenter.x - localPosition.x) +
                (nearest.sphereCenter.y - localPosition.y) * (nearest.sphereCenter.y - localPosition.y) +
                (nearest.sphereCenter.z - localPosition.z) * (nearest.sphereCenter.z - localPosition.z);
            float minDist = float.MaxValue;
            Cell nearestVisible = nearest;
            for (int k = 0; k < cells.Length; k++) {
                Cell newNearest = GetNearestCellToPosition(nearest.neighbours, localPosition, out cellDist);
                if (cellDist < minDist) {
                    minDist = cellDist;
                    nearest = newNearest;
                    if (nearest.visible)
                        nearestVisible = nearest;
                } else {
                    break;
                }
            }
            if (nearest == nearestVisible) {
                lastHitCellIndex = nearestVisible.index;
                return lastHitCellIndex;
            } else {
                return -1;
            }
        }



        #endregion


    }

}