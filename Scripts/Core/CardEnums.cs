using Godot;

/// <summary>
/// 卡牌系统通用枚举定义
/// </summary>

/// <summary>
/// 卡牌类型
/// </summary>
public enum CardType
{
    Monster,    // 怪物卡
    Skill,      // 技能卡
    Spell,      // 法术卡（未来扩展）
    Equipment,  // 装备卡（未来扩展）
    Environment // 环境卡（未来扩展）
}

/// <summary>
/// 卡牌稀有度
/// </summary>
public enum CardRarity
{
    Common,     // 普通
    Uncommon,   // 非凡
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary   // 传奇
}



/// <summary>
/// 魔法颜色（万智牌风格）
/// </summary>
public enum MagicColor
{
    White,  // 白色 - 秩序、治疗、保护
    Blue,   // 蓝色 - 智慧、魔法、控制
    Black,  // 黑色 - 黑暗、诅咒、死亡
    Red,    // 红色 - 火焰、愤怒、破坏
    Green,  // 绿色 - 自然、生命、成长
    Colorless // 无色
}



/// <summary>
/// 羁绊类型
/// </summary>
public enum BondType
{
    // 双色羁绊
    OrderMage,      // 白蓝 - 秩序法师
    FallenKnight,   // 白黑 - 堕落骑士
    HolyWarrior,    // 白红 - 圣战士
    NatureGuard,    // 白绿 - 自然守护
    DarkMage,       // 蓝黑 - 暗黑法师
    ElementalMage,  // 蓝红 - 元素法师
    Druid,          // 蓝绿 - 德鲁伊
    ChaosDemon,     // 黑红 - 混沌恶魔
    CorruptNature,  // 黑绿 - 腐化自然
    WildBerserker,  // 红绿 - 野性狂战
    
    // 三色羁绊
    NatureTemple,   // 白蓝绿 - 自然圣殿
    PrimalChaos,    // 黑红绿 - 原始混沌
    JudgmentLegion, // 白黑红 - 审判军团
    
    // 特殊羁绊
    SameColor,      // 同色羁绊
    Multicolor,     // 多色羁绊
    Tribal          // 种族羁绊
}

/// <summary>
/// 怪物技能类型
/// </summary>
public enum MonsterSkillType
{
    // 攻击类
    Rush,           // 冲锋
    CriticalStrike, // 暴击
    AoEAttack,      // 范围攻击
    Burn,           // 燃烧
    Poison,         // 中毒
    Freeze,         // 冰冻
    MagicAttack,    // 魔法攻击
    
    // 防御类
    Protect,        // 保护
    Shield,         // 护盾
    Taunt,          // 嘲讽
    Stealth,        // 潜行
    
    // 治疗类
    Heal,           // 治疗
    Regeneration,   // 再生
    
    // 控制类
    Stun,           // 眩晕
    Silence,        // 沉默
    Fear,           // 恐惧
    
    // 增益类
    Buff,           // 增益
    Pack,           // 群体效应
    
    // 特殊类
    Summon,         // 召唤
    Transform,      // 变形
    Sacrifice       // 献祭
}

/// <summary>
/// 召唤师技能类型
/// </summary>
public enum SummonerSkillType
{
    TypingSpeedBoost,    // 打字速度提升
    AccuracyBoost,       // 准确度提升
    DamageAmplifier,     // 伤害放大器
    ManaCostReduction,   // 法力消耗减少
    SummonBonus,         // 召唤加成
    BondEnhancer,        // 羁绊增强器
    ElementalMastery,    // 元素精通
    CombatExpertise,     // 战斗专精
    TypingEnhancement,   // 打字强化
    TypingMastery        // 打字精通
}

/// <summary>
/// 卡牌颜色系统枚举定义
/// 类似炉石传说死亡骑士的符文机制
/// </summary>

public enum CardColor
{
    Red,    // 红色 - 力量、直接伤害
    Blue,   // 蓝色 - 充能速度、抽卡效率
    Green,  // 绿色 - 下毒、防御
    Yellow, // 黄色 - 治疗、支援
    Black,  // 黑色 - 诅咒、削弱
    White   // 白色 - 净化、保护
}

/// <summary>
/// 颜色主题定义
/// 每种颜色代表的核心概念和机制
/// </summary>
public enum ColorTheme
{
    Power,      // 力量 (红色)
    Efficiency, // 效率 (蓝色)
    Nature,     // 自然 (绿色)
    Light,      // 光明 (黄色)
    Shadow,     // 阴影 (黑色)
    Purity      // 纯净 (白色)
}

/// <summary>
/// 卡牌稀有度与颜色槽位的关系
/// </summary>
public enum ColorSlotRarity
{
    Basic,      // 基础牌 - 1个颜色槽位
    Advanced,   // 进阶牌 - 2个颜色槽位
    Master      // 大师牌 - 3个颜色槽位
}