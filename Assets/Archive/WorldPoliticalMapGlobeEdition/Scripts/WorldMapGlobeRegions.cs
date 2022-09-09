using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	public partial class WorldMapGlobe : MonoBehaviour {


		public float GetRegionZoomExtents (Region region) {

			// Compute world positions of region corners
			Vector2 min = region.latlonRect2D.min;
			Vector2 max = region.latlonRect2D.max;

			// Width
			Vector2 minL = new Vector2 ((min.x + max.x) * 0.5f, min.y);
			Vector2 maxR = new Vector2 ((min.x + max.x) * 0.5f, max.y);
			Vector3 wsLeft = Conversion.GetSpherePointFromLatLon (minL);
			Vector3 wsRight = Conversion.GetSpherePointFromLatLon (maxR);
			float width = Vector3.Distance (wsLeft, wsRight);

			// Height
			Vector2 minB = new Vector2 (min.x, (max.y + min.y) * 0.5f);
			Vector2 maxT = new Vector2 (max.x, (max.y + min.y) * 0.5f);
			Vector3 wsBottom = Conversion.GetSpherePointFromLatLon (minB);
			Vector3 wsTop = Conversion.GetSpherePointFromLatLon (maxT);
			float height = Vector3.Distance (wsBottom, wsTop);

			// Apply to sphere
			float scale = transform.lossyScale.x;
			return  GetFrustumZoomLevel (width * scale, height * scale);
		}

		public float GetZoomExtents (Rect rect) {
			float scale = transform.lossyScale.x;
			return  GetFrustumZoomLevel (rect.width * scale, rect.height * scale);
		}

		
	}

}