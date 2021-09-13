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

        [HideInInspector]
        public vItem Item;

        //public virtual vItemAttribute AmmoCount
        //{
        //    get {

        //        return item.attributes.Find(a => a.name.Equals(Invector.vItemManager.vItemAttributes.AmmoCount));
        //    }
        //}

        //public virtual vItemType Type
        //{
        //    get { return item.type; }
        //}

        //[HideInInspector]
        //public virtual Sprite WheelItemSprite
        //{
        //    get { return item.icon; }
        //}

        //[HideInInspector]
        //public virtual string WheelItemString
        //{
        //    get { return item.name; }
        //}

        //public void SetVItem(vItem item)
        //{
        //    this.item = item;
        //}

        [HideInInspector]
		public vShooterWeapon ShooterWeapon;

		[HideInInspector]
		public float Rotation;

		[HideInInspector]
		public Image CachedImage;
	}
}
