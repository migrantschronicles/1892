using UnityEngine;

using System.Collections.Generic;
using WPM.Poly2Tri;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        const float REFINE_VERTICES_MIN_DISTANCE = 0.1f;

        Material markerMatOther, markerMatLine;
        List<Vector2> tempCoords = new List<Vector2>();
        List<int> tempIndices = new List<int>();
        List<Vector3> tempVertices = new List<Vector3>();
        List<Vector4> tempUVs = new List<Vector4>();

        Region tempRegion;

        void CheckMarkersLayer() {
            if (markersLayer == null) { // try to capture an existing marker layer
                Transform t = transform.Find("Markers");
                if (t != null)
                    markersLayer = t.gameObject;
            }
            if (markersLayer == null) { // create it otherwise
                markersLayer = new GameObject("Markers");
                markersLayer.layer = gameObject.layer;
                markersLayer.transform.SetParent(transform, false);
                markersLayer.transform.localPosition = Misc.Vector3zero;
            }
        }

        void PrepareOverlayLayerForRendering() {
            GameObject overlayLayer = GetOverlayLayer(true, true);
            if (overlayMarkersLayer == null) { // try to capture an existing marker layer
                Transform t = overlayLayer.transform.Find(OVERLAY_MARKER_LAYER_NAME);
                if (t != null)
                    overlayMarkersLayer = t.gameObject;
            }
            if (overlayMarkersLayer == null) { // create it otherwise
                overlayMarkersLayer = new GameObject(OVERLAY_MARKER_LAYER_NAME);
                overlayMarkersLayer.transform.SetParent(overlayLayer.transform, false);
                overlayMarkersLayer.transform.localPosition = Misc.Vector3zero;
                overlayMarkersLayer.layer = overlayLayer.layer;
            }
            requestMapperCamShot = true;
        }

        /// <summary>
        /// Adds a custom marker (gameobject) to the globe on specified location and with custom scale.
        /// </summary>
        /// <param name="marker">Your gameobject. Must be created by you.</param>
        /// <param name="sphereLocation">Location for the gameobject in sphere coordinates... [x,y,z] = (-0.5, 0.5).</param>
        /// <param name="markerScale">Scale to be applied to the gameobject. Your gameobject should have a scale of 1 and then pass to this function the desired scale.</param>
        /// <param name="isBillboard">If set to <c>true</c> the gameobject will be rotated 90º over local X-Axis.</param>
        /// <param name="surfaceOffset">Surface offset. </param>
        /// <param name="baselineAtBottom">If set to <c>true</c> the bottom of the gameobject boundary will sit over the surface or calculated height. If set to false, it's center will be used. Usually you pass true for buildings or stuff that sit on ground and false for anything that flies.</param>
        /// <param name="preserveOriginalRotation">If set to true, the object will finally rotated according to the original rotation</param>
        void mAddMarker(GameObject marker, Vector3 sphereLocation, float markerScale, bool isBillboard, float surfaceOffset, bool baselineAtBottom, bool preserveOriginalRotation = true) {
            // Try to get the height of the object

            float h = 0;
            float height = 0;
            if (baselineAtBottom) {
                if (marker.GetComponent<MeshFilter>() != null)
                    height = marker.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
                else if (marker.GetComponent<Collider>() != null)
                    height = marker.GetComponent<Collider>().bounds.size.y;
            }
            height += surfaceOffset;
            h = height * markerScale / sphereLocation.magnitude; // lift the marker so it appears on the surface of the globe

            CheckMarkersLayer();

            // Assign marker parent, position, rotation
            if (marker.transform != markersLayer.transform) {
                marker.transform.SetParent(markersLayer.transform, false);
                // Does it have a collider? Then check it also has a rigidbody to enable interaction
                if (marker.GetComponent<Collider>() != null && marker.GetComponent<Rigidbody>() == null) {
                    Rigidbody rb = marker.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                }
            }
            marker.transform.localPosition = _earthInvertedMode ? sphereLocation * (1.0f - h) : sphereLocation * (1.0f + h * 0.5f);

            // apply custom scale
            marker.transform.localScale = Misc.Vector3one * markerScale;

            Quaternion originalRotation = marker.transform.localRotation;

            if (_earthInvertedMode) {
                // flip localscale.x
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                // once the marker is on the surface, rotate it so it looks to the surface
                marker.transform.LookAt(transform.position, transform.up);
                if (!isBillboard)
                    marker.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
                // flip back localscale.x
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            } else {
                // once the marker is on the surface, rotate it so it looks to the surface
                marker.transform.LookAt(transform.position, transform.up);
                if (!isBillboard) {
                    marker.transform.Rotate(new Vector3(-90, 0, 0), Space.Self);
                }
            }

            if (preserveOriginalRotation) {
                marker.transform.Rotate(originalRotation.eulerAngles, Space.Self);
            }

        }

        /// <summary>
        /// Adds a polygon over the sphere.
        /// </summary>
        GameObject mAddMarkerCircle(MARKER_TYPE type, Vector3 sphereLocation, float kmRadius, float ringWidthStart, float ringWidthEnd, Color color) {
            GameObject marker = null;
            PrepareOverlayLayerForRendering();
            Vector2 position = Conversion.GetBillboardPosFromSpherePoint(sphereLocation);
            switch (type) {
                case MARKER_TYPE.CIRCLE: {
                        float rw = 2.0f * Mathf.PI * EARTH_RADIUS_KM;
                        float w = kmRadius / rw;
                        w *= 2.0f * overlayWidth;
                        float h = w;
                        marker = Drawing.DrawCircle("MarkerCircle", position, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 64, GetColoredMarkerOtherMaterial(color), false);
                        if (marker != null) {
                            marker.transform.SetParent(overlayMarkersLayer.transform, false);
                            marker.transform.localPosition = new Vector3(position.x, position.y, -0.01f);
                            marker.layer = overlayMarkersLayer.layer;

                            // Check seam
                            Vector2 midPos = position;
                            if (w + position.x > overlayWidth * 0.5f) {
                                midPos.x -= overlayWidth;
                            } else if (position.x - w < -overlayWidth * 0.5f) {
                                midPos.x += overlayWidth;
                            }
                            if (midPos.x != position.x) {
                                GameObject midCircle = Drawing.DrawCircle("MarkerCircleMid", midPos, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 64, GetColoredMarkerOtherMaterial(color), false);
                                midCircle.transform.SetParent(overlayMarkersLayer.transform, false);
                                midCircle.transform.localPosition = new Vector3(midPos.x, midPos.y, -0.01f);
                                midCircle.transform.SetParent(marker.transform, true);
                                midCircle.layer = overlayMarkersLayer.layer;
                            }

                        }
                    }
                    break;
                case MARKER_TYPE.CIRCLE_PROJECTED: {
                        float rw = 2.0f * Mathf.PI * EARTH_RADIUS_KM;
                        float w = kmRadius / rw;
                        w *= 2.0f * overlayWidth;
                        float h = w;
                        marker = Drawing.DrawCircle("MarkerCircle", position, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 128, GetColoredMarkerOtherMaterial(color), true);
                        if (marker != null) {
                            marker.transform.SetParent(overlayMarkersLayer.transform, false);
                            marker.transform.localPosition = new Vector3(position.x, position.y, -0.01f);
                            marker.layer = overlayMarkersLayer.layer;

                            // Check seam
                            Vector2 midPos = position;
                            if (position.x > 0) {
                                midPos.x -= overlayWidth;
                            } else {
                                midPos.x += overlayWidth;
                            }
                            GameObject midCircle = Drawing.DrawCircle("MarkerCircleMid", midPos, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 128, GetColoredMarkerOtherMaterial(color), true);
                            midCircle.transform.SetParent(overlayMarkersLayer.transform, false);
                            midCircle.transform.localPosition = new Vector3(midPos.x, midPos.y, -0.01f);
                            midCircle.transform.SetParent(marker.transform, true);
                            midCircle.layer = overlayMarkersLayer.layer;
                        }
                    }
                    break;
            }
            return marker;
        }

        /// <summary>
        /// Adds a polygon over the sphere.
        /// </summary>
        GameObject mAddMarkerQuad(MARKER_TYPE type, Vector3 sphereLocationTopLeft, Vector3 sphereLocationBottomRight, Color fillColor, Color borderColor, float borderWidth) {
            GameObject marker = null;
            PrepareOverlayLayerForRendering();
            Vector2 position1 = Conversion.GetBillboardPosFromSpherePoint(sphereLocationTopLeft);
            Vector2 position2 = Conversion.GetBillboardPosFromSpherePoint(sphereLocationBottomRight);

            // clamp to edges of billboard
            if (Mathf.Abs(position2.x - position1.x) > overlayWidth * 0.5f) {
                if (position1.x > 0) {
                    position2.x = overlayWidth * 0.5f;
                } else {
                    position1.x = 0;
                }
            }

            switch (type) {
                case MARKER_TYPE.QUAD:
                    marker = Drawing.DrawQuad("MarkerQuad", position1, position2, GetColoredMarkerOtherMaterial(fillColor));
                    marker.transform.SetParent(overlayMarkersLayer.transform, false);
                    marker.transform.localPosition += Vector3.back * -0.01f;
                    marker.layer = overlayMarkersLayer.layer;

                    if (borderWidth > 0) {
                        float dx = Mathf.Abs(position2.x - position1.x);
                        float dy = Mathf.Abs(position2.y - position1.y);
                        Vector3[] points = new Vector3[4];
                        points[0] = new Vector3(-dx * 0.5f, -dy * 0.5f) * 100f;
                        points[1] = points[0] + Misc.Vector3right * dx * 100f;
                        points[2] = points[1] + Misc.Vector3up * dy * 100f;
                        points[3] = points[2] - Misc.Vector3right * dx * 100f;
                        Material mat = GetColoredMarkerOtherMaterial(borderColor);
                        GameObject lines = new GameObject("Border");
                        LineRenderer lr = lines.AddComponent<LineRenderer>();
                        lr.alignment = LineAlignment.TransformZ;
                        lr.positionCount = 4;
                        lr.SetPositions(points);
                        lr.loop = true;
                        lr.material = mat;
                        lr.useWorldSpace = false;
                        lr.startWidth = borderWidth * 100f;
                        lr.endWidth = borderWidth * 100f;
                        lines.transform.SetParent(marker.transform, false);     // final parent
                        lines.transform.localPosition += Vector3.back * -0.02f;
                        lines.transform.localScale = new Vector3(1f / (100f * (marker.transform.localScale.x + 0.001f)), 1f / (100f * (marker.transform.localScale.y + 0.001f)), 1f);
                    }
                    break;
            }
            return marker;
        }

        void RefineCoordinates(Vector2[] latlon) {
            tempCoords.Clear();
            int count = latlon.Length + 1;
            Vector2 prev = latlon[latlon.Length - 1];
            for (int k = 0; k < count; k++) {
                Vector2 pos = k == count - 1 ? latlon[0] : latlon[k];
                float dist = Vector2.Distance(prev, pos);
                if (dist > REFINE_VERTICES_MIN_DISTANCE) {
                    int steps = (int)(dist / REFINE_VERTICES_MIN_DISTANCE) + 1;
                    for (int j=1;j<steps;j++) {
                        Vector2 nextPos = Vector2.Lerp(prev, pos, (float)j / steps);
                        tempCoords.Add(nextPos);
                    }
                }
                tempCoords.Add(pos);
                prev = pos;
            }
        }

        void TempCoordinatesToVertices() {
            tempVertices.Clear();
            tempIndices.Clear();
            int count = tempCoords.Count;

            for (int k=0;k<count;k++) {
                tempVertices.Add(Conversion.GetSpherePointFromLatLon(tempCoords[k]));
                tempIndices.Add(k);
            }
        }


        GameObject GeneratePolygonSurface(Region region, Material material) {

            // Triangulate to get the polygon vertex indices
            Poly2Tri.Polygon poly = new Poly2Tri.Polygon(region.latlon);

            const float step = 2f;
            if (steinerPoints == null) {
                steinerPoints = new List<TriangulationPoint>(1000);
            } else {
                steinerPoints.Clear();
            }
            float x0 = region.latlonRect2D.min.x + step / 2f;
            float x1 = region.latlonRect2D.max.x - step / 2f;
            float y0 = region.latlonRect2D.min.y + step / 2f;
            float y1 = region.latlonRect2D.max.y - step / 2f;
            for (float x = x0; x < x1; x += step) {
                for (float y = y0; y < y1; y += step) {
                    float xp = x + Random.Range(-0.0001f, 0.0001f);
                    float yp = y + Random.Range(-0.0001f, 0.0001f);
                    if (region.Contains(xp, yp)) {
                        steinerPoints.Add(new TriangulationPoint(xp, yp));
                    }
                }
            }
            if (steinerPoints.Count > 0) {
                poly.AddSteinerPoints(steinerPoints);
            }
            P2T.Triangulate(poly);

            int flip1, flip2;
            if (_earthInvertedMode) {
                flip1 = 2;
                flip2 = 1;
            } else {
                flip1 = 1;
                flip2 = 2;
            }
            int triCount = poly.Triangles.Count;
            Vector3[] revisedSurfPoints = new Vector3[triCount * 3];
            for (int k = 0; k < triCount; k++) {
                DelaunayTriangle dt = poly.Triangles[k];
                revisedSurfPoints[k * 3] = Conversion.GetSpherePointFromLatLon(dt.Points[0].X, dt.Points[0].Y);
                revisedSurfPoints[k * 3 + flip1] = Conversion.GetSpherePointFromLatLon(dt.Points[1].X, dt.Points[1].Y);
                revisedSurfPoints[k * 3 + flip2] = Conversion.GetSpherePointFromLatLon(dt.Points[2].X, dt.Points[2].Y);
            }

            int revIndex = revisedSurfPoints.Length - 1;

            // Generate surface mesh
            GameObject surf = Drawing.CreateSurface(SURFACE_GAMEOBJECT, revisedSurfPoints, revIndex, material);
            if (_earthInvertedMode) {
                surf.transform.localScale = Misc.Vector3one * 0.998f;
            }
            return surf;
        }

    }

}