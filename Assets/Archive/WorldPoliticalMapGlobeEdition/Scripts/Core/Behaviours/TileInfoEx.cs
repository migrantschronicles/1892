using UnityEngine;
using System.Collections;

namespace WPM {
	public class TileInfoEx : MonoBehaviour {

		public bool debug;
		public int zoomLevel;
		public bool bigTile;
		public TILE_LOAD_STATUS loadStatus;
		public bool visible;
		public bool placeholderImageSet;
		public int lastFrameUsed;
		public Material material;
		public Texture2D parentTexture;

	}
}