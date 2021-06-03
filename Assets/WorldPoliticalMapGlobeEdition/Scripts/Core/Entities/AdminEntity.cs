using UnityEngine;
using System.Collections.Generic;

namespace WPM {
	public abstract class AdminEntity: IAdminEntity, IExtendableAttribute {

		/// <summary>
		/// Entity name (country or province).
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Setting hidden to true will hide completely the entity (border, label) and it won't be highlighted
		/// </summary>
		public bool hidden;

		protected List<Region> _regions;

		/// <summary>
		/// List of all regions for the admin entity.
		/// </summary>
		public abstract List<Region> regions { get; set; }

		/// <summary>
		/// Index of the biggest region
		/// </summary>
		public int mainRegionIndex { get; set; }

		/// <summary>
		/// Returns the region object which is the main region of the country
		/// </summary>
		public abstract Region mainRegion { get; }

		public virtual float mainRegionArea { get { return mainRegion.rect2DArea; } }

		/// <summary>
		/// Computed Rect area that includes all regions. Used to fast hovering.
		/// </summary>
		public virtual Rect regionsRect2D { get; set; }

		public virtual float regionsRect2DArea { get { return regionsRect2D.width * regionsRect2D.height; } }

		protected Vector2 _latlonCenter;

		/// <summary>
		/// Center of the admin entity in the plane
		/// </summary>
		public virtual Vector2 latlonCenter {
			get { return _latlonCenter; } 
			set {
				_latlonCenter = value;
				_sphereCenter = Conversion.GetSpherePointFromLatLon (_latlonCenter);
			}
		}

		protected Vector3 _sphereCenter;


		[System.Obsolete("Use localPosition instead")]
		public Vector3 sphereCenter {
			get { return _sphereCenter; }
		}

		public virtual Vector3 localPosition { get { return _sphereCenter; } }

        protected JSONObject _attrib;

        /// <summary>
        /// Use this property to add/retrieve custom attributes for this country/province
        /// </summary>
        public JSONObject attrib { get { if (_attrib == null) { _attrib = new JSONObject(); } return _attrib; } set { _attrib = value; } }

        public bool hasAttributes { get { return _attrib != null; } }


        /// <summary>
        /// Returns true if any region of this entity contains the position
        /// </summary>
        /// <param name="mapPosition">Map position.</param>
        public bool Contains (Vector2 mapPosition) {
			if (!regionsRect2D.Contains(mapPosition)) {
				mapPosition.y += 360;
                // check for world-edge-cross-countries
				if (!regionsRect2D.Contains(mapPosition)) {
					return false;
				}
			}

			if (regions == null)
				return false;
			int regionCount = regions.Count;
			for (int k = 0; k < regionCount; k++) {
				if (regions [k].Contains (mapPosition)) {
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Returns true if any region of this entity overlaps a given region
		/// </summary>
		/// <param name="otherRegion">Other region.</param>
		public bool Overlaps (Region otherRegion) {
			if (regions == null) {
                return false;
            }

            int regionCount = regions.Count;
			for (int k = 0; k < regionCount; k++) {
				if (regions [k].Intersects (otherRegion)) {
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Used internally by Map Editor.
		/// </summary>
		public bool foldOut { get; set; }

		/// <summary>
        /// Deletes the surfaces gameobjects related to this entity
        /// </summary>
		public void DestroySurfaces() {
			if (_regions == null) return;
			int regionsCount = _regions.Count;
			for (int r = 0; r < regionsCount; r++) {
				if (_regions[r].surfaceGameObject != null) {
                    Object.DestroyImmediate(_regions[r].surfaceGameObject);
				}
			}

		}
	}
}