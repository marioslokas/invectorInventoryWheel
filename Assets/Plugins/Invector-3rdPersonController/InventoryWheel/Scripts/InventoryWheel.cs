using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Invector.vCamera;
using Invector.vItemManager;
using Invector.vCharacterController;
using Invector.vShooter;
using System;

namespace Invector
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(AudioSource))]
	public class InventoryWheel : MonoBehaviour
	{
        #region Variables
        [Header("Modifications")]
        [SerializeField] private WheelAreaCategory[] categories;


		[Header("Items Filter")]
		public List<vItemType> ItemsFilter = new List<vItemType> ();

		[Header("Theme")]
		public Color BackgroundColor = Color.white;
		public Color IndicatorColor = Color.blue;
		public Color DisabledIndicatorColor = Color.grey;
		public Color SelectedItemColor = Color.white;
		public Color DefaultItemColor = Color.white;

		[Header("Customization")]
		public bool UseIndicatorArrow = true;
		public GameObject IndicatorArrowPrefab;
		public bool UseSeperators = true;
		public GameObject SeparatorPrefab;

		[Header("Settings")]
		public bool SnapToClosestItem = true;
		public Vector2 WheelItemSize = new Vector2 (85f, 85f);
		[Range(1f, 5f)]
		public float WheelScale = 1.35f;
		[Range(0f, 1f)]
		public float LerpSpeed = .35f;

		[Header("References")]
		public CanvasGroup InventoryContent;
		public Image Background;
		public Image Indicator;
		public Transform ItemHolder;
		public Transform SeparatorHolder;
		public InventoryWheelWeaponDisplay WeaponDisplay;
        [SerializeField] private vThirdPersonInput thirdPersonInput;

		[Header("Audio Settings")]
		public AudioClip OpenInventorySound;
		public AudioClip CloseInventorySound;
		public AudioClip NavigationSound;

		[Header("Input Settings")]
		public GenericInput OpenCloseWheelInput = new GenericInput("I", "LB", "LB");


		private int WheelItemCount {
			get {
				return GetWheelItems ().Count;
			}
		}
		private List<InventoryWheelItem> WheelItemInstances = new List<InventoryWheelItem> ();
		private List<Transform> WheelSeparatorInstances = new List<Transform> ();
		private InventoryWheelItem SelectedItem, OldSelectedItem;

		private InputDevice _InputDevice = InputDevice.Joystick;
		private vThirdPersonCamera _vCamera;
		private Animator _Animator;
		private AudioSource _AudioSource;
		private GameObject _IndicatorArrow;

		private float FillAmount;
		private float FillRadius;


		[HideInInspector]
		public vInventory Inventory;

		[HideInInspector]
		public bool IsWheelOpen = false;
		#endregion


		#region Main Methods
		void Start ()
		{
			// assign references
			_Animator = GetComponent<Animator> ();
			_AudioSource = GetComponent<AudioSource> ();

			// assign vThirdPersonCamera
			_vCamera = (vThirdPersonCamera)FindObjectOfType(typeof(vThirdPersonCamera));

			// check for item manager
			if (Inventory == null) {
				Debug.LogError ("You need to assign the vInventory component!");
				this.gameObject.SetActive (false);
				return;
			}
			
			// check missing references
			if (InventoryContent == null || Background == null || Indicator == null || ItemHolder == null || SeparatorHolder == null || WeaponDisplay == null) {
				Debug.LogError ("Some values on the Inventory Wheel are not assigned!");
				this.gameObject.SetActive (false);
				return;
			}

			// assign input device
			if (vInput.instance != null) {
				// subscribe to invector events
				vInput.instance.onChangeInputType += ChangeInputDevice;

				// set initial input device
				_InputDevice = vInput.instance.inputDevice;
			}

			// hide inventory wheel on startup
			IsWheelOpen = false;
			_Animator.SetBool("IsWheelOpen", IsWheelOpen);

			// apply theme
			ApplyTheme ();

			// initial refresh
			Refresh ();
		}


		void Update ()
		{
			// handle input
			HandleInput ();

			// handle animator
			HandleAnimator ();

			// lock camera input while wheel is open
			if (_vCamera != null)
				_vCamera.lockCamera = IsWheelOpen;

			// return if wheel is not open
			if (!IsWheelOpen)
				return;

			// check if we need to refresh the wheel
			if (WheelItemCount > WheelItemInstances.Count) {
				Refresh ();
				return;
			}

			// update wheel
			UpdateWheel ();
		}


		void HandleInput ()
		{
			// open/close inventory
			if (OpenCloseWheelInput.GetButtonDown ()) {
				OpenWheel ();
			} else if (OpenCloseWheelInput.GetButtonUp ()) {
				CloseWheel ();
			}
		}


		void HandleAnimator ()
		{
			if (_Animator == null)
				return;

			_Animator.SetBool ("IsWheelOpen", IsWheelOpen);
		}


		void Refresh ()
		{
			// clear items/separators on initialization
			ClearIndicatorArrow ();
			ClearItemsInWheel ();
			ClearSeperatorsInWheel ();

            // build wheel
            BuildWheel();
            //BuildCategoryWheel();
        }


		void UpdateWheel ()
		{
            Cursor.lockState = CursorLockMode.None;

            // return if wheel has no items
            if (WheelItemCount <= 0)
				return;

			// resize indicator to fit item amount
			Indicator.fillAmount = Mathf.Lerp (Indicator.fillAmount, FillAmount, .25f);

			// calculate pointer position/rotation
			Vector3 screenBounds = new Vector3 (Screen.width / 2f, Screen.height / 2f, 0f);
			Vector3 pointerPosition = Input.mousePosition - screenBounds;
			float pointerAngle = ((_InputDevice == InputDevice.MouseKeyboard) ? Mathf.Atan2 (pointerPosition.x, pointerPosition.y) : Mathf.Atan2 (Input.GetAxis ("RightAnalogHorizontal"), Input.GetAxis ("RightAnalogVertical"))) * Mathf.Rad2Deg;
			pointerAngle = (pointerAngle < 0f) ? pointerAngle += 360f : pointerAngle;
			float pointerRotation = (pointerAngle - Indicator.fillAmount * 360f / 2f) * -1f;
			bool joystickIdleState = pointerAngle == 0f && _InputDevice == InputDevice.Joystick;

			// prevent selection when joystick is moving back
			if (!joystickIdleState) {
				// find closest item in wheel
				float closestOffset = Mathf.Infinity;
				for (int i = 0; i < WheelItemInstances.Count; i++) {
					// check current distance
					float itemRotation = WheelItemInstances [i].Rotation;
					float itemOffset = Mathf.Abs (itemRotation - pointerAngle);
					if (itemOffset < closestOffset) {
						SelectedItem = WheelItemInstances [i];
						closestOffset = itemOffset;
					}
				}
			}

			// fallback if no item is selected (initial opening)
			if (SelectedItem == null) {
				// select first item
				SelectedItem = WheelItemInstances [0];
			}

			// indicate selected item
			if (SelectedItem != null) {
				if (OldSelectedItem != SelectedItem) {
					// assign new selected item
					OldSelectedItem = SelectedItem;

					// play sound
					if (NavigationSound != null)
						_AudioSource.PlayOneShot (NavigationSound);
				}
				
				// snap to closest item?
				if (SnapToClosestItem)
					pointerRotation = (SelectedItem.Rotation - Indicator.fillAmount * 360f / 2f) * -1f;

				// update indicator
				if (!joystickIdleState || SnapToClosestItem)
					Indicator.transform.localRotation = Quaternion.Slerp (Indicator.transform.localRotation, Quaternion.Euler (0f, 0f, pointerRotation), LerpSpeed);
				Indicator.color = Color.Lerp (Indicator.color, (SelectedItem.Disabled) ? DisabledIndicatorColor : IndicatorColor, LerpSpeed);

				// highlight selected item
				if (!SelectedItem.Disabled)
					SelectedItem.CachedImage.color = Color.Lerp (SelectedItem.CachedImage.color, SelectedItemColor, LerpSpeed);
			}

			// reset unselected items
			for (int i = 0; i < WheelItemInstances.Count; i++) {
				InventoryWheelItem IWheelItem = WheelItemInstances [i];

				// don't reset the selected item
				if (SelectedItem != null && IWheelItem == SelectedItem)
					continue;

				// reset item color
				IWheelItem.CachedImage.color = Color.Lerp (IWheelItem.CachedImage.color, DefaultItemColor, LerpSpeed);
			}

			// update weapon display
			UpdateWeaponDisplay (SelectedItem);
		}


		void OpenWheel ()
		{
            thirdPersonInput.LockCursor(false);
			if (WheelItemCount <= 0)
				return;
				
			// open wheel
			IsWheelOpen = true;

			// play sound
			if (OpenInventorySound != null)
				_AudioSource.PlayOneShot (OpenInventorySound);
		}


		void CloseWheel ()
		{
            thirdPersonInput.LockCursor(true);

            if (WheelItemCount <= 0) {
				if (IsWheelOpen)
					IsWheelOpen = false;

				return;
			}
			
			// equip selected item
			if (SelectedItem != null)
				EquipItem (SelectedItem);

			// close wheel
			IsWheelOpen = false;

			// play sound
			if (CloseInventorySound != null)
				_AudioSource.PlayOneShot (CloseInventorySound);
		}
		#endregion


		#region Utility Methods
		void ApplyTheme ()
		{
			Background.color = BackgroundColor;
			Indicator.color = IndicatorColor;

			InventoryContent.transform.localScale = Vector3.one * WheelScale;
		}


		void ClearIndicatorArrow ()
		{
			if (_IndicatorArrow == null)
				return;

			Destroy (_IndicatorArrow.gameObject);
		}


		void ClearItemsInWheel ()
		{
			if (WheelItemInstances.Count <= 0)
				return;

			foreach (InventoryWheelItem IWheelItemInstance in WheelItemInstances) {
				if (IWheelItemInstance != null && IWheelItemInstance.gameObject != null)
					Destroy (IWheelItemInstance.gameObject);
			}
			WheelItemInstances.Clear ();
		}


		void ClearSeperatorsInWheel ()
		{
			if (WheelSeparatorInstances.Count <= 0)
				return;

			foreach (Transform IWheelSeparatorInstance in WheelSeparatorInstances) {
				if (IWheelSeparatorInstance != null && IWheelSeparatorInstance.gameObject != null)
					Destroy (IWheelSeparatorInstance.gameObject);
			}
			WheelSeparatorInstances.Clear ();
		}


        // build the wheel based on number of categories
		void BuildWheel ()
		{
			if (WheelItemCount <= 0)
				return;

			// calculate fill amount depending on the amount of items
			FillAmount = 1f / (float)WheelItemCount;
			FillRadius = FillAmount * 360f;

			// build items / separators
			float prevRotation = 0f;
			float rotationRadius = 120f;
			List<vItem> InventoryItems = GetWheelItems ();
			for (int i = 0; i < WheelItemCount; i++) {
				// get inventory item
				vItem Item = InventoryItems [i];

				// calculate rotation
				float rotationOffset = FillRadius / 2f;
				float itemRotation = prevRotation + rotationOffset;
				itemRotation = (itemRotation > 360f) ? itemRotation -= 360f : itemRotation;
				prevRotation = itemRotation + rotationOffset;

				// calculate position
				Vector3 itemPosition = new Vector3 (rotationRadius * Mathf.Cos ((itemRotation - 90) * Mathf.Deg2Rad), -rotationRadius * Mathf.Sin ((itemRotation - 90) * Mathf.Deg2Rad), 0f);

				// create item
				GameObject IWheelItem = new GameObject (Item.name);
				IWheelItem.transform.SetParent (ItemHolder, false);
				IWheelItem.transform.position = Vector3.zero;
				IWheelItem.transform.rotation = transform.rotation;
				IWheelItem.transform.localPosition = itemPosition;

				// assign item references and add it to the instance list
				InventoryWheelItem IWheelItemScript = IWheelItem.AddComponent<InventoryWheelItem> ();
                IWheelItemScript.Item = Item;
				if (Item.originalObject != null) {
					IWheelItemScript.ShooterWeapon = Item.originalObject.GetComponent<vShooterWeapon> ();
				}
				IWheelItemScript.Rotation = itemRotation;
				IWheelItemScript.CachedImage = IWheelItem.GetComponent<Image> ();
				IWheelItemScript.CachedImage.sprite = Item.icon;
				IWheelItemScript.CachedImage.rectTransform.sizeDelta = WheelItemSize;
				WheelItemInstances.Add (IWheelItemScript);

				// instantiate separator
				if (UseSeperators && SeparatorPrefab != null) {
					GameObject IWheelSeparator = Instantiate (SeparatorPrefab, Vector3.zero, Quaternion.identity) as GameObject;
					IWheelSeparator.transform.SetParent (SeparatorHolder, false);
					IWheelSeparator.transform.localRotation = Quaternion.Euler (0f, 0f, prevRotation);

					// add separator to instance list
					WheelSeparatorInstances.Add (IWheelSeparator.transform);
				}
			}

			// create indicator arrow
			if (UseIndicatorArrow && IndicatorArrowPrefab != null && Indicator.transform != null) {
				_IndicatorArrow = Instantiate (IndicatorArrowPrefab, Vector3.zero, Quaternion.identity) as GameObject;
				_IndicatorArrow.transform.SetParent (Indicator.transform, false);
				_IndicatorArrow.transform.localRotation = Quaternion.Euler (0f, 0f, (FillRadius / 2f) * -1f);
			}
		}


		void ChangeInputDevice (InputDevice newInputDevice)
		{
			_InputDevice = newInputDevice;
		}
		#endregion

		#region Replacement Methods
		/// <summary>
		/// Modify to load different items into the inventory wheel.
		/// </summary>
		/// <returns>List of vItem used by the inventory wheel.</returns>
		List<vItem> GetWheelItems ()
		{
			return Inventory.items.FindAll(item => ItemsFilter.Contains(item.type));
		}


		/// <summary>
		/// Modify the method to fit your needs.
		/// </summary>
		/// <param name="selectedItem">Selected item.</param>
		void UpdateWeaponDisplay (InventoryWheelItem selectedItem = null)
		{
			if (WeaponDisplay == null)
				return;

			if (selectedItem != null) {
				WeaponDisplay.SetWeaponIcon (selectedItem.Item.icon);
				WeaponDisplay.SetWeaponText (selectedItem.Item.name);

				// get ammo item for current weapon
				if (SelectedItem.Item.type == vItemType.ShooterWeapon && selectedItem.ShooterWeapon != null) {
					vItem ammoItem = Inventory.items.Where (_item => _item.id.Equals (selectedItem.ShooterWeapon.ammoID)).FirstOrDefault ();
					if (ammoItem != null) {
                        vItemAttribute weaponAmmoCount = selectedItem.Item.attributes.Find(a => a.name.Equals(Invector.vItemManager.vItemAttributes.AmmoCount));

                        // update ammo
                        if (weaponAmmoCount != null) {
							// ammo in weapon / inventory
							string totalAmmo = string.Format ("{0} / {1}", weaponAmmoCount.value, ammoItem.amount);

							// update display
							WeaponDisplay.SetAmmoIcon (ammoItem.icon);
							WeaponDisplay.SetAmmoText (totalAmmo);
						}
					}
                    else {
						WeaponDisplay.RemoveAmmoIcon ();
						WeaponDisplay.RemoveAmmoText ();
					}
				}
                else {
					WeaponDisplay.RemoveAmmoIcon ();
					WeaponDisplay.RemoveAmmoText ();
				}
			} else {
				WeaponDisplay.RemoveWeaponIcon ();
				WeaponDisplay.RemoveWeaponText ();
				WeaponDisplay.RemoveAmmoIcon ();
				WeaponDisplay.RemoveAmmoText ();
			}
		}


		/// <summary>
		/// Modify to change logic when selecting an item in the inventory wheel.
		/// </summary>
		/// <param name="selectedItem">Selected item.</param>
		void EquipItem (InventoryWheelItem selectedItem)
		{
            if (selectedItem == null || selectedItem.Item == null || selectedItem.Disabled)
				return;

			if (Inventory == null || !Inventory.canEquip)
				return;

			switch (selectedItem.Item.type) {
				case vItemType.Ammo:
					Debug.Log ("Want to eat some ammo? That's weird...");
					break;
				case vItemType.Consumable:
					Debug.LogFormat ("Consuming item: {0}", selectedItem.Item.name);
					break;
				case vItemType.MeleeWeapon:
					Debug.LogFormat ("Equipping melee weapon: {0}", selectedItem.Item.name);
                    Inventory.changeEquipmentControllers[0].equipArea.AddItemToEquipSlot(0, selectedItem.Item);
                    break;
				case vItemType.ShooterWeapon:
					Debug.LogFormat ("Equipping shooter weapon: {0}", selectedItem.Item.name);
                    Inventory.changeEquipmentControllers[0].equipArea.AddItemToEquipSlot(0, selectedItem.Item);
                    break;
				default:
                    Debug.LogErrorFormat("Don't know how to equip item type: {0}!", selectedItem.Item.type);
                    break;
			}
		}
        #endregion

        [Serializable]
        private class WheelAreaCategory
        {
            [SerializeField] private string nameDisplay;
            [SerializeField] private vItemType itemType;
            [SerializeField] private Sprite displaySprite;

            public string NameDisplay => nameDisplay;
            public Sprite DisplaySprite => displaySprite;
        }
    }


	#region Subclasses
    
	#endregion
}
