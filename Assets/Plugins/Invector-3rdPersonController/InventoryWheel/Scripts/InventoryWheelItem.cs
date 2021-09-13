using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Invector;
using Invector.vItemManager;
using Invector.vShooter;

namespace Invector
{
    [RequireComponent(typeof(Image))]
    public class InventoryWheelItem : MonoBehaviour
    {
        public bool Disabled;

        // the current viewed/selectedItem in this category
        public vItem CurrentItem => Items[CurrentItemIndex];

        [HideInInspector]
        public int CurrentItemIndex;

        [HideInInspector]
        public List<vItem> Items;

        [HideInInspector]
        public vShooterWeapon ShooterWeapon;

        [HideInInspector]
        public float Rotation;

        [HideInInspector]
        public Image CachedImage;

        public void CycleCategory(bool up)
        {
            if (up)
            {
                CurrentItemIndex++;
                if (CurrentItemIndex >= Items.Count)
                {
                    CurrentItemIndex = 0;
                }
            }
            else
            {
                CurrentItemIndex--;
                if (CurrentItemIndex < 0)
                {
                    CurrentItemIndex = Items.Count - 1;
                }
            }
            
        }
	}
}
