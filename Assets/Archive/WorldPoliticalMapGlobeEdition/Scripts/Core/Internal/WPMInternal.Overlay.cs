// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using UnityEngine;
using System.Collections;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        #region Overlay

        static int globeCount = 0;
        [SerializeField, HideInInspector] int _globeIndex;

        // don't change these values or
        // overlay wont' work
        public const int overlayWidth = 200;
        public const int overlayHeight = 100;
        RenderTexture overlayRT;
        GameObject overlayLayer, sphereOverlayLayer;
        Camera mapperCam;


        void CheckOverlay() {
            if (!Application.isPlaying) {
                // when saving the scene from Editor, the material of the sphere label layer is cleared - here's a fix to recreate it
                if (_showCountryNames && sphereOverlayLayer != null && sphereOverlayLayer.GetComponent<Renderer>() == null) {
                    CreateOverlay();
                }
            }
        }


        GameObject CreateOverlay() {

            if (!gameObject.activeInHierarchy)
                return null;

            // Prepare layer
            Transform t = transform.Find(WPM_OVERLAY_NAME);
            if (t == null) {
                overlayLayer = new GameObject(WPM_OVERLAY_NAME);
                overlayLayer.transform.SetParent(transform, false);
            } else {
                overlayLayer = t.gameObject;
            }
            float y = 1000 * _globeIndex;
            if (_globeIndex == 0) {
                globeCount++;
                _globeIndex = globeCount % 10;
            }
            overlayLayer.transform.position = new Vector3(5000, y, 0);
            overlayLayer.transform.localScale = new Vector3(1.0f / transform.localScale.x, 1.0f / transform.localScale.y, 1.0f / transform.localScale.z);
            overlayLayer.layer = _overlayLayerIndex;
            t = transform.Find(SPHERE_OVERLAY_LAYER_NAME);
            if (t != null) {
                Renderer r = t.gameObject.GetComponent<Renderer>();
                if (r == null || r.sharedMaterial == null) {
                    DestroyImmediate(t.gameObject);
                    t = null;
                }
            }
            if (t == null) {
                sphereOverlayLayer = Instantiate(Resources.Load<GameObject>("Prefabs/SphereOverlayLayer"));
                sphereOverlayLayer.hideFlags = HideFlags.DontSave;
                sphereOverlayLayer.name = SPHERE_OVERLAY_LAYER_NAME;
                sphereOverlayLayer.transform.SetParent(transform, false);
                sphereOverlayLayer.layer = gameObject.layer;
                sphereOverlayLayer.transform.localPosition = Misc.Vector3zero;
            } else {
                sphereOverlayLayer = t.gameObject;
            }
            sphereOverlayLayer.SetActive(true);

            // Material
            if (sphereOverlayMatDefault == null) {
                sphereOverlayMatDefault = Instantiate(Resources.Load<Material>("Materials/SphereOverlay")) as Material;
                sphereOverlayMatDefault.hideFlags = HideFlags.DontSave;
            }
            // Sphere labels layer
            Material sphereOverlayMaterial = sphereOverlayMatDefault;
            Renderer sphereOverlayRenderer = sphereOverlayLayer.GetComponent<Renderer>();
            sphereOverlayRenderer.sharedMaterial = sphereOverlayMaterial;
            sphereOverlayRenderer.enabled = _labelsQuality != LABELS_QUALITY.NotUsed;

            // Billboard
            GameObject billboard;
            t = overlayLayer.transform.Find("Billboard");
            if (t == null) {
                billboard = Instantiate(Resources.Load<GameObject>("Prefabs/Billboard"));
                billboard.name = "Billboard";
                billboard.transform.SetParent(overlayLayer.transform, false);
                billboard.transform.localPosition = Misc.Vector3zero;
                billboard.transform.localScale = new Vector3(overlayWidth, overlayHeight, 1);
                billboard.layer = overlayLayer.layer;
            }

            // Render texture
            int imageWidth, imageHeight;
            switch (_labelsQuality) {
                case LABELS_QUALITY.Medium:
                    imageWidth = 4096;
                    imageHeight = 2048;
                    break;
                case LABELS_QUALITY.High:
                    imageWidth = 8192;
                    imageHeight = 4096;
                    break;
                case LABELS_QUALITY.NotUsed:
                    imageWidth = imageHeight = 4;
                    break;
                default:
                    imageWidth = 2048;
                    imageHeight = 1024;
                    break;
            }
            if (overlayRT != null && (overlayRT.width != imageWidth || overlayRT.height != imageHeight)) {
                overlayRT.Release();
                DestroyImmediate(overlayRT);
                overlayRT = null;
            }

            Transform camTransform = overlayLayer.transform.Find(MAPPER_CAM);

            if (overlayRT == null) {
                overlayRT = new RenderTexture(imageWidth, imageHeight, 24, RenderTextureFormat.ARGB32);
                overlayRT.hideFlags = HideFlags.DontSave;
                overlayRT.filterMode = FilterMode.Trilinear;
                if (camTransform != null) {
                    camTransform.GetComponent<Camera>().targetTexture = overlayRT;
                }
            }

            // Camera
            if (camTransform == null) {
                GameObject camObj = Instantiate(Resources.Load<GameObject>("Prefabs/MapperCam"));
                camObj.name = MAPPER_CAM;
                camObj.transform.SetParent(overlayLayer.transform, false);
                camTransform = camObj.transform;
            }
            camTransform.gameObject.layer = _overlayLayerIndex;

            if (mapperCam == null) {
                mapperCam = camTransform.GetComponent<Camera>();
                mapperCam.transform.localPosition = Vector3.back * 86.6f; // (10000.0f - 9999.13331f);
                mapperCam.clearFlags = CameraClearFlags.Color;
                mapperCam.backgroundColor = Misc.ColorTransparent;
                mapperCam.aspect = 2;
                mapperCam.targetTexture = overlayRT;
                mapperCam.cullingMask = 1 << camTransform.gameObject.layer;
                mapperCam.fieldOfView = 60f;
                mapperCam.enabled = false;
                mapperCam.Render();
            }

            // Assigns render texture to current material and recreates the camera
            sphereOverlayMaterial.mainTexture = overlayRT;

            // Reverse normals if inverted mode is enabled
            Drawing.ReverseSphereNormals(sphereOverlayLayer, _earthInvertedMode, _earthHighDensityMesh);
            AdjustSphereOverlayLayerScale();
            return overlayLayer;
        }

        void AdjustSphereOverlayLayerScale() {
            if (_earthInvertedMode) {
                sphereOverlayLayer.transform.localScale = Misc.Vector3one * (0.998f - _labelsElevation * 0.5f);
            } else {
                sphereOverlayLayer.transform.localScale = Misc.Vector3one * (1.01f + _labelsElevation * 0.05f);
            }
        }

        void DestroyOverlay() {

            if (sphereOverlayLayer != null) {
                sphereOverlayLayer.SetActive(false);
            }

            if (overlayLayer != null) {
                DestroyImmediate(overlayLayer);
                overlayLayer = null;
            }

            if (overlayRT != null) {
                overlayRT.Release();
                DestroyImmediate(overlayRT);
                overlayRT = null;
            }
        }

        #endregion

    }

}