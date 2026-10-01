using UnityEditor;
using UnityEngine;

namespace Blacksite.WeaponShop.Editor
{
    public static class WeaponShopMenu
    {
        [MenuItem("GameObject/UI/Blacksite Weapon Shop")]
        private static void CreateWeaponShop(MenuCommand command)
        {
            GameObject shopObject = new GameObject("Blacksite Weapon Shop");
            GameObjectUtility.SetParentAndAlign(shopObject, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(shopObject, "Create Blacksite Weapon Shop");
            shopObject.AddComponent<WeaponShopUI>();
            Selection.activeGameObject = shopObject;
        }
    }
}
