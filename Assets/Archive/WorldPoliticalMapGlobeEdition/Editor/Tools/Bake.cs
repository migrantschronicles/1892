using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WPM {

	/// <summary>
	/// Bakes water mask into alpha channel of another texture
	/// </summary>
	public class Bake : MonoBehaviour {

		public Texture2D water;
		public Texture2D tex;
		public Rect rect = new Rect (0, 0, 1, 1);
		public bool testMode;

		void Start () {
			if (water == null || tex == null)
				return;

			int x = (int)(water.width * rect.xMin);
			int y = (int)(water.height * rect.yMin);
			int width = (int)(water.width * rect.width);
			int height = (int)(water.height * rect.width);

			Color[] w = water.GetPixels (x, y, width, height);
			Color[] t = tex.GetPixels ();
			for (int index = 0, j = 0; j < tex.height; j++) {
				int wj = (int)(j * height / tex.height) * width;
				for (int k = 0; k < tex.width; k++, index++) {
					int wk = (int)(k * width / tex.width);	
					t [index].a = w [wj + wk].r;
				}
			}

			Texture2D newTex = new Texture2D (tex.width, tex.height, TextureFormat.ARGB32, false);
			newTex.SetPixels (t);
			newTex.Apply ();

			if (testMode) {
				System.IO.File.WriteAllBytes ("test.png", newTex.EncodeToPNG ());
				Debug.Log ("Test texture written to test.png at root");
			} else {
				string path = AssetDatabase.GetAssetPath (tex);
				System.IO.File.WriteAllBytes (path, newTex.EncodeToPNG ());
				Debug.Log ("Saved");
			}

			AssetDatabase.Refresh ();

		}
	
	}
}
