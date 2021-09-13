using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Invector;
using Invector.vItemManager;

namespace Invector
{
	public class InventoryWheelWeaponDisplay : vDisplayWeaponStandalone
	{
		[Header("Ammo sources")]
		public Image ammoIcon;
		public Text ammoText;


		protected override void Start ()
		{
			base.Start ();

			RemoveAmmoIcon ();
			RemoveAmmoText ();
		}


		public virtual void SetAmmoIcon (Sprite icon)
		{
			if (!ammoIcon)
				return;
			
			ammoIcon.sprite = icon;
			if (!ammoIcon.gameObject.activeSelf)
				ammoIcon.gameObject.SetActive (true);
		}


		public virtual void SetAmmoText (string text)
		{
			if (!ammoText)
				return;
			
			ammoText.text = text;
			if (!ammoText.gameObject.activeSelf)
				ammoText.gameObject.SetActive (true);
		}


		public virtual void RemoveAmmoIcon ()
		{
			if (!ammoIcon)
				return;
			
			ammoIcon.sprite = defaultIcon;
			if (ammoIcon.gameObject.activeSelf && ammoIcon.sprite == null)
				ammoIcon.gameObject.SetActive (false);
		}


		public virtual void RemoveAmmoText ()
		{
			if (!ammoText)
				return;
			
			ammoText.text = defaultText;
		}
	}
}
