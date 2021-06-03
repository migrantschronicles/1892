using UnityEngine;
using System.Collections;

namespace WPM {
	public class AddMarkerScript : MonoBehaviour {
		GameObject destinationSprite;

		void Start () {
			float latitude = 40.71f;
			float longitude = -74f;

			WorldMapGlobe map = WorldMapGlobe.instance;
			Vector3 sphereLocation = Conversion.GetSpherePointFromLatLon (latitude, longitude);

			// Create sprite
			destinationSprite = new GameObject ();                                 
			SpriteRenderer dest_sprite = destinationSprite.AddComponent<SpriteRenderer> ();                      
			dest_sprite.sprite = Resources.Load<Sprite> ("NewYork");

			// Add sprite billboard to the map with custom scale, billboard mode and little bit elevated from surface (to prevent clipping with city spots)
			map.AddMarker (destinationSprite, sphereLocation, 0.02f, true, 0.1f);

			// Add click handlers
			destinationSprite.AddComponent<SpriteClickHandler> ();

			// Locate it on the map
			map.FlyToLocation (sphereLocation, 4f, 0.4f);
			map.autoRotationSpeed = 0f;
		}

		void Update () {
			destinationSprite.transform.Rotate (new Vector3 (0, 0, 5f * Time.deltaTime), Space.Self);
		}

	}
}
