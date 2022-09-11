using UnityEngine;
using UnityEditor;

namespace WPM {
    [CustomEditor(typeof(WorldMapGlobe))]
    public class WorldMapGlobeInspector : Editor {
        const int CALCULATOR = 0;
        const int TICKERS = 1;
        const int DECORATOR = 2;
        const int EDITOR = 3;
        const string WPM_BUILD_HINT = "WPMBuildHint";
        WorldMapGlobe _map;
        Texture2D _headerTexture;
        string[] earthStyleOptions, frontiersDetailOptions, labelsRenderMethods, gridModeOptions, navigationModeOptions, rotationAxisOptions;
        GUIContent[] labelsQualityOptions, zoomModeOptions;
        int[] earthStyleValues, gridModeValues, navigationModeValues, zoomModeValues, rotationAxisValues;
        GUIStyle blackBack, sectionHeaderNormalStyle;
        readonly bool[] extracomp = new bool[4];
        bool expandUniverseSection, expandEarthSection, expandGridSection, expandTilesSection, expandCitiesSection, expandFoWSection;
        bool expandCountriesSection, expandProvincesSection, expandInteractionSection, expandDevicesSection, expandCameraControlSection;
        bool tileSizeComputed;
        SerializedProperty isDirty;
        float zoomLevel;

        void OnEnable() {
            _map = (WorldMapGlobe)target;
            _headerTexture = Resources.Load<Texture2D>("EditorHeader");
            blackBack = new GUIStyle();
            blackBack.normal.background = MakeTex(4, 4, Color.black);

            earthStyleOptions = new string[] {
                "Natural (2K, Unlit)", "Natural (2K, Standard Shader)", "Natural (2K, Scenic)", "Natural (2K, Scenic + City Lights)", "Alternate Style 1 (2K)", "Alternate Style 2 (2K)", "Alternate Style 3 (2K)", "Natural (8K, Unlit)", "Natural (8K, Standard Shader)", "Natural (8K Scenic)", "Natural (8K Scenic + City Lights)", "Natural (8K Scenic Scatter)", "Natural (8K Scenic Scatter + City Lights)",  "Natural (16K, Unlit)",  "Natural (16K Scenic)", "Natural (16K Scenic + City Lights)",  "Natural (16K Scenic Scatter)", "Natural (16K Scenic Scatter + City Lights)", "Solid Color", "Custom"
            };
            earthStyleValues = new int[] {
                (int)EARTH_STYLE.Natural, (int)EARTH_STYLE.StandardShader2K, (int)EARTH_STYLE.Scenic, (int)EARTH_STYLE.ScenicCityLights, (int)EARTH_STYLE.Alternate1, (int)EARTH_STYLE.Alternate2, (int)EARTH_STYLE.Alternate3,  (int)EARTH_STYLE.NaturalHighRes, (int)EARTH_STYLE.StandardShader8K, (int)EARTH_STYLE.NaturalHighResScenic, (int)EARTH_STYLE.NaturalHighResScenicCityLights, (int)EARTH_STYLE.NaturalHighResScenicScatter, (int)EARTH_STYLE.NaturalHighResScenicScatterCityLights, (int)EARTH_STYLE.NaturalHighRes16K, (int)EARTH_STYLE.NaturalHighRes16KScenic, (int)EARTH_STYLE.NaturalHighRes16KScenicCityLights, (int)EARTH_STYLE.NaturalHighRes16KScenicScatter, (int)EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights, (int)EARTH_STYLE.SolidColor, (int)EARTH_STYLE.Custom
            };

            frontiersDetailOptions = new string[] {
                "Low",
                "High"
            };
            labelsQualityOptions = new GUIContent[] {
                new GUIContent("Low (2048x1024)"),
                new GUIContent("Medium (4096x2048)"),
                new GUIContent("High (8192x4096)"),
                new GUIContent("Not Used")
            };
            labelsRenderMethods = new string[] {
                "Blended",
                "World Space"
            };
            gridModeOptions = new string[] {
                "Overlay",
                "Masked"
            };
            gridModeValues = new int[] {
                (int)GRID_MODE.OVERLAY, (int)GRID_MODE.MASKED
            };

            navigationModeOptions = new string[] {
                "Earth Rotates",
                "Camera Rotates"
            };
            navigationModeValues = new int[] {
                (int)NAVIGATION_MODE.EARTH_ROTATES, (int)NAVIGATION_MODE.CAMERA_ROTATES
            };
            zoomModeOptions = new GUIContent[] {
                new GUIContent("Camera Moves"),
                new GUIContent("Earth Moves")
            };
            zoomModeValues = new int[] {
                (int)ZOOM_MODE.CAMERA_MOVES, (int)ZOOM_MODE.EARTH_MOVES
            };
            rotationAxisOptions = new string[] { "Both", "X-Axis", "Y-Axis" };
            rotationAxisValues = new int[] {
                (int)ROTATION_AXIS_ALLOWED.BOTH_AXIS,
                (int)ROTATION_AXIS_ALLOWED.X_AXIS_ONLY,
                (int)ROTATION_AXIS_ALLOWED.Y_AXIS_ONLY
            };

            expandUniverseSection = EditorPrefs.GetBool("WPMGlobeUniverseExpand", false);
            expandEarthSection = EditorPrefs.GetBool("WPMGlobeEarthExpand", false);
            expandGridSection = EditorPrefs.GetBool("WPMGlobeGridExpand", false);
            expandTilesSection = EditorPrefs.GetBool("WPMGlobeTilesExpand", false);
            expandCitiesSection = EditorPrefs.GetBool("WPMGlobeCitiesExpand", false);
            expandFoWSection = EditorPrefs.GetBool("WPMGlobeFoWExpand", false);
            expandCountriesSection = EditorPrefs.GetBool("WPMGlobeCountriesExpand", false);
            expandProvincesSection = EditorPrefs.GetBool("WPMGlobeProvincesExpand", false);
            expandInteractionSection = EditorPrefs.GetBool("WPMGlobeInteractionExpand", false);
            expandDevicesSection = EditorPrefs.GetBool("WPMGlobeDevicesExpand", false);
            expandCameraControlSection = EditorPrefs.GetBool("WPMGlobeCameraControlExpand", false);

            UpdateExtraComponentStatus();

            isDirty = serializedObject.FindProperty("isDirty");
            zoomLevel = Mathf.Clamp(_map.GetZoomLevel(), 0, 5f);
        }

        void OnDisable() {
            EditorPrefs.SetBool("WPMGlobeUniverseExpand", expandUniverseSection);
            EditorPrefs.SetBool("WPMGlobeEarthExpand", expandEarthSection);
            EditorPrefs.SetBool("WPMGlobeGridExpand", expandGridSection);
            EditorPrefs.SetBool("WPMGlobeTilesExpand", expandTilesSection);
            EditorPrefs.SetBool("WPMGlobeCitiesExpand", expandCitiesSection);
            EditorPrefs.SetBool("WPMGlobeFoWExpand", expandFoWSection);
            EditorPrefs.SetBool("WPMGlobeCountriesExpand", expandCountriesSection);
            EditorPrefs.SetBool("WPMGlobeProvincesExpand", expandProvincesSection);
            EditorPrefs.SetBool("WPMGlobeInteractionExpand", expandInteractionSection);
            EditorPrefs.SetBool("WPMGlobeDevicesExpand", expandDevicesSection);
            EditorPrefs.SetBool("WPMGlobeCameraControlExpand", expandCameraControlSection);
        }

        void UpdateExtraComponentStatus() {
            extracomp[CALCULATOR] = _map.gameObject.GetComponent<WorldMapCalculator>() != null;
            extracomp[TICKERS] = _map.gameObject.GetComponent<WorldMapTicker>() != null;
            extracomp[DECORATOR] = _map.gameObject.GetComponent<WorldMapDecorator>() != null;
            extracomp[EDITOR] = _map.gameObject.GetComponent<WorldMapEditor>() != null;
        }

        public override void OnInspectorGUI() {
            if (_map == null || _map.countries == null) {
                return;
            }

            if (EditorPrefs.GetInt(WPM_BUILD_HINT) == 0) {
                EditorPrefs.SetInt(WPM_BUILD_HINT, 1);
                EditorUtility.DisplayDialog("World Political Map Globe Edition", "Thanks for purchasing!\nPlease read documentation for important tips about reducing application build size as this version includes many high resolution textures.\n\nFor additional help or questions please visit our Support Forum on kronnect.com\n\nWe hope you enjoy using WPM Globe Edition. Please consider rating WPM Globe Edition on the Asset Store.", "Ok");
            }


            if (_map.isDirty || (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "UndoRedoPerformed")) {
                serializedObject.UpdateIfRequiredOrScript();
            }

            if (sectionHeaderNormalStyle == null) {
                sectionHeaderNormalStyle = new GUIStyle(EditorStyles.foldout);
            }
            sectionHeaderNormalStyle.SetFoldoutColor();

            EditorGUILayout.Separator();
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginHorizontal(blackBack);
            GUILayout.Label(_headerTexture, GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            expandUniverseSection = EditorGUILayout.Foldout(expandUniverseSection, "Universe Settings", sectionHeaderNormalStyle);
            if (expandUniverseSection) {
                _map.skyboxStyle = (SKYBOX_STYLE)EditorGUILayout.EnumPopup("Skybox", _map.skyboxStyle);
                if (_map.skyboxStyle == SKYBOX_STYLE.DualSkybox) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox("A dual skybox which shows starfield or an environment cubemap depending on camera altitude", MessageType.Info);
                    _map.skyboxEnvironmentTransitionAltitudeMin = EditorGUILayout.FloatField("Transition Min Altitude", _map.skyboxEnvironmentTransitionAltitudeMin);
                    _map.skyboxEnvironmentTransitionAltitudeMax = EditorGUILayout.FloatField("Transition Max Altitude", _map.skyboxEnvironmentTransitionAltitudeMax);
                    _map.skyboxEnvironmentTextureHDR = (Texture2D)EditorGUILayout.ObjectField("Environment Texture", _map.skyboxEnvironmentTextureHDR, typeof(Texture2D), false);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.BeginHorizontal();
                _map.sun = (Transform)EditorGUILayout.ObjectField(new GUIContent("Sun GameObject", "Instead of setting a Sun Position manually, assign a Game Object (usually a Directional Light that acts as the Sun) to automatically synchronize the light direction."), _map.sun, typeof(Transform), true);
                if (GUILayout.Button("Flares?", GUILayout.Width(60))) {
                    if (EditorUtility.DisplayDialog("Sun Lens Flares FX", "For an additional Sun lens flares effect, including animated solar wind, we recommend using Beautify.\n\nBeautify is a full-screen image effect asset that enhances the image quality in real time and provides nice effects like anamorphic and Sun lens flares.", "More information", "Close")) {
                        Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/61730?aid=1101lGsd");
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                _map.syncTimeOfDay = EditorGUILayout.Toggle(new GUIContent("Sync Time of Day", "Aligns Earth and Sun rotation according to current daylight."), _map.syncTimeOfDay);
                if (_map.sun != null || _map.syncTimeOfDay)
                    GUI.enabled = false;
                _map.earthScenicLightDirection = EditorGUILayout.Vector3Field(new GUIContent("Sun Position", "Relative to the center of the Earth. Used for light direction calculation in scenic/scatter styles."), _map.earthScenicLightDirection);
                GUI.enabled = true;
                _map.showMoon = EditorGUILayout.Toggle("Show Moon", _map.showMoon);
                if (_map.showMoon) {
                    EditorGUI.indentLevel++;
                    _map.moonAutoScale = EditorGUILayout.Toggle(new GUIContent("Auto Scale", "Manages Moon position and scale automatically based on Earth dimensions."), _map.moonAutoScale);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            expandEarthSection = EditorGUILayout.Foldout(expandEarthSection, "Earth & Atmosphere Settings", sectionHeaderNormalStyle);
            if (expandEarthSection) {

                EditorGUILayout.BeginHorizontal();
                _map.showEarth = EditorGUILayout.Toggle("Show Earth", _map.showEarth);

                if (GUILayout.Button("Straighten")) {
                    _map.StraightenGlobe(1.0f);
                }

                if (GUILayout.Button("Tilt")) {
                    _map.TiltGlobe();
                }

                if (GUILayout.Button("Redraw")) {
                    _map.Redraw();
                }

                EditorGUILayout.EndHorizontal();

                if (_map.showEarth) {
                    EditorGUI.indentLevel++;
                    _map.earthStyle = (EARTH_STYLE)EditorGUILayout.IntPopup("Earth Style", (int)_map.earthStyle, earthStyleOptions, earthStyleValues);

                    if (_map.earthStyle == EARTH_STYLE.SolidColor) {
                        _map.earthColor = WPMEditorStyles.HDRColorPicker("Color", _map.earthColor, false);
                    }

                    if (_map.earthStyle.isScenic() || _map.earthStyle.isScatter()) {
                        _map.earthBumpMapEnabled = EditorGUILayout.Toggle("Enable BumpMap", _map.earthBumpMapEnabled);

                        if (_map.earthBumpMapEnabled) {
                            EditorGUI.indentLevel++;
                            _map.earthBumpMapIntensity = EditorGUILayout.Slider("Amount", _map.earthBumpMapIntensity, 0f, 1f);
                            EditorGUI.indentLevel--;
                        }

                        _map.earthSpecularEnabled = EditorGUILayout.Toggle("Enable Specular", _map.earthSpecularEnabled);

                        if (_map.earthSpecularEnabled) {
                            _map.earthSpecularPower = EditorGUILayout.Slider("Power", _map.earthSpecularPower, 1f, 128f);
                            _map.earthSpecularIntensity = EditorGUILayout.Slider("Intensity", _map.earthSpecularIntensity, 0f, 5f);
                        }

                        if (_map.earthStyle.isScatter()) {
                            _map.atmosphereScatterAlpha = EditorGUILayout.Slider("Atmosphere", _map.atmosphereScatterAlpha, 0f, 1f);
                        }

                        _map.contrast = EditorGUILayout.Slider(new GUIContent("Contrast", "Final Earth image contrast adjustment. Allows you to create more vivid images."), _map.contrast, 0.5f, 1.5f);
                        _map.brightness = EditorGUILayout.Slider(new GUIContent("Brightness", "Final Earth image brightness adjustment."), _map.brightness, 0f, 2f);
                        _map.ambientLight = EditorGUILayout.Slider(new GUIContent("Ambient", "Ambient light."), _map.ambientLight, 0f, 1f);

                        if (_map.earthStyle.hasCityLights()) {
                            EditorGUI.indentLevel--;
                            EditorGUILayout.LabelField("Cities");
                            EditorGUI.indentLevel++;
                            _map.citiesBrightness = EditorGUILayout.FloatField(new GUIContent("Brightness", "Brightness multiplier for city lights."), _map.citiesBrightness);
                        }

                        EditorGUI.indentLevel--;
                        EditorGUILayout.LabelField("Clouds");

                        EditorGUI.indentLevel++;
                        _map.cloudsAlpha = EditorGUILayout.Slider(new GUIContent("Alpha", "Transparency of the cloud layer."), _map.cloudsAlpha, 0f, 1f);
                        _map.cloudsSpeed = EditorGUILayout.Slider("Speed", _map.cloudsSpeed, -1f, 1f);
                        _map.cloudsElevation = EditorGUILayout.Slider("Elevation", _map.cloudsElevation, 0.001f, 0.1f);
                        _map.cloudsShadowEnabled = EditorGUILayout.Toggle("Shadows", _map.cloudsShadowEnabled);
                        if (_map.cloudsShadowEnabled) {
                            _map.cloudsShadowStrength = EditorGUILayout.Slider("Strength", _map.cloudsShadowStrength, 0f, 1f);
                        }
                    }

                    if (_map.earthStyle.isScenic()) {

                        EditorGUILayout.LabelField("Scenic Atmosphere");

                        EditorGUI.indentLevel++;
                        _map.atmosphereColor = WPMEditorStyles.HDRColorPicker("Tint Color", _map.atmosphereColor, false);
                        _map.atmosphereAlpha = EditorGUILayout.Slider("Alpha", _map.atmosphereAlpha, 0f, 1f);
                        _map.atmosphereFallOff = EditorGUILayout.Slider("Fall Off", _map.atmosphereFallOff, 0f, 5f);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                } else {
                    _map.showBackSide = EditorGUILayout.Toggle("Double Sided", _map.showBackSide);
                }

                EditorGUILayout.LabelField("Glow");

                EditorGUI.indentLevel++;
                _map.earthScenicGlowIntensity = EditorGUILayout.Slider("Intensity", _map.earthScenicGlowIntensity, 0, 2);

                if (!_map.earthGlowScatter) {
                    _map.earthScenicGlowColor = WPMEditorStyles.HDRColorPicker("Color", _map.earthScenicGlowColor, false);
                    _map.atmosphereThickness = EditorGUILayout.Slider("Glow Thickness", _map.atmosphereThickness, 0.88f, 1.12f);
                }

                if (!_map.earthStyle.isScatter()) {
                    // scatter always uses physically based glow
                    _map.earthGlowScatter = EditorGUILayout.Toggle("Phys. Based", _map.earthGlowScatter);
                }
                EditorGUI.indentLevel--;

                if (_map.earthStyle.isScenic()) {
                    _map.earthScenicAtmosphereIntensity = EditorGUILayout.Slider("Effect Intensity", _map.earthScenicAtmosphereIntensity, 0, 1);
                }
                if (_map.showTiles || _map.earthStyle.isScatter() || _map.earthStyle.isScenic()) {
                    GUI.enabled = false;
                    _map.earthInvertedMode = false;
                    EditorGUILayout.LabelField("Inverted Mode", "(not compatible with Scenic/Scatter/Tile modes)");
                    GUI.enabled = true;
                } else {
                    _map.earthInvertedMode = EditorGUILayout.Toggle("Inverted Mode", _map.earthInvertedMode);
                }

                if (!_map.showTiles) {
                    _map.earthHighDensityMesh = EditorGUILayout.Toggle("High Density Mesh", _map.earthHighDensityMesh);
                }

                _map.labelsQuality = (LABELS_QUALITY)EditorGUILayout.Popup(
                    new GUIContent("Overlay Resolution", "Resolution of the render texture used to draw labels in blended mode as well as tickers or custom markers. If labels are rendered in world space and you don't use markers or tickers, select 'Not Used' to save memory."),
                    (int)_map.labelsQuality,
                    labelsQualityOptions);

            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            expandGridSection = EditorGUILayout.Foldout(expandGridSection, "Grid Settings", sectionHeaderNormalStyle);
            if (expandGridSection) {
                _map.showLatitudeLines = EditorGUILayout.Toggle("Show Latitude Lines", _map.showLatitudeLines);
                _map.latitudeStepping = EditorGUILayout.IntSlider("Stepping", _map.latitudeStepping, 5, 45);
                _map.showLongitudeLines = EditorGUILayout.Toggle("Show Longitude Lines", _map.showLongitudeLines);
                _map.longitudeStepping = EditorGUILayout.IntSlider("Stepping", _map.longitudeStepping, 5, 45);
                _map.gridLinesColor = WPMEditorStyles.HDRColorPicker("Lines Color", _map.gridLinesColor);
                _map.gridMode = (GRID_MODE)EditorGUILayout.IntPopup("Mode", (int)_map.gridMode, gridModeOptions, gridModeValues);

                _map.showHexagonalGrid = EditorGUILayout.Toggle("Show Hexagonal Grid", _map.showHexagonalGrid);
                if (_map.showHexagonalGrid) {
                    EditorGUI.indentLevel++;
                    _map.hexaGridDivisions = EditorGUILayout.IntSlider("Divisions", _map.hexaGridDivisions, 15, 200);
                    float prevAlpha = _map.hexaGridColor.a;
                    _map.hexaGridColor = WPMEditorStyles.HDRColorPicker("Color", _map.hexaGridColor);
                    GUICheckTransparentColor(_map.hexaGridColor.a, prevAlpha);
                    _map.hexaGridUseMask = EditorGUILayout.Toggle("Use Mask", _map.hexaGridUseMask);
                    _map.hexaGridMask = (Texture2D)EditorGUILayout.ObjectField("Mask Texture", _map.hexaGridMask, typeof(Texture2D), false);
                    if (_map.hexaGridMask != null) {
                        EditorGUI.indentLevel++;
                        _map.hexaGridMaskThreshold = EditorGUILayout.IntSlider("Height Threshold", _map.hexaGridMaskThreshold, 0, 255);
                        EditorGUI.indentLevel--;
                    }

                    _map.hexaGridRotationShift = EditorGUILayout.Vector3Field("Rotation Shift", _map.hexaGridRotationShift);
                    _map.hexaGridHighlightEnabled = EditorGUILayout.Toggle("Enable Highlight", _map.hexaGridHighlightEnabled);
                    _map.hexaGridHighlightColor = WPMEditorStyles.HDRColorPicker("Highlight Color", _map.hexaGridHighlightColor, false);
                    _map.hexaGridHighlightSpeed = EditorGUILayout.Slider("Highlight Speed", _map.hexaGridHighlightSpeed, 0.1f, 5f);
                    _map.pathFindingHeuristicFormula = (PathFinding.HeuristicFormula)EditorGUILayout.EnumPopup("PathFinding Method", _map.pathFindingHeuristicFormula);
                    _map.pathFindingSearchLimit = EditorGUILayout.IntField("Search Limit", _map.pathFindingSearchLimit);
                    _map.pathFindingSearchMaxCost = EditorGUILayout.IntField("Max Path Cost", _map.pathFindingSearchMaxCost);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            expandTilesSection = EditorGUILayout.Foldout(expandTilesSection, "Tile System Settings", sectionHeaderNormalStyle);
            if (expandTilesSection) {

                EditorGUILayout.BeginHorizontal();
                _map.showTiles = EditorGUILayout.Toggle("Show Tiles", _map.showTiles);

                if (_map.showTiles) {
                    if (!Application.isPlaying)
                        GUI.enabled = false;
                    if (GUILayout.Button("Reload Tiles")) {
                        _map.ResetTiles();
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    _map.tileServer = (TILE_SERVER)EditorGUILayout.IntPopup("Server", (int)_map.tileServer, WorldMapGlobe.tileServerNames, WorldMapGlobe.tileServerValues);

                    if (_map.tileServer == TILE_SERVER.Custom) {
                        EditorGUILayout.LabelField("Url Template");
                        EditorGUILayout.BeginHorizontal();
                        _map.tileServerCustomUrl = EditorGUILayout.TextField(_map.tileServerCustomUrl);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.HelpBox("Use:\n$N$ for random [a-c] node (optional)\n$Z$ for zoom level (required)\n$X$ and $Y$ for X/Y tile indices (required).", MessageType.Info);
                    } else if (_map.tileServer.IsAerisWeather()) {
                        EditorGUILayout.LabelField("Copyright Notice");
                        EditorGUILayout.SelectableLabel(_map.tileServerCopyrightNotice);
                        _map.tileServerClientId = EditorGUILayout.TextField(new GUIContent("Client Id", "The client id of your Aeris Weather account."), _map.tileServerClientId);
                        _map.tileServerAPIKey = EditorGUILayout.TextField(new GUIContent("Secret Key", "Secret key linked to your Aeris Weather account."), _map.tileServerAPIKey);
                        _map.tileServerLayerTypes = EditorGUILayout.TextField(new GUIContent("Layer Types", "Enter the desired layer types (eg: radar,radar-2m,fradar,satellite-visible,satellite,satellite-infrared-color,satellite-water-vapor,fsatellite"), _map.tileServerLayerTypes);
                        if (string.IsNullOrEmpty(_map.tileServerClientId) || string.IsNullOrEmpty(_map.tileServerAPIKey) || string.IsNullOrEmpty(_map.tileServerLayerTypes)) {
                            EditorGUILayout.HelpBox("To access AerisWeather service, ClientId, SecretKey as well as one or more Layer Types must be specified.", MessageType.Warning);
                        }
                        _map.tileServerTimeOffset = EditorGUILayout.TextField(new GUIContent("Time Offset", "Enter the map time offset from now (eg. current or -10min or +1hour) or an exact date with format: YYYYMMDDhhiiss."), _map.tileServerTimeOffset);
                    } else if (_map.tileServer.IsMapBox()) {
                        EditorGUILayout.LabelField("Copyright Notice");
                        EditorGUILayout.SelectableLabel(_map.tileServerCopyrightNotice);
                        _map.tileServerAPIKey = EditorGUILayout.TextField(new GUIContent("Access Token (Required)", "Access token of your MapBox account."), _map.tileServerAPIKey);
                    } else {
                        EditorGUILayout.LabelField("Copyright Notice");
                        EditorGUILayout.SelectableLabel(_map.tileServerCopyrightNotice);
                        _map.tileServerAPIKey = EditorGUILayout.TextField(new GUIContent("API Key", "Custom portion added to tile request url. For example: apikey=1234589"), _map.tileServerAPIKey);
                    }

                    if (Application.isPlaying) {
                        EditorGUILayout.LabelField("Current Zoom Level", _map.tileCurrentZoomLevel.ToString());
                    }

                    _map.tileResolutionFactor = EditorGUILayout.Slider(new GUIContent("Tile Resolution", "A value of 2 provides de best quality whereas a lower value will reduce downloads."), _map.tileResolutionFactor, 0.2f, 2f);
                    _map.tileMaxZoomLevel = EditorGUILayout.IntSlider(new GUIContent("Max Zoom Level", "Allowed maximum zoom level. Also check Zoom Distance Min under Interaction section."), _map.tileMaxZoomLevel, WorldMapGlobe.TILE_MIN_ZOOM_LEVEL, WorldMapGlobe.TILE_MAX_ZOOM_LEVEL);
                    _map.tileMaxZoomLevelFrontiers = EditorGUILayout.IntSlider(new GUIContent("Max Zoom Level Frontiers", "Automatically hide frontiers beyond this zoom level."), _map.tileMaxZoomLevelFrontiers, WorldMapGlobe.TILE_MIN_ZOOM_LEVEL, WorldMapGlobe.TILE_MAX_ZOOM_LEVEL);
                    _map.tileTransparentLayer = EditorGUILayout.Toggle(new GUIContent("Transparent Tiles", "Enable this option to render tiles with transparency (slower)."), _map.tileTransparentLayer);

                    if (_map.tileTransparentLayer) {
                        _map.tileMaxAlpha = EditorGUILayout.Slider(new GUIContent("Max Alpha", "Maximum level of opacity."), _map.tileMaxAlpha, 0, 1f);
                    }

                    _map.tileBackgroundColor = EditorGUILayout.ColorField(new GUIContent("Background Color", "Default color for rendering background Earth. For best results, assign a color that matches the current tile theme/style"), _map.tileBackgroundColor);
                    _map.tileMaxConcurrentDownloads = EditorGUILayout.IntField(new GUIContent("Max Concurrent Downloads", "Maximum number of web downloads at any given time."), _map.tileMaxConcurrentDownloads);
                    _map.tileDownloadTimeout = EditorGUILayout.IntField(new GUIContent("Download Timeout", "Sets the maximum time (in seconds) for downloading a tile image."), _map.tileDownloadTimeout);
                    _map.tileMaxTileLoadsPerFrame = EditorGUILayout.IntField(new GUIContent("Max Loads Per Frame", "Maximum number of tiles showing up per frame."), _map.tileMaxTileLoadsPerFrame);
                    _map.tilePreloadTiles = EditorGUILayout.Toggle(new GUIContent("Preload Main Tiles", "Enable this option to quickly load from local cache all tiles belonging to first zoom level (Local cache must be enabled)."), _map.tilePreloadTiles);
                    _map.tilesUnloadInactiveTiles = EditorGUILayout.Toggle(new GUIContent("Unload Inactive Tiles", "Save memory by unloading textures from inactive tiles."), _map.tilesUnloadInactiveTiles);

                    if (_map.tilesUnloadInactiveTiles) {
                        EditorGUI.indentLevel++;
                        _map.tileKeepAlive = EditorGUILayout.FloatField(new GUIContent("Tiles Keep Alive", "Time in seconds to keep an inactive/hidden tile in memory before releasing it."), _map.tileKeepAlive);
                        EditorGUI.indentLevel--;
                    }

                    _map.tileDebugErrors = EditorGUILayout.Toggle("Show Console Errors", _map.tileDebugErrors);
                    _map.tilesShowDebugInfo = EditorGUILayout.Toggle("Show Debug Info", _map.tilesShowDebugInfo);

                    _map.tileEnableLocalCache = EditorGUILayout.Toggle("Enable Local Cache", _map.tileEnableLocalCache);

                    if (_map.tileEnableLocalCache) {
                        EditorGUI.indentLevel++;
                        _map.tileMaxLocalCacheSize = EditorGUILayout.LongField("Cache Size (Mb)", _map.tileMaxLocalCacheSize);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Cache Usage");
                    if (tileSizeComputed) {
                        GUILayout.Label((_map.tileCurrentCacheUsage / (1024f * 1024f)).ToString("F1") + " Mb");
                    }
                    if (GUILayout.Button("Recalculate")) {
                        _map.TileRecalculateCacheUsage();
                        tileSizeComputed = true;
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Purge")) {
                        _map.PurgeTileCache();
                    }
                    EditorGUILayout.EndHorizontal();

                    _map.tileEnableOfflineTiles = EditorGUILayout.Toggle("Enable Offline Tiles", _map.tileEnableOfflineTiles);
                    if (_map.tileEnableOfflineTiles) {
                        _map.tileResourcePathBase = EditorGUILayout.TextField("Resources Path", _map.tileResourcePathBase);
                        _map.tileOfflineTilesOnly = EditorGUILayout.Toggle(new GUIContent("Only Offline Tiles", "If enabled, only existing tiles from Resources path will be loaded - cache and online tiles will be ignored."), _map.tileOfflineTilesOnly);
                        if (_map.tileEnableOfflineTiles) {
                            _map.tileResourceFallbackTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Fallback Texture", "Fallback texture if the tile is not found in Resources path."), _map.tileResourceFallbackTexture, typeof(Texture2D), false);
                        }

                        if (GUILayout.Button("Open Tiles Downloader")) {
                            WorldMapTilesDownloader.ShowWindow();
                        }
                        EditorGUI.indentLevel--;
                    }

                } else {
                    EditorGUILayout.EndHorizontal();
                }

            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            expandCountriesSection = EditorGUILayout.Foldout(expandCountriesSection, "Countries Settings", sectionHeaderNormalStyle);
            if (expandCountriesSection) {

                EditorGUILayout.BeginHorizontal();
                _map.frontiersDetail = (FRONTIERS_DETAIL)EditorGUILayout.Popup("Frontiers Detail", (int)_map.frontiersDetail, frontiersDetailOptions);
                GUILayout.Label(_map.countries.Length.ToString());
                EditorGUILayout.EndHorizontal();

                _map.showInlandFrontiers = EditorGUILayout.Toggle("Inland Frontiers", _map.showInlandFrontiers);
                if (_map.showInlandFrontiers) {
                    EditorGUI.indentLevel++;
                    _map.inlandFrontiersColor = WPMEditorStyles.HDRColorPicker("Color", _map.inlandFrontiersColor);
                    EditorGUI.indentLevel--;
                }

                _map.showFrontiers = EditorGUILayout.Toggle("Show Countries", _map.showFrontiers);
                if (_map.showFrontiers) {
                    EditorGUI.indentLevel++;

                    float prevAlpha = _map.frontiersColor.a;
                    _map.frontiersColor = WPMEditorStyles.HDRColorPicker("Frontiers Color", _map.frontiersColor);
                    GUICheckTransparentColor(_map.frontiersColor.a, prevAlpha);

                    _map.showCoastalFrontiers = EditorGUILayout.Toggle("Coastal Frontiers", _map.showCoastalFrontiers);
                    _map.frontiersThicknessMode = (FRONTIERS_THICKNESS)EditorGUILayout.EnumPopup("Line Thickness", _map.frontiersThicknessMode);
                    EditorGUILayout.BeginHorizontal();
                    if (_map.frontiersThicknessMode == FRONTIERS_THICKNESS.Thin)
                        GUI.enabled = false;
                    _map.frontiersThickness = EditorGUILayout.FloatField("Line Width", _map.frontiersThickness);
                    GUI.enabled = true;
                    if (GUILayout.Button("?", GUILayout.Width(20))) {
                        EditorUtility.DisplayDialog("Custom Width", "Please note that this option is only available on systems compatible with Shader Model 4+. Where not possible, a normal thin line will be drawn instead (many mobile devices still do not support geometry shaders)", "Ok");
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }

                _map.enableCountryHighlight = EditorGUILayout.Toggle("Country Highlight", _map.enableCountryHighlight);

                if (_map.enableCountryHighlight) {
                    EditorGUI.indentLevel++;
                    _map.fillColor = WPMEditorStyles.HDRColorPicker("Highlight Color", _map.fillColor, false);

                    _map.showOutline = EditorGUILayout.Toggle("Draw Outline", _map.showOutline);
                    if (_map.showOutline) {
                        EditorGUI.indentLevel++;
                        _map.outlineColor = WPMEditorStyles.HDRColorPicker("Outline Color", _map.outlineColor);
                        if (_map.surfacesCount > 75) {
                            GUIStyle warningLabelStyle = new GUIStyle(GUI.skin.label);
                            warningLabelStyle.normal.textColor = new Color(0.31f, 0.38f, 0.56f);
                            GUILayout.Label("Consider disabling outline to improve performance", warningLabelStyle);
                        }
                        EditorGUI.indentLevel--;
                    }
                    _map.countryHighlightMaxScreenAreaSize = EditorGUILayout.Slider(new GUIContent("Max Screen Size", "Defines the maximum screen area of a highlighted country. To prevent filling the whole screen with the highlight color, you can reduce this value and if the highlighted screen area size is greater than this factor (1=whole screen) the country won't be filled at all (it will behave as selected though)"), _map.countryHighlightMaxScreenAreaSize, 0, 1f);
                    _map.highlightAllCountryRegions = EditorGUILayout.Toggle("Include All Regions", _map.highlightAllCountryRegions);
                    EditorGUI.indentLevel--;
                }
                _map.enableCountryEnclaves = EditorGUILayout.Toggle(new GUIContent("Enable Enclaves", "Allow a country to be surrounded by another country."), _map.enableCountryEnclaves);

                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
                EditorGUILayout.BeginVertical();

                _map.showCountryNames = EditorGUILayout.Toggle("Show Country Names", _map.showCountryNames);

                if (_map.showCountryNames) {
                    EditorGUI.indentLevel++;
                    _map.labelsRenderMethod = (LABELS_RENDER_METHOD)EditorGUILayout.Popup("Render Method", (int)_map.labelsRenderMethod, labelsRenderMethods);

                    if (_map.labelsRenderMethod == LABELS_RENDER_METHOD.Blended) {
                        EditorGUILayout.LabelField("Labels Quality", "(use 'Overlay Resolution' setting under Earth section)");
                    }

                    _map.countryLabelsSize = EditorGUILayout.Slider("Relative Size", _map.countryLabelsSize, 0.001f, 0.9f);
                    _map.countryLabelsAbsoluteMinimumSize = EditorGUILayout.Slider("Minimum Size", _map.countryLabelsAbsoluteMinimumSize, 0.001f, 2.5f);
                    _map.countryLabelsFont = (Font)EditorGUILayout.ObjectField("Font", _map.countryLabelsFont, typeof(Font), false);
                    _map.labelsElevation = EditorGUILayout.Slider("Elevation", _map.labelsElevation, 0.0f, 1.0f);
                    _map.countryLabelsColor = WPMEditorStyles.HDRColorPicker("Labels Color", _map.countryLabelsColor);
                    _map.showLabelsShadow = EditorGUILayout.Toggle("Draw Shadow", _map.showLabelsShadow);
                    if (_map.showLabelsShadow) {
                        EditorGUI.indentLevel++;
                        _map.countryLabelsShadowColor = WPMEditorStyles.HDRColorPicker("Shadow Color", _map.countryLabelsShadowColor);
                        _map.countryLabelsShadowOffset = EditorGUILayout.Slider("Offset", _map.countryLabelsShadowOffset, 0, 2f);
                        EditorGUI.indentLevel--;
                    }

                    _map.countryLabelsEnableAutomaticFade = EditorGUILayout.Toggle("Auto Fade Labels", _map.countryLabelsEnableAutomaticFade);

                    if (_map.countryLabelsEnableAutomaticFade) {
                        EditorGUI.indentLevel++;
                        _map.countryLabelsAutoFadeMinHeight = EditorGUILayout.Slider("Min Height", _map.countryLabelsAutoFadeMinHeight, 0.01f, 0.25f);
                        _map.countryLabelsAutoFadeMinHeightFallOff = EditorGUILayout.Slider("Min Height Fall Off", _map.countryLabelsAutoFadeMinHeightFallOff, 0.001f, _map.countryLabelsAutoFadeMinHeight);
                        _map.countryLabelsAutoFadeMaxHeight = EditorGUILayout.Slider("Max Height", _map.countryLabelsAutoFadeMaxHeight, 0.1f, 1.0f);
                        _map.countryLabelsAutoFadeMaxHeightFallOff = EditorGUILayout.Slider("Max Height Fall Off", _map.countryLabelsAutoFadeMaxHeightFallOff, 0.01f, 1f);
                        _map.countryLabelsFadePerFrame = EditorGUILayout.IntSlider("Labels Per Frame", _map.countryLabelsFadePerFrame, 1, _map.countries.Length);
                        EditorGUI.indentLevel--;
                    }

                    _map.labelsFaceToCamera = EditorGUILayout.Toggle("Upright Labels", _map.labelsFaceToCamera);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            expandProvincesSection = EditorGUILayout.Foldout(expandProvincesSection, "Provinces Settings", sectionHeaderNormalStyle);
            EditorGUILayout.EndHorizontal();
            if (expandProvincesSection) {

                _map.showProvinces = EditorGUILayout.Toggle("Show Provinces", _map.showProvinces);
                if (_map.showProvinces) {
                    _map.drawAllProvinces = EditorGUILayout.Toggle("Draw All Provinces", _map.drawAllProvinces);
                    float prevAlpha = _map.provincesColor.a;
                    _map.provincesColor = WPMEditorStyles.HDRColorPicker("Provinces Color", _map.provincesColor);
                    GUICheckTransparentColor(_map.provincesColor.a, prevAlpha);
                    _map.enableProvinceHighlight = EditorGUILayout.Toggle("Enable Highlight", _map.enableProvinceHighlight);
                    if (_map.enableProvinceHighlight) {
                        EditorGUI.indentLevel++;
                        _map.provincesFillColor = WPMEditorStyles.HDRColorPicker("Color", _map.provincesFillColor);
                        _map.provinceHighlightMaxScreenAreaSize = EditorGUILayout.Slider(new GUIContent("Max Screen Size", "Defines the maximum screen area of a highlighted province. To prevent filling the whole screen with the highlight color, you can reduce this value and if the highlighted screen area size is greater than this factor (1=whole screen) the province won't be filled at all (it will behave as selected though)"), _map.provinceHighlightMaxScreenAreaSize, 0, 1f);
                        _map.showProvinceCountryOutline = EditorGUILayout.Toggle("Show Country Outline", _map.showProvinceCountryOutline);
                        EditorGUI.indentLevel--;
                    }
                }
                _map.enableProvinceEnclaves = EditorGUILayout.Toggle(new GUIContent("Enable Enclaves", "Allow a province to be surrounded by another province."), _map.enableProvinceEnclaves);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            expandCitiesSection = EditorGUILayout.Foldout(expandCitiesSection, "Cities Settings", sectionHeaderNormalStyle);
            EditorGUILayout.EndHorizontal();
            if (expandCitiesSection) {

                _map.showCities = EditorGUILayout.Toggle("Show Cities", _map.showCities);

                if (_map.showCities && _map.cities != null) {
                    EditorGUI.indentLevel++;

                    _map.citiesColor = WPMEditorStyles.HDRColorPicker("Cities Color", _map.citiesColor);
                    _map.citiesRegionCapitalColor = WPMEditorStyles.HDRColorPicker("Region Cap. Color", _map.citiesRegionCapitalColor);
                    _map.citiesCountryCapitalColor = WPMEditorStyles.HDRColorPicker("Capital Color", _map.citiesCountryCapitalColor);
                    _map.cityIconSize = EditorGUILayout.Slider("Icon Size", _map.cityIconSize, 0.02f, 1f);
                    _map.combineCityMeshes = EditorGUILayout.Toggle("Combine Meshes", _map.combineCityMeshes);

                    EditorGUILayout.BeginHorizontal();
                    _map.minPopulation = EditorGUILayout.IntSlider("Min Population (K)", _map.minPopulation, 0, 3000);
                    GUILayout.Label(_map.numCitiesDrawn + "/" + _map.cities.Count);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Always Visible:");
                    EditorGUI.indentLevel++;
                    int cityClassFilter = 0;
                    bool cityBit;
                    cityBit = EditorGUILayout.ToggleLeft("Region Capitals", (_map.cityClassAlwaysShow & WorldMapGlobe.CITY_CLASS_FILTER_REGION_CAPITAL_CITY) != 0);
                    if (cityBit) {
                        cityClassFilter += WorldMapGlobe.CITY_CLASS_FILTER_REGION_CAPITAL_CITY;
                    }
                    cityBit = EditorGUILayout.ToggleLeft("Country Capitals", (_map.cityClassAlwaysShow & WorldMapGlobe.CITY_CLASS_FILTER_COUNTRY_CAPITAL_CITY) != 0);
                    if (cityBit) {
                        cityClassFilter += WorldMapGlobe.CITY_CLASS_FILTER_COUNTRY_CAPITAL_CITY;
                    }
                    _map.cityClassAlwaysShow = cityClassFilter;

                    EditorGUI.indentLevel--;

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            expandFoWSection = EditorGUILayout.Foldout(expandFoWSection, "Fog Of War Settings", sectionHeaderNormalStyle);
            EditorGUILayout.EndHorizontal();
            if (expandFoWSection) {

                _map.showFogOfWar = EditorGUILayout.Toggle("Show Fog Of War", _map.showFogOfWar);

                if (_map.showFogOfWar) {
                    EditorGUI.indentLevel++;
                    _map.fogOfWarResolution = EditorGUILayout.IntSlider("Resolution", _map.fogOfWarResolution, 8, 12);
                    _map.fogOfWarColor1 = WPMEditorStyles.HDRColorPicker("Color 1", _map.fogOfWarColor1);
                    _map.fogOfWarColor2 = WPMEditorStyles.HDRColorPicker("Color 2", _map.fogOfWarColor2);
                    _map.fogOfWarNoise = EditorGUILayout.Slider("Noise", _map.fogOfWarNoise, 0, 1f);
                    _map.fogOfWarAlpha = EditorGUILayout.Slider("Alpha", _map.fogOfWarAlpha, 0, 1f);
                    _map.fogOfWarElevation = EditorGUILayout.Slider("Elevation", _map.fogOfWarElevation, 0f, 0.25f);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            expandInteractionSection = EditorGUILayout.Foldout(expandInteractionSection, "Interaction Settings", sectionHeaderNormalStyle);
            EditorGUILayout.EndHorizontal();
            if (expandInteractionSection) {

                _map.showCursor = EditorGUILayout.Toggle("Show Cursor", _map.showCursor);

                if (_map.showCursor) {
                    EditorGUI.indentLevel++;
                    _map.cursorStyle = (CURSOR_STYLE)EditorGUILayout.EnumPopup("Cursor Style", _map.cursorStyle);
                    _map.cursorColor = WPMEditorStyles.HDRColorPicker("Cursor Color", _map.cursorColor);
                    _map.cursorFollowMouse = EditorGUILayout.Toggle("Follow Mouse", _map.cursorFollowMouse);
                    _map.cursorAlwaysVisible = EditorGUILayout.Toggle("Always Visible", _map.cursorAlwaysVisible);
                    EditorGUI.indentLevel--;
                }

                _map.respectOtherUI = EditorGUILayout.Toggle("Respect Other UI", _map.respectOtherUI);

                _map.allowUserRotation = EditorGUILayout.Toggle("Allow User Rotation", _map.allowUserRotation);
                if (_map.allowUserRotation) {
                    EditorGUI.indentLevel++;
                    if (_map.syncTimeOfDay) {
                        EditorGUILayout.LabelField("Mode", "(Set to Camera Rotates)");
                        EditorGUILayout.LabelField("", "(Sync Time of Day is ON)");
                    } else {
                        _map.navigationMode = (NAVIGATION_MODE)EditorGUILayout.IntPopup("Mode", (int)_map.navigationMode, navigationModeOptions, navigationModeValues);
                    }
                    _map.mouseDragSensitivity = EditorGUILayout.Slider(new GUIContent("Speed (Earth Rotates)", "Speed when rotation mode is set to Earth Rotates"), _map.mouseDragSensitivity, 0.1f, 3);
                    _map.cameraRotationSensibility = EditorGUILayout.Slider(new GUIContent("Speed (Camera Rotates)", "Speed when rotation mode is set to Camera Rotates"), _map.cameraRotationSensibility, 0.1f, 3);
                    _map.rotationAxisAllowed = (ROTATION_AXIS_ALLOWED)EditorGUILayout.IntPopup("Allowed Axis", (int)_map.rotationAxisAllowed, rotationAxisOptions, rotationAxisValues);
                    _map.dragConstantSpeed = EditorGUILayout.Toggle("Constant Drag Speed", _map.dragConstantSpeed);
                    EditorGUI.indentLevel++;
                    if (_map.dragConstantSpeed) {
                        _map.dragConstantSpeedDampingMultiplier = EditorGUILayout.Slider(new GUIContent("Damping Speed", "The speed at which damping occurs when dragging ends."), _map.dragConstantSpeedDampingMultiplier, 0, 1f);
                    } else {
                        _map.dragMaxDuration = EditorGUILayout.FloatField(new GUIContent("Max Drag Duration", "The duration of a drag or swipe in seconds (0=no max duration). If you wish to avoid continuous drag, set this value to 0.3 or similar to only allow a short/quick swipe gesture."), _map.dragMaxDuration);
                        _map.dragDampingDuration = EditorGUILayout.FloatField(new GUIContent("Damping Duration", "The duration of the drag/rotation after a drag until the Earth stops completely."), _map.dragDampingDuration);
                    }
                    EditorGUI.indentLevel--;
                    _map.mouseDragThreshold = EditorGUILayout.IntField(new GUIContent("Drag Threshold", "Enter a threshold value to avoid accidental map dragging when clicking on HiDpi screens. Values of 5, 10, 20 or more, depending on the sensitivity of the screen."), _map.mouseDragThreshold);
                    _map.centerOnRightClick = EditorGUILayout.Toggle("Right Click Centers", _map.centerOnRightClick);
                    _map.rightButtonDragBehaviour = (DRAG_BEHAVIOUR)EditorGUILayout.EnumPopup("Right Drag Behaviour", _map.rightButtonDragBehaviour);
                    if (_map.rightButtonDragBehaviour == DRAG_BEHAVIOUR.CameraOrbit && !_map.enableOrbit) {
                        EditorGUILayout.HelpBox("Enable Orbit option in Navigation & Camera Control section.", MessageType.Warning);
                    }

                    if (_map.rightButtonDragBehaviour == DRAG_BEHAVIOUR.Rotate) {
                        EditorGUI.indentLevel++;
                        _map.rightClickRotatingClockwise = EditorGUILayout.Toggle("Clockwise Rotation", _map.rightClickRotatingClockwise);
                        EditorGUI.indentLevel--;
                    }

                    _map.allowUserKeys = EditorGUILayout.Toggle("Allow Keys (WASD)", _map.allowUserKeys);
                    _map.allowHighlightWhileDragging = EditorGUILayout.Toggle("Highlight While Dragging", _map.allowHighlightWhileDragging);

                    _map.keepStraight = EditorGUILayout.Toggle("Keep Straight", _map.keepStraight);
                    _map.constraintPositionEnabled = EditorGUILayout.Toggle("Constraint Rotation", _map.constraintPositionEnabled);
                    if (_map.constraintPositionEnabled) {
                        EditorGUI.indentLevel++;
                        _map.constraintPosition = EditorGUILayout.Vector3Field("Sphere Position", _map.constraintPosition);
                        _map.constraintPositionAngle = EditorGUILayout.FloatField("Max Angle", _map.constraintPositionAngle);
                        EditorGUI.indentLevel--;
                    }
                    _map.constraintLatitudeEnabled = EditorGUILayout.Toggle("Constraint Latitude", _map.constraintLatitudeEnabled);
                    if (_map.constraintLatitudeEnabled) {
                        EditorGUI.indentLevel++;
                        _map.constraintLatitudeMinAngle = EditorGUILayout.Slider("Min Angle", _map.constraintLatitudeMinAngle, -90, 90);
                        _map.constraintLatitudeMaxAngle = EditorGUILayout.Slider("Max Angle", _map.constraintLatitudeMaxAngle, -90, 90);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                _map.dragOnScreenEdges = EditorGUILayout.Toggle("Drag On Screen Edges", _map.dragOnScreenEdges);
                if (_map.dragOnScreenEdges) {
                    EditorGUI.indentLevel++;
                    _map.dragOnScreenEdgesMarginPercentage = EditorGUILayout.Slider("Margin Percentage", _map.dragOnScreenEdgesMarginPercentage, 0, 0.3f);
                    _map.dragOnScreenEdgesSpeed = EditorGUILayout.Slider("Speed", _map.dragOnScreenEdgesSpeed, 0.1f, 3f);
                    EditorGUI.indentLevel--;
                }

                _map.allowUserZoom = EditorGUILayout.Toggle("Allow User Zoom", _map.allowUserZoom);
                if (_map.allowUserZoom) {
                    EditorGUI.indentLevel++;
                    if (_map.earthInvertedMode) {
                        GUI.enabled = false;
                    }
                    _map.zoomMode = (ZOOM_MODE)EditorGUILayout.IntPopup(new GUIContent("Mode", "If inverted mode is enabled, zoom works by changing the field of view (no Earth nor camera moves)"), (int)_map.zoomMode, zoomModeOptions, zoomModeValues);
                    GUI.enabled = true;
                    _map.mouseWheelSensitivity = EditorGUILayout.Slider(new GUIContent("Speed (Earth Moves)", "Speed when zoom mode is set to Earth Moves"), _map.mouseWheelSensitivity, 0.1f, 3);
                    _map.cameraZoomSpeed = EditorGUILayout.Slider(new GUIContent("Speed (Camera Moves)", "Speed when zoom mode is set to Camera Moves"), _map.cameraZoomSpeed, 0.1f, 3);
                    _map.zoomConstantSpeed = EditorGUILayout.Toggle("Constant Zoom Speed", _map.zoomConstantSpeed);
                    if (!_map.zoomConstantSpeed) {
                        _map.zoomDamping = EditorGUILayout.FloatField(new GUIContent("Damping Duration", "The duration of the translation after zoom is performed."), _map.zoomDamping);
                    }
                    if (_map.earthInvertedMode) {
                        EditorGUILayout.LabelField("Zoom At Mouse Pos", "(not available in Inverted Mode)");
                    } else {
                        _map.zoomAtMousePosition = EditorGUILayout.Toggle("Zoom At Mouse Pos", _map.zoomAtMousePosition);
                    }
                    _map.invertZoomDirection = EditorGUILayout.Toggle("Invert Direction", _map.invertZoomDirection);
                    _map.zoomMinDistance = EditorGUILayout.FloatField(new GUIContent("Distance Min", "Minimum camera distance to Earth surface"), _map.zoomMinDistance);
                    _map.zoomMaxDistance = EditorGUILayout.FloatField(new GUIContent("Distance Max", "Maximum camera distance to Earth surface"), _map.zoomMaxDistance);
                    if (!Application.isPlaying) {
                        float prevZoomLevel = zoomLevel;
                        zoomLevel = EditorGUILayout.Slider(new GUIContent("Zoom Level", "Set the zoom level according to the distance min/max."), zoomLevel, 0, 5f);
                        if (zoomLevel != prevZoomLevel) {
                            _map.SetZoomLevel(zoomLevel);
                        }
                    }

                    EditorGUI.indentLevel--;

                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            expandCameraControlSection = EditorGUILayout.Foldout(expandCameraControlSection, "Navigation & Camera Control", sectionHeaderNormalStyle);
            EditorGUILayout.EndHorizontal();

            if (expandCameraControlSection) {
                _map.mainCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "If empty, it will use Camera.main for any interaction."), _map.mainCamera, typeof(Camera), true);
                if (_map.syncTimeOfDay)
                    GUI.enabled = false;
                EditorGUILayout.BeginHorizontal();
                _map.autoRotationSpeed = EditorGUILayout.Slider("AutoRotation (Earth)", _map.autoRotationSpeed, -2f, 2f);
                if (GUILayout.Button("Stop")) {
                    _map.autoRotationSpeed = 0;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _map.cameraAutoRotationSpeed = EditorGUILayout.Slider("AutoRotation (Camera)", _map.cameraAutoRotationSpeed, -2f, 2f);
                if (GUILayout.Button("Stop")) {
                    _map.cameraAutoRotationSpeed = 0;
                }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;

                _map.navigationTime = EditorGUILayout.Slider("Navigation Time", _map.navigationTime, 0, 10);
                _map.navigationBounceIntensity = EditorGUILayout.Slider("Navigation Bounce", _map.navigationBounceIntensity, 0, 1);

                float prevZoomLevel = Mathf.Clamp(_map.GetZoomLevel(), 0, 2);
                float zoomLevel = EditorGUILayout.Slider("Zoom", prevZoomLevel, 0, 2);
                if (zoomLevel != prevZoomLevel) {
                    _map.SetZoomLevel(zoomLevel);
                }

                _map.enableOrbit = EditorGUILayout.Toggle("Enable Orbit", _map.enableOrbit);
                if (_map.enableOrbit) {
                    EditorGUI.indentLevel++;
                    _map.pitch = EditorGUILayout.Slider("Pitch", _map.pitch, 0, _map.maxPitch);
                    _map.maxPitch = EditorGUILayout.Slider("Max Pitch", _map.maxPitch, 0, 90);
                    _map.lockPitch = EditorGUILayout.Toggle("Lock Pitch", _map.lockPitch);
                    _map.pitchAdjustByDistance = EditorGUILayout.Toggle(new GUIContent("Adjust Pitch By Distance", "Reduce pitch as camera gets away from target."), _map.pitchAdjustByDistance);
                    if (_map.pitchAdjustByDistance) {
                        EditorGUI.indentLevel++;
                        _map.pitchMaxDistanceTilt = EditorGUILayout.FloatField(new GUIContent("Tilt Max Distance", "The maximum distance at which user can tilt/orbit the camera."), _map.pitchMaxDistanceTilt);
                        _map.pitchMinDistanceTilt = EditorGUILayout.FloatField(new GUIContent("Tilt Min Distance", "The minimum distance at which user can fully tilt/orbit the camera."), _map.pitchMinDistanceTilt);
                        _map.orbitTiltOnZoomIn = EditorGUILayout.Toggle(new GUIContent("Tilt When Zooming", "Tilts the camera automatically when zooming in."), _map.orbitTiltOnZoomIn);
                        EditorGUI.indentLevel--;
                    }
                    _map.yaw = EditorGUILayout.Slider("Yaw", _map.yaw, -359, 359);
                    _map.lockYaw = EditorGUILayout.Toggle("Lock Yaw", _map.lockYaw);
                    _map.targetElevation = EditorGUILayout.FloatField(new GUIContent("Target Elevation", "Elevation of target in km from surface"), _map.targetElevation);
                    _map.orbitSmoothSpeed = EditorGUILayout.FloatField("Smooth Speed", _map.orbitSmoothSpeed);
                    _map.orbitTarget = (Transform)EditorGUILayout.ObjectField(new GUIContent("Target", "Optionally assign a target gameobject to perform the orbit. If not provided, the surface position of the globe is orbited instead."), _map.orbitTarget, typeof(Transform), true);
                    _map.orbitSingleAxis = EditorGUILayout.Toggle(new GUIContent("Single Axis", "Orbit around a single axis (X or Y)."), _map.orbitSingleAxis);
                    _map.orbitInvertDragDirection = EditorGUILayout.Toggle("Invert Drag Direction", _map.orbitInvertDragDirection);
                    EditorGUI.indentLevel--;
                }
                _map.tilePreciseRotation = EditorGUILayout.Toggle(new GUIContent("Precise Rotation", "Enable this option to improve drag/rotation at higher zoom levels in tile mode. When enabled, camera rotation mode switches automatically to 'Camera Rotates' when zoom level is greater than 16 and switches back to 'Earth Rotates' when zoom level is reduced. This change solves floating point precision issues that arise when using higher zoom levels."), _map.tilePreciseRotation);
                if (_map.tilePreciseRotation) {
                    EditorGUI.indentLevel++;
                    _map.tilePreciseRotationZoomLevel = EditorGUILayout.IntSlider(new GUIContent("Tile Zoom Level", "Zoom level beyond which the camera zoom mode changes."), _map.tilePreciseRotationZoomLevel, 10, 25);
                    EditorGUI.indentLevel--;
                }
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            expandDevicesSection = EditorGUILayout.Foldout(expandDevicesSection, "Other Settings", sectionHeaderNormalStyle);
            EditorGUILayout.EndHorizontal();
            if (expandDevicesSection) {
                _map.followDeviceGPS = EditorGUILayout.Toggle(new GUIContent("Follow Device GPS", "If set to true, the location service will be initialized (if needed) and map will be centered on current coordinates returned by the device GPS (if available)."), _map.followDeviceGPS);
                _map.VREnabled = EditorGUILayout.Toggle(new GUIContent("VR Enabled", "Set this to true if you wish to enable interaction in normal mode. When inverted mode is enabled, VR Enabled is automatically enabled as well."), _map.VREnabled);
                _map.overlayLayerIndex = EditorGUILayout.LayerField(new GUIContent("Overlay Layer", "Layer index for the overlay child which is the root for all the labels and markers. Can be used to apply selective bloom effects using Beautify."), _map.overlayLayerIndex);

                EditorGUILayout.BeginHorizontal();
                _map.geodataResourcesPath = EditorGUILayout.TextField(new GUIContent("Geodata Folder", "Path after any Resources folder where geodata files reside."), _map.geodataResourcesPath);
                if (GUILayout.Button("Show", GUILayout.Width(60))) {
                    string path = GetGeodataReourcesFullPath();
                    if (System.IO.Directory.Exists(path)) {
                        Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                        if (obj != null) {
                            EditorGUIUtility.PingObject(obj);
                        }
                    } else {
                        EditorUtility.DisplayDialog("Geodata Folder", "Folder not found.", "Ok");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            // Extra components opener
            EditorGUILayout.Separator();
            float buttonWidth = EditorGUIUtility.currentViewWidth * 0.4f;

            if (_map.gameObject.activeInHierarchy) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (extracomp[CALCULATOR]) {
                    if (GUILayout.Button("Close Calculator", GUILayout.Width(buttonWidth))) {
                        WorldMapCalculator e = _map.gameObject.GetComponent<WorldMapCalculator>();
                        if (e != null)
                            DestroyImmediate(e);
                        UpdateExtraComponentStatus();
                        EditorGUIUtility.ExitGUI();
                    }
                } else {
                    if (GUILayout.Button("Open Calculator", GUILayout.Width(buttonWidth))) {
                        WorldMapCalculator e = _map.gameObject.GetComponent<WorldMapCalculator>();
                        if (e == null)
                            _map.gameObject.AddComponent<WorldMapCalculator>();
                        UpdateExtraComponentStatus();
                    }
                }

                if (extracomp[TICKERS]) {
                    if (GUILayout.Button("Close Ticker", GUILayout.Width(buttonWidth))) {
                        WorldMapTicker e = _map.gameObject.GetComponent<WorldMapTicker>();
                        if (e != null)
                            DestroyImmediate(e);
                        UpdateExtraComponentStatus();
                        EditorGUIUtility.ExitGUI();
                    }
                } else {
                    if (GUILayout.Button("Open Ticker", GUILayout.Width(buttonWidth))) {
                        WorldMapTicker e = _map.gameObject.GetComponent<WorldMapTicker>();
                        if (e == null)
                            _map.gameObject.AddComponent<WorldMapTicker>();
                        UpdateExtraComponentStatus();
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (extracomp[EDITOR]) {
                    if (GUILayout.Button("Close Map Editor", GUILayout.Width(buttonWidth))) {
                        _map.HideProvinces();
                        _map.HideCountrySurfaces();
                        _map.HideProvinceSurfaces();
                        _map.Redraw();
                        WorldMapEditor e = _map.gameObject.GetComponent<WorldMapEditor>();
                        if (e != null) {
                            WorldMapEditorInspector[] editors = Resources.FindObjectsOfTypeAll<WorldMapEditorInspector>();
                            if (editors.Length > 0)
                                editors[0].OnCloseEditor();
                            DestroyImmediate(e);
                        }
                        UpdateExtraComponentStatus();
                        GUIUtility.ExitGUI();
                    }
                } else {
                    if (GUILayout.Button("Open Map Editor", GUILayout.Width(buttonWidth))) {
                        WorldMapEditor e = _map.gameObject.GetComponent<WorldMapEditor>();
                        if (e == null)
                            _map.gameObject.AddComponent<WorldMapEditor>();
                        // cancel scenic shaders since they look awful in editor window
                        if (_map.earthStyle == EARTH_STYLE.Scenic || _map.earthStyle == EARTH_STYLE.ScenicCityLights)
                            _map.earthStyle = EARTH_STYLE.Natural;
                        if (_map.earthStyle == EARTH_STYLE.NaturalHighResScenic || _map.earthStyle == EARTH_STYLE.NaturalHighResScenicScatter ||
                            _map.earthStyle == EARTH_STYLE.NaturalHighResScenicScatterCityLights || _map.earthStyle == EARTH_STYLE.NaturalHighResScenicCityLights ||
                            _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenicScatter || _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights ||
                            _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenic || _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenicCityLights)
                            _map.earthStyle = EARTH_STYLE.NaturalHighRes;
                        UpdateExtraComponentStatus();
                        // Unity 5.3.1 prevents raycasting in the scene view if rigidbody is present
                        Rigidbody rb = _map.gameObject.GetComponent<Rigidbody>();
                        if (rb != null) {
                            DestroyImmediate(rb);
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }
                }

                if (extracomp[DECORATOR]) {
                    if (GUILayout.Button("Close Decorator", GUILayout.Width(buttonWidth))) {
                        WorldMapDecorator e = _map.gameObject.GetComponent<WorldMapDecorator>();
                        if (e != null)
                            DestroyImmediate(e);
                        UpdateExtraComponentStatus();
                        GUIUtility.ExitGUI();
                    }
                } else {
                    if (GUILayout.Button("Open Decorator", GUILayout.Width(buttonWidth))) {
                        WorldMapDecorator e = _map.gameObject.GetComponent<WorldMapDecorator>();
                        if (e == null)
                            _map.gameObject.AddComponent<WorldMapDecorator>();
                        UpdateExtraComponentStatus();
                    }
                }
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("About", GUILayout.Width(buttonWidth * 2.0f))) {
                WorldMapAbout.ShowAboutWindow();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_map.isDirty) {
                serializedObject.UpdateIfRequiredOrScript();
                isDirty.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }


        void GUICheckTransparentColor(float newAlpha, float prevAlpha) {
            if (newAlpha != prevAlpha)
                GUIUtility.ExitGUI();
            if (newAlpha < 1f) {
                EditorGUILayout.HelpBox("Transparent color selected: rendering can be slower.", MessageType.Info);
            }
        }


#if !UNITY_WEBPLAYER
        // Add a menu item called "Bake Earth Texture" to a WPM's context menu.
        [MenuItem("CONTEXT/WorldMapGlobe/Bake Earth Texture")]
        static void RestoreBackup(MenuCommand command) {
            if (!EditorUtility.DisplayDialog("Bake Earth Texture", "This command will render the colorized areas to the current texture and save it to EarthCustom.png file inside Textures folder (existing file from a previous bake texture operation will be replaced).\n\nThis command can take some time depending on the current texture resolution and CPU speed, from a few seconds to one minute for high-res (8K) texstures.\n\nProceed?", "Ok", "Cancel"))
                return;

            // Proceed and restore
            string[] paths = AssetDatabase.GetAllAssetPaths();
            string textureFolder = "";
            for (int k = 0; k < paths.Length; k++) {
                if (paths[k].EndsWith("WorldPoliticalMapGlobeEdition/Resources/Textures")) {
                    textureFolder = paths[k];
                    break;
                }
            }
            if (textureFolder.Length > 0) {
                string fileName = "EarthCustom.png";
                string outputFile = textureFolder + "/" + fileName;
                if (((WorldMapGlobe)command.context).BakeTexture(outputFile) != null) {
                    AssetDatabase.Refresh();
                }
                EditorUtility.DisplayDialog("Operation successful!", "Texture saved as \"" + fileName + "\" in WorldPoliticalMapGlobeEdition/Resources/Textures folder.\n\nTo use this texture:\n1- Check the import settings of the texture file (ensure max resolution is appropiated; should be 2048 at least, 8192 for high-res 8K textures).\n2- Set Earth style to Custom.", "Ok");
            } else {
                EditorUtility.DisplayDialog("Required folder not found", "Cannot find \".../WorldPoliticalMapGlobeEdition/Resources/Textures\" folder!", "Ok");
            }

        }
#endif


        [MenuItem("CONTEXT/WorldMapGlobe/Tiles Downloader")]
        static void TilesDownloaderMenuOption(MenuCommand command) {
            WorldMapTilesDownloader.ShowWindow();
        }



        Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            TextureFormat tf = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat) ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
            Texture2D result = new Texture2D(width, height, tf, false);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }


        string GetGeodataReourcesFullPath() {
            string rootFolder;
            string path = "";
            string[] paths = AssetDatabase.GetAllAssetPaths();
            for (int k = 0; k < paths.Length; k++) {
                if (paths[k].EndsWith("WorldPoliticalMapGlobeEdition")) {
                    rootFolder = paths[k];
                    path = rootFolder + "/Resources/" + _map.geodataResourcesPath;
                    break;
                }
            }
            return path;
        }


    }

}