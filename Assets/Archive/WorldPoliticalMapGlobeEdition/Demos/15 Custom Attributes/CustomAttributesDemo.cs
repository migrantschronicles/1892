using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace WPM
{

	public class CustomAttributesDemo : MonoBehaviour
	{

		void Start()
		{

			WorldMapGlobe map = WorldMapGlobe.instance;

			// ***********************************************************************
			// Adding custom attributes to a country (same for provinces, cities, ...)
			// ***********************************************************************

			Country canada = map.GetCountry("Canada");

			canada.attrib["Language"] = "French";                       // Add language as a custom attribute
			canada.attrib["ConstitutionDate"] = new DateTime(1867, 7, 1);       // Add the date of British North America Act, 1867
			canada.attrib["AreaKm2"] = 9984670;                                 // Add the land area in km2

			// List example		
			List<int> values = new List<int>(10);
			for (int j = 0; j < 10; j++)
				values.Add(j);
			canada.attrib["List"] = JSONObject.FromArray(values);

			// ******************************************************
			// Obtain attributes and print them out over the console. 
			// ******************************************************

			Debug.Log("Language = " + canada.attrib["Language"]);
			Debug.Log("Constitution Date = " + canada.attrib["ConstitutionDate"].d);    // Note the use of .d to force cast the internal number representation to DateTime
			Debug.Log("Area in km2 = " + canada.attrib["AreaKm2"]);
			Debug.Log("List = " + canada.attrib["List"]);

            // *********************************************************
            // Now, look up by attribute example using lambda expression
            // *********************************************************

            List<Country> results = new List<Country>();
            map.GetCountries((attrib) => "French".Equals(attrib["Language"]) && attrib["AreaKm2"] > 1000000, results);
			Debug.Log("Matches found = " + results.Count);
			foreach (Country c in results)
			{
				Debug.Log("Match: " + c.name);
			}

			// *****************************************************************
			// Export/import individual country attributes in JSON format sample
			// *****************************************************************

			string json = canada.attrib.Print();    // Get raw jSON
			Debug.Log(json);

			canada.attrib = new JSONObject(json);   // Import from raw jSON
			int keyCount = canada.attrib.keys.Count;
			Debug.Log("Imported JSON has " + keyCount + " keys.");
			for (int k = 0; k < keyCount; k++)
			{
				Debug.Log("Key " + (k + 1) + ": " + canada.attrib.keys[k] + " = " + canada.attrib[k]);
			}

			// *****************************************************************
			// Finally, export all countries attributes in one single JSON file
			// *****************************************************************

			string jsonCountries = map.GetCountriesAttributes(true);    // get the complete json for all countries with attributes
			Debug.Log(jsonCountries);

			canada.attrib = null;
			map.SetCountriesAttributes(jsonCountries);                  // parse the jsonCountries string (expects a jSON compliant string) and loads the attributes
			Debug.Log("Canada's attributes restored: Lang = " + canada.attrib["Language"] + ", Date = " + canada.attrib["ConstitutionDate"].d + ", Area = " + canada.attrib["AreaKm2"]);

		}



	}

}

