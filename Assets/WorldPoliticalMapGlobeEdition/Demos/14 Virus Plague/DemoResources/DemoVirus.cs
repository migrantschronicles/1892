using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	
	public class DemoVirus : MonoBehaviour {

		const int MAX_BOUNCES = 8;

		public Material circleMat, combineMat;
		public Texture2D earthMask;

		WorldMapGlobe map;
		GUIStyle buttonStyle;
		RenderTexture[] rtEarth, rtVirusMap, rtCombined;
		EarthTexture[] earthTextures;
		Material earthMat;
		int bounces;

		void Start () {
			buttonStyle = new GUIStyle ();
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.normal.background = Texture2D.whiteTexture;
			buttonStyle.normal.textColor = Color.white;

			// setup GUI resizer - only for the demo
			GUIResizer.Init (800, 500); 

			// Get map instance to Globe API methods
			map = WorldMapGlobe.instance;
			earthMat = map.earthMaterial;

			// Gets a copy of current Earth texture and make it available as render texture for faster blending
			earthTextures = map.earthTextures;

			int numTextures = earthTextures.Length;
			int width = earthTextures [0].texture.width;
			int height = earthTextures [0].texture.height;
			rtEarth = new RenderTexture[numTextures];
			rtVirusMap = new RenderTexture[numTextures];
			rtCombined = new RenderTexture[numTextures];
			for (int k = 0; k < numTextures; k++) {
				rtEarth [k] = new RenderTexture (width, height, 0);
				Graphics.Blit (earthTextures [k].texture, rtEarth [k]);
				rtVirusMap [k] = new RenderTexture (width, height, 0);
				rtCombined [k] = new RenderTexture (width, height, 0);
			}

			circleMat.SetTexture ("_MaskTex", earthMask); // to avoid painting over Sea

			map.OnClick += (Vector3 sphereLocation, int mouseButtonIndex) => {
				StartCoroutine (SpreadAtLatLon (Conversion.GetLatLonFromSpherePoint (sphereLocation), 0.005f, 1f));
			};

			StartPlague ();
		}

		void OnGUI () {
			GUIResizer.AutoResize ();
			GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.5f);
			if (GUI.Button (new Rect (10, 10, 160, 30), "  Start Plague Again", buttonStyle)) {
				StartPlague ();
			}
			GUI.Label (new Rect (20, 50, 300, 30), "Click to spread plague at cursor position");
		}

		void StartPlague () {
			for (int k = 0; k < rtVirusMap.Length; k++) {
				rtVirusMap [k].Clear (false, true, Misc.ColorTransparent);
			}

			// Get a random city
			int cityRandom = Random.Range (0, map.cities.Count);
			Vector2 pointZero = map.cities [cityRandom].latlon;
			Spread (cityRandom);
		}



		void Spread (int cityIndex) {
			City city = map.cities [cityIndex];
			Vector2 latlon = city.latlon;
			float radius = 0.002f;
			switch (city.cityClass) {
			case CITY_CLASS.REGION_CAPITAL:
				radius += 0.004f;
				break;
			case CITY_CLASS.COUNTRY_CAPITAL:
				radius += 0.007f;
				break;
			}
			radius += Random.value * 0.005f;

			StartCoroutine (SpreadAtLatLon (latlon, radius, 4f));
		}

		IEnumerator SpreadAtLatLon (Vector2 latlon, float radius, float duration) {
			
			WaitForEndOfFrame w = new WaitForEndOfFrame ();
			Vector2 uv = Conversion.GetUVFromLatLon (latlon.x, latlon.y);
			float startTime = Time.time;
			float t = 0;
			Material circleInstancedMat = Instantiate (circleMat); // copy of material to allow different _UVRect per circle

			do {
				t = (Time.time - startTime) / duration;
				if (t > 1f)
					t = 1f;
				for (int k = 0; k < earthTextures.Length; k++) {
					Vector4 uvRect = earthTextures [k].uvRect;
					if (uv.x + radius >= uvRect.x && uv.x - radius <= uvRect.x + uvRect.z && uv.y + radius >= uvRect.y && uv.y - radius <= uvRect.y + uvRect.w) {
						circleInstancedMat.SetVector ("_UVRect", uvRect);
						rtVirusMap [k].Circle (uv, t * radius, circleInstancedMat);
					}
				}
				yield return w;

				if (Random.value > 0.97f && bounces < MAX_BOUNCES) {
					Bounce (latlon);
				}
			} while(t < 1f);
			Bounce (latlon);
			bounces--;
		}

		void Bounce (Vector2 latlonStart) {
			// Spread to another near city
			int anotherCity = 0;
			float minDist = float.MaxValue;
			for (int k = 0; k < 25; k++) {
				int c = Random.Range (0, map.cities.Count);
				float dist = map.calc.Distance (latlonStart, map.cities [c].latlon);
				if (dist < minDist) {
					anotherCity = c;
					minDist = dist;
				}
			}
			Vector2 dest = map.cities [anotherCity].latlon;
			LineMarkerAnimator line = map.AddLine (latlonStart, dest, Color.yellow, 0.1f, 2f, 0.05f, 0.1f);
			line.OnLineDrawingEnd += (LineMarkerAnimator lma) => {
				Spread (anotherCity);
			};
			bounces++;

		}


		void Update () {
			// Combine virus map with Earth texture
			for (int k = 0; k < rtVirusMap.Length; k++) {
				combineMat.SetTexture ("_SecondTex", rtVirusMap [k]);
				Graphics.Blit (rtEarth [k], rtCombined [k], combineMat);
				earthMat.SetTexture (earthTextures [k].shaderTextureName, rtCombined [k]);
			}
		}
	}


	public static class RenderTextureExtensions {

		public static void Clear (this RenderTexture rt, bool clearDepth, bool clearColor, Color backgroundColor) {
			RenderTexture old = RenderTexture.active;
			RenderTexture.active = rt;
			GL.Clear (clearDepth, clearColor, backgroundColor);
			RenderTexture.active = old;
		}

		public static void Circle (this RenderTexture rt, Vector2 uv, float radius, Material mat) {
			RenderTexture old = RenderTexture.active;
			RenderTexture.active = rt;
			mat.SetVector ("_CenterAndRadius", new Vector3 (uv.x, uv.y, radius * radius));
			Graphics.Blit (null, rt, mat);
			RenderTexture.active = old;
		}
	}
}
