using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	/// <summary>
	/// City scaler. Checks the city icons' size is always appropiate
	/// </summary>
	public class CityScaler : MonoBehaviour {

		const int CITY_SIZE_ON_SCREEN = 10;
		Vector3 lastCamPos, lastPos;
		float lastIconSize;
		float lastCustomSize;

		[NonSerialized]
		public WorldMapGlobe map;

		void Start () {
			if (map == null) {
				Destroy (this);
			} else {
				ScaleCities ();
			}
		}
	
		// Update is called once per frame
		void Update () {
			if (map == null || lastPos == transform.position && lastCamPos == map.mainCamera.transform.position && lastIconSize == map.cityIconSize)
				return;
			ScaleCities ();
		}

		public static float GetScale (WorldMapGlobe map) {
			Camera cam = map.mainCamera;
			if (cam == null || cam.pixelWidth == 0)
				return 0;
			float oldFV = cam.fieldOfView;
			if (!UnityEngine.XR.XRSettings.enabled && !map.earthInvertedMode) { 
				cam.fieldOfView = 60.0f;
			}
			Vector3 refPos = map.transform.position;
            if (map.earthInvertedMode) {
                refPos += cam.transform.forward * (map.transform.lossyScale.y * 0.5f); 
            }
			Vector3 a = cam.WorldToScreenPoint (refPos);
			Vector3 b = new Vector3 (a.x, a.y + CITY_SIZE_ON_SCREEN * map.cityIconSize, a.z);
			Vector3 aa = cam.ScreenToWorldPoint (a);
			Vector3 bb = cam.ScreenToWorldPoint (b);
			if (!UnityEngine.XR.XRSettings.enabled) {
				cam.fieldOfView = oldFV;
			}
			float scale = (aa - bb).magnitude / map.transform.localScale.y;
			return Mathf.Clamp (scale, 0.00001f, 0.005f);
		}

		public void ScaleCities () {
			if (map == null)
				return;
			Camera cam = map.mainCamera;
			if (cam == null || cam.pixelWidth == 0)
				return;
			lastPos = transform.position;
			lastCamPos = cam.transform.position;
			lastIconSize = map.cityIconSize;
			float oldFV = cam.fieldOfView;
			if (!UnityEngine.XR.XRSettings.enabled && !map.earthInvertedMode) {
				cam.fieldOfView = 60.0f;
			}
			Vector3 refPos = transform.position;
			if (map.earthInvertedMode)
				refPos += cam.transform.forward * (map.transform.lossyScale.y * 0.5f); ; // otherwise, transform.position = 0 in inverted mode
            Vector3 a = cam.WorldToScreenPoint (refPos);
			Vector3 b = new Vector3 (a.x, a.y + CITY_SIZE_ON_SCREEN * map.cityIconSize, a.z);
			Vector3 aa = cam.ScreenToWorldPoint (a);
			Vector3 bb = cam.ScreenToWorldPoint (b);
			if (!UnityEngine.XR.XRSettings.enabled) {
				cam.fieldOfView = oldFV;
			}
			float scale = (aa - bb).magnitude / map.transform.localScale.y; // * map.cityIconSize;
			scale = Mathf.Clamp (scale, 0.00001f, 0.005f);
			Vector3 newScale = new Vector3 (scale, scale, 1.0f);
			ScaleCities (newScale);
		}

		public void ScaleCities (float customSize) {
			customSize = Mathf.Clamp (customSize, 0, 0.005f);
			if (customSize == lastCustomSize)
				return;
			lastCustomSize = customSize;
			Vector3 newScale = new Vector3 (customSize, customSize, 1);
			ScaleCities (newScale);
		}

		void ScaleCities (Vector3 newScale) {
			Transform normalCities = transform.Find ("Normal Cities");
			if (normalCities != null) {
				foreach (Transform t in normalCities)
					t.localScale = newScale;
			}
			Transform regionCapitals = transform.Find ("Region Capitals");
			if (regionCapitals != null) {
				foreach (Transform t in regionCapitals)
					t.localScale = newScale * 1.75f;
			}
			Transform countryCapitals = transform.Find ("Country Capitals");
			if (countryCapitals != null) {
				foreach (Transform t in countryCapitals)
					t.localScale = newScale * 2.0f;
			}
		}
	}

}