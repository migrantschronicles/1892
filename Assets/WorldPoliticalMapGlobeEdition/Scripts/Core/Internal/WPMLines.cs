// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        #region World Lines

        const string CURSOR_LAYER = "Cursor";
        const string LATITUDE_LINES_LAYER = "LatitudeLines";
        const string LONGITUDE_LINES_LAYER = "LongitudeLines";

        GameObject cursorLayer, cursorVertical, cursorHorizontal;
        GameObject latitudeLayer, longitudeLayer;
        Material cursorMat, gridMatOverlay, gridMatMasked;


        void CheckCursorVisibility() {
            if (cursorLayer != null && _showCursor) {
                if ((mouseIsOverUIElement || !mouseIsOver) && cursorLayer.activeSelf && !cursorAlwaysVisible) {   // not over globe?
                    cursorLayer.SetActive(false);
                } else if (!mouseIsOverUIElement && mouseIsOver && !cursorLayer.activeSelf) {   // finally, should be visible?
                    cursorLayer.SetActive(true);
                }
            }
        }

        void DrawCursor() {
            // Cursor root
            Transform t = transform.Find(CURSOR_LAYER);
            if (t != null) {
                DestroyImmediate(t.gameObject);
            }
            cursorLayer = new GameObject(CURSOR_LAYER);
            cursorLayer.transform.SetParent(transform, false);
            cursorLayer.layer = gameObject.layer;
            cursorLayer.transform.localPosition = Misc.Vector3zero;
            cursorLayer.transform.localRotation = Misc.QuaternionZero;
            cursorLayer.SetActive(_showCursor);
            switch (_cursorStyle) {
                case CURSOR_STYLE.LatitudeLongitudeCursor: case CURSOR_STYLE.Legacy:
                    DrawCursorLatitudeLongitude();
                    break;
            }
        }


        void DrawCursorLatitudeLongitude() {
            // Compute cursor dash lines
            float r = _earthInvertedMode ? 0.498f : 0.5f;
            Vector3 north = new Vector3(0, r, 0);
            Vector3 south = new Vector3(0, -r, 0);
            Vector3 west = new Vector3(-r, 0, 0);
            Vector3 east = new Vector3(r, 0, 0);
            Vector3 equatorFront = new Vector3(0, 0, r);
            Vector3 equatorPost = new Vector3(0, 0, -r);

            Vector3[] points = new Vector3[400];
            int[] indices = new int[400];

            // Generate circumference V
            for (int k = 0; k < 400; k++) {
                indices[k] = k;
            }
            for (int k = 0; k < 100; k++) {
                points[k] = Vector3.Lerp(north, equatorFront, k / 100.0f).normalized * r;
            }
            for (int k = 0; k < 100; k++) {
                points[100 + k] = Vector3.Lerp(equatorFront, south, k / 100.0f).normalized * r;
            }
            for (int k = 0; k < 100; k++) {
                points[200 + k] = Vector3.Lerp(south, equatorPost, k / 100.0f).normalized * r;
            }
            for (int k = 0; k < 100; k++) {
                points[300 + k] = Vector3.Lerp(equatorPost, north, k / 100.0f).normalized * r;
            }
            cursorVertical = GetCursorHalf("Vertical", points, indices);

            // Generate circumference H
            for (int k = 0; k < 100; k++) {
                points[k] = Vector3.Lerp(equatorPost, west, k / 100.0f).normalized * r;
            }
            for (int k = 0; k < 100; k++) {
                points[100 + k] = Vector3.Lerp(west, equatorFront, k / 100.0f).normalized * r;
            }
            for (int k = 0; k < 100; k++) {
                points[200 + k] = Vector3.Lerp(equatorFront, east, k / 100.0f).normalized * r;
            }
            for (int k = 0; k < 100; k++) {
                points[300 + k] = Vector3.Lerp(east, equatorPost, k / 100.0f).normalized * r;
            }
            cursorHorizontal = GetCursorHalf("Horizontal", points, indices);
        }

        GameObject GetCursorHalf(string name, Vector3[] points, int[] indices) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(cursorLayer.transform, false);
            go.layer = gameObject.layer;
            go.transform.localPosition = Misc.Vector3zero;
            go.transform.localRotation = Misc.QuaternionZero;

            Mesh mesh = new Mesh();
            mesh.vertices = points;
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0, true);
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial = cursorMat;

            return go;
        }

        void DrawGrid() {
            DrawLatitudeLines();
            DrawLongitudeLines();
        }

        void DrawLatitudeLines() {
            // Generate latitude lines
            List<Vector3> points = new List<Vector3>();
            List<int> indices = new List<int>();
            float r = _earthInvertedMode ? 0.498f : 0.501f;
            int idx = 0;
            float m = _frontiersDetail == FRONTIERS_DETAIL.High ? 4.0f : 5.0f;

            for (float a = 0; a < 90; a += _latitudeStepping) {
                for (int h = 1; h >= -1; h--) {
                    if (h == 0)
                        continue;

                    float angle = a * Mathf.Deg2Rad;
                    float y = h * Mathf.Sin(angle) * r;
                    float r2 = Mathf.Cos(angle) * r;

                    int step = Mathf.Min(1 + Mathf.FloorToInt(m * r / r2), 24);
                    if ((100 / step) % 2 != 0)
                        step++;

                    for (int k = 0; k < 360 + step; k += step) {
                        float ax = k * Mathf.Deg2Rad;
                        float x = Mathf.Cos(ax) * r2;
                        float z = Mathf.Sin(ax) * r2;
                        points.Add(new Vector3(x, y, z));
                        if (k > 0) {
                            indices.Add(idx);
                            indices.Add(++idx);
                        }
                    }
                    idx++;
                    if (a == 0)
                        break;
                }
            }

            Transform t = transform.Find(LATITUDE_LINES_LAYER);
            if (t != null)
                DestroyImmediate(t.gameObject);
            latitudeLayer = new GameObject(LATITUDE_LINES_LAYER);
            latitudeLayer.transform.SetParent(transform, false);
            latitudeLayer.layer = gameObject.layer;
            latitudeLayer.transform.localPosition = Misc.Vector3zero;
            latitudeLayer.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler(Misc.Vector3zero);
            latitudeLayer.SetActive(_showLatitudeLines);

            Mesh mesh = new Mesh();
            mesh.vertices = points.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = latitudeLayer.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = latitudeLayer.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial = _gridMode == GRID_MODE.OVERLAY ? gridMatOverlay : gridMatMasked;

        }

        void DrawLongitudeLines() {
            // Generate longitude lines
            List<Vector3> points = new List<Vector3>();
            List<int> indices = new List<int>();
            float r = _earthInvertedMode ? 0.498f : 0.501f;
            int idx = 0;
            int step = _frontiersDetail == FRONTIERS_DETAIL.High ? 4 : 5;

            for (float a = 0; a < 180; a += 180 / _longitudeStepping) {
                float angle = a * Mathf.Deg2Rad;

                for (int k = 0; k < 360 + step; k += step) {
                    float ax = k * Mathf.Deg2Rad;
                    float x = Mathf.Cos(ax) * r * Mathf.Sin(angle); //Mathf.Cos (ax) * Mathf.Sin (angle) * r;
                    float y = Mathf.Sin(ax) * r;
                    float z = Mathf.Cos(ax) * r * Mathf.Cos(angle);
                    points.Add(new Vector3(x, y, z));
                    if (k > 0) {
                        indices.Add(idx);
                        indices.Add(++idx);
                    }
                }
                idx++;
            }

            Transform t = transform.Find(LONGITUDE_LINES_LAYER);
            if (t != null)
                DestroyImmediate(t.gameObject);
            longitudeLayer = new GameObject(LONGITUDE_LINES_LAYER);
            longitudeLayer.transform.SetParent(transform, false);
            longitudeLayer.layer = gameObject.layer;
            longitudeLayer.transform.localPosition = Misc.Vector3zero;
            longitudeLayer.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler(Misc.Vector3zero);
            longitudeLayer.SetActive(_showLongitudeLines);

            Mesh mesh = new Mesh();
            mesh.vertices = points.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = longitudeLayer.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = longitudeLayer.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial = _gridMode == GRID_MODE.OVERLAY ? gridMatOverlay : gridMatMasked;
        }

        #endregion

    }

}