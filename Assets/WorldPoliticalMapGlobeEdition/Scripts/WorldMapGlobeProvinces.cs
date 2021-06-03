// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WPM.ClipperLib;

namespace WPM {
    public delegate void ProvinceBeforeEnter(int provinceIndex, int regionIndex, ref bool ignoreProvince);

    public delegate void ProvinceEvent(int provinceIndex, int regionIndex);

    /* Public WPM Class */
    public partial class WorldMapGlobe : MonoBehaviour {

        public event ProvinceBeforeEnter OnProvinceBeforeEnter;
        public event ProvinceEvent OnProvinceEnter;
        public event ProvinceEvent OnProvinceExit;
        public event ProvinceEvent OnProvincePointerDown;
        public event ProvinceEvent OnProvincePointerUp;
        public event ProvinceEvent OnProvinceClick;

        Province[] _provinces;

        /// <summary>
        /// Complete array of states and provinces and the country name they belong to.
        /// </summary>
        public Province[] provinces {
            get {
                if (_provinces == null)
                    ReadProvincesPackedString();
                return _provinces;
            }
            set {
                _provinces = value;
                lastProvinceLookupCount = -1;
            }
        }

        Province _provinceHighlighted;

        /// <summary>
        /// Returns Province under mouse position or null if none.
        /// </summary>
        public Province provinceHighlighted { get { return _provinceHighlighted; } }

        int _provinceHighlightedIndex = -1;

        /// <summary>
        /// Returns current highlighted province index.
        /// </summary>
        public int provinceHighlightedIndex { get { return _provinceHighlightedIndex; } }



        [SerializeField]
        bool
        _enableProvinceHighlight = true;

        /// <summary>
        /// Enable/disable province highlight when mouse is over.
        /// </summary>
        public bool enableProvinceHighlight {
            get {
                return _enableProvinceHighlight;
            }
            set {
                if (_enableProvinceHighlight != value) {
                    _enableProvinceHighlight = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float
        _provinceHighlightMaxScreenAreaSize = 1f;

        /// <summary>
        /// Defines the maximum area of a highlighted province. To prevent filling the whole screen with the highlight color, you can reduce this value and if the highlighted screen area size is greater than this factor (1=whole screen) the province won't be filled (it will behave as selected though)
        /// </summary>
        public float provinceHighlightMaxScreenAreaSize {
            get {
                return _provinceHighlightMaxScreenAreaSize;
            }
            set {
                if (_provinceHighlightMaxScreenAreaSize != value) {
                    _provinceHighlightMaxScreenAreaSize = value;
                    isDirty = true;
                }
            }
        }



        int _provinceLastClicked = -1;

        /// <summary>
        /// Returns the last clicked province index.
        /// </summary>
        public int provinceLastClicked { get { return _provinceLastClicked; } }

        int _provinceRegionLastClicked = -1;

        /// <summary>
        /// Returns the last clicked province region index.
        /// </summary>
        public int provinceRegionLastClicked { get { return _provinceRegionLastClicked; } }

        Region _provinceRegionHighlighted;

        /// <summary>
        /// Returns currently highlightd province's region.
        /// </summary>
        /// <value>The country region highlighted.</value>
        public Region provinceRegionHighlighted { get { return _provinceRegionHighlighted; } }

        int _provinceRegionHighlightedIndex = -1;

        /// <summary>
        /// Returns current highlighted province's region index.
        /// </summary>
        public int provinceRegionHighlightedIndex { get { return _provinceRegionHighlightedIndex; } }

        [SerializeField]
        bool
            _showProvinces = false;

        /// <summary>
        /// Toggle frontiers visibility.
        /// </summary>
        public bool showProvinces {
            get {
                return _showProvinces;
            }
            set {
                if (value != _showProvinces) {
                    _showProvinces = value;
                    isDirty = true;

                    if (_showProvinces) {
                        if (_provinces == null) {
                            ReadProvincesPackedString();
                        }
                        if (_drawAllProvinces) {
                            DrawAllProvinceBorders(true);
                        }
                    } else {
                        HideProvinces();
                    }
                }
            }
        }

        [SerializeField]
        bool
            _drawAllProvinces = false;

        /// <summary>
        /// Forces drawing of all provinces and not only thouse of currently selected country.
        /// </summary>
        public bool drawAllProvinces {
            get {
                return _drawAllProvinces;
            }
            set {
                if (value != _drawAllProvinces) {
                    _drawAllProvinces = value;
                    isDirty = true;
                    DrawAllProvinceBorders(true);
                }
            }
        }

        [SerializeField]
        Color
            _provincesFillColor = new Color(0, 0, 1, 0.7f);

        /// <summary>
        /// Fill color to use when the mouse hovers a country's region.
        /// </summary>
        public Color provincesFillColor {
            get {
                if (hudMatProvince != null) {
                    return hudMatProvince.color;
                } else {
                    return _provincesFillColor;
                }
            }
            set {
                if (value != _provincesFillColor) {
                    _provincesFillColor = value;
                    isDirty = true;
                    if (hudMatProvince != null && _provincesFillColor != hudMatProvince.color) {
                        hudMatProvince.color = _provincesFillColor;
                    }
                }
            }
        }

        [SerializeField]
        Color
            _provincesColor = Color.white;

        /// <summary>
        /// Global color for provinces.
        /// </summary>
        public Color provincesColor {
            get {
                return _provincesColor;
            }
            set {
                if (value != _provincesColor) {
                    _provincesColor = value;
                    isDirty = true;
                    UpdateProvincesMat();
                }
            }
        }

        [SerializeField]
        bool
            _enableProvinceEnclaves = false;

        /// <summary>
        /// Allows a province to be surrounded by another province
        /// </summary>
        public bool enableProvinceEnclaves {
            get {
                return _enableProvinceEnclaves;
            }
            set {
                if (value != _enableProvinceEnclaves) {
                    _enableProvinceEnclaves = value;
                    isDirty = true;
                }
            }
        }


        string _provinceAttributeFile = PROVINCE_ATTRIB_DEFAULT_FILENAME;

        public string provinceAttributeFile {
            get { return _provinceAttributeFile; }
            set {
                if (value != _provinceAttributeFile) {
                    _provinceAttributeFile = value;
                    if (_provinceAttributeFile == null)
                        _provinceAttributeFile = PROVINCE_ATTRIB_DEFAULT_FILENAME;
                    isDirty = true;
                    ReloadProvincesAttributes();
                }
            }
        }




        #region Public API area

        /// <summary>
        /// Draws the borders of the provinces/states a country by its id. Returns true is country is found, false otherwise.
        /// Note: if you need persistent provinces, call DrawProvinces(List<Country>...) instead.
        /// </summary>
        public bool DrawProvince(int countryIndex, bool includeNeighbours, bool forceRefresh) {
            if (countryIndex >= 0) {
                return mDrawProvinces(countryIndex, includeNeighbours, forceRefresh);
            }
            return false;
        }

        /// <summary>
        /// Draws the borders of provinces of a list of countries. Province borders remain regardless of country selection.
        /// </summary>
        /// <param name="countryIndices"></param>
        public void DrawProvinces(List<Country> countryIndices) {
            if (provinces == null) ReadProvincesPackedString();
            for (int k = 0; k < _countries.Length; k++) {
                Country c = _countries[k];
                c.allowShowProvinces = countryIndices.Contains(c);
            }
            _drawAllProvinces = true;
            _showProvinces = true;
            DrawAllProvinceBorders(true);
        }


        /// <summary>
        /// Hides all provinces.
        /// </summary>
        public void HideProvinces() {
            if (provincesObj != null) {
                DestroyImmediate(provincesObj);
            }
            countryProvincesDrawnIndex = -1;
            HideProvinceRegionHighlight();
        }


        /// <summary>
        /// Gets the province object by its index. This function equals to map.provinces[provinceIndex].
        /// </summary>
        /// <returns>The province.</returns>
        public Province GetProvince(int provinceIndex) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length) return null;
            return _provinces[provinceIndex];
        }

        /// <summary>
        /// Gets the province object by its name and country name.
        /// </summary>
        /// <returns>The province.</returns>
        /// <param name="countryName">Country name.</param>
        /// <param name="provinceName">Province name.</param>
        public Province GetProvince(string countryName, string provinceName) {
            int provinceIndex = GetProvinceIndex(countryName, provinceName);
            if (provinceIndex >= 0)
                return _provinces[provinceIndex];
            return null;
        }

        /// <summary>
        /// Returns the index of a province in the provinces array by its reference.
        /// </summary>
        public int GetProvinceIndex(Province province) {
            int provinceIndex;
            if (provinceLookup.TryGetValue(province, out provinceIndex))
                return provinceIndex;
            else
                return -1;
        }

        /// <summary>
        /// Returns the index of a province in the global provinces array.
        /// </summary>
        public int GetProvinceIndex(string countryName, string provinceName) {
            int countryIndex = GetCountryIndex(countryName);
            return GetProvinceIndex(countryIndex, provinceName);
        }


        /// <summary>
        /// Returns the index of a province in the global provinces array.
        /// </summary>
        public int GetProvinceIndex(int countryIndex, string provinceName) {
            if (countryIndex < 0 || countryIndex >= countries.Length)
                return -1;
            Country country = countries[countryIndex];
            if (country.provinces == null)
                return -1;
            for (int k = 0; k < country.provinces.Length; k++) {
                if (country.provinces[k].name.Equals(provinceName)) {
                    return GetProvinceIndex(country.provinces[k]);
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the province index by screen position.
        /// </summary>
        public bool GetProvinceIndex(int countryIndex, Ray ray, out int provinceIndex, out int regionIndex) {
            Vector3 hitPos;
            if (GetGlobeIntersection(ray, out hitPos)) {
                Vector3 localHit = transform.InverseTransformPoint(hitPos);
                if (GetProvinceUnderMouse(countryIndex, localHit, out provinceIndex, out regionIndex))
                    return true;
            }
            provinceIndex = -1;
            regionIndex = -1;
            return false;
        }

        /// <summary>
        /// Gets the index of the province that contains the provided map coordinates. This will ignore hidden countries.
        /// </summary>
        /// <returns>The province index.</returns>
        public int GetProvinceIndex(Vector3 spherePosition) {
            // verify if hitPos is inside any country polygon
            int countryIndex = GetCountryIndex(spherePosition);
            if (countryIndex >= 0) {
                int provinceIndex, provinceRegionIndex;
                if (GetProvinceUnderSpherePosition(countryIndex, spherePosition, out provinceIndex, out provinceRegionIndex)) {
                    return provinceIndex;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the province that contains a given map coordinate or the province whose center is nearest to that coordinate.
        /// </summary>
        public int GetProvinceNearPoint(Vector3 spherePosition) {
            int provinceIndex = GetProvinceIndex(spherePosition);
            if (provinceIndex >= 0)
                return provinceIndex;
            float minDist = float.MaxValue;
            for (int k = 0; k < _provinces.Length; k++) {
                float dist = FastVector.SqrDistanceByValue(_provinces[k].localPosition, spherePosition); // Vector3.SqrMagnitude (_provinces [k].sphereCenter - spherePosition);
                if (dist < minDist) {
                    minDist = dist;
                    provinceIndex = k;
                }
            }
            return provinceIndex;
        }


        /// <summary>
        /// Returns the province located in the sphere point provided (must provide the country index to which the province belongs). See also GetCountryUnderSpherePosition.
        /// </summary>
        public bool GetProvinceUnderSpherePosition(int countryIndex, Vector3 spherePoint, out int provinceIndex, out int provinceRegionIndex) {
            return GetProvinceUnderMouse(countryIndex, spherePoint, out provinceIndex, out provinceRegionIndex);
        }


        /// <summary>
        /// Returns an array of province objects for the specified country.
        /// </summary>
        public Province[] GetProvinces(string countryName) {
            int countryIndex = GetCountryIndex(countryName);
            return GetProvinces(countryIndex);
        }


        /// <summary>
        /// Returns an array of province objects for the specified country.
        /// </summary>
        public Province[] GetProvinces(Country country) {
            if (country == null || provinces == null) {
                return null;
            }
            return country.provinces;
        }


        /// <summary>
        /// Returns an array of province objects for the specified country.
        /// </summary>
        public Province[] GetProvinces(int countryIndex) {
            if (countryIndex < 0 || countryIndex >= countries.Length) {
                return null;
            }
            return GetProvinces(_countries[countryIndex]);
        }

        /// <summary>
        /// Returns a list of provinces whose center is contained in a given region
        /// </summary>
        public void GetProvinces(Region region, List<Province> provinces) {
            int provCount = provinces.Count;
            provinces.Clear();
            for (int k = 0; k < provCount; k++) {
                if (region.Contains(_provinces[k].latlonCenter))
                    provinces.Add(_provinces[k]);
            }
        }


        /// <summary>
        /// Returns an array of province names. The returning list can be grouped by country.
        /// </summary>
        public string[] GetProvinceNames(bool groupByCountry) {
            List<string> c = new List<string>(provinces.Length + countries.Length);
            if (provinces == null)
                return c.ToArray();
            bool[] countriesAdded = new bool[countries.Length];
            for (int k = 0; k < provinces.Length; k++) {
                Province province = provinces[k];
                if (province != null) { // could be null if country doesn't exist in this level of quality
                    if (groupByCountry) {
                        if (!countriesAdded[province.countryIndex]) {
                            countriesAdded[province.countryIndex] = true;
                            c.Add(countries[province.countryIndex].name);
                        }
                        c.Add(countries[province.countryIndex].name + "|" + province.name + " (" + k + ")");
                    } else {
                        c.Add(province.name + " (" + k + ")");

                    }
                }
            }
            c.Sort();

            if (groupByCountry) {
                int k = -1;
                while (++k < c.Count) {
                    int i = c[k].IndexOf('|');
                    if (i > 0) {
                        c[k] = "  " + c[k].Substring(i + 1);
                    }
                }
            }
            return c.ToArray();
        }


        /// <summary>
        /// Returns an array of province names for the specified country.
        /// </summary>
        public string[] GetProvinceNames(int countryIndex) {
            List<string> c = new List<string>(100);
            if (provinces == null || countryIndex < 0 || countryIndex >= countries.Length)
                return c.ToArray();
            for (int k = 0; k < provinces.Length; k++) {
                Province province = provinces[k];
                if (province.countryIndex == countryIndex) {
                    c.Add(province.name + " (" + k + ")");
                }
            }
            c.Sort();
            return c.ToArray();
        }


        /// <summary>
        /// Returns proposedName if it's unique in the country collection. Otherwise it adds a suffix to the name to make it unique.
        /// </summary>
        /// <returns>The country unique name.</returns>
        public string GetProvinceUniqueName(string proposedName) {
            string n = proposedName;
            int iteration = 2;
            bool repeat = true;
            while (repeat) {
                repeat = false;
                for (int k = 0; k < _provinces.Length; k++) {
                    if (_provinces[k].name.Equals(proposedName)) {
                        proposedName = n + " " + iteration++;
                        repeat = true;
                        break;
                    }
                }
            }
            return proposedName;
        }

        public string[] GetProvinceCountriesNeighboursNames(int provinceIndex, bool includeProvinceIndex) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return null;
            List<string> c = new List<string>(50);
            Region region = provinces[provinceIndex].mainRegion;
            int countryIndex = provinces[provinceIndex].countryIndex;
            int nc = region.neighbours.Count;
            if (nc == 0)
                return c.ToArray();
            for (int k = 0; k < nc; k++) {
                Region nr = region.neighbours[k];
                Province op = (Province)nr.entity;
                int cIndex = op.countryIndex;
                if (cIndex == countryIndex)
                    continue;
                string s;
                if (includeProvinceIndex) {
                    s = "  " + countries[cIndex].name + " (" + cIndex + ")";
                } else {
                    s = "  " + countries[cIndex].name;
                }
                if (!c.Contains(s))
                    c.Add(s);
            }
            if (c.Count > 0) {
                c.Sort();
                c.Insert(0, "Neighbours of " + provinces[provinceIndex].name);
            }
            return c.ToArray();
        }



        /// <summary>
        /// Returns a list of provinces whose attributes matches predicate
        /// </summary>
        public void GetProvinces(AttribPredicate predicate, List<Province> results) {
            if (results == null) return;
            int provinceCount = provinces.Length;
            for (int k = 0; k < provinceCount; k++) {
                Province province = _provinces[k];
                if (province.hasAttributes && predicate(province.attrib))
                    results.Add(province);
            }
        }

        /// <summary>
        /// Gets XML attributes of all provinces in jSON format.
        /// </summary>
        public string GetProvincesAttributes(bool prettyPrint = true) {
            if (provinces == null) return null;
            return GetProvincesAttributes(new List<Province>(provinces), prettyPrint);
        }

        /// <summary>
        /// Gets XML attributes of provided provinces in jSON format.
        /// </summary>
        public string GetProvincesAttributes(List<Province> provinces, bool prettyPrint = true) {
            JSONObject composed = new JSONObject();
            int provinceCount = provinces.Count;
            for (int k = 0; k < provinceCount; k++) {
                Province province = _provinces[k];
                if (province.hasAttributes && province.attrib.keys != null)
                    composed.AddField(k.ToString(), province.attrib);
            }
            return composed.Print(prettyPrint);
        }

        /// <summary>
        /// Sets provinces attributes from a jSON formatted string.
        /// </summary>
        public void SetProvincesAttributes(string jSON) {
            JSONObject composed = new JSONObject(jSON);
            if (composed.keys == null)
                return;
            int keyCount = composed.keys.Count;
            for (int k = 0; k < keyCount; k++) {
                int provinceIndex = int.Parse(composed.keys[k]);
                if (provinceIndex >= 0) {
                    provinces[provinceIndex].attrib = composed[k];
                }
            }
        }


        /// <summary>
        /// Adds a new province which has been properly initialized. Used by the Map Editor. Name must be unique.
        /// </summary>
        /// <returns><c>true</c> if province was added, <c>false</c> otherwise.</returns>
        public bool ProvinceAdd(Province province) {
            if (province.countryIndex < 0 || province.countryIndex >= countries.Length)
                return false;
            Province[] newProvinces = new Province[provinces.Length + 1];
            for (int k = 0; k < provinces.Length; k++) {
                newProvinces[k] = provinces[k];
            }
            newProvinces[newProvinces.Length - 1] = province;
            provinces = newProvinces;
            lastProvinceLookupCount = -1;
            // add the new province to the country internal list
            Country country = countries[province.countryIndex];
            if (country.provinces == null)
                country.provinces = new Province[0];
            Province[] newCountryProvinces = new Province[country.provinces.Length + 1];
            for (int k = 0; k < country.provinces.Length; k++) {
                newCountryProvinces[k] = country.provinces[k];
            }
            newCountryProvinces[newCountryProvinces.Length - 1] = province;
            country.provinces = newCountryProvinces;
            return true;
        }



        /// <summary>
        /// Renames the province. Name must be unique, different from current and one letter minimum.
        /// </summary>
        /// <returns><c>true</c> if country was renamed, <c>false</c> otherwise.</returns>
        public bool ProvinceRename(int countryIndex, string oldName, string newName) {
            if (newName == null || newName.Length == 0)
                return false;
            int provinceIndex = GetProvinceIndex(countryIndex, oldName);
            int newProvinceIndex = GetProvinceIndex(countryIndex, newName);
            if (provinceIndex < 0 || newProvinceIndex >= 0)
                return false;
            provinces[provinceIndex].name = newName;
            // Updates all cities that depends on this province
            int cityCount = cities.Count;
            for (int k = 0; k < cityCount; k++) {
                if (_cities[k].province.Equals(oldName)) {
                    _cities[k].province = newName;
                }
            }
            lastProvinceLookupCount = -1;
            return true;

        }


        /// <summary>
        /// Delete all provinces from specified continent.
        /// </summary>
        public void ProvincesDeleteOfSameContinent(string continentName) {
            HideProvinceRegionHighlights(true);
            if (provinces == null)
                return;
            int numProvinces = _provinces.Length;
            List<Province> newProvinces = new List<Province>(numProvinces);
            for (int k = 0; k < numProvinces; k++) {
                if (_provinces[k] != null) {
                    int c = _provinces[k].countryIndex;
                    if (!countries[c].continent.Equals(continentName)) {
                        newProvinces.Add(_provinces[k]);
                    }
                }
            }
            provinces = newProvinces.ToArray();
        }



        /// <summary>
        /// Returns all neighbour provinces
        /// </summary>
        public List<Province> ProvinceNeighbours(int provinceIndex) {

            List<Province> provinceNeighbours = new List<Province>();

            // Get province object
            Province province = provinces[provinceIndex];

            // Iterate for all regions (a province can have several separated regions)
            for (int provinceRegionIndex = 0; provinceRegionIndex < province.regions.Count; provinceRegionIndex++) {
                Region provinceRegion = province.regions[provinceRegionIndex];

                // Get the neighbours for this region
                for (int neighbourIndex = 0; neighbourIndex < provinceRegion.neighbours.Count; neighbourIndex++) {
                    Region neighbour = provinceRegion.neighbours[neighbourIndex];
                    Province neighbourProvince = (Province)neighbour.entity;
                    if (!provinceNeighbours.Contains(neighbourProvince)) {
                        provinceNeighbours.Add(neighbourProvince);
                    }
                }
            }

            return provinceNeighbours;
        }


        /// <summary>
        /// Get neighbours of the main region of a province
        /// </summary>
        public List<Province> ProvinceNeighboursOfMainRegion(int provinceIndex) {

            List<Province> provinceNeighbours = new List<Province>();

            // Get main region
            Province province = provinces[provinceIndex];
            Region provinceRegion = province.regions[province.mainRegionIndex];

            // Get the neighbours for this region
            for (int neighbourIndex = 0; neighbourIndex < provinceRegion.neighbours.Count; neighbourIndex++) {
                Region neighbour = provinceRegion.neighbours[neighbourIndex];
                Province neighbourProvince = (Province)neighbour.entity;
                if (!provinceNeighbours.Contains(neighbourProvince)) {
                    provinceNeighbours.Add(neighbourProvince);
                }
            }
            return provinceNeighbours;
        }


        /// <summary>
        /// Get neighbours of the currently selected region
        /// </summary>
        public List<Province> ProvinceNeighboursOfCurrentRegion() {

            List<Province> provinceNeighbours = new List<Province>();

            // Get main region
            Region selectedRegion = provinceRegionHighlighted;
            if (selectedRegion == null)
                return provinceNeighbours;

            // Get the neighbours for this region
            for (int neighbourIndex = 0; neighbourIndex < selectedRegion.neighbours.Count; neighbourIndex++) {
                Region neighbour = selectedRegion.neighbours[neighbourIndex];
                Province neighbourProvince = (Province)neighbour.entity;
                if (!provinceNeighbours.Contains(neighbourProvince)) {
                    provinceNeighbours.Add(neighbourProvince);
                }
            }
            return provinceNeighbours;
        }



        /// <summary>
        /// Starts navigation to target province/state. Returns false if not found.
        /// </summary>
        public CallbackHandler FlyToProvince(string name) {
            for (int k = 0; k < provinces.Length; k++) {
                if (name.Equals(provinces[k].name)) {
                    return FlyToProvince(k, _navigationTime);
                }
            }
            return null;
        }

        /// <summary>
        /// Starts navigation to target province/state by index in the provinces collection.
        /// </summary>
        public CallbackHandler FlyToProvince(int provinceIndex) {
            return FlyToProvince(provinceIndex, _navigationTime);
        }

        /// <summary>
        /// Starts navigation to target province/state by index in the provinces collection and providing the duration in seconds.
        /// </summary>
        public CallbackHandler FlyToProvince(int provinceIndex, float duration) {
            return FlyToLocation(provinces[provinceIndex].localPosition, duration);
        }


        /// <summary>
        /// Starts navigating to target province/state by index in the provinces collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// Set zoomLevel to a value from 0 to 1 for the destination zoom level.
        /// </summary>
        public CallbackHandler FlyToProvince(int provinceIndex, float duration, float zoomLevel) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return CallbackHandler.Null;
            return FlyToLocation(_provinces[provinceIndex].localPosition, duration, zoomLevel);
        }

        /// <summary>
        /// Starts navigating to target province/state by index in the provinces collection with specified duration, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// Set zoomLevel to a value from 0 to 1 for the destination zoom level.
        /// Set bounceIntensity to a value from 0 to 1 for a bouncing effect between current position and destination
        /// </summary>
        public CallbackHandler FlyToProvince(int provinceIndex, float duration, float zoomLevel, float bounceIntensity) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return CallbackHandler.Null;
            return FlyToLocation(_provinces[provinceIndex].localPosition, duration, zoomLevel, bounceIntensity);
        }


        /// <summary>
        /// Colorize all regions of specified province/state. Returns false if not found.
        /// </summary>
        public bool ToggleProvinceSurface(string countryName, string provinceName, bool visible, Color color) {
            int provinceIndex = GetProvinceIndex(countryName, provinceName);
            return ToggleProvinceSurface(provinceIndex, visible, color);
        }

        /// <summary>
        /// Colorize all regions of specified province/state.
        /// </summary>
        public bool ToggleProvinceSurface(Province province, bool visible, Color color) {
            int provinceIndex = GetProvinceIndex(province);
            return ToggleProvinceSurface(provinceIndex, visible, color);
        }

        /// <summary>
        /// Colorize all regions of specified province/state by index in the provinces collection.
        /// </summary>
        public bool ToggleProvinceSurface(int provinceIndex, bool visible, Color color) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return false;
            if (!visible) {
                HideProvinceSurfaces(provinceIndex);
                return true;
            }
            int regionCount = provinces[provinceIndex].regions.Count;
            for (int r = 0; r < regionCount; r++) {
                ToggleProvinceRegionSurface(provinceIndex, r, visible, color);
            }
            return true;
        }

        /// <summary>
        /// Colorize the main region of specified province/state by index in the provinces collection.
        /// </summary>
        public void ToggleProvinceMainRegionSurface(int provinceIndex, bool visible, Color color) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return;
            int regionIndex = provinces[provinceIndex].mainRegionIndex;
            ToggleProvinceRegionSurface(provinceIndex, regionIndex, visible, color);
        }

        /// <summary>
        /// Colorize the main region of specified province/state by index in the provinces collection.
        /// </summary>
        public void ToggleProvinceMainRegionSurface(int provinceIndex, bool visible, Color color, Texture2D texture) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return;
            int regionIndex = provinces[provinceIndex].mainRegionIndex;
            ToggleProvinceRegionSurface(provinceIndex, regionIndex, visible, color, texture, Misc.Vector2one, Misc.Vector2zero, 0, false);
        }


        /// <summary>
        /// Colorize the main region of specified province/state by index in the provinces collection.
        /// </summary>
        public void ToggleProvinceMainRegionSurface(int provinceIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return;
            int regionIndex = provinces[provinceIndex].mainRegionIndex;
            ToggleProvinceRegionSurface(provinceIndex, regionIndex, visible, color, texture, textureScale, textureOffset, textureRotation, false);
        }


        /// <summary>
        /// Colorize or texture a region of specified province/state by index in the provinces collection.
        /// </summary>
        public void ToggleProvinceRegionSurface(int provinceIndex, int regionIndex, bool visible, Color color) {
            ToggleProvinceRegionSurface(provinceIndex, regionIndex, visible, color, null, Vector2.one, Vector2.zero, 0, false);
        }

        /// <summary>
        /// Colorize or texture a region of specified province/state by index in the provinces collection.
        /// </summary>
        public void ToggleProvinceRegionSurface(int provinceIndex, int regionIndex, bool visible, Color color, Texture2D texture) {
            ToggleProvinceRegionSurface(provinceIndex, regionIndex, visible, color, texture, Misc.Vector2one, Misc.Vector2zero, 0, false);
        }

        /// <summary>
        /// Colorize or texture a region of specified province/state by index in the provinces collection.
        /// </summary>
        public GameObject ToggleProvinceRegionSurface(int provinceIndex, int regionIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool temporary) {

            if (!visible) {
                HideProvinceRegionSurface(provinceIndex, regionIndex);
                return null;
            }
            GameObject surf = null;
            Region region = provinces[provinceIndex].regions[regionIndex];
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            // Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
            surfaces.TryGetValue(cacheIndex, out surf);

            // Should the surface be recreated?
            Material surfMaterial;
            if (surf != null) {
                surfMaterial = surf.GetComponent<Renderer>().sharedMaterial;
                if (texture != null && (region.customMaterial == null || textureScale != region.customTextureScale || textureOffset != region.customTextureOffset ||
                                textureRotation != region.customTextureRotation || !region.customMaterial.name.Equals(provinceTexturizedMat.name))) {
                    surfaces.Remove(cacheIndex);
                    DestroyImmediate(surf);
                    surf = null;
                }
            }
            // If it exists, activate and check proper material, if not create surface
            bool isHighlighted = provinceHighlightedIndex == provinceIndex && provinceRegionHighlightedIndex == regionIndex && _enableProvinceHighlight;
            if (surf != null) {
                bool needMaterial = false;
                if (!surf.activeSelf) {
                    surf.SetActive(true);
                    needMaterial = true;
                    UpdateSurfaceCount();
                } else {
                    // Check if material is ok
                    surfMaterial = surf.GetComponent<Renderer>().sharedMaterial;
                    if ((texture == null && !surfMaterial.name.Equals(provinceColoredMat.name)) || (texture != null && !surfMaterial.name.Equals(provinceTexturizedMat.name))
                                       || (surfMaterial.color != color && !isHighlighted) || (texture != null && region.customMaterial.mainTexture != texture))
                        needMaterial = true;
                }
                if (needMaterial) {
                    Material goodMaterial = GetProvinceColoredTexturedMaterial(color, texture);
                    region.customMaterial = goodMaterial;
                    region.customTextureOffset = textureOffset;
                    region.customTextureRotation = textureRotation;
                    region.customTextureScale = textureScale;
                    ApplyMaterialToSurface(surf, goodMaterial);

                }
            } else {
                surfMaterial = GetProvinceColoredTexturedMaterial(color, texture);
                surf = GenerateProvinceRegionSurface(provinceIndex, regionIndex, surfMaterial, textureScale, textureOffset, textureRotation, temporary);
            }
            // If it was highlighted, highlight it again
            if (region.customMaterial != null && isHighlighted && region.customMaterial.color != hudMatProvince.color) {
                Material clonedMat = Instantiate(region.customMaterial);
                clonedMat.name = region.customMaterial.name;
                clonedMat.color = hudMatProvince.color;
                surf.GetComponent<Renderer>().sharedMaterial = clonedMat;
                provinceRegionHighlightedObj = surf;
            }
            return surf;
        }

        /// <summary>
        /// Disables all province regions highlights. This doesn't destroy custom materials.
        /// </summary>
        public void HideProvinceRegionHighlights(bool destroyCachedSurfaces) {
            HideProvinceRegionHighlight();
            if (_provinces == null)
                return;
            int provincesCount = provinces.Length;
            for (int c = 0; c < provincesCount; c++) {
                Province province = _provinces[c];
                if (province == null || province.regions == null)
                    continue;
                for (int cr = 0; cr < province.regions.Count; cr++) {
                    Region region = province.regions[cr];
                    int cacheIndex = GetCacheIndexForProvinceRegion(c, cr);
                    GameObject surf;
                    if (surfaces.TryGetValue(cacheIndex, out surf)) {
                        if (surf == null) {
                            surfaces.Remove(cacheIndex);
                        } else {
                            if (destroyCachedSurfaces) {
                                surfaces.Remove(cacheIndex);
                                DestroyImmediate(surf);
                            } else {
                                if (region.customMaterial == null) {
                                    surf.SetActive(false);
                                } else {
                                    ApplyMaterialToSurface(surf, region.customMaterial);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Hides all colorized regions of all provinces/states.
        /// </summary>
        public void HideProvinceSurfaces() {
            if (provinces == null)
                return;
            for (int p = 0; p < provinces.Length; p++) {
                HideProvinceSurfaces(p);
            }
        }


        /// <summary>
        /// Hides all colorized regions of one province/state.
        /// </summary>
        public void HideProvinceSurfaces(int provinceIndex, bool destroyCachedSurface = false) {
            if (provinces[provinceIndex].regions == null)
                return;
            for (int r = 0; r < provinces[provinceIndex].regions.Count; r++) {
                HideProvinceRegionSurface(provinceIndex, r, destroyCachedSurface);
            }
        }

        /// <summary>
        /// Hides all regions of one province.
        /// </summary>
        public void HideProvinceRegionSurface(int provinceIndex, int regionIndex, bool destroyCachedSurface = false) {
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            GameObject surf = null;
            if (surfaces.TryGetValue(cacheIndex, out surf)) {
                if (surf == null) {
                    surfaces.Remove(cacheIndex);
                } else if (destroyCachedSurface) {
                    DestroyImmediate(surf);
                    surfaces.Remove(cacheIndex);
                } else {
                    surf.SetActive(false);
                }
                UpdateSurfaceCount();
            }
            provinces[provinceIndex].regions[regionIndex].customMaterial = null;
        }


        /// <summary>
        /// Flashes specified province by index in the global province array.
        /// </summary>
        public void BlinkProvince(int provinceIndex, Color color1, Color color2, float duration, float blinkingSpeed, bool smoothBlink = false) {
            int mainRegionIndex = provinces[provinceIndex].mainRegionIndex;
            BlinkProvince(provinceIndex, mainRegionIndex, color1, color2, duration, blinkingSpeed, smoothBlink);
        }

        /// <summary>
        /// Flashes specified province's region.
        /// </summary>
        public void BlinkProvince(int provinceIndex, int regionIndex, Color color1, Color color2, float duration, float blinkingSpeed, bool smoothBlink = false) {
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            GameObject surf;
            bool disableAtEnd;
            if (surfaces.ContainsKey(cacheIndex)) {
                surf = surfaces[cacheIndex];
                disableAtEnd = !surf.activeSelf;
            } else {
                surf = GenerateProvinceRegionSurface(provinceIndex, regionIndex, hudMatProvince, true);
                disableAtEnd = true;
            }
            surf.SetActive(true);
            SurfaceBlinker sb = surf.AddComponent<SurfaceBlinker>();
            sb.blinkMaterial = hudMatProvince;
            sb.color1 = color1;
            sb.color2 = color2;
            sb.duration = duration;
            sb.speed = blinkingSpeed;
            sb.disableAtEnd = disableAtEnd;
            sb.customizableSurface = provinces[provinceIndex].regions[regionIndex];
            sb.smoothBlink = smoothBlink;
        }

        /// <summary>
        /// Returns the colored surface (game object) of a province. If it has not been colored yet, it will return null.
        /// </summary>
        public GameObject GetProvinceRegionSurfaceGameObject(int provinceIndex, int regionIndex) {
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            GameObject surf = null;
            surfaces.TryGetValue(cacheIndex, out surf);
            return surf;
        }


        /// <summary>
        /// Returns the zoom level which shows the province main region in full screen
        /// </summary>
        /// <returns>The province region zoom level.</returns>
        /// <param name="provinceIndex">Country index.</param>
        public float GetProvinceMainRegionZoomExtents(int provinceIndex) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return 0;

            Province province = _provinces[provinceIndex];
            return GetProvinceRegionZoomExtents(provinceIndex, province.mainRegionIndex);
        }


        /// <summary>
        /// Returns the zoom level which shows the province region in full screen
        /// </summary>
        /// <returns>The province region zoom level.</returns>
        /// <param name="provinceIndex">Province index.</param>
        /// <param name="regionIndex">Region index.</param>
        public float GetProvinceRegionZoomExtents(int provinceIndex, int regionIndex) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return 0;

            Province province = _provinces[provinceIndex];
            if (regionIndex < 0 || regionIndex >= province.regions.Count)
                return 0;

            return GetRegionZoomExtents(province.regions[regionIndex]);
        }


        #endregion


        /// <summary>
        /// Returns a list of provinces that are visible (front facing camera)
        /// </summary>
        public List<Province> GetVisibleProvinces() {
            if (provinces == null)
                return null;
            List<Country> vc = GetVisibleCountries();
            List<Province> vp = new List<Province>(30);
            Camera cam = mainCamera;
            for (int k = 0; k < vc.Count; k++) {
                Country country = vc[k];
                if (country.provinces == null)
                    continue;
                for (int p = 0; p < country.provinces.Length; p++) {
                    Province prov = country.provinces[p];
                    Vector3 center = transform.TransformPoint(prov.localPosition);
                    Vector3 dir = center - transform.position;
                    float d = Vector3.Dot(cam.transform.forward, dir);
                    if (d < -0.2f) {
                        Vector3 vpos = cam.WorldToViewportPoint(center);
                        float viewportMinX = cam.rect.xMin;
                        float viewportMaxX = cam.rect.xMax;
                        float viewportMinY = cam.rect.yMin;
                        float viewportMaxY = cam.rect.yMax;
                        if (vpos.x >= viewportMinX && vpos.x <= viewportMaxX && vpos.y >= viewportMinY && vpos.y <= viewportMaxY) {
                            vp.Add(prov);
                        }
                    }
                }
            }
            return vp;
        }

        /// <summary>
        /// Returns a list of provinces that are visible and overlaps the rectangle defined by two given sphere points
        /// </summary>
        public List<Province> GetVisibleProvinces(Vector3 rectTopLeft, Vector3 rectBottomRight) {
            Vector2 latlon0, latlon1;
            latlon0 = Conversion.GetBillboardPosFromSpherePoint(rectTopLeft);
            latlon1 = Conversion.GetBillboardPosFromSpherePoint(rectBottomRight);
            Rect rect = new Rect(latlon0.x, latlon1.y, latlon1.x - latlon0.x, latlon0.y - latlon1.y);
            List<Province> selectedProvinces = new List<Province>();

            if (_provinces == null)
                ReadProvincesPackedString();
            List<Country> countries = GetVisibleCountries(rectTopLeft, rectBottomRight);
            int countryCount = countries.Count;
            for (int k = 0; k < countryCount; k++) {
                Country country = countries[k];
                if (country.hidden)
                    continue;
                if (country.provinces == null)
                    continue;
                for (int p = 0; p < country.provinces.Length; p++) {
                    Province province = country.provinces[p];
                    if (selectedProvinces.Contains(province))
                        continue;
                    // Check if any of province's regions is inside rect
                    if (province.regions == null)
                        ReadProvincePackedString(province);
                    if (province.regions == null)
                        continue;
                    int crc = province.regions.Count;
                    for (int cr = 0; cr < crc; cr++) {
                        Region region = province.regions[cr];
                        if (rect.Overlaps(region.rect2Dbillboard)) {
                            selectedProvinces.Add(province);
                            break;
                        }
                    }
                }
            }
            return selectedProvinces;
        }


        /// <summary>
        /// Makes provinceIndex absorb sourceProvinceIndex. All regions are transfered to target province.
        /// This function is quite slow with high definition frontiers.  Note that the province indices may change after this operation.
        /// </summary>
        /// <param name="provinceIndex">Province index of the conquering province.</param>
        /// <param name="sourceProvinceIndex">The index for the source province to be absorved.</param>
        public bool ProvinceMerge(int provinceIndex, int sourceProvinceIndex, bool redraw) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length || sourceProvinceIndex < 0 || sourceProvinceIndex >= provinces.Length) {
                return false;
            }
            Region sourceProvinceRegion = provinces[sourceProvinceIndex].mainRegion;
            return ProvinceTransferProvinceRegion(provinceIndex, sourceProvinceRegion, redraw);
        }


        /// <summary>
        /// Make the first province absorb the rest of provinces. This function is quite slow with high definition frontiers. Note that the province indices may change after this operation.
        /// </summary>
        public bool ProvincesMerge(List<Province> provinces, bool redraw, bool redrawNeighbours = false) {
            if (provinces == null || provinces.Count < 2) return false;

            Province firstProvince = provinces[0];

            List<int> affectedCountries = BufferPool<int>.Get();
            affectedCountries.Add(firstProvince.countryIndex);

            for (int k=1;k<provinces.Count;k++) {
                Region otherProvinceRegion = provinces[k].mainRegion;
                if (otherProvinceRegion != null) {
                    int provIndex = GetProvinceIndex(provinces[0]);
                    if (ProvinceTransferProvinceRegion(provIndex, otherProvinceRegion, false)) {
                        int otherCountryIndex = ((Province)otherProvinceRegion.entity).countryIndex;
                        if (!affectedCountries.Contains(otherCountryIndex)) {
                            affectedCountries.Add(otherCountryIndex);
                        }
                    } else { 
                        BufferPool<int>.Release(affectedCountries);
                        return false;
                    }
                }
            }
            if (redraw) {
                for (int k = 0; k < affectedCountries.Count; k++) {
                    DrawProvince(affectedCountries[k], redrawNeighbours, true);
                }
            }
            BufferPool<int>.Release(affectedCountries);
            return true;
        }

        /// <summary>
        /// Makes provinceIndex absorb another province providing any of its regions. All regions are transfered to target province.
        /// This function is quite slow with high definition frontiers.
        /// </summary>
        /// <param name="provinceIndex">Province index of the conquering province.</param>
        /// <param name="sourceRegion">Source region of the loosing province.</param>
        public bool ProvinceTransferProvinceRegion(int provinceIndex, Region sourceProvinceRegion, bool redraw) {
            int sourceProvinceIndex = GetProvinceIndex((Province)sourceProvinceRegion.entity);
            if (provinceIndex < 0 || sourceProvinceIndex < 0 || provinceIndex == sourceProvinceIndex)
                return false;

            // Transfer cities
            Province sourceProvince = provinces[sourceProvinceIndex];
            Province targetProvince = provinces[provinceIndex];
            if (sourceProvince.countryIndex != targetProvince.countryIndex) {
                // Transfer source province to target country province
                if (!CountryTransferProvinceRegion(targetProvince.countryIndex, sourceProvinceRegion)) {
                    return false;
                }
            }

            int cityCount = cities.Count;
            for (int k = 0; k < cityCount; k++) {
                if (_cities[k].countryIndex == sourceProvince.countryIndex && _cities[k].province.Equals(sourceProvince.name))
                    _cities[k].province = targetProvince.name;
            }

            // Transfer mount points
            int mountPointCount = mountPoints.Count;
            for (int k = 0; k < mountPointCount; k++) {
                if (mountPoints[k].provinceIndex == sourceProvinceIndex)
                    mountPoints[k].provinceIndex = provinceIndex;
            }

            // Transfer regions
            if (sourceProvince.regions.Count > 0) {
                List<Region> targetRegions = new List<Region>(targetProvince.regions);
                for (int k = 0; k < sourceProvince.regions.Count; k++) {
                    targetRegions.Add(sourceProvince.regions[k]);
                }
                targetProvince.regions = targetRegions;
            }

            // Fusion any adjacent regions that results from merge operation
            ProvinceMergeAdjacentRegions(targetProvince);
            RegionSanitize(targetProvince.regions, false);
            targetProvince.mainRegionIndex = 0; // will be updated on RefreshProvinceDefinition

            // Finish operation
            ProvinceDeleteAndDependencies(sourceProvinceIndex);
            if (provinceIndex > sourceProvinceIndex) {
                provinceIndex--;
            }

            if (redraw) {
                RefreshProvinceDefinition(provinceIndex);
            } else {
                RefreshProvinceGeometry(provinceIndex);
            }
            
            return true;
        }


        /// <summary>
        /// Deletes current region or province if this was the last region.
        /// </summary>
        public void ProvinceDeleteAndDependencies(int provinceIndex) {
            if (provinceIndex < 0 || provinceIndex >= provinces.Length)
                return;

            // Clears references from mount points
            if (mountPoints != null) {
                for (int k = 0; k < mountPoints.Count; k++) {
                    if (mountPoints[k].provinceIndex == provinceIndex) {
                        mountPoints[k].provinceIndex = -1;
                    }
                }
            }

            List<Province> newProvinces = new List<Province>(_provinces.Length);
            // Clears references from cities
            int countryIndex = _provinces[provinceIndex].countryIndex;
            if (countryIndex >= 0 && countryIndex < _countries.Length) {
                string provinceName = _provinces[provinceIndex].name;
                if (cities != null) {
                    for (int k = 0; k < _cities.Count; k++) {
                        if (_cities[k].countryIndex == countryIndex && _cities[k].name.Equals(provinceName)) {
                            _cities[k].name = "";
                        }
                    }
                }

                // Remove it from the country array
                Country country = _countries[countryIndex];
                if (country.provinces != null) {
                    for (int k = 0; k < country.provinces.Length; k++) {
                        if (!country.provinces[k].name.Equals(provinceName))
                            newProvinces.Add(country.provinces[k]);
                    }
                    newProvinces.Sort(ProvinceSizeComparer);
                    country.provinces = newProvinces.ToArray();
                }
            }

            // Remove from the global array
            newProvinces.Clear();
            for (int k = 0; k < _provinces.Length; k++) {
                if (k != provinceIndex) {
                    newProvinces.Add(_provinces[k]);
                }
            }
            provinces = newProvinces.ToArray();
        }



        /// <summary>
        /// Makes provinceIndex absorb an hexagonal portion of the map. If that portion belong to another province, it will be substracted from that province as well.
        /// This function is quite slow with high definition frontiers.
        /// </summary>
        /// <param name="provinceIndex">Province index of the conquering province.</param>
        /// <param name="cellIndex">Index of the cell to add to the province.</param>
        public bool ProvinceTransferCell(int provinceIndex, int cellIndex, bool redraw = true) {
            if (provinceIndex < 0 || cellIndex < 0 || cells == null || cellIndex >= cells.Length)
                return false;

            // Start process
            Province province = provinces[provinceIndex];
            Cell cell = cells[cellIndex];

            // Create a region for the cell
            Region sourceRegion = new Region(province, province.regions.Count);
            // Convert cell points to latlon coordinates
            sourceRegion.UpdatePointsAndRect(cell.latlon);

            // Transfer cities
            List<City> citiesInCell = GetCities(sourceRegion);
            int cityCount = citiesInCell.Count;
            for (int k = 0; k < cityCount; k++) {
                City city = citiesInCell[k];
                if (city.countryIndex != province.countryIndex) {
                    city.countryIndex = province.countryIndex;
                    city.province = ""; // clear province since it does not apply anymore
                }
            }

            // Transfer mount points
            List<MountPoint> mountPointsInCell = new List<MountPoint>();
            int mountPointCount = GetMountPoints(sourceRegion, mountPointsInCell);
            for (int k = 0; k < mountPointCount; k++) {
                MountPoint mp = mountPointsInCell[k];
                if (mp.countryIndex != province.countryIndex) {
                    mp.countryIndex = province.countryIndex;
                    mp.provinceIndex = -1;  // same as cities - province cleared in case it's informed since it does not apply anymore
                }
            }

            // Add region to target country's polygon - only if the country is touching or crossing target country frontier
            Region targetRegion = province.mainRegion;
            RegionMagnet(sourceRegion, targetRegion);
            Clipper clipper = new Clipper();
            clipper.AddPath(sourceRegion, PolyType.ptClip);
            clipper.AddPaths(province.regions, PolyType.ptSubject);
            clipper.Execute(ClipType.ctUnion, province);

            // Finish operation with the country
            RegionSanitize(province.regions, true);
            RefreshProvinceGeometry(provinceIndex);

            // Substract cell region from any other country
            for (int k = 0; k < _provinces.Length; k++) {
                Province otherProvince = _provinces[k];
                if (otherProvince == province || !otherProvince.Overlaps(sourceRegion))
                    continue;
                clipper = new Clipper();
                clipper.AddPath(sourceRegion, PolyType.ptClip);
                clipper.AddPaths(otherProvince.regions, PolyType.ptSubject);
                clipper.Execute(ClipType.ctDifference, otherProvince);
                RegionSanitize(otherProvince.regions, true);
                int otherProvinceIndex = GetProvinceIndex(otherProvince);
                if (otherProvince.regions.Count == 0) {
                    ProvinceDeleteAndDependencies(otherProvinceIndex);
                    if (k >= otherProvinceIndex)
                        k--;
                } else {
                    RefreshProvinceGeometry(otherProvinceIndex);
                }
            }

            OptimizeFrontiers();

            if (redraw)
                Redraw();
            return true;
        }

    }

}