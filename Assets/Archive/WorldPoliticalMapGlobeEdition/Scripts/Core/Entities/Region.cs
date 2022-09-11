using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WPM.Poly2Tri;

namespace WPM {
    public partial class Region {

        Vector2[] _latlon;

        /// <summary>
        /// Region border in lat/lon coordinates.
        /// </summary>
        public Vector2[] latlon {
            get { return _latlon; }
            set {
                _latlon = value;
                UpdateSpherePointsFromLatLon();
            }
        }

        Vector3[] _spherePoints;

        /// <summary>
        /// Region border in spherical coordinates. These values are computed upon calling Region.ComputeSphereCoordiantes()
        /// </summary>
        public Vector3[] spherePoints {
            get { return _spherePoints; }
            set {
                _spherePoints = value;
                UpdateLatLonFromSpherePoints();
            }
        }


        Vector2 _latlonCenter;

        /// <summary>
        /// Center of this region
        /// </summary>
        public Vector2 latlonCenter {
            get { return _latlonCenter; }
            set {
                _latlonCenter = value;
                _sphereCenter = Conversion.GetSpherePointFromLatLon(_latlonCenter);
            }
        }

        Vector3 _sphereCenter;

        public Vector3 sphereCenter { get { return _sphereCenter; } }

        Rect _latlonRect2D;

        /// <summary>
        /// 2D rect enclosing all points
        /// </summary>
        public Rect latlonRect2D {
            get { return _latlonRect2D; }
            set {
                _latlonRect2D = value;
                _rect2Dbillboard = Conversion.GetBillboardRectFromLatLonRect(_latlonRect2D);
            }
        }

        Rect _rect2Dbillboard;

        public Rect rect2Dbillboard {
            get { return _rect2Dbillboard; }
        }

        /// <summary>
        /// Equals to rect2D.width * rect2D.height. Precomputed for performance purposes in comparison functions
        /// </summary>
        public float rect2DArea;

        public Material customMaterial;
        public Vector2 customTextureScale, customTextureOffset;
        public float customTextureRotation;
        public bool customOutline;
        public Color customOutlineColor;

        public List<Region> neighbours { get; set; }

        public IAdminEntity entity { get; set; }
        // country or province index
        public int regionIndex { get; set; }

        /// <summary>
        /// Some operations require to sanitize the region point list. This flag determines if the point list changed and should pass a sanitize call.
        /// </summary>
        public bool sanitized;

        /// <summary>
        /// Reference to the surface gameobject when colored/textured
        /// </summary>
		public GameObject surfaceGameObject;


        public Region(IAdminEntity entity, int regionIndex) {
            this.entity = entity;
            this.regionIndex = regionIndex;
            this.sanitized = true;
            neighbours = new List<Region>();
        }

        public Region Clone() {
            Region c = new Region(entity, regionIndex);
            c._latlonCenter = this._latlonCenter;
            c._latlonRect2D = this._latlonRect2D;
            c._rect2Dbillboard = this._rect2Dbillboard;
            c._sphereCenter = this._sphereCenter;
            c.customMaterial = this.customMaterial;
            c.customTextureScale = this.customTextureScale;
            c.customTextureOffset = this.customTextureOffset;
            c.customTextureRotation = this.customTextureRotation;
            c.customOutline = this.customOutline;
            c.customOutlineColor = customOutlineColor;
            c._spherePoints = new Vector3[_spherePoints.Length];
            Array.Copy(_spherePoints, c._spherePoints, _spherePoints.Length);
            c._latlon = new Vector2[_latlon.Length];
            Array.Copy(_latlon, c._latlon, _latlon.Length);
            return c;
        }


        public void Clear() {
            _latlon = new Vector2[0];
            _spherePoints = new Vector3[0];
            _latlonRect2D = new Rect(0, 0, 0, 0);
            _rect2Dbillboard = _latlonRect2D;
        }


        public void UpdateSpherePointsFromLatLon() {
            if (latlon == null)
                return;
            int pointCount = latlon.Length;
            if (spherePoints == null || spherePoints.Length != pointCount) {
                _spherePoints = new Vector3[pointCount];
            }

            for (int k = 0; k < pointCount; k++) {
                _spherePoints[k] = Conversion.GetSpherePointFromLatLon(_latlon[k]);
            }
        }


        public void UpdateLatLonFromSpherePoints() {
            if (spherePoints == null)
                return;
            int pointCount = spherePoints.Length;
            if (_latlon == null || _latlon.Length != pointCount) {
                _latlon = new Vector2[pointCount];
            }
            for (int k = 0; k < pointCount; k++) {
                _latlon[k] = Conversion.GetLatLonFromSpherePoint(_spherePoints[k]);
            }
        }


        public bool Contains(float lat, float lon) {
            return Contains(new Vector2(lat, lon));
        }

        /// <summary>
        /// p = (lat/lon)
        /// </summary>
        public bool Contains(Vector2 p) {

            if (!latlonRect2D.Contains(p)) {
                // check world-edge-crossing country
                p.y += 360;
                if (!latlonRect2D.Contains(p)) {
                    return false;
                }
            }

            int numPoints = latlon.Length;
            int j = numPoints - 1;
            bool inside = false;
            for (int i = 0; i < numPoints; j = i++) {
                if (((latlon[i].y <= p.y && p.y < latlon[j].y) || (latlon[j].y <= p.y && p.y < latlon[i].y)) &&
                    (p.x < (latlon[j].x - latlon[i].x) * (p.y - latlon[i].y) / (latlon[j].y - latlon[i].y) + latlon[i].x))
                    inside = !inside;
            }
            return inside;
        }

        public bool Contains(Region other) {

            if (!latlonRect2D.Overlaps(other.latlonRect2D))
                return false;

            int numPoints = other.latlon.Length;
            for (int i = 0; i < numPoints; i++) {
                if (!Contains(other.latlon[i]))
                    return false;
            }
            return true;
        }

        public bool Intersects(Region other) {

            Rect otherRect = other.latlonRect2D;

            if (otherRect.xMin > latlonRect2D.xMax)
                return false;
            if (otherRect.xMax < latlonRect2D.xMin)
                return false;
            if (otherRect.yMin > latlonRect2D.yMax)
                return false;
            if (otherRect.yMax < latlonRect2D.yMin)
                return false;

            int pointCount = latlon.Length;
            int otherPointCount = other.latlon.Length;

            for (int k = 0; k < otherPointCount; k++) {
                int j = pointCount - 1;
                bool inside = false;
                Vector2 p = other.latlon[k];
                for (int i = 0; i < pointCount; j = i++) {
                    if (((latlon[i].y <= p.y && p.y < latlon[j].y) || (latlon[j].y <= p.y && p.y < latlon[i].y)) &&
                        (p.x < (latlon[j].x - latlon[i].x) * (p.y - latlon[i].y) / (latlon[j].y - latlon[i].y) + latlon[i].x))
                        inside = !inside;
                }
                if (inside)
                    return true;
            }

            for (int k = 0; k < pointCount; k++) {
                int j = otherPointCount - 1;
                bool inside = false;
                Vector2 p = latlon[k];
                for (int i = 0; i < otherPointCount; j = i++) {
                    if (((other.latlon[i].y <= p.y && p.y < other.latlon[j].y) || (other.latlon[j].y <= p.y && p.y < other.latlon[i].y)) &&
                        (p.x < (other.latlon[j].x - other.latlon[i].x) * (p.y - other.latlon[i].y) / (other.latlon[j].y - other.latlon[i].y) + other.latlon[i].x))
                        inside = !inside;
                }
                if (inside)
                    return true;
            }

            return false;
        }




        /// <summary>
        /// Updates the region rect2D. Needed if points is updated manually.
        /// </summary>
        public void UpdatePointsAndRect(List<Vector2> newPoints) {
            sanitized = false;
            latlon = newPoints.ToArray();
            UpdateRect();
            UpdateSpherePointsFromLatLon();
        }

        /// <summary>
        /// Updates the region rect2D. Needed if points is updated manually.
        /// </summary>
        public void UpdatePointsAndRect(Vector2[] newPoints) {
            sanitized = false;
            latlon = newPoints;
            UpdateRect();
            UpdateSpherePointsFromLatLon();
        }

        /// <summary>
        /// Updates the region rect2D. Needed if points is updated manually.
        /// </summary>
        public void UpdateRect() {
            Vector2 min = new Vector2(1000f, 1000f);
            Vector2 max = -min;
            int pointCount = _latlon.Length;
            for (int k = 0; k < pointCount; k++) {
                float x = _latlon[k].x;
                float y = _latlon[k].y;
                if (x < min.x)
                    min.x = x;
                if (x > max.x)
                    max.x = x;
                if (y < min.y)
                    min.y = y;
                if (y > max.y)
                    max.y = y;
            }
            latlonRect2D = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
            rect2DArea = latlonRect2D.width * latlonRect2D.height;
            latlonCenter = (min + max) * 0.5f;
        }

        /// <summary>
        /// If this region crossed the West or East edge, shift the longitude of the other part
        /// </summary>
        public void CheckWorldEdgesAndOffset() {
            // if the separation between two adjacent points is greater than 180, split the region in two
            if (latlon == null || latlon.Length < 6)
                return;
            Vector2 latlon0 = _latlon[0];
            bool cross = false;
            for (int k = 1; k < _latlon.Length; k++) {
                Vector2 latlon1 = _latlon[k];
                float diff = latlon1.y - latlon0.y;
                if (diff > 270 || diff < -270) {
                    cross = true;
                    break;
                }
                latlon0 = latlon1;
            }
            if (cross) {
                for (int k = 0; k < _latlon.Length; k++) {
                    if (_latlon[k].y < 0) {
                        _latlon[k].y += 360;
                    }
                }
            }
        }


        /// <summary>
        /// Check if latlon needs to be shifted 360º
        /// </summary>
        public Vector2 AdjustLongitudeBeyond180(Vector2 latLon) {
            if (latlon == null || latlon.Length < 6)
                return latLon;
            Vector2 latlon0 = _latlon[0];
            if (latlon0.y > 180) latlon0.y -= 360;
            bool cross = false;
            for (int k = 1; k < _latlon.Length; k++) {
                Vector2 latlon1 = _latlon[k];
                if (latlon1.y > 180) latlon1.y -= 360;
                float diff = latlon1.y - latlon0.y;
                if (diff > 270 || diff < -270) {
                    cross = true;
                    break;
                }
                latlon0 = latlon1;
            }
            if (cross) {
                if (latLon.y < 0) {
                    latLon.y += 360f;
                }
            }
            return latLon;
        }



        /// <summary>
        /// If this region crossed the West or East edge, split it in two and add the second half to the regions list of the entity
        /// </summary>
        public void CheckWorldEdgesAndSplit() {

            // if the separation between two adjacent points is greater than 180, split the region in two
            if (latlon == null || latlon.Length < 6)
                return;
            Vector2 latlon0 = _latlon[0];
            bool split = false;
            for (int k = 1; k < _latlon.Length; k++) {
                Vector2 latlon1 = _latlon[k];
                float diff = latlon1.y - latlon0.y;
                if (diff > 270 || diff < -270) {
                    split = true;
                    break;
                }
                latlon0 = latlon1;
            }
            if (split) {
                List<Vector2> newRegionPoints = new List<Vector2>();
                List<Vector2> firstRegionPoints = newRegionPoints;
                Vector2 prev = Misc.Vector2zero;
                int regionsNewStart = entity.regions.Count;
                for (int k = 0; k < _latlon.Length; k++) {
                    if (prev.y * _latlon[k].y < 0) {
                        // crosses edge
                        float absPrevY = Mathf.Abs(prev.y);
                        float edgeDx = 179.999f - absPrevY;
                        float dx = (180f + _latlon[k].y) + (180f - absPrevY);
                        float dy = _latlon[k].x - prev.x;
                        float lat = prev.x + edgeDx * dy / dx;
                        float otherEdge = prev.y > 0 ? 179.999f : -179.999f;
                        float thisEdge = otherEdge * -1f;
                        newRegionPoints.Add(new Vector2(lat, otherEdge));
                        if (newRegionPoints != firstRegionPoints) {
                            // before adding a new region, check if any existing region contains last point. If so, add the points to that existing region
                            bool isNewRegion = true;
                            int regionsCount = entity.regions.Count;
                            for (int r = regionsNewStart; r < regionsCount; r++) {
                                if (entity.regions[r].Contains(prev)) {
                                    newRegionPoints.AddRange(entity.regions[r].latlon);
                                    entity.regions[r].UpdatePointsAndRect(newRegionPoints);
                                    isNewRegion = false;
                                    break;
                                }
                            }
                            // check if this region surrounds another region by checking it that other region center is contained, in that case add to that region instead
                            Region newRegion = new Region(entity, entity.regions.Count);
                            newRegion.UpdatePointsAndRect(newRegionPoints);
                            if (isNewRegion) {
                                for (int r = regionsNewStart; r < regionsCount; r++) {
                                    if (newRegion.Contains(entity.regions[r]._latlonCenter)) {
                                        newRegionPoints.AddRange(entity.regions[r].latlon);
                                        entity.regions[r].UpdatePointsAndRect(newRegionPoints);
                                        isNewRegion = false;
                                        break;
                                    }
                                }
                            }
                            if (isNewRegion) {
                                // add a new region
                                entity.regions.Add(newRegion);
                            }
                        }
                        newRegionPoints = new List<Vector2>();
                        newRegionPoints.Add(new Vector2(lat, thisEdge));
                    }
                    newRegionPoints.Add(_latlon[k]);
                    prev = _latlon[k];
                }
                // update this region which will be the West region
                firstRegionPoints.AddRange(newRegionPoints);
                UpdatePointsAndRect(firstRegionPoints);
            }

        }
    }
}