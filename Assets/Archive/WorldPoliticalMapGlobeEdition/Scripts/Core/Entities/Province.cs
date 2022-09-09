using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

    public partial class Province : AdminEntity {
        public int countryIndex;

        /// <summary>
        /// List of land regions belonging to the province. Main/biggest region is defined by mainRegionIndex field.
        /// </summary>
        /// <value>The regions.</value>
        public override List<Region> regions {
            get {
                LazyLoadCheck();
                return _regions;
            }
            set { _regions = value; }
        }

        /// <summary>
        /// Returns the region object which is the main region of the country
        /// </summary>
        public override Region mainRegion {
            get {
                LazyLoadCheck();
                if (regions == null || mainRegionIndex < 0 || mainRegionIndex >= regions.Count) return null;
                return regions[mainRegionIndex];
            }
        }

        public override float mainRegionArea { get { return mainRegion.rect2DArea; } }

        public override Vector2 latlonCenter {
            get {
                LazyLoadCheck();
                return _latlonCenter;
            }
            set {
                _latlonCenter = value;
                _sphereCenter = Conversion.GetSpherePointFromLatLon(_latlonCenter);
            }
        }

        public override Vector3 localPosition {
            get {
                LazyLoadCheck();
                return _sphereCenter;
            }
        }

        int _mainRegionIndex;
        /// <summary>
        /// Index of the biggest region
        /// </summary>
        public new int mainRegionIndex {
            get {
                LazyLoadCheck();
                return _mainRegionIndex;
            }
            set { _mainRegionIndex = value; }
        }

        #region internal fields
        /// Used internally. Don't change this value.
        public string packedRegions;
        #endregion


        public Province(string name, int countryIndex) {
            this.name = name;
            this.countryIndex = countryIndex;
            this.regions = null; // lazy load during runtime due to size of data
        }

        /// <summary>
        /// Checks if province regions info has been loaded before one of its accesor gets called and reads the info from disk if needed.
        /// </summary>
        public void LazyLoadCheck() {
            if (_regions == null) {
                WorldMapGlobe.instance.ReadProvincePackedString(this);
            }
        }

        public Province Clone() {
            Province c = new Province(name, countryIndex);
            c.latlonCenter = latlonCenter;
            c.regions = regions;
            c.hidden = this.hidden;
            if (_attrib != null) {
                c.attrib = new JSONObject();
                c.attrib.Absorb(attrib);
            }
            c.regionsRect2D = regionsRect2D;
            return c;
        }

    }
}

