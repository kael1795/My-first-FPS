using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blacksite.WeaponShop
{
    public enum ShopItemType
    {
        Weapon,
        Ammo
    }

    [Serializable]
    public class ShopItemData
    {
        public string id = "new-item";
        public string displayName = "新装备";
        public string categoryId = "rifle";
        public string categoryLabel = "突击步枪";
        public string rarityLabel = "标准";
        public Color rarityColor = new Color(0.62f, 0.65f, 0.58f, 1f);
        public ShopItemType type = ShopItemType.Weapon;
        public int price = 1000;
        public int stock = 10;
        public string iconKey = "rifle";

        [TextArea(2, 4)]
        public string description = "战术装备说明。";

        [Header("Weapon Stats")]
        [Range(0, 100)] public int damage = 50;
        [Range(0, 100)] public int fireRate = 50;
        [Range(0, 100)] public int stability = 50;
        [Range(0, 100)] public int mobility = 50;

        [Header("Ammo")]
        public int packSize = 60;
        public string compatibleWeapons = "";
    }

    public static class DefaultWeaponShopCatalog
    {
        public static List<ShopItemData> Create()
        {
            return new List<ShopItemData>
            {
                new ShopItemData
                {
                    id = "vanguard-ar9",
                    displayName = "先锋 AR-9",
                    categoryId = "rifle",
                    categoryLabel = "突击步枪",
                    rarityLabel = "史诗",
                    rarityColor = new Color(0.70f, 0.55f, 1f, 1f),
                    price = 8600,
                    stock = 8,
                    iconKey = "rifle",
                    description = "中距离主力步枪，模块化导轨兼顾火力与操控。",
                    damage = 78,
                    fireRate = 64,
                    stability = 71,
                    mobility = 58
                },
                new ShopItemData
                {
                    id = "raptor-r12",
                    displayName = "迅羽 R-12",
                    categoryId = "smg",
                    categoryLabel = "冲锋枪",
                    rarityLabel = "稀有",
                    rarityColor = new Color(0.38f, 0.84f, 0.85f, 1f),
                    price = 5350,
                    stock = 14,
                    iconKey = "smg",
                    description = "近距室内作战特化，高射速与快速换弹兼备。",
                    damage = 54,
                    fireRate = 91,
                    stability = 62,
                    mobility = 86
                },
                new ShopItemData
                {
                    id = "longbow-mk2",
                    displayName = "长弓 MK-II",
                    categoryId = "sniper",
                    categoryLabel = "精确步枪",
                    rarityLabel = "传说",
                    rarityColor = new Color(1f, 0.72f, 0.30f, 1f),
                    price = 14800,
                    stock = 3,
                    iconKey = "sniper",
                    description = "重型栓动平台，牺牲机动换取极远的致命射程。",
                    damage = 98,
                    fireRate = 22,
                    stability = 92,
                    mobility = 31
                },
                new ShopItemData
                {
                    id = "breaker-m12",
                    displayName = "破门者 M12",
                    categoryId = "shotgun",
                    categoryLabel = "战术霰弹枪",
                    rarityLabel = "稀有",
                    rarityColor = new Color(0.38f, 0.84f, 0.85f, 1f),
                    price = 6900,
                    stock = 7,
                    iconKey = "shotgun",
                    description = "近距离火力压制用半自动霰弹枪，适合狭窄区域。",
                    damage = 94,
                    fireRate = 48,
                    stability = 43,
                    mobility = 56
                },
                new ShopItemData
                {
                    id = "viper-p9",
                    displayName = "蝰蛇 P9",
                    categoryId = "pistol",
                    categoryLabel = "战术手枪",
                    rarityLabel = "标准",
                    rarityColor = new Color(0.62f, 0.65f, 0.58f, 1f),
                    price = 2800,
                    stock = 22,
                    iconKey = "pistol",
                    description = "可靠的基础副武器，部署迅速，弹药通用性高。",
                    damage = 49,
                    fireRate = 58,
                    stability = 66,
                    mobility = 88
                },
                new ShopItemData
                {
                    id = "havoc-h40",
                    displayName = "重锤 H-40",
                    categoryId = "rifle",
                    categoryLabel = "轻型机枪",
                    rarityLabel = "史诗",
                    rarityColor = new Color(0.70f, 0.55f, 1f, 1f),
                    price = 11200,
                    stock = 4,
                    iconKey = "rifle",
                    description = "持续火力平台，弹链供弹适合守住关键通道。",
                    damage = 84,
                    fireRate = 72,
                    stability = 78,
                    mobility = 38
                },
                new ShopItemData
                {
                    id = "ammo-556",
                    displayName = "5.56mm 穿甲弹",
                    categoryId = "ammo",
                    categoryLabel = "步枪弹药",
                    type = ShopItemType.Ammo,
                    price = 360,
                    stock = 999,
                    packSize = 120,
                    compatibleWeapons = "AR-9 / H-40",
                    iconKey = "ammo",
                    description = "标准穿甲弹芯，对中甲目标保持稳定的穿透表现。"
                },
                new ShopItemData
                {
                    id = "ammo-9mm",
                    displayName = "9mm 空尖弹",
                    categoryId = "ammo",
                    categoryLabel = "冲锋枪弹药",
                    type = ShopItemType.Ammo,
                    price = 240,
                    stock = 999,
                    packSize = 150,
                    compatibleWeapons = "R-12 / P9",
                    iconKey = "armor-ammo",
                    description = "近距离停止力优先，适合冲锋枪与战术手枪。"
                },
                new ShopItemData
                {
                    id = "ammo-762",
                    displayName = "7.62mm 高爆弹",
                    categoryId = "ammo",
                    categoryLabel = "精确弹药",
                    rarityLabel = "稀有",
                    rarityColor = new Color(0.38f, 0.84f, 0.85f, 1f),
                    type = ShopItemType.Ammo,
                    price = 780,
                    stock = 64,
                    packSize = 60,
                    compatibleWeapons = "MK-II / H-40",
                    iconKey = "ammo",
                    description = "强化装药弹头，对重型护甲和装置造成额外破坏。"
                },
                new ShopItemData
                {
                    id = "ammo-12g",
                    displayName = "12G 双头弹",
                    categoryId = "ammo",
                    categoryLabel = "霰弹",
                    type = ShopItemType.Ammo,
                    price = 420,
                    stock = 118,
                    packSize = 48,
                    compatibleWeapons = "破门者 M12",
                    iconKey = "shell",
                    description = "双弹丸结构，在近距离显著提高爆发伤害。"
                }
            };
        }
    }
}
