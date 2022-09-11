using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using WPM.Poly2Tri;
using WPM.PolygonTools;

namespace WPM {
	public partial class WorldMapEditor : MonoBehaviour {

		public int GUICountryIndex;
		public string GUICountryName = "";
		public string GUICountryNewName = "";
		public string GUICountryNewContinent = "";
		public string GUICountryNewFIPS10_4 = "";
		public string GUICountryNewISO_A2 = "";
		public string GUICountryNewISO_A3 = "";
		public string GUICountryNewISO_N3 = "";
		public int GUICountryTransferToCountryIndex = -1;
		public bool groupByParentAdmin = true;
		public int countryIndex = -1, countryRegionIndex = -1;
		public bool countryChanges;
        // if there's any pending change to be saved
        public bool countryAttribChanges;

        [SerializeField]
		bool _GUICountryHidden;

		public bool GUICountryHidden {
			get {
				return _GUICountryHidden;
			}
			set {
				if (_GUICountryHidden != value) {
					_GUICountryHidden = value;
					countryChanges = true;
					if (countryIndex >= 0 && _map.countries [countryIndex].hidden != _GUICountryHidden) {
						_map.countries [countryIndex].hidden = _GUICountryHidden;
						ClearSelection ();
						_map.OptimizeFrontiers ();
						_map.Redraw ();
					}
				}
			}
		}

		// private fields
		int lastCountryCount = -1;
		string[] _countryNames;


		public string[] countryNames {
			get {
				if (map.countries != null && lastCountryCount != map.countries.Length) {
					countryIndex = -1;
					ReloadCountryNames ();
				}
				return _countryNames;
			}
		}

		string[] _countryNeighboursNames;

		public string[] countryNeighboursNames {
			get {
				if (_countryNeighboursNames == null)
					ReloadCountryNeighboursNames ();
				return _countryNeighboursNames;
			}
		}


		#region Editor functionality


		public bool CountryRename () {
			if (countryIndex < 0)
				return false;
			string prevName = map.countries [countryIndex].name;
			GUICountryNewName = GUICountryNewName.Trim ();
			if (prevName.Equals (GUICountryNewName))
				return false;
			if (map.CountryRename (prevName, GUICountryNewName)) {
				GUICountryName = GUICountryNewName;
				lastCountryCount = -1;
				ReloadCountryNames ();
				map.RedrawMapLabels ();
				countryChanges = true;
				provinceChanges = true;
				cityChanges = true;
				mountPointChanges = true;
				return true;
			}
			return false;
		}

		public bool CountryChangeContinent () {
			if (countryIndex < 0 || countryIndex >= map.countries.Length)
				return false;
			map.countries [countryIndex].continent = GUICountryNewContinent;
			countryChanges = true;
			return true;
		}

		public bool CountryChangeFIPSAndISOCodes () {
			if (countryIndex < 0 || countryIndex >= map.countries.Length)
				return false;
			map.countries [countryIndex].fips10_4 = GUICountryNewFIPS10_4;
			map.countries [countryIndex].iso_a2 = GUICountryNewISO_A2;
			map.countries [countryIndex].iso_a3 = GUICountryNewISO_A3;
			map.countries [countryIndex].iso_n3 = GUICountryNewISO_N3;
			countryChanges = true;
			return true;
		}

		/// <summary>
		/// Updates all countries within same continent to new country name
		/// </summary>
		public bool ContinentRename () {

			if (countryIndex < 0)
				return false;

			string currentContinent = map.countries [countryIndex].continent;
			for (int k = 0; k < map.countries.Length; k++) {
				if (map.countries [k].continent.Equals (currentContinent))
					map.countries [k].continent = GUICountryNewContinent;
			}
			countryChanges = true;
			return true;
		}



		public void CountrySelectByCombo (int selection) {
			GUICountryName = "";
			GUICountryIndex = selection;
			if (GetCountryIndexByGUISelection ()) {
				if (Application.isPlaying) {
					map.BlinkCountry (countryIndex, Color.black, Color.green, 1.2f, 0.2f);
				}
			}
			CountryRegionSelect ();
		}

		bool GetCountryIndexByGUISelection () {
			if (GUICountryIndex < 0 || GUICountryIndex >= _countryNames.Length)
				return false;
			string[] s = _countryNames [GUICountryIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				GUICountryName = s [0].Trim ();
				if (int.TryParse (s [1], out countryIndex)) {
					countryRegionIndex = map.countries [countryIndex].mainRegionIndex;
					return true;
				}
			}
			return false;
		}

		public void CountryRegionSelect () {
			if (countryIndex < 0 || countryIndex > map.countries.Length)
				return;

			// Just in case makes GUICountryIndex selects appropiate value in the combobox
			GUICountryName = map.countries [countryIndex].name;
			SyncGUICountrySelection ();
			GUICountryNewName = map.countries [countryIndex].name;
			GUICountryNewContinent = map.countries [countryIndex].continent;
			GUICountryNewFIPS10_4 = map.countries [countryIndex].fips10_4;
			GUICountryNewISO_A2 = map.countries [countryIndex].iso_a2;
			GUICountryNewISO_A3 = map.countries [countryIndex].iso_a3;
			GUICountryNewISO_N3 = map.countries [countryIndex].iso_n3;
			_GUICountryHidden = map.countries [countryIndex].hidden;

			if (editingMode == EDITING_MODE.COUNTRIES)
				CountryHighlightSelection ();
			else if (editingMode == EDITING_MODE.PROVINCES) {
				map.HighlightCountryRegion (countryIndex, countryRegionIndex, false, true, Color.black);
				_map.DrawProvince (countryIndex, false, false);
			}
			lastCityCount = -1;

			// Autoselect transfer to country in case there's none selected yet
			ReloadCountryNeighboursNames ();
			int countryTargetIndex = GetCountryTransferToIndex ();
			if (countryTargetIndex == countryIndex)
				GUICountryTransferToCountryIndex = -1;
			if (GUICountryTransferToCountryIndex < 0 && _countryNeighboursNames.Length != _countryNames.Length)
				GUICountryTransferToCountryIndex = 1; // != means there's a neighbour section at top

			RegionSelected ();
		}

		public bool CountrySelectByScreenClick (Ray ray) {
			int targetCountryIndex, targetRegionIndex;
			if (map.GetCountryIndex (ray, out targetCountryIndex, out targetRegionIndex)) {
				countryIndex = targetCountryIndex;
				countryRegionIndex = targetRegionIndex;
				CountryRegionSelect ();
				return true;
			}
			return false;
		}

		void CountryHighlightSelection () {
			CountryHighlightSelection (null);
		}

		void CountryHighlightSelection (List <Region>filterRegions) {

			if (highlightedRegions == null)
				highlightedRegions = new List<Region> ();
			else
				highlightedRegions.Clear ();
			if (countryIndex < 0 || countryIndex >= map.countries.Length)
				return;
			if (countryRegionIndex >= map.countries [countryIndex].regions.Count)
				countryRegionIndex = map.countries [countryIndex].mainRegionIndex;

			// Colorize neighours
			Color color = new Color (1, 1, 1, 0.4f);
			map.HideCountryRegionHighlights (true);
			Region region = map.countries [countryIndex].regions [countryRegionIndex];
			for (int cr = 0; cr < region.neighbours.Count; cr++) {
				Region neighbourRegion = region.neighbours [cr];
				if (filterRegions == null || filterRegions.Contains (neighbourRegion)) {
					int c = map.GetCountryIndex ((Country)neighbourRegion.entity);
					if (c >= 0) {
						map.ToggleCountryRegionSurfaceHighlight (c, neighbourRegion.regionIndex, color, false);
						highlightedRegions.Add (neighbourRegion.entity.regions [neighbourRegion.regionIndex]);
					}
				}
			}
			map.HighlightCountryRegion (countryIndex, countryRegionIndex, false, false, Color.black);
			highlightedRegions.Add (region);

			shouldHideEditorMesh = true;
		}

		
		public void ReloadCountryNames () {
			if (map == null || map.countries == null) {
				lastCountryCount = -1;
				return;
			}
			lastCountryCount = map.countries.Length; // check this size, and not result from GetCountryNames
			string oldCountryTransferName = GetCountryTransferIndexByGUISelection ();
			_countryNames = map.GetCountryNames (groupByParentAdmin);
			SyncGUICountrySelection ();
			SyncGUICountryTransferSelection (oldCountryTransferName);
			CountryRegionSelect (); // refresh selection
		}

		public void ReloadCountryNeighboursNames () {
			if (countryIndex < 0 || _countryNames == null)
				return;
			string[] neighbourNames = map.GetCountryNeighboursNames (countryIndex, true);
			List<string> cn = new List<string> (neighbourNames);
			cn.AddRange (_countryNames);
			_countryNeighboursNames = cn.ToArray ();
		}

		public void ReloadProvinceCountriesNeighboursNames () {
			if (provinceIndex < 0 || _countryNames == null)
				return;
			string[] neighbourNames = map.GetProvinceCountriesNeighboursNames (provinceIndex, true);
			List<string> cn = new List<string> (neighbourNames);
			cn.AddRange (_countryNames);
			_provinceNeighbourCountriesNames = cn.ToArray ();
		}

		void SyncGUICountrySelection () {
			// recover GUI country index selection
			if (GUICountryName.Length > 0) {
				for (int k = 0; k < _countryNames.Length; k++) {  // don't use countryNames or the array will be reloaded again if grouped option is enabled causing an infinite loop
					if (_countryNames [k].TrimStart ().StartsWith (GUICountryName)) {
						GUICountryIndex = k;
						countryIndex = map.GetCountryIndex (GUICountryName);
						return;
					}
				}
				SetInfoMsg ("Country " + GUICountryName + " not found in this geodata file.");
			}
			GUICountryIndex = -1;
			GUICountryName = "";
		}

		string GetCountryTransferIndexByGUISelection () {
			if (_countryNames == null || GUICountryTransferToCountryIndex < 0 || GUICountryTransferToCountryIndex >= _countryNames.Length)
				return "";
			string[] s = _countryNames [GUICountryTransferToCountryIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				return s [0].Trim ();
			}
			return "";
		}

		void SyncGUICountryTransferSelection (string oldName) {
			// recover GUI country index selection
			if (oldName.Length > 0) {
				for (int k = 0; k < _countryNames.Length; k++) {  // don't use countryNames or the array will be reloaded again if grouped option is enabled causing an infinite loop
					if (_countryNames [k].TrimStart ().StartsWith (oldName)) {
						GUICountryTransferToCountryIndex = k;
						return;
					}
				}
				SetInfoMsg ("Country " + oldName + " not found in this geodata file.");
			}
			GUICountryTransferToCountryIndex = -1;
		}


		/// <summary>
		/// Deletes current region of country but not any of its dependencies
		/// </summary>
		public void CountryRegionDelete () {
			if (countryIndex < 0 || countryIndex >= map.countries.Length)
				return;
			map.HideCountryRegionHighlights (true);
			
			if (map.countries [countryIndex].regions.Count > 1) {
				map.countries [countryIndex].regions.RemoveAt (countryRegionIndex);
				map.RefreshCountryDefinition (countryIndex, null);
			}
			ClearSelection ();
			RedrawFrontiers ();
			map.RedrawMapLabels ();
			countryChanges = true;
		}

		/// <summary>
		/// Deletes completely the country and its dependencies
		/// </summary>
		public void CountryDelete () {
			if (countryIndex < 0 || countryIndex >= map.countries.Length)
				return;
			map.HideCountryRegionHighlights (true);

			mDeleteCountryProvinces ();
			DeleteCountryCities ();
			DeleteCountryMountPoints ();
			List<Country> newAdmins = new List<Country> (map.countries.Length - 1);
			for (int k = 0; k < map.countries.Length; k++) {
				if (k != countryIndex) {
					newAdmins.Add (map.countries [k]);
				}
			}
			map.countries = newAdmins.ToArray ();
			// Updates country index in provinces
			for (int k = 0; k < map.provinces.Length; k++) {
				if (map.provinces [k].countryIndex > countryIndex) {
					map.provinces [k].countryIndex--;
				}
			}
			// Updates country index in cities
			for (int k = 0; k < map.cities.Count; k++) {
				if (map.cities [k].countryIndex > countryIndex) {
					map.cities [k].countryIndex--;
				}
			}
			// Updates country index in mount points
			if (map.mountPoints != null) {
				for (int k = 0; k < map.mountPoints.Count; k++) {
					if (map.mountPoints [k].countryIndex > countryIndex) {
						map.mountPoints [k].countryIndex--;
					}
				}
			}

			ClearSelection ();
			RedrawFrontiers ();
			map.RedrawMapLabels ();
			countryChanges = true;
			shouldHideEditorMesh = true;
		}


		public void CountryDeleteSameContinent () {
			if (countryIndex < 0 || countryIndex >= map.countries.Length)
				return;
			
			string continent = map.countries [countryIndex].continent;
			map.CountriesDeleteFromContinent (continent);
			
			ClearSelection ();
			RedrawFrontiers ();
			map.RedrawMapLabels ();
			countryChanges = true;

			SyncGUICitySelection ();
			map.DrawCities ();
			cityChanges = true;
			
			SyncGUIProvinceSelection ();
			provinceChanges = true;
			mountPointChanges = true;

			shouldHideEditorMesh = true;
		}

		public void CountrySanitize () {
			if (countryIndex < 0 || countryIndex >= _map.countries.Length)
				return;

			Country country = _map.countries [countryIndex];
			int rcount = country.regions.Count;
			bool changes = false;
			for (int k = 0; k < rcount; k++) {
				Region region = country.regions [k];
				if (_map.RegionSanitize (region))
					changes = true;
			}
			if (changes) {
				_map.RefreshCountryDefinition (countryIndex, null);
				countryChanges = true;
			}
		}


		/// <summary>
		/// Makes one country to annex another. Used internally by Map Editor.
		/// </summary>
		public void CountryTransferTo () {
			if (countryIndex < 0 || GUICountryTransferToCountryIndex < 0)
				return;
			// Get target country
			// recover GUI country index selection
			int targetCountryIndex = GetCountryTransferToIndex ();
			if (targetCountryIndex < 0)
				return;
			map.HideCountryRegionHighlights (true);
			map.HideProvinceRegionHighlights (true);
			Country sourceCountry = map.countries [countryIndex];
			int mainRegionIndex = sourceCountry.mainRegionIndex;
			_map.CountryTransferCountryRegion (targetCountryIndex, sourceCountry.regions [mainRegionIndex]);
			countryChanges = true;
			if (editingMode == EDITING_MODE.PROVINCES) {
				provinceChanges = true;
			}
			cityChanges = true;
			mountPointChanges = true;
			countryIndex = targetCountryIndex;
			countryRegionIndex = map.countries [countryIndex].mainRegionIndex;
			CountryRegionSelect ();
			map.RedrawMapLabels ();
		}

		/// <summary>
		/// Makes one country to annex another. Used internally by Map Editor.
		/// </summary>
		public void CountryTransferAsProvinceTo() {
			if (countryIndex < 0 || GUICountryTransferToCountryIndex < 0)
				return;
			// Get target country
			// recover GUI country index selection
			int targetCountryIndex = GetCountryTransferToIndex();
			if (targetCountryIndex < 0)
				return;
			map.HideCountryRegionHighlights(true);
			map.HideProvinceRegionHighlights(true);
			_map.CountryTransferAsProvince(targetCountryIndex, countryIndex);
			countryChanges = true;
			provinceChanges = true;
			cityChanges = true;
			mountPointChanges = true;
			countryIndex = targetCountryIndex;
			countryRegionIndex = map.countries[countryIndex].mainRegionIndex;
			CountryRegionSelect();
			map.RedrawMapLabels();
		}

		int GetCountryTransferToIndex () {
			int targetCountryIndex = -1;
			if (GUICountryTransferToCountryIndex < 0 || _countryNeighboursNames == null)
				return -1;
			string[] s = _countryNeighboursNames [GUICountryTransferToCountryIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				int.TryParse (s [1], out targetCountryIndex);
			}
			return targetCountryIndex;
		}

		#endregion

	

		/// <summary>
		/// Exports the geographic data in packed string format with reduced quality.
		/// </summary>
		public string GetCountryGeoDataLowQuality () {

			// step 1: duplicate data
			IAdminEntity[] entities;
			if (editingMode == EDITING_MODE.COUNTRIES)
				entities = map.countries;
			else
				entities = map.provinces;
			List<IAdminEntity> entities1 = new List<IAdminEntity> (entities);

			// step 1: prepare data structures
			for (int k = 0; k < entities1.Count; k++) {
				entities1 [k].regions = new List<Region> (entities1 [k].regions);
			}

			// step 2: catalog points
			int totalPoints = 0;
			List<Vector2> allPoints = new List<Vector2> (150000);
			for (int k = 0; k < entities1.Count; k++) {
				for (int r = 0; r < entities1 [k].regions.Count; r++) {
					Region region1 = entities1 [k].regions [r];
					totalPoints += region1.latlon.Length;
					allPoints.AddRange (region1.latlon);
				}
			}

			allPoints = DouglasPeucker.SimplifyCurve (allPoints, 0.1);
			Dictionary<Vector2, bool> allPointsLookup = new Dictionary<Vector2, bool> (allPoints.Count);
			for (int k = 0; k < allPoints.Count; k++) {
				if (!allPointsLookup.ContainsKey (allPoints [k]))
					allPointsLookup.Add (allPoints [k], true);
			}

			// step 3: reduce region points according to exclusion catalog
			int savings = 0;
			List<Vector2> goodLatLons = new List<Vector2> (15000);
			for (int k = 0; k < entities1.Count; k++) {
				for (int r = 0; r < entities1 [k].regions.Count; r++) {
					goodLatLons.Clear ();
					Region region = entities1 [k].regions [r];
					for (int p = 0; p < region.latlon.Length; p++) {
						Vector2 latlon = region.latlon [p];
						if (allPointsLookup.ContainsKey (latlon))
							goodLatLons.Add (latlon);
					}
					PolygonSanitizer.RemoveCrossingSegments (goodLatLons);
					if (goodLatLons.Count < 5) {
						entities1 [k].regions.Remove (region);
						r--;
					} else {
						totalPoints += region.latlon.Length;
						savings += (region.latlon.Length - goodLatLons.Count);
						region.latlon = goodLatLons.ToArray ();
					}
				}
			}
			Debug.Log (savings + " points removed of " + totalPoints + " (" + (((float)savings / totalPoints) * 100.0f).ToString ("F1") + "%)");

			StringBuilder sb = new StringBuilder ();
			for (int k = 0; k < entities1.Count; k++) {
				IAdminEntity entity = entities1 [k];
				if (entity.regions.Count == 0)
					continue;
				if (k > 0)
					sb.Append ("|");
				sb.Append (entity.name);
				sb.Append("$");
				if (entity is Country) {
					sb.Append (((Country)entity).continent);
					sb.Append("$");
				} else {
					sb.Append (map.countries [((Province)entity).countryIndex].name);
					sb.Append("$");
				}
				for (int r = 0; r < entity.regions.Count; r++) {
					if (r > 0)
						sb.Append ("*");
					Region region = entity.regions [r];
					for (int p = 0; p < region.latlon.Length; p++) {
						if (p > 0)
							sb.Append (";");
						Vector2 point = region.latlon [p] * WorldMapGlobe.MAP_PRECISION;
						sb.Append (Mathf.RoundToInt (point.x).ToString ());
						sb.Append(",");
						sb.Append (Mathf.RoundToInt (point.y).ToString ());
					}
				}
			}
			return sb.ToString ();
		}


		int GetNearestCountryToShape () {
			int countryIndex = -1;
			float minDist = float.MaxValue;
			Vector3 p = newShape [0];
			for (int k = 0; k < map.countries.Length; k++) {
				float dist = (p - map.countries [k].localPosition).sqrMagnitude;
				if (dist < minDist) {
					minDist = dist;
					countryIndex = k;
				}
			}
			return countryIndex;
		}

		/// <summary>
		/// Creates a new country with the current shape
		/// </summary>
		public void CountryCreate () {
			if (newShape.Count < 3)
				return;
			int nearestCountry = GetNearestCountryToShape ();

			string continent = nearestCountry >= 0 ? map.countries [nearestCountry].continent : "New Continent";
			countryIndex = map.countries.Length;
			countryRegionIndex = 0;
			Country newCountry = new Country ("New Country" + (countryIndex + 1).ToString (), continent);
			Region region = new Region (newCountry, 0);
			region.spherePoints = newShape.ToArray ();
			newCountry.regions.Add (region);
			region.CheckWorldEdgesAndOffset();
			map.CountryAdd (newCountry);
			map.RefreshCountryDefinition (countryIndex, null);
			if (!map.showFrontiers)
				map.showFrontiers = true;
			lastCountryCount = -1;
			ReloadCountryNames ();
			countryChanges = true;
			CountryRegionSelect ();
			map.RedrawMapLabels ();
		}

		/// <summary>
		/// Adds a new region to current country
		/// </summary>
		public void CountryRegionCreate () {
			if (newShape.Count < 3 || countryIndex < 0)
				return;

			Country country = map.countries [countryIndex];
			countryRegionIndex = country.regions.Count;
			Region region = new Region (country, countryRegionIndex);
			region.spherePoints = newShape.ToArray ();
			country.regions.Add (region);
			region.CheckWorldEdgesAndOffset();
			_map.CountryMergeAdjacentRegions (country);
			map.RefreshCountryDefinition (countryIndex, null);
			countryChanges = true;
		}


		string GetCountryUniqueName(string proposedName) {

			string goodName = proposedName;
			int suffix = 0;

			while (_map.GetCountryIndex(goodName) >= 0) {
				suffix++;
				goodName = proposedName + suffix.ToString();
			}
			return goodName;

		}

		/// <summary>
		/// Creates a new country based on a given region. Existing region is removed from its source entity.
		/// </summary>
		/// <param name="region">Region.</param>
		public void CountryCreate(Region region) {
			// Remove region from source entity
			IAdminEntity entity = region.entity;
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

			// Create the new country
			string uniqueName = GetCountryUniqueName(country.name);
			Country newCountry = new Country(uniqueName, country.continent);
			if (entity is Country) {
				newCountry.regions.Add(region);
			} else {
				Region newRegion = new Region(newCountry, 0);
				newRegion.UpdatePointsAndRect(region.latlon);
				newCountry.regions.Add(newRegion);
			}
			countryIndex = map.CountryAdd(newCountry);
			countryRegionIndex = 0;
			lastCountryCount = -1;
			GUICountryName = "";
			ReloadCountryNames();
			countryChanges = true;

			// Update cities
			List<City> cities = _map.GetCities(region);
			if (cities.Count > 0) {
				for (int k = 0; k < cities.Count; k++) {
					if (cities[k].countryIndex != countryIndex) {
						cities[k].countryIndex = countryIndex;
						cityChanges = true;
					}
				}
			}

			// Update mount points
			List<MountPoint> mp = new List<MountPoint>();
			map.GetMountPoints(region, mp);
			if (mp.Count > 0) {
				for (int k = 0; k < mp.Count; k++) {
					if (mp[k].countryIndex != countryIndex) {
						mp[k].countryIndex = countryIndex;
						mountPointChanges = true;
					}
				}
			}

			// Transfer any contained province
			if (entity is Country) {
				List<Province> provinces = new List<Province>();
				 _map.GetProvinces(region, provinces);
				for (int k = 0; k < provinces.Count; k++) {
					Province prov = provinces[k];
					if (prov.regions == null)
						_map.ReadProvincePackedString(prov);
					if (prov.regions == null)
						continue;
					if (_map.CountryTransferProvinceRegion(countryIndex, prov.mainRegion, false)) {
						provinceChanges = true;
					}
				}
			}

			map.Redraw();
			CountryRegionSelect();
		}

	
	}
}
