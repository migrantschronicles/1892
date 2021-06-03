using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace WPM {
    [CustomEditor(typeof(WorldMapEditor))]
    public class WorldMapEditorInspector : Editor {

        const float HANDLE_SIZE = 0.05f;
        const float HIT_PRECISION = 0.001f;
        const string EDITORPREF_SCALE_WARNED = "ScaleWarned";
        const string INFO_MSG_CHANGES_SAVED = "Changes saved. Original geodata files in /Backup folder.";
        const string INFO_MSG_REGION_DELETED = "Region deleted!";
        const string INFO_MSG_BACKUP_NOT_FOUND = "Backup folder not found!";
        const string INFO_MSG_BACKUP_RESTORED = "Backup restored.";
        const string INFO_MSG_GEODATA_LOW_QUALITY_CREATED = "Low quality geodata file created.";
        const string INFO_MSG_CITY_DELETED = "City deleted!";
        const string INFO_MSG_NO_CHANGES_TO_SAVE = "Nothing changed to save!";
        const string INFO_MSG_CHOOSE_COUNTRY = "Choose a country first.";
        const string INFO_MSG_CHOOSE_PROVINCE = "Choose a province first.";
        const string INFO_MSG_CONTINENT_DELETED = "Continent deleted!";
        const string INFO_MSG_COUNTRY_DELETED = "Country deleted!";
        const string INFO_MSG_PROVINCE_DELETED = "Province deleted!";
        const string INFO_MSG_MOUNT_POINT_DELETED = "Mount point deleted!";

        static Vector3 pointSnap = Misc.Vector3one * 0.1f;
        WorldMapEditor _editor;
        GUIStyle labelsStyle, attribHeaderStyle, editorCaptionLabelStyle;
        GUIContent[] mainToolbarIcons;
        GUIContent[] reshapeRegionToolbarIcons, reshapeCityToolbarIcons, reshapeMountPointToolbarIcons, createToolbarIcons;
        int[] controlIds;
        bool startedReshapeRegion, startedReshapeCity, startedReshapeMountPoint, undoPushStarted;
        long tickStart;
        string[] reshapeRegionModeExplanation, reshapeCityModeExplanation, reshapeMountPointModeExplanation, editingModeOptions, editingCountryFileOptions, createModeExplanation, cityClassOptions;
        int[] cityClassValues;
        string[] emptyStringArray;
        EditorAttribGroup mountPointAttribGroup, countryAttribGroup, provinceAttribGroup, cityAttribGroup;
        bool zoomed;
        Vector3 zoomOldValue;
        Quaternion frontFaceQuaternion;
        LABELS_QUALITY labelsOldQuality;
        StringBuilder sb = new StringBuilder();

        WorldMapGlobe _map { get { return _editor.map; } }


        #region Inspector lifecycle

        void OnEnable() {

            // Setup basic inspector stuff
            _editor = (WorldMapEditor)target;
            if (_map.countries == null) {
                _map.Init();
            }

            if (Application.isPlaying)
                return;

            // Load UI icons
            Texture2D[] icons = new Texture2D[20];
            icons[0] = Resources.Load<Texture2D>("IconSelect");
            icons[1] = Resources.Load<Texture2D>("IconPolygon");
            icons[2] = Resources.Load<Texture2D>("IconUndo");
            icons[3] = Resources.Load<Texture2D>("IconConfirm");
            icons[4] = Resources.Load<Texture2D>("IconPoint");
            icons[5] = Resources.Load<Texture2D>("IconCircle");
            icons[6] = Resources.Load<Texture2D>("IconMagnet");
            icons[7] = Resources.Load<Texture2D>("IconSplitVert");
            icons[8] = Resources.Load<Texture2D>("IconSplitHoriz");
            icons[9] = Resources.Load<Texture2D>("IconDelete");
            icons[10] = Resources.Load<Texture2D>("IconEraser");
            icons[11] = Resources.Load<Texture2D>("IconMorePoints");
            icons[12] = Resources.Load<Texture2D>("IconCreate");
            icons[13] = Resources.Load<Texture2D>("IconPenCountry");
            icons[14] = Resources.Load<Texture2D>("IconTarget");
            icons[15] = Resources.Load<Texture2D>("IconPenCountryRegion");
            icons[16] = Resources.Load<Texture2D>("IconPenProvince");
            icons[17] = Resources.Load<Texture2D>("IconPenProvinceRegion");
            icons[18] = Resources.Load<Texture2D>("IconMove");
            icons[19] = Resources.Load<Texture2D>("IconMountPoint");

            // Setup main toolbar
            mainToolbarIcons = new GUIContent[5];
            mainToolbarIcons[0] = new GUIContent("Select", icons[0], "Selection mode");
            mainToolbarIcons[1] = new GUIContent("Reshape", icons[1], "Change the shape of this entity");
            mainToolbarIcons[2] = new GUIContent("Create", icons[12], "Add a new entity to this layer");
            mainToolbarIcons[3] = new GUIContent("Revert", icons[2], "Restore shape information");
            mainToolbarIcons[4] = new GUIContent("Save", icons[3], "Confirm changes and save to file");

            // Setup reshape region command toolbar
            int RESHAPE_REGION_TOOLS_COUNT = 8;
            reshapeRegionToolbarIcons = new GUIContent[RESHAPE_REGION_TOOLS_COUNT];
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.POINT] = new GUIContent("Point", icons[4], "Single Point Tool");
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.CIRCLE] = new GUIContent("Circle", icons[5], "Group Move Tool");
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.SPLITV] = new GUIContent("SplitV", icons[7], "Split Vertically");
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.SPLITH] = new GUIContent("SplitH", icons[8], "Split Horizontally");
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.MAGNET] = new GUIContent("Magnet", icons[6], "Join frontiers between different regions");
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.SMOOTH] = new GUIContent("Smooth", icons[11], "Add Point Tool");
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.ERASER] = new GUIContent("Erase", icons[10], "Removes Point Tool");
            reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.DELETE] = new GUIContent("Delete", icons[9], "Delete Region or Country");
            reshapeRegionModeExplanation = new string[RESHAPE_REGION_TOOLS_COUNT];
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.POINT] = "Drag a SINGLE point of currently selected region (and its neighbour)";
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.CIRCLE] = "Drag a GROUP of points of currently selected region (and from its neighbour region if present)";
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.SPLITV] = "Splits VERTICALLY currently selected region. One of the two splitted parts will form a new country.";
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.SPLITH] = "Splits HORIZONTALLY currently selected region. One of the two splitted parts will form a new country.";
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.MAGNET] = "Click several times on a group of points next to a neighbour frontier to makes them JOIN. You may need to add additional points on both sides using the smooth tool.";
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.SMOOTH] = "Click around currently selected region to ADD new points.";
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.ERASER] = "Click on target points of currently selected region to ERASE them.";
            reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.DELETE] = "DELETES currently selected region. If this is the last region of the country or province, then the country or province will be deleted completely.";

            // Setup create command toolbar
            int CREATE_TOOLS_COUNT = 6;
            createToolbarIcons = new GUIContent[CREATE_TOOLS_COUNT];
            createToolbarIcons[(int)CREATE_TOOL.CITY] = new GUIContent("City", icons[14], "Create a new city");
            createToolbarIcons[(int)CREATE_TOOL.COUNTRY] = new GUIContent("Country", icons[13], "Draw a new country");
            createToolbarIcons[(int)CREATE_TOOL.COUNTRY_REGION] = new GUIContent("Co. Region", icons[15], "Draw a new region for current selected country");
            createToolbarIcons[(int)CREATE_TOOL.PROVINCE] = new GUIContent("Province", icons[16], "Draw a new province for current selected country");
            createToolbarIcons[(int)CREATE_TOOL.PROVINCE_REGION] = new GUIContent("Prov. Region", icons[17], "Draw a new region for current selected province");
            createToolbarIcons[(int)CREATE_TOOL.MOUNT_POINT] = new GUIContent("Mount Point", icons[19], "Create a new mount point");
            createModeExplanation = new string[CREATE_TOOLS_COUNT];
            createModeExplanation[(int)CREATE_TOOL.CITY] = "Click over the map in SceneView\nto create a NEW CITY for currrent COUNTRY";
            createModeExplanation[(int)CREATE_TOOL.COUNTRY] = "Click over the map in SceneView\nto create a polygon and add points for a NEW COUNTRY";
            createModeExplanation[(int)CREATE_TOOL.COUNTRY_REGION] = "Click over the map in SceneView\nto create a polygon and add points for a NEW REGION of currently selected COUNTRY";
            createModeExplanation[(int)CREATE_TOOL.PROVINCE] = "Click over the map in SceneView\nto create a polygon and add points for a NEW PROVINCE of currently selected country";
            createModeExplanation[(int)CREATE_TOOL.PROVINCE_REGION] = "Click over the map in SceneView\nto create a polygon and add points for a NEW REGION of currently selected PROVINCE";
            createModeExplanation[(int)CREATE_TOOL.MOUNT_POINT] = "Click over the map in SceneView\nto create a NEW MOUNT POINT for current COUNTRY and optional PROVINCE";

            // Setup reshape city tools
            int RESHAPE_CITY_TOOLS_COUNT = 2;
            reshapeCityToolbarIcons = new GUIContent[RESHAPE_CITY_TOOLS_COUNT];
            reshapeCityToolbarIcons[(int)RESHAPE_CITY_TOOL.MOVE] = new GUIContent("Move", icons[18], "Move city");
            reshapeCityToolbarIcons[(int)RESHAPE_CITY_TOOL.DELETE] = new GUIContent("Delete", icons[9], "Delete city");
            reshapeCityModeExplanation = new string[RESHAPE_CITY_TOOLS_COUNT];
            reshapeCityModeExplanation[(int)RESHAPE_CITY_TOOL.MOVE] = "Click and drag currently selected CITY to change its POSITION";
            reshapeCityModeExplanation[(int)RESHAPE_CITY_TOOL.DELETE] = "DELETES currently selected CITY.";

            // Setup reshape mount point tools
            int RESHAPE_MOUNT_POINT_TOOLS_COUNT = 2;
            reshapeMountPointToolbarIcons = new GUIContent[RESHAPE_MOUNT_POINT_TOOLS_COUNT];
            reshapeMountPointToolbarIcons[(int)RESHAPE_MOUNT_POINT_TOOL.MOVE] = new GUIContent("Move", icons[18], "Move mount point");
            reshapeMountPointToolbarIcons[(int)RESHAPE_MOUNT_POINT_TOOL.DELETE] = new GUIContent("Delete", icons[9], "Delete mount point");
            reshapeMountPointModeExplanation = new string[RESHAPE_MOUNT_POINT_TOOLS_COUNT];
            reshapeMountPointModeExplanation[(int)RESHAPE_MOUNT_POINT_TOOL.MOVE] = "Click and drag currently selected MOUNT POINT to change its POSITION";
            reshapeMountPointModeExplanation[(int)RESHAPE_MOUNT_POINT_TOOL.DELETE] = "DELETES currently selected MOUNT POINT.";


            editingModeOptions = new string[] {
                "Only Countries",
                "Countries + Provinces"
            };

            editingCountryFileOptions = new string[] {
                "High Definition Geodata File",
                "Low Definition Geodata File"
            };
            cityClassOptions = new string[] {
                "City",
                "Country Capital",
                "Region Capital"
            };
            cityClassValues = new int[] {
                (int)CITY_CLASS.CITY,
                (int)CITY_CLASS.COUNTRY_CAPITAL,
                (int)CITY_CLASS.REGION_CAPITAL
            };

            emptyStringArray = new string[0];

            // Setup scene view
            _editor.shouldHideEditorMesh = true;
            zoomed = _map.transform.localScale.x >= 1000.0f;
            if (zoomed) {
                zoomOldValue = Misc.Vector3one;
            } else {
                zoomOldValue = _map.transform.localScale;
            }
            labelsOldQuality = _map.labelsQuality;

            // Select globe and focus it
            if (Selection.activeGameObject != _map.gameObject) {
                Selection.activeGameObject = _map.gameObject;
                if (SceneView.lastActiveSceneView != null)
                    SceneView.lastActiveSceneView.FrameSelected();
            }
            // Update icons scale
            AdjustCityIconsScale();
            AdjustMountPointIconsScale();
            // Hint about changing scales
            CheckScale();

#if UNITY_2019_1_OR_NEWER
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv != null) {
                sv.drawGizmos = true;
            }
#endif
        }

        public void OnCloseEditor() {
            // Disables zoom
            if (zoomed) {
                DisableZoom();
                _map.ScaleCities();
                _map.ScaleMountPoints();
            }
        }

        public override void OnInspectorGUI() {
            if (_editor == null)
                return;
            if (_map.showProvinces) {
                _editor.editingMode = EDITING_MODE.PROVINCES;
            } else {
                _editor.editingMode = EDITING_MODE.COUNTRIES;
                if (_map.frontiersDetail == FRONTIERS_DETAIL.High) {
                    _editor.editingCountryFile = EDITING_COUNTRY_FILE.COUNTRY_HIGHDEF;
                } else {
                    _editor.editingCountryFile = EDITING_COUNTRY_FILE.COUNTRY_LOWDEF;
                }
            }

            CheckEditorStyles();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            if (Application.isPlaying) {
                EditorGUILayout.BeginHorizontal();
                DrawWarningLabel("Map Editor not available at runtime");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawWarningLabel("Map Editor");
            GUILayout.FlexibleSpace();
            if (_map.transform.localScale.x >= 1000)
                DrawWarningLabel("Zoom: ON");
            else
                DrawWarningLabel("Zoom: OFF");
            if (GUILayout.Button("Toggle Zoom"))
                ToggleZoom();
            if (GUILayout.Button("Redraw")) {
                _editor.RedrawAll();
                CheckHideEditorMesh();
            }
            if (GUILayout.Button("Help"))
                EditorUtility.DisplayDialog("World Map Editor", "This editor component allows you to modify the borders of the map, and also perform some operations with provinces, countries and cities, like creating and merging.\n\nRemember that the map editor works on the Scene View and not in the Game View. Please read the documentation included for general instructions about the editor and this asset. For questions and support, visit kronnect.com", "Ok");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Show Layers", GUILayout.Width(90));
            EDITING_MODE prevEditingMode = _editor.editingMode;
            _editor.editingMode = (EDITING_MODE)EditorGUILayout.Popup((int)_editor.editingMode, editingModeOptions);
            if (_editor.editingMode != prevEditingMode) {
                ChangeEditingMode(_editor.editingMode);
                if (_editor.editingMode == EDITING_MODE.PROVINCES && _map.frontiersDetail == FRONTIERS_DETAIL.Low) {
                    if (!EditorUtility.DisplayDialog("Switch Geodata File?", "Do you want to switch to high definition country frontiers? (Recommended when editing provinces).\n\nNote: any unsaved change will be lost?", "Yes", "Cancel")) {
                        return;
                    }
                    _editor.editingCountryFile = EDITING_COUNTRY_FILE.COUNTRY_HIGHDEF;
                    SwitchEditingFrontiersFile();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Country File", GUILayout.Width(90));
            EDITING_COUNTRY_FILE prevCountryFile = _editor.editingCountryFile;
            _editor.editingCountryFile = (EDITING_COUNTRY_FILE)EditorGUILayout.Popup((int)_editor.editingCountryFile, editingCountryFileOptions);
            if (_editor.editingCountryFile != prevCountryFile) {
                if (!EditorUtility.DisplayDialog("Switch Geodata File", "Choosing a different country file will reload definitions and any unsaved change to current file will be lost. Continue?", "Switch Geodata File", "Cancel")) {
                    _editor.editingCountryFile = prevCountryFile;
                    CheckScale();
                    return;
                }
                SwitchEditingFrontiersFile();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Show Cities", GUILayout.Width(90));
            bool prevShowCities = _editor.map.showCities;
            _editor.map.showCities = EditorGUILayout.Toggle(_editor.map.showCities, GUILayout.Width(20));
            if (_editor.map.showCities != prevShowCities && _editor.map.showCities) {
                _map.minPopulation = 0; // make sure all cities are visible
                AdjustCityIconsScale();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            ShowEntitySelectors();

            EditorGUILayout.BeginVertical();

            // main toolbar
            GUIStyle toolbarStyle = new GUIStyle(GUI.skin.button);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            OPERATION_MODE prevOp = _editor.operationMode;
            _editor.operationMode = (OPERATION_MODE)GUILayout.SelectionGrid((int)_editor.operationMode, mainToolbarIcons, 3, toolbarStyle, GUILayout.Height(48), GUILayout.MaxWidth(310));
            if (prevOp != _editor.operationMode) {
                NewShapeInit();
                ProcessOperationMode();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_editor.infoMsg.Length > 0) {
                if (Event.current.type == EventType.Layout && (DateTime.Now - _editor.infoMsgStartTime).TotalSeconds > 5) {
                    _editor.infoMsg = "";
                } else {
                    EditorGUILayout.HelpBox(_editor.infoMsg, MessageType.Info);
                }
            }
            EditorGUILayout.Separator();
            switch (_editor.operationMode) {
                case OPERATION_MODE.UNDO:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    DrawWarningLabel("Discard current changes?");
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Discard", GUILayout.Width(80))) {
                        _editor.DiscardChanges();
                        _editor.operationMode = OPERATION_MODE.SELECTION;
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                        _editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Separator();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical();
                    break;
                case OPERATION_MODE.CONFIRM:
                    if (_editor.countryChanges) {
                        DrawCenteredLabel("There're pending country modifications.");
                    }
                    if (_editor.countryAttribChanges) {
                        DrawCenteredLabel("There're pending country attribute modifications.");
                    }
                    if (_editor.provinceChanges) {
                        DrawCenteredLabel("There're pending province modifications.");
                    }
                    if (_editor.provinceAttribChanges) {
                        DrawCenteredLabel("There're pending province attributes modifications.");
                    }
                    if (_editor.cityChanges) {
                        DrawCenteredLabel("There're pending city modifications.");
                    }
                    if (_editor.cityAttribChanges) {
                        DrawCenteredLabel("There're pending city attributes modifications.");
                    }
                    if (_editor.mountPointChanges) {
                        DrawCenteredLabel("There're pending mount point modifications.");
                    }
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    DrawWarningLabel("Save changes?");
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Save", GUILayout.Width(80))) {
                        if (SaveChanges()) {
                            _editor.SetInfoMsg(INFO_MSG_CHANGES_SAVED);
                        } else {
                            _editor.SetInfoMsg(INFO_MSG_NO_CHANGES_TO_SAVE);
                        }
                        _editor.operationMode = OPERATION_MODE.SELECTION;
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                        _editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Separator();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical();
                    break;
                case OPERATION_MODE.RESHAPE:
                    if (_editor.countryIndex < 0 && _editor.cityIndex < 0) {
                        DrawWarningLabel("No country, province nor city selected.");
                        break;
                    }

                    if (_editor.countryIndex >= 0) {
                        ShowReshapingRegionTools();
                    }
                    if (_editor.cityIndex >= 0) {
                        ShowReshapingCityTools();
                    }
                    if (_editor.mountPointIndex >= 0) {
                        ShowReshapingMountPointTools();
                    }
                    break;
                case OPERATION_MODE.CREATE:
                    ShowCreateTools();
                    break;
            }

            EditorGUILayout.Separator();

            EditorGUILayout.EndVertical();

            CheckHideEditorMesh();
        }

        void OnSceneGUI() {
            if (Application.isPlaying)
                return;
            CheckEditorStyles();
            ProcessOperationMode();
        }

        Camera GetSceneViewCamera() {
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv != null)
                return sv.camera;
            return null;
        }

        void ToggleZoom() {
            Camera cam = GetSceneViewCamera();
            if (cam == null) {
                EditorUtility.DisplayDialog("Ops!", "Could not get a reference to the camera in the scene view. Try selecting the Scene View first before using this command. Alternatively you can modify the globe scale and adjust the camera distance manually.", "Ok");
                return;
            }
            if (zoomed) {
                DisableZoom();
            } else {
                zoomed = true;
                _map.transform.localScale = Misc.Vector3one * 1000.0f;
                RepositionOverlay();
                _map.labelsQuality = LABELS_QUALITY.High;
                _map.Redraw();
                SceneView.lastActiveSceneView.LookAtDirect(_map.transform.position - cam.transform.forward * _map.transform.localScale.x, cam.transform.rotation, 2f);
            }
            _editor.ClearSelection();
            _editor.RedrawFrontiers();
            _editor.shouldHideEditorMesh = true;

        }

        void DisableZoom() {
            _map.transform.localScale = zoomOldValue;
            RepositionOverlay();
            _map.labelsQuality = labelsOldQuality;
            _map.Redraw();
            if (SceneView.lastActiveSceneView != null) {
                SceneView.lastActiveSceneView.FrameSelected();
            }
            zoomed = false;
        }

        void RepositionOverlay() {
            _map.GetOverlayLayer(false, false).transform.position = new Vector3(_map.transform.position.x + 5000f, 5000f, 0); // since it's already displaced, applying a zoom of 1000 will throw it into problematic coordinates due to floating point limitations
        }

        void ChangeEditingMode(EDITING_MODE newMode) {
            _editor.editingMode = newMode;
            // Ensure file is loaded by the map
            switch (_editor.editingMode) {
                case EDITING_MODE.COUNTRIES:
                    _map.showFrontiers = true;
                    _map.showProvinces = false;
                    _map.HideProvinces();
                    break;
                case EDITING_MODE.PROVINCES:
                    _map.showProvinces = true;
                    break;
            }
        }

        void ShowEntitySelectors() {

            // preprocesssing logic first to not interfere with layout and repaint events
            string[] provinceNames, countryNames = _editor.countryNames, countryNeighboursNames = _editor.countryNeighboursNames, provinceCountriesNeighboursNames;
            string[] cityNames = _editor.cityNames, mountPointNames = _editor.mountPointNames;
            if (_editor.editingMode != EDITING_MODE.PROVINCES) {
                provinceNames = emptyStringArray;
                provinceCountriesNeighboursNames = emptyStringArray;
            } else {
                provinceNames = _editor.provinceNames;
                if (provinceNames == null)
                    provinceNames = emptyStringArray;
                provinceCountriesNeighboursNames = _editor.provinceNeighbourCountriesNames;
                if (provinceCountriesNeighboursNames == null)
                    provinceCountriesNeighboursNames = emptyStringArray;
            }
            if (mountPointNames == null)
                mountPointNames = emptyStringArray;

            EditorGUILayout.BeginVertical();
            // country selector
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Country", GUILayout.Width(90));
            int selection = EditorGUILayout.Popup(_editor.GUICountryIndex, countryNames);
            if (selection != _editor.GUICountryIndex) {
                _editor.CountrySelectByCombo(selection);
                if (_editor.countryIndex >= 0 && _editor.countryRegionIndex >= 0)
                    FocusSpherePoint(_map.countries[_editor.countryIndex].regions[_editor.countryRegionIndex].sphereCenter);
                GUIUtility.ExitGUI();
                return;
            }
            bool prevc = _editor.groupByParentAdmin;
            GUILayout.Label("Grouped");
            _editor.groupByParentAdmin = EditorGUILayout.Toggle(_editor.groupByParentAdmin, GUILayout.Width(20));
            if (_editor.groupByParentAdmin != prevc) {
                _editor.ReloadCountryNames();
            }
            EditorGUILayout.EndHorizontal();
            if (_editor.countryIndex >= 0) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   Name", GUILayout.Width(90));
                _editor.GUICountryNewName = EditorGUILayout.TextField(_editor.GUICountryNewName);
                if (GUILayout.Button("Update")) {
                    _editor.CountryRename();
                }
                if (GUILayout.Button("Sanitize")) {
                    if (EditorUtility.DisplayDialog("Sanitize Frontiers", "This option detects polygon issues (like self-crossing polygon) and fix them. Only use if you encounter some problem with the shape of this country.\n\nContinue?", "Ok", "Cancel")) {
                        _editor.CountrySanitize();
                        _editor.CountryRegionSelect();
                    }
                }
                if (GUILayout.Button("Delete")) {
                    if (EditorUtility.DisplayDialog("Delete Country", "This option will completely delete current country and all its dependencies (cities, provinces, mount points, ...)\n\nContinue?", "Yes", "No")) {
                        _editor.CountryDelete();
                        _editor.SetInfoMsg(INFO_MSG_COUNTRY_DELETED);
                        _editor.operationMode = OPERATION_MODE.SELECTION;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   Hidden", GUILayout.Width(90));
                _editor.GUICountryHidden = EditorGUILayout.Toggle(_editor.GUICountryHidden);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   Sovereign", GUILayout.Width(90));
                _editor.GUICountryTransferToCountryIndex = EditorGUILayout.Popup(_editor.GUICountryTransferToCountryIndex, countryNeighboursNames);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(90));
                if (GUILayout.Button("Transfer Country Region")) {
                    if (_editor.GUICountryIndex != _editor.GUICountryTransferToCountryIndex) {
                        string sourceCountry = countryNames[_editor.GUICountryIndex].Trim();
                        string targetCountry = countryNeighboursNames[_editor.GUICountryTransferToCountryIndex].Trim();
                        if (EditorUtility.DisplayDialog("Change Country's Sovereignty", "Current country " + sourceCountry + " land will join target country " + targetCountry + ".\n\nAre you sure (can take some time on big countries)?", "Ok", "Cancel")) {
                            _editor.CountryTransferTo();
                            _editor.operationMode = OPERATION_MODE.SELECTION;
                        }
                    } else {
                        EditorUtility.DisplayDialog("Invalid destination", "Can't transfer to itself.", "Ok");
                    }
                }
                if (_editor.editingMode == EDITING_MODE.PROVINCES) {
                    if (GUILayout.Button("Transfer As Province")) {
                        if (_editor.GUICountryIndex != _editor.GUICountryTransferToCountryIndex) {
                            string sourceCountry = countryNames[_editor.GUICountryIndex].Trim();
                            string targetCountry = countryNeighboursNames[_editor.GUICountryTransferToCountryIndex].Trim();
                            if (EditorUtility.DisplayDialog("Change Country's Sovereignty", "Current country " + sourceCountry + " will be converted to a province and will join target country " + targetCountry + ".\n\nAre you sure (can take some time on big countries)?", "Ok", "Cancel")) {
                                _editor.CountryTransferAsProvinceTo();
                                _editor.operationMode = OPERATION_MODE.SELECTION;
                            }
                        } else {
                            EditorUtility.DisplayDialog("Invalid destination", "Can't transfer to itself.", "Ok");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   Continent", GUILayout.Width(90));
                _editor.GUICountryNewContinent = EditorGUILayout.TextField(_editor.GUICountryNewContinent);
                GUI.enabled = _editor.countryIndex >= 0;
                if (GUILayout.Button("Update")) {
                    _editor.CountryChangeContinent();
                }
                if (GUILayout.Button("Rename")) {
                    if (EditorUtility.DisplayDialog("Continent Renaming", "This option will rename the continent affecting to all countries in same continent. Continue?", "Yes", "No")) {
                        _editor.ContinentRename();
                    }
                }
                if (GUILayout.Button("Delete")) {
                    if (EditorUtility.DisplayDialog("Delete all countries (in same continent)", "You're going to delete all countries and provinces in continent " + _map.countries[_editor.countryIndex].continent + ".\n\nAre you sure?", "Yes", "No")) {
                        _editor.CountryDeleteSameContinent();
                        _editor.SetInfoMsg(INFO_MSG_CONTINENT_DELETED);
                        _editor.operationMode = OPERATION_MODE.SELECTION;
                    }
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   FIPS 10 4", GUILayout.Width(90));
                _editor.GUICountryNewFIPS10_4 = EditorGUILayout.TextField(_editor.GUICountryNewFIPS10_4, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   ISO A2", GUILayout.Width(90));
                _editor.GUICountryNewISO_A2 = EditorGUILayout.TextField(_editor.GUICountryNewISO_A2, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   ISO A3", GUILayout.Width(90));
                _editor.GUICountryNewISO_A3 = EditorGUILayout.TextField(_editor.GUICountryNewISO_A3, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   ISO N3", GUILayout.Width(90));
                _editor.GUICountryNewISO_N3 = EditorGUILayout.TextField(_editor.GUICountryNewISO_N3, GUILayout.Width(60));
                GUI.enabled = _editor.countryIndex >= 0;
                if (GUILayout.Button("Update FIPS/ISO Codes")) {
                    _editor.CountryChangeFIPSAndISOCodes();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // Country attributes
                if (_editor.countryIndex >= 0 && _editor.countryIndex < _map.countries.Length) {
                    Country country = _map.countries[_editor.countryIndex];
                    if (countryAttribGroup == null) {
                        countryAttribGroup = new EditorAttribGroup();
                    }
                    if (countryAttribGroup.itemGroup != country) {
                        countryAttribGroup.SetItemGroup(country);
                    }
                    if (ShowAttributeGroup(countryAttribGroup, "Country Attributes")) {
                        _editor.countryAttribChanges = true;
                        GUIUtility.ExitGUI();
                        return;
                    }

                    // Country Regions
                    if (ShowRegionsGroup(country, _editor.countryRegionIndex)) {
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
            }

            if (_editor.editingMode == EDITING_MODE.PROVINCES && _editor.countryIndex >= 0) {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Province/State", GUILayout.Width(90));
                int provSelection = EditorGUILayout.Popup(_editor.GUIProvinceIndex, provinceNames);
                if (provSelection != _editor.GUIProvinceIndex) {
                    _editor.ProvinceSelectByCombo(provSelection);
                    if (_editor.provinceIndex >= 0 && _editor.provinceRegionIndex >= 0)
                        FocusSpherePoint(_map.provinces[_editor.provinceIndex].regions[_editor.provinceRegionIndex].sphereCenter);
                }
                EditorGUILayout.EndHorizontal();
                if (_editor.provinceIndex >= 0) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Name", GUILayout.Width(90));
                    _editor.GUIProvinceNewName = EditorGUILayout.TextField(_editor.GUIProvinceNewName);
                    if (GUILayout.Button("Update")) {
                        _editor.ProvinceRename();
                    }
                    if (GUILayout.Button("Sanitize")) {
                        if (EditorUtility.DisplayDialog("Sanitize Borders", "This option detects polygon issues (like self-crossing polygon) and fix them. Only use if you encounter some problem with the shape of this province.\n\nContinue?", "Ok", "Cancel")) {
                            _editor.ProvinceSanitize();
                            _editor.ProvinceRegionSelect();
                        }
                    }
                    if (GUILayout.Button("Delete")) {
                        if (EditorUtility.DisplayDialog("Delete Province", "This option will completely delete current province.\n\nContinue?", "Yes", "No")) {
                            _editor.ProvinceDelete();
                            _editor.SetInfoMsg(INFO_MSG_PROVINCE_DELETED);
                            _editor.operationMode = OPERATION_MODE.SELECTION;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Sovereign", GUILayout.Width(90));
                    _editor.GUIProvinceTransferToCountryIndex = EditorGUILayout.Popup(_editor.GUIProvinceTransferToCountryIndex, provinceCountriesNeighboursNames);
                    if (GUILayout.Button("Transfer")) {
                        if (_editor.GUIProvinceIndex != _editor.GUIProvinceTransferToCountryIndex) {
                            string sourceProvince = provinceNames[_editor.GUIProvinceIndex].Trim();
                            string targetCountry = provinceCountriesNeighboursNames[_editor.GUIProvinceTransferToCountryIndex].Trim();
                            if (_editor.editingCountryFile == EDITING_COUNTRY_FILE.COUNTRY_LOWDEF) {
                                EditorUtility.DisplayDialog("Change Province's Sovereignty", "This command is only available with High-Definition Country File selected.", "Ok");
                            } else if (EditorUtility.DisplayDialog("Change Province's Sovereignty", "Current province " + sourceProvince + " will join target country " + targetCountry + ".\n\nAre you sure?", "Ok", "Cancel")) {
                                _editor.ProvinceTransferTo();
                                _editor.operationMode = OPERATION_MODE.SELECTION;
                            }
                        } else {
                            EditorUtility.DisplayDialog("Invalid destination", "Can't transfer to itself.", "Ok");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Other Prov.", GUILayout.Width(90));
                    _editor.GUIProvinceMergeWithIndex = EditorGUILayout.Popup(_editor.GUIProvinceMergeWithIndex, provinceNames);
                    if (GUILayout.Button("Merge")) {
                        if (_editor.GUIProvinceIndex != _editor.GUIProvinceMergeWithIndex) {
                            string sourceProvince = provinceNames[_editor.GUIProvinceIndex].Trim();
                            string targetProvince = provinceNames[_editor.GUIProvinceMergeWithIndex].Trim();
                            if (EditorUtility.DisplayDialog("Merge Provinces", "Current province " + sourceProvince + " will be merged into province " + targetProvince + ".\n\nAre you sure?", "Ok", "Cancel")) {
                                _editor.ProvinceMerge();
                                _editor.operationMode = OPERATION_MODE.SELECTION;
                            }
                        } else {
                            EditorUtility.DisplayDialog("Invalid destination", "Can't merge with itself.", "Ok");
                        }
                    }
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   New Country", GUILayout.Width(90));
                    _editor.GUIProvinceToNewCountryName = EditorGUILayout.TextField(_editor.GUIProvinceToNewCountryName);
                    if (string.IsNullOrEmpty(_editor.GUIProvinceToNewCountryName))
                        GUI.enabled = false;
                    if (GUILayout.Button("Create")) {
                        if (EditorUtility.DisplayDialog("Convert Province Into a Country", "This command will extract current province " + _editor.GUIProvinceName + " from its country " + _editor.GUICountryName + " and create a new country named " + _editor.GUIProvinceToNewCountryName + ".\n\nContinue?", "Yes", "No")) {
                            _editor.ProvinceToNewCountry();
                        }
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(" ", GUILayout.Width(90));
                if (GUILayout.Button("Delete All Country Provinces", GUILayout.Width(180))) {
                    if (EditorUtility.DisplayDialog("Delete All Country Provinces", "This option will delete all provinces of current country.\n\nContinue?", "Yes", "No")) {
                        _editor.DeleteCountryProvinces();
                        _editor.SetInfoMsg(INFO_MSG_PROVINCE_DELETED);
                        _editor.operationMode = OPERATION_MODE.SELECTION;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Province attributes
                if (_editor.provinceIndex >= 0) {
                    Province province = _map.provinces[_editor.provinceIndex];
                    if (provinceAttribGroup == null) {
                        provinceAttribGroup = new EditorAttribGroup();
                    }
                    if (provinceAttribGroup.itemGroup != province) {
                        provinceAttribGroup.SetItemGroup(province);
                    }
                    if (ShowAttributeGroup(provinceAttribGroup, "Province Attributes")) {
                        _editor.provinceAttribChanges = true;
                        GUIUtility.ExitGUI();
                        return;
                    }
                    // Province Regions
                    if (ShowRegionsGroup(province, _editor.provinceRegionIndex)) {
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
            }

            if (_editor.countryIndex >= 0) {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("City", GUILayout.Width(90));
                int citySelection = EditorGUILayout.Popup(_editor.GUICityIndex, cityNames);
                if (citySelection != _editor.GUICityIndex) {
                    _editor.CitySelectByCombo(citySelection);
                    if (_editor.cityIndex >= 0)
                        FocusSpherePoint(_map.cities[_editor.cityIndex].localPosition);
                }
                EditorGUILayout.EndHorizontal();
                if (_editor.cityIndex >= 0) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Name", GUILayout.Width(90));
                    _editor.GUICityNewName = EditorGUILayout.TextField(_editor.GUICityNewName);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Class", GUILayout.Width(90));
                    _editor.GUICityClass = (CITY_CLASS)EditorGUILayout.IntPopup((int)_editor.GUICityClass, cityClassOptions, cityClassValues);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Population", GUILayout.Width(90));
                    _editor.GUICityPopulation = EditorGUILayout.TextField(_editor.GUICityPopulation);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Province", GUILayout.Width(90));
                    _editor.GUICityProvince = EditorGUILayout.TextField(_editor.GUICityProvince);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Lat/Lon", GUILayout.Width(90));
                    _editor.GUICityLatLon = EditorGUILayout.Vector2Field("", _editor.GUICityLatLon);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("");
                    if (GUILayout.Button("Update City")) {
                        UndoPushCityStartOperation("Undo City Change");
                        _editor.CityUpdate();
                        UndoPushCityEndOperation();
                    }
                    EditorGUILayout.EndHorizontal();


                    // City attributes
                    City city = _map.cities[_editor.cityIndex];
                    if (cityAttribGroup == null) {
                        cityAttribGroup = new EditorAttribGroup();
                    }
                    if (cityAttribGroup.itemGroup != city) {
                        cityAttribGroup.SetItemGroup(city);
                    }
                    if (ShowAttributeGroup(cityAttribGroup, "City Attributes")) {
                        _editor.cityAttribChanges = true;
                        GUIUtility.ExitGUI();
                        return;
                    }

                }
            }

            if (_editor.countryIndex >= 0) {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Mount Point", GUILayout.Width(90));
                int mpSelection = EditorGUILayout.Popup(_editor.GUIMountPointIndex, mountPointNames);
                if (mpSelection != _editor.GUIMountPointIndex) {
                    _editor.MountPointSelectByCombo(mpSelection);
                    if (_editor.mountPointIndex >= 0)
                        FocusSpherePoint(_map.mountPoints[_editor.mountPointIndex].localPosition);
                }
                EditorGUILayout.EndHorizontal();
                if (_editor.mountPointIndex >= 0) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Name", GUILayout.Width(90));
                    _editor.GUIMountPointNewName = EditorGUILayout.TextField(_editor.GUIMountPointNewName);
                    if (GUILayout.Button("Update")) {
                        UndoPushMountPointStartOperation("Undo Rename Mount Point");
                        _editor.MountPointRename();
                        UndoPushMountPointEndOperation();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Type", GUILayout.Width(90));
                    _editor.GUIMountPointNewType = EditorGUILayout.TextField(_editor.GUIMountPointNewType);
                    if (GUILayout.Button("Update")) {
                        UndoPushMountPointStartOperation("Undo Change Mount Point Type");
                        _editor.MountPointUpdateType();
                        UndoPushMountPointEndOperation();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Mount point attributes
                    MountPoint mp = _map.mountPoints[_editor.mountPointIndex];
                    if (mountPointAttribGroup == null) {
                        mountPointAttribGroup = new EditorAttribGroup();
                    }
                    if (mountPointAttribGroup.itemGroup != mp) {
                        mountPointAttribGroup.SetItemGroup(mp);
                    }
                    if (ShowAttributeGroup(mountPointAttribGroup, "Mount Point Attributes")) {
                        _editor.mountPointChanges = true;
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
        }

        /// <summary>
        /// Returns true if there're changes
        /// </summary>
        bool ShowRegionsGroup(IAdminEntity entity, int currentSelectedRegionIndex) {

            EditorGUILayout.BeginHorizontal();
            entity.foldOut = EditorGUILayout.Foldout(entity.foldOut, "Regions", attribHeaderStyle);
            EditorGUILayout.EndHorizontal();
            if (!entity.foldOut)
                return false;

            int regionCount = entity.regions.Count;
            for (int k = 0; k < regionCount; k++) {
                EditorGUILayout.BeginHorizontal();
                sb.Length = 0;
                sb.Append("    ");
                sb.Append(k.ToString());
                if (k == entity.mainRegionIndex)
                    sb.Append(" (Main)");
                if (currentSelectedRegionIndex == k)
                    sb.Append(" (Selected)");
                GUILayout.Label(sb.ToString(), GUILayout.Width(100));
                if (GUILayout.Button("Select", GUILayout.Width(60))) {
                    if (entity is Country) {
                        _editor.countryRegionIndex = k;
                        _editor.CountryRegionSelect();
                    } else {
                        _editor.provinceRegionIndex = k;
                        _editor.ProvinceRegionSelect();
                    }
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Remove", GUILayout.Width(60))) {
                    if (EditorUtility.DisplayDialog("Remove Region", "Are you sure you want to remove this region?", "Ok", "Cancel")) {
                        if (entity is Country) {
                            _editor.countryRegionIndex = k;
                            _editor.CountryRegionDelete();
                        } else {
                            _editor.provinceRegionIndex = k;
                            _editor.ProvinceRegionDelete();
                        }
                        _editor.ClearSelection();
                        GUIUtility.ExitGUI();
                        return true;
                    }
                }
                if (GUILayout.Button("New Country", GUILayout.Width(90))) {
                    if (EditorUtility.DisplayDialog("New Country From Region", "Are you sure you want to create a new country based on this region (note: any contained province will also be moved to the new country)?", "Ok", "Cancel")) {
                        _editor.CountryCreate(entity.regions[k]);
                        _editor.ClearSelection();
                        GUIUtility.ExitGUI();
                        return true;
                    }
                }
                if (_editor.editingMode == EDITING_MODE.PROVINCES) {
                    if (GUILayout.Button("New Province", GUILayout.Width(90))) {
                        if (EditorUtility.DisplayDialog("New Province From Region", "Are you sure you want to create a new province based on this region (note: if a province already contains this region this process will be cancelled)?", "Ok", "Cancel")) {
                            _editor.ProvinceCreate(entity.regions[k]);
                            GUIUtility.ExitGUI();
                            return true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            return false;
        }



        void ShowReshapingRegionTools() {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawWarningLabel("REGION MODIFYING TOOLS");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            RESHAPE_REGION_TOOL prevTool = _editor.reshapeRegionMode;
            int selectionGridRows = (reshapeRegionToolbarIcons.Length - 1) / 4 + 1;
            GUIStyle selectionGridStyle = new GUIStyle(GUI.skin.button);
            selectionGridStyle.margin = new RectOffset(2, 2, 2, 2);
            _editor.reshapeRegionMode = (RESHAPE_REGION_TOOL)GUILayout.SelectionGrid((int)_editor.reshapeRegionMode, reshapeRegionToolbarIcons, 4, selectionGridStyle, GUILayout.Height(24 * selectionGridRows), GUILayout.MaxWidth(300));
            if (_editor.reshapeRegionMode != prevTool) {
                if (_editor.countryIndex >= 0) {
                    tickStart = DateTime.Now.Ticks;
                }
                ProcessOperationMode();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUIStyle explanationStyle = new GUIStyle(GUI.skin.box);
            explanationStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.32f, 0.33f, 0.6f);
            GUILayout.Box(reshapeRegionModeExplanation[(int)_editor.reshapeRegionMode], explanationStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.Label("Region Rectangle Info");
            GUILayout.Label("Latitude from " + _editor.regionRectLatLon.xMin + " to " + _editor.regionRectLatLon.xMax);
            GUILayout.Label("Longitude from " + _editor.regionRectLatLon.yMin + " to " + _editor.regionRectLatLon.yMax);

            EditorGUILayout.Separator();

            if (_editor.reshapeRegionMode.hasCircle()) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Circle Width", GUILayout.Width(90));
                _editor.reshapeCircleWidth = EditorGUILayout.Slider(_editor.reshapeCircleWidth, 0.0001f, 0.2f);
                EditorGUILayout.EndHorizontal();
            }

            if (_editor.reshapeRegionMode == RESHAPE_REGION_TOOL.POINT || _editor.reshapeRegionMode == RESHAPE_REGION_TOOL.CIRCLE || _editor.reshapeRegionMode == RESHAPE_REGION_TOOL.ERASER) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Selected Region Only", GUILayout.Width(150));
                _editor.circleCurrentRegionOnly = EditorGUILayout.Toggle(_editor.circleCurrentRegionOnly, GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
            }

            switch (_editor.reshapeRegionMode) {
                case RESHAPE_REGION_TOOL.CIRCLE:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Constant Move", GUILayout.Width(150));
                    _editor.circleMoveConstant = EditorGUILayout.Toggle(_editor.circleMoveConstant, GUILayout.Width(20));
                    EditorGUILayout.EndHorizontal();
                    break;
                case RESHAPE_REGION_TOOL.MAGNET:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Agressive Mode", GUILayout.Width(150));
                    _editor.magnetAgressiveMode = EditorGUILayout.Toggle(_editor.magnetAgressiveMode, GUILayout.Width(20));
                    EditorGUILayout.EndHorizontal();
                    break;
                case RESHAPE_REGION_TOOL.SPLITV:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Split At", GUILayout.Width(150));
                    _editor.splitVerticallyAt = EditorGUILayout.Slider(_editor.splitVerticallyAt, _editor.regionRectLatLon.yMin, _editor.regionRectLatLon.yMax);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    DrawWarningLabel("Confirm split vertically?");
                    if (GUILayout.Button("Split", GUILayout.Width(80))) {
                        _editor.SplitVertically();
                        _editor.operationMode = OPERATION_MODE.SELECTION;
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                        _editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    break;
                case RESHAPE_REGION_TOOL.SPLITH:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Split At", GUILayout.Width(150));
                    _editor.splitHorizontallyAt = EditorGUILayout.Slider(_editor.splitHorizontallyAt, _editor.regionRectLatLon.xMin, _editor.regionRectLatLon.xMax);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    DrawWarningLabel("Confirm split horizontally?");
                    if (GUILayout.Button("Split", GUILayout.Width(80))) {
                        _editor.SplitHorizontally();
                        _editor.operationMode = OPERATION_MODE.SELECTION;
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                        _editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    break;
                case RESHAPE_REGION_TOOL.DELETE:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (_editor.entityIndex < 0) {
                        DrawWarningLabel("Select a region to delete.");
                    } else {
                        if (_editor.editingMode == EDITING_MODE.COUNTRIES) {
                            bool deletingRegion = _map.countries[_editor.countryIndex].regions.Count > 1;
                            if (deletingRegion) {
                                DrawWarningLabel("Confirm delete this region?");
                            } else {
                                DrawWarningLabel("Confirm delete this country?");
                            }
                            if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                                if (deletingRegion) {
                                    _editor.CountryRegionDelete();
                                    _editor.SetInfoMsg(INFO_MSG_REGION_DELETED);
                                } else {
                                    _editor.CountryDelete();
                                    _editor.SetInfoMsg(INFO_MSG_COUNTRY_DELETED);
                                }
                                _editor.operationMode = OPERATION_MODE.SELECTION;
                            }
                        } else {
                            if (_editor.provinceIndex >= 0 && _editor.provinceIndex < _map.provinces.Length) {
                                bool deletingRegion = _map.provinces[_editor.provinceIndex].regions != null && _map.provinces[_editor.provinceIndex].regions.Count > 1;
                                if (deletingRegion) {
                                    DrawWarningLabel("Confirm delete this region?");
                                } else {
                                    DrawWarningLabel("Confirm delete this province/state?");
                                }
                                if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                                    if (deletingRegion) {
                                        _editor.ProvinceRegionDelete();
                                        _editor.SetInfoMsg(INFO_MSG_REGION_DELETED);
                                    } else {
                                        _editor.ProvinceDelete();
                                        _editor.SetInfoMsg(INFO_MSG_PROVINCE_DELETED);
                                    }
                                    _editor.operationMode = OPERATION_MODE.SELECTION;
                                }
                            }
                        }

                        if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                            _editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
        }

        void ShowReshapingCityTools() {
            GUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawWarningLabel("CITY MODIFYING TOOLS");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            RESHAPE_CITY_TOOL prevTool = _editor.reshapeCityMode;
            int selectionGridRows = (reshapeCityToolbarIcons.Length - 1) / 2 + 1;
            GUIStyle selectionGridStyle = new GUIStyle(GUI.skin.button);
            selectionGridStyle.margin = new RectOffset(2, 2, 2, 2);
            _editor.reshapeCityMode = (RESHAPE_CITY_TOOL)GUILayout.SelectionGrid((int)_editor.reshapeCityMode, reshapeCityToolbarIcons, 2, selectionGridStyle, GUILayout.Height(24 * selectionGridRows), GUILayout.MaxWidth(150));
            if (_editor.reshapeCityMode != prevTool) {
                ProcessOperationMode();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUIStyle explanationStyle = new GUIStyle(GUI.skin.box);
            explanationStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.32f, 0.33f, 0.6f);
            GUILayout.Box(reshapeCityModeExplanation[(int)_editor.reshapeCityMode], explanationStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();

            switch (_editor.reshapeCityMode) {
                case RESHAPE_CITY_TOOL.DELETE:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (_editor.cityIndex < 0) {
                        DrawWarningLabel("Select a city to delete.");
                    } else {
                        DrawWarningLabel("Confirm delete this city?");
                        if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                            UndoPushCityStartOperation("Undo Delete City");
                            _editor.DeleteCity();
                            UndoPushCityEndOperation();
                            _editor.SetInfoMsg(INFO_MSG_CITY_DELETED);
                            _editor.operationMode = OPERATION_MODE.SELECTION;
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                            _editor.reshapeCityMode = RESHAPE_CITY_TOOL.MOVE;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            GUILayout.EndVertical();
        }

        void ShowReshapingMountPointTools() {
            GUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawWarningLabel("MOUNT POINT MODIFYING TOOLS");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            RESHAPE_MOUNT_POINT_TOOL prevTool = _editor.reshapeMountPointMode;
            int selectionGridRows = (reshapeMountPointToolbarIcons.Length - 1) / 2 + 1;
            GUIStyle selectionGridStyle = new GUIStyle(GUI.skin.button);
            selectionGridStyle.margin = new RectOffset(2, 2, 2, 2);
            _editor.reshapeMountPointMode = (RESHAPE_MOUNT_POINT_TOOL)GUILayout.SelectionGrid((int)_editor.reshapeMountPointMode, reshapeMountPointToolbarIcons, 2, selectionGridStyle, GUILayout.Height(24 * selectionGridRows), GUILayout.MaxWidth(150));
            if (_editor.reshapeMountPointMode != prevTool) {
                ProcessOperationMode();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUIStyle explanationStyle = new GUIStyle(GUI.skin.box);
            explanationStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.32f, 0.33f, 0.6f);
            GUILayout.Box(reshapeMountPointModeExplanation[(int)_editor.reshapeMountPointMode], explanationStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();

            switch (_editor.reshapeMountPointMode) {
                case RESHAPE_MOUNT_POINT_TOOL.DELETE:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (_editor.mountPointIndex < 0) {
                        DrawWarningLabel("Select a mount point to delete.");
                    } else {
                        DrawWarningLabel("Confirm delete this mount point?");
                        if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                            UndoPushMountPointStartOperation("Undo Delete Mount Point");
                            _editor.DeleteMountPoint();
                            UndoPushMountPointEndOperation();
                            _editor.SetInfoMsg(INFO_MSG_MOUNT_POINT_DELETED);
                            _editor.operationMode = OPERATION_MODE.SELECTION;
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                            _editor.reshapeMountPointMode = RESHAPE_MOUNT_POINT_TOOL.MOVE;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            GUILayout.EndVertical();
        }

        void ShowCreateTools() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            CREATE_TOOL prevCTool = _editor.createMode;
            GUIStyle selectionCGridStyle = new GUIStyle(GUI.skin.button);
            int selectionCGridRows = (createToolbarIcons.Length - 1) / 3 + 1;
            selectionCGridStyle.margin = new RectOffset(2, 2, 2, 2);
            _editor.createMode = (CREATE_TOOL)GUILayout.SelectionGrid((int)_editor.createMode, createToolbarIcons, 3, selectionCGridStyle, GUILayout.Height(24 * selectionCGridRows), GUILayout.MaxWidth(310));
            if (_editor.createMode != prevCTool) {
                ProcessOperationMode();
                NewShapeInit();
                if (_editor.editingMode == EDITING_MODE.COUNTRIES && (_editor.createMode == CREATE_TOOL.PROVINCE || _editor.createMode == CREATE_TOOL.PROVINCE_REGION)) {
                    ChangeEditingMode(EDITING_MODE.PROVINCES);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUIStyle explanationCStyle = new GUIStyle(GUI.skin.box);
            explanationCStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.32f, 0.33f, 0.6f);
            GUILayout.Box(createModeExplanation[(int)_editor.createMode], explanationCStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
        }

#endregion

#region Processing logic

        // Add a menu item called "Restore Backup" to a WPM's context menu.
        [MenuItem("CONTEXT/WorldMapEditor/Restore Backup")]
        static void RestoreBackup(MenuCommand command) {
            if (!EditorUtility.DisplayDialog("Restore original geodata files?", "Current geodata files will be replaced by the original files from Backup folder. Any changes will be lost. This operation can't be undone.\n\nRestore files?", "Restore", "Cancel")) {
                return;
            }

            // Proceed and restore
            string[] paths = AssetDatabase.GetAllAssetPaths();
            bool backupFolderExists = false;
            string geoDataFolder = "", backupFolder = "";
            for (int k = 0; k < paths.Length; k++) {
                if (paths[k].EndsWith(WorldMapGlobe.instance.geodataResourcesPath)) {
                    geoDataFolder = paths[k];
                } else if (paths[k].EndsWith("WorldPoliticalMapGlobeEdition/Backup")) {
                    backupFolder = paths[k];
                    backupFolderExists = true;
                }
            }


            WorldMapEditor editor = (WorldMapEditor)command.context;

            if (!backupFolderExists) {
                editor.SetInfoMsg(INFO_MSG_BACKUP_NOT_FOUND);
                return;
            }

            // Countries110
            AssetDatabase.DeleteAsset(geoDataFolder + "/countries110.txt");
            AssetDatabase.SaveAssets();
            AssetDatabase.CopyAsset(backupFolder + "/countries110.txt", geoDataFolder + "/countries110.txt");
            // Countries10
            AssetDatabase.DeleteAsset(geoDataFolder + "/countries10.txt");
            AssetDatabase.SaveAssets();
            AssetDatabase.CopyAsset(backupFolder + "/countries10.txt", geoDataFolder + "/countries10.txt");
            // Provinces10
            AssetDatabase.DeleteAsset(geoDataFolder + "/provinces10.txt");
            AssetDatabase.SaveAssets();
            AssetDatabase.CopyAsset(backupFolder + "/provinces10.txt", geoDataFolder + "/provinces10.txt");
            // Cities10
            AssetDatabase.DeleteAsset(geoDataFolder + "/cities10.txt");
            AssetDatabase.SaveAssets();
            AssetDatabase.CopyAsset(backupFolder + "/cities10.txt", geoDataFolder + "/cities10.txt");
            // Mount points
            AssetDatabase.DeleteAsset(geoDataFolder + "/mountPoints.txt");
            AssetDatabase.SaveAssets();
            AssetDatabase.CopyAsset(backupFolder + "/mountPoints.txt", geoDataFolder + "/mountPoints.txt");

            AssetDatabase.Refresh();

            // Save changes
            editor.SetInfoMsg(INFO_MSG_BACKUP_RESTORED);
            editor.DiscardChanges();
        }


        // Add a menu item called "Create Low Definition Geodata File" to a WPM's context menu.
        [MenuItem("CONTEXT/WorldMapEditor/Create Low Definition Geodata File")]
        static void CreateLowDefinitionFile(MenuCommand command) {
            WorldMapEditor editor = (WorldMapEditor)command.context;
            if (editor.editingCountryFile != EDITING_COUNTRY_FILE.COUNTRY_HIGHDEF) {
                EditorUtility.DisplayDialog("Create Low Definition Geodata File", "Switch to the high definition country geodata file first.", "Ok");
                return;
            }
            if (!EditorUtility.DisplayDialog("Create Low Definition Geodata File", "The low definition geodata file will be replaced by a reduced quality version of the high definition geodata file.\n\nChanges to the low definition file will be lost. Continue?", "Proceed", "Cancel")) {
                return;
            }

            string geoDataFolder;
            CheckBackup(out geoDataFolder);

            // Save changes
            string dataFileName = "countries110.txt";
            string fullPathName = Application.dataPath;
            int pos = fullPathName.LastIndexOf("/Assets");
            if (pos > 0)
                fullPathName = fullPathName.Substring(0, pos + 1);
            fullPathName += geoDataFolder + "/" + dataFileName;
            string data = editor.GetCountryGeoDataLowQuality();
            File.WriteAllText(fullPathName, data, Encoding.UTF8);
            AssetDatabase.Refresh();

            editor.SetInfoMsg(INFO_MSG_GEODATA_LOW_QUALITY_CREATED);
            editor.ClearSelection();
            editor.map.frontiersDetail = FRONTIERS_DETAIL.Low; // switch to low quality to see results
            editor.DiscardChanges();
        }

        [MenuItem("CONTEXT/WorldMapEditor/Equalize Provinces")]
        static void EqualizeProvincesMenuOption(MenuCommand command) {

            WorldMapProvincesEqualizer.ShowWindow();

            WorldMapEditor editor = (WorldMapEditor)command.context;
            editor.ClearSelection();
            editor.map.Redraw();
        }

        [MenuItem("CONTEXT/WorldMapEditor/Fix Orphan Cities")]
        static void FixOrphanCities(MenuCommand command) {
            if (!EditorUtility.DisplayDialog("Fix Orphan Cities", "This option will assign a country and province to each orphan city (cities without country or province assigned).\n\nThe country surrounding the city or nearest country will be assigned.\nAlso the province surrounding the city will be also assigned.", "Continue?", "Cancel")) {
                return;
            }

            WorldMapEditor editor = (WorldMapEditor)command.context;
            editor.ClearSelection();
            editor.FixOrphanCities();

            EditorUtility.DisplayDialog("Fix Orphan Cities", "Process completed. Note: click 'Save' to write changes to geodata file.", "Ok.");
        }

        [MenuItem("CONTEXT/WorldMapEditor/Fix Duplicate Provinces")]
        static void FixDuplicateProvinces(MenuCommand command) {
            WorldMapEditor editor = (WorldMapEditor)command.context;
            if (editor.editingMode != EDITING_MODE.PROVINCES) {
                EditorUtility.DisplayDialog("Fix Duplicate Provinces", "This option is only available when editing mode is set to Countries + Provinces.", "Ok");
                return;
            }
            if (!EditorUtility.DisplayDialog("Fix Duplicate Provinces", "This option will check for duplicate province names in the same country and assigns a temporary name for the second province with [DUPLICATE] prefix.", "Continue?", "Cancel")) {
                return;
            }

            editor.ClearSelection();
            if (editor.FixDuplicateProvinces()) {
                editor.ReloadProvinceNames();
                EditorUtility.DisplayDialog("Fix Duplicate Provinces", "Process completed. Note: click 'Save' to write changes to geodata file.", "Ok.");
            } else {
                EditorUtility.DisplayDialog("Fix Duplicate Provinces", "Process completed. No duplicate provinces found.", "Ok.");
            }
        }

        [MenuItem("CONTEXT/WorldMapEditor/Export To File")]
        static void ExportToFile(MenuCommand command) {
            if (!EditorUtility.DisplayDialog("Export To File", "This option will export country, province and city names and indices to plain text files (placed at root of Unity project).", "Continue?", "Cancel")) {
                return;
            }
            WorldMapEditor editor = (WorldMapEditor)command.context;
            editor.ExportEntitiesToFile();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Export To File", "Data exported.", "Ok");
        }

        void SwitchEditingFrontiersFile() {
            if (_editor.editingCountryFile == EDITING_COUNTRY_FILE.COUNTRY_HIGHDEF) {
                _map.frontiersDetail = FRONTIERS_DETAIL.High;
            } else {
                _map.frontiersDetail = FRONTIERS_DETAIL.Low;
            }
            _editor.DiscardChanges();
        }

        void ProcessOperationMode() {

            AdjustCityIconsScale();
            AdjustMountPointIconsScale();
            if (SceneView.lastActiveSceneView == null || SceneView.lastActiveSceneView.camera == null)
                return;
            Ray forward = SceneView.lastActiveSceneView.camera.ViewportPointToRay(Misc.ViewportCenter);
            frontFaceQuaternion = Quaternion.LookRotation(-forward.direction);

            // Check mouse buttons state and react to possible undo/redo operations
            bool mouseDown = false;
            Event e = Event.current;

            // locks control on map
            if (e != null && !e.alt) {
                var controlID = GUIUtility.GetControlID(FocusType.Passive);
                var eventType = e.GetTypeForControl(controlID);
                if (GUIUtility.hotControl == controlID && eventType == EventType.MouseUp && e.button == 0) {   // release hot control to allow standard navigation
                    GUIUtility.hotControl = 0;
                } else if (eventType == EventType.MouseDown && e.button == 0) {
                    mouseDown = true;
                    GUIUtility.hotControl = controlID;
                    startedReshapeRegion = false;
                    startedReshapeCity = false;
                } else if (eventType == EventType.MouseUp && e.button == 0) {
                    if (undoPushStarted) {
                        if (startedReshapeRegion) {
                            UndoPushRegionEndOperation();
                        }
                        if (startedReshapeCity) {
                            UndoPushCityEndOperation();
                        }
                    }
                }
            }

            if (e.type == EventType.ValidateCommand && e.commandName.Equals("UndoRedoPerformed")) {
                _editor.UndoHandle();
                EditorUtility.SetDirty(target);
                return;
            }

            switch (_editor.operationMode) {
                case OPERATION_MODE.SELECTION:
                    // do we click inside a country or province?
                    if (Camera.current == null) // can't ray-trace
                        return;
                    if (mouseDown) {
                        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                        bool selected = _editor.CountrySelectByScreenClick(ray);
                        if (!selected) {
                            _editor.ClearSelection();
                        } else {
                            if (_editor.editingMode == EDITING_MODE.PROVINCES) {
                                _map.DrawProvince(_editor.countryIndex, true, false);
                                selected = _editor.ProvinceSelectByScreenClick(_editor.countryIndex, ray);
                                if (!selected)
                                    _editor.ClearProvinceSelection();
                            }
                        }
                        if (!_editor.CitySelectByScreenClick(ray)) {
                            _editor.ClearCitySelection();
                        }
                        if (!_editor.MountPointSelectByScreenClick(ray)) {
                            _editor.ClearMountPointSelection();
                        }
                        // Reset the cursor if entity selected
                        if (selected) {
                            if (_editor.editingMode == EDITING_MODE.PROVINCES)
                                _map.DrawProvince(_editor.countryIndex, true, false);
                            if (_editor.entities != null && _editor.entityIndex >= 0 && _editor.entityIndex < _editor.entities.Length)
                                _editor.cursor = _editor.entities[_editor.entityIndex].localPosition;
                        }
                    }

                    // Draw selection
                    ShowShapePoints(false);
                    ShowCitySelected();
                    ShowMountPointSelected();
                    break;

                case OPERATION_MODE.RESHAPE:
                    // do we move any handle to change frontiers?
                    switch (_editor.reshapeRegionMode) {
                        case RESHAPE_REGION_TOOL.POINT:
                        case RESHAPE_REGION_TOOL.CIRCLE:
                            ExecuteMoveTool();
                            break;
                        case RESHAPE_REGION_TOOL.MAGNET:
                        case RESHAPE_REGION_TOOL.ERASER:
                        case RESHAPE_REGION_TOOL.SMOOTH:
                            ExecuteClickTool(e.mousePosition, mouseDown);
                            break;
                        case RESHAPE_REGION_TOOL.SPLITH:
                        case RESHAPE_REGION_TOOL.SPLITV:
                        case RESHAPE_REGION_TOOL.DELETE:
                            ShowShapePoints(false);
                            break;
                    }
                    switch (_editor.reshapeCityMode) {
                        case RESHAPE_CITY_TOOL.MOVE:
                            ExecuteCityMoveTool();
                            break;
                    }
                    switch (_editor.reshapeMountPointMode) {
                        case RESHAPE_MOUNT_POINT_TOOL.MOVE:
                            ExecuteMountPointMoveTool();
                            break;
                    }
                    break;
                case OPERATION_MODE.CREATE:
                    switch (_editor.createMode) {
                        case CREATE_TOOL.CITY:
                            ExecuteCityCreateTool(e.mousePosition, mouseDown);
                            break;
                        case CREATE_TOOL.COUNTRY:
                            ExecuteShapeCreateTool(e.mousePosition, mouseDown);
                            break;
                        case CREATE_TOOL.COUNTRY_REGION:
                        case CREATE_TOOL.PROVINCE:
                            if (_editor.countryIndex >= 0 && _editor.countryIndex < _map.countries.Length) {
                                ExecuteShapeCreateTool(e.mousePosition, mouseDown);
                            } else {
                                _editor.SetInfoMsg(INFO_MSG_CHOOSE_COUNTRY);
                            }
                            break;
                        case CREATE_TOOL.PROVINCE_REGION:
                            if (_editor.countryIndex <= 0 || _editor.countryIndex >= _map.countries.Length) {
                                _editor.SetInfoMsg(INFO_MSG_CHOOSE_COUNTRY);
                            } else if (_editor.provinceIndex < 0 || _editor.provinceIndex >= _map.provinces.Length) {
                                _editor.SetInfoMsg(INFO_MSG_CHOOSE_PROVINCE);
                            } else {
                                ExecuteShapeCreateTool(e.mousePosition, mouseDown);
                            }
                            break;
                        case CREATE_TOOL.MOUNT_POINT:
                            ExecuteMountPointCreateTool(e.mousePosition, mouseDown);
                            break;
                    }
                    break;
                case OPERATION_MODE.CONFIRM:
                case OPERATION_MODE.UNDO:
                    break;
            }

            if (_editor.editingMode == EDITING_MODE.PROVINCES) {
                DrawEditorProvinceNames();
            }
            CheckHideEditorMesh();
        }

        int lastMovedPointIndex;
        bool onePointSelected;
        Vector3 selectedPoint;

        void ExecuteMoveTool() {
            if (_editor.entityIndex < 0 || _editor.regionIndex < 0)
                return;
            bool frontiersUnchanged = true;
            if (_editor.entities[_editor.entityIndex].regions == null)
                return;
            Vector3[] points = _editor.entities[_editor.entityIndex].regions[_editor.regionIndex].spherePoints;
            Vector3 oldPoint, newPoint, sourcePosition = Misc.Vector3zero, displacement = Misc.Vector3zero, newCoor = Misc.Vector3zero;
            Transform mapTransform = _map.transform;
            if (controlIds == null || controlIds.Length < points.Length)
                controlIds = new int[points.Length];

            for (int i = 0; i < points.Length; i++) {
                oldPoint = mapTransform.TransformPoint(points[i]);
                float handleSize = HandleUtility.GetHandleSize(oldPoint) * HANDLE_SIZE;
                newPoint = Handles.FreeMoveHandle(oldPoint, frontFaceQuaternion, handleSize, pointSnap,
                    (handleControlID, position, rotation, size, eventType) => {
                        controlIds[i] = handleControlID;
                        Handles.DotHandleCap(handleControlID, position, rotation, size, eventType); 
                    });
                if (GUIUtility.hotControl == controlIds[i] && GUIUtility.hotControl != 0) {
                    onePointSelected = true;
                    selectedPoint = oldPoint;
                }
                if (frontiersUnchanged && oldPoint != newPoint) {
                    frontiersUnchanged = false;
                    newCoor = mapTransform.InverseTransformPoint(newPoint);
                    sourcePosition = points[i];
                    displacement = new Vector3(newCoor.x - points[i].x, newCoor.y - points[i].y, newCoor.z - points[i].z);
                    lastMovedPointIndex = i;
                }
            }

            if (_editor.reshapeRegionMode.hasCircle()) {
                if (!onePointSelected) {
                    selectedPoint = mapTransform.TransformPoint(points[0]);
                }
                float size = _editor.reshapeCircleWidth * mapTransform.localScale.y;
                Handles.CircleHandleCap(0, selectedPoint, frontFaceQuaternion, size, EventType.Repaint);
                HandleUtility.Repaint();
            }

            if (_editor.reshapeRegionMode == RESHAPE_REGION_TOOL.POINT && startedReshapeRegion) {
                // Show tooltip and handle hotkeys
                if (Camera.current != null) {
                    string text = "Hotkeys: press Shift+S to snap to nearest vertex";
                    Vector3 labelPos = Camera.current.ScreenToWorldPoint(new Vector3(10, 30, Camera.current.nearClipPlane));
                    Handles.Label(labelPos, text, editorCaptionLabelStyle);
                }
                if (Event.current != null && Event.current.type == EventType.KeyDown) {
                    if (Event.current.shift && Event.current.keyCode == KeyCode.S) {
                        Vector3 otherVertex;
                        sourcePosition = points[lastMovedPointIndex];
                        if (_editor.GetVertexNearSpherePos(sourcePosition, out otherVertex, true)) {
                            displacement = otherVertex - sourcePosition;
                            frontiersUnchanged = false;
                        }
                        Event.current.Use();
                    }
                }
            }

            if (!frontiersUnchanged) {
                List<Region> affectedRegions = null;
                switch (_editor.reshapeRegionMode) {
                    case RESHAPE_REGION_TOOL.POINT:
                        if (!startedReshapeRegion)
                            UndoPushRegionStartOperation("Undo Point Move");
                        affectedRegions = _editor.MovePoint(sourcePosition, displacement);
                        break;
                    case RESHAPE_REGION_TOOL.CIRCLE:
                        if (!startedReshapeRegion)
                            UndoPushRegionStartOperation("Undo Group Move");
                        affectedRegions = _editor.MoveCircle(sourcePosition, displacement, _editor.reshapeCircleWidth, _editor.circleMoveConstant);
                        break;
                }
                if (affectedRegions.Count > 0) {
                    _editor.RedrawFrontiers(affectedRegions, false, false);
                    HandleUtility.Repaint();
                }
            }
        }

        void ExecuteClickTool(Vector2 mousePosition, bool clicked) {
            if (_editor.entityIndex < 0 || _editor.entityIndex >= _editor.entities.Length)
                return;

            // Show the mouse cursor
            if (Camera.current == null)
                return;

            // Show the points
            ShowShapePoints(_editor.reshapeRegionMode != RESHAPE_REGION_TOOL.SMOOTH);
            Transform mapTransform = _map.transform;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Vector3 cursorPos;
            if (_map.GetGlobeIntersection(ray, out cursorPos)) {
                _editor.cursor = mapTransform.InverseTransformPoint(cursorPos);
                if (_editor.reshapeRegionMode == RESHAPE_REGION_TOOL.SMOOTH) {
                    ShowCandidatePoint();
                } else {
                    // Show circle cursor
                    float seconds = (float)new TimeSpan(DateTime.Now.Ticks - tickStart).TotalSeconds;
                    seconds *= 4.0f;
                    float t = seconds % 2;
                    if (t >= 1)
                        t = 2 - t;
                    float effect = Mathf.SmoothStep(0, 1, t) / 10.0f;
                    float size = _editor.reshapeCircleWidth * mapTransform.localScale.y * (0.9f + effect);
                    Handles.CircleHandleCap(0, cursorPos, frontFaceQuaternion, size, EventType.Repaint);
                }

                if (clicked) {
                    switch (_editor.reshapeRegionMode) {
                        case RESHAPE_REGION_TOOL.MAGNET:
                            if (!startedReshapeRegion)
                                UndoPushRegionStartOperation("Undo Magnet");
                            _editor.Magnet(_editor.cursor, _editor.reshapeCircleWidth);
                            break;
                        case RESHAPE_REGION_TOOL.ERASER:
                            if (!startedReshapeRegion)
                                UndoPushRegionStartOperation("Undo Eraser");
                            _editor.Erase(_editor.cursor, _editor.reshapeCircleWidth);
                            break;
                        case RESHAPE_REGION_TOOL.SMOOTH:
                            if (!startedReshapeRegion)
                                UndoPushRegionStartOperation("Undo Smooth");
                            _editor.AddPoint(_editor.cursor);
                            break;
                    }
                }
                HandleUtility.Repaint();
            }
        }

        void ExecuteCityCreateTool(Vector2 mousePosition, bool clicked) {

            // Show the mouse cursor
            if (Camera.current == null)
                return;

            // Show the points
            Transform mapTransform = _map.transform;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Vector3 cursorPos;
            if (_map.GetGlobeIntersection(ray, out cursorPos)) {
                _editor.cursor = mapTransform.InverseTransformPoint(cursorPos);

                Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                float handleSize = HandleUtility.GetHandleSize(cursorPos) * HANDLE_SIZE * 4.0f;
                Handles.SphereHandleCap(0, cursorPos, frontFaceQuaternion, handleSize, EventType.Repaint);
                Handles.color = Color.white;

                if (clicked) {
                    if (_editor.countryIndex < 0 || _editor.countryIndex >= _map.countries.Length) {
                        EditorUtility.DisplayDialog("Add new city", "Please choose a country first.", "Ok");
                        return;
                    }
                    UndoPushCityStartOperation("Undo Create City");
                    _editor.CityCreate(_editor.cursor);
                    UndoPushCityEndOperation();
                    _editor.operationMode = OPERATION_MODE.SELECTION;
                    return;
                }
            }
            HandleUtility.Repaint();
        }

        void AdjustCityIconsScale() {
            // Adjust city icons in scene view
            if (_map == null || _map.cities == null)
                return;

            Transform t = _map.transform.Find("Cities");
            if (t != null) {
                Camera cam = GetSceneViewCamera();
                float f = cam != null ? ((cam.transform.position - _map.transform.position) / _map.transform.localScale.x).sqrMagnitude : 1.0f;
                CityScaler scaler = t.GetComponent<CityScaler>();
                if (scaler != null)
                    scaler.ScaleCities(0.005f * f);
            } else {
                // This should not happen but maybe the user deleted the layer. Forces refresh.
                _map.showCities = true;
                _map.DrawCities();
            }
        }

        void ShowCitySelected() {
            if (_editor.cityIndex < 0 || _editor.cityIndex >= _map.cities.Count)
                return;
            Vector3 cityPos = _map.cities[_editor.cityIndex].localPosition;
            Vector3 worldPos = _map.transform.TransformPoint(cityPos);
            float handleSize = HandleUtility.GetHandleSize(worldPos) * HANDLE_SIZE * 2.0f;
            Handles.RectangleHandleCap(0, worldPos, frontFaceQuaternion, handleSize, EventType.Repaint);
        }

        void ExecuteCityMoveTool() {
            if (_editor.cityIndex < 0 || _editor.cityIndex >= _map.cities.Count)
                return;

            Transform mapTransform = _map.transform;
            Vector3 cityPos = _map.cities[_editor.cityIndex].localPosition;
            Vector3 oldPoint = mapTransform.TransformPoint(cityPos);
            float handleSize = HandleUtility.GetHandleSize(oldPoint) * HANDLE_SIZE * 2.0f;
            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, frontFaceQuaternion, handleSize, pointSnap,
                                            (handleControlID, position, rotation, size, eventType) => {
                                                Handles.RectangleHandleCap(handleControlID, position, rotation, size, eventType);
                                            });
            if (newPoint != oldPoint) {
                newPoint = mapTransform.InverseTransformPoint(newPoint);
                if (!startedReshapeCity)
                    UndoPushCityStartOperation("Undo City Move");
                _editor.CityMove(newPoint);
                HandleUtility.Repaint();
            }
        }

        void AdjustMountPointIconsScale() {
            // Adjust city icons in scene view
            if (_map == null || _map.mountPoints == null)
                return;

            Transform t = _map.transform.Find(WorldMapGlobe.MOUNT_POINTS_LAYER);
            if (t != null) {
                Camera cam = GetSceneViewCamera();
                float f = cam != null ? ((cam.transform.position - _map.transform.position) / _map.transform.localScale.x).sqrMagnitude : 1.0f;
                MountPointScaler scaler = t.GetComponent<MountPointScaler>();
                scaler.ScaleMountPoints(0.005f * f);
            } else {
                // This should not happen but maybe the user deleted the layer. Forces refresh.
                _map.DrawMountPoints();
            }
        }

        void ExecuteMountPointCreateTool(Vector2 mousePosition, bool clicked) {

            // Show the mouse cursor
            if (Camera.current == null)
                return;

            // Show the points
            Transform mapTransform = _map.transform;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Vector3 cursorPos;
            if (_map.GetGlobeIntersection(ray, out cursorPos)) {
                _editor.cursor = mapTransform.InverseTransformPoint(cursorPos);

                Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                float handleSize = HandleUtility.GetHandleSize(cursorPos) * HANDLE_SIZE * 4.0f;
                Handles.SphereHandleCap(0, cursorPos, frontFaceQuaternion, handleSize, EventType.Repaint);
                Handles.color = Color.white;

                if (clicked) {
                    if (_editor.countryIndex < 0 || _editor.countryIndex >= _map.countries.Length) {
                        EditorUtility.DisplayDialog("Add new city", "Please choose a country first.", "Ok");
                        return;
                    }
                    UndoPushMountPointStartOperation("Undo Create Mount Point");
                    _editor.MountPointCreate(_editor.cursor);
                    UndoPushMountPointEndOperation();
                }
            }
            HandleUtility.Repaint();
        }

        void ShowMountPointSelected() {
            if (_editor.mountPointIndex < 0 || _editor.mountPointIndex >= _map.mountPoints.Count)
                return;
            Vector3 mountPointPos = _map.mountPoints[_editor.mountPointIndex].localPosition;
            Vector3 worldPos = _map.transform.TransformPoint(mountPointPos);
            float handleSize = HandleUtility.GetHandleSize(worldPos) * HANDLE_SIZE * 2.0f;
            Handles.RectangleHandleCap(0, worldPos, frontFaceQuaternion, handleSize, EventType.Repaint);
        }

        void ExecuteMountPointMoveTool() {
            if (_map.mountPoints == null || _editor.mountPointIndex < 0 || _editor.mountPointIndex >= _map.mountPoints.Count)
                return;

            Transform mapTransform = _map.transform;
            Vector3 mountPointPos = _map.mountPoints[_editor.mountPointIndex].localPosition;
            Vector3 oldPoint = mapTransform.TransformPoint(mountPointPos);
            float handleSize = HandleUtility.GetHandleSize(oldPoint) * HANDLE_SIZE * 2.0f;
            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, frontFaceQuaternion, handleSize, pointSnap,
                                            (handleControlID, position, rotation, size, eventType) => {
                                                Handles.RectangleHandleCap(handleControlID, position, rotation, size, eventType);
                                            });
            if (newPoint != oldPoint) {
                newPoint = mapTransform.InverseTransformPoint(newPoint);
                if (!startedReshapeMountPoint)
                    UndoPushMountPointStartOperation("Undo Mount Point Move");
                _editor.MountPointMove(newPoint);
                HandleUtility.Repaint();
            }
        }

        void UndoPushRegionStartOperation(string operationName) {
            startedReshapeRegion = !startedReshapeRegion;
            undoPushStarted = true;
            Undo.RecordObject(target, operationName);   // record changes to the undo dummy flag
            _editor.UndoRegionsPush(_editor.highlightedRegions);

        }

        void UndoPushRegionEndOperation() {
            undoPushStarted = false;
            _editor.UndoRegionsInsertAtCurrentPos(_editor.highlightedRegions);
            if (_editor.reshapeRegionMode != RESHAPE_REGION_TOOL.SMOOTH) { // Smooth operation doesn't need to refresh labels
                _map.RedrawMapLabels();
            }
            _editor.RedrawFrontiers(null, true, true); // draw all frontiers again
        }

        void UndoPushCityStartOperation(string operationName) {
            startedReshapeCity = !startedReshapeCity;
            undoPushStarted = true;
            Undo.RecordObject(target, operationName);   // record changes to the undo dummy flag
            _editor.UndoCitiesPush();
        }

        void UndoPushCityEndOperation() {
            undoPushStarted = false;
            _editor.UndoCitiesInsertAtCurrentPos();
        }

        void UndoPushMountPointStartOperation(string operationName) {
            startedReshapeMountPoint = !startedReshapeMountPoint;
            undoPushStarted = true;
            Undo.RecordObject(target, operationName);   // record changes to the undo dummy flag
            _editor.UndoMountPointsPush();
        }

        void UndoPushMountPointEndOperation() {
            undoPushStarted = false;
            _editor.UndoMountPointsInsertAtCurrentPos();
        }

        static void CheckBackup(out string geoDataFolder) {

            string[] paths = AssetDatabase.GetAllAssetPaths();
            bool backupFolderExists = false;
            string rootFolder = "";
            geoDataFolder = "";
            WorldMapGlobe globe = FindObjectOfType<WorldMapGlobe>();

            for (int k = 0; k < paths.Length; k++) {
                if (paths[k].EndsWith(globe.geodataResourcesPath)) {
                    geoDataFolder = paths[k];
                } else if (paths[k].EndsWith("WorldPoliticalMapGlobeEdition")) {
                    rootFolder = paths[k];
                } else if (paths[k].EndsWith("WorldPoliticalMapGlobeEdition/Backup")) {
                    backupFolderExists = true;
                }
            }


            if (!backupFolderExists) {
                // Do the backup
                AssetDatabase.CreateFolder(rootFolder, "Backup");
                string backupFolder = rootFolder + "/Backup";
                AssetDatabase.CopyAsset(geoDataFolder + "/countries110.txt", backupFolder + "/countries110.txt");
                AssetDatabase.CopyAsset(geoDataFolder + "/countries10.txt", backupFolder + "/countries10.txt");
                AssetDatabase.CopyAsset(geoDataFolder + "/" + globe.countryAttributeFile, backupFolder + "/" + globe.countryAttributeFile);
                AssetDatabase.CopyAsset(geoDataFolder + "/provinces10.txt", backupFolder + "/provinces10.txt");
                AssetDatabase.CopyAsset(geoDataFolder + "/" + globe.provinceAttributeFile, backupFolder + "/" + globe.provinceAttributeFile);
                AssetDatabase.CopyAsset(geoDataFolder + "/cities10.txt", backupFolder + "/cities10.txt");
                AssetDatabase.CopyAsset(geoDataFolder + "/" + globe.cityAttributeFile, backupFolder + "/" + globe.cityAttributeFile);
                AssetDatabase.CopyAsset(geoDataFolder + "/" + globe.mountPointsAttributeFile, backupFolder + "/" + globe.mountPointsAttributeFile);
            }
        }

        string GetAssetsFolder() {
            string fullPathName = Application.dataPath;
            int pos = fullPathName.LastIndexOf("/Assets");
            if (pos > 0)
                fullPathName = fullPathName.Substring(0, pos + 1);
            return fullPathName;
        }

        bool SaveChanges() {
            if (!_editor.countryChanges && !_editor.countryAttribChanges && !_editor.provinceChanges && !_editor.provinceAttribChanges && !_editor.cityChanges && !_editor.cityAttribChanges && !_editor.mountPointChanges)
                return false;

            // First we make a backup if it doesn't exist
            string geoDataFolder;
            CheckBackup(out geoDataFolder);

            string dataFileName, fullPathName;
            // Save changes to country attributes
            if (_editor.countryAttribChanges || _editor.countryChanges) {
                fullPathName = GetAssetsFolder() + geoDataFolder + "/" + _map.countryAttributeFile + ".json";
                string data = _map.GetCountriesAttributes(true);
                if (data != null) {
                    File.WriteAllText(fullPathName, data, System.Text.Encoding.UTF8);
                }
                _editor.countryAttribChanges = false;
            }
            // Save changes to countries
            if (_editor.countryChanges) {
                dataFileName = _editor.map.GetCountryGeoDataFileName();
                fullPathName = GetAssetsFolder() + geoDataFolder + "/" + dataFileName;
                string data = _editor.map.GetCountryGeoData();
                if (data != null) {
                    File.WriteAllText(fullPathName, data, Encoding.UTF8);
                    Debug.Log("Country geodata file updated.");
                }
                _editor.countryChanges = false;
            }
            // Save changes to province attributes
            if (_editor.provinceAttribChanges || _editor.provinceChanges) {
                fullPathName = GetAssetsFolder() + geoDataFolder + "/" + _map.provinceAttributeFile + ".json";
                string data = _map.GetProvincesAttributes(true);
                if (data != null) {
                    File.WriteAllText(fullPathName, data, System.Text.Encoding.UTF8);
                }
                _editor.provinceAttribChanges = false;
            }
            // Save changes to provinces
            if (_editor.provinceChanges) {
                dataFileName = _editor.map.GetProvinceGeoDataFileName();
                fullPathName = GetAssetsFolder();
                string fullAssetPathName = fullPathName + geoDataFolder + "/" + dataFileName;
                string data = _editor.map.GetProvinceGeoData();
                if (data != null) {
                    File.WriteAllText(fullAssetPathName, data, Encoding.UTF8);
                    Debug.Log("Province geodata file updated.");
                }
                _editor.provinceChanges = false;
            }
            // Save changes to cities attributes
            if (_editor.cityAttribChanges || _editor.cityChanges) {
                fullPathName = GetAssetsFolder() + geoDataFolder + "/" + _map.cityAttributeFile + ".json";
                string data = _map.GetCitiesAttributes(true);
                if (data != null) {
                    File.WriteAllText(fullPathName, data, System.Text.Encoding.UTF8);
                }
                _editor.cityAttribChanges = false;
            }
            // Save changes to cities
            if (_editor.cityChanges) {
                dataFileName = _editor.map.GetCityGeoDataFileName();
                fullPathName = GetAssetsFolder() + geoDataFolder + "/" + dataFileName;
                string data = _editor.map.GetCityGeoData();
                if (data != null) {
                    File.WriteAllText(fullPathName, data, Encoding.UTF8);
                    Debug.Log("City geodata file updated.");
                }
                _editor.cityChanges = false;
            }
            // Save changes to mount points
            if (_editor.mountPointChanges) {
                fullPathName = GetAssetsFolder() + geoDataFolder + "/" + _map.mountPointsAttributeFile + ".json";
                string data = _editor.GetMountPointsGeoData();
                if (data != null) {
                    File.WriteAllText(fullPathName, data, Encoding.UTF8);
                    Debug.Log("Mount Point file updated.");
                }
                _editor.mountPointChanges = false;
            }
            AssetDatabase.Refresh();
            return true;
        }

        float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n) {
            // angle in [0,180]
            float angle = FastVector.Angle(a, b);
            float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

            // angle in [-179,180]
            float signed_angle = angle * sign;

            return signed_angle;
        }

        void FocusSpherePoint(Vector3 point) {
            if (SceneView.lastActiveSceneView == null)
                return;
            Camera cam = SceneView.lastActiveSceneView.camera;
            if (cam == null)
                return;

            Vector3 v1 = point;
            Vector3 v2 = cam.transform.position - _map.transform.position;
            float angle = FastVector.Angle(v1, v2);
            Vector3 axis = Vector3.Cross(v1, v2);
            _map.transform.localRotation = Quaternion.AngleAxis(angle, axis);
            // straighten view
            float angle2 = SignedAngleBetween(Misc.Vector3up, _map.transform.up, v2);
            _map.transform.Rotate(v2, -angle2, Space.World);
        }


#endregion

#region Editor UI handling

        void CheckHideEditorMesh() {
            if (!_editor.shouldHideEditorMesh)
                return;
            _editor.shouldHideEditorMesh = false;
            Transform s = _map.transform;
            Renderer[] rr = s.GetComponentsInChildren<Renderer>(true);
            for (int k = 0; k < rr.Length; k++) {
                EditorUtility.SetSelectedRenderState(rr[k], EditorSelectedRenderState.Hidden);
            }
        }

        void ShowShapePoints(bool highlightInsideCircle) {
            if (_map.countries == null)
                return;
            if (_editor.entityIndex >= 0 && _editor.entities != null && _editor.entityIndex < _editor.entities.Length && _editor.regionIndex >= 0) {
                if (_editor.entities[_editor.entityIndex].regions == null)
                    return;
                Region region = _editor.entities[_editor.entityIndex].regions[_editor.regionIndex];
                Transform mapTransform = _map.transform;
                float circleSizeSqr = _editor.reshapeCircleWidth * _editor.reshapeCircleWidth;
                for (int i = 0; i < region.spherePoints.Length; i++) {
                    Vector3 rp = region.spherePoints[i];
                    float sizeModifier = 1f;
                    bool showHandleFilled = false;
                    if (_editor.operationMode == OPERATION_MODE.RESHAPE) {
                        Vector2 latlon = region.latlon[i];
                        Vector2 latlonPrev = i > 0 ? region.latlon[i - 1] : region.latlon[region.latlon.Length - 1];
                        switch (_editor.reshapeRegionMode) {
                            case RESHAPE_REGION_TOOL.SPLITV:
                                if (latlon.y > _editor.splitVerticallyAt && latlonPrev.y < _editor.splitVerticallyAt ||
                                    latlon.y < _editor.splitVerticallyAt && latlonPrev.y > _editor.splitVerticallyAt) {
                                    float t = (_editor.splitVerticallyAt - latlonPrev.y) / (latlon.y - latlonPrev.y);
                                    latlon = Vector2.Lerp(latlonPrev, latlon, t);
                                    rp = Conversion.GetSpherePointFromLatLon(latlon);
                                    sizeModifier = 0.5f + UnityEngine.Random.value;
                                    showHandleFilled = true;
                                }
                                break;
                            case RESHAPE_REGION_TOOL.SPLITH:
                                if (latlon.x > _editor.splitHorizontallyAt && latlonPrev.x < _editor.splitHorizontallyAt ||
                                    latlon.x < _editor.splitHorizontallyAt && latlonPrev.x > _editor.splitHorizontallyAt) {
                                    float t = (_editor.splitHorizontallyAt - latlonPrev.x) / (latlon.x - latlonPrev.x);
                                    latlon = Vector2.Lerp(latlonPrev, latlon, t);
                                    rp = Conversion.GetSpherePointFromLatLon(latlon);
                                    sizeModifier = 0.5f + UnityEngine.Random.value;
                                    showHandleFilled = true;
                                }
                                break;
                        }
                    }
                    Vector3 p = mapTransform.TransformPoint(rp);
                    float handleSize = HandleUtility.GetHandleSize(p) * HANDLE_SIZE * sizeModifier;

                    if (highlightInsideCircle) {
                        float dist = (rp - _editor.cursor).sqrMagnitude;
                        if (dist < circleSizeSqr) {
                            showHandleFilled = true;
                        }
                    }
                    if (showHandleFilled) {
                        Handles.color = Color.green;
                        Handles.DotHandleCap(0, p, frontFaceQuaternion, handleSize, EventType.Repaint);
                    } else {
                        Handles.color = Color.white;
                    }
                    Handles.RectangleHandleCap(0, p, frontFaceQuaternion, handleSize, EventType.Repaint);
                }
            }
            Handles.color = Color.white;

            HandleUtility.Repaint();
        }

        /// <summary>
        /// Shows a potential new point near from cursor location (point parameter, which is in local coordinates)
        /// </summary>
        void ShowCandidatePoint() {
            if (_editor.entityIndex < 0 || _editor.regionIndex < 0 || _editor.entities[_editor.entityIndex].regions == null)
                return;
            Region region = _editor.entities[_editor.entityIndex].regions[_editor.regionIndex];
            int max = region.latlon.Length;
            float minDist = float.MaxValue;
            int nearest = -1, previous = 0;
            Vector2 latlonCursor = Conversion.GetLatLonFromSpherePoint(_editor.cursor);
            latlonCursor = region.AdjustLongitudeBeyond180(latlonCursor);
            for (int p = 0; p < max; p++) {
                int q = p == 0 ? max - 1 : p - 1;
                Vector2 rp = (region.latlon[p] + region.latlon[q]) * 0.5f;
                float dist = (rp - latlonCursor).sqrMagnitude; 
                if (dist < minDist) {
                    // Get nearest point
                    minDist = dist;
                    nearest = p;
                    previous = q;
                }
            }

            if (nearest >= 0) {
                Transform mapTransform = _map.transform;
                Vector2 latlonToInsert = (region.latlon[nearest] + region.latlon[previous]) * 0.5f;
                Vector3 pointToInsert = Conversion.GetSpherePointFromLatLon(latlonToInsert);
                Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                Vector3 pt = mapTransform.TransformPoint(pointToInsert);
                float handleSize = HandleUtility.GetHandleSize(pt) * HANDLE_SIZE;
                Handles.DotHandleCap(0, pt, frontFaceQuaternion, handleSize, EventType.Repaint);
                Handles.color = Color.white;
            }
        }

        void NewShapeInit() {
            if (_editor.newShape == null) {
                _editor.newShape = new List<Vector3>();
            } else {
                _editor.newShape.Clear();
            }
        }


        /// <summary>
        /// Returns any city near the point specified in local coordinates.
        /// </summary>
        int NewShapeGetIndexNearPoint(Vector3 localPoint) {
            for (int c = 0; c < _editor.newShape.Count; c++) {
                Vector3 cityLoc = _editor.newShape[c];
                if ((cityLoc - localPoint).magnitude < HIT_PRECISION)
                    return c;
            }
            return -1;
        }

        /// <summary>
        /// Shows a potential point to be added to the new shape and draws current shape polygon
        /// </summary>
        void ExecuteShapeCreateTool(Vector3 mousePosition, bool mouseDown) {
            // Show the mouse cursor
            if (Camera.current == null)
                return;

            // Show the points
            Transform mapTransform = _map.transform;

            int numPoints = _editor.newShape.Count;
            Vector3[] shapePoints = new Vector3[numPoints + 1];
            for (int k = 0; k < numPoints; k++) {
                shapePoints[k] = mapTransform.TransformPoint(_editor.newShape[k]);
            }
            shapePoints[numPoints] = mapTransform.TransformPoint(_editor.cursor);

            // Draw shape polygon in same color as corresponding frontiers
            if (numPoints >= 1) {
                if (_editor.createMode == CREATE_TOOL.COUNTRY || _editor.createMode == CREATE_TOOL.COUNTRY_REGION) {
                    Handles.color = _map.frontiersColor;
                } else {
                    Handles.color = _map.provincesColor;
                }
                Handles.DrawPolyLine(shapePoints);
                Handles.color = Color.white;
            }

            // Draw handles
            for (int i = 0; i < shapePoints.Length - 1; i++) {
                float handleSize = HandleUtility.GetHandleSize(shapePoints[i]) * HANDLE_SIZE;
                Handles.RectangleHandleCap(0, shapePoints[i], frontFaceQuaternion, handleSize, EventType.Repaint);
            }

            // Show tooltip and handle hotkeys
            if (Camera.current != null) {
                string text = "Hotkeys: Shift+C = Close polygon (requires +5 vertices, currently: " + numPoints + "), Shift+X = Remove last point, Shift+S: Snap to nearest vertex, Esc = Clear All";
                Vector3 labelPos = Camera.current.ScreenToWorldPoint(new Vector3(10, 30, Camera.current.nearClipPlane));
                Handles.Label(labelPos, text, editorCaptionLabelStyle);
            }

            bool snapRequested = false;
            if (Event.current != null && Event.current.type == EventType.KeyDown) {
                // Shift + X: remove last point
                if (numPoints > 0 && Event.current.shift && Event.current.keyCode == KeyCode.X) {
                    _editor.newShape.RemoveAt(numPoints - 1);
                    Event.current.Use();
                    // Escape: remove all points
                } else if (Event.current.keyCode == KeyCode.Escape) {
                    _editor.newShape.Clear();
                    Event.current.Use();
                } else if (Event.current.shift && Event.current.keyCode == KeyCode.S) {
                    snapRequested = true;
                    Event.current.Use();
                }
            }

            // Draw handles
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Vector3 cursorPos;
            bool canClosePolygon = false;
            if (_map.GetGlobeIntersection(ray, out cursorPos)) {
                Vector3 newPos = mapTransform.InverseTransformPoint(cursorPos);

                // Shift + S: create a new vertex next to another near existing vertex
                if (snapRequested) {
                    if (_editor.GetVertexNearSpherePos(newPos, out newPos, false)) {
                        mouseDown = true;
                    }
                }

                _editor.cursor = newPos;
                if (numPoints > 2) { // Check if we're over the first point
                    int i = NewShapeGetIndexNearPoint(newPos);
                    if (i == 0) {
                        Vector3 labelPos;
                        if (Camera.current != null) {
                            Vector3 screenPos = Camera.current.WorldToScreenPoint(cursorPos);
                            labelPos = Camera.current.ScreenToWorldPoint(screenPos + Misc.Vector3up * 20f + Misc.Vector3right * 12f);
                        } else {
                            labelPos = cursorPos + Misc.Vector3up * 0.17f;
                        }
                        if (numPoints > 5) {
                            canClosePolygon = true;
                            Handles.Label(labelPos, "Click to close polygon", editorCaptionLabelStyle);
                        } else {
                            Handles.Label(labelPos, "Add " + (6 - numPoints) + " more point(s)", editorCaptionLabelStyle);
                        }
                    }
                }
                Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                Vector3 pt = mapTransform.TransformPoint(_editor.cursor);
                float handleSize = HandleUtility.GetHandleSize(pt) * HANDLE_SIZE;
                Handles.DotHandleCap(0, pt, frontFaceQuaternion, handleSize, EventType.Repaint);
                Handles.color = Color.white;

                // Hotkey for closing polygon (Control + C)
                if (numPoints > 4 && (Event.current != null && Event.current.shift && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.C)) {
                    mouseDown = true;
                    canClosePolygon = true;
                    Event.current.Use();
                }

                if (mouseDown) {
                    if (canClosePolygon) {
                        switch (_editor.createMode) {
                            case CREATE_TOOL.COUNTRY:
                                _editor.CountryCreate();
                                break;
                            case CREATE_TOOL.COUNTRY_REGION:
                                _editor.CountryRegionCreate();
                                _editor.CountryRegionSelect();
                                break;
                            case CREATE_TOOL.PROVINCE:
                                _editor.CountryRegionCreate();
                                _editor.ProvinceCreate();
                                break;
                            case CREATE_TOOL.PROVINCE_REGION:
                                _editor.CountryRegionCreate();
                                _editor.ProvinceRegionCreate();
                                break;
                        }
                        NewShapeInit();
                    } else {
                        _editor.newShape.Add(_editor.cursor);
                    }
                }

                HandleUtility.Repaint();
            }
        }


        /// <summary>
        /// Returns true if there're changes
        /// </summary>
        bool ShowAttributeGroup(EditorAttribGroup attribGroup, string title) {

            EditorGUILayout.BeginHorizontal();
            attribGroup.foldOut = EditorGUILayout.Foldout(attribGroup.foldOut, title, attribHeaderStyle);
            EditorGUILayout.EndHorizontal();
            if (!attribGroup.foldOut)
                return false;

            JSONObject attrib = attribGroup.itemGroup.attrib;
            if (attrib.keys != null) {
                foreach (string key in attrib.keys) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   Tag", GUILayout.Width(90));
                    string newKey = EditorGUILayout.TextField(key);
                    string currentValue = attrib[key];
                    if (!newKey.Equals(key)) {
                        attrib.RenameKey(key, newKey);
                        return true;
                    }
                    GUILayout.Label("Value");
                    string newValue = EditorGUILayout.TextField(currentValue);
                    if (!newValue.Equals(currentValue)) {
                        attrib[key] = newValue;
                        return true;
                    }
                    if (GUILayout.Button("Remove")) {
                        attrib.RemoveField(key);
                        return true;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            // new tag line
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("   Tag", GUILayout.Width(90));
            attribGroup.newTagKey = EditorGUILayout.TextField(attribGroup.newTagKey);
            GUILayout.Label("Value");
            attribGroup.newTagValue = EditorGUILayout.TextField(attribGroup.newTagValue);
            if (GUILayout.Button("Add") && attribGroup.newTagKey.Length > 0) {
                if (!attrib.HasField(attribGroup.newTagKey)) {
                    attrib[attribGroup.newTagKey] = attribGroup.newTagValue;
                    attribGroup.newTagKey = "";
                    attribGroup.newTagValue = "";
                    return true;
                }
            }
            EditorGUILayout.EndHorizontal();
            return false;
        }


        GUIStyle warningLabelStyle;

        void DrawCenteredLabel(string s) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(s);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void DrawWarningLabel(string s) {
            if (warningLabelStyle == null) {
                warningLabelStyle = new GUIStyle(GUI.skin.label);
            }
            warningLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.46f, 0.9f) : new Color(0.12f, 0.26f, 0.7f);
            ;
            GUILayout.Label(s, warningLabelStyle);
        }

        void DrawEditorProvinceNames() {
            if (_editor.highlightedRegions == null || labelsStyle == null)
                return;
            Transform mapTransform = _map.transform;
            for (int p = 0; p < _editor.highlightedRegions.Count; p++) {
                Region region = _editor.highlightedRegions[p];
                if (region.regionIndex == region.entity.mainRegionIndex) {
                    Vector3 regionCenter = mapTransform.TransformPoint(region.sphereCenter);
                    Handles.Label(regionCenter, region.entity.name, labelsStyle);
                }
            }
        }

        void CheckScale() {
            if (EditorPrefs.HasKey(EDITORPREF_SCALE_WARNED))
                return;
            EditorPrefs.SetBool(EDITORPREF_SCALE_WARNED, true);
            if (_editor.editingCountryFile == EDITING_COUNTRY_FILE.COUNTRY_HIGHDEF && _map.transform.localScale.x < 1000) {
                EditorUtility.DisplayDialog("Tip", "You're now in editor mode. Please note that all editing occurs on the Scene View window while the editor component is active, NOT in the Game View. This means that some objects (like cities) will appear with incorrect sizes in the Game View. They will recover their correct sizes for the Game View window when you close the editor component.\n\nIt's important to increase the scale of the globe gameobject to something like (X=1000,Y=1000,Z=1000) or click on 'Toggle Zoom' button to change it automatically so navigating and making precise selections on the map is easier.\n\nWhen you finish editing the map, remember to set its scale back to the original values (default scale is X=1, Y=1, Z=1) or alternatively click on 'Toggle Zoom' again.", "Ok");
            }
        }

        void CheckEditorStyles() {

            if (labelsStyle == null) {
                labelsStyle = new GUIStyle();
                labelsStyle.normal.textColor = Color.green;
                labelsStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (attribHeaderStyle == null) {
                attribHeaderStyle = new GUIStyle(EditorStyles.foldout);
                attribHeaderStyle.SetFoldoutColor();
                attribHeaderStyle.margin = new RectOffset(12, 0, 0, 0);
            }

            if (editorCaptionLabelStyle == null) {
                editorCaptionLabelStyle = new GUIStyle();
            }
            editorCaptionLabelStyle.normal.textColor = Color.white;

        }

        #endregion




    }

}