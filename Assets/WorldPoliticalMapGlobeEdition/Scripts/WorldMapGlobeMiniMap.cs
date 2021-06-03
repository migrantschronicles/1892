using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WPM {

	[RequireComponent (typeof(RectTransform))]
	public class WorldMapGlobeMiniMap : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler {

		[Tooltip ("Enable to specify a different zoom level when using the minimap.")]
		public bool customZoomLevel;

		[Tooltip ("Zoom Level to apply when clicking on the minimap")]
		public float zoomLevel = 0.5f;

		[Tooltip ("Enable to specify a different navigation duration when using the minimap.")]
		public bool customDuration;

		[Tooltip ("Duration of navigation")]
		public float duration = 2f;

		public RectTransform horizontalLine, verticalLine;

		/// <summary>
		/// The reference to the Globe asset
		/// </summary>
		public WorldMapGlobe map;
		RectTransform rt;
		bool focus;
		Vector3 lastPosition;

		void Start () {
			if (map == null) {
				map = WorldMapGlobe.instance;
			}
			rt = GetComponent<RectTransform> ();

			SetCursorVisibility (false);

			// Check Event System existence
			if (EventSystem.current == null) {
				GameObject o = new GameObject ("EventSystem");
				o.AddComponent<EventSystem> ();
				o.AddComponent<StandaloneInputModule> ();
			}
		}


		void Update () {
			if (focus && lastPosition != Input.mousePosition) {
				lastPosition = Input.mousePosition;
				UpdateCursor (lastPosition);
			}
		}


		public void OnPointerEnter (PointerEventData dat) {
			focus = true;
		}


		public void OnPointerClick (PointerEventData dat) {

			Vector2 localCursor;
			Vector2 pos = dat.position;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (rt, pos, null, out localCursor))
				return;

			localCursor.x -= rt.rect.xMin;
			localCursor.y -= rt.rect.yMin;

			localCursor.x /= rt.rect.width;
			localCursor.y /= rt.rect.height;

			Vector2 latlon = Conversion.GetLatLonFromUV (localCursor);

			if (customZoomLevel && customDuration) {
				map.FlyToLocation (latlon, duration, zoomLevel);
			} else if (customZoomLevel) {
				map.FlyToLocation (latlon, map.navigationTime, zoomLevel);
			} else if (customDuration) {
				map.FlyToLocation (latlon, duration);
			} else {
				map.FlyToLocation (latlon);
			}
		}

		public void OnPointerExit (PointerEventData eventData) {
			focus = false;
			SetCursorVisibility (false);
		}


		void UpdateCursor (Vector3 screenPosition) {
			Vector2 localCursor;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (rt, screenPosition, null, out localCursor))
				return;

			SetCursorVisibility (true);
			if (horizontalLine != null) {
				horizontalLine.localPosition = new Vector2 (horizontalLine.localPosition.x, localCursor.y);
			}
			if (verticalLine != null) {
				verticalLine.localPosition = new Vector2 (localCursor.x, verticalLine.localPosition.y);
			}
		}


		void SetCursorVisibility (bool visible) {
			if (horizontalLine != null) {
				horizontalLine.gameObject.SetActive (visible);
			}
			if (verticalLine != null) {
				verticalLine.gameObject.SetActive (visible);
			}
		}


	}

}