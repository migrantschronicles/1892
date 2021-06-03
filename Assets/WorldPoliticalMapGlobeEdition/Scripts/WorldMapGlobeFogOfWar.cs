using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace WPM
{
	public partial class WorldMapGlobe : MonoBehaviour {
	
		#region Fog of War API

		[SerializeField]
		bool _showFogOfWar;
		
		/// <summary>
		/// Enables or disables fog of war layer
		/// </summary>
		public bool showFogOfWar {
			get { return _showFogOfWar; }
			set { if (_showFogOfWar != value) {
					_showFogOfWar = value;
					DrawFogOfWar();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		int _fogOfWarResolution = 10;
		
		/// <summary>
		/// Resolution for the internal fog of war mask texture
		/// </summary>
		public int fogOfWarResolution {
			get { return _fogOfWarResolution; }
			set { if (_fogOfWarResolution != value) {
					_fogOfWarResolution = value;
					DrawFogOfWar();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		float _fogOfWarElevation = 0.14f;
		
		/// <summary>
		/// Elevation for the fog of war layer (scale multiplier)
		/// </summary>
		public float fogOfWarElevation {
			get { return _fogOfWarElevation; }
			set { if (_fogOfWarElevation != value) {
					_fogOfWarElevation = value;
					DrawFogOfWar();
					isDirty = true;
				}
			}
		}

		[SerializeField, Range(0,1)]
		float _fogOfWarNoise = 1f;

		/// <summary>
		/// Elevation for the fog of war layer (scale multiplier)
		/// </summary>
		public float fogOfWarNoise {
			get { return _fogOfWarNoise; }
			set {
				if (_fogOfWarNoise != value) {
					_fogOfWarNoise = value;
					DrawFogOfWar();
					isDirty = true;
				}
			}
		}

		//		[SerializeField]
		//		float _fogOfWarSneak = 0.6f;
		//		
		//		/// <summary>
		//		/// Sneak factor (ability to see under fog of war from angled view)
		//		/// </summary>
		//		public float fogOfWarSneak {
		//			get { return _fogOfWarSneak; }
		//			set { if (_fogOfWarSneak != value) {
		//					_fogOfWarSneak = value;
		//					DrawFogOfWar();
		//					isDirty = true;
		//				}
		//			}
		//		}


		[SerializeField]
		float _fogOfWarAlpha = 1f;
		
		/// <summary>
		/// Alpha for the fog of war layer
		/// </summary>
		public float fogOfWarAlpha {
			get { return _fogOfWarAlpha; }
			set { if (_fogOfWarAlpha != value) {
					_fogOfWarAlpha = value;
					DrawFogOfWar();
					isDirty = true;
				}
			}
		}

		
		[SerializeField]
		Color _fogOfWarColor1 = new Color(1,1,0);
		
		/// <summary>
		/// Enables or disables fog of war layer
		/// </summary>
		public Color fogOfWarColor1 {
			get { return _fogOfWarColor1; }
			set { if (_fogOfWarColor1 != value) {
					_fogOfWarColor1 = value;
					DrawFogOfWar();
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		Color _fogOfWarColor2 = new Color(0,1,1);
		
		/// <summary>
		/// Enables or disables fog of war layer
		/// </summary>
		public Color fogOfWarColor2 {
			get { return _fogOfWarColor2; }
			set { if (_fogOfWarColor2 != value) {
					_fogOfWarColor2 = value;
					DrawFogOfWar();
					isDirty = true;
				}
			}
		}


		/// <summary>
		/// Fills a circle centered on spherePos location with fog using alpha (transparency) and custom width.
		/// </summary>
		/// <param name="spherePos">Sphere position.</param>
		/// <param name="alpha">Alpha in the range 0..1.</param>
		/// <param name="radius">Width in the range 0..1.</param>
		/// <param name="strength">Strength of the brush in the range 0..1.</param>
		public void SetFogOfWarAlpha(Vector3 spherePos, float alpha, float radius, float strength) {
			Vector2 latLon = Conversion.GetLatLonFromSpherePoint(spherePos);
			Vector2 uv = Conversion.GetUVFromLatLon(latLon.x, latLon.y);
			SetFowAlpha (uv, alpha, radius, strength);
		}

		/// <summary>
		/// Assigns an alpha to the entire fog of war layer.
		/// </summary>
		public void SetFogOfWarAlpha(float alpha) {
			SetFowAlpha (Misc.Vector2zero, alpha, 2, 1);
		}

		/// <summary>
		/// Fills a region with fog using alpha (transparency) and custom width.
		/// </summary>
		/// <param name="spherePos">Sphere position.</param>
		/// <param name="alpha">Alpha in the range 0..1.</param>
		/// <param name="strength">Strength of the brush in the range 0..1.</param>
		public void SetFogOfWarAlpha(Region region, float alpha, float strength) {

			if (region==null) return;

			Rect rect = region.latlonRect2D;

			float stepX = rect.width * 0.2f;
			float stepY = rect.height * 0.2f;
			Vector2 pos = new Vector2(rect.xMin, rect.yMin);
			float radius = Mathf.Max(Mathf.Max (stepX, stepY) / overlayHeight, 0.001f);
			while (pos.y<rect.yMax) {
				if (region.Contains(pos)) {
					Vector2 uv = Conversion.GetUVFromLatLon(pos.x, pos.y);
					SetFowAlpha (uv, alpha, radius, strength);
				}
				pos.x += stepX;
				if (pos.x>rect.xMax) {
					pos.x = rect.xMin;
					pos.y += stepY;
				}
			}
		}

		/// <summary>
		/// Fills a country's main region with fog using alpha (transparency) and custom width.
		/// </summary>
		/// <param name="Country">Country.</param>
		/// <param name="alpha">Alpha in the range 0..1.</param>
		/// <param name="strength">Strength of the brush in the range 0..1.</param>
		public void SetFogOfWarAlpha(Country country, float alpha, float strength) {
			if (country==null) return;
			SetFogOfWarAlpha(country.mainRegion, alpha, strength);
		}

		
		/// <summary>
		/// Fills a province's main region with fog using alpha (transparency) and custom width.
		/// </summary>
		/// <param name="Province">Province.</param>
		/// <param name="alpha">Alpha in the range 0..1.</param>
		/// <param name="strength">Strength of the brush in the range 0..1.</param>
		public void SetFogOfWarAlpha(Province province, float alpha, float strength) {
			if (province==null) return;
			SetFogOfWarAlpha(province.mainRegion, alpha, strength);
		}
		
		/// <summary>
		/// Fills a cell with fog using alpha (transparency) and custom width.
		/// </summary>
		/// <param name="cell">Cell.</param>
		/// <param name="alpha">Alpha in the range 0..1.</param>
		/// <param name="radius">Width in the range 0..1.</param>
		/// <param name="strength">Strength of the brush in the range 0..1.</param>
		public void SetFogOfWarAlpha(Cell cell, float alpha, float radius, float strength) {
			if (cell==null) return;
			SetFogOfWarAlpha(cell.sphereCenter, alpha, radius, strength);
		}

		#endregion

	}

}