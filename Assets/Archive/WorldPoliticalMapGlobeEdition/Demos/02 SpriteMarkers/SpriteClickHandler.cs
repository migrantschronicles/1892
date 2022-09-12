using UnityEngine;

namespace WPM {

	public class SpriteClickHandler : MonoBehaviour {

		public WorldMapGlobe map;

		void Start () {
			if (GetComponent<Collider2D> () == null)
				gameObject.AddComponent<BoxCollider2D> ();
			if (map == null)
				map = WorldMapGlobe.instance;
		}

		void OnMouseDown () {
			Debug.Log ("Mouse down on sprite!");
		}

		void OnMouseUp () {
			Debug.Log ("Mouse up on sprite!");

			int countryIndex = map.countryLastClicked;
			if (countryIndex >= 0) {
				Debug.Log ("Clicked on " + map.countries [countryIndex].name);
			}
		}

		void OnMouseEnter () {
			Debug.Log ("Mouse over the sprite!");
		}

		void OnMouseExit () {
			Debug.Log ("Mouse exited the sprite!");
		}

	}
}
