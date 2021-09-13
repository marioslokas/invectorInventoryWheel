using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Invector;

namespace Sickscore
{
	public static class RI_Menus
	{
		[MenuItem("Invector/3rd Party Addons/Inventory Wheel/Create Inventory Wheel", false, 11)]
		public static void CreateInventoryWheel ()
		{
			InventoryWheelCreator.InitEditorWindow ();
		}


		[MenuItem("Invector/3rd Party Addons/Inventory Wheel/About")]
		public static void About ()
		{
			Application.OpenURL ("http://invector.proboards.com/thread/576/inventory-wheel-radial-system-uni");
		}
	}
}
