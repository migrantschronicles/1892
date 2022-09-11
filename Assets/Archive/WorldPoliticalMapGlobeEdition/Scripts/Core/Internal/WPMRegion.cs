// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
#endif


namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        const string OUTLINE_GAMEOBJECT_NAME = "Outline";

        // cache
        Dictionary<Color, Material> outlineMatCache;

        /// <summary>
        /// Gets a list of regions that overlap with a given region
        /// </summary>
        public List<Region> GetRegionsOverlap(Region region, bool includeProvinces = false) {
            List<Region> rr = new List<Region>();
            for (int k = 0; k < _countries.Length; k++) {
                Country country = _countries[k];
                if (country.regions == null)
                    continue;
                int rCount = country.regions.Count;
                for (int r = 0; r < rCount; r++) {
                    Region otherRegion = country.regions[r];
                    if (region.Intersects(otherRegion)) {
                        rr.Add(otherRegion);
                    }
                }
            }

            if (includeProvinces) {
                int provinceCount = provinces.Length; // triggers lazy load
                for (int k = 0; k < provinceCount; k++) {
                    Province province = _provinces[k];
                    if (province.regions == null)
                        continue;
                    int rCount = province.regions.Count;
                    for (int r = 0; r < rCount; r++) {
                        Region otherRegion = province.regions[r];
                        if (region.Intersects(otherRegion)) {
                            rr.Add(otherRegion);
                        }
                    }
                }
            }
            return rr;
        }



        /// <summary>
        /// Adds extra points if distance between 2 consecutive points exceed some threshold 
        /// </summary>
        /// <returns><c>true</c>, if region was smoothed, <c>false</c> otherwise.</returns>
        /// <param name="region">Region.</param>
        public bool RegionSmooth(Region region, float smoothDistance) {
            int lastPoint = region.latlon.Length - 1;
            bool changes = false;
            List<Vector2> newPoints = new List<Vector2>(lastPoint + 1);
            for (int k = 0; k <= lastPoint; k++) {
                Vector2 p0 = region.latlon[k];
                Vector2 p1;
                if (k == lastPoint) {
                    p1 = region.latlon[0];
                } else {
                    p1 = region.latlon[k + 1];
                }
                newPoints.Add(p0);
                float dist = (p0 - p1).magnitude;
                if (dist > smoothDistance) {
                    changes = true;
                    int steps = Mathf.FloorToInt(dist / smoothDistance);
                    float inc = dist / (steps + 1);
                    float acum = inc;
                    for (int j = 0; j < steps; j++) {
                        newPoints.Add(Vector2.Lerp(p0, p1, acum / dist));
                        acum += inc;
                    }
                }
                newPoints.Add(p1);
            }
            if (changes)
                region.latlon = newPoints.ToArray();
            return changes;
        }


        /// <summary>
        /// Modifies the borders of a region so it matches the hexagonal grid.
        /// </summary>
        /// <param name="region">Region.</param>
        public void RegionClampToCells(Region region, List<int> cellIndices) {

            // Get minimal distance between 2 cells
            float threshold = (cells[0].latlonCenter - cells[0].neighbours[0].latlonCenter).magnitude * 0.5f;
            float thresholdSqr = threshold * threshold;

            // Smooth borders
            RegionSmooth(region, threshold);

            // Clamp points to nearest cell vertex
            int cc = cellIndices.Count;
            Vector2[] regionLatlon = region.latlon;
            int pointCount = regionLatlon.Length;
            for (int k = 0; k < pointCount; k++) {
                float minDist = float.MaxValue;
                Vector2 nearest = Misc.Vector2zero;
                for (int c = 0; c < cc; c++) {
                    Cell cell = cells[cellIndices[c]];
                    Vector2[] cellLatlon = cell.latlon;
                    for (int v = 0; v < cellLatlon.Length; v++) {
                        float dist = (cellLatlon[v].x - regionLatlon[k].x) * (cellLatlon[v].x - regionLatlon[k].x) + (cellLatlon[v].y - regionLatlon[k].y) * (cellLatlon[v].y - regionLatlon[k].y);
                        if (dist < minDist) {
                            minDist = dist;
                            nearest = cellLatlon[v];
                            if (minDist < thresholdSqr) {
                                c = cc;
                                break;
                            }
                        }
                    }
                }
                regionLatlon[k] = nearest;
            }
            region.UpdateRect();
            RegionSanitize(region); // remove duplicates
        }

        /// <summary>
        /// Draws the country outline.
        /// </summary>
        GameObject DrawRegionOutline(Region region, GameObject parent, bool temporary, Color outlineColor = new Color()) {

            if (region == null || parent == null) return null;

            region.customOutline = !temporary;
            Transform t = parent.transform.Find(OUTLINE_GAMEOBJECT_NAME);
            if (t != null) {
                DestroyImmediate(t.gameObject);
            }

            int[] indices = new int[region.spherePoints.Length + 1];
            Vector3[] outlinePoints = new Vector3[region.spherePoints.Length + 1];
            for (int k = 0; k < region.spherePoints.Length; k++) {
                indices[k] = k;
                outlinePoints[k] = region.spherePoints[k];
            }
            indices[region.spherePoints.Length] = indices[0];
            outlinePoints[region.spherePoints.Length] = region.spherePoints[0];

            GameObject boldFrontiers = new GameObject(OUTLINE_GAMEOBJECT_NAME);
            boldFrontiers.hideFlags = HideFlags.DontSave;

            boldFrontiers.transform.SetParent(parent.transform, false);
            boldFrontiers.transform.localPosition = Misc.Vector3zero;
            boldFrontiers.transform.localRotation = Misc.QuaternionZero;

            Mesh mesh = new Mesh();
            if (_frontiersThicknessMode == FRONTIERS_THICKNESS.Custom) {
                ComputeExplodedVertices(mesh, outlinePoints);
            } else {
                mesh.vertices = outlinePoints;
                mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            }
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = boldFrontiers.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = boldFrontiers.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            UpdateOutlineMatProperties();

            if (temporary) {
                mr.sharedMaterial = outlineMatCurrent;
            } else {
                Material mat = GetOutlineMaterialFromCache(outlineColor);
                mr.sharedMaterial = mat;
            }
            return boldFrontiers;
        }


        Material GetOutlineMaterialFromCache(Color color) {
            if (outlineMatCache == null) {
                outlineMatCache = new Dictionary<Color, Material>();
            }
            Material outlineMat;
            if (!outlineMatCache.TryGetValue(color, out outlineMat)) {
                outlineMat = Instantiate(outlineMatCurrent);
                outlineMat.color = color;
                outlineMatCache[color] = outlineMat;
            }
            return outlineMat;
        }



        void ComputeExplodedVertices(Mesh mesh, Vector3[] vertices) {
            tempVertices.Clear();
            tempUVs.Clear();
            tempIndices.Clear();
            Vector4 uv;
            for (int k = 0; k < vertices.Length - 1; k ++) {
                // Triangle points
                tempVertices.Add(vertices[k]);
                tempVertices.Add(vertices[k]);
                tempVertices.Add(vertices[k + 1]);
                tempVertices.Add(vertices[k + 1]);
                uv = vertices[k + 1]; uv.w = -1; tempUVs.Add(uv);
                uv = vertices[k + 1]; uv.w = 1; tempUVs.Add(uv);
                uv = vertices[k]; uv.w = 1; tempUVs.Add(uv);
                uv = vertices[k]; uv.w = -1; tempUVs.Add(uv);
                // First triangle
                tempIndices.Add(k * 4);
                tempIndices.Add(k * 4 + 1);
                tempIndices.Add(k * 4 + 2);
                // Second triangle
                tempIndices.Add(k * 4 + 1);
                tempIndices.Add(k * 4 + 3);
                tempIndices.Add(k * 4 + 2);
            }
            mesh.SetVertices(tempVertices);
            mesh.SetUVs(0, tempUVs);
            mesh.SetTriangles(tempIndices, 0);
        }


        void ToggleRegionOutline(Region region, bool visible, Color color = default(Color)) {
            if (region == null) return;
            if (region.surfaceGameObject == null) {
                GameObject surf = new GameObject(SURFACE_GAMEOBJECT);
                surf.layer = surfacesLayer.layer;
                surf.transform.SetParent(surfacesLayer.transform, false);
                surf.transform.localPosition = Misc.Vector3zero;
                surf.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                region.surfaceGameObject = surf;
            }
            if (!visible) {
                Transform t = region.surfaceGameObject.transform.Find(OUTLINE_GAMEOBJECT_NAME);
                if (t != null) {
                    DestroyImmediate(t.gameObject);
                }
                region.customOutline = false;
                return;
            }
            DrawRegionOutline(region, region.surfaceGameObject, false, color);
        }

    }

}