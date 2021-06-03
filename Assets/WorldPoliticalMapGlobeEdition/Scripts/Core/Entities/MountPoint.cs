using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	
	/// <summary>
	/// Mount Point record. Mount points are stored in the mountPoints file, in packed string editable format inside Resources/Geodata folder.
	/// </summary>
	public partial class MountPoint : IExtendableAttribute {
		
		/// <summary>
		/// Name of this mount point.
		/// </summary>
		public string name;
		
		/// <summary>
		/// Type of mount point. This is an optional, user-defined integer value.
		/// </summary>
		public int type;
		
		/// <summary>
		/// The index of the country.
		/// </summary>
		public int countryIndex;
		
		/// <summary>
		/// The index of the province or -1 if the mount point is not linked to any province.
		/// </summary>
		public int provinceIndex;

        [System.Obsolete("Use localPosition instead")]
		public Vector3 unitySphereLocation {
			get { return localPosition; }
        }

		/// <summary>
		/// The location of the mount point on the sphere.
		/// </summary>
		public Vector3 localPosition;

		public float latitude {
			get {
				return latlon.x;
			}
		}

		public float longitude {
			get {
				return latlon.y;
			}
		}

		Vector2 _latlon;

		public Vector2 latlon {
			get {
				if (latlonPending) {
					latlonPending = false;
					_latlon = Conversion.GetLatLonFromSpherePoint (localPosition);
				}
				return _latlon;
			}

		}

        JSONObject _attrib;

        /// <summary>
        /// Use this property to add/retrieve custom attributes for this country/province
        /// </summary>
        public JSONObject attrib { get { if (_attrib == null) { _attrib = new JSONObject(); } return _attrib; } set { _attrib = value; } }

        public bool hasAttributes { get { return _attrib != null; } }


        bool latlonPending;

		public MountPoint (string name, int countryIndex, int provinceIndex, Vector3 location, int type) {
			this.name = name;
			this.countryIndex = countryIndex;
			this.provinceIndex = provinceIndex;
			this.localPosition = location;
			this.type = type;
			this.latlonPending = true;
		}

		public MountPoint (string name, int countryIndex, int provinceIndex, Vector3 location) : this (name, countryIndex, provinceIndex, location, 0) {
		}

		public MountPoint Clone () {
			MountPoint c = new MountPoint (name, countryIndex, provinceIndex, localPosition, type);
            if (_attrib != null) {
                c.attrib = new JSONObject();
                c.attrib.Absorb(attrib);
            }
            return c;
		}

	}
}