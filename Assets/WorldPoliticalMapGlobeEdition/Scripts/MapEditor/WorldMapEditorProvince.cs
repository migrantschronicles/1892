using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using WPM.Poly2Tri;

namespace WPM {
	public partial class WorldMapEditor : MonoBehaviour {

		public int provinceIndex = -1, provinceRegionIndex = -1;
		public int GUIProvinceIndex;
		public string GUIProvinceName = "";
		public string GUIProvinceNewName = "";
		public int GUIProvinceTransferToCountryIndex = -1;
		public int GUIProvinceMergeWithIndex = -1;
		public string GUIProvinceToNewCountryName = "";
		public bool provinceChanges;
        // if there's any pending change to be saved
        public bool provinceAttribChanges;

        int lastProvinceCount = -1;
		string[] _provinceNames;
		string[] emptyStringArray = new string[0];

		public string[] provinceNames {
			get {
				if (countryIndex == -1 || map.countries[countryIndex].provinces == null)
					return emptyStringArray;
				if (lastProvinceCount != map.countries [countryIndex].provinces.Length) {
					provinceIndex = -1;
					ReloadProvinceNames ();
				}
				return _provinceNames;
			}
		}

		string[] _provinceNeighbourCountriesNames;

		public string[] provinceNeighbourCountriesNames {
			get {
				if (_provinceNeighbourCountriesNames == null)
					ReloadProvinceCountriesNeighboursNames ();
				return _provinceNeighbourCountriesNames;
			}
		}


		#region Editor functionality

		
		public void ClearProvinceSelection () {
			map.HideProvinceRegionHighlights (true);
			map.HideProvinces ();
			provinceIndex = -1;
			provinceRegionIndex = -1;
			GUIProvinceName = "";
			GUIProvinceNewName = "";
			GUIProvinceIndex = -1;
			GUIProvinceToNewCountryName = "";
		}

		public bool ProvinceSelectByScreenClick (int countryIndex, Ray ray) {
			int targetProvinceIndex, targetRegionIndex;
			if (map.GetProvinceIndex (countryIndex, ray, out targetProvinceIndex, out targetRegionIndex)) {
				provinceIndex = targetProvinceIndex;
				if (provinceIndex >= 0 && countryIndex != map.provinces [provinceIndex].countryIndex) { // sanity check
					ClearSelection ();
					countryIndex = map.provinces [provinceIndex].countryIndex;
					countryRegionIndex = map.countries [countryIndex].mainRegionIndex;
					CountryRegionSelect ();
				}
				provinceRegionIndex = targetRegionIndex;
				ProvinceRegionSelect ();
				return true;
			}
			return false;
		}

		bool GetProvinceIndexByGUISelection () {
			if (GUIProvinceIndex < 0 || GUIProvinceIndex >= provinceNames.Length)
				return false;
			string[] s = provinceNames [GUIProvinceIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				GUIProvinceName = s [0].Trim ();
				if (int.TryParse (s [1], out provinceIndex)) {
					provinceRegionIndex = map.provinces [provinceIndex].mainRegionIndex;
					return true;
				}
			}
			return false;
		}

		public void ProvinceSelectByCombo (int selection) {
			GUIProvinceName = "";
			GUIProvinceIndex = selection;
			if (GetProvinceIndexByGUISelection ()) {
				if (Application.isPlaying) {
					map.BlinkProvince (provinceIndex, Color.black, Color.green, 1.2f, 0.2f);
				}
			}
			ProvinceRegionSelect ();
		}

		public void ReloadProvinceNames () {
			if (map == null || map.provinces == null || countryIndex < 0 || countryIndex >= map.countries.Length) {
				return;
			}
			_provinceNames = map.GetProvinceNames (countryIndex);
			lastProvinceCount = _provinceNames.Length; 
			SyncGUIProvinceSelection ();
			ProvinceRegionSelect (); // refresh selection
		}

		public void ProvinceRegionSelect () {
			if (countryIndex < 0 || countryIndex >= map.countries.Length || provinceIndex < 0 || provinceIndex >= map.provinces.Length || editingMode != EDITING_MODE.PROVINCES)
				return;

			// Checks country selected is correct
			Province province = map.provinces [provinceIndex];
			if (province.countryIndex != countryIndex) {
				ClearSelection ();
				countryIndex = province.countryIndex;
				countryRegionIndex = map.countries [countryIndex].mainRegionIndex;
				CountryRegionSelect ();
			}

			// Just in case makes GUICountryIndex selects appropiate value in the combobox
			GUIProvinceName = province.name;
			SyncGUIProvinceSelection ();
			if (provinceIndex >= 0 && provinceIndex < map.provinces.Length) {
				GUIProvinceNewName = province.name;
				ProvinceHighlightSelection ();
			}
			ReloadProvinceCountriesNeighboursNames ();
			int provinceTargetIndex = GetProvinceTransferToIndex ();
			if (provinceTargetIndex == provinceIndex)
				GUIProvinceTransferToCountryIndex = -1;
			if (GUIProvinceTransferToCountryIndex < 0 && _provinceNeighbourCountriesNames.Length != _countryNames.Length)
				GUIProvinceTransferToCountryIndex = 1; // != means there's a neighbour section at top
			int provinceMergeIndex = GetProvinceMergeWithIndex ();
			if (provinceMergeIndex == provinceIndex)
				GUIProvinceMergeWithIndex = -1;

			RegionSelected ();
		}

		public void ProvinceSanitize () {
			if (provinceIndex < 0 || provinceIndex >= _map.provinces.Length)
				return;
			
			Province province = _map.provinces [provinceIndex];
			_map.RegionSanitize (province.regions, true);
			_map.HideProvinceSurfaces (provinceIndex, true);
			_map.RefreshProvinceDefinition (provinceIndex);
			provinceChanges = true;
		}

		void ProvinceHighlightSelection () {
			
			if (highlightedRegions == null)
				highlightedRegions = new List<Region> ();
			else
				highlightedRegions.Clear ();
			map.HideProvinceRegionHighlights (true);

			if (provinceIndex < 0 || provinceIndex >= map.provinces.Length || countryIndex < 0 || countryIndex >= map.countries.Length || map.countries [countryIndex].provinces == null ||
			             provinceRegionIndex < 0 || map.provinces [provinceIndex].regions == null || provinceRegionIndex >= map.provinces [provinceIndex].regions.Count)
				return;

			// Highlight current province
			for (int p = 0; p < map.countries [countryIndex].provinces.Length; p++) {
				Province province = map.countries [countryIndex].provinces [p];
				if (province.regions == null)
					continue;
				// if province is current province then highlight it
				if (province.name.Equals (map.provinces [provinceIndex].name)) {
					map.HighlightProvinceRegion (provinceIndex, provinceRegionIndex, false);
					highlightedRegions.Add (map.provinces [provinceIndex].regions [provinceRegionIndex]);
				} else {
					// if this province belongs to the country but it's not current province, add to the collection of highlighted (not colorize because country is already colorized and that includes provinces area)
					highlightedRegions.Add (province.regions [province.mainRegionIndex]);
				}
			}
			shouldHideEditorMesh = true;
		}

		void SyncGUIProvinceSelection () {
			// recover GUI country index selection
			if (GUIProvinceName.Length > 0 && provinceNames != null) {
				for (int k = 0; k < _provinceNames.Length; k++) {
					if (_provinceNames [k].TrimStart ().StartsWith (GUIProvinceName)) {
						GUIProvinceIndex = k;
						provinceIndex = map.GetProvinceIndex (countryIndex, GUIProvinceName);
						return;
					}
				}
			}
			GUIProvinceIndex = -1;
			GUIProvinceName = "";
		}

		public bool ProvinceRename () {
			if (countryIndex < 0 || provinceIndex < 0)
				return false;
			string prevName = map.provinces [provinceIndex].name;
			GUIProvinceNewName = GUIProvinceNewName.Trim ();
			if (prevName.Equals (GUIProvinceNewName))
				return false;
			if (map.ProvinceRename (countryIndex, prevName, GUIProvinceNewName)) {
				GUIProvinceName = GUIProvinceNewName;
				lastProvinceCount = -1;
				ReloadProvinceNames ();
				provinceChanges = true;
				cityChanges = true;
				return true;
			}
			return false;
		}

	
		/// <summary>
		/// Delete all provinces of current country. Called from DeleteCountry.
		/// </summary>
		void mDeleteCountryProvinces () {
			if (map.provinces == null)
				return;
			if (countryIndex < 0)
				return;

			map.HideProvinceRegionHighlights (true);
			map.countries [countryIndex].provinces = new Province[0];
			map.CountryDeleteProvinces (countryIndex);
			provinceChanges = true;
		}

		public void DeleteCountryProvinces () {
			mDeleteCountryProvinces ();
			ClearSelection ();
			RedrawFrontiers ();
			map.RedrawMapLabels ();
		}


		/// <summary>
		/// Delete all provinces of current country's continent.
		/// </summary>
		void DeleteProvincesSameContinent () {
			if (map.provinces == null)
				return;
			int numProvinces = map.provinces.Length;
			List<Province> newProvinces = new List<Province> (numProvinces);
			string continent = map.countries [countryIndex].continent;
			for (int k = 0; k < numProvinces; k++) {
				if (map.provinces [k] != null) {
					int c = map.provinces [k].countryIndex;
					if (!map.countries [c].continent.Equals (continent)) {
						newProvinces.Add (map.provinces [k]);
					}
				}
			}
			map.provinces = newProvinces.ToArray ();
			provinceChanges = true;
		}

		/// <summary>
		/// Deletes current region or province if this was the last region
		/// </summary>
		public void ProvinceDelete () {
			if (provinceIndex < 0 || provinceIndex >= map.provinces.Length)
				return;
			map.HideProvinceRegionHighlights (true);

			// Clears references from mount points
			if (map.mountPoints != null) {
				for (int k = 0; k < map.mountPoints.Count; k++) {
					map.mountPoints [k].provinceIndex = -1;
				}
			}
			// Remove it from the country array
			List<Province> newProvinces = new List<Province> (map.countries [countryIndex].provinces.Length - 1);
			for (int k = 0; k < map.countries [countryIndex].provinces.Length; k++)
				if (!map.countries [countryIndex].provinces [k].name.Equals (GUIProvinceName))
					newProvinces.Add (map.countries [countryIndex].provinces [k]);
			map.countries [countryIndex].provinces = newProvinces.ToArray ();
			// Remove from the global array
			newProvinces = new List<Province> (map.provinces.Length - 1);
			for (int k = 0; k < map.provinces.Length; k++) {
				if (k != provinceIndex) {
					newProvinces.Add (map.provinces [k]);
				}
			}
			map.provinces = newProvinces.ToArray ();

			ClearProvinceSelection ();
			RedrawFrontiers ();
			provinceChanges = true;
		}

		/// <summary>
		/// Deletes current region or province if this was the last region
		/// </summary>
		public void ProvinceRegionDelete () {
			if (provinceIndex < 0 || provinceIndex >= map.provinces.Length)
				return;
			map.HideProvinceRegionHighlights (true);
			
			if (map.provinces [provinceIndex].regions != null && map.provinces [provinceIndex].regions.Count > 1) {
				map.provinces [provinceIndex].regions.RemoveAt (provinceRegionIndex);
				map.RefreshProvinceDefinition (provinceIndex);
			} 
			ClearProvinceSelection ();
			RedrawFrontiers ();
			provinceChanges = true;
		}


		/// <summary>
		/// Creates a new province with the current shape
		/// </summary>
		public void ProvinceCreate () {
			if (newShape.Count < 3 || countryIndex < 0)
				return;

			provinceIndex = map.provinces.Length;
			provinceRegionIndex = 0;
			Province newProvince = new Province ("New Province" + (provinceIndex + 1).ToString (), countryIndex);
			Region region = new Region (newProvince, 0);
			region.spherePoints = newShape.ToArray ();
			newProvince.regions = new List<Region> ();
			newProvince.regions.Add (region);
			region.CheckWorldEdgesAndOffset();
			map.ProvinceAdd (newProvince);
			map.RefreshProvinceDefinition (provinceIndex);
			lastProvinceCount = -1;
			ReloadProvinceNames ();
			ProvinceRegionSelect ();
			provinceChanges = true;
		}

		/// <summary>
		/// Adds a new province to current province
		/// </summary>
		public void ProvinceRegionCreate () {
			if (newShape.Count < 3 || provinceIndex < 0)
				return;
			
			Province province = map.provinces [provinceIndex];
			if (province.regions == null)
				province.regions = new List<Region> ();
			provinceRegionIndex = province.regions.Count;
			Region region = new Region (province, provinceRegionIndex);
			region.spherePoints = newShape.ToArray ();
			if (province.regions == null)
				province.regions = new List<Region> ();
			province.regions.Add (region);
			region.CheckWorldEdgesAndOffset();
			map.RefreshProvinceDefinition (provinceIndex);
			provinceChanges = true;
			ProvinceRegionSelect ();
		}



		bool isProvinceNameUsed(string name) {
			if (_map.provinces == null)
				return false;

			for (int k = 0; k < _map.provinces.Length; k++) {
				if (_map.provinces[k].name.Equals(name))
					return true;
			}
			return false;
		}


		string GetProvinceUniqueName(string proposedName) {

			string goodName = proposedName;
			int suffix = 0;

			while (isProvinceNameUsed(goodName)) {
				suffix++;
				goodName = proposedName + suffix.ToString();
			}
			return goodName;
		}


		/// <summary>
		/// Creates a new province with the given region
		/// </summary>
		public void ProvinceCreate(Region region) {
			if (region == null)
				return;

			// Remove region from source entity
			IAdminEntity entity = region.entity;
			if (entity != null) {
				entity.regions.Remove(region);
				Country country;
				// Refresh entity definition
				if (region.entity is Country) {
					int countryIndex = _map.GetCountryIndex((Country)region.entity);
					country = _map.countries[countryIndex];
					_map.RefreshCountryGeometry(country);
				} else {
					int provinceIndex = map.GetProvinceIndex((Province)region.entity);
					country = _map.countries[_map.provinces[provinceIndex].countryIndex];
					_map.RefreshProvinceGeometry(provinceIndex);
				}
			}

			provinceIndex = map.provinces.Length;
			provinceRegionIndex = 0;
			string newProvinceName = GetProvinceUniqueName("New Province");
			Province newProvince = new Province(newProvinceName, countryIndex);
			Region newRegion = new Region(newProvince, 0);
			newRegion.UpdatePointsAndRect(region.latlon);
			newProvince.regions = new List<Region>();
			newProvince.regions.Add(newRegion);
			map.ProvinceAdd(newProvince);
			map.RefreshProvinceDefinition(provinceIndex);

			// Update cities
			List<City> cities = _map.GetCities(region);
			if (cities.Count > 0) {
				for (int k = 0; k < cities.Count; k++) {
					if (cities[k].province != newProvinceName) {
						cities[k].province = newProvinceName;
						cityChanges = true;
					}
				}
			}

			lastProvinceCount = -1;
			GUIProvinceName = newProvince.name;
			SyncGUIProvinceSelection();
			ProvinceRegionSelect();
			provinceChanges = true;
		}



		/// <summary>
		/// Changes province's owner to specified country
		/// </summary>
		public void ProvinceTransferTo () {
			if (provinceIndex < 0 || GUIProvinceTransferToCountryIndex < 0 || GUIProvinceTransferToCountryIndex >= countryNames.Length)
				return;
			
			// Get target country
			// recover GUI country index selection
			int targetCountryIndex = GetProvinceTransferToIndex ();
			if (targetCountryIndex < 0)
				return;

			map.HideCountryRegionHighlights (true);
			map.HideProvinceRegionHighlights (true);
			map.CountryTransferProvinceRegion (targetCountryIndex, map.provinces [provinceIndex].regions [provinceRegionIndex]);
			countryChanges = true;
			provinceChanges = true;
			cityChanges = true;
			mountPointChanges = true;
			countryIndex = targetCountryIndex;
			countryRegionIndex = map.countries [targetCountryIndex].mainRegionIndex;
			ProvinceRegionSelect ();
		}

		public void ProvinceToNewCountry () {

			if (map.GetCountryIndex (GUIProvinceToNewCountryName) >= 0) {
				Debug.LogError ("Country name is already in use.");
				return;
			}			

			// Add new country
			Province province = map.provinces [provinceIndex];
			string continent = map.GetCountry (province.countryIndex).continent;
			Country newCountry = new Country (GUIProvinceToNewCountryName, continent);
			
			// Create dummy region
			int newCountryIndex = map.CountryAdd (newCountry);
			
			// Transfer province
			map.HideCountryRegionHighlights (true);
			map.HideProvinceRegionHighlights (true);

			map.CountryTransferProvinceRegion (newCountryIndex, province.mainRegion);
			
			// Remove dummy region
			countryChanges = true;
			provinceChanges = true;
			cityChanges = true;
			mountPointChanges = true;
			lastCountryCount = -1;
			ReloadCountryNames ();
			countryIndex = newCountryIndex;
			countryRegionIndex = 0;
			CountryRegionSelect ();
			ProvinceRegionSelect ();
		}

		int GetProvinceTransferToIndex () {
			int targetProvinceIndex = -1;
			if (GUIProvinceTransferToCountryIndex < 0 || _provinceNeighbourCountriesNames == null)
				return -1;
			string[] s = _provinceNeighbourCountriesNames [GUIProvinceTransferToCountryIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				int.TryParse (s [1], out targetProvinceIndex);
			}
			return targetProvinceIndex;
		}

		/// <summary>
		/// Merge currently selected province into target province
		/// </summary>
		public void ProvinceMerge () {
			if (provinceIndex < 0 || GUIProvinceMergeWithIndex < 0 || GUIProvinceMergeWithIndex >= provinceNames.Length)
				return;
			
			// Get target country
			// recover GUI country index selection
			int targetProvinceIndex = GetProvinceMergeWithIndex ();
			if (targetProvinceIndex < 0)
				return;
			
			map.HideCountryRegionHighlights (true);
			map.HideProvinceRegionHighlights (true);
			_map.ProvinceMerge (targetProvinceIndex, provinceIndex, true);
			countryChanges = true;
			provinceChanges = true;
			cityChanges = true;
			mountPointChanges = true;
			provinceIndex = targetProvinceIndex;
			provinceRegionIndex = 0;
			ProvinceRegionSelect ();
		}

		int GetProvinceMergeWithIndex () {
			int targetProvinceIndex = -1;
			if (GUIProvinceMergeWithIndex < 0 || _provinceNames == null)
				return -1;
			string[] s = _provinceNames [GUIProvinceMergeWithIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				int.TryParse (s [1], out targetProvinceIndex);
			}
			return targetProvinceIndex;
		}

		
		/// <summary>
		/// Merges all provinces in each country so their number fits a given range
		/// </summary>
		/// <param name="min">Minimum number of provinces.</param>
		/// <param name="max">Maximum number of provinces.</param>
		public void ProvincesEqualize (int min, int max) {
			if (min < 1)
				return;
			if (max < min)
				max = min;
			
			map.showProvinces = true;
			map.drawAllProvinces = true;
			
			for (int c = 0; c < map.countries.Length; c++) {
				Country country = map.countries [c];
				if (country == null || country.provinces == null)
					continue;
				int targetProvCount = UnityEngine.Random.Range (min, max);
				int provCount = country.provinces.Length;
				float provStartSize = 0;
				while (provCount > targetProvCount) {
					// Take the smaller province and merges with a neighbour
					float minAreaSize = float.MaxValue;
					int provinceIndex = -1;
					for (int p = 0; p < provCount; p++) {
						Province prov = country.provinces [p];
						if (prov == null)
							continue;
						if (prov.regions == null)
							map.ReadProvincePackedString (prov);
						if (prov.regions == null || prov.regions.Count == 0 || prov.mainRegion.neighbours == null || prov.mainRegion.neighbours.Count == 0)
							continue;
						if (prov.regionsRect2DArea < minAreaSize && prov.regionsRect2DArea > provStartSize) {
							minAreaSize = prov.regionsRect2DArea;
							provinceIndex = map.GetProvinceIndex (prov);
						}
					}
					
					if (provinceIndex < 0)
						break;
					
					provStartSize = minAreaSize;
					
					// Get the smaller neighbour
					int neighbourIndex = -1;
					Province province = map.provinces [provinceIndex];
					int neighbourCount = province.mainRegion.neighbours.Count;
					minAreaSize = float.MaxValue;
					for (int n = 0; n < neighbourCount; n++) {
						Region neighbour = province.mainRegion.neighbours [n];
						Province neighbourProvince = (Province)neighbour.entity;
						if (neighbourProvince != null && neighbourProvince != province && neighbourProvince.countryIndex == c && neighbour.rect2DArea < minAreaSize) {
							int neighbourProvIndex = map.GetProvinceIndex (neighbourProvince);
							if (neighbourProvIndex >= 0) {
								minAreaSize = neighbour.rect2DArea;
								neighbourIndex = neighbourProvIndex;
							}
						}
					}
					if (neighbourIndex < 0)
						continue;
					
					// Merges province into neighbour
					string provinceSource = map.provinces [provinceIndex].name;
					string provinceTarget = map.provinces [neighbourIndex].name;
					if (!map.ProvinceTransferProvinceRegion (neighbourIndex, map.provinces [provinceIndex].mainRegion, false)) {
						Debug.LogWarning ("Country: " + map.countries [c].name + " => " + provinceSource + " failed merge into " + provinceTarget + ".");
						break;
					}
					provCount = country.provinces.Length;
				}
			}
			map.Redraw ();
			provinceChanges = true;
			cityChanges = true;
			mountPointChanges = true;
		}

		
		/// <summary>
		/// Detect duplicate province names in same country and assign a different name
		/// </summary>
		public bool FixDuplicateProvinces () {
			if (_map.provinces == null)
				return false;

			bool changes = false;
			// Check duplicate provinces
			for (int k = 0; k < map.provinces.Length; k++) {
				string provName1 = map.provinces [k].name;
				int countryIndex1 = map.provinces [k].countryIndex;
				for (int j = 0; j < map.provinces.Length; j++) {
					string provName2 = map.provinces [j].name;
					int countryIndex2 = map.provinces [j].countryIndex;
					if (j != k && countryIndex2 == countryIndex1 && provName2.Equals (provName1)) {
						Debug.Log (map.countries [countryIndex1].name + " had duplicate province: " + provName1);
						map.provinces [j].name = "[DUPLICATE] " + map.provinces [j].name;
						changes = true;
					}
				}
			}
			if (changes)
				provinceChanges = true;
			return changes;
		}

		#endregion

	}
}
