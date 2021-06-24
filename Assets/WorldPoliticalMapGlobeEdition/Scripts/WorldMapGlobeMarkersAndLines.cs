// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPM {

    public enum MARKER_TYPE {
        CIRCLE = 0,
        CIRCLE_PROJECTED = 1,
        QUAD = 2
    }

    public enum GRID_MODE {
        OVERLAY = 0,
        MASKED = 1
    }

    public enum CURSOR_STYLE {
        Legacy = 0,
        LatitudeLongitudeCursor = 1
    }

    /* Public WPM Class */
    public partial class WorldMapGlobe : MonoBehaviour {


        [SerializeField]
        bool
            _showCursor = true;

        /// <summary>
        /// Toggle cursor lines visibility.
        /// </summary>
        public bool showCursor {
            get {
                return _showCursor;
            }
            set {
                if (value != _showCursor) {
                    _showCursor = value;
                    isDirty = true;

                    if (cursorLayer != null) {
                        cursorLayer.SetActive(_showCursor);
                    }
                }
            }
        }
     

        [SerializeField]
        CURSOR_STYLE
            _cursorStyle = CURSOR_STYLE.LatitudeLongitudeCursor;

        /// <summary>
        /// Gets/sets cursor style.
        /// </summary>
        public CURSOR_STYLE cursorStyle {
            get {
                return _cursorStyle;
            }
            set {
                if (value != _cursorStyle) {
                    _cursorStyle = value;
                    isDirty = true;
                    DrawCursor();
                }
            }
        }

        /// <summary>
        /// Cursor lines color.
        /// </summary>
        [SerializeField]
        Color
            _cursorColor = new Color(0.56f, 0.47f, 0.68f);

        public Color cursorColor {
            get {
                if (cursorMat != null) {
                    return cursorMat.color;
                } else {
                    return _cursorColor;
                }
            }
            set {
                if (value != _cursorColor) {
                    _cursorColor = value;
                    isDirty = true;

                    if (cursorMat != null && _cursorColor != cursorMat.color) {
                        cursorMat.color = _cursorColor;
                    }
                }
            }
        }

        [SerializeField]
        bool
            _cursorFollowMouse = true;

        /// <summary>
        /// Makes the cursor follow the mouse when it's over the World.
        /// </summary>
        public bool cursorFollowMouse {
            get {
                return _cursorFollowMouse;
            }
            set {
                if (value != _cursorFollowMouse) {
                    _cursorFollowMouse = value;
                    isDirty = true;
                }
            }
        }

        [NonSerialized]
        Vector3
            _cursorLocation;

        public Vector3 cursorLocation {
            get {
                return _cursorLocation;
            }
            set {
                if (_cursorLocation.x != value.x || _cursorLocation.y != value.y || _cursorLocation.z != value.z) {
                    _cursorLocation = value;
                    if (_cursorFollowMouse && cursorLayer != null) {
                        switch (_cursorStyle) {
                            case CURSOR_STYLE.Legacy: {
                                    Vector3 wpos = transform.TransformPoint(_cursorLocation);
                                    cursorLayer.transform.LookAt(wpos, transform.up);
                                }
                                break;
                            case CURSOR_STYLE.LatitudeLongitudeCursor: {
                                    // Adjust vertical half cursor
                                    Vector3 wpos = transform.TransformPoint(_cursorLocation);
                                    cursorVertical.transform.LookAt(wpos, transform.up);
                                    cursorHorizontal.transform.LookAt(wpos, transform.up);
                                    // Adjust horizontal half cursor
                                    Vector3 c = _cursorLocation;
                                    c.x *= 2f;
                                    c.z *= 2f;
                                    float s = Mathf.Sqrt(c.x * c.x + c.z * c.z);
                                    c.x = c.z = 0;
                                    cursorHorizontal.transform.localPosition = c;
                                    cursorHorizontal.transform.localScale = new Vector3(s, 1, s);
                                }
                                break;
                        }

                    }
                }
            }
        }


        /// <summary>
        /// If set to false, cursor will be hidden when mouse if not over the globe.
        /// </summary>
        [SerializeField]
        bool
            _cursorAllwaysVisible = true;

        public bool cursorAlwaysVisible {
            get {
                return _cursorAllwaysVisible;
            }
            set {
                if (value != _cursorAllwaysVisible) {
                    _cursorAllwaysVisible = value;
                    isDirty = true;
                    CheckCursorVisibility();
                }
            }
        }

        [SerializeField]
        bool
            _showLatitudeLines = true;

        /// <summary>
        /// Toggle latitude lines visibility.
        /// </summary>
        public bool showLatitudeLines {
            get {
                return _showLatitudeLines;
            }
            set {
                if (value != _showLatitudeLines) {
                    _showLatitudeLines = value;
                    isDirty = true;

                    if (latitudeLayer != null) {
                        latitudeLayer.SetActive(_showLatitudeLines);
                    } else {
                        DrawLatitudeLines();
                    }
                }
            }
        }

        [SerializeField]
        [Range(5.0f, 45.0f)]
        int
            _latitudeStepping = 15;

        /// <summary>
        /// Specify latitude lines separation.
        /// </summary>
        public int latitudeStepping {
            get {
                return _latitudeStepping;
            }
            set {
                if (value != _latitudeStepping) {
                    _latitudeStepping = value;
                    isDirty = true;

                    DrawLatitudeLines();
                }
            }
        }

        [SerializeField]
        bool
            _showLongitudeLines = true;

        /// <summary>
        /// Toggle longitude lines visibility.
        /// </summary>
        public bool showLongitudeLines {
            get {
                return _showLongitudeLines;
            }
            set {
                if (value != _showLongitudeLines) {
                    _showLongitudeLines = value;
                    isDirty = true;

                    if (longitudeLayer != null) {
                        longitudeLayer.SetActive(_showLongitudeLines);
                    } else {
                        DrawLongitudeLines();
                    }
                }
            }
        }

        [SerializeField]
        [Range(5.0f, 45.0f)]
        int
            _longitudeStepping = 15;

        /// <summary>
        /// Specify longitude lines separation.
        /// </summary>
        public int longitudeStepping {
            get {
                return _longitudeStepping;
            }
            set {
                if (value != _longitudeStepping) {
                    _longitudeStepping = value;
                    isDirty = true;

                    DrawLongitudeLines();
                }
            }
        }

        [SerializeField]
        Color
            _gridColor = new Color(0.16f, 0.33f, 0.498f);

        /// <summary>
        /// Color for imaginary lines (longitude and latitude).
        /// </summary>
        public Color gridLinesColor {
            get {
                return _gridColor;
            }
            set {
                if (value != _gridColor) {
                    _gridColor = value;
                    isDirty = true;

                    if (gridMatOverlay != null && _gridColor != gridMatOverlay.color) {
                        gridMatOverlay.color = _gridColor;
                    }
                    if (gridMatMasked != null && _gridColor != gridMatMasked.color) {
                        gridMatMasked.color = _gridColor;
                    }
                }
            }
        }

        [SerializeField]
        GRID_MODE _gridMode = GRID_MODE.OVERLAY;

        public GRID_MODE gridMode {
            get {
                return _gridMode;
            }
            set {
                if (value != _gridMode) {
                    _gridMode = value;
                    isDirty = true;
                    DrawGrid();
                }
            }
        }


        #region Public API area

        /// <summary>
        /// Adds a text label over the globe.
        /// </summary>
        /// <returns>The TextMesh component attached to the label gameobject.</returns>
        /// <param name="sphereLocation">Sphere location.</param>
        /// <param name="name">Text label.</param>
        public TextMesh AddText(string name, Vector3 sphereLocation, Color color, float scale = 0.004f, Font font = null, FontStyle fontStyle = FontStyle.Normal) {
            if (fontMaterial == null) {
                fontMaterial = Instantiate<Material>(Resources.Load<Material>("Materials/Font"));
            }
            GameObject go = new GameObject(name);
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = sphereLocation; // <-- the location of the city in spherical coordinates
            go.transform.localScale = Vector3.one * scale;
            go.transform.LookAt(transform.position, transform.up);
            TextMesh tm = go.AddComponent<TextMesh>();
            if (font != null) {
                tm.font = font;
            }
            if (fontStyle != FontStyle.Normal) {
                tm.fontStyle = fontStyle;
            }
            fontMaterial.mainTexture = tm.font.material.mainTexture;
            go.GetComponent<Renderer>().sharedMaterial = fontMaterial;
            tm.text = name;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = color;
            return tm;
        }

        public TextMesh AddTextCustom(string name, Vector3 sphereLocation, Color color, float scale = 0.004f, Font font = null, FontStyle fontStyle = FontStyle.Normal)
        {
            if (fontMaterial == null)
            {
                fontMaterial = Instantiate<Material>(Resources.Load<Material>("Materials/Font"));
            }
            GameObject go = new GameObject(name);
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = sphereLocation; // <-- the location of the city in spherical coordinates
            go.transform.localScale = Vector3.one * scale;
            go.transform.LookAt(_pivotTransform.position);
            TextMesh tm = go.AddComponent<TextMesh>();
            if (font != null)
            {
                tm.font = font;
            }
            if (fontStyle != FontStyle.Normal)
            {
                tm.fontStyle = fontStyle;
            }
            fontMaterial.mainTexture = tm.font.material.mainTexture;
            fontMaterial.renderQueue = 5000;
            go.GetComponent<Renderer>().sharedMaterial = fontMaterial;
            tm.text = name;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = color;
            tm.characterSize = 0.03f;
            tm.fontSize = 350;
            return tm;
        }

        public void UpdateTextCustom(TextMesh tm, float scale)
        {
            tm.transform.LookAt(new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + pitch, -mainCamera.transform.position.z));
            tm.transform.localScale = Vector3.one * scale;
        }


        /// <summary>
        /// Adds a custom marker (gameobject) to the globe on specified location and with custom scale.
        /// </summary>
        public void AddMarker(GameObject marker, Vector3 sphereLocation, float markerScale) {
            mAddMarker(marker, sphereLocation, markerScale, false, 0f, true);
        }

        /// <summary>
        /// Adds a custom marker (gameobject) to the globe on specified location and with custom scale.
        /// </summary>
        /// <param name="isBillboard">If set to <c>true</c> game object will be oriented to position normal facing outside.</param>
        /// <param name="surfaceSeparation">Makes the marker a little bit off the surface to prevent clipping with other elements like city spots.</param>
        /// <param name="baseLineAtBottom">Takes into account the height of the gameobject mesh and uses the bottom of the object as the reference instead of the middle.</param>
        public void AddMarker(GameObject marker, Vector3 sphereLocation, float markerScale, bool isBillboard, float surfaceSeparation = 0f, bool baseLineAtBottom = false, bool preserveOriginalRotation = true) {
            mAddMarker(marker, sphereLocation, markerScale, isBillboard, surfaceSeparation, baseLineAtBottom, preserveOriginalRotation);
        }

        public void UpdateMarkerPosition(GameObject marker, Vector3 sphereLocation, float markerScale, float surfaceSeparation = 0f, bool baseLineAtBottom = false)
        {
            float height = 0;
            if (baseLineAtBottom)
            {
                if (marker.GetComponent<MeshFilter>() != null)
                    height = marker.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
                else if (marker.GetComponent<Collider>() != null)
                    height = marker.GetComponent<Collider>().bounds.size.y;
            }
            height += surfaceSeparation;
            var h = height * markerScale / sphereLocation.magnitude;
            marker.transform.localPosition = _earthInvertedMode ? sphereLocation * (1.0f - h) : sphereLocation * (1.0f + h * 0.5f);
        }

        /// <summary>
        /// Adds a custom circle polygon to the globe on specified location and with custom size in km.
        /// </summary>
        /// <param name="type">Polygon type.</param>
        /// <param name="sphereLocation">Sphere location.</param>
        /// <param name="kmRadius">Radius in KM.</param>
        /// <param name="ringWidthStart">Ring inner limit (0..1). Pass 0 to draw a full circle.</param>
        /// <param name="ringWidthEnd">Ring outer limit (0..1). Pass 1 to draw a full circle.</param>
        /// <param name="color">Color</param>
        public GameObject AddMarker(MARKER_TYPE type, Vector3 sphereLocation, float kmRadius, float ringWidthStart, float ringWidthEnd, Color color) {
            return mAddMarkerCircle(type, sphereLocation, kmRadius, ringWidthStart, ringWidthEnd, color);
        }

        /// <summary>
        /// Adds a quad polygon to the globe on specified location and with custom size.
        /// </summary>
        /// <param name="type">Polygon type.</param>
        /// <param name="position">Sphere coordinates of the center of the quad.</param>
        /// <param name="size">size of quad measured in differences of latitude/longitudes.</param>
        /// <param name="color">Color</param>
        public GameObject AddMarker(MARKER_TYPE type, Vector3 sphereLocationTopLeft, Vector3 sphereLocationBottomRight, Color fillColor, Color borderColor = default(Color), float borderWidth = 0) {
            return mAddMarkerQuad(type, sphereLocationTopLeft, sphereLocationBottomRight, fillColor, borderColor, borderWidth);
        }


        /// <summary>
        /// Adds a line to the globe with options (returns the line gameobject).
        /// </summary>
        /// <param name="Color">line color</param>
        /// <param name="arcElevation">arc elevation relative to the sphere size (0-1 range).</param>
        /// <param name="duration">drawing speed (0 for instant drawing)</param>
        /// <param name="fadeOutAfter">duration of the line once drawn after which it fades out (set this to 0 to make the line stay forever)</param>
        public LineMarkerAnimator AddLine(Vector2 latLonStart, Vector2 latLonEnd, Color color, float arcElevation, float duration, float lineWidth, float fadeOutAfter) {
            Vector3 start = Conversion.GetSpherePointFromLatLon(latLonStart.x, latLonStart.y);
            Vector3 end = Conversion.GetSpherePointFromLatLon(latLonEnd.x, latLonEnd.y);
            return AddLine(start, end, color, arcElevation, duration, lineWidth, fadeOutAfter);
        }


        /// <summary>
        /// Adds a line to the globe with options (returns the line gameobject).
        /// </summary>
        /// <param name="Color">line color</param>
        /// <param name="arcElevation">arc elevation relative to the sphere size (0-1 range).</param>
        /// <param name="duration">drawing speed (0 for instant drawing)</param>
        /// <param name="fadeOutAfter">duration of the line once drawn after which it fades out (set this to 0 to make the line stay forever)</param>
        public LineMarkerAnimator AddLine(float latitudeStart, float longitudeStart, float latitudeEnd, float longitudeEnd, Color color, float arcElevation, float duration, float lineWidth, float fadeOutAfter) {
            Vector3 start = Conversion.GetSpherePointFromLatLon(latitudeStart, longitudeStart);
            Vector3 end = Conversion.GetSpherePointFromLatLon(latitudeEnd, longitudeEnd);
            return AddLine(start, end, color, arcElevation, duration, lineWidth, fadeOutAfter);
        }

        /// <summary>
        /// Adds a line to the globe with options (returns the line gameobject).
        /// </summary>
        /// <param name="start">starting location on the sphere</param>
        /// <param name="end">end location on the sphere</param>
        /// <param name="Color">line color</param>
        /// <param name="arcElevation">arc elevation relative to the sphere size (0-1 range).</param>
        /// <param name="duration">drawing speed (0 for instant drawing)</param>
        /// <param name="fadeOutAfter">duration of the line once drawn after which it fades out (set this to 0 to make the line stay forever)</param>
        public LineMarkerAnimator AddLine(Vector3 spherePosStart, Vector3 spherePosEnd, Color color, float arcElevation, float duration, float lineWidth, float fadeOutAfter) {
            CheckMarkersLayer();
            GameObject newLine = new GameObject("MarkerLine", typeof(LineMarkerAnimator));
            newLine.transform.SetParent(markersLayer.transform, false);
            newLine.layer = markersLayer.layer;
            LineMarkerAnimator lma = newLine.GetComponent<LineMarkerAnimator>();
            lma.start = spherePosStart;
            lma.end = spherePosEnd;
            lma.color = color;
            lma.arcElevation = arcElevation;
            lma.duration = duration;
            lma.lineWidth = lineWidth;
            lma.lineMaterial = GetColoredMarkerLineMaterial(color);
            lma.autoFadeAfter = fadeOutAfter;
            lma.earthInvertedMode = _earthInvertedMode;
            lma.reuseMaterial = true;
            return lma;
        }

        /// <summary>
        /// Adds a line to the globe along a set of points with options (returns the line gameobject).
        /// </summary>
        /// <param name="latlon">sequence of map coordinates</param>
        /// <param name="color">line color</param>
        public LineMarkerAnimator AddLine(Vector2[] latlon, Color color, float lineWidth) {
            CheckMarkersLayer();
            GameObject newLine = new GameObject("MarkerLine", typeof(LineMarkerAnimator));
            newLine.transform.SetParent(markersLayer.transform, false);
            newLine.layer = markersLayer.layer;
            LineMarkerAnimator lma = newLine.GetComponent<LineMarkerAnimator>();
            lma.SetVertices(latlon);
            lma.color = color;
            lma.arcElevation = 0;
            lma.lineWidth = lineWidth;
            lma.lineMaterial = GetColoredMarkerLineMaterial(color);
            lma.earthInvertedMode = _earthInvertedMode;
            lma.reuseMaterial = true;
            return lma;
        }

        public LineMarkerAnimator AddLineCustom(Vector2[] latlon, Color color, float lineWidth)
        {
            CheckMarkersLayer();
            GameObject newLine = new GameObject("MarkerLine", typeof(LineMarkerAnimator));
            newLine.transform.SetParent(markersLayer.transform, false);
            newLine.layer = markersLayer.layer;
            LineMarkerAnimator lma = newLine.GetComponent<LineMarkerAnimator>();
            lma.SetVerticesCustom(latlon);
            lma.color = color;
            lma.arcElevation = 0;
            lma.lineWidth = lineWidth;
            lma.lineMaterial = GetColoredMarkerLineMaterial(color);
            lma.lineMaterial.renderQueue = 4000;
            lma.earthInvertedMode = _earthInvertedMode;
            lma.reuseMaterial = true;
            return lma;
        }

        public LineMarkerAnimator AddLineCustom(Vector2[] latlon, Color color, float lineWidth, float speed)
        {
            CheckMarkersLayer();
            GameObject newLine = new GameObject("MarkerLine", typeof(LineMarkerAnimator));
            newLine.transform.SetParent(markersLayer.transform, false);
            newLine.layer = markersLayer.layer;
            LineMarkerAnimator lma = newLine.GetComponent<LineMarkerAnimator>();
            lma.SetVerticesCustom(latlon);
            lma.color = color;
            lma.arcElevation = 0;
            lma.lineWidth = lineWidth;
            lma.lineMaterial = GetColoredMarkerLineMaterial(color);
            lma.lineMaterial.renderQueue = 4500;
            lma.earthInvertedMode = _earthInvertedMode;
            lma.reuseMaterial = true;
            lma.duration = speed;
            return lma;
        }

        /// <summary>
        /// Adds a line to the globe along a set of points with options (returns the line gameobject).
        /// </summary>
        /// <param name="latlon">sequence of map coordinates</param>
        /// <param name="color">line color</param>
        /// <param name="arcElevation">arc elevation relative to the sphere size (0-1 range).</param>
        /// <param name="duration">drawing speed (0 for instant drawing)</param>
        /// <param name="fadeOutAfter">duration of the line once drawn after which it fades out (set this to 0 to make the line stay forever)</param>
        /// <param name="reuseMaterial">If the provided material should be instantiated. Set this to true to reuse given material and avoid instantiation.</param>
        public LineMarkerAnimator AddLine(Vector2[] latlon, Color color, float lineWidth, float arcElevation, float duration, float fadeOutAfter, bool reuseMaterial) {
            CheckMarkersLayer();
            GameObject newLine = new GameObject("MarkerLine", typeof(LineMarkerAnimator));
            newLine.transform.SetParent(markersLayer.transform, false);
            newLine.layer = markersLayer.layer;
            LineMarkerAnimator lma = newLine.GetComponent<LineMarkerAnimator>();
            lma.SetVertices(latlon);
            lma.color = color;
            lma.arcElevation = arcElevation;
            lma.duration = duration;
            lma.lineWidth = lineWidth;
            lma.lineMaterial = markerMatLine;
            lma.autoFadeAfter = fadeOutAfter;
            lma.earthInvertedMode = _earthInvertedMode;
            lma.reuseMaterial = reuseMaterial;
            return lma;
        }


        /// <summary>
        /// Adds a polygon with custom color and optional fill color over the globe
        /// </summary>
        /// <param name="latlon">Array of latitude/longitude coordinates</param>
        /// <returns>The polygon gameobject</returns>
        public GameObject AddPolygon3D(Vector2[] latlon, Color borderColor, Color fillColor = default(Color)) {
            if (latlon == null || latlon.Length < 3) return null;
            RefineCoordinates(latlon);
            TempCoordinatesToVertices();

            GameObject go = null;
            // Draw polygon
            if (borderColor.a > 0) {
                Mesh mesh = new Mesh();
                mesh.SetVertices(tempVertices);
                mesh.SetIndices(tempIndices.ToArray(), MeshTopology.LineStrip, 0);
                go = new GameObject("Polygon3D");
                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.mesh = mesh;
                Material mat = Instantiate(borderColor.a < 1f ? frontiersMatThinAlpha : frontiersMatThinOpaque);
                mat.color = borderColor;
                mat.renderQueue++;
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = mat;
                go.transform.SetParent(transform, false);
            }
            // Fill poly
            if (fillColor.a > 0) {
                Material mat = GetProvinceColoredTexturedMaterial(fillColor, null, true);
                if (tempRegion == null) {
                    tempRegion = new Region(null, 0);
                }
                tempRegion.UpdatePointsAndRect(tempCoords);
                GameObject fillGo = GeneratePolygonSurface(tempRegion, mat);
                fillGo.hideFlags = 0;
                if (go != null) {
                    fillGo.transform.SetParent(go.transform, false);
                } else {
                    fillGo.transform.SetParent(transform, false);
                    go = fillGo;
                }
                fillGo.layer = fillGo.transform.parent.gameObject.layer;
            }
            return go;
        }


        /// <summary>
        /// Destroys a  marker added to the globe and returned by the function AddMarker
        /// </summary>
        public void ClearMarker(GameObject marker) {
            if (marker != null) {
                Destroy(marker);
            }
        }



        /// <summary>
        /// Deletes a line added to the globe and returned when calling AddLine
        /// </summary>
        /// <param name="line">Line.</param>
        public void ClearLineMarker(LineMarkerAnimator line) {
            if (line != null) {
                Destroy(line.gameObject);
            }
        }


        /// <summary>
        /// Deletes all custom markers and lines
        /// </summary>
        public void ClearMarkers() {
            if (markersLayer != null) {
                DestroyImmediate(markersLayer);
            }
            if (overlayMarkersLayer != null) {
                DestroyImmediate(overlayMarkersLayer);
            }
            requestMapperCamShot = true;
        }


        /// <summary>
        /// Removes all marker lines.
        /// </summary>
        public void ClearLineMarkers() {
            if (markersLayer == null)
                return;
            LineRenderer[] t = markersLayer.transform.GetComponentsInChildren<LineRenderer>();
            for (int k = 0; k < t.Length; k++) {
                Destroy(t[k].gameObject);
            }
        }

        #endregion


    }

}