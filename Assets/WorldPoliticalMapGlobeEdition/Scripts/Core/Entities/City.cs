using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	
	public enum CITY_CLASS {
		CITY = 1,
		REGION_CAPITAL = 2,
		COUNTRY_CAPITAL = 4
	}

	public partial class City : IExtendableAttribute {

		public string name;

		public int countryIndex;

		public int regionIndex = -1;

		public string province;

		[System.Obsolete("Use localPosition instead")]
		public Vector3 unitySphereLocation {
			get { return localPosition; }
		}

		public Vector3 localPosition;
		public int population;
		public CITY_CLASS cityClass;
		
		/// <summary>
		/// Reference to the city icon drawn over the globe.
		/// </summary>
		public GameObject gameObject;
		
		/// <summary>
		/// Returns if city is visible on the map based on minimum population filter.
		/// </summary>
		public bool isShown;

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
			set {
				if (value != _latlon) {
					_latlon = value;
					localPosition = Conversion.GetSpherePointFromLatLon (_latlon);
				}
			}
		}

        JSONObject _attrib;

        /// <summary>
        /// Use this property to add/retrieve custom attributes for this country/province
        /// </summary>
        public JSONObject attrib { get { if (_attrib == null) { _attrib = new JSONObject(); } return _attrib; } set { _attrib = value; } }

        public bool hasAttributes { get { return _attrib != null;  } }

        bool latlonPending;

		public City (string name, string province, int countryIndex, int population, Vector3 location, CITY_CLASS cityClass) {
			this.name = name;
			this.province = province;
			this.countryIndex = countryIndex;
			this.population = population;
			this.localPosition = location;
			this.cityClass = cityClass;
			this.latlonPending = true;
		}

		public City Clone () {
			City c = new City (name, province, countryIndex, population, localPosition, cityClass);
            if (_attrib != null) {
                c.attrib = new JSONObject();
                c.attrib.Absorb(attrib);
            }
            return c;
		}


	}
}