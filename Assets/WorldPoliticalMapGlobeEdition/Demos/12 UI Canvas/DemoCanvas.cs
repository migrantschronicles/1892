using UnityEngine;
using UnityEngine.UI;

namespace WPM {

    public class DemoCanvas : MonoBehaviour {

        public GameObject prefab;
        WorldMapGlobe map;
        GUIStyle labelStyle;
        GameObject currentPanel;

        void Start() {
            // Get a reference to the World Map API:
            map = WorldMapGlobe.instance;

            // UI Setup - non-important, only for this demo
            labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = Color.white;

            // setup GUI resizer - only for the demo
            GUIResizer.Init(800, 500);

        }


        void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                AddPanel();
            }
        }


        void AddPanel() {

            // If previous panel exists, destroy it
            if (currentPanel != null) {
                Destroy(currentPanel);
            }

            // Instantiate panel
            currentPanel = Instantiate<GameObject>(prefab);

            // Update panel texts
            Text countryName, provinceName, cityName, population;
            countryName = currentPanel.transform.Find("Panel/RowCountry/CountryName").GetComponent<Text>();
            provinceName = currentPanel.transform.Find("Panel/RowProvince/ProvinceName").GetComponent<Text>();
            cityName = currentPanel.transform.Find("Panel/RowCity/CityName").GetComponent<Text>();
            population = currentPanel.transform.Find("Panel/RowCityData1/CityData1Value").GetComponent<Text>();

            // Gets a random city and populate data
            int cityIndex = Random.Range(0, map.cities.Count - 1);
            City city = map.GetCity(cityIndex);
            cityName.text = city.name;
            population.text = city.population.ToString();
            countryName.text = map.GetCityCountryName(cityIndex);
            provinceName.text = map.GetCityProvinceName(cityIndex);
            
            // Position the canvas over the globe
            float distaceToGlobeCenter = 1.2f;
            Vector3 worldPos = map.transform.TransformPoint(city.localPosition * distaceToGlobeCenter);
            currentPanel.transform.position = worldPos;

            // Draw a circle around the city
            map.AddMarker(MARKER_TYPE.CIRCLE_PROJECTED, city.localPosition, 100, 0.8f, 1f, Color.green);

            // Parent the panel to the globe so it rotates with it
            currentPanel.transform.SetParent(map.transform, true);

            // Finally fly to city to show the panel
            map.FlyToCity(cityIndex, 2f);
        }


    }

}

