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

	/* Public WPM Class */
	public partial class WorldMapGlobe : MonoBehaviour {

		static WorldMapGlobe _instance;

		/// <summary>
		/// Instance of the world map. Use this property to access World Map functionality.
		/// </summary>
		public static WorldMapGlobe instance {
			get {
				if (_instance == null) {
					_instance = FindObjectOfType<WorldMapGlobe> ();
					if (_instance == null) {
						Debug.LogWarning ("'WorldMapGlobe' GameObject could not be found in the scene. Make sure it's created with this name before using any map functionality.");
					}
				}
				return _instance;
			}
		}

		int _surfacesCount;

		/// <summary>
		/// Returns number of visible (active) colorized surfaces.
		/// </summary>
		public int surfacesCount { get { return _surfacesCount; } }

        [SerializeField]
        string _geodataResourcesPath = "Geodata";

        /// <summary>
        /// Path where geodata files reside. This path is a relative path below Resources folder. So a geodata file would be read as Resources/<geodataResourcesPath>/cities10 for example.
        /// Note that your project can contain several Resources folders. Create your own Resources folder so you don't have to backup your geodata folder on each update if you make any modifications to the files.
        /// </summary>
        public string geodataResourcesPath {
            get { return _geodataResourcesPath; }
            set {
                if (_geodataResourcesPath != value) {
                    _geodataResourcesPath = value.Trim();
                    if (_geodataResourcesPath.Length < 1) {
                        _geodataResourcesPath = "Geodata";
                    }
                    string lc = _geodataResourcesPath.Substring(_geodataResourcesPath.Length - 1, 1);
                    if (lc.Equals("/") || lc.Equals("\\"))
                        _geodataResourcesPath = _geodataResourcesPath.Substring(0, _geodataResourcesPath.Length - 1);
                    isDirty = true;
                }
            }
        }



        #region Public API area


		[SerializeField]
		Camera
			_mainCamera;

		public Camera mainCamera {
			get {
				if (_mainCamera == null) {
					_mainCamera = Camera.main;
				}
				return _mainCamera;
			}
			set {
				if (_mainCamera != value) {
					_mainCamera = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		int _overlayLayerIndex;

		/// <summary>
		/// The layer index used for overlay stuff (markers, labels, ...). Can be used to apply selective bloom on those objects using Beautify.
		/// </summary>
		/// <value>The index of the overlay layer.</value>
		public int overlayLayerIndex {
			get {
				return _overlayLayerIndex;
			}
			set {
				if (_overlayLayerIndex != value) {
					_overlayLayerIndex = value;
					DestroyOverlay ();
					Redraw ();
				}
			}
		}


		/// <summary>
		/// Returns the overlay base layer (parent gameObject), useful to overlay stuff on the map that needs to be overlayed (ie. flat icons or labels). It will be created if it doesn't exist.
		/// </summary>
		/// <param name="needRenderTexture">True if the overlay layer contains blended elements like country names not rendering in world space or tickers in blended mode</param>
		public GameObject GetOverlayLayer (bool createIfNotExists, bool requireRenderTexture) {
			if (createIfNotExists && requireRenderTexture && _labelsQuality == LABELS_QUALITY.NotUsed) {
				DestroyOverlay ();
				_labelsQuality = LABELS_QUALITY.Medium;
			}
			if (overlayLayer != null && sphereOverlayLayer != null) {
				overlayLayer.transform.localScale = new Vector3 (1.0f / transform.localScale.x, 1.0f / transform.localScale.y, 1.0f / transform.localScale.z);
				if (sphereOverlayLayer != null) {
					sphereOverlayLayer.SetActive (true);
                    Renderer sphereOverlayRenderer = sphereOverlayLayer.GetComponent<Renderer>();
                    sphereOverlayRenderer.enabled = _labelsQuality != LABELS_QUALITY.NotUsed;
				}
				return overlayLayer;
			} else if (createIfNotExists) {
				DestroyOverlay ();
				return CreateOverlay ();
			} else {
				return null;
			}
		}

		/// <summary>
		/// Destroys all cached and visible region surfaces.
		/// </summary>
		public void DestroySurfaces () {
			InitSurfacesCache ();
		}

		#if !UNITY_WEBPLAYER
		public Texture2D BakeTexture (string outputFile) {

			// Get all triangles and its colors
			Texture2D texture;
			if (_earthStyle == EARTH_STYLE.SolidColor) {
				int tw = 2048;
				int th = 1024;
				texture = new Texture2D (tw, th, TextureFormat.RGB24, false);
				Color32[] colors32 = new Color32[tw * th];
				Color32 solidColor = _earthColor;
				for (int k = 0; k < colors32.Length; k++) {
					colors32 [k] = solidColor;
				}
				texture.SetPixels32 (colors32);
				texture.Apply ();
			} else {
				texture = Instantiate (transform.Find ("WorldMapGlobeEarth").GetComponent<Renderer> ().sharedMaterial.mainTexture) as Texture2D;
			}
			texture.hideFlags = HideFlags.DontSave;
			int width = texture.width;
			int height = texture.height;
			Color[] colors = texture.GetPixels ();

			if (_surfacesLayer != null) {
				Transform[] surfaces = _surfacesLayer.GetComponentsInChildren<Transform> ();
				// Antartica k = 16
				for (int k = 0; k < surfaces.Length; k++) {
					// Get the color
					Color color;
					Renderer rr = surfaces [k].GetComponent<Renderer> ();
					if (rr != null)
						color = rr.sharedMaterial.color;
					else
						continue; // not valid

					// Get triangles and paint over the texture
					MeshFilter mf = surfaces [k].GetComponent<MeshFilter> ();
					if (mf == null || mf.sharedMesh.GetTopology (0) != MeshTopology.Triangles)
						continue;
					Vector3[] vertex = mf.sharedMesh.vertices;
					int[] index = mf.sharedMesh.GetTriangles (0);

					float maxEdge = width * 0.8f;
					float minEdge = width * 0.2f;
					for (int i = 0; i < index.Length; i += 3) {
						Vector2 p1 = Conversion.ConvertToTextureCoordinates (vertex [index [i]], width, height);
						Vector2 p2 = Conversion.ConvertToTextureCoordinates (vertex [index [i + 1]], width, height);
						Vector2 p3 = Conversion.ConvertToTextureCoordinates (vertex [index [i + 2]], width, height);
						// Sort points
						if (p2.x > p3.x) {
							Vector3 p = p2;
							p2 = p3;
							p3 = p;
						}
						if (p1.x > p2.x) {
							Vector3 p = p1;
							p1 = p2;
							p2 = p;
							if (p2.x > p3.x) {
								p = p2;
								p2 = p3;
								p3 = p;
							}
						}
						if (p1.x < minEdge && p2.x < minEdge && p3.x > maxEdge) {
							if (p1.x < 1 && p2.x < 1) {
								p1.x = width - p1.x;
								p2.x = width - p2.x;
							} else
								p3.x = width - p3.x;
						} else if (p1.x < minEdge && p2.x > maxEdge && p3.x > maxEdge) {
							p1.x = width + p1.x;
						} 
						Drawing.DrawTriangle (colors, width, height, p1, p2, p3, color);
					}
				}
				texture.SetPixels (colors);
				texture.Apply ();
			}

			if (File.Exists (outputFile))
				File.Delete (outputFile);
			File.WriteAllBytes (outputFile, texture.EncodeToPNG ());
			return texture;
		}

		/// <summary>
		/// Hides globe in the scene. This method works faster than disabling the game object.
		/// </summary>
		public void Hide () {
			ToggleGlobalVisibility (false);
		}

		/// <summary>
		/// Shows globe in the scene after hiding it with Hide() method. This method works faster than enabling the game object.
		/// </summary>
		public void Show () {
			ToggleGlobalVisibility (true);
		}
		#endif



		/// <summary>
		/// Enables Calculator component and returns a reference to its API.
		/// </summary>
		public WorldMapCalculator calc { get { return GetComponent<WorldMapCalculator> () ?? gameObject.AddComponent<WorldMapCalculator> (); } }

		/// <summary>
		/// Enables Ticker component and returns a reference to its API.
		/// </summary>
		public WorldMapTicker ticker { get { return GetComponent<WorldMapTicker> () ?? gameObject.AddComponent<WorldMapTicker> (); } }

		/// <summary>
		/// Enables Decorator component and returns a reference to its API.
		/// </summary>
		public WorldMapDecorator decorator { get { return GetComponent<WorldMapDecorator> () ?? gameObject.AddComponent<WorldMapDecorator> (); } }

		/// <summary>
		/// Enables Editor component and returns a reference to its API.
		/// </summary>
		public WorldMapEditor editor { get { return GetComponent<WorldMapEditor> () ?? gameObject.AddComponent<WorldMapEditor> (); } }

		#endregion


	}

}