using Godot;

public enum HeroRarity
{
    Rare,       // 稀有 (蓝色)
    Epic,       // 史诗 (紫色)
    Legendary,  // 传说 (橙色)
    Mythic      // 神话 (红色)
}

public enum HeroClass
{
    Warrior,    // 战士
    Mage,       // 法师
    Archer,     // 弓手
    Assassin,   // 刺客
    Support     // 辅助
}

public enum HeroObtainMethod
{
    Summon,     // 召唤
    Quest,      // 任务
    Shop,       // 商店
    Event,      // 活动
    Achievement // 成就
}

public enum SpecialTraitType
{
    Passive,        // 被动特性
    OnCombatStart,  // 战斗开始时
    OnAttack,       // 攻击时
    OnHit,          // 命中时
    OnCrit,         // 暴击时
    OnKill,         // 击杀时
    OnDamaged,      // 受伤时
    OnDeath,        // 死亡时
    OnSkillCast,    // 释放技能时
    OnTurnStart,    // 回合开始时
    OnTurnEnd,      // 回合结束时
    Conditional     // 条件触发
}

public enum SpecialTraitTrigger
{
    Always,         // 始终生效
    OnCondition,    // 满足条件时
    OnCooldown,     // 冷却时间
    OnCharge,       // 充能触发
    OnStack,        // 叠加触发
    Random,         // 随机触发
    OnCombatStart   // 战斗开始时
}

public enum TraitEffectType
{
    StatBoost,      // 属性提升
    Heal,           // 治疗
    Damage,         // 伤害
    Shield,         // 护盾
    Buff,           // 增益
    Debuff,         // 减益
    Summon,         // 召唤
    Transform,      // 变身
    Special         // 特殊效果
}

public enum SoulLinkType
{
    ClassBased,     // 基于职业
    RarityBased,    // 基于品级
    SpecificHero,   // 特定英雄
    Universal,      // 通用链接
    Synergy         // 协同链接
}

public enum SoulLinkConditionType
{
    HeroClass,      // 英雄职业
    HeroRarity,     // 英雄品级
    HeroLevel,      // 英雄等级
    HeroStar,       // 英雄星级
    SpecificHero,   // 特定英雄
    TeamComposition // 队伍组成
}

public enum PassiveEffectType
{
    StatBoost,      // 属性提升
    SkillEnhance,   // 技能增强
    SpecialAbility, // 特殊能力
    Resistance,     // 抗性提升
    CriticalBonus,  // 暴击加成
    HealingBonus,   // 治疗加成
    DamageBonus     // 伤害加成
}

public enum AwakeningRequirementType
{
    Level,          // 等级要求
    Star,           // 星级要求
    Material,       // 材料要求
    Achievement,    // 成就要求
    QuestComplete   // 任务完成
}