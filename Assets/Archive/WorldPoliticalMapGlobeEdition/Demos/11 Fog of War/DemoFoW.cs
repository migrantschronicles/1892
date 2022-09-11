using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM
{
	public class DemoFoW : MonoBehaviour
	{
		WorldMapGlobe map;
		bool enableRotation = true;
		GUIStyle labelStyle, buttonStyle, sliderStyle, sliderThumbStyle;
		float penWidth = 0.02f, penStrength = 0.02f;
		Vector3 lastMousePos;

		void Start ()
		{

			labelStyle = new GUIStyle ();
			labelStyle.normal.textColor = Color.white;
			buttonStyle = new GUIStyle (labelStyle);
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.normal.background = Texture2D.whiteTexture;
			buttonStyle.normal.textColor = Color.white;
			sliderStyle = new GUIStyle ();
			sliderStyle.normal.background = Texture2D.whiteTexture;
			sliderStyle.fixedHeight = 4.0f;
			sliderThumbStyle = new GUIStyle ();
			sliderThumbStyle.normal.background = Resources.Load<Texture2D> ("thumb");
			sliderThumbStyle.overflow = new RectOffset (0, 0, 8, 0);
			sliderThumbStyle.fixedWidth = 20.0f;
			sliderThumbStyle.fixedHeight = 12.0f;


			// setup GUI resizer - only for the demo
			GUIResizer.Init (800, 500); 

			// Get map instance to Globe API methods
			map = WorldMapGlobe.instance;
			map.OnDrag += ClearFoW;

			// Load prefab
			GameObject tower = Resources.Load<GameObject>("Tower/Tower");

			// Colorize some countries
			for (int colorizeIndex =0; colorizeIndex < map.countries.Length; colorizeIndex++) {
				Country country = map.countries[colorizeIndex];
				if (country.continent.Equals ("Europe")) {

					// Color country surface
					Color color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), 0.2f);
					map.ToggleCountrySurface (country.name, true, color);

					// Clear fog around the country
					map.SetFogOfWarAlpha(country, 0, 0.1f);

					// Add a random moving sphere for this country
					GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					obj.transform.SetParent(map.transform, false);
					obj.transform.localScale = Vector3.one * 0.02f;
					obj.transform.localPosition = country.localPosition;
					obj.AddComponent<AnimateSphereAround>();
					// Set a random color for the sphere
					obj.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);

					// Add a tower on the center of the country
					GameObject thisTower = Instantiate(tower);
					map.AddMarker(thisTower, country.localPosition, 0.15f, false, 0,  true);
				}
			}

			// Center on Paris
			map.FlyToCity( map.GetCity("France", "Paris") );

		}
	
		void OnGUI ()
		{
			GUIResizer.AutoResize ();
			GUI.Label (new Rect (10, 10, 350, 30), "Click and drag over the Earth to clear Fog of War", labelStyle);

			enableRotation = GUI.Toggle (new Rect (10, 40, 350, 30), enableRotation, "Enable Earth rotation");
			map.allowUserRotation = enableRotation;

			GUI.Label (new Rect (10, 70, 85, 25), "  Pen Width", labelStyle);
			penWidth = GUI.HorizontalSlider (new Rect (110, 75, 100, 20), penWidth, 0, 0.1f, sliderStyle, sliderThumbStyle);
			
			GUI.Label (new Rect (10, 100, 85, 25), "  Pen Strength", labelStyle);
			penStrength = GUI.HorizontalSlider (new Rect (110, 105, 100, 20), penStrength, 0, 0.2f, sliderStyle, sliderThumbStyle);

			GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.5f);
		
			if (GUI.Button (new Rect (10, 130, 150, 30), "  Reset Fog Of War", buttonStyle)) {
				map.SetFogOfWarAlpha(1);
			}
		}

		void ClearFoW (Vector3 cursorLocation)
		{
			if (enableRotation)
				return;

			if (Input.mousePosition == lastMousePos) return;
			lastMousePos = Input.mousePosition;

			map.SetFogOfWarAlpha (cursorLocation, 0, penWidth, penStrength);
		}





	}
}
