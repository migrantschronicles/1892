// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM


using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        const float MOUNT_POINT_HIT_PRECISION = 0.0015f;
        public const string MOUNT_POINTS_LAYER = "Mount Points";

        #region Internal variables

        // resources
        Material mountPointsMat;
        GameObject mountPointSpot, mountPointsLayer;

        #endregion

        #region System initialization

        void ReloadMountPointsData() {
            string mountPointsCatalogFileName = _geodataResourcesPath + "/" + mountPointsAttributeFile;
            TextAsset ta = Resources.Load<TextAsset>(mountPointsCatalogFileName);
            string s = ta != null ? ta.text : null;
            if (!string.IsNullOrEmpty(s)) {
                SetMountPointsGeoData(s);
            } else {
                mountPoints = new List<MountPoint>();
            }
        }



        void ReadMountPointsXML(string s) {
            JSONObject json = new JSONObject(s);
            int mountPointsCount = json.list.Count;
            mountPoints = new List<MountPoint>(mountPointsCount);
            for (int k = 0; k < mountPointsCount; k++) {
                JSONObject mpJSON = json[k];
                string name = mpJSON["Name"];
                string countryName = mpJSON["Country"];
                int countryIndex = GetCountryIndex(countryName);
                int provinceIndex = mpJSON["Province"];
                float x = mpJSON["X"] / MAP_PRECISION;
                float y = mpJSON["Y"] / MAP_PRECISION;
                float z = mpJSON["Z"] / MAP_PRECISION;
                // Try to locate country and provinces in case data does not match
                Vector3 location = new Vector3(x, y, z);
                if (countryIndex < 0) {
                    countryIndex = GetCountryIndex(location);
                }
                if (provinceIndex < 0) {
                    provinceIndex = GetProvinceIndex(location);
                }
                int type = mpJSON["Type"];
                MountPoint mp = new MountPoint(name, countryIndex, provinceIndex, location, type);
                mp.attrib = mpJSON["Attrib"];
                mountPoints.Add(mp);
            }
        }

        /// <summary>
        /// Reads the mount points data from a packed string.
        /// </summary>
        void SetMountPointsGeoData(string s) {
            if (s.IndexOf('{') >= 0) {
                ReadMountPointsXML(s);
                return;
            }
            string[] mountPointsList = s.Split(SPLIT_SEP_PIPE, StringSplitOptions.RemoveEmptyEntries);
            int mountPointsCount = mountPointsList.Length;
            mountPoints = new List<MountPoint>(mountPointsCount);

            for (int k = 0; k < mountPointsCount; k++) {
                string[] mountPointInfo = mountPointsList[k].Split(SPLIT_SEP_DOLLAR);
                if (mountPointInfo.Length < 3) continue;

                string name = mountPointInfo[0];
                string country = mountPointInfo[2];
                int countryIndex = GetCountryIndex(country);
                if (countryIndex >= 0) {
                    string province = mountPointInfo[1];
                    int provinceIndex = GetProvinceIndex(countryIndex, province);
                    int type = int.Parse(mountPointInfo[3]);
                    float x = float.Parse(mountPointInfo[4], Misc.InvariantCulture) / MAP_PRECISION;
                    float y = float.Parse(mountPointInfo[5], Misc.InvariantCulture) / MAP_PRECISION;
                    float z = float.Parse(mountPointInfo[6], Misc.InvariantCulture) / MAP_PRECISION;

                    MountPoint mountPoint = new MountPoint(name, countryIndex, provinceIndex, new Vector3(x, y, z), type);

                    for (int t = 7; t < mountPointInfo.Length; t++) {
                        string tag = mountPointInfo[t];
                        string[] tagInfo = tag.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tagInfo != null && tagInfo.Length > 1) {
                            string key = tagInfo[0];
                            string value = tagInfo[1];
                            mountPoint.attrib[key] = value;
                        }
                    }

                    mountPoints.Add(mountPoint);
                }
            }

#if UNITY_EDITOR
            // Migrate to JSON
            if (!Application.isPlaying) {
                string mountPointsCatalogFileName = _geodataResourcesPath + "/" + mountPointsAttributeFile;
                TextAsset ta = Resources.Load<TextAsset>(mountPointsCatalogFileName);
                string path = UnityEditor.AssetDatabase.GetAssetPath(ta);
                string json = editor.GetMountPointsGeoData();
                File.Delete(path);
                path = Path.GetDirectoryName(path) + "/" + mountPointsAttributeFile + ".json";
                File.WriteAllText(path, json, Encoding.UTF8);
                UnityEditor.AssetDatabase.Refresh();
            }
#endif
        }


        #endregion

        #region Drawing stuff

        /// <summary>
        /// Redraws the mounts points but only in editor time. This is automatically called by Redraw(). Used internally by the Map Editor. You should not need to call this method directly.
        /// </summary>
        public void DrawMountPoints() {
            // Create mount points layer
            Transform t = transform.Find(MOUNT_POINTS_LAYER);
            if (t != null)
                DestroyImmediate(t.gameObject);
            if (Application.isPlaying || mountPoints == null)
                return;

            mountPointsLayer = new GameObject(MOUNT_POINTS_LAYER);
            mountPointsLayer.transform.SetParent(transform, false);
            mountPointsLayer.layer = gameObject.layer;
            if (_earthInvertedMode)
                mountPointsLayer.transform.localScale *= 0.99f;

            // Draw mount points marks
            int mountPointCount = mountPoints.Count;
            for (int k = 0; k < mountPointCount; k++) {
                MountPoint mp = mountPoints[k];
                GameObject mpObj = Instantiate(mountPointSpot);
                mpObj.name = k.ToString();
                mpObj.transform.position = transform.TransformPoint(mp.localPosition);
                if (_earthInvertedMode) {
                    mpObj.transform.LookAt(transform.position + mp.localPosition * 2);
                } else {
                    mpObj.transform.LookAt(transform.position);
                }
                mpObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                mpObj.transform.SetParent(mountPointsLayer.transform, true);
            }

            MountPointScaler mpScaler = mountPointsLayer.GetComponent<MountPointScaler>() ?? mountPointsLayer.AddComponent<MountPointScaler>();
            mpScaler.map = this;
            mpScaler.ScaleMountPoints();
        }


        #endregion

        #region Internal Cities API

        /// <summary>
        /// Returns any city near the point specified in local coordinates.
        /// </summary>
        public int GetMountPointNearPoint(Vector3 localPoint) {
            if (mountPoints == null)
                return -1;
            int mountPointCount = mountPoints.Count;
            for (int c = 0; c < mountPointCount; c++) {
                Vector3 mpLoc = mountPoints[c].localPosition;
                float dist = (mpLoc - localPoint).magnitude;
                if (dist < MOUNT_POINT_HIT_PRECISION) {
                    return c;
                }
            }
            return -1;
        }

        bool GetMountPointUnderMouse(int countryIndex, Vector3 localPoint, out int mountPointIndex) {
            float hitPrecision = MOUNT_POINT_HIT_PRECISION * _cityIconSize * 5.0f;
            int mountPointCount = mountPoints.Count;
            for (int c = 0; c < mountPointCount; c++) {
                MountPoint mp = mountPoints[c];
                if (mp.countryIndex == countryIndex) {
                    if ((mp.localPosition - localPoint).magnitude < hitPrecision) {
                        mountPointIndex = c;
                        return true;
                    }
                }
            }
            mountPointIndex = -1;
            return false;
        }

        /// <summary>
        /// Updates the mount points scale.
        /// </summary>
        public void ScaleMountPoints() {
            if (mountPointsLayer != null) {
                MountPointScaler scaler = mountPointsLayer.GetComponent<MountPointScaler>();
                if (scaler != null)
                    scaler.ScaleMountPoints();
            }
        }

        #endregion
    }

}