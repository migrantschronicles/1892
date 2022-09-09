using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

	public partial class Country: AdminEntity {

		/// <summary>
		/// Continent name.
		/// </summary>
		public string continent;

		// Standardized codes
		public string fips10_4 = "";
		public string iso_a2 = "";
		public string iso_a3 = "";
		public string iso_n3 = "";

		/// <summary>
		/// Optional custom label. It set, it will be displayed instead of the country name.
		/// </summary>
		public string customLabel;

		/// <summary>
		/// Set it to true to specify a custom color for the label.
		/// </summary>
		public bool labelColorOverride;

		/// <summary>
		/// The color of the label.
		/// </summary>
		public Color labelColor = Color.white;


		/// <summary>
		/// If the label has its own font size.
		/// </summary>
		public bool labelFontSizeOverride = false;

		/// <summary>
		/// Manual font size for the label. Must set labelOverridesFontSize = true to have effect.
		/// </summary>
		public float labelFontSize = 0.1f;


		Font _labelFont;
		Material _labelShadowFontMaterial;

		/// <summary>
		/// Internal method used to obtain the shadow material associated to a custom Font provided.
		/// </summary>
		/// <value>The label shadow font material.</value>
		public Material labelFontShadowMaterial { get { return _labelShadowFontMaterial; } }

		/// <summary>
		/// Optional font for this label. Note that the font material will be instanced so it can change color without affecting other labels.
		/// </summary>
		public Font labelFontOverride { 
			get {
				return _labelFont;
			}
			set {
				if (value != _labelFont) {
					_labelFont = value;
					if (_labelFont != null) {
						Material fontMaterial = GameObject.Instantiate (_labelFont.material);
						fontMaterial.hideFlags = HideFlags.DontSave;
						_labelFont.material = fontMaterial;
						_labelShadowFontMaterial = GameObject.Instantiate (fontMaterial);
						_labelShadowFontMaterial.hideFlags = HideFlags.DontSave;
						_labelShadowFontMaterial.renderQueue--;
					}
				}
			}
		}

		/// <summary>
		/// Sets whether the country name will be shown or not.
		/// </summary>
		public bool labelVisible = false;

		/// <summary>
		/// If set to a value > 0 degrees then label will be rotated according to this value (and not automatically calculated).
		/// </summary>
		public float labelRotation = 0;

		/// <summary>
		/// If set to a value != 0 in both x/y then label will be moved according to this value (and not automatically calculated).
		/// </summary>
		public Vector2 labelOffset = Misc.Vector2zero;

		public override List<Region> regions {
			get { return _regions; }
			set { _regions = value; }
		}

		public override Region mainRegion {
			get {
				if (mainRegionIndex < 0 || mainRegionIndex >= regions.Count)
					return null;
				else
					return regions [mainRegionIndex];
			}
		}

		/// <summary>
		/// Set to false to prevent drawing provinces for this country
		/// </summary>
		public bool allowShowProvinces = true;

		/// <summary>
		/// The index of the capital city
		/// </summary>
		public int cityCapitalIndex = -1;

		#region internal fields

		/// Used internally. Don't change fields below.

		public TextMesh labelGameObject, labelShadowGameObject;
		public float labelMeshWidth, labelMeshHeight;
		public Vector2 labelMeshCenter;
		Province[] _provinces;

		public Province[] provinces {
			get {
				if (_provinces == null && WorldMapGlobe.instance.provinces == null) {
					// do nothing as the second clause already loads provinces if not initialized
				}
				return _provinces;
			}
			set {
				_provinces = value;
			}
		}


		public Vector3 labelSphereEdgeTop, labelSphereEdgeBottom;
		public Renderer labelRenderer, labelShadowRenderer;

		#endregion

		public Country (string name, string continent) {
			this.name = name;
			this.continent = continent;
			this.regions = new List<Region> ();
		}

		public Country Clone () {
			Country c = new Country (name, continent);
			c.latlonCenter = latlonCenter;
			c.regions = regions;
			c.customLabel = customLabel;
			c.labelColor = labelColor;
			c.labelColorOverride = labelColorOverride;
			c.labelFontOverride = labelFontOverride;
			c.labelVisible = labelVisible;
			c.labelOffset = labelOffset;
			c.labelRotation = labelRotation;
			c.provinces = provinces;
			c.hidden = this.hidden;
			c.fips10_4 = fips10_4;
			c.iso_a2 = iso_a2;
			c.iso_a3 = iso_a3;
			c.iso_n3 = iso_n3;
            if (_attrib != null) {
                c.attrib = new JSONObject();
                c.attrib.Absorb(attrib);
            }
            return c;
		}
	}

}