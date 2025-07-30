using Godot;
using Godot.Collections;

public enum RelicRarity
{
    Common,     // 普通
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary   // 传说
}

public enum RelicCategory
{
    Attack,     // 攻击强化
    Defense,    // 防御强化
    Charge,     // 充能强化
    Skill,      // 技能强化
    Special,    // 特殊机制
    Utility     // 实用工具
}

public enum RelicTriggerType
{
    Passive,        // 被动效果
    OnDamageDealt,  // 造成伤害时
    OnDamageTaken,  // 受到伤害时
    OnEnemyKilled,  // 击败敌人时
    OnSkillUsed,    // 使用技能时
    OnChargeGained, // 获得充能时
    OnComboReached, // 达到连击时
    OnLevelStart,   // 关卡开始时
    OnLevelEnd,     // 关卡结束时
    Manual          // 手动触发
}

public enum RelicEffectType
{
    StatModifier,       // 属性修改
    SkillEnhancement,   // 技能增强
    ChargeBoost,        // 充能提升
    SpecialAbility,     // 特殊能力
    BuffGeneration,     // 生成Buff
    RuleModification    // 规则修改
}

[GlobalClass]
public partial class RelicEffectData : Resource
{
    [Export] public RelicEffectType EffectType { get; set; }    // 效果类型
    [Export] public string TargetProperty { get; set; }         // 目标属性
    [Export] public float Value { get; set; }                   // 效果数值
    [Export] public bool IsPercentage { get; set; }             // 是否为百分比
    [Export] public float Duration { get; set; }                // 持续时间
    [Export] public Dictionary Parameters { get; set; }         // 额外参数
    
    public RelicEffectData()
    {
        Parameters = new Dictionary();
    }
}

[GlobalClass]
public partial class RelicSynergy : Resource
{
    [Export] public Array<int> RequiredRelics { get; set; }     // 需要的遗物ID
    [Export] public string SynergyName { get; set; }            // 协同效果名称
    [Export] public string Description { get; set; }            // 效果描述
    [Export] public Array<RelicEffectData> BonusEffects { get; set; } // 额外效果
    
    public RelicSynergy()
    {
        RequiredRelics = new Array<int>();
        BonusEffects = new Array<RelicEffectData>();
    }
}

[GlobalClass]
public partial class RelicConfig : Resource
{
    [Export] public int Id { get; set; }                    // 遗物唯一ID
    [Export] public string Name { get; set; }               // 遗物名称
    [Export] public string Description { get; set; }        // 遗物描述
    [Export] public string FlavorText { get; set; }         // 风味文本
    [Export] public RelicRarity Rarity { get; set; }        // 稀有度
    [Export] public RelicCategory Category { get; set; }    // 分类
    [Export] public string IconPath { get; set; }           // 图标路径
    [Export] public Color RarityColor { get; set; }         // 稀有度颜色
    
    // 效果配置
    [Export] public Array<RelicEffectData> Effects { get; set; } // 效果列表
    [Export] public RelicTriggerType TriggerType { get; set; }   // 触发类型
    [Export] public float TriggerChance { get; set; } = 1.0f;    // 触发概率
    [Export] public float Cooldown { get; set; }                 // 冷却时间
    
    // 获取配置
    [Export] public float DropWeight { get; set; } = 1.0f;  // 掉落权重
    [Export] public int MinLevel { get; set; }              // 最低出现层数
    [Export] public bool IsUnique { get; set; }             // 是否唯一
    [Export] public Array<int> ConflictRelics { get; set; } // 冲突遗物
    
    // 组合效果
    [Export] public Array<RelicSynergy> Synergies { get; set; } // 协同效果
    
    public RelicConfig()
    {
        Effects = new Array<RelicEffectData>();
        ConflictRelics = new Array<int>();
        Synergies = new Array<RelicSynergy>();
        RarityColor = Colors.White;
    }
}