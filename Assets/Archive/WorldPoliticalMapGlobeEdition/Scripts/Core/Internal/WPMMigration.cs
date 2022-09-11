// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

/*
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WPM.Poly2Tri;
using WPM.ClipperLib;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        #region Migration code

        void SolveProvincesData() {
            TextAsset ta = Resources.Load<TextAsset>(_geodataResourcesPath + "/countries10-2d");
            SetCountryGeoData2D(ta.text);
            Country[] newCountries = this.countries;
            ta = Resources.Load<TextAsset>(_geodataResourcesPath + "/provinces10-2d");
            SetProvincesGeoData2D(ta.text);
            Province[] newProvinces = this.provinces;
            foreach (Province prov in newProvinces) ReadProvincePackedString2D(prov);
            ta = Resources.Load<TextAsset>(_geodataResourcesPath + "/cities10-2d");
            SetCityGeoData2D(ta.text);
            List<City> newCities = new List<City>(cities);

            ta = Resources.Load<TextAsset>(_geodataResourcesPath + "/countries10");
            SetCountryGeoData(ta.text);

            ta = Resources.Load<TextAsset>(_geodataResourcesPath + "/provinces10");
            string provinceDataString = ta.text;
            SetProvincesGeoData(provinceDataString);
            foreach (Province prov in provinces) ReadProvincePackedString(prov);

            ta = Resources.Load<TextAsset>(_geodataResourcesPath + "/cities10");
            string citiesDataString = ta.text;
            SetCityGeoData(citiesDataString);

            int totalDiff = 0;
            int totalNew = 0;

            Debug.Log("Old provinces: " + provinces.Length + " New provinces: " + newProvinces.Length);
            for (int k = 0; k < newProvinces.Length; k++) {
                Province newProvince = newProvinces[k];
                int similarId = -1;
                int bestScore = 0;
                string newCountryName = newCountries[newProvince.countryIndex].name;
                if (string.IsNullOrEmpty(newCountryName)) continue;

                for (int j = 0; j < provinces.Length; j++) {
                    Province oldProvince = provinces[j];
                    Country oldCountry = countries[oldProvince.countryIndex];
                    if (oldCountry.provinces == null) continue;
                    string oldCountryName = oldCountry.name;
                    if (newCountryName != oldCountryName) continue;

                    if (oldProvince.name == newProvince.name) {
                        similarId = j;
                        break;
                    }

                    int score = SimilarScore(newProvince.name, oldProvince.name);
                    if (score > bestScore) {
                        bestScore = score;
                        similarId = j;
                    }
                }
                if (similarId < 0) {
                    for (int u = 0; u < this.countries.Length; u++) {
                        if (this.countries[u].name == newCountryName) {
                            if (this.countries[u].provinces == null) {
                                int newCountryIndex = newProvince.countryIndex;
                                Country newCountry = newCountries[newCountryIndex];
                                // Añade todas las provincias
                                foreach (Province prov in newCountry.provinces) {
                                    Debug.Log("$$$ New province: " + prov.name + " (Country: " + newCountryName + ") | Old country has no provinces");
                                    // Add province
                                    prov.countryIndex = u;
                                    ProvinceAdd(prov);
                                    // Añade ciudades
                                    for (int c = 0; c < newCities.Count; c++) {
                                        City newCity = newCities[c];
                                        if (newCity.countryIndex == newCountryIndex && newCity.province == prov.name) {
                                            newCity.countryIndex = u;
                                            cities.Add(newCity);
                                            Debug.Log("$$$ New City: " + newCity.name + " (" + newCity.province + ") ");
                                        }
                                    }
                                }
                                newCountry.name = "";
                                totalNew++;
                            }
                            break;
                        }
                    }
                    //} else if (newProvince.name != provinces[similarId].name) {

                    //    string bestByName = provinces[similarId].name;
                    //    if (newProvince.name[0] == bestByName[0] && newProvince.name[newProvince.name.Length - 1] == bestByName[bestByName.Length - 1]) {
                    //        if (newProvince.name.Substring(0, 2) == bestByName.Substring(0, 2) || newProvince.name.Substring(newProvince.name.Length - 2, 2) == bestByName.Substring(bestByName.Length - 2, 2)) {
                    //            totalDiff++;
                    //            int countryIndex = provinces[similarId].countryIndex;
                    //            string oldCountryName = countries[countryIndex].name;
                    //            //provinceDataString = provinceDataString.Replace(bestByName, newProvince.name);
                    //            //citiesDataString = citiesDataString.Replace(bestByName, newProvince.name);
                    //            //Debug.Log("*** Diff Country: " + newCountryName + ", Prov New: " + newProvince.name + ", Prov Old: " + bestByName);
                    //            //string oldProvinceName = provinces[similarId].name;
                    //            //// update cities ref
                    //            //for (int c = 0; c < cities.Count; c++) {
                    //            //    City city = cities[c];
                    //            //    if (city.countryIndex == countryIndex && city.province == oldProvinceName) {
                    //            //        //Debug.Log("City updated: " + city.name + " (old province name: " + city.province + ", new name: " + newProvince.name + ", ");
                    //            //        city.province = newProvince.name;
                    //            //    }
                    //            //}
                    //            provinces[similarId].name = newProvince.name;
                    //        }
                    //    }
                }
            }
            Debug.Log("Total Province Diff: " + totalDiff + ", Total New: " + totalNew);

            // Save back file
            string newData = GetProvinceGeoData();
            string fullPathName = "Assets/WorldPoliticalMapGlobeEdition/Resources/Geodata/provinces10-new.txt";
            File.WriteAllText(fullPathName, newData, System.Text.Encoding.UTF8);

            newData = GetCityGeoData();
            fullPathName = "Assets/WorldPoliticalMapGlobeEdition/Resources/Geodata/cities10-new.txt";
            File.WriteAllText(fullPathName, newData, System.Text.Encoding.UTF8);

            UnityEditor.AssetDatabase.Refresh();
        }


        int SimilarScore(string s1, string s2) {

            int min = Mathf.Min(s1.Length, s2.Length);
            int score = 0;

            s1 = s1.ToUpper();
            s2 = s2.ToUpper();

            // do they start same
            for (int k = 0; k < min; k++) {
                if (s2[k] == s1[k]) {
                    score += 5;
                } else {
                    break;
                }
            }

            // do they end same
            for (int k = min - 1; k >= 0; k--) {
                if (s2[k] == s1[k]) {
                    score += 5;
                } else {
                    break;
                }
            }

            // check consonants
            int s1k = 0;
            int s2k = 0;
            while (s1k < min && s2k < min) {
                char c = s1[s1k++];
                if (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U') continue;
                if (c < 'A' || c > 'Z') continue;
                char d = s2[s2k++];
                while (c != d && s2k < min) {
                    d = s2[s2k++];
                    if (c == d) {
                        score += min + 2 - Mathf.Abs(s2k - s1k);
                        break;
                    }
                }
            }
            return score;
        }


        public void SetCountryGeoData2D(string s) {
            string[] countryList = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            int countryCount = countryList.Length;
            countries = new Country[countryCount];

            char[] separatorCountries = new char[] { '$' };
            char[] separatorRegions = new char[] { '*' };
            char[] separatorCoordinates = new char[] { ';' };

            Vector2 max, min;
            Vector2 minCountry, maxCountry;

            for (int k = 0; k < countryCount; k++) {
                string[] countryInfo = countryList[k].Split(separatorCountries, StringSplitOptions.RemoveEmptyEntries);
                string name = countryInfo[0];
                string continent = countryInfo[1];
                Country country = new Country(name, continent);
                string[] regions = countryInfo[2].Split(separatorRegions, StringSplitOptions.RemoveEmptyEntries);
                int regionCount = regions.Length;
                country.regions = new List<Region>();
                float maxVol = 0;
                minCountry.x = minCountry.y = float.MaxValue;
                maxCountry.x = maxCountry.y = float.MinValue;
                for (int r = 0; r < regionCount; r++) {
                    string[] coordinates = regions[r].Split(separatorCoordinates, StringSplitOptions.RemoveEmptyEntries);
                    int coorCount = coordinates.Length;
                    if (coorCount < 3)
                        continue;
                    min.x = min.y = float.MaxValue;
                    max.x = max.y = float.MinValue;
                    Region countryRegion = new Region(country, country.regions.Count);
                    Vector2[] latlon = new Vector2[coorCount];
                    for (int c = 0; c < coorCount; c++) {
                        float x, y;
                        GetPointFromPackedString(coordinates[c], out x, out y);
                        x *= 180f;
                        y *= 360f;
                        if (x < min.x)
                            min.x = x;
                        if (x > max.x)
                            max.x = x;
                        if (y < min.y)
                            min.y = y;
                        if (y > max.y)
                            max.y = y;
                        latlon[c].x = x;
                        latlon[c].y = y;
                    }

                    Vector2 latlonCenter = Misc.Vector2zero;
                    FastVector.Average(ref min, ref max, ref latlonCenter);
                    countryRegion.latlonCenter = latlonCenter;

                    countryRegion.UpdatePointsAndRect(latlon);
                    // Calculate country bounding rect
                    if (min.x < minCountry.x)
                        minCountry.x = min.x;
                    if (min.y < minCountry.y)
                        minCountry.y = min.y;
                    if (max.x > maxCountry.x)
                        maxCountry.x = max.x;
                    if (max.y > maxCountry.y)
                        maxCountry.y = max.y;
                    float vol = (max - min).sqrMagnitude;
                    if (vol > maxVol) {
                        maxVol = vol;
                        country.mainRegionIndex = country.regions.Count;
                        country.latlonCenter = countryRegion.latlonCenter;
                    }
                    country.regions.Add(countryRegion);
                }
                // hidden
                if (countryInfo.Length >= 4) {
                    int hidden;
                    if (int.TryParse(countryInfo[3], out hidden)) {
                        country.hidden = hidden > 0;
                    }
                }
                country.regionsRect2D = new Rect(minCountry.x, minCountry.y, Math.Abs(maxCountry.x - minCountry.x), Mathf.Abs(maxCountry.y - minCountry.y));
                countries[k] = country;
            }
        }

        static System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;

        void SetCityGeoData2D(string s) {
            string[] cityList = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            char[] citySep = new char[] { '$' };
            int cityCount = cityList.Length;
            cities = new List<City>(cityCount);
            for (int k = 0; k < cityCount; k++) {
                string[] cityInfo = cityList[k].Split(citySep);
                string country = cityInfo[2];
                int countryIndex = GetCountryIndex(country);
                if (countryIndex >= 0) {
                    string name = cityInfo[0];
                    string province = cityInfo[1];
                    int population = int.Parse(cityInfo[3]);
                    float x = float.Parse(cityInfo[4], invariantCulture);
                    float y = float.Parse(cityInfo[5], invariantCulture);
                    CITY_CLASS cityClass = (CITY_CLASS)int.Parse(cityInfo[6], invariantCulture);
                    x *= 180f;
                    y *= 360f;
                    Vector3 wpos = Conversion.GetSpherePointFromLatLon(x, y);
                    City city = new City(name, province, countryIndex, population, wpos, cityClass);
                    cities.Add(city);
                }
            }
        }


        void SetProvincesGeoData2D(string s) {
            string[] provincesPackedStringData = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            char[] separatorProvinces = new char[] { '$' };
            int provinceCount = provincesPackedStringData.Length;
            List<Province> newProvinces = new List<Province>(provinceCount);
            List<Province>[] countryProvinces = new List<Province>[countries.Length];
            for (int k = 0; k < provinceCount; k++) {
                string[] provinceInfo = provincesPackedStringData[k].Split(separatorProvinces, StringSplitOptions.RemoveEmptyEntries);
                if (provinceInfo.Length <= 2)
                    continue;
                string name = provinceInfo[0];
                string countryName = provinceInfo[1];
                int countryIndex = GetCountryIndex(countryName);
                if (countryIndex >= 0) {
                    Province province = new Province(name, countryIndex);
                    province.packedRegions = provinceInfo[2];
                    newProvinces.Add(province);
                    if (countryProvinces[countryIndex] == null) {
                        countryProvinces[countryIndex] = new List<Province>(50);
                    }
                    countryProvinces[countryIndex].Add(province);
                }
            }
            _provinces = newProvinces.ToArray();
            for (int k = 0; k < countries.Length; k++) {
                if (countryProvinces[k] != null) {
                    countries[k].provinces = countryProvinces[k].ToArray();
                }
            }
        }

        public void ReadProvincePackedString2D(Province province) {
            string[] regions = province.packedRegions.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            int regionCount = regions.Length;
            province.regions = new List<Region>(regionCount);
            float maxVol = float.MinValue;
            char[] separatorRegions = new char[] { ';' };
            Vector2 minProvince;
            Vector2 maxProvince;
            minProvince.x = minProvince.y = float.MaxValue;
            maxProvince.x = maxProvince.y = float.MinValue;


            for (int r = 0; r < regionCount; r++) {
                string[] coordinates = regions[r].Split(separatorRegions, StringSplitOptions.RemoveEmptyEntries);
                int coorCount = coordinates.Length;
                if (coorCount < 3)
                    continue;
                Vector2 min, max;
                min.x = min.y = float.MaxValue;
                max.x = max.y = float.MinValue;
                Region provinceRegion = new Region(province, province.regions.Count);
                Vector2[] latlon = new Vector2[coorCount];
                for (int c = 0; c < coorCount; c++) {
                    float x, y;
                    GetPointFromPackedString(coordinates[c], out x, out y);
                    x *= 180f;
                    y *= 360f;
                    latlon[c].x = x;
                    latlon[c].y = y;

                    if (x < min.x)
                        min.x = x;
                    if (x > max.x)
                        max.x = x;
                    if (y < min.y)
                        min.y = y;
                    if (y > max.y)
                        max.y = y;
                }

                Vector2 latlonCenter = Misc.Vector2zero;
                FastVector.Average(ref min, ref max, ref latlonCenter);
                provinceRegion.latlonCenter = latlonCenter;

                provinceRegion.UpdatePointsAndRect(latlon);

                province.regions.Add(provinceRegion);

                // Calculate country bounding rect
                if (min.x < minProvince.x)
                    minProvince.x = min.x;
                if (min.y < minProvince.y)
                    minProvince.y = min.y;
                if (max.x > maxProvince.x)
                    maxProvince.x = max.x;
                if (max.y > maxProvince.y)
                    maxProvince.y = max.y;
                float vol = (max - min).sqrMagnitude;
                if (vol > maxVol) {
                    maxVol = vol;
                    province.mainRegionIndex = r;
                    province.latlonCenter = provinceRegion.latlonCenter;
                }
            }
            province.regionsRect2D = new Rect(minProvince.x, minProvince.y, Math.Abs(maxProvince.x - minProvince.x), Mathf.Abs(maxProvince.y - minProvince.y));
        }



        #endregion

    }

}
*/