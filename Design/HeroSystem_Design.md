# 英雄系统设计文档 (Godot 4.4+)

## 📋 目录
1. [系统概述](#系统概述)
2. [核心架构](#核心架构)
3. [英雄基础系统](#英雄基础系统)
4. [品级系统](#品级系统)
5. [专属特性系统](#专属特性系统)
6. [灵魂链接系统](#灵魂链接系统)
7. [数据结构设计](#数据结构设计)
8. [技术实现](#技术实现)
9. [UI设计](#ui设计)
10. [平衡性设计](#平衡性设计)
11. [扩展性设计](#扩展性设计)

## 🎯 系统概述

### 设计目标
- **多样性**：每个英雄都有独特的专属特性和玩法
- **成长性**：通过品级系统提供长期成长目标
- **策略性**：灵魂链接系统增加队伍搭配的策略深度
- **收集性**：激励玩家收集和培养不同英雄
- **平衡性**：确保各品级英雄都有使用价值

### 核心特性
- **英雄收集**：多种获取途径和稀有度设计
- **专属特性**：每个英雄独有的被动技能或特殊机制
- **品级分层**：从普通到传说的品级体系
- **灵魂链接**：非上场英雄为上场英雄提供被动加成
- **成长培养**：等级、装备、觉醒等多维度成长

## 🏗️ 核心架构

### 系统组件图
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   HeroManager   │◄──►│   HeroInstance  │◄──►│   HeroConfig    │
│   (英雄管理器)  │    │   (英雄实例)    │    │   (英雄配置)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
│                       │                       │
▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  SoulLinkSystem │    │  SpecialTrait   │    │  HeroDatabase   │
│  (灵魂链接)     │    │  (专属特性)     │    │  (英雄数据库)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘

### 核心类关系
- **HeroManager**: 英雄系统总管理器
- **HeroInstance**: 英雄实例，包含等级、装备等动态数据
- **HeroConfig**: 英雄配置，定义基础属性和特性
- **SoulLinkSystem**: 灵魂链接系统管理器
- **SpecialTrait**: 专属特性处理器
- **HeroDatabase**: 英雄数据库

## 👤 英雄基础系统

### 英雄属性
```csharp
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class HeroConfig : Resource
{
    [Export] public int Id { get; set; }                    // 英雄唯一ID
    [Export] public string Name { get; set; }               // 英雄名称
    [Export] public string Description { get; set; }        // 英雄描述
    [Export] public HeroRarity Rarity { get; set; }         // 英雄品级
    [Export] public HeroClass Class { get; set; }           // 英雄职业
    [Export] public string AvatarPath { get; set; }         // 头像路径
    [Export] public string ModelPath { get; set; }          // 模型路径
    
    // 基础属性
    [Export] public HeroStats BaseStats { get; set; }       // 基础属性
    [Export] public HeroStats GrowthStats { get; set; }     // 成长属性
    
    // 专属特性
    [Export] public SpecialTraitConfig SpecialTrait { get; set; } // 专属特性
    
    // 技能配置
    [Export] public Array<int> SkillIds { get; set; }       // 技能ID列表
    
    // 灵魂链接
    [Export] public SoulLinkConfig SoulLink { get; set; }   // 灵魂链接配置
    
    // 获取途径
    [Export] public Array<HeroObtainMethod> ObtainMethods { get; set; } // 获取方式
}

[GlobalClass]
public partial class HeroStats : Resource
{
    [Export] public float Health { get; set; }              // 生命值
    [Export] public float Attack { get; set; }              // 攻击力
    [Export] public float Defense { get; set; }             // 防御力
    [Export] public float CritRate { get; set; }            // 暴击率
    [Export] public float CritDamage { get; set; }          // 暴击伤害
}

public enum HeroRarity
{
    Rare,       // 稀有 (蓝色)
    Epic,       // 史诗 (紫色)
    Legendary,  // 传说 (橙色)
    Mythic      // 神话 (红色)
}

[GlobalClass]
public partial class HeroInstance : Resource
{
    [Export] public string InstanceId { get; set; }         // 实例ID
    [Export] public int ConfigId { get; set; }              // 配置ID
    [Export] public HeroConfig Config { get; set; }         // 配置引用
    
    // 动态属性
    [Export] public int Level { get; set; } = 1;            // 等级
    [Export] public int Experience { get; set; }            // 经验值
    [Export] public int Star { get; set; } = 1;             // 星级
    [Export] public int Awakening { get; set; }             // 觉醒等级
    
    // 装备
    [Export] public Array<int> EquippedItems { get; set; }  // 装备ID列表
    
    // 状态
    [Export] public bool IsUnlocked { get; set; }           // 是否解锁
    [Export] public bool IsSoulLinked { get; set; }         // 是否被灵魂链接
    
    // 时间戳
    [Export] public double ObtainTime { get; set; }         // 获得时间
    [Export] public double LastUsedTime { get; set; }       // 最后使用时间
    
    // 计算最终属性
    public HeroStats GetFinalStats()
    {
        var stats = new HeroStats();
        
        // 基础属性 + 等级成长
        stats.Health = Config.BaseStats.Health + Config.GrowthStats.Health * (Level - 1);
        stats.Attack = Config.BaseStats.Attack + Config.GrowthStats.Attack * (Level - 1);
        // ... 其他属性计算
        
        // 星级加成
        ApplyStarBonus(stats);
        
        // 觉醒加成
        ApplyAwakeningBonus(stats);
        
        // 装备加成
        ApplyEquipmentBonus(stats);
        
        return stats;
    }
}

[GlobalClass]
public partial class RarityConfig : Resource
{
    [Export] public HeroRarity Rarity { get; set; }
    [Export] public string Name { get; set; }               // 品级名称
    [Export] public Color Color { get; set; }               // 品级颜色
    [Export] public string BorderTexture { get; set; }      // 边框贴图
    [Export] public string EffectTexture { get; set; }      // 特效贴图
    
    // 属性加成
    [Export] public float StatMultiplier { get; set; }      // 属性倍率
    [Export] public int MaxLevel { get; set; }              // 最大等级
    [Export] public int MaxStar { get; set; }               // 最大星级
    [Export] public int MaxAwakening { get; set; }          // 最大觉醒
    
    // 获取概率
    [Export] public float DropRate { get; set; }            // 掉落概率
    [Export] public int FragmentsToSummon { get; set; }     // 召唤所需碎片
    
    // 升级消耗
    [Export] public Array<LevelUpCost> LevelUpCosts { get; set; } // 升级消耗
    [Export] public Array<StarUpCost> StarUpCosts { get; set; }   // 升星消耗
}

[GlobalClass]
public partial class StarUpCost : Resource
{
    [Export] public int Star { get; set; }                  // 目标星级
    [Export] public int Gold { get; set; }                  // 金币消耗
    [Export] public int HeroFragments { get; set; }         // 英雄碎片
    [Export] public Dictionary<int, int> Materials { get; set; } // 材料消耗
}

public partial class RaritySystem : Node
{
    private Dictionary<HeroRarity, RarityConfig> _rarityConfigs;
    
    // 获取品级配置
    public RarityConfig GetRarityConfig(HeroRarity rarity)
    {
        return _rarityConfigs.GetValueOrDefault(rarity);
    }
    
    // 计算品级加成
    public HeroStats ApplyRarityBonus(HeroStats baseStats, HeroRarity rarity)
    {
        var config = GetRarityConfig(rarity);
        if (config == null) return baseStats;
        
        var bonusStats = new HeroStats();
        bonusStats.Health = baseStats.Health * config.StatMultiplier;
        bonusStats.Attack = baseStats.Attack * config.StatMultiplier;
        // ... 其他属性
        
        return bonusStats;
    }
    
    // 获取品级颜色
    public Color GetRarityColor(HeroRarity rarity)
    {
        return rarity switch
        {
            HeroRarity.Rare => Colors.Blue,
            HeroRarity.Epic => Colors.Purple,
            HeroRarity.Legendary => Colors.Orange,
            HeroRarity.Mythic => Colors.Red,
            _ => Colors.White
        };
    }
}

[GlobalClass]
public partial class SpecialTraitConfig : Resource
{
    [Export] public int Id { get; set; }                    // 特性ID
    [Export] public string Name { get; set; }               // 特性名称
    [Export] public string Description { get; set; }        // 特性描述
    [Export] public string IconPath { get; set; }           // 图标路径
    [Export] public SpecialTraitType Type { get; set; }     // 特性类型
    [Export] public SpecialTraitTrigger Trigger { get; set; } // 触发条件
    
    // 效果配置
    [Export] public Array<TraitEffect> Effects { get; set; } // 效果列表
    [Export] public Dictionary Parameters { get; set; }     // 参数配置
    
    // 等级相关
    [Export] public bool ScalesWithLevel { get; set; }      // 是否随等级缩放
    [Export] public float LevelScaling { get; set; }        // 等级缩放系数
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
    Random          // 随机触发
}

[GlobalClass]
public partial class TraitEffect : Resource
{
    [Export] public TraitEffectType EffectType { get; set; } // 效果类型
    [Export] public string TargetType { get; set; }         // 目标类型
    [Export] public float Value { get; set; }               // 效果数值
    [Export] public float Duration { get; set; }            // 持续时间
    [Export] public int MaxStacks { get; set; }             // 最大叠加
    [Export] public Dictionary Parameters { get; set; }     // 额外参数
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

public partial class SpecialTraitProcessor : Node
{
    [Signal] public delegate void TraitTriggeredEventHandler(HeroInstance hero, SpecialTraitConfig trait);
    
    private Dictionary<int, ISpecialTraitHandler> _traitHandlers;
    
    public override void _Ready()
    {
        InitializeTraitHandlers();
    }
    
    // 处理特性触发
    public void ProcessTrait(HeroInstance hero, SpecialTraitTrigger trigger, Variant context = default)
    {
        var trait = hero.Config.SpecialTrait;
        if (trait == null || trait.Trigger != trigger) return;
        
        if (_traitHandlers.TryGetValue(trait.Id, out var handler))
        {
            handler.Execute(hero, trait, context);
            EmitSignal(SignalName.TraitTriggered, hero, trait);
        }
    }
    
    // 获取特性描述
    public string GetTraitDescription(SpecialTraitConfig trait, int heroLevel)
    {
        var description = trait.Description;
        
        // 替换参数占位符
        foreach (var param in trait.Parameters)
        {
            var value = param.Value;
            if (trait.ScalesWithLevel)
            {
                value = CalculateScaledValue(value, heroLevel, trait.LevelScaling);
            }
            description = description.Replace($"{{{param.Key}}}", value.ToString());
        }
        
        return description;
    }
}

// 特性处理器接口
public interface ISpecialTraitHandler
{
    void Execute(HeroInstance hero, SpecialTraitConfig trait, Variant context);
}

// 示例：战斗狂热特性
public partial class BattleFrenzyTrait : RefCounted, ISpecialTraitHandler
{
    public void Execute(HeroInstance hero, SpecialTraitConfig trait, Variant context)
    {
        // 每次击杀敌人时，攻击力提升10%，最多叠加5层
        var killCount = context.AsInt32();
        var attackBonus = trait.Parameters["attack_bonus"].AsSingle();
        var maxStacks = trait.Parameters["max_stacks"].AsInt32();
        
        var actualStacks = Mathf.Min(killCount, maxStacks);
        var totalBonus = attackBonus * actualStacks;
        
        // 应用攻击力加成
        ApplyAttackBonus(hero, totalBonus);
    }
}

[GlobalClass]
public partial class SoulLinkConfig : Resource
{
    [Export] public int Id { get; set; }                    // 链接ID
    [Export] public string Name { get; set; }               // 链接名称
    [Export] public string Description { get; set; }        // 链接描述
    [Export] public SoulLinkType Type { get; set; }         // 链接类型
    
    // 链接条件
    [Export] public Array<SoulLinkCondition> Conditions { get; set; } // 链接条件
    
    // 提供的被动效果
    [Export] public Array<PassiveEffect> PassiveEffects { get; set; } // 被动效果
    
    // 链接限制
    [Export] public int MaxLinks { get; set; } = 1;         // 最大链接数
    [Export] public bool RequiresUnlock { get; set; }       // 是否需要解锁
    [Export] public Array<int> UnlockRequirements { get; set; } // 解锁条件
}

public enum SoulLinkType
{
    ClassBased,     // 基于职业
    RarityBased,    // 基于品级
    SpecificHero,   // 特定英雄
    Universal,      // 通用链接
    Synergy         // 协同链接
}

[GlobalClass]
public partial class SoulLinkCondition : Resource
{
    [Export] public SoulLinkConditionType Type { get; set; } // 条件类型
    [Export] public Variant Value { get; set; }             // 条件值
    [Export] public string Description { get; set; }        // 条件描述
}

public enum SoulLinkConditionType
{
    HeroClass,      // 英雄职业
    HeroRarity,     // 英雄品级
    HeroStar,       // 英雄星级
    SpecificHero,   // 特定英雄
    TeamComposition // 队伍组成
}

[GlobalClass]
public partial class PassiveEffect : Resource
{
    [Export] public PassiveEffectType Type { get; set; }    // 效果类型
    [Export] public string TargetProperty { get; set; }     // 目标属性
    [Export] public float Value { get; set; }               // 效果数值
    [Export] public bool IsPercentage { get; set; }         // 是否为百分比
    [Export] public string Description { get; set; }        // 效果描述
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

public partial class SoulLinkSystem : Node
{
    [Signal] public delegate void SoulLinkEstablishedEventHandler(HeroInstance activeHero, HeroInstance linkedHero);
    [Signal] public delegate void SoulLinkBrokenEventHandler(HeroInstance activeHero, HeroInstance linkedHero);
    
    private Dictionary<string, Array<string>> _activeSoulLinks; // 活跃链接 <活跃英雄ID, 链接英雄ID列表>
    private Dictionary<string, Array<PassiveEffect>> _linkEffects; // 链接效果缓存
    
    public override void _Ready()
    {
        _activeSoulLinks = new Dictionary<string, Array<string>>();
        _linkEffects = new Dictionary<string, Array<PassiveEffect>>();
    }
    
    // 建立灵魂链接
    public bool EstablishSoulLink(HeroInstance activeHero, HeroInstance linkedHero)
    {
        // 检查链接条件
        if (!CanEstablishLink(activeHero, linkedHero))
            return false;
        
        // 检查链接数量限制
        if (!CheckLinkLimit(activeHero))
            return false;
        
        // 建立链接
        if (!_activeSoulLinks.ContainsKey(activeHero.InstanceId))
            _activeSoulLinks[activeHero.InstanceId] = new Array<string>();
        
        _activeSoulLinks[activeHero.InstanceId].Add(linkedHero.InstanceId);
        linkedHero.IsSoulLinked = true;
        
        // 更新效果缓存
        UpdateLinkEffects(activeHero);
        
        EmitSignal(SignalName.SoulLinkEstablished, activeHero, linkedHero);
        return true;
    }
    
    // 断开灵魂链接
    public bool BreakSoulLink(HeroInstance activeHero, HeroInstance linkedHero)
    {
        if (!_activeSoulLinks.ContainsKey(activeHero.InstanceId))
            return false;
        
        var links = _activeSoulLinks[activeHero.InstanceId];
        if (!links.Contains(linkedHero.InstanceId))
            return false;
        
        links.Remove(linkedHero.InstanceId);
        linkedHero.IsSoulLinked = false;
        
        // 更新效果缓存
        UpdateLinkEffects(activeHero);
        
        EmitSignal(SignalName.SoulLinkBroken, activeHero, linkedHero);
        return true;
    }
    
    // 获取链接效果
    public Array<PassiveEffect> GetLinkEffects(HeroInstance activeHero)
    {
        return _linkEffects.GetValueOrDefault(activeHero.InstanceId, new Array<PassiveEffect>());
    }
    
    // 检查是否可以建立链接
    private bool CanEstablishLink(HeroInstance activeHero, HeroInstance linkedHero)
    {
        var linkConfig = linkedHero.Config.SoulLink;
        if (linkConfig == null) return false;
        
        // 检查所有链接条件
        foreach (var condition in linkConfig.Conditions)
        {
            if (!CheckCondition(activeHero, linkedHero, condition))
                return false;
        }
        
        return true;
    }
    
    // 检查链接条件
    private bool CheckCondition(HeroInstance activeHero, HeroInstance linkedHero, SoulLinkCondition condition)
    {
        return condition.Type switch
        {
            SoulLinkConditionType.HeroClass => activeHero.Config.Class == condition.Value.AsInt32(),
            SoulLinkConditionType.HeroRarity => (int)activeHero.Config.Rarity >= condition.Value.AsInt32(),
            SoulLinkConditionType.HeroLevel => activeHero.Level >= condition.Value.AsInt32(),
            SoulLinkConditionType.HeroStar => activeHero.Star >= condition.Value.AsInt32(),
            SoulLinkConditionType.SpecificHero => activeHero.ConfigId == condition.Value.AsInt32(),
            _ => true
        };
    }
    
    // 更新链接效果缓存
    private void UpdateLinkEffects(HeroInstance activeHero)
    {
        var effects = new Array<PassiveEffect>();
        
        if (_activeSoulLinks.TryGetValue(activeHero.InstanceId, out var linkedHeroIds))
        {
            foreach (var linkedHeroId in linkedHeroIds)
            {
                var linkedHero = HeroManager.Instance.GetHeroInstance(linkedHeroId);
                if (linkedHero?.Config.SoulLink != null)
                {
                    foreach (var effect in linkedHero.Config.SoulLink.PassiveEffects)
                    {
                        effects.Add(effect);
                    }
                }
            }
        }
        
        _linkEffects[activeHero.InstanceId] = effects;
    }
}

public partial class HeroManager : Node
{
    private static HeroManager _instance;
    public static HeroManager Instance => _instance;
    
    [Signal] public delegate void HeroObtainedEventHandler(HeroInstance hero);
    [Signal] public delegate void HeroLevelUpEventHandler(HeroInstance hero, int newLevel);
    [Signal] public delegate void HeroStarUpEventHandler(HeroInstance hero, int newStar);
    
    private Dictionary<string, HeroInstance> _ownedHeroes;
    private Dictionary<int, HeroConfig> _heroConfigs;
    private SoulLinkSystem _soulLinkSystem;
    private SpecialTraitProcessor _traitProcessor;
    
    public override void _Ready()
    {
        if (_instance == null)
        {
            _instance = this;
            InitializeSystem();
        }
        else
        {
            QueueFree();
        }
    }
    
    private void InitializeSystem()
    {
        _ownedHeroes = new Dictionary<string, HeroInstance>();
        _heroConfigs = new Dictionary<int, HeroConfig>();
        
        _soulLinkSystem = GetNode<SoulLinkSystem>("SoulLinkSystem");
        _traitProcessor = GetNode<SpecialTraitProcessor>("SpecialTraitProcessor");
        
        LoadHeroDatabase();
        LoadPlayerHeroes();
    }
    
    // 获得英雄
    public HeroInstance ObtainHero(int configId)
    {
        var config = GetHeroConfig(configId);
        if (config == null) return null;
        
        var hero = new HeroInstance();
        hero.InstanceId = Guid.NewGuid().ToString();
        hero.ConfigId = configId;
        hero.Config = config;
        hero.Level = 1;
        hero.Star = 1;
        hero.IsUnlocked = true;
        hero.ObtainTime = Time.GetUnixTimeFromSystem();
        
        _ownedHeroes[hero.InstanceId] = hero;
        
        EmitSignal(SignalName.HeroObtained, hero);
        return hero;
    }
    
    // 英雄升星
    public bool StarUpHero(string heroId)
    {
        var hero = GetHeroInstance(heroId);
        if (hero == null) return false;
        
        var rarityConfig = RaritySystem.Instance.GetRarityConfig(hero.Config.Rarity);
        if (hero.Star >= rarityConfig.MaxStar) return false;
        
        // 检查升星消耗
        if (!CheckStarUpCost(hero)) return false;
        
        // 消耗资源
        ConsumeStarUpCost(hero);
        
        // 升星
        hero.Star++;
        
        EmitSignal(SignalName.HeroStarUp, hero, hero.Star);
        return true;
    }
    
    // 获取英雄实例
    public HeroInstance GetHeroInstance(string instanceId)
    {
        return _ownedHeroes.GetValueOrDefault(instanceId);
    }
    
    // 获取英雄配置
    public HeroConfig GetHeroConfig(int configId)
    {
        return _heroConfigs.GetValueOrDefault(configId);
    }
    
    // 获取所有拥有的英雄
    public Array<HeroInstance> GetOwnedHeroes()
    {
        return new Array<HeroInstance>(_ownedHeroes.Values);
    }
    
    // 按品级筛选英雄
    public Array<HeroInstance> GetHeroesByRarity(HeroRarity rarity)
    {
        return GetOwnedHeroes().Where(h => h.Config.Rarity == rarity).ToArray();
    }
    
    // 按职业筛选英雄
    public Array<HeroInstance> GetHeroesByClass(HeroClass heroClass)
    {
        return GetOwnedHeroes().Where(h => h.Config.Class == heroClass).ToArray();
    }
}

public partial class TeamSystem : Node
{
    [Export] public int MaxSoulLinks { get; set; } = 2;      // 最大灵魂链接数
    
    private Array<HeroInstance> _soulLinkedHeroes;
    
    public override void _Ready()
    {
        _soulLinkedHeroes = new Array<HeroInstance>();
    }
    
    // 添加灵魂链接英雄
    public bool AddSoulLinkedHero(HeroInstance activeHero, HeroInstance linkedHero)
    {
        if (!_activeTeam.Contains(activeHero)) return false;
        if (_soulLinkedHeroes.Count >= MaxSoulLinks) return false;
        
        if (SoulLinkSystem.Instance.EstablishSoulLink(activeHero, linkedHero))
        {
            _soulLinkedHeroes.Add(linkedHero);
            return true;
        }
        
        return false;
    }
    
    // 计算队伍总战力
    public float CalculateTeamPower()
    {
        float totalPower = 0;
        
        foreach (var hero in _activeTeam)
        {
            var stats = hero.GetFinalStats();
            var linkEffects = SoulLinkSystem.Instance.GetLinkEffects(hero);
            
            // 应用灵魂链接效果
            ApplySoulLinkEffects(stats, linkEffects);
            
            // 计算单个英雄战力
            float heroPower = CalculateHeroPower(stats);
            totalPower += heroPower;
        }
        
        return totalPower;
    }
    
    private float CalculateHeroPower(HeroStats stats)
    {
        // 战力计算公式
        return stats.Health * 0.5f + 
               stats.Attack * 2.0f + 
               stats.Defense * 1.5f + 
               stats.Speed * 1.0f;
    }
}

public partial class HeroUI : Control
{
    [Export] public GridContainer HeroGrid { get; set; }     // 英雄网格
    [Export] public Control HeroDetailPanel { get; set; }   // 英雄详情面板
    [Export] public OptionButton RarityFilter { get; set; } // 品级筛选
    [Export] public OptionButton ClassFilter { get; set; }  // 职业筛选
    [Export] public LineEdit SearchInput { get; set; }      // 搜索输入
    
    private Array<HeroInstance> _displayedHeroes;
    private HeroInstance _selectedHero;
    
    public override void _Ready()
    {
        InitializeFilters();
        RefreshHeroList();
        
        // 连接信号
        RarityFilter.ItemSelected += OnRarityFilterChanged;
        ClassFilter.ItemSelected += OnClassFilterChanged;
        SearchInput.TextChanged += OnSearchTextChanged;
    }
    
    private void RefreshHeroList()
    {
        // 清空现有显示
        foreach (Node child in HeroGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // 获取筛选后的英雄列表
        _displayedHeroes = GetFilteredHeroes();
        
        // 创建英雄卡片
        foreach (var hero in _displayedHeroes)
        {
            var heroCard = CreateHeroCard(hero);
            HeroGrid.AddChild(heroCard);
        }
    }
    
    private Control CreateHeroCard(HeroInstance hero)
    {
        var card = GD.Load<PackedScene>("res://ui/HeroCard.tscn").Instantiate<Control>();
        var heroCard = card.GetComponent<HeroCard>();
        
        heroCard.SetHero(hero);
        heroCard.HeroSelected += OnHeroSelected;
        
        return card;
    }
    
    private void OnHeroSelected(HeroInstance hero)
    {
        _selectedHero = hero;
        ShowHeroDetails(hero);
    }
    
    private void ShowHeroDetails(HeroInstance hero)
    {
        var detailView = HeroDetailPanel.GetComponent<HeroDetailView>();
        detailView.DisplayHero(hero);
        HeroDetailPanel.Visible = true;
    }
}

public partial class HeroCard : Control
{
    [Signal] public delegate void HeroSelectedEventHandler(HeroInstance hero);
    
    [Export] public TextureRect Avatar { get; set; }        // 头像
    [Export] public Label NameLabel { get; set; }           // 名称
    [Export] public Label LevelLabel { get; set; }          // 等级
    [Export] public Control StarContainer { get; set; }     // 星级容器
    [Export] public NinePatchRect RarityBorder { get; set; } // 品级边框
    [Export] public Control SoulLinkIndicator { get; set; } // 灵魂链接指示器
    
    private HeroInstance _hero;
    
    public void SetHero(HeroInstance hero)
    {
        _hero = hero;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (_hero == null) return;
        
        // 设置头像
        var avatarTexture = GD.Load<Texture2D>(_hero.Config.AvatarPath);
        if (avatarTexture != null)
            Avatar.Texture = avatarTexture;
        
        // 设置名称和等级
        NameLabel.Text = _hero.Config.Name;
        LevelLabel.Text = $"Lv.{_hero.Level}";
        
        // 设置星级
        UpdateStarDisplay();
        
        // 设置品级边框
        var rarityColor = RaritySystem.Instance.GetRarityColor(_hero.Config.Rarity);
        RarityBorder.Modulate = rarityColor;
        
        // 设置灵魂链接指示器
        SoulLinkIndicator.Visible = _hero.IsSoulLinked;
    }
    
    private void UpdateStarDisplay()
    {
        // 清空现有星星
        foreach (Node child in StarContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // 添加星星
        for (int i = 0; i < _hero.Star; i++)
        {
            var star = new TextureRect();
            star.Texture = GD.Load<Texture2D>("res://ui/icons/star.png");
            StarContainer.AddChild(star);
        }
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                EmitSignal(SignalName.HeroSelected, _hero);
            }
        }
    }
}

public partial class SoulLinkUI : Control
{
    [Export] public GridContainer ActiveHeroGrid { get; set; }   // 活跃英雄网格
    [Export] public GridContainer AvailableHeroGrid { get; set; } // 可链接英雄网格
    [Export] public Control LinkEffectPanel { get; set; }       // 链接效果面板
    [Export] public Button EstablishLinkButton { get; set; }    // 建立链接按钮
    [Export] public Button BreakLinkButton { get; set; }        // 断开链接按钮
    
    private HeroInstance _selectedActiveHero;
    private HeroInstance _selectedLinkHero;
    
    public override void _Ready()
    {
        EstablishLinkButton.Pressed += OnEstablishLinkPressed;
        BreakLinkButton.Pressed += OnBreakLinkPressed;
        
        RefreshHeroGrids();
    }
    
    private void RefreshHeroGrids()
    {
        // 刷新活跃英雄网格
        RefreshActiveHeroes();
        
        // 刷新可链接英雄网格
        RefreshAvailableHeroes();
    }
    
    private void RefreshActiveHeroes()
    {
        // 清空现有显示
        foreach (Node child in ActiveHeroGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // 获取队伍中的英雄
        var teamHeroes = TeamSystem.Instance.GetActiveTeam();
        
        foreach (var hero in teamHeroes)
        {
            var heroCard = CreateSelectableHeroCard(hero, true);
            ActiveHeroGrid.AddChild(heroCard);
        }
    }
    
    private void RefreshAvailableHeroes()
    {
        // 清空现有显示
        foreach (Node child in AvailableHeroGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        if (_selectedActiveHero == null) return;
        
        // 获取可链接的英雄
        var availableHeroes = GetAvailableLinkHeroes(_selectedActiveHero);
        
        foreach (var hero in availableHeroes)
        {
            var heroCard = CreateSelectableHeroCard(hero, false);
            AvailableHeroGrid.AddChild(heroCard);
        }
    }
    
    private Array<HeroInstance> GetAvailableLinkHeroes(HeroInstance activeHero)
    {
        var allHeroes = HeroManager.Instance.GetOwnedHeroes();
        var availableHeroes = new Array<HeroInstance>();
        
        foreach (var hero in allHeroes)
        {
            // 排除已在队伍中的英雄
            if (hero.IsInTeam) continue;
            
            // 排除已被链接的英雄
            if (hero.IsSoulLinked) continue;
            
            // 检查是否可以建立链接
            if (SoulLinkSystem.Instance.CanEstablishLink(activeHero, hero))
            {
                availableHeroes.Add(hero);
            }
        }
        
        return availableHeroes;
    }
    
    private void OnEstablishLinkPressed()
    {
        if (_selectedActiveHero == null || _selectedLinkHero == null) return;
        
        if (SoulLinkSystem.Instance.EstablishSoulLink(_selectedActiveHero, _selectedLinkHero))
        {
            ShowMessage("灵魂链接建立成功！");
            RefreshHeroGrids();
            UpdateLinkEffectDisplay();
        }
        else
        {
            ShowMessage("灵魂链接建立失败！");
        }
    }
    
    private void UpdateLinkEffectDisplay()
    {
        if (_selectedActiveHero == null) return;
        
        var effects = SoulLinkSystem.Instance.GetLinkEffects(_selectedActiveHero);
        var effectText = "";
        
        foreach (var effect in effects)
        {
            effectText += $"• {effect.Description}\n";
        }
        
        LinkEffectPanel.GetNode<RichTextLabel>("EffectLabel").Text = effectText;
    }
}

public partial class BalanceConfig : Resource
{
    // 品级属性倍率
    [Export] public Dictionary<HeroRarity, float> RarityMultipliers { get; set; } = new()
    {
        { HeroRarity.Common, 1.0f },
        { HeroRarity.Uncommon, 1.2f },
        { HeroRarity.Rare, 1.5f },
        { HeroRarity.Epic, 2.0f },
        { HeroRarity.Legendary, 2.8f },
        { HeroRarity.Mythic, 4.0f }
    };
    
    // 获取概率
    [Export] public Dictionary<HeroRarity, float> DropRates { get; set; } = new()
    {
        { HeroRarity.Common, 50.0f },
        { HeroRarity.Uncommon, 30.0f },
        { HeroRarity.Rare, 15.0f },
        { HeroRarity.Epic, 4.0f },
        { HeroRarity.Legendary, 0.9f },
        { HeroRarity.Mythic, 0.1f }
    };
    
    // 升级成本倍率
    [Export] public Dictionary<HeroRarity, float> UpgradeCostMultipliers { get; set; } = new()
    {
        { HeroRarity.Common, 1.0f },
        { HeroRarity.Uncommon, 1.5f },
        { HeroRarity.Rare, 2.5f },
        { HeroRarity.Epic, 4.0f },
        { HeroRarity.Legendary, 6.5f },
        { HeroRarity.Mythic, 10.0f }
    };
}

### 特性平衡原则
1. 低品级英雄 ：简单直接的数值加成
2. 中品级英雄 ：条件触发的强力效果
3. 高品级英雄 ：复杂的机制和多重效果
4. 灵魂链接 ：提供有意义但不过强的被动加成
5. 协同效应 ：鼓励特定组合但避免必须搭配

[GlobalClass]
public partial class AwakeningConfig : Resource
{
    [Export] public int AwakeningLevel { get; set; }         // 觉醒等级
    [Export] public Array<AwakeningRequirement> Requirements { get; set; } // 觉醒条件
    [Export] public Array<AwakeningBonus> Bonuses { get; set; } // 觉醒奖励
    [Export] public SpecialTraitConfig NewTrait { get; set; } // 新增特性
}

[GlobalClass]
public partial class AwakeningRequirement : Resource
{
    [Export] public AwakeningRequirementType Type { get; set; }
    [Export] public int Value { get; set; }
    [Export] public Array<int> MaterialIds { get; set; }
}

public enum AwakeningRequirementType
{
    Level,          // 等级要求
    Star,           // 星级要求
    Material,       // 材料要求
    Achievement,    // 成就要求
    QuestComplete   // 任务完成
}

