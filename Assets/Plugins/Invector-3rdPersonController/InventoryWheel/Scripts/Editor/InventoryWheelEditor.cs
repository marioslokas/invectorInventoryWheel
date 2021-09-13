using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Invector;
using Invector.vItemManager;

namespace Sickscore
{
	[CustomEditor(typeof(InventoryWheel))]
	public class InventoryWheelEditor : Editor
	{
		protected InventoryWheel inventoryWheel;
		protected GUISkin skin;
		protected Texture2D logo = null;

		private bool isPropertiesVisible;


		void OnEnable ()
		{
			inventoryWheel = (InventoryWheel)target;

			skin = Resources.Load ("skin") as GUISkin;
			logo = (Texture2D)Resources.Load ("icon_v2", typeof(Texture2D));
		}


		public override void OnInspectorGUI ()
		{
			// assign GUI skin
			if (skin != null)
				GUI.skin = skin;

			GUILayout.BeginVertical ("Inventory Wheel (AddOn)", "window");

			GUILayout.Label (logo, GUILayout.MaxHeight (25));

			inventoryWheel.Inventory = (vInventory)EditorGUILayout.ObjectField ("V Inventory", inventoryWheel.Inventory, typeof(vInventory), true);
			GUILayout.Space (5);

			isPropertiesVisible = GUILayout.Toggle (isPropertiesVisible, isPropertiesVisible ? "Close Properties" : "Open Properties", EditorStyles.toolbarButton);
			if (isPropertiesVisible) {
				base.DrawDefaultInspector ();

				// CUSTOM CONTENT
			}

			GUILayout.EndVertical ();

			// mark as dirty
			if (GUI.changed)
				EditorUtility.SetDirty (target);
		}
	}
}
