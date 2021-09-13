using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Invector;
using Invector.vItemManager;
using Invector.vCharacterController;
using Sickscore;

namespace Sickscore
{
	public class InventoryWheelCreator : EditorWindow
	{
		#region Variables
		protected static InventoryWheelCreator CurrentWindow;
		protected static string _Name = "Inventory Wheel Creator";
		protected static Vector2 windowSize = new Vector2 (420, 160);

		protected GUISkin skin;

		private GameObject InventoryWheelPrefab;
		#endregion


		#region Main Methods
		public static void InitEditorWindow ()
		{
			CurrentWindow = (InventoryWheelCreator)EditorWindow.GetWindow<InventoryWheelCreator> ();
			CurrentWindow.titleContent = new GUIContent (_Name, _Name);
			CurrentWindow.minSize = windowSize;
		}


		void OnEnable ()
		{
			// try to assign the prefab automatically
			if (InventoryWheelPrefab == null)
				InventoryWheelPrefab = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Sickscore/InventoryWheel/Prefabs/InventoryWheel_Canvas.prefab", typeof(GameObject));
		}


		void OnGUI ()
		{
			skin = Resources.Load ("skin") as GUISkin;
			if (skin != null)
				GUI.skin = skin;

			GUILayout.BeginVertical (_Name, "window");

			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("box");
			InventoryWheelPrefab = EditorGUILayout.ObjectField("InventoryWheel Prefab: ", InventoryWheelPrefab, typeof(GameObject), false) as GameObject;
			GUILayout.EndVertical();

			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<vThirdPersonController> () != null) {
				if (Selection.activeGameObject.GetComponent<vItemManager> () != null) {
					if (InventoryWheelPrefab != null) {
						GUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();
						if (GUILayout.Button ("Create"))
							Create ();
						GUILayout.FlexibleSpace ();
						GUILayout.EndHorizontal ();
					} else {
						EditorGUILayout.HelpBox ("You need to assign the InventoryWheel_Canvas from the InventoryWheel > Prefabs folder.", MessageType.Warning);
					}
				} else {
					EditorGUILayout.HelpBox ("You need to add an Item Manager to your vThirdPersonController gameobject.", MessageType.Warning);

					GUILayout.BeginHorizontal ("box");
					EditorGUILayout.LabelField ("Add vItemManager now?");
					if (GUILayout.Button ("Yes, do it!")) {
						GetWindow<vCreateInventoryEditor> ();
					}
					GUILayout.EndHorizontal ();
				}
			} else {
				EditorGUILayout.HelpBox ("Please select the Player to add this component.", MessageType.Warning);
			}

			GUILayout.EndVertical ();

			Repaint ();
		}
		#endregion


		#region Utility Methods
		void Create ()
		{
			// create inventory wheel
			Instantiate (InventoryWheelPrefab, Vector3.zero, Quaternion.identity, null);

			// close window
			if (CurrentWindow != null)
				CurrentWindow.Close ();
		}
		#endregion
	}
}
