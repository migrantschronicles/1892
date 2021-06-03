//#define DEBUG_TILES
using UnityEngine;
using System;
using System.Linq;
using System.Text;

#if UNITY_WSA && !UNITY_EDITOR
using System.Threading.Tasks;

#else
using System.Threading;
#endif
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        class ZoomLevelInfo {
            public int xMax, yMax;
            public GameObject tilesContainer;
            public int zoomLevelHash, yHash;
        }

        /// <summary>
        /// This is the minimum zoom level for tiles download. TILE_MIN_ZOOM_LEVEL must stay at 5. A lower value produces tiles that are too big to adapt to the sphere shape resulting in intersection artifacts with higher zoom levels.
        /// </summary>
        public const int TILE_MIN_ZOOM_LEVEL = 5;

        /// <summary>
        /// TILE_MAX_ZOOM_LEVEL can be increased if needed
        /// </summary>
        public const int TILE_MAX_ZOOM_LEVEL = 19;

        const int TILE_MIN_SIZE = 256;
        const string PREFIX_MIN_ZOOM_LEVEL = "z5_";
        const float TILE_MAX_QUEUE_TIME = 10;
        const string TILES_ROOT = "Tiles";
        int[] tileIndices = { 2, 3, 0, 2, 0, 1 };
        Vector2[] tileUV = {
            Misc.Vector2up,
            Misc.Vector2one,
            Misc.Vector2right,
            Misc.Vector2zero
        };

        Color[][] meshColors = {
            new Color[] {
                new Color (1, 0, 0, 0),
                new Color (1, 0, 0, 0),
                new Color (1, 0, 0, 0),
                new Color (1, 0, 0, 0)
            },
            new Color[] {
                new Color (0, 1, 0, 0),
                new Color (0, 1, 0, 0),
                new Color (0, 1, 0, 0),
                new Color (0, 1, 0, 0)
            },
            new Color[] {
                new Color (0, 0, 1, 0),
                new Color (0, 0, 1, 0),
                new Color (0, 0, 1, 0),
                new Color (0, 0, 1, 0)
            },
            new Color[] {
                new Color (0, 0, 0, 1),
                new Color (0, 0, 0, 1),
                new Color (0, 0, 0, 1),
                new Color (0, 0, 0, 1)
            }
        };
        Vector2[] offsets = {
            new Vector2 (0, 0),
            new Vector2 (1f, 0),
            new Vector2 (0, 1f),
            new Vector2 (1f, 1f)
        };

        Vector4[] placeHolderUV = {
            new Vector4 (0, 0.5f, 0.5f, 1f),
            new Vector4 (0.5f, 0.5f, 1f, 1f),
            new Vector4 (0, 0f, 0.5f, 0.5f),
            new Vector4 (0.5f, 0, 1f, 0.5f)
        };

        Color color1000 = new Color(1, 0, 0, 0);
        int _concurrentLoads;
        int _currentZoomLevel;
        int _webDownloads, _cacheLoads, _resourceLoads;
        long _webDownloadTotalSize, _cacheLoadTotalSize;
        int _tileSize = 0;
        string _tileServerCopyrightNotice;
        string _tileLastError;
        DateTime _tileLastErrorDate;
        Dictionary<int, TileInfo> cachedTiles;
        ZoomLevelInfo[] zoomLevelsInfo = new ZoomLevelInfo[TILE_MAX_ZOOM_LEVEL + 1];
        List<TileInfo> loadQueue, newQueue;
        List<TileInfo> inactiveTiles;
        bool shouldCheckTiles, resortLoadQueue;
        Material tileMatRef, tileMatTransRef;
        Transform tilesRoot;
        Renderer northPoleObj, southPoleObj;
        int subserverSeq;
        long _tileCurrentCacheUsage;
        FileInfo[] cachedFiles;
        float currentTileSize;
        Plane[] cameraPlanes;
        float lastDisposalTime;
        Texture2D currentEarthTexture;
        int spreadLoadAmongFrames;
        Camera currentCamera;
        Vector3 currentCameraPosition, currentCameraForward, globePos;
        string cachePath;
        float localScaleFactor;
        bool firstLoad, requestPaintMainTiles;
        Vector2 currentLatLon;
        bool purgedThisSession;

        /// <summary>
        /// List of tiles currently visible when map is in Tile System Mode
        /// </summary>
        [NonSerialized]
        public List<TileInfo> visibleTiles = new List<TileInfo>();

        void InitTileSystem() {
            _tileServerCopyrightNotice = GetTileServerCopyrightNotice(_tileServer);

            cachePath = Application.persistentDataPath + "/TilesCache";
            if (!Directory.Exists(cachePath)) {
                Directory.CreateDirectory(cachePath);
            }

            if (!Application.isPlaying)
                return;

            if (_tileTransparentLayer) {
                tileMatRef = Resources.Load<Material>("Materials/TileOverlayAlpha") as Material;
                tileMatTransRef = Resources.Load<Material>("Materials/TileOverlayTransAlpha") as Material;
                Color alpha = new Color(1f, 1f, 1f, _tileMaxAlpha);
                tileMatRef.color = alpha;
                tileMatTransRef.color = alpha;
            } else {
                tileMatRef = Resources.Load<Material>("Materials/TileOverlay") as Material;
                tileMatTransRef = Resources.Load<Material>("Materials/TileOverlayTrans") as Material;
            }
            cameraPlanes = new Plane[6];

            if (_earthRenderer != null && _earthRenderer.sharedMaterial != null) {
                currentEarthTexture = (Texture2D)_earthRenderer.sharedMaterial.mainTexture;
            } else {
                currentEarthTexture = Texture2D.whiteTexture;
            }

            _tileSize = 0;

            InitZoomLevels();
            if (loadQueue != null) {
                loadQueue.Clear();
            } else {
                loadQueue = new List<TileInfo>();
            }
            if (inactiveTiles != null) {
                inactiveTiles.Clear();
            } else {
                inactiveTiles = new List<TileInfo>();
            }
            if (cachedTiles != null) {
                cachedTiles.Clear();
            } else {
                cachedTiles = new Dictionary<int, TileInfo>();
            }

            if (Application.isPlaying && !purgedThisSession) {
                purgedThisSession = true;
                PurgeCacheOldFiles();
            }

            if (tilesRoot == null) {
                tilesRoot = transform.Find(TILES_ROOT);
            }
            if (tilesRoot != null)
                DestroyImmediate(tilesRoot.gameObject);
            if (tilesRoot == null) {
                GameObject tilesRootObj = new GameObject(TILES_ROOT);
                tilesRoot = tilesRootObj.transform;
                tilesRoot.SetParent(transform, false);
            }
            shouldCheckTiles = true;
            firstLoad = true;

        }

        void DestroyTiles() {
            if (tilesRoot != null)
                DestroyImmediate(tilesRoot.gameObject);
            if (northPoleObj != null) {
                DestroyImmediate(northPoleObj);
                northPoleObj = null;
            }
            if (southPoleObj != null) {
                DestroyImmediate(southPoleObj);
                southPoleObj = null;
            }
            if (cachedTiles != null) {
                foreach (KeyValuePair<int, TileInfo> kvp in cachedTiles) {
                    TileInfo ti = kvp.Value;
                    if (ti != null && ti.texture != null && ti.source != TILE_SOURCE.Resources && ti.source != TILE_SOURCE.Unknown && ti.texture != currentEarthTexture) {
                        DestroyImmediate(ti.texture);
                        ti.texture = null;
                    }
                }
                cachedTiles.Clear();
            }
        }

        /// <summary>
        /// Reloads tiles
        /// </summary>
        public void ResetTiles() {
            DestroyTiles();
            InitTileSystem();
        }

        void PurgeCacheOldFiles() {
            PurgeCacheOldFiles(_tileMaxLocalCacheSize);
        }

        public void TileRecalculateCacheUsage() {
            _tileCurrentCacheUsage = 0;
            if (!Directory.Exists(cachePath))
                return;
            DirectoryInfo dir = new DirectoryInfo(cachePath);
            cachedFiles = dir.GetFiles().OrderBy(p => p.LastAccessTime).ToArray();
            for (int k = 0; k < cachedFiles.Length; k++) {
                _tileCurrentCacheUsage += cachedFiles[k].Length;
            }
        }

        /// <summary>
        /// Purges the cache old files.
        /// </summary>
        /// <param name="maxSize">Max size is in Mb.</param>
        void PurgeCacheOldFiles(long maxSize) {
            _tileCurrentCacheUsage = 0;
            if (!Directory.Exists(cachePath))
                return;
            DirectoryInfo dir = new DirectoryInfo(cachePath);
            // Delete old jpg files
            FileInfo[] jpgs = dir.GetFiles("*.jpg.*");
            for (int k = 0; k < jpgs.Length; k++) {
                jpgs[k].Delete();
            }

            cachedFiles = dir.GetFiles().OrderBy(p => p.LastAccessTime).ToArray();
            maxSize *= 1024 * 1024;
            for (int k = 0; k < cachedFiles.Length; k++) {
                _tileCurrentCacheUsage += cachedFiles[k].Length;
            }
            if (_tileCurrentCacheUsage <= maxSize)
                return;

            // Purge files until total size gets under max cache size
            for (int k = 0; k < cachedFiles.Length; k++) {
                if (_tilePreloadTiles && cachedFiles[k].Name.StartsWith(PREFIX_MIN_ZOOM_LEVEL)) {
                    continue;
                }
                _tileCurrentCacheUsage -= cachedFiles[k].Length;
                cachedFiles[k].Delete();
                if (_tileCurrentCacheUsage <= maxSize)
                    return;
            }
        }

        void InitZoomLevels() {
            for (int k = 0; k < zoomLevelsInfo.Length; k++) {
                ZoomLevelInfo zi = new ZoomLevelInfo();
                zi.xMax = (int)Mathf.Pow(2, k);
                zi.yMax = zi.xMax;
                zi.zoomLevelHash = (int)Mathf.Pow(4, k);
                zi.yHash = (int)Mathf.Pow(2, k);
                zoomLevelsInfo[k] = zi;
            }
        }

        void LateUpdateTiles() {
            if (!Application.isPlaying || cachedTiles == null)
                return;

            if (_tilesUnloadInactiveTiles && Time.time - lastDisposalTime > 3) {
                lastDisposalTime = Time.time;
                MonitorInactiveTiles();
            }

            if (shouldCheckTiles || flyToActive) {

                shouldCheckTiles = false;
                currentCamera = mainCamera; // for optimization purposes
                currentCameraPosition = currentCamera.transform.position;
                currentCameraForward = currentCamera.transform.forward;
                globePos = transform.position;
                currentLatLon = Conversion.GetLatLonFromSpherePoint(GetCurrentMapLocation());

                _currentZoomLevel = GetCenterTileZoomLevel();

                ToggleFrontiers();

                int startingZoomLevel = TILE_MIN_ZOOM_LEVEL - 1;
                ZoomLevelInfo zi = zoomLevelsInfo[startingZoomLevel];
                int currentLoadQueueSize = loadQueue.Count;

                for (int k = 0; k < currentLoadQueueSize; k++) {
                    loadQueue[k].visible = false;
                }

                GeometryUtility.CalculateFrustumPlanes(currentCamera.projectionMatrix * currentCamera.worldToCameraMatrix, cameraPlanes);

                visibleTiles.Clear();

                localScaleFactor = transform.localScale.x * 0.01f;
                for (int k = 0; k < zi.xMax; k++) {
                    for (int j = 0; j < zi.yMax; j++) {
                        CheckTiles(null, _currentZoomLevel, k, j, startingZoomLevel, 0);
                    }
                }

                if (OnTileRecomputed != null) {
                    OnTileRecomputed(visibleTiles);
                }

                int newQueueCount = loadQueue.Count;
                if (currentLoadQueueSize != newQueueCount) {
                    resortLoadQueue = true;
                    for (int k = 0; k < newQueueCount; k++) {
                        TileInfo ti = loadQueue[k];
                        Vector3 midPos;
                        midPos.x = (ti.cornerWorldPos[0].x + ti.cornerWorldPos[3].x) * 0.5f;
                        midPos.y = (ti.cornerWorldPos[0].y + ti.cornerWorldPos[3].y) * 0.5f;
                        midPos.z = (ti.cornerWorldPos[0].z + ti.cornerWorldPos[3].z) * 0.5f;
                        ti.distToCamera = FastVector.SqrDistance(ref midPos, ref currentCameraPosition) * ti.zoomLevel;
                        if (!ti.visible) ti.distToCamera += 1e20f;
                    }
                }
                if (resortLoadQueue) {
                    resortLoadQueue = false;
                    loadQueue.Sort((TileInfo x, TileInfo y) => {
                        if (x.distToCamera < y.distToCamera)
                            return -1;
                        else if (x.distToCamera > y.distToCamera)
                            return 1;
                        else
                            return 0;
                    });
                }
                // Ensure local cache max size is not exceeded
                long maxLocalCacheSize = _tileMaxLocalCacheSize * 1024 * 1024;
                if (cachedFiles != null && _tileCurrentCacheUsage > maxLocalCacheSize) {
                    for (int f = 0; f < cachedFiles.Length; f++) {
                        if (cachedFiles[f] != null && cachedFiles[f].Exists) {
                            if (_tilePreloadTiles && cachedFiles[f].Name.StartsWith(PREFIX_MIN_ZOOM_LEVEL)) {
                                continue;
                            }
                            _tileCurrentCacheUsage -= cachedFiles[f].Length;
                            cachedFiles[f].Delete();
                        }
                        if (_tileCurrentCacheUsage <= maxLocalCacheSize)
                            break;
                    }
                }
            }

            requestPaintMainTiles = false;
            CheckTilesContent();

            spreadLoadAmongFrames = _tileMaxTileLoadsPerFrame;

            if (requestPaintMainTiles && firstLoad) {
                firstLoad = false;
                LateUpdateTiles();
            }

            if (_tilePreciseRotation) {
                if (_currentZoomLevel > _tilePreciseRotationZoomLevel && _navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
                    _navigationMode = NAVIGATION_MODE.CAMERA_ROTATES;
                } else if (_currentZoomLevel <= _tilePreciseRotationZoomLevel && _navigationMode == NAVIGATION_MODE.CAMERA_ROTATES) {
                    _navigationMode = NAVIGATION_MODE.EARTH_ROTATES;
                }
            }
        }

        void ToggleFrontiers() {
            // Check if frontiers/provinces can be shown
            if (_showFrontiers) {
                if (_currentZoomLevel < _tileMaxZoomLevelFrontiers) {
                    if (!frontiersLayer.activeSelf) {
                        frontiersLayer.SetActive(true);
                    }
                } else if (_currentZoomLevel > _tileMaxZoomLevelFrontiers) {
                    if (frontiersLayer.activeSelf) {
                        frontiersLayer.SetActive(false);
                    }
                }
            }
            if (_showInlandFrontiers) {
                if (_currentZoomLevel < _tileMaxZoomLevelFrontiers) {
                    if (!inlandFrontiersLayer.activeSelf) {
                        inlandFrontiersLayer.SetActive(true);
                    }
                } else if (_currentZoomLevel > _tileMaxZoomLevelFrontiers) {
                    if (inlandFrontiersLayer.activeSelf) {
                        inlandFrontiersLayer.SetActive(false);
                    }
                }
            }
        }


        void MonitorInactiveTiles() {

            int inactiveCount = inactiveTiles.Count;
            bool changes = false;
            bool releasedMemory = false;
            for (int k = 0; k < inactiveCount; k++) {
                TileInfo ti = inactiveTiles[k];
                if (ti == null) {
                    changes = true;
                    continue;
                }
                if (ti.gameObject == null || ti.visible || ti.texture == currentEarthTexture || ti.loadStatus != TILE_LOAD_STATUS.Loaded) {
                    inactiveTiles[k] = null;
                    ti.isAddedToInactive = false;
                    changes = true;
                    continue;
                }
                if (Time.time - ti.inactiveTime > _tileKeepAlive) {
                    bool relatedTileIsVisible = false;
                    // if a children is visible, do not unload this tile
                    if (ti.children != null) {
                        int cCount = ti.children.Count;
                        for (int c = 0; c < cCount; c++) {
                            TileInfo tiChild = ti.children[c];
                            if (tiChild.visible) {
                                relatedTileIsVisible = true;
                                break;
                            }
                        }
                        if (relatedTileIsVisible) continue;
                    }
                    // if a parent is visible, do not unload this tile
                    TileInfo tiParent = ti.parent;
                    while (tiParent != null) {
                        if (tiParent.visible) {
                            relatedTileIsVisible = true;
                            break;
                        }
                        tiParent = tiParent.parent;
                    }
                    if (relatedTileIsVisible) continue;

                    // Proceed to unload tile
                    inactiveTiles[k] = null;
                    ti.isAddedToInactive = false;
                    ti.loadStatus = TILE_LOAD_STATUS.Inactive;
                    if (ti.source != TILE_SOURCE.Resources && ti.source != TILE_SOURCE.Unknown && ti.texture != currentEarthTexture) {
                        DestroyImmediate(ti.texture);
                        releasedMemory = true;
                    }
                    // tile is now invisible, setup material for when it appears again:
                    ti.Reset();
                    ResolvePlaceholderImage(ti.parent);

                    // Reset parentcoords on children
                    if (ti.children != null) {
                        ResolvePlaceholderImage(ti);
                    }

                    changes = true;
                }
            }
            if (changes) {
                shouldCheckTiles = true;
                List<TileInfo> newInactiveList = new List<TileInfo>();
                for (int k = 0; k < inactiveCount; k++) {
                    if (inactiveTiles[k] != null)
                        newInactiveList.Add(inactiveTiles[k]);
                }
                inactiveTiles.Clear();
                inactiveTiles = newInactiveList;
                if (releasedMemory) {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                }
            }
        }


        void CheckTilesContent() {
            int qCount = loadQueue.Count;
            bool cleanQueue = false;
            for (int k = 0; k < qCount; k++) {
                TileInfo ti = loadQueue[k];
                if (ti == null) {
                    cleanQueue = true;
                    continue;
                }
                if (ti.loadStatus == TILE_LOAD_STATUS.InQueue) {
                    if (ti.loadDelay > 0) {
                        ti.stage = 10;
                        ti.loadDelay--;
                        continue;
                    }
                    if (ti.visible) {
                        if (_tilePreloadTiles && ti.zoomLevel == TILE_MIN_ZOOM_LEVEL && ReloadTextureFromCacheOrMarkForDownload(ti)) {
                            loadQueue[k] = null;
                            cleanQueue = true;
                            requestPaintMainTiles = true;
                            continue;
                        }
                        if (_concurrentLoads <= _tileMaxConcurrentDownloads) {
                            ti.loadStatus = TILE_LOAD_STATUS.Loading;
                            ti.stage = 20;
                            _concurrentLoads++;
                            StartCoroutine(LoadTileContentBackground(ti));
                        }
                    } else if (Time.time - ti.queueTime > TILE_MAX_QUEUE_TIME) {
                        ti.loadStatus = TILE_LOAD_STATUS.Inactive;
                        loadQueue[k] = null;
                        cleanQueue = true;
                    }
                }
            }

            if (cleanQueue) {
                if (newQueue == null) {
                    newQueue = new List<TileInfo>(qCount);
                } else {
                    newQueue.Clear();
                }
                for (int k = 0; k < qCount; k++) {
                    TileInfo ti = loadQueue[k];
                    if (ti != null) {
                        newQueue.Add(ti);
                    }
                }
                List<TileInfo> tmp = loadQueue;
                loadQueue = newQueue;
                newQueue = tmp;
            }
        }

        void CheckTiles(TileInfo parent, int currentZoomLevel, int xTile, int yTile, int zoomLevel, int subquadIndex) {
            // Is this tile visible?
            TileInfo ti;

            int tileCode = GetTileHashCode(xTile, yTile, zoomLevel);
            if (!cachedTiles.TryGetValue(tileCode, out ti)) {
                ti = new TileInfo(xTile, yTile, zoomLevel, subquadIndex, currentEarthTexture);
                ti.parent = parent;
                if (parent != null) {
                    if (parent.children == null) {
                        parent.children = new List<TileInfo>();
                    }
                    parent.children.Add(ti);
                }
                for (int k = 0; k < 4; k++) {
                    float xt = xTile + offsets[k].x;
                    float yt = yTile + offsets[k].y;
                    ti.latlons[k] = Conversion.GetLatLonFromTile(xt, yt, zoomLevel);
                }
                // Defult UVs wrt Earth texture
                Vector2 latLonTL = ti.latlons[0];
                Vector2 latLonBR = ti.latlons[3];
                Vector2 uv_tl = new Vector2((latLonTL.y + 180) / 360f, (latLonTL.x + 90) / 180f);
                Vector2 uv_br = new Vector2((latLonBR.y + 180) / 360f, (latLonBR.x + 90) / 180f);
                if (uv_tl.x > 0.5f && uv_br.x < 0.5f) {
                    uv_br.x = 1f;
                }
                ti.worldTextureCoords = new Vector4(uv_tl.x, uv_br.y, uv_br.x, uv_tl.y);
                for (int k = 0; k < 4; k++) {
                    Vector3 spherePos = Conversion.GetSpherePointFromLatLon(ti.latlons[k]);
                    ti.spherePos[k] = spherePos;
                }
                cachedTiles[tileCode] = ti;
            }

#if DEBUG_TILES
            if (ti.gameObject != null) {
                if (ti.gameObject.GetComponent<TileInfoEx>().debug) {
                    int jj = 9; // can put a break point here to debug this tile
                    jj++;
                }
            }
#endif

            // Check if any tile corner is visible
            // Phase I
            Vector3 tmp = Misc.Vector3zero;
            bool cornersOccluded = true;
            Vector3 minWorldPos = Misc.Vector3Max;
            Vector3 maxWorldPos = Misc.Vector3Min;
            float globeRadiusSqr = radius * radius;

            for (int c = 0; c < 4; c++) {
                Vector3 wpos = transform.TransformPoint(ti.spherePos[c]);
                ti.cornerWorldPos[c] = wpos;
                if (wpos.x < minWorldPos.x)
                    minWorldPos.x = wpos.x;
                if (wpos.y < minWorldPos.y)
                    minWorldPos.y = wpos.y;
                if (wpos.z < minWorldPos.z)
                    minWorldPos.z = wpos.z;
                if (wpos.x > maxWorldPos.x)
                    maxWorldPos.x = wpos.x;
                if (wpos.y > maxWorldPos.y)
                    maxWorldPos.y = wpos.y;
                if (wpos.z > maxWorldPos.z)
                    maxWorldPos.z = wpos.z;
                if (cornersOccluded) {
                    FastVector.NormalizedDirection(ref wpos, ref currentCameraPosition, ref tmp);
                    Vector3 st = wpos;
                    FastVector.Add(ref st, ref tmp, localScaleFactor);
                    float magSqr = (st.x - globePos.x) * (st.x - globePos.x) + (st.y - globePos.y) * (st.y - globePos.y) + (st.z - globePos.z) * (st.z - globePos.z);
                    if (magSqr > globeRadiusSqr) {
                        cornersOccluded = false;
                    }
                }
            }

            FastVector.Average(ref minWorldPos, ref maxWorldPos, ref tmp);
            Vector3 tileMidPoint = tmp;
            // Check center of quad
            if (cornersOccluded) {
                Vector2 latLonTL = ti.latlons[0];
                Vector2 latLonBR = ti.latlons[3];
                if (currentLatLon.x >= latLonBR.x && currentLatLon.x <= latLonTL.x && currentLatLon.y >= latLonTL.y && currentLatLon.y <= latLonBR.y) {
                    cornersOccluded = false;
                }
            }

            bool insideViewport = false;
            Rect camRect = currentCamera.pixelRect;
            float minX = Screen.width, minY = Screen.height;
            float maxX = 0, maxY = 0;
            if (!cornersOccluded) {
                // Phase II
                float screenRectMinX = camRect.xMin;
                float screenRectMaxX = camRect.xMax;
                float screenRectMinY = camRect.yMin;
                float screenRectMaxY = camRect.yMax;

                for (int c = 0; c < 4; c++) {
                    Vector3 scrPos = currentCamera.WorldToScreenPoint(ti.cornerWorldPos[c]);
                    insideViewport = insideViewport || (scrPos.z > 0 && scrPos.x >= screenRectMinX && scrPos.x < screenRectMaxX && scrPos.y >= screenRectMinY && scrPos.y < screenRectMaxY);
                    if (scrPos.x < minX)
                        minX = scrPos.x;
                    if (scrPos.x > maxX)
                        maxX = scrPos.x;
                    if (scrPos.y < minY)
                        minY = scrPos.y;
                    if (scrPos.y > maxY)
                        maxY = scrPos.y;
                }
                if (!insideViewport) {
                    Vector3 boundsSize = maxWorldPos - minWorldPos;
                    // Add a bit margin. Due to projection, smaller tiles could be a bit outside the area of the parent so we add a bit margin to ensure that
                    // if we disable a parent, we do only when it and all its children are outside of viewport
                    boundsSize.x *= 1.02f;
                    boundsSize.y *= 1.02f;
                    boundsSize.z *= 1.02f;
                    Bounds bounds = new Bounds(tileMidPoint, boundsSize);
                    insideViewport = GeometryUtility.TestPlanesAABB(cameraPlanes, bounds);
                }
            }

            // Ensure the screen bounds are not degenerated (ie. a quad almost on its side which barely results visible)
            if (insideViewport) {
                if (maxY - minY < 2 || maxX - minX < 2) {
                    insideViewport = false;
                }
            }

            ti.insideViewport = insideViewport;
            ti.visible = false;
            bool tileIsBig = false;

            if (insideViewport) {
                if (!ti.created) {
                    CreateTileQuad(ti);
                }

                if (!ti.gameObject.activeSelf) {
                    ti.gameObject.SetActive(true);
                }

                // Manage hierarchy of tiles
                FastVector.NormalizedDirection(ref globePos, ref tileMidPoint, ref tmp);
                float dd = Vector3.Dot(currentCameraForward, tmp);
                if (dd > -0.8f || currentZoomLevel > 9) { // prevents big seams on initial zooms
                    float aparentSize = Mathf.Max(maxX - minX, maxY - minY);
                    tileIsBig = aparentSize > currentTileSize;
                } else {
                    tileIsBig = ti.zoomLevel < currentZoomLevel;
                }

                if ((tileIsBig || zoomLevel < TILE_MIN_ZOOM_LEVEL) && zoomLevel < _tileMaxZoomLevel) {
                    // Load nested tiles
                    CheckTiles(ti, currentZoomLevel, xTile * 2, yTile * 2, zoomLevel + 1, 0);
                    CheckTiles(ti, currentZoomLevel, xTile * 2 + 1, yTile * 2, zoomLevel + 1, 1);
                    CheckTiles(ti, currentZoomLevel, xTile * 2, yTile * 2 + 1, zoomLevel + 1, 2);
                    CheckTiles(ti, currentZoomLevel, xTile * 2 + 1, yTile * 2 + 1, zoomLevel + 1, 3);
                    ti.StopAnimation();
                    ti.renderer.enabled = false;
                } else {
                    ti.visible = true;

                    visibleTiles.Add(ti);

                    // Show tile renderer
                    if (!ti.renderer.enabled) {
                        if (ti.loadStatus != TILE_LOAD_STATUS.Loaded) {
                            ti.renderer.sharedMaterial = ti.parent.transMat;
                            ti.SetAlpha(0);
                        }
                        ti.renderer.enabled = true;

                        if (OnTileBecameVisible != null) {
                            OnTileBecameVisible(ti);
                        }
                    }

                    // If tile is not loaded yet and parent tile is loaded then use that as placeholder texture
                    if (ti.zoomLevel > TILE_MIN_ZOOM_LEVEL && !ti.parent.placeholderImageSet) {
                        ResolvePlaceholderImage(ti.parent);
                    }

                    if (ti.loadStatus == TILE_LOAD_STATUS.Loaded) {
                        if (!ti.hasAnimated) {
                            ti.hasAnimated = true;
                            if (_tilePreloadTiles && ti.zoomLevel <= TILE_MIN_ZOOM_LEVEL) {
                                ti.Animate(0, AnimationEnded);
                            } else {
                                ti.Animate(1f, AnimationEnded);
                            }
                        }
                    } else if (ti.loadStatus == TILE_LOAD_STATUS.Inactive) {
                        ti.loadStatus = TILE_LOAD_STATUS.InQueue;
                        ti.queueTime = Time.time;
                        loadQueue.Add(ti);
                    }
                    if (ti.children != null) {
                        int count = ti.children.Count;
                        for (int k = 0; k < count; k++) {
                            TileInfo tiChild = ti.children[k];
                            if (tiChild.gameObject != null && tiChild.gameObject.activeSelf) {
                                if (ti.isAnimating || !tiChild.renderer.enabled || tiChild.loadStatus != TILE_LOAD_STATUS.Loaded || ti.texture == currentEarthTexture) {
                                    HideTile(tiChild);
                                } else if (!tiChild.isAnimating) {
                                    tiChild.Animate(1f, EndChildFadeOut, true);
                                }
                            }
                        }
                    }
                }
            } else {
                HideTile(ti);
            }

#if DEBUG_TILES
            if (ti.gameObject != null) {
                TileInfoEx info = ti.gameObject.GetComponent<TileInfoEx>();
                info.bigTile = tileIsBig;
                info.loadStatus = ti.loadStatus;
                info.visible = ti.visible;
                info.zoomLevel = ti.zoomLevel;
                info.placeholderImageSet = ti.parent != null ? ti.parent.placeholderImageSet : false;
                info.lastFrameUsed = Time.frameCount;
                info.material = ti.renderer.sharedMaterial;
                if (ti.parent != null) {
                    info.parentTexture = ti.parent.texture;
                }
            }
#endif
        }


        /// <summary>
        /// Finds a parent with a texture and updates parentTex and parentCoords for all children 
        /// </summary>
        void ResolvePlaceholderImage(TileInfo parent) {

            if (parent == null) return;

            int childrenCount = parent.children.Count;

            // First, if parent is loaded, simple update children
            if (parent.zoomLevel >= TILE_MIN_ZOOM_LEVEL && parent.loadStatus == TILE_LOAD_STATUS.Loaded) {
                parent.placeholderImageSet = true;
                for (int k = 0; k < childrenCount; k++) {
                    TileInfo child = parent.children[k];
                    child.parentTextureCoords = placeHolderUV[child.subquadIndex];
                    child.SetPlaceholderImage(parent.texture);
                }
                return;
            }


            // Parent is not loaded, find a grand parent
            TileInfo grandParent = parent;
            while (grandParent.loadStatus != TILE_LOAD_STATUS.Loaded) {
                grandParent = grandParent.parent;
            }

            // Annotate that this placeholder is temporary (set placeholderImageSet to false)
            parent.placeholderImageSet = false;

            if (grandParent.zoomLevel < TILE_MIN_ZOOM_LEVEL) {
                // Reset to Earth texture uvs
                for (int k = 0; k < childrenCount; k++) {
                    TileInfo child = parent.children[k];
                    child.parentTextureCoords = child.worldTextureCoords;
                    child.SetPlaceholderImage(grandParent.texture);
                }
            } else {
                // Compute subquad uvs
                float uv_width = grandParent.worldTextureCoords.z - grandParent.worldTextureCoords.x;
                float uv_height = grandParent.worldTextureCoords.w - grandParent.worldTextureCoords.y;
                for (int k = 0; k < childrenCount; k++) {
                    TileInfo child = parent.children[k];
                    float uv_xmin = (child.worldTextureCoords.x - grandParent.worldTextureCoords.x) / uv_width;
                    float uv_xmax = (child.worldTextureCoords.z - grandParent.worldTextureCoords.x) / uv_width;
                    float uv_ymin = (child.worldTextureCoords.y - grandParent.worldTextureCoords.y) / uv_height;
                    float uv_ymax = (child.worldTextureCoords.w - grandParent.worldTextureCoords.y) / uv_height;
                    child.parentTextureCoords = new Vector4(uv_xmin, uv_ymin, uv_xmax, uv_ymax);
                    child.SetPlaceholderImage(grandParent.texture);
                }
            }
        }


#if UNITY_EDITOR
        private void OnGUI() {
            if (!_tilesShowDebugInfo || visibleTiles == null) return;
            GUI.color = Color.red;
            int debugTilesCount = visibleTiles.Count;
            for (int k = 0; k < debugTilesCount; k++) {
                TileInfo ti = visibleTiles[k];
                if (!ti.visible) continue;
                Vector3 pos = (ti.cornerWorldPos[0] + ti.cornerWorldPos[3]) * 0.5f;
                Vector3 scrPos = _mainCamera.WorldToScreenPoint(pos);
                if (scrPos.z < 0) continue;
                scrPos.y = Screen.height - scrPos.y;
                string text = "ZL:" + ti.zoomLevel + " X:" + ti.x + " Y:" + ti.y + " QI:" + ti.subquadIndex + " " + ti.loadStatus.ToString() + " ST:" + ti.stage;
                var textSize = GUI.skin.label.CalcSize(new GUIContent(text));
                scrPos.x -= textSize.x * 0.5f;
                GUI.Label(new Rect(scrPos.x, scrPos.y, textSize.x, textSize.y), text);
            }
        }
#endif

        void HideTile(TileInfo ti) {
            if (ti.gameObject != null && ti.gameObject.activeSelf) {
                ti.gameObject.SetActive(false);
                ti.visible = false;
                ti.renderer.enabled = false;
                if (OnTileBecameInvisible != null) {
                    OnTileBecameInvisible(ti);
                }
                if (ti.loadStatus == TILE_LOAD_STATUS.Loaded && ti.zoomLevel > TILE_MIN_ZOOM_LEVEL) {
                    if (!ti.isAddedToInactive) {
                        ti.isAddedToInactive = true;
                        inactiveTiles.Add(ti);
                    }
                    ti.inactiveTime = Time.time;
                }
            }
        }

        void EndChildFadeOut(TileInfo ti) {
            ti.hasAnimated = false;
            HideTile(ti);
            shouldCheckTiles = true;
        }

        void AnimationEnded(TileInfo ti) {
            shouldCheckTiles = true;
        }

        int GetTileHashCode(int x, int y, int zoomLevel) {
            ZoomLevelInfo zi = zoomLevelsInfo[zoomLevel];
            if (zi == null)
                return 0;
            int xMax = zi.xMax;
            x = (x + xMax) % xMax;
            int hashCode = zi.zoomLevelHash + zi.yHash * y + x;
            return hashCode;
        }

        int GetCenterTileZoomLevel() {
            // Get screen dimensions of central tile
            int zoomLevel0 = 1;
            int zoomLevel1 = TILE_MAX_ZOOM_LEVEL;
            int zoomLevel = TILE_MIN_ZOOM_LEVEL;
            Vector2 latLon = Conversion.GetLatLonFromSpherePoint(GetCurrentMapLocation());
            int xTile, yTile;
            currentTileSize = _tileSize > TILE_MIN_SIZE ? _tileSize : TILE_MIN_SIZE;
            currentTileSize *= 2.25f / _tileResolutionFactor; // (3.0f - _tileResolutionFactor);
            float dist = 0;
            Camera cam = currentCamera;
            Quaternion oldRot = cam.transform.rotation;
            cam.transform.LookAt(transform.position, cam.transform.up);
            for (int i = 0; i < 5; i++) {
                zoomLevel = (zoomLevel0 + zoomLevel1) / 2;
                Conversion.GetTileFromLatLon(zoomLevel, latLon.x, latLon.y, out xTile, out yTile);
                Vector2 latLonTL = Conversion.GetLatLonFromTile(xTile, yTile, zoomLevel);
                Vector2 latLonBR = Conversion.GetLatLonFromTile(xTile + 0.999f, yTile + 0.999f, zoomLevel);
                Vector3 spherePointTL = Conversion.GetSpherePointFromLatLon(latLonTL);
                Vector3 spherePointBR = Conversion.GetSpherePointFromLatLon(latLonBR);
                Vector3 wposTL = cam.WorldToScreenPoint(transform.TransformPoint(spherePointTL));
                Vector3 wposBR = cam.WorldToScreenPoint(transform.TransformPoint(spherePointBR));
                dist = Mathf.Max(Mathf.Abs(wposBR.x - wposTL.x), Mathf.Abs(wposTL.y - wposBR.y));
                if (dist > currentTileSize) {
                    zoomLevel0 = zoomLevel;
                } else {
                    zoomLevel1 = zoomLevel;
                }
            }
            if (dist > currentTileSize) {
                zoomLevel++;
            }

            cam.transform.rotation = oldRot;

            zoomLevel = Mathf.Clamp(zoomLevel, TILE_MIN_ZOOM_LEVEL, TILE_MAX_ZOOM_LEVEL);
            return zoomLevel;
        }

        void CreateTileQuad(TileInfo ti) {
            ZoomLevelInfo zi = zoomLevelsInfo[ti.zoomLevel];

            // Create container
            GameObject parentObj;
            if (ti.parent == null) {
                parentObj = zi.tilesContainer;
                if (parentObj == null) {
                    parentObj = new GameObject("Tiles" + ti.zoomLevel);
                    parentObj.transform.SetParent(tilesRoot, false);
                    zi.tilesContainer = parentObj;
                }
            } else {
                parentObj = ti.parent.gameObject;
            }

            // Prepare mesh vertices
            Vector3[] tileCorners = new Vector3[4];
            tileCorners[0] = ti.spherePos[0];
            tileCorners[1] = ti.spherePos[1];
            tileCorners[2] = ti.spherePos[3];
            tileCorners[3] = ti.spherePos[2];

            // Setup tile materials
            TileInfo parent = ti.parent != null ? ti.parent : ti;
            if (parent.opaqueMat == null) {
                parent.opaqueMat = Instantiate(tileMatRef);
                parent.opaqueMat.hideFlags = HideFlags.DontSave;
#if UNITY_EDITOR
                if (_tilesShowDebugInfo) parent.opaqueMat.EnableKeyword("SHOW_BORDER");
#endif
            }
            if (parent.transMat == null) {
                parent.transMat = Instantiate(tileMatTransRef);
                parent.transMat.hideFlags = HideFlags.DontSave;
#if UNITY_EDITOR
                if (_tilesShowDebugInfo) parent.transMat.EnableKeyword("SHOW_BORDER");
#endif
            }

            Material tileMat = ti.zoomLevel < TILE_MIN_ZOOM_LEVEL ? parent.opaqueMat : parent.transMat;

            if (ti.zoomLevel < TILE_MIN_ZOOM_LEVEL) {
                ti.loadStatus = TILE_LOAD_STATUS.Loaded;
            }
            if (ti.zoomLevel <= TILE_MIN_ZOOM_LEVEL) {
                ti.ClearPlaceholderImage();
            } else {
                ResolvePlaceholderImage(ti.parent);
            }
            ti.texture = currentEarthTexture;
            ti.source = TILE_SOURCE.Resources;
            ti.renderer = CreateGameObject(parentObj.transform, "Tile", tileCorners, tileIndices, tileUV, tileMat, ti.subquadIndex);
            ti.gameObject = ti.renderer.gameObject;
            ti.renderer.enabled = false;
            ti.created = true;

#if DEBUG_TILES
            ti.gameObject.AddComponent<TileInfoEx>();
#endif

            if (OnTileCreated != null) {
                OnTileCreated(ti);
            }

        }

        internal IEnumerator LoadTileContentBackground(TileInfo ti) {
            yield return new WaitForEndOfFrame();

            string url = GetTileURL(_tileServer, ti);
            if (string.IsNullOrEmpty(url)) {
                _concurrentLoads--;
                Debug.LogError("Tile server url not set. Aborting");
                yield break;
            }

            ti.stage = 30;
            long downloadedBytes = 0;
            string error = null;
            string filePath = "";
            byte[] textureBytes = null;
            ti.source = TILE_SOURCE.Unknown;

            // Check if tile is given by external event
            if (OnTileRequest != null) {
                if (OnTileRequest(ti.zoomLevel, ti.x, ti.y, out ti.texture, out error) && ti.texture != null) {
                    ti.source = TILE_SOURCE.Resources;
                }
            }

            // Check if tile is in Resources
            if (ti.source == TILE_SOURCE.Unknown && _tileEnableOfflineTiles) {
                string path = GetTileResourcePath(ti.x, ti.y, ti.zoomLevel, false);
                ResourceRequest request = Resources.LoadAsync<Texture2D>(path);
                yield return request;
                if (request.asset != null) {
                    ti.texture = (Texture2D)request.asset;
                    ti.source = TILE_SOURCE.Resources;
                } else if (tileOfflineTilesOnly) {
                    ti.texture = tileResourceFallbackTexture;
                    ti.source = TILE_SOURCE.Resources;
                }
            }

            CustomWWW www = null;
            if (ti.source == TILE_SOURCE.Unknown) {
                ti.stage = 40;
                www = getCachedWWW(url, ti);
                yield return www;
                if (www.isDone) {
                    error = www.error;
                }
            }

            ti.stage = 50;
            for (int tries = 0; tries < 100; tries++) {
                if (spreadLoadAmongFrames > 0)
                    break;
                yield return new WaitForEndOfFrame();
            }
            spreadLoadAmongFrames--;
            _concurrentLoads--;

            ti.stage = 70;
            if (!string.IsNullOrEmpty(error)) {
                _tileLastError = "Error getting tile z:" + ti.zoomLevel + " x:" + ti.x + " " + ti.y + ": " + error + " url=" + url + " max timeout:" + _tileDownloadTimeout;
                _tileLastErrorDate = DateTime.Now;
                if (_tileDebugErrors) {
                    Debug.Log(_tileLastErrorDate + " " + _tileLastError);
                }
                ti.loadStatus = TILE_LOAD_STATUS.InQueue;
                ti.loadDelay += 10;
                yield break;
            }

            // Load texture
            ti.stage = 80;
            if (ti.source != TILE_SOURCE.Resources) {
                downloadedBytes = www.bytesDownloaded;
                textureBytes = www.bytes;
                ti.texture = www.textureNonReadable;
                www.Dispose();
                www = null;

                // Check texture consistency
                if (ti.loadedFromCache || _tileEnableLocalCache) {
                    filePath = GetLocalFilePathForURL(url, ti);
                }

                if (ti.loadedFromCache && ti.texture.width <= 16) { // Invalid texture in local cache, retry
                    if (File.Exists(filePath)) {
                        File.Delete(filePath);
                    }
                    ti.loadStatus = TILE_LOAD_STATUS.Inactive;
                    ti.queueTime = Time.time;
                    yield break;
                }
            }

            ti.texture.wrapMode = TextureWrapMode.Clamp;
            _tileSize = ti.texture.width;

            // Save texture
            ti.stage = 90;
            if (_tileEnableLocalCache && ti.source != TILE_SOURCE.Resources && !File.Exists(filePath)) {
                _tileCurrentCacheUsage += textureBytes.Length;
                BackgroundSaver saver = new BackgroundSaver(textureBytes, filePath);
                saver.Start();
            }

            // Update stats
            switch (ti.source) {
                case TILE_SOURCE.Cache:
                    _cacheLoads++;
                    _cacheLoadTotalSize += downloadedBytes;
                    break;
                case TILE_SOURCE.Resources:
                    _resourceLoads++;
                    break;
                default:
                    _webDownloads++;
                    _webDownloadTotalSize += downloadedBytes;
                    break;
            }

            if (loadQueue.Contains(ti)) {
                loadQueue.Remove(ti);
            }

            ti.stage = 95;
            FinishLoadingTile(ti);
        }

        void CreatePole(TileInfo ti) {

            Vector3 polePos;
            Vector3 latLon0;
            string name;
            bool isNorth = (ti.y == 0);
            if (isNorth) {
                if (northPoleObj != null)
                    return;
                polePos = Misc.Vector3up * 0.5f;
                latLon0 = ti.latlons[0];
                name = "North Pole";
            } else {
                if (southPoleObj != null)
                    return;
                polePos = Misc.Vector3down * 0.5f;
                latLon0 = ti.latlons[2];
                name = "South Pole";
            }
            Vector3 latLon3 = latLon0;
            float lonDX = 360f / zoomLevelsInfo[ti.zoomLevel].xMax;
            latLon3.y += lonDX;
            int steps = (int)(360f / lonDX);
            int vertexCount = steps * 3;
            List<Vector3> vertices = new List<Vector3>(vertexCount);
            List<int> indices = new List<int>(vertexCount);
            List<Vector2> uv = new List<Vector2>(vertexCount);
            for (int k = 0; k < steps; k++) {
                Vector3 p0 = Conversion.GetSpherePointFromLatLon(latLon0);
                Vector3 p1 = Conversion.GetSpherePointFromLatLon(latLon3);
                latLon0 = latLon3;
                latLon3.y += lonDX;
                vertices.Add(p0);
                vertices.Add(p1);
                vertices.Add(polePos);
                indices.Add(k * 3);
                if (isNorth) {
                    indices.Add(k * 3 + 2);
                    indices.Add(k * 3 + 1);
                } else {
                    indices.Add(k * 3 + 1);
                    indices.Add(k * 3 + 2);
                }
                uv.Add(Misc.Vector2zero);
                uv.Add(Misc.Vector2up);
                uv.Add(Misc.Vector2right);
            }
            Renderer obj = CreateGameObject(tilesRoot.transform, name, vertices.ToArray(), indices.ToArray(), uv.ToArray(), ti.parent.opaqueMat, 0);
            if (isNorth) {
                northPoleObj = obj;
            } else {
                southPoleObj = obj;
            }
        }

        Renderer CreateGameObject(Transform parent, string name, Vector3[] vertices, int[] indices, Vector2[] uv, Material mat, int subquadIndex) {
            GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            obj.layer = parent.gameObject.layer;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Misc.Vector3zero;
            obj.transform.localScale = Misc.Vector3one;
            obj.transform.localRotation = Misc.QuaternionZero;
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.uv = uv;
            Color[] meshColor;
            if (vertices.Length != 4) {
                meshColor = new Color[vertices.Length];
                for (int k = 0; k < vertices.Length; k++)
                    meshColor[k] = color1000;
            } else {
                meshColor = meshColors[subquadIndex];
            }
            mesh.colors = meshColor;
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            return mr;
        }

        class BackgroundSaver {
            byte[] tex;
            string filePath;

            public BackgroundSaver(byte[] tex, string filePath) {
                this.tex = tex;
                this.filePath = filePath;
            }

            public void Start() {
#if UNITY_WSA && !UNITY_EDITOR
                Task.Run(() => SaveTextureToCache());
#elif UNITY_WEBGL
				SaveTextureToCache();
#else
                Thread thread = new Thread(SaveTextureToCache);
                thread.Start();
#endif
            }

            void SaveTextureToCache() {
                File.WriteAllBytes(filePath, tex);
            }

        }

        StringBuilder filePathStr = new StringBuilder(250);

        string GetLocalFilePathForURL(string url, TileInfo ti) {
            filePathStr.Length = 0;
            filePathStr.Append(cachePath);
            filePathStr.Append("/z");
            filePathStr.Append(ti.zoomLevel);
            filePathStr.Append("_x");
            filePathStr.Append(ti.x);
            filePathStr.Append("_y");
            filePathStr.Append(ti.y);
            filePathStr.Append("_");
            filePathStr.Append(url.GetHashCode());
            filePathStr.Append(".png");
            return filePathStr.ToString();
        }


        public string GetTileResourcePath(int x, int y, int zoomLevel, bool fullPath = true) {
            filePathStr.Length = 0;
            if (fullPath) {
                filePathStr.Append(_tileResourcePathBase);
                filePathStr.Append("/");
            }
            filePathStr.Append("Tiles");
            filePathStr.Append("/");
            filePathStr.Append((int)_tileServer);
            filePathStr.Append("/z");
            filePathStr.Append(zoomLevel);
            filePathStr.Append("_x");
            filePathStr.Append(x);
            filePathStr.Append("_y");
            filePathStr.Append(y);
            if (fullPath) {
                filePathStr.Append(".png");
            }
            return filePathStr.ToString();
        }


        CustomWWW getCachedWWW(string url, TileInfo ti) {
            string filePath = GetLocalFilePathForURL(url, ti);
            CustomWWW www;
            bool useCached = false;
            useCached = _tileEnableLocalCache && System.IO.File.Exists(filePath);
            if (useCached) {
                if (!_tilePreloadTiles || !filePath.Contains(PREFIX_MIN_ZOOM_LEVEL)) {
                    //check how old
                    System.DateTime written = File.GetLastWriteTimeUtc(filePath);
                    System.DateTime now = System.DateTime.UtcNow;
                    double totalHours = now.Subtract(written).TotalHours;
                    if (totalHours > 300) {
                        File.Delete(filePath);
                        useCached = false;
                    }
                }
            }
            ti.source = useCached ? TILE_SOURCE.Cache : TILE_SOURCE.Online;
            if (useCached) {
#if UNITY_STANDALONE_WIN || UNITY_WSA
				string pathforwww = "file:///" + filePath;
#else
                string pathforwww = "file://" + filePath;
#endif
                www = new CustomWWW(pathforwww, 0);
            } else {
                www = new CustomWWW(url, _tileDownloadTimeout);
            }
            return www;
        }


        bool ReloadTextureFromCacheOrMarkForDownload(TileInfo ti) {
            if (!_tileEnableLocalCache)
                return false;

            string url = GetTileURL(_tileServer, ti);
            if (string.IsNullOrEmpty(url)) {
                return false;
            }

            string filePath = GetLocalFilePathForURL(url, ti);
            if (System.IO.File.Exists(filePath)) {
                //check how old
                if (!_tilePreloadTiles || ti.zoomLevel != TILE_MIN_ZOOM_LEVEL) {
                    System.DateTime written = File.GetLastWriteTimeUtc(filePath);
                    System.DateTime now = System.DateTime.UtcNow;
                    double totalHours = now.Subtract(written).TotalHours;
                    if (totalHours > 300) {
                        File.Delete(filePath);
                        return false;
                    }
                }
            } else {
                return false;
            }
            byte[] bb = System.IO.File.ReadAllBytes(filePath);
            ti.texture = new Texture2D(0, 0);
            ti.texture.LoadImage(bb);
            if (ti.texture.width <= 16) { // Invalid texture in local cache, retry
                if (File.Exists(filePath)) {
                    File.Delete(filePath);
                }
                return false;
            }
            ti.texture.wrapMode = TextureWrapMode.Clamp;

            _cacheLoads++;
            _cacheLoadTotalSize += bb.Length;

            FinishLoadingTile(ti);
            return true;
        }

        void FinishLoadingTile(TileInfo ti) {

            // Good to go, update tile info
            ti.SetTexture(ti.texture);

            ti.loadStatus = TILE_LOAD_STATUS.Loaded;
            if (ti.zoomLevel >= TILE_MIN_ZOOM_LEVEL) {
                if (ti.y == 0 || ti.y == zoomLevelsInfo[ti.zoomLevel].yMax - 1) {
                    CreatePole(ti);
                }
            }

            // Notify children of new placeholder 
            if (ti.children != null) {
                int childCount = ti.children.Count;
                for (int k = 0; k < childCount; k++) {
                    TileInfo tiChild = ti.children[k];
                    if (tiChild != null) {
                        tiChild.placeholderImageSet = false;
                    }
                }
            }

            shouldCheckTiles = true;
            ti.stage = 99;

        }



    }

}