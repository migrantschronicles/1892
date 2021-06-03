using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

	[ExecuteInEditMode]
	public class WorldMapTilesDownloader : EditorWindow {

		const int MAX_CONCURRENT_DOWNLOADS = 128;
		const int ONE_MEGABYTE = 1024 * 1024;

		int concurrentDownloads = 16;
		int zoomLevelMin = WorldMapGlobe.TILE_MIN_ZOOM_LEVEL;
		int zoomLevelMax = WorldMapGlobe.TILE_MIN_ZOOM_LEVEL + 1;

		enum RestrictMode {
			FullWorld = 0,
			Restricted = 1
		}

		struct DownloadSlot {
			public CustomWWW www;
			public bool busy;
			public int retries;
			public int x, y, zoomLevel;
			public string path;
		}

		enum DownloadStatus {
			Idle = 0,
			Estimating = 1,
			Downloading = 2
		}

		RestrictMode worldArea = RestrictMode.FullWorld;
		float latMin = -10;
		float lonMin = -20;
		float latMax = 10;
		float lonMax = 20;
		Texture2D worldTex;
		Vector4 worldRect;
		Color32[] worldColors, tmp;
		int tw, th;
		DownloadStatus status;
		DownloadSlot[] downloads;
		WorldMapGlobe map;
		int x, y, zoomLevel;
		long numTiles, downloadedTilesCount, downloadedTilesSize;
		float storageSize;
		long estimationDownloads, estimationTotalSize;
		int bytesDownloaded;
		int xmin, ymin, xmax, ymax;
		TileInfo ti;
		int progressTileCount;
		int countryIndex;
		string[] countryNames;


		string cachePath {
			get {
				return map.tileResourcePathBase + "/Tiles/" + (int)map.tileServer;
			}
		}

		public static void ShowWindow () {
			int w = 450;
			int h = 540;
			Rect rect = new Rect (Screen.currentResolution.width / 2 - w / 2, Screen.currentResolution.height / 2 - h / 2, w, h);
			GetWindowWithRect<WorldMapTilesDownloader> (rect, true, "Tiles Downloader", true);
		}

		void OnEnable () {
			Texture2D tex = Resources.Load<Texture2D> ("WorldMapArea");
			if (tex != null) {
				worldColors = tex.GetPixels32 ();
				tmp = new Color32[worldColors.Length];
				tw = tex.width;
				th = tex.height;
				worldTex = new Texture2D (tw, th, TextureFormat.RGBA32, false);
			}
			downloads = new DownloadSlot[MAX_CONCURRENT_DOWNLOADS];
			map = WorldMapGlobe.instance;
			countryNames = map.GetCountryNames (true);
			ti = new TileInfo (0, 0, 0, 0, null);
			numTiles = GetNumberOfTiles ();
			GetDownloadedTilesCount ();
		}


		void OnGUI () {
			if (map == null) {
				Close ();
				EditorGUIUtility.ExitGUI ();
				return;
			}

			EditorGUILayout.HelpBox ("Download tiles and bundle them with application. Please check server license information for terms of use of tiles.", MessageType.Info);
			EditorGUILayout.Separator ();
			EditorGUILayout.HelpBox ("Using current server: " + map.tileServer.ToString () + "\nResources path: " + cachePath, MessageType.Info);

			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Zoom Level Min", GUILayout.Width (120));
			zoomLevelMin = EditorGUILayout.IntSlider (zoomLevelMin, 0, WorldMapGlobe.TILE_MAX_ZOOM_LEVEL);
			EditorGUILayout.EndHorizontal ();
			if (zoomLevelMin < WorldMapGlobe.TILE_MIN_ZOOM_LEVEL) {
				EditorGUILayout.HelpBox ("Zoom levels lower than " + WorldMapGlobe.TILE_MIN_ZOOM_LEVEL + " are not used by Globe.", MessageType.Warning);
			}
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Zoom Level Max", GUILayout.Width (120));
			zoomLevelMax = EditorGUILayout.IntSlider (zoomLevelMax, 0, WorldMapGlobe.TILE_MAX_ZOOM_LEVEL);
			EditorGUILayout.EndHorizontal ();
			if (zoomLevelMax < zoomLevelMin)
				zoomLevelMax = zoomLevelMin;

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Concurrent Downloads", GUILayout.Width (120));
			concurrentDownloads = EditorGUILayout.IntSlider (concurrentDownloads, 1, MAX_CONCURRENT_DOWNLOADS);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("World Area", GUILayout.Width (120));
			worldArea = (RestrictMode)EditorGUILayout.EnumPopup (worldArea);
			EditorGUILayout.EndHorizontal ();

			if (worldArea == RestrictMode.Restricted) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("   Country", GUILayout.Width (120));
				countryIndex = EditorGUILayout.Popup (countryIndex, countryNames);
				if (GUILayout.Button ("Pick LatLon Rect")) {
					PickCountryLatLon ();
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("   Latitude Min", GUILayout.Width (120));
				latMin = EditorGUILayout.Slider (latMin, -90, 90);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("   Longitude Min", GUILayout.Width (120));
				lonMin = EditorGUILayout.Slider (lonMin, -180, 180);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("   Latitude Max", GUILayout.Width (120));
				latMax = EditorGUILayout.Slider (latMax, -90, 90);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("   Longitude Max", GUILayout.Width (120));
				lonMax = EditorGUILayout.Slider (lonMax, -180, 180);
				EditorGUILayout.EndHorizontal ();

				if (latMax < latMin)
					latMax = latMin;
				if (lonMax < lonMin)
					lonMax = lonMin;

				EditorGUILayout.Separator ();
				Rect space = EditorGUILayout.BeginVertical ();
				GUILayout.Space (Mathf.Min (160, EditorGUIUtility.currentViewWidth));
				EditorGUILayout.EndVertical ();
				EditorGUI.DrawPreviewTexture (space, GetWorldTexture (), null, ScaleMode.ScaleToFit);
			}

			if (EditorGUI.EndChangeCheck ()) {
				numTiles = GetNumberOfTiles ();
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Tile Count", GUILayout.Width (120));
			EditorGUILayout.LabelField (numTiles.ToString ());
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Estimated Size", GUILayout.Width (120));
			if (status == DownloadStatus.Estimating) {
				EditorGUILayout.LabelField ("Sampling... " + ((int)storageSize).ToString () + " MB", GUILayout.Width (120));
				if (GUILayout.Button ("Cancel", GUILayout.Width (80))) {
					StopOperation ();
				}
			} else {
				EditorGUILayout.LabelField (storageSize > 0 ? ((int)storageSize / 2).ToString () + "-" + ((int)storageSize + 1).ToString () + " MB" : "-", GUILayout.Width (120));
				if (GUILayout.Button ("Estimate", GUILayout.Width (80))) {
					StartEstimate ();
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Downloaded tiles", GUILayout.Width (120));
			EditorGUILayout.LabelField (downloadedTilesCount.ToString () + " (" + ((float)downloadedTilesSize / ONE_MEGABYTE).ToString ("F2") + " MB)", GUILayout.Width (120));
			GUI.enabled = downloadedTilesCount > 0;
			if (GUILayout.Button ("Delete", GUILayout.Width (80))) {
				if (EditorUtility.DisplayDialog ("Delete Downloaded Tiles", "Are you sure you want to delete " + numTiles + " tiles from " + cachePath + "?", "Yes", "Cancel")) {
					DeleteResourcesTiles ();
				}
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginHorizontal ();
			if (status == DownloadStatus.Downloading) {
				ShowProgressBar (progressTileCount / (float)numTiles);
				if (GUILayout.Button ("Stop", GUILayout.Width (60))) {
					StopOperation ();
				}
			} else {
				EditorGUILayout.HelpBox ("This operation can take some time. Cancel at any time.", MessageType.Warning);
				if (GUILayout.Button ("Start", GUILayout.Width (60))) {
					StartDownload ();
				}
			}
			if (GUILayout.Button ("Close", GUILayout.Width (60))) {
				Close ();
			}
		}

		Texture2D GetWorldTexture () {
			if (worldRect.x == latMin && worldRect.y == lonMin && worldRect.z == latMax && worldRect.w == lonMax || worldColors == null) {
				return worldTex;
			}
			worldRect.x = latMin;
			worldRect.y = lonMin;
			worldRect.z = latMax;
			worldRect.w = lonMax;

			Vector2 uv0 = Conversion.GetUVFromLatLon (latMin, lonMin);
			Vector2 uv1 = Conversion.GetUVFromLatLon (latMax, lonMax);
			int tx0 = (int)(uv0.x * tw);
			int tx1 = (int)(uv1.x * tw);
			int ty0 = (int)(uv0.y * th);
			int ty1 = (int)(uv1.y * th);

			Array.Copy (worldColors, tmp, tmp.Length);
			for (int j = ty0; j <= ty1; j++) {
				for (int k = tx0; k <= tx1; k++) {
					tmp [j * tw + k].g = (byte)(255 - tmp [j * tw + k].g);
				}
			}

			worldTex.SetPixels32 (tmp);
			worldTex.Apply ();
			return worldTex;
		}

		void PickCountryLatLon () {
			string[] s = countryNames [countryIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				int i;
				if (int.TryParse (s [1], out i)) {
					Rect rect = map.countries [i].mainRegion.latlonRect2D;
					latMin = rect.xMin;
					lonMin = rect.yMin;
					latMax = rect.xMax;
					lonMax = rect.yMax;
				}
			}
		}

		void ShowProgressBar (float progress) {
			Rect r = EditorGUILayout.BeginVertical ();
			string text = (progress * 100).ToString ("F2") + "% " + ((float)bytesDownloaded / ONE_MEGABYTE).ToString ("F2") + " MB downloaded";
			EditorGUI.ProgressBar (r, progress, text);
			GUILayout.Space (18);
			EditorGUILayout.EndVertical ();
		}


		void OnInspectorUpdate () {
			switch (status) {
			case DownloadStatus.Downloading:
				Download ();
				break;
			case DownloadStatus.Estimating:
				Estimate ();
				break;
			}
		}

		void StartDownload () {
			if (!Directory.Exists (cachePath)) {
				Directory.CreateDirectory (cachePath);
			}
			zoomLevel = zoomLevelMin;
			GetTileRect (zoomLevel, out xmin, out ymin, out xmax, out ymax);
			y = ymin;
			x = xmin - 1;
			for (int k = 0; k < downloads.Length; k++) {
				downloads [k].busy = false;
				downloads [k].retries = 0;
			}
			progressTileCount = 0;
			status = DownloadStatus.Downloading;
		}

		void StopOperation () {
			status = DownloadStatus.Idle;
			RepaintStats ();
			AssetDatabase.Refresh ();
		}


		void StartEstimate () {
			estimationTotalSize = 0;
			estimationDownloads = 0;
			zoomLevel = zoomLevelMin;
			for (int k = 0; k < downloads.Length; k++) {
				downloads [k].busy = false;
				downloads [k].retries = 0;
			}
			progressTileCount = 0;
			status = DownloadStatus.Estimating;
		}


		void Download () {

			// Manage downloads
			int activeDownloads = 0;
			for (int k = 0; k < concurrentDownloads; k++) {
				if (downloads [k].busy) {
                    CustomWWW www = downloads [k].www;
					if (www.isDone) {
						bool good = string.IsNullOrEmpty (www.error);
						// Check texture consistency
						if (good) {
							Texture2D tex = www.textureNonReadable;
							if (tex == null) {
								good = false;
							} else if (tex.width < 16) {
								good = false;
								DestroyImmediate (tex);
							}
						}
						if (good) {
							// save to resources
							bytesDownloaded += www.bytes.Length;
							File.WriteAllBytes (downloads [k].path, www.bytes);
							downloadedTilesCount++;
							downloadedTilesSize += bytesDownloaded;
							downloads [k].busy = false;
						} else {
							if (downloads [k].retries < 3) {
								downloads [k].retries++;
								Debug.LogError ("Retrying " + downloads [k].retries + " times due error on tile XYZ = " + downloads [k].x + "/" + downloads [k].y + "/" + downloads [k].zoomLevel + " : " + www.error);
								downloads [k].www = new CustomWWW (www.url, 0);
								activeDownloads++;
							} else {
								downloads [k].busy = false;
							}
						}
						www.Dispose ();
					} else {
						activeDownloads++;
					}
				}
			}

			int iterations = 0;
			for (int k = 0; k < concurrentDownloads; k++) {
				if (!downloads [k].busy) {

					if (activeDownloads >= concurrentDownloads)
						return;

					// Get next tile
					bool remainingTiles = GetNextTile ();

					// Have we finished?
					if (!remainingTiles) {
						if (activeDownloads == 0) {
							StopOperation ();
						}
						return;
					}

					ti.x = x;
					ti.y = y;
					ti.zoomLevel = zoomLevel;
					string url = map.GetTileURL (map.tileServer, ti);
					// Is current tile already in Resources path?
					string tilePath = map.GetTileResourcePath (x, y, zoomLevel);
					if (File.Exists (tilePath)) {
						if (++iterations < 128) {
							k--;
						}
					} else {
						downloads [k].busy = true;
						downloads [k].retries = 0;
						downloads [k].x = x;
						downloads [k].y = y;
						downloads [k].zoomLevel = zoomLevel;
						downloads [k].path = tilePath;
						downloads [k].www = new CustomWWW (url, 0);
						activeDownloads++;
					}
				}
			}
		}


		void Estimate () {

			// Manage downloads
			int activeDownloads = 0;
			for (int k = 0; k < concurrentDownloads; k++) {
				if (downloads [k].busy) {
                    CustomWWW www = downloads [k].www;
					if (www.isDone) {
						bool good = string.IsNullOrEmpty (www.error);
						// Check texture consistency
						if (good) {
							Texture2D tex = www.textureNonReadable;
							if (tex == null) {
								good = false;
							} else if (tex.width < 16) {
								good = false;
								DestroyImmediate (tex);
							}
						}
						if (good) {
							// save to resources
							estimationTotalSize += www.bytesDownloaded * www.bytesDownloaded;
							estimationDownloads++;
							storageSize = numTiles * Mathf.Sqrt (estimationTotalSize / estimationDownloads) / ONE_MEGABYTE;
							downloads [k].busy = false;
						} else {
							if (downloads [k].retries < 3) {
								downloads [k].retries++;
								Debug.LogError ("Retrying " + downloads [k].retries + " times due error on tile XYZ = " + downloads [k].x + "/" + downloads [k].y + "/" + downloads [k].zoomLevel + " : " + www.error);
								downloads [k].www = new CustomWWW (www.url, 0);
								activeDownloads++;
							} else {
								downloads [k].busy = false;
							}
						}
						www.Dispose ();
					} else {
						activeDownloads++;
					}
				}
			}

			for (int k = 0; k < concurrentDownloads; k++) {
				if (!downloads [k].busy) {

					if (activeDownloads >= concurrentDownloads)
						return;

					// Get next tile
					bool remainingTiles = GetNextTileRandom ();

					// Have we finished?
					if (!remainingTiles) {
						if (activeDownloads == 0) {
							StopOperation ();
						}
						return;
					}

					ti.x = x;
					ti.y = y;
					ti.zoomLevel = zoomLevel;
					string url = map.GetTileURL (map.tileServer, ti);
					downloads [k].busy = true;
					downloads [k].retries = 0;
					downloads [k].x = x;
					downloads [k].y = y;
					downloads [k].zoomLevel = zoomLevel;
					downloads [k].www = new CustomWWW (url, 0);
					activeDownloads++;
				}
			}
		}

		void RepaintStats () {
			GetDownloadedTilesCount ();
			Repaint ();
		}


		int GetNumberOfTiles () {
			// Check if current tile is within restricted area
			int count = 0;
			for (int zl = zoomLevelMin; zl <= zoomLevelMax; zl++) {
				if (worldArea == RestrictMode.Restricted) {
					int x0, y0, x1, y1;
					GetTileRect (zl, out x0, out y0, out x1, out y1);
					count += (y1 - y0 + 1) * (x1 - x0 + 1);
				} else {
					count += (int)Mathf.Pow (4, zl);
				}
			}
			return count;
		}


		void GetDownloadedTilesCount () {
			if (Directory.Exists (cachePath)) {
				string[] files = Directory.GetFiles (cachePath, "*.png");
				downloadedTilesCount = files.Length;
				downloadedTilesSize = 0;
				foreach (string file in files) {
					FileInfo finfo = new FileInfo (file);
					downloadedTilesSize += finfo.Length;
				}
			} else {
				downloadedTilesCount = 0;
				downloadedTilesSize = 0;
			}
		}

		void GetTileRect (int zoomLevel, out int x0, out int y0, out int x1, out int y1) {
			if (worldArea == RestrictMode.Restricted) {
				Conversion.GetTileFromLatLon (zoomLevel, latMin, lonMin, out x0, out y1);
				Conversion.GetTileFromLatLon (zoomLevel, latMax, lonMax, out x1, out y0);
			} else {
				x0 = y0 = 0;
				x1 = y1 = (int)Mathf.Pow (2, zoomLevel) - 1;
			}
		}

		bool GetNextTile () {

			x++;
			if (x > xmax) {
				x = xmin;
				y++;
				if (y > ymax) {
					if (zoomLevel >= zoomLevelMax) {
						x = xmax;
						return false;
					} else {
						zoomLevel++;
						GetTileRect (zoomLevel, out xmin, out ymin, out xmax, out ymax);
						y = ymin;
						x = xmin;
					}
				}
			}
			progressTileCount++;
			if (progressTileCount % 10 == 0) {
				RepaintStats ();
			}
			return true;
		}

		bool GetNextTileRandom () {
			GetTileRect (zoomLevel, out xmin, out ymin, out xmax, out ymax);
			x = UnityEngine.Random.Range (xmin, xmax + 1);
			y = UnityEngine.Random.Range (ymin, ymax + 1);
			progressTileCount++;
			if (progressTileCount % 10 == 0) {
				RepaintStats ();
			}

			int t = (int)Mathf.Pow (4, zoomLevel);
			int q = t < 64 ? t : 64;
			if (progressTileCount > q) {
				if (zoomLevel <= zoomLevelMax) {
					zoomLevel++;
				} else {
					return false;
				}
			}
			return true;
		}

		void DeleteResourcesTiles () {
			if (Directory.Exists (cachePath)) {
				Directory.Delete (cachePath, true);
			}
			if (File.Exists (cachePath + ".meta")) {
				File.Delete (cachePath + ".meta");
			}
			AssetDatabase.Refresh ();
			RepaintStats ();
		}

	}

}