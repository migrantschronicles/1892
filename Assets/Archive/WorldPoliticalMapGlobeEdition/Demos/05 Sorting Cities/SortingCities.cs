using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

	public class SortingCities : MonoBehaviour {

		WorldMapGlobe map;

		void Start () {
			Debug.Log ("This script contains code for sorting cities based on different criteria. Double click here to examine the code");

			map = WorldMapGlobe.instance;
			SortCitiesByPopulation ();

			SortCitiesByLatitude ();
		}

		void SortCitiesByPopulation () {

			List<City> sortedCities = new List<City> (map.cities);
			sortedCities.Sort (PopulationComparer);

			Debug.Log ("Less populated city: " + map.GetCityFullName(sortedCities [0]));
			Debug.Log ("Most populated city: " + map.GetCityFullName(sortedCities [sortedCities.Count - 1]));
		}

		int PopulationComparer (City city1, City city2) {
			return city1.population.CompareTo (city2.population);
		}


		void SortCitiesByLatitude () {
		
			List<City> sortedCities = new List<City> (map.cities);
			sortedCities.Sort (LatitudeComparer);
		
			Debug.Log ("Southernmost city: " + map.GetCityFullName(sortedCities [0]));
			Debug.Log ("Northernmost city: " + map.GetCityFullName(sortedCities [sortedCities.Count - 1]));
		}

		int LatitudeComparer (City city1, City city2) {
			return city1.latitude.CompareTo (city2.latitude);
		}



	}
}
