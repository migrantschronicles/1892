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

    public enum EARTH_STYLE {
        Natural = 0,
        Alternate1 = 1,
        Alternate2 = 2,
        Alternate3 = 3,
        SolidColor = 4,
        NaturalHighRes = 5,
        Scenic = 6,
        NaturalHighResScenic = 7,
        NaturalHighResScenicScatter = 8,
        NaturalHighResScenicScatterCityLights = 9,
        Custom = 10,
        NaturalHighResScenicCityLights = 11,
        ScenicCityLights = 12,
        NaturalHighRes16K = 13,
        NaturalHighRes16KScenic = 14,
        NaturalHighRes16KScenicCityLights = 15,
        NaturalHighRes16KScenicScatter = 16,
        NaturalHighRes16KScenicScatterCityLights = 17,
        StandardShader2K = 18,
        StandardShader8K = 19,
    }

    public struct EarthTexture {
        public Texture2D texture;
        public Vector4 uvRect;
        public string shaderTextureName;
    }


    public static class EarthStyleEnumExtensions {

        public static bool isScenic(this EARTH_STYLE style) {
            return style == EARTH_STYLE.NaturalHighResScenic || style == EARTH_STYLE.Scenic ||
            style == EARTH_STYLE.NaturalHighResScenicCityLights || style == EARTH_STYLE.ScenicCityLights ||
            style == EARTH_STYLE.NaturalHighRes16KScenic || style == EARTH_STYLE.NaturalHighRes16KScenicCityLights;
        }

        public static bool isScatter(this EARTH_STYLE style) {
            return style == EARTH_STYLE.NaturalHighRes16KScenicScatter || style == EARTH_STYLE.NaturalHighResScenicScatter || style == EARTH_STYLE.NaturalHighResScenicScatterCityLights
            || style == EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights;
        }

        public static bool isSurfaceShader(this EARTH_STYLE style) {
            return style == EARTH_STYLE.StandardShader2K || style == EARTH_STYLE.StandardShader8K;
        }

        public static bool is16K(this EARTH_STYLE style) {
            return style == EARTH_STYLE.NaturalHighRes16K || style == EARTH_STYLE.NaturalHighRes16KScenic || style == EARTH_STYLE.NaturalHighRes16KScenicCityLights || style == EARTH_STYLE.NaturalHighRes16KScenicScatter || style == EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights;
        }


        public static bool hasCityLights(this EARTH_STYLE style) {
            return style == EARTH_STYLE.NaturalHighRes16KScenicCityLights || style == EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights || style == EARTH_STYLE.NaturalHighResScenicCityLights || style == EARTH_STYLE.NaturalHighResScenicScatterCityLights;
        }


        public static int numTextures(this EARTH_STYLE style) {
            return style.is16K() ? 4 : 1;
        }

    }

    /* Public WPM Class */
    public partial class WorldMapGlobe : MonoBehaviour {


        [SerializeField]
        float
            _contrast = 1.02f;

        public float contrast {
            get { return _contrast; }
            set {
                if (_contrast != value) {
                    _contrast = value;
                    isDirty = true;
                    UpdateMaterialBrightness();
                }
            }
        }


        [SerializeField]
        float _citiesBrightness = 1f;

        public float citiesBrightness {
            get { return _citiesBrightness; }
            set {
                if (_citiesBrightness != value) {
                    _citiesBrightness = Mathf.Max(0, value);
                    isDirty = true;
                    UpdateMaterialBrightness();
                }
            }

        }

        [SerializeField]
        float
                _brightness = 1.05f;

        public float brightness {
            get { return _brightness; }
            set {
                if (_brightness != value) {
                    _brightness = value;
                    isDirty = true;
                    UpdateMaterialBrightness();
                }
            }
        }


        [SerializeField]
        float
            _ambientLight = 0f;

        public float ambientLight {
            get { return _ambientLight; }
            set {
                if (_ambientLight != value) {
                    _ambientLight = value;
                    isDirty = true;
                    UpdateMaterialBrightness();
                }
            }
        }


        [SerializeField]
        float
            _cloudsSpeed = -0.04f;

        public float cloudsSpeed {
            get { return _cloudsSpeed; }
            set {
                if (_cloudsSpeed != value) {
                    _cloudsSpeed = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        float
            _cloudsAlpha = 1f;

        public float cloudsAlpha {
            get { return _cloudsAlpha; }
            set {
                if (_cloudsAlpha != value) {
                    _cloudsAlpha = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        float
            _cloudsShadowStrength = 0.28f;

        public float cloudsShadowStrength {
            get { return _cloudsShadowStrength; }
            set {
                if (_cloudsShadowStrength != value) {
                    _cloudsShadowStrength = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        float
            _cloudsElevation = 0.003f;

        public float cloudsElevation {
            get { return _cloudsElevation; }
            set {
                if (_cloudsElevation != value) {
                    _cloudsElevation = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }


        [SerializeField]
        Color
            _atmosphereColor = new Color(64f / 255f, 115f / 255f, 230f / 255f);

        public Color atmosphereColor {
            get { return _atmosphereColor; }
            set {
                if (_atmosphereColor != value) {
                    _atmosphereColor = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }


        [SerializeField]
        float
            _atmosphereAlpha = 0.265f;

        public float atmosphereAlpha {
            get { return _atmosphereAlpha; }
            set {
                if (_atmosphereAlpha != value) {
                    _atmosphereAlpha = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }


        [SerializeField]
        float
            _atmosphereThickness = 0.9f;

        public float atmosphereThickness {
            get { return _atmosphereThickness; }
            set {
                if (_atmosphereThickness != value) {
                    _atmosphereThickness = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }


        [SerializeField]
        float
            _atmosphereFallOff = 1.35f;

        public float atmosphereFallOff {
            get { return _atmosphereFallOff; }
            set {
                if (_atmosphereFallOff != value) {
                    _atmosphereFallOff = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        float
            _earthBumpMapIntensity = 0.5f;

        public float earthBumpMapIntensity {
            get { return _earthBumpMapIntensity; }
            set {
                if (_earthBumpMapIntensity != value) {
                    _earthBumpMapIntensity = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        bool
            _earthBumpMapEnabled = false;

        /// <summary>
        /// Enables bump maps in Scenic and Scatter styles
        /// </summary>
        public bool earthBumpMapEnabled {
            get {
                return _earthBumpMapEnabled;
            }
            set {
                if (value != _earthBumpMapEnabled) {
                    _earthBumpMapEnabled = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        float
        _earthSpecularIntensity = 2.0f;

        public float earthSpecularIntensity {
            get { return _earthSpecularIntensity; }
            set {
                if (_earthSpecularIntensity != value) {
                    _earthSpecularIntensity = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        float
        _earthSpecularPower = 32;

        public float earthSpecularPower {
            get { return _earthSpecularPower; }
            set {
                if (_earthSpecularPower != value) {
                    _earthSpecularPower = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        bool
        _earthSpecularEnabled = false;

        /// <summary>
        /// Enables specular effect
        /// </summary>
        public bool earthSpecularEnabled {
            get {
                return _earthSpecularEnabled;
            }
            set {
                if (value != _earthSpecularEnabled) {
                    _earthSpecularEnabled = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        bool
            _cloudsShadowEnabled = false;

        /// <summary>
        /// Enables cloud shadows
        /// </summary>
        public bool cloudsShadowEnabled {
            get {
                return _cloudsShadowEnabled;
            }
            set {
                if (value != _cloudsShadowEnabled) {
                    _cloudsShadowEnabled = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        bool
            _showInlandFrontiers = false;

        /// <summary>
        /// Toggle frontiers visibility.
        /// </summary>
        public bool showInlandFrontiers {
            get {
                return _showInlandFrontiers;
            }
            set {
                if (value != _showInlandFrontiers) {
                    _showInlandFrontiers = value;
                    isDirty = true;

                    OptimizeFrontiers();
                    DrawFrontiers();
                    DrawInlandFrontiers();
                }
            }
        }

        /// <summary>
        /// Global color for inland frontiers.
        /// </summary>
        public Color inlandFrontiersColor {
            get {
                if (inlandFrontiersMatCurrent != null) {
                    return inlandFrontiersMatCurrent.color;
                } else {
                    return _inlandFrontiersColor;
                }
            }
            set {
                if (value != _inlandFrontiersColor) {
                    _inlandFrontiersColor = value;
                    isDirty = true;
                    UpdateInlandFrontiersMat();
                }
            }
        }


        [SerializeField]
        bool
            _showWorld = true;

        /// <summary>
        /// Toggle Earth visibility.
        /// </summary>
        public bool showEarth {
            get {
                return _showWorld;
            }
            set {
                if (value != _showWorld) {
                    _showWorld = value;
                    isDirty = true;
                    earthRenderer.enabled = _showWorld;
                    RestyleEarth();
                }
            }
        }

        [SerializeField]
        bool
            _showBackSide = false;

        /// <summary>
        /// Toggle Earth visibility.
        /// </summary>
        public bool showBackSide {
            get {
                return _showBackSide;
            }
            set {
                if (value != _showBackSide) {
                    _showBackSide = value;
                    isDirty = true;
                    UpdateMaterialsZWrite();
                }
            }
        }

        [SerializeField]
        [Range(-2f, 2f)]
        float
            _autoRotationSpeed = 0.02f;

        public float autoRotationSpeed {
            get { return _autoRotationSpeed; }
            set {
                if (value != _autoRotationSpeed) {
                    _autoRotationSpeed = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        [Range(-2f, 2f)]
        float
            _cameraAutoRotationSpeed;

        public float cameraAutoRotationSpeed {
            get { return _cameraAutoRotationSpeed; }
            set {
                if (value != _cameraAutoRotationSpeed) {
                    _cameraAutoRotationSpeed = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        Color
            _inlandFrontiersColor = new Color(0.1f, 0.5f, 0.1f, 1);

        [SerializeField]
        EARTH_STYLE
            _earthStyle = EARTH_STYLE.Natural;

        public EARTH_STYLE earthStyle {
            get {
                return _earthStyle;
            }
            set {
                if (value != _earthStyle) {
                    _earthStyle = value;
                    isDirty = true;
                    if (_showTiles) {
                        _earthScenicLightDirection = Vector3.back;
                    }
                    RestyleEarth();
                }
            }
        }

        [SerializeField]
        float _earthScenicAtmosphereIntensity = 1.0f;

        public float earthScenicAtmosphereIntensity {
            get { return _earthScenicAtmosphereIntensity; }
            set {
                if (value != _earthScenicAtmosphereIntensity) {
                    _earthScenicAtmosphereIntensity = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        float _earthScenicGlowIntensity = 1.0f;

        public float earthScenicGlowIntensity {
            get { return _earthScenicGlowIntensity; }
            set {
                if (value != _earthScenicGlowIntensity) {
                    _earthScenicGlowIntensity = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        Color
            _earthScenicGlowColor = new Color(49f / 255f, 79f / 255f, 146f / 255f);

        public Color earthScenicGlowColor {
            get { return _earthScenicGlowColor; }
            set {
                if (_earthScenicGlowColor != value) {
                    _earthScenicGlowColor = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }

        [SerializeField]
        bool _earthGlowScatter = true;

        /// <summary>
        /// Uses the atmospheric scattering glow. Not compatible with mobile.
        /// </summary>
        public bool earthGlowScatter {
            get { return _earthGlowScatter; }
            set {
                if (value != _earthGlowScatter) {
                    _earthGlowScatter = value;
                    isDirty = true;
                    RestyleEarth();
                }
            }
        }

        [SerializeField]
        float
            _atmosphereScatterAlpha = 1f;

        public float atmosphereScatterAlpha {
            get { return _atmosphereScatterAlpha; }
            set {
                if (_atmosphereScatterAlpha != value) {
                    _atmosphereScatterAlpha = value;
                    isDirty = true;
                    DrawAtmosphere();
                }
            }
        }


        [SerializeField]
        bool _earthInvertedMode;

        /// <summary>
        /// Enables Inverted Mode (sits you at the center of the globe). Useful for VR applications.
        /// </summary>
        public bool earthInvertedMode {
            get {
                return _earthInvertedMode;
            }
            set {
                if (value != _earthInvertedMode) {
                    _earthInvertedMode = value;
                    isDirty = true;
                    DestroyOverlay();
                    DestroySurfacesLayer();
                    Redraw();
                    Camera cam = mainCamera;
                    if (_earthInvertedMode) {
                        pivotTransform.position = transform.position;
                        pivotTransform.rotation = Misc.QuaternionZero;
                        if (!UnityEngine.XR.XRSettings.enabled) {
                            cam.fieldOfView = MAX_FIELD_OF_VIEW;
                        }
                    } else {
                        pivotTransform.position = transform.position + Vector3.back * lastRestyleEarthNormalsScaleCheck.z * 1.2f;
                        pivotTransform.LookAt(transform.position);
                        if (!UnityEngine.XR.XRSettings.enabled) {
                            cam.fieldOfView = 60;
                        }
                    }
                }
            }
        }

        [SerializeField]
        Color
            _earthColor = Color.black;

        /// <summary>
        /// Color for Earth (for SolidColor style)
        /// </summary>
        public Color earthColor {
            get {
                return _earthColor;
            }
            set {
                if (value != _earthColor) {
                    _earthColor = value;
                    isDirty = true;

                    if (_earthStyle == EARTH_STYLE.SolidColor && earthRenderer != null) {
                        earthRenderer.sharedMaterial.color = _earthColor;
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates current Earth material and returns a reference.
        /// </summary>
        public Material earthMaterial {
            get {
                Renderer r = earthRenderer;
                if (r == null)
                    return null;
                string matName = r.sharedMaterial.name;
                Material mat = r.material;
                mat.name = matName;
                earthRenderer.sharedMaterial = mat;
                return mat;
            }
        }

        /// <summary>
        /// Instantiates current Earth texture and returns a reference.
        /// </summary>
        public Texture2D earthTexture {
            get {
                Texture earthTex = earthRenderer.sharedMaterial.mainTexture;
                if (earthTex == null)
                    return null;

                Texture2D tex = Instantiate(earthTex) as Texture2D;
                tex.name = earthTex.name;
                earthRenderer.sharedMaterial.mainTexture = tex;
                return tex;
            }
        }


        /// <summary>
        /// Returns copies of current textures applied to Earth. Usually there's only one texture but 16K mode use 4 and future options could use more than 4 textures.
        /// </summary>
        /// <value>The earth textures.</value>
        public EarthTexture[] earthTextures {
            get {
                int numTextures = earthStyle.numTextures();
                if (numTextures == 1) {
                    EarthTexture[] tex = new EarthTexture[1];
                    tex[0].texture = earthTexture;
                    tex[0].uvRect = new Vector4(0, 0, 1, 1);
                    tex[0].shaderTextureName = "_MainTex";
                    return tex;
                } else if (numTextures == 4) {
                    Material mat = earthRenderer.sharedMaterial;
                    EarthTexture[] tt = new EarthTexture[4];
                    Texture tex = mat.GetTexture("_TexTL");
                    tt[0].texture = Instantiate(tex) as Texture2D;
                    tt[0].texture.name = tex.name;
                    tt[0].uvRect = new Vector4(0, 0.5f, 0.5f, 0.5f);
                    tt[0].shaderTextureName = "_TexTL";
                    tex = mat.GetTexture("_TexTR");
                    tt[1].texture = Instantiate(tex) as Texture2D;
                    tt[1].texture.name = tex.name;
                    tt[1].uvRect = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
                    tt[1].shaderTextureName = "_TexTR";
                    tex = mat.GetTexture("_TexBL");
                    tt[2].texture = Instantiate(tex) as Texture2D;
                    tt[2].texture.name = tex.name;
                    tt[2].uvRect = new Vector4(0f, 0f, 0.5f, 0.5f);
                    tt[2].shaderTextureName = "_TexBL";
                    tex = mat.GetTexture("_TexBR");
                    tt[3].texture = Instantiate(tex) as Texture2D;
                    tt[3].texture.name = tex.name;
                    tt[3].uvRect = new Vector4(0.5f, 0f, 0.5f, 0.5f);
                    tt[3].shaderTextureName = "_TexBR";
                    return tt;
                } else {
                    // undefined
                    EarthTexture[] tex = new EarthTexture[0];
                    return tex;
                }
            }
        }



        [SerializeField]
        bool _earthHighDensityMesh = true;

        /// <summary>
        /// Specifies the mesh asset to load and render as Earth mesh
        /// </summary>
        public bool earthHighDensityMesh {
            get { return _earthHighDensityMesh; }
            set {
                if (value != _earthHighDensityMesh) {
                    _earthHighDensityMesh = value;
                    isDirty = true;
                    RestyleEarth();
                }
            }
        }


        #region Public API area

        /// <summary>
        /// Makes the globe's north points upwards.
        /// </summary>
        public void StraightenGlobe() {
            StraightenGlobe(0, true);
        }

        /// <summary>
        /// Makes the globe's north points upwards smoothly
        /// </summary>
        public void StraightenGlobe(float duration) {
            StraightenGlobe(duration, false);
        }

        /// <summary>
        /// Makes the globe's north points upwards smoothly and optionally retains current location on the center of globe
        /// </summary>
        public void StraightenGlobe(float duration, bool keepLocationOnCenter) {
            Camera cam = mainCamera;
            if (_earthInvertedMode) {
                if (keepLocationOnCenter) {
                    Quaternion oldRotation = transform.localRotation;
                    Vector3 v2 = pivotTransform.forward;
                    Vector3 v3 = Vector3.ProjectOnPlane(transform.up, v2);
                    float angle2 = SignedAngleBetween(cam.transform.up, v3, v2);
                    if (duration == SMOOTH_STRAIGHTEN_ON_POLES) {
                        if (Mathf.Abs(Vector3.Dot(transform.up, v2)) < 0.96f) { // avoid crazy rotation on poles
                            angle2 = Mathf.Clamp(angle2, -2, 2);
                        } else {
                            angle2 = 0;
                        }
                    }
                    transform.Rotate(v2, -angle2, Space.World);
                    flyToEndQuaternion = transform.localRotation;
                    transform.localRotation = oldRotation;
                } else {
                    flyToEndQuaternion = Misc.QuaternionZero;
                }
            } else {
                Quaternion oldRotation = transform.localRotation;
                if (keepLocationOnCenter) {
                    Vector3 v2 = -pivotTransform.forward;
                    Vector3 v3 = Vector3.ProjectOnPlane(transform.up, v2);
                    float angle2 = SignedAngleBetween(pivotTransform.up, v3, v2);
                    if (duration == SMOOTH_STRAIGHTEN_ON_POLES) {
                        if (Mathf.Abs(Vector3.Dot(transform.up, v2.normalized)) < 0.96f) {  // avoid crazy rotation on poles
                            angle2 = Mathf.Clamp(angle2, -2, 2);
                        } else {
                            angle2 = 0;
                        }
                    }
                    transform.Rotate(v2, -angle2, Space.World);
                    Vector3 currentDestination = transform.InverseTransformVector(pivotTransform.position - transform.position);
                    flyToEndDestination = currentDestination;
                } else {
                    Vector3 v1 = pivotTransform.position - transform.position;
                    float angleY = SignedAngleBetween(v1, transform.right, transform.up) + 90.0f;
                    transform.localRotation = pivotTransform.localRotation;
                    transform.Rotate(Misc.Vector3up * angleY, Space.Self);
                }
                flyToEndQuaternion = transform.localRotation;
                transform.localRotation = oldRotation;
            }
            if (!Application.isPlaying || duration == SMOOTH_STRAIGHTEN_ON_POLES) {
                duration = 0;
            }
            flyToDuration = duration;
            flyToStartQuaternion = transform.localRotation;
            flyToStartTime = Time.time;
            flyToActive = true;
            flyToCameraStartPosition = flyToCameraEndPosition = _cursorLocation;
            flyToCameraStartUpVector = pivotTransform.up;
            flyToMode = NAVIGATION_MODE.EARTH_ROTATES;
            if (flyToDuration == 0) {
                NavigateToDestination();
            }
        }

        /// <summary>
        /// Set Earth rotation to default declination.
        /// </summary>
        public void TiltGlobe() {
            TiltGlobe(new Vector3(0, 0, 23.4f), 1.0f);
        }

        /// <summary>
        /// Set Earth declination and moves smoothly.
        /// </summary>
        public void TiltGlobe(Vector3 angles, float duration) {
            if (_earthInvertedMode) {
                flyToEndQuaternion = Quaternion.Euler(angles);
            } else {
                Vector3 v1 = mainCamera.transform.position - transform.position;
                float angleY = SignedAngleBetween(v1, transform.right, transform.up) + 90.0f;
                flyToEndQuaternion = Quaternion.Euler(angles) * Quaternion.Euler(Misc.Vector3up * angleY);
            }
            if (!Application.isPlaying)
                duration = 0;
            flyToDuration = duration;
            flyToStartQuaternion = transform.localRotation;
            flyToStartTime = Time.time;
            flyToActive = true;
            flyToMode = NAVIGATION_MODE.EARTH_ROTATES;
            if (flyToDuration == 0) {
                NavigateToDestination();
            }
        }

        /// <summary>
        /// Iterates for the countries list and colorizes those belonging to specified continent name.
        /// </summary>
        public void ToggleContinentSurface(string continentName, bool visible, Color color) {
            for (int colorizeIndex = 0; colorizeIndex < countries.Length; colorizeIndex++) {
                if (countries[colorizeIndex].continent.Equals(continentName)) {
                    ToggleCountrySurface(countries[colorizeIndex].name, visible, color);
                }

            }
        }


        /// <summary>
        /// Uncolorize/hide specified countries beloning to a continent.
        /// </summary>
        public void HideContinentSurface(string continentName) {
            for (int colorizeIndex = 0; colorizeIndex < countries.Length; colorizeIndex++) {
                if (countries[colorizeIndex].continent.Equals(continentName)) {
                    HideCountrySurface(colorizeIndex);
                }
            }
        }


        /// <summary>
        /// Discards current material and reloads it based on earthStyle
        /// </summary>
        public void ReloadEarthTexture() {
            if (earthRenderer != null)
                earthRenderer.sharedMaterial = null;
            RestyleEarth();
        }


        #endregion


    }

}