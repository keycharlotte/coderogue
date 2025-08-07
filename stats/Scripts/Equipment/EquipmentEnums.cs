using Godot;

/// <summary>
/// 装备系统枚举定义
/// </summary>

/// <summary>
/// 装备类型
/// </summary>
public enum EquipmentType
{
    Weapon,         // 武器
    Armor,          // 护甲
    Accessory,      // 饰品
    Consumable      // 消耗品
}

/// <summary>
/// 装备槽位
/// </summary>
public enum EquipmentSlot
{
    MainHand,       // 主手武器
    OffHand,        // 副手武器/盾牌
    Helmet,         // 头盔
    Chest,          // 胸甲
    Legs,           // 腿甲
    Boots,          // 靴子
    Ring1,          // 戒指1
    Ring2,          // 戒指2
    Necklace,       // 项链
    Bracelet        // 手镯
}

/// <summary>
/// 装备品质
/// </summary>
public enum EquipmentQuality
{
    Common,         // 普通（白色）
    Uncommon,       // 不常见（绿色）
    Rare,           // 稀有（蓝色）
    Epic,           // 史诗（紫色）
    Legendary,      // 传说（橙色）
    Mythic          // 神话（红色）
}