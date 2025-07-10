# 遗物系统设计文档 (Godot 4.4+)

## 📋 目录
1. [系统概述](#系统概述)
2. [核心架构](#核心架构)
3. [遗物分类体系](#遗物分类体系)
4. [获取机制](#获取机制)
5. [效果系统](#效果系统)
6. [与其他系统整合](#与其他系统整合)
7. [数据结构设计](#数据结构设计)
8. [技术实现](#技术实现)
9. [UI设计](#ui设计)
10. [平衡性设计](#平衡性设计)
11. [扩展性设计](#扩展性设计)

## 🎯 系统概述

### 设计目标
- **随机性**：每次游戏提供不同的遗物组合体验
- **策略性**：遗物选择影响构建方向和战术策略
- **成长性**：遗物提供即时和长期的能力提升
- **收集性**：激励玩家探索和尝试不同遗物组合
- **平衡性**：确保强力遗物有相应的获取难度

### 核心特性
- **随机获取**：通过多种途径随机获得遗物
- **即时生效**：获得后立即产生效果
- **叠加效果**：多个遗物可以产生协同效应
- **稀有度分层**：从普通到传说的稀有度体系
- **主题分类**：不同类型的遗物服务于不同的构建方向

### 系统定位
遗物系统作为**辅助增强系统**，为核心的技能系统提供多样化的增强效果，丰富构建策略的深度。

## 🏗️ 核心架构

### 系统组件图
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   RelicManager  │◄──►│  RelicInstance  │◄──►│   RelicConfig   │
│   (遗物管理器)  │    │   (遗物实例)    │    │   (遗物配置)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
│                       │                       │
▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ RelicEffectSys  │    │  RelicDropSys   │    │  RelicDatabase  │
│ (效果系统)      │    │  (掉落系统)     │    │  (遗物数据库)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘

### 核心类关系
- **RelicManager**: 遗物系统总管理器
- **RelicInstance**: 遗物实例，包含获得时间、触发次数等动态数据
- **RelicConfig**: 遗物配置，定义基础属性和效果
- **RelicEffectSystem**: 遗物效果处理器
- **RelicDropSystem**: 遗物掉落和获取系统
- **RelicDatabase**: 遗物数据库

## 🗂️ 遗物分类体系

### 按稀有度分类

#### 🟢 普通遗物 (Common) - 60%
基础增强效果，容易获得

**示例遗物**：
- **数据碎片**
  - 效果：每次击败敌人额外获得1点充能
  - 描述："残留的数据痕迹，蕴含着微弱的能量"

- **备份电池**
  - 效果：技能轨道基础充能速度+20%
  - 描述："老旧但可靠的能源设备"

- **错误日志**
  - 效果：打字错误时不会重置连击数（每10秒触发一次）
  - 描述："记录着无数次失败的尝试"

#### 🔵 稀有遗物 (Rare) - 25%
显著的能力提升，有一定获取难度

**示例遗物**：
- **量子处理器**
  - 效果：同时充能两个技能轨道时，充能效率+50%
  - 描述："利用量子叠加态的先进处理器"

- **防火墙核心**
  - 效果：受到伤害时有30%概率获得2秒无敌
  - 描述："从废弃防火墙中提取的核心组件"

- **代码注入器**
  - 效果：技能释放时有25%概率不消耗充能
  - 描述："能够重写现实规则的危险工具"

#### 🟣 史诗遗物 (Epic) - 12%
强力效果，显著改变游戏体验

**示例遗物**：
- **时间循环装置**
  - 效果：死亡时复活并回到3秒前的状态（每局游戏一次）
  - 描述："扭曲时空的禁忌科技"

- **病毒母体**
  - 效果：击败敌人时有概率感染附近敌人，造成持续伤害
  - 描述："自我复制的恶性代码集合体"

- **神经链接**
  - 效果：连击数每达到10的倍数时，随机触发一个技能
  - 描述："直接连接大脑与数字世界的接口"

#### 🟠 传说遗物 (Legendary) - 3%
游戏改变级别的效果，极其稀有

**示例遗物**：
- **创世代码**
  - 效果：每个技能轨道可以同时装备2张技能卡牌
  - 描述："构建这个数字世界的原始代码片段"

- **无限循环**
  - 效果：技能释放后有50%概率立即重置冷却时间
  - 描述："永不停息的完美算法"

- **系统管理员权限**
  - 效果：可以手动选择下一个获得的遗物（每层一次）
  - 描述："至高无上的系统访问权限"

### 按功能分类

#### ⚔️ 攻击强化类
- 提升伤害输出
- 增加攻击频率
- 添加特殊攻击效果

#### 🛡️ 防御强化类
- 增加生命值和护盾
- 提供伤害减免
- 增加生存能力

#### ⚡ 充能强化类
- 提升充能速度
- 优化能量管理
- 增加充能获取途径

#### 🎯 技能强化类
- 增强技能效果
- 减少技能消耗
- 提供技能组合效果

#### 🎲 特殊机制类
- 改变游戏规则
- 提供独特能力
- 创造新的玩法可能

## 🎰 获取机制

### 获取途径

#### 1. 关卡奖励
- **普通关卡**：20%概率获得遗物
- **精英关卡**：60%概率获得遗物
- **Boss关卡**：100%获得遗物，且稀有度更高

#### 2. 宝箱发现
- **普通宝箱**：30%概率包含遗物
- **稀有宝箱**：80%概率包含遗物
- **传说宝箱**：100%包含高稀有度遗物

#### 3. 商店购买
- 使用游戏内货币购买
- 每层商店刷新1-2个遗物
- 价格根据稀有度调整

#### 4. 特殊事件
- 随机事件可能奖励遗物
- 某些事件需要做出选择权衡
- 风险越高，奖励越好

#### 5. 成就解锁
- 完成特定成就获得特殊遗物
- 首次击败Boss获得纪念遗物
- 达成连击记录获得相关遗物

### 掉落权重系统

```csharp
public class RelicDropWeights
{
    // 基础权重
    public Dictionary<RelicRarity, float> BaseWeights = new()
    {
        { RelicRarity.Common, 60f },
        { RelicRarity.Rare, 25f },
        { RelicRarity.Epic, 12f },
        { RelicRarity.Legendary, 3f }
    };
    
    // 根据游戏进度调整权重
    public void AdjustWeightsByProgress(int currentLevel)
    {
        // 随着关卡深入，高稀有度遗物概率增加
        float progressMultiplier = 1f + (currentLevel * 0.1f);
        
        BaseWeights[RelicRarity.Rare] *= progressMultiplier;
        BaseWeights[RelicRarity.Epic] *= progressMultiplier * 1.5f;
        BaseWeights[RelicRarity.Legendary] *= progressMultiplier * 2f;
    }
}

## ⚡ 效果系统
### 效果触发时机 被动效果 (Passive)
- 持续生效，无需触发条件
- 例：基础属性提升、充能速度增加 触发效果 (Triggered)
- 满足特定条件时触发
- 例：受伤时、击败敌人时、释放技能时 主动效果 (Active)
- 需要玩家手动激活
- 例：消耗品类遗物、一次性效果
### 效果叠加规则 1. 数值叠加
- 相同类型的数值效果可以叠加
- 例：多个充能速度遗物效果累加 2. 概率叠加
- 概率类效果使用独立计算
- 例：两个30%概率效果 = 1-(0.7×0.7) = 51% 3. 互斥效果
- 某些遗物效果互相冲突
- 后获得的遗物会覆盖前一个 4. 协同效果
- 特定遗物组合产生额外效果
- 例："数据三件套"提供特殊加成
## 🔗 与其他系统整合
### 与技能系统的关系 技能增强
- 遗物可以直接增强技能效果
- 提供额外的技能触发条件
- 改变技能的消耗和冷却 轨道系统增强
- 增加轨道数量（稀有遗物）
- 提升充能效率
- 改变充能分配规则
### 与英雄系统的关系 英雄特性增强
- 某些遗物与特定英雄产生协同
- 增强英雄专属特性效果
- 提供额外的被动能力 灵魂链接增强
- 遗物效果可以影响灵魂链接的英雄
- 增加链接槽位（传说遗物）
- 提升链接效果强度
### 与Buff系统的关系 Buff生成
- 某些遗物会产生持续性Buff
- 遗物效果通过Buff系统实现
- 提供Buff免疫或增强效果

using Godot;
using Godot.Collections;

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
    [Export] public float TriggerChance { get; set; }            // 触发概率
    [Export] public float Cooldown { get; set; }                 // 冷却时间
    
    // 获取配置
    [Export] public float DropWeight { get; set; }          // 掉落权重
    [Export] public int MinLevel { get; set; }              // 最低出现层数
    [Export] public bool IsUnique { get; set; }             // 是否唯一
    [Export] public Array<int> ConflictRelics { get; set; } // 冲突遗物
    
    // 组合效果
    [Export] public Array<RelicSynergy> Synergies { get; set; } // 协同效果
}

[GlobalClass]
public partial class RelicInstance : RefCounted
{
    [Export] public string InstanceId { get; set; }         // 实例ID
    [Export] public int ConfigId { get; set; }              // 配置ID
    [Export] public RelicConfig Config { get; set; }        // 配置引用
    
    // 动态数据
    [Export] public double ObtainTime { get; set; }         // 获得时间
    [Export] public int TriggerCount { get; set; }          // 触发次数
    [Export] public double LastTriggerTime { get; set; }    // 最后触发时间
    [Export] public bool IsActive { get; set; } = true;     // 是否激活
    
    // 状态数据
    [Export] public Dictionary CustomData { get; set; }     // 自定义数据
    [Export] public Array<BuffInstance> GeneratedBuffs { get; set; } // 生成的Buff
}

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

[GlobalClass]
public partial class RelicEffectData : Resource
{
    [Export] public RelicEffectType EffectType { get; set; }    // 效果类型
    [Export] public string TargetProperty { get; set; }         // 目标属性
    [Export] public float Value { get; set; }                   // 效果数值
    [Export] public bool IsPercentage { get; set; }             // 是否为百分比
    [Export] public float Duration { get; set; }                // 持续时间
    [Export] public Dictionary Parameters { get; set; }         // 额外参数
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

using Godot;
using Godot.Collections;

public partial class RelicManager : Node
{
    private static RelicManager _instance;
    public static RelicManager Instance => _instance;
    
    [Signal] public delegate void RelicObtainedEventHandler(RelicInstance relic);
    [Signal] public delegate void RelicActivatedEventHandler(RelicInstance relic);
    [Signal] public delegate void RelicEffectTriggeredEventHandler(RelicInstance relic, string effectType);
    
    private Array<RelicInstance> _ownedRelics;
    private Dictionary<int, RelicConfig> _relicConfigs;
    private RelicDropSystem _dropSystem;
    private RelicEffectSystem _effectSystem;
    
    public override void _Ready()
    {
        _instance = this;
        _ownedRelics = new Array<RelicInstance>();
        _relicConfigs = new Dictionary<int, RelicConfig>();
        
        _dropSystem = new RelicDropSystem();
        _effectSystem = new RelicEffectSystem();
        
        LoadRelicConfigs();
    }
    
    // 获得遗物
    public void ObtainRelic(int relicId)
    {
        if (!_relicConfigs.ContainsKey(relicId))
        {
            GD.PrintErr($"Relic config not found: {relicId}");
            return;
        }
        
        var config = _relicConfigs[relicId];
        
        // 检查唯一性
        if (config.IsUnique && HasRelic(relicId))
        {
            GD.Print($"Unique relic already owned: {config.Name}");
            return;
        }
        
        // 检查冲突
        if (HasConflictingRelic(config))
        {
            GD.Print($"Conflicting relic exists for: {config.Name}");
            return;
        }
        
        var instance = new RelicInstance
        {
            InstanceId = Guid.NewGuid().ToString(),
            ConfigId = relicId,
            Config = config,
            ObtainTime = Time.GetUnixTimeFromSystem(),
            CustomData = new Dictionary()
        };
        
        _ownedRelics.Add(instance);
        _effectSystem.ApplyRelicEffects(instance);
        
        EmitSignal(SignalName.RelicObtained, instance);
        
        GD.Print($"Obtained relic: {config.Name}");
    }
    
    // 检查是否拥有遗物
    public bool HasRelic(int relicId)
    {
        return _ownedRelics.Any(r => r.ConfigId == relicId);
    }
    
    // 检查冲突遗物
    private bool HasConflictingRelic(RelicConfig config)
    {
        if (config.ConflictRelics == null) return false;
        
        return config.ConflictRelics.Any(conflictId => HasRelic(conflictId));
    }
    
    // 触发遗物效果
    public void TriggerRelicEffects(RelicTriggerType triggerType, Dictionary parameters = null)
    {
        foreach (var relic in _ownedRelics)
        {
            if (!relic.IsActive) continue;
            if (relic.Config.TriggerType != triggerType) continue;
            
            // 检查冷却时间
            if (IsOnCooldown(relic)) continue;
            
            // 检查触发概率
            if (relic.Config.TriggerChance < 1f)
            {
                if (GD.Randf() > relic.Config.TriggerChance) continue;
            }
            
            _effectSystem.TriggerRelicEffect(relic, parameters);
            
            relic.TriggerCount++;
            relic.LastTriggerTime = Time.GetUnixTimeFromSystem();
            
            EmitSignal(SignalName.RelicEffectTriggered, relic, triggerType.ToString());
        }
    }
    
    // 检查冷却时间
    private bool IsOnCooldown(RelicInstance relic)
    {
        if (relic.Config.Cooldown <= 0) return false;
        
        var timeSinceLastTrigger = Time.GetUnixTimeFromSystem() - relic.LastTriggerTime;
        return timeSinceLastTrigger < relic.Config.Cooldown;
    }
    
    // 获取拥有的遗物列表
    public Array<RelicInstance> GetOwnedRelics()
    {
        return new Array<RelicInstance>(_ownedRelics);
    }
    
    // 按稀有度获取遗物
    public Array<RelicInstance> GetRelicsByRarity(RelicRarity rarity)
    {
        return new Array<RelicInstance>(_ownedRelics.Where(r => r.Config.Rarity == rarity));
    }
    
    // 按分类获取遗物
    public Array<RelicInstance> GetRelicsByCategory(RelicCategory category)
    {
        return new Array<RelicInstance>(_ownedRelics.Where(r => r.Config.Category == category));
    }
    
    private void LoadRelicConfigs()
    {
        // 从资源文件加载遗物配置
        // 实现省略...
    }
}
## 🎨 UI设计
### 遗物收藏界面
- 网格布局 ：按稀有度和分类展示遗物
- 筛选功能 ：按稀有度、分类、效果类型筛选
- 详细信息 ：点击查看遗物详细描述和效果
- 获取进度 ：显示遗物收集进度
### 游戏内遗物栏
- 紧凑显示 ：游戏界面边缘显示当前遗物
- 效果提示 ：鼠标悬停显示效果说明
- 触发反馈 ：遗物触发时的视觉反馈
- 快速查看 ：快捷键打开遗物详情
### 遗物获得界面
- 戏剧性展示 ：获得遗物时的特殊动画
- 稀有度强调 ：不同稀有度的特殊效果
- 效果预览 ：显示遗物将产生的效果
- 选择界面 ：某些情况下可以选择遗物
## ⚖️ 平衡性设计
### 稀有度平衡
- 普通遗物 ：提供基础但稳定的增强
- 稀有遗物 ：显著提升但不改变核心玩法
- 史诗遗物 ：强力效果，可能改变战术
- 传说遗物 ：游戏改变级别，极其稀有
### 获取平衡
- 早期关卡 ：主要获得普通和稀有遗物
- 中期关卡 ：史诗遗物开始出现
- 后期关卡 ：传说遗物有小概率出现
- 特殊途径 ：某些传说遗物只能通过特定方式获得
### 效果平衡
- 数值控制 ：强力效果配合低触发概率
- 代价机制 ：某些遗物有负面效果作为代价
- 互斥设计 ：防止过于强力的组合
- 渐进增强 ：效果随游戏进度逐渐增强
## 🔧 扩展性设计
### 遗物套装系统
- 收集特定遗物组合获得套装效果
- 套装效果比单个遗物更强力
- 鼓励玩家收集特定主题的遗物
### 遗物进化系统
- 某些遗物可以通过特定条件进化
- 进化后获得更强的效果
- 增加遗物的成长性和收集价值
### 诅咒遗物系统
- 带有负面效果的特殊遗物
- 提供强力正面效果作为补偿
- 增加风险与收益的权衡
### 季节性遗物
- 限时活动期间的特殊遗物
- 节日主题的特殊效果
- 增加游戏的时效性和新鲜感
## 📈 数据统计
### 收集统计
- 遗物获得率统计
- 稀有度分布分析
- 玩家偏好分析
### 效果统计
- 遗物使用率统计
- 效果触发频率分析
- 胜率影响分析
### 平衡调整
- 基于数据的平衡性调整
- 过强/过弱遗物的识别
- 新遗物的效果预测
## 🎯 总结
遗物系统作为Code Rogue的重要组成部分，将为玩家提供：

1. 丰富的随机性 ：每次游戏都有不同的体验
2. 深度的策略性 ：遗物选择影响构建方向
3. 持续的成长感 ：即时和长期的能力提升
4. 强烈的收集欲 ：激励探索和重复游玩
通过与技能系统、英雄系统和Buff系统的深度整合，遗物系统将成为游戏策略深度的重要来源，同时保持作为辅助系统的定位，不会喧宾夺主。

文档版本 ：v1.0 最后更新 ：2024年 状态 ：✅ 设计完成，待实现
