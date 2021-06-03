using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
    public partial class WorldMapEditor : MonoBehaviour {

        public int GUICityIndex;
        public string GUICityName = "";
        public string GUICityNewName = "";
        public string GUICityPopulation = "";
        public string GUICityProvince = "";
        public Vector2 GUICityLatLon;
        public CITY_CLASS GUICityClass = CITY_CLASS.CITY;
        public int cityIndex = -1;
        public bool cityChanges;
        // if there's any pending change to be saved
        public bool cityAttribChanges;

        // private fields
        int lastCityCount = -1;
        string[] _cityNames;

        public string[] cityNames {
            get {
                if (map.cities != null && lastCityCount != map.cities.Count) {
                    cityIndex = -1;
                    ReloadCityNames();
                }
                return _cityNames;
            }
        }


        #region Editor functionality


        public void ClearCitySelection() {
            map.HideCityHighlights();
            cityIndex = -1;
            GUICityName = "";
            GUICityIndex = -1;
            GUICityNewName = "";
        }


        /// <summary>
        /// Adds a new city to current country.
        /// </summary>
        public void CityCreate(Vector3 newPoint) {
            if (countryIndex < 0)
                return;
            GUICityName = "New City " + (map.cities.Count + 1);
            newPoint = newPoint.normalized * 0.5f;
            City newCity = new City(GUICityName, GUIProvinceName, countryIndex, 100, newPoint, GUICityClass);
            map.cities.Add(newCity);
            map.DrawCities();
            lastCityCount = -1;
            ReloadCityNames();
            cityChanges = true;
        }

        public bool CityUpdate() {
            if (cityIndex < 0)
                return false;

            bool changes = false;
            City city = map.cities[cityIndex];
            if (city.cityClass != GUICityClass) {
                city.cityClass = GUICityClass;
                changes = true;
            }
            int newPopulation;
            if (int.TryParse(GUICityPopulation, out newPopulation) && city.population != newPopulation) {
                city.population = newPopulation;
                changes = true;
            }
            if (city.latlon != GUICityLatLon) {
                city.latlon = GUICityLatLon;
                changes = true;
            }

            string prevName = city.name;
            GUICityNewName = GUICityNewName.Trim();
            if (!prevName.Equals(GUICityNewName)) {
                city.name = GUICityNewName;
                GUICityName = GUICityNewName;
                lastCityCount = -1;
                ReloadCityNames();
                changes = true;
            }

            if (changes) {
                map.DrawCities();
                cityChanges = true;
            }
            return true;
        }


        public void CityMove(Vector3 destination) {
            if (cityIndex < 0)
                return;
            map.cities[cityIndex].localPosition = destination.normalized * 0.5f;
            UpdateCityVisualPosition();
            cityChanges = true;
        }

        void UpdateCityVisualPosition() {
            GameObject cityObj = map.cities[cityIndex].gameObject;
            if (cityObj != null) {
                cityObj.transform.localPosition = map.cities[cityIndex].localPosition * 1.001f;
            }
        }

        public void CitySelectByCombo(int selection) {
            GUICityName = "";
            GUICityIndex = selection;
            if (GetCityIndexByGUISelection()) {
                if (Application.isPlaying) {
                    map.BlinkCity(cityIndex, Color.black, Color.green, 1.2f, 0.2f);
                }
            }
            CitySelect();
        }

        bool GetCityIndexByGUISelection() {
            if (GUICityIndex < 0 || GUICityIndex >= cityNames.Length)
                return false;
            string[] s = cityNames[GUICityIndex].Split(new char[] {
                '(',
                ')'
            }, System.StringSplitOptions.RemoveEmptyEntries);
            if (s.Length >= 2) {
                GUICityName = s[0].Trim();
                if (int.TryParse(s[1], out cityIndex)) {
                    return true;
                }
            }
            return false;
        }

        public void CitySelect() {
            if (cityIndex < 0 || cityIndex > map.cities.Count)
                return;

            // If no country is selected (the city could be at sea) select it
            City city = map.cities[cityIndex];
            int cityCountryIndex = city.countryIndex;
            if (cityCountryIndex < 0) {
                SetInfoMsg("Country not found in this country file.");
            }

            if (countryIndex != cityCountryIndex && cityCountryIndex >= 0) {
                ClearSelection();
                countryIndex = cityCountryIndex;
                countryRegionIndex = map.countries[countryIndex].mainRegionIndex;
                CountryRegionSelect();
            }

            // Just in case makes GUICountryIndex selects appropiate value in the combobox
            GUICityName = city.name;
            GUICityPopulation = city.population.ToString();
            GUICityClass = city.cityClass;
            GUICityProvince = city.province;
            GUICityLatLon = city.latlon;
            SyncGUICitySelection();
            if (cityIndex >= 0) {
                GUICityNewName = city.name;
                CityHighlightSelection();
            }
        }

        public bool CitySelectByScreenClick(Ray ray) {
            int targetCityIndex;
            if (map.GetCityIndex(ray, out targetCityIndex)) {
                cityIndex = targetCityIndex;
                CitySelect();
                return true;
            }
            return false;
        }

        void CityHighlightSelection() {

            if (cityIndex < 0 || cityIndex >= map.cities.Count)
                return;

            // Colorize city
            map.HideCityHighlights();
            map.ToggleCityHighlight(cityIndex, Color.blue, true);
        }

        public void ReloadCityNames() {
            if (map == null || map.cities == null) {
                lastCityCount = -1;
                return;
            }
            lastCityCount = map.cities.Count; // check this size, and not result from GetCityNames because it could return additional rows (separators and so)
            _cityNames = map.GetCityNames(countryIndex, true);
            SyncGUICitySelection();
            CitySelect(); // refresh selection
        }

        void SyncGUICitySelection() {
            // recover GUI city index selection
            if (GUICityName.Length > 0) {
                for (int k = 0; k < cityNames.Length; k++) {
                    if (_cityNames[k].TrimStart().StartsWith(GUICityName)) {
                        GUICityIndex = k;
                        if (provinceIndex >= 0) {
                            cityIndex = map.GetCityIndexInProvince(provinceIndex, GUICityName);
                        }
                        if (cityIndex < 0) {
                            cityIndex = map.GetCityIndexInCountry(countryIndex, GUICityName);
                        }
                        return;
                    }
                }
                if (map.GetCityIndex(GUICityName) < 0) {
                    SetInfoMsg("City " + GUICityName + " not found in database.");
                }
            }
            GUICityIndex = -1;
            GUICityName = "";
        }

        /// <summary>
        /// Deletes current city
        /// </summary>
        public void DeleteCity() {
            if (cityIndex < 0 || cityIndex >= map.cities.Count)
                return;

            map.HideCityHighlights();
            map.cities.RemoveAt(cityIndex);
            cityIndex = -1;
            GUICityName = "";
            SyncGUICitySelection();
            map.DrawCities();
            cityChanges = true;
        }

        /// <summary>
        /// Deletes all cities of current selected country
        /// </summary>
        public void DeleteCountryCities() {
            if (countryIndex < 0)
                return;

            map.HideCityHighlights();
            int k = -1;
            while (++k < map.cities.Count) {
                if (map.cities[k].countryIndex == countryIndex) {
                    map.cities.RemoveAt(k);
                    k--;
                }
            }
            cityIndex = -1;
            GUICityName = "";
            SyncGUICitySelection();
            map.DrawCities();
            cityChanges = true;
        }


        /// <summary>
        /// Deletes all cities of current selected country's continent
        /// </summary>
        public void DeleteCitiesSameContinent() {
            if (countryIndex < 0)
                return;

            map.HideCityHighlights();
            int k = -1;
            string continent = map.countries[countryIndex].continent;
            while (++k < map.cities.Count) {
                int cindex = map.cities[k].countryIndex;
                if (cindex >= 0) {
                    string cityContinent = map.countries[cindex].continent;
                    if (cityContinent.Equals(continent)) {
                        map.cities.RemoveAt(k);
                        k--;
                    }
                }
            }
            cityIndex = -1;
            GUICityName = "";
            SyncGUICitySelection();
            map.DrawCities();
            cityChanges = true;
        }

        /// <summary>
        /// Calculates correct province for cities
        /// </summary>
        public void FixOrphanCities() {
            if (_map.provinces == null)
                return;

            int countryAssigned = 0;
            int provinceAssigned = 0;

            int cityCount = _map.cities.Count;
            for (int c = 0; c < cityCount; c++) {
                City city = _map.cities[c];
                if (city.countryIndex == -1) {
                    for (int k = 0; k < _map.countries.Length; k++) {
                        Country co = _map.countries[k];
                        if (co.regions == null)
                            continue;
                        int regCount = co.regions.Count;
                        for (int kr = 0; kr < regCount; kr++) {
                            if (co.regions[kr].Contains(city.latlon)) {
                                city.countryIndex = k;
                                countryAssigned++;
				cityChanges = true;
                                k = 100000;
                                break;
                            }
                        }
                    }
                }
                if (city.countryIndex == -1) {
                    float minDist = float.MaxValue;
                    for (int k = 0; k < _map.countries.Length; k++) {
                        Country co = _map.countries[k];
                        if (co.regions == null)
                            continue;
                        int regCount = co.regions.Count;
                        for (int kr = 0; kr < regCount; kr++) {
                            float dist = (co.regions[kr].latlonCenter - city.latlon).sqrMagnitude;
                            if (dist < minDist) {
                                minDist = dist;
                                city.countryIndex = k;
				cityChanges = true;
                                countryAssigned++;
                            }
                        }
                    }
                }

                if (city.province.Length == 0) {
                    Country country = _map.countries[city.countryIndex];
                    if (country.provinces == null)
                        continue;
                    for (int p = 0; p < country.provinces.Length; p++) {
                        Province province = country.provinces[p];
                        if (province.regions == null)
                            _map.ReadProvincePackedString(province);
                        if (province.regions == null)
                            continue;
                        int regCount = province.regions.Count;
                        for (int pr = 0; pr < regCount; pr++) {
                            Region reg = province.regions[pr];
                            if (reg.Contains(city.latlon)) {
                                city.province = province.name;
				cityChanges = true;
                                p = 100000;
                                break;
                            }
                        }
                    }
                }
            }

            for (int c = 0; c < cityCount; c++) {
                City city = _map.cities[c];
                if (city.province.Length == 0) {
                    float minDist = float.MaxValue;
                    int pg = -1;
                    for (int p = 0; p < _map.provinces.Length; p++) {
                        Province province = _map.provinces[p];
                        if (province.regions == null)
                            _map.ReadProvincePackedString(province);
                        if (province.regions == null)
                            continue;
                        int regCount = province.regions.Count;
                        for (int pr = 0; pr < regCount; pr++) {
                            Region pregion = province.regions[pr];
                            for (int prp = 0; prp < pregion.latlon.Length; prp++) {
                                float dist = (city.latlon - pregion.latlon[prp]).sqrMagnitude;
                                if (dist < minDist) {
                                    minDist = dist;
                                    pg = p;
                                }
                            }
                        }
                    }
                    if (pg >= 0) {
                        city.province = _map.provinces[pg].name;
                        provinceAssigned++;
			cityChanges = true;
                    }
                } else { // check for differences in capitals
#if !UNITY_WSA
                    if (!city.name.Equals(city.province) && String.Compare(city.name, city.province, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreNonSpace) == 0) {
                        city.name = city.province;
                        cityChanges = true;
                    }
#endif
                }
            }

            Debug.Log(countryAssigned + " cities were assigned a new country.");
            Debug.Log(provinceAssigned + " cities were assigned a new province.");

            if (countryAssigned > 0 || provinceAssigned > 0) {
                cityChanges = true;
            }
        }

        #endregion

   

    }
}
