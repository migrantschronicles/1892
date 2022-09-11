// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using WPM.Poly2Tri;

namespace WPM {
    public partial class WorldMapGlobe : MonoBehaviour {

        #region Universe setup and initialization

        const string MOON_NAME = "Moon";

        void UpdateMoon() {
            Transform t = transform.Find(MOON_NAME);
            if (!_showMoon) {
                if (t != null)
                    t.gameObject.SetActive(false);
            } else {
                if (t == null) {
                    GameObject mgo = Instantiate(Resources.Load<GameObject>("Prefabs/Moon"));
                    mgo.name = MOON_NAME;
                    t = mgo.transform;
                    t.SetParent(transform, false);
                } else {
                    t.gameObject.SetActive(true);
                }
                // Move and scale moon according to Earth dimensions
                if (_moonAutoScale) {
                    t.transform.localScale = transform.lossyScale * 0.2726f;
                    t.transform.localPosition = Misc.Vector3left * 384400f / 12742f; // distance of moon at scale (384.400 km / 12.742 km Earth diameter)
                }
            }

        }

        void UpdateSkybox() {
            switch (_skyboxStyle) {
                case SKYBOX_STYLE.Basic:
                    RenderSettings.skybox = Resources.Load<Material>("Skybox/Starfield Basic/Starfield");
                    break;
                case SKYBOX_STYLE.MilkyWay:
                    RenderSettings.skybox = Resources.Load<Material>("Skybox/Starfield Tycho/Starfield Tycho");
                    break;
                case SKYBOX_STYLE.DualSkybox:
                    Material mat = Resources.Load<Material>("Skybox/DualSkybox/DualSkybox");
                    if (mat != null) {
                        mat = Instantiate(mat);
                        mat.SetTexture("_Environment", _skyboxEnvironmentTextureHDR);
                        RenderSettings.skybox = mat;
                    }

                    break;
            }
        }

        #endregion
    }

}