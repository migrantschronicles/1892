using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
    public class Cell {

        #region Public properties

        /// <summary>
        /// The index of this tile in the tiles list.
        /// </summary>
        public int index;


        /// <summary>
        /// The original points used to create the tile. Used internally. Use Vertices property to get the vertices in Vector3 format instead.
        /// </summary>
        public Point[] vertexPoints;

        /// <summary>
        /// Gets the center of this tile in local sphere coordinates.
        /// </summary>
        public Vector3 sphereCenter;

        /// <summary>
        /// Gets the vertices in local space coordinates. Note that the grid contains a few pentagons.
        /// </summary>
        public Vector3[] vertices {
            get {
                if (!_verticesComputed) {
                    int l = vertexPoints.Length;
                    _vertices = new Vector3[l];
                    for (int k = 0; k < l; k++) {
                        _vertices[k] = vertexPoints[k].projectedVector3;
                    }
                    _verticesComputed = true;
                }
                return _vertices;
            }
        }

        /// <summary>
        /// Gets the neighbours tiles.
        /// </summary>
        public Cell[] neighbours {
            get {
                if (!_neighboursComputed) {
                    ComputeNeighbours();
                }
                return _neighbours;
            }
        }

        /// <summary>
        /// Gets the vertices of the cell in lat/lon format
        /// </summary>
        public Vector2[] latlon {
            get {
                if (_latlon == null) {
                    ComputeLatLon();
                }
                return _latlon;
            }
        }

        /// <summary>
        /// Returns the cell center in lat/lon coordinates
        /// </summary>
        /// <value>The latlon center.</value>
        public Vector2 latlonCenter {
            get {
                if (_latlon == null) {
                    ComputeLatLon();
                }
                return _latlonCenter;
            }
        }

        /// <summary>
        /// Gets the neighbours tiles indices.
        /// </summary>
        public int[] neighboursIndices {
            get {
                if (!_neighboursComputed) {
                    ComputeNeighbours();
                }
                return _neighboursIndices;
            }
        }


        /// <summary>
        /// Additional cost for crossing a neighbour tile.
        /// </summary>
        public int[] neighboursCosts {
            get {
                if (_neighboursCosts == null) {
                    int neighbourCount = _neighbours != null ? _neighbours.Length : 6;
                    _neighboursCosts = new int[neighbourCount];
                    for (int k = 0; k < neighbourCount; k++) {
                        _neighboursCosts[k] = 0;
                    }
                }
                return _neighboursCosts;
            }
        }

        /// <summary>
        /// Sets if this tile can be crossed when using PathFinding functions.
        /// </summary>
        public bool canCross = true;

        /// <summary>
        /// The tile mesh's renderer. Created when SetCellColor or SetCellTexture is used. Get the tile gameobject using renderer.gameObject
        /// </summary>
        public Renderer renderer;

        /// <summary>
        /// The base material assigned to this tile. 
        /// </summary>
        public Material customMat;

        /// <summary>
        /// The temporary material assigned to this tile. 
        /// </summary>
        public Material tempMat;

        /// <summary>
        /// Extrude amount for this tile. 0 = no extrusion, will render a flat tile which is faster.
        /// </summary>
        public float extrudeAmount = 0f;
        public int uvShadedChunkIndex;
        public int uvShadedChunkStart;
        public int uvShadedChunkLength;
        public int uvWireChunkIndex;
        public int uvWireChunkStart;
        public int uvWireChunkLength;

        /// <summary>
        /// Original value loaded from the heightmap
        /// </summary>
        public float heightMapValue;

        /// <summary>
        /// User-defined misc value (not used by Hexasphere)
        /// </summary>
        public string tag;

        /// <summary>
        /// If the tile is visible or not. Defaults to true.
        /// </summary>
        public bool visible;

        #endregion

        #region Internal logic

        Vector3[] _vertices;
        bool _verticesComputed;
        Point centerPoint;
        Cell[] _neighbours;
        int[] _neighboursIndices;
        int[] _neighboursCosts;
        bool _neighboursComputed;
        static Triangle[] tempTriangles = new Triangle[20];
        Vector2[] _latlon;
        Vector2 _latlonCenter;

        public Cell(Point centerPoint, int index) {
            this.index = index;
            this.centerPoint = centerPoint;
            this.centerPoint.tile = this;
            this.sphereCenter = centerPoint.projectedVector3;
            this.visible = true;
            int facesCount = centerPoint.GetOrderedTriangles(tempTriangles);
            vertexPoints = new Point[facesCount];

            for (int f = 0; f < facesCount; f++) {
                vertexPoints[f] = tempTriangles[f].GetCentroid();
            }

            // resort if wrong order
            if (facesCount == 6) {
                Vector3 p0 = (Vector3)vertexPoints[0];
                Vector3 p1 = (Vector3)vertexPoints[1];
                Vector3 p5 = (Vector3)vertexPoints[5];
                Vector3 v0 = p1 - p0;
                Vector3 v1 = p5 - p0;
                Vector3 cp = Vector3.Cross(v0, v1);
                float dp = Vector3.Dot(cp, p1);
                if (dp < 0) {
                    Point aux;
                    aux = vertexPoints[0];
                    vertexPoints[0] = vertexPoints[5];
                    vertexPoints[5] = aux;
                    aux = vertexPoints[1];
                    vertexPoints[1] = vertexPoints[4];
                    vertexPoints[4] = aux;
                    aux = vertexPoints[2];
                    vertexPoints[2] = vertexPoints[3];
                    vertexPoints[3] = aux;
                }
            } else if (facesCount == 5) {
                Vector3 p0 = (Vector3)vertexPoints[0];
                Vector3 p1 = (Vector3)vertexPoints[1];
                Vector3 p4 = (Vector3)vertexPoints[4];
                Vector3 v0 = p1 - p0;
                Vector3 v1 = p4 - p0;
                Vector3 cp = Vector3.Cross(v0, v1);
                float dp = Vector3.Dot(cp, p1);
                if (dp < 0) {
                    Point aux;
                    aux = vertexPoints[0];
                    vertexPoints[0] = vertexPoints[4];
                    vertexPoints[4] = aux;
                    aux = vertexPoints[1];
                    vertexPoints[1] = vertexPoints[3];
                    vertexPoints[3] = aux;
                }
            }
        }

        static List<int> tempInt = new List<int>(6);
        static List<Cell> temp = new List<Cell>(6);

        void ComputeNeighbours() {
            tempInt.Clear();
            temp.Clear();
            for (int k = 0; k < centerPoint.triangleCount; k++) {
                Triangle other = centerPoint.triangles[k];
                for (int j = 0; j < 3; j++) {
                    Cell tile = other.points[j].tile;
                    if (tile != null && !other.points[j].Equals(centerPoint) && !temp.Contains(tile)) {
                        temp.Add(tile);
                        tempInt.Add(tile.index);
                    }
                }
            }
            _neighbours = temp.ToArray();
            _neighboursIndices = tempInt.ToArray();
            _neighboursComputed = true;
        }

        public int GetNeighbourCost(int neighbourIndex) {
            if (_neighboursCosts == null || neighbourIndex < 0 || neighbourIndex >= _neighboursCosts.Length) {
                return 0;
            }
            return _neighboursCosts[neighbourIndex];
        }

        void ComputeLatLon() {
            Vector3[] verts = vertices;
            _latlon = new Vector2[verts.Length];
            Vector3 midPoint = Misc.Vector3zero;
            if (_latlon.Length > 0) {
                for (int k = 0; k < verts.Length; k++) {
                    _latlon[k] = Conversion.GetLatLonFromSpherePoint(verts[k]);
                    midPoint += verts[k];
                }
                // the average lat/lon must be done in cartesian coordinates (3D) to avoid summing negative and positive latitude / longitudes which lend to incorrect average position
                midPoint /= verts.Length;
                _latlonCenter = Conversion.GetLatLonFromSpherePoint(midPoint);
            }
        }

        #endregion
    }
}