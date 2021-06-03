//#define VR_STEAM	// Uncomment this line to add support for SteamVR controller, then add this script to the Controller (left) or (right) or both

#if VR_STEAM
using UnityEngine;
using System.Collections;

namespace WPM
{
	public class SteamVRControllerWrapper : MonoBehaviour
	{

		public WorldMapGlobe map;
		SteamVR_TrackedObject trackedObj;
		Vector2 lastTouchPadPos;

		private SteamVR_Controller.Device controller {
			get { return SteamVR_Controller.Input ((int)trackedObj.index); }
		}

		void Start ()
		{
			trackedObj = GetComponent<SteamVR_TrackedObject> ();
			if (map==null) map = WorldMapGlobe.instance;
			if (map!=null) map.VREnabled = true;
		}

		void Update ()
		{

			if (map == null || controller == null)
				return;

			// Enable click on map selections
			if (controller.GetHairTriggerDown ()) {
				map.SetSimulatedMouseButtonClick (0);
			}

			// Enable dragging and zooming on map
			bool trigger = controller.GetHairTrigger ();
			if (trigger) {
				map.SetSimulatedMouseButtonPressed (0);
			}

			// Touch pad can be used for rotating the globe or zooming (if trigger is pressed)
			Vector2 touchPos = controller.GetAxis ();
			Vector2 diff = touchPos - lastTouchPadPos;
			if (diff != Vector2.zero) {
				lastTouchPadPos = touchPos;
				if (trigger) {
					// zoom in/out
					float curZoom = (float)map.GetZoomLevel ();
					curZoom += diff.y * map.mouseWheelSensitivity;
					map.SetZoomLevel (Mathf.Clamp01 (curZoom));
				} else {
					// rotate globe
					float speed = map.mouseDragSensitivity;
					map.transform.Rotate (Vector3.up * diff.x * speed, Space.World);
					map.transform.Rotate (Vector3.right * diff.y * speed, Space.World);
				}
			}
		}

	}

}

#endif