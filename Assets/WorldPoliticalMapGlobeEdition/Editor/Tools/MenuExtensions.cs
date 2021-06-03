using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WPM
{
	static class MenuExtensions
	{

		[MenuItem ("GameObject/3D Object/World Political Map Globe Edition", false)]
		static void CreateGlobeMenuOption (MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = Object.Instantiate(Resources.Load<GameObject>("Prefabs/WorldMapGlobe"));
			go.name = "WorldMapGlobe";
			Undo.RegisterCreatedObjectUndo (go, "Create " + go.name);
			go.transform.localRotation = Quaternion.Euler (0, 0, 0);
			go.transform.localScale = new Vector3 (1f, 1f, 1f);
			Selection.activeObject = go;
		}
	}

}