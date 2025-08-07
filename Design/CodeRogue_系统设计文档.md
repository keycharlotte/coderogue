# Code Rogue: 代码深渊 - 系统设计文档

## 1. 系统架构概述

### 1.1 设计理念

**技能系统（卡牌）是游戏的绝对核心**，所有其他系统都围绕技能系统进行设计和服务。游戏采用模块化架构，各系统既相对独立又紧密协作，共同构建完整的游戏体验。

### 1.2 系统优先级

1. **技能系统（30%）** - 核心系统，游戏玩法中心
2. **打字战斗系统（20%）** - 核心输入机制，技能系统驱动引擎
3. **英雄系统（18%）** - 重要支撑系统，提供个性化载体
4. **遗物系统（17%）** - 策略增强系统，提供随机性和深度
5. **Buff系统（10%）** - 通用支撑系统，技术底层框架
6. **其他系统（5%）** - 基础支撑，确保游戏完整性

## 2. 核心系统设计

### 2.1 技能系统（卡牌系统）

#### 2.1.1 系统概述
技能系统是游戏的核心，采用卡牌化设计，通过轨道充能机制实现技能的获取和释放。

#### 2.1.2 核心机制

**轨道系统**
- 每个角色拥有4个技能轨道
- 轨道独立充能，能量达到阈值自动释放技能
- 支持轨道优先级设置和充能权重调整

**卡牌轮换**
- 卡牌库采用FIFO（先进先出）原则
- 技能释放后，卡牌回到库底，新卡牌进入轨道
- 支持卡牌预览和轮换策略自定义

**充能机制**
- 打字输入：每成功输入一个字符，所有轨道获得1点能量
- 自动充能：基础0.5点/秒，可通过装备和天赋提升
- 连击加成：连击数达到阈值时，充能效率提升

**羁绊系统**
- 卡牌拥有标签属性（职业、元素、类型等）
- 激活特定标签组合获得额外效果
- 支持多层羁绊效果叠加

#### 2.1.3 技术实现

```csharp
// 核心类结构
public class SkillTrackSystem : Node
{
    private SkillTrack[] _tracks = new SkillTrack[4];
    private CardLibrary _cardLibrary;
    private SynergyCalculator _synergyCalculator;
    
    public void ChargeAllTracks(float energy) { /* 充能逻辑 */ }
    public void ReleaseSkill(int trackIndex) { /* 技能释放 */ }
    public void RotateCard(int trackIndex) { /* 卡牌轮换 */ }
}

public class SkillCard : Resource
{
    [Export] public string Id { get; set; }
    [Export] public string Name { get; set; }
    [Export] public int EnergyCost { get; set; }
    [Export] public string[] Tags { get; set; }
    [Export] public SkillEffect[] Effects { get; set; }
}
```

### 2.2 打字战斗系统

#### 2.2.1 系统概述
打字战斗系统是游戏的核心输入机制，通过双轨联动实现打字和技能的有机结合。

#### 2.2.2 核心机制

**双轨联动**
- 打字轨道：基础攻击 + 充能积累
- 技能轨道：消耗充能释放强力技能
- 双向增强：打字为技能提供能量，技能增强打字体验

**连击系统**
- 连续正确输入增加连击数
- 连击数影响伤害倍率和充能效率
- 错误输入重置连击数

**构筑效率计算**
- 攻击效率：伤害输出能力评估
- 防御效率：生存和容错能力评估
- 轮转效率：技能使用频率和灵活性评估

#### 2.2.3 技术实现

```csharp
public class TypingCombatSystem : Node
{
    private int _comboCount = 0;
    private float _comboMultiplier = 1.0f;
    private SkillTrackSystem _skillTracks;
    
    public void ProcessInput(char character)
    {
        if (ValidateInput(character))
        {
            _comboCount++;
            UpdateComboMultiplier();
            DealDamage(CalculateDamage());
            ChargeSkillTracks(1.0f);
        }
        else
        {
            ResetCombo();
        }
    }
}

public class BuildEfficiencyCalculator : Node
{
    public EfficiencyMetrics CalculateEfficiency(SkillCard[] cards)
    {
        return new EfficiencyMetrics
        {
            AttackEfficiency = CalculateAttackPower(cards),
            DefenseEfficiency = CalculateDefensePower(cards),
            RotationEfficiency = CalculateRotationSpeed(cards)
        };
    }
}
```

### 2.3 英雄系统

#### 2.3.1 系统概述
英雄系统为技能系统和打字系统提供个性化载体，通过不同英雄的专属特性影响核心玩法。

#### 2.3.2 核心机制

**品级系统**
- 稀有（蓝色）：基础英雄，容易获得
- 史诗（紫色）：强力英雄，中等稀有度
- 传说（橙色）：顶级英雄，高稀有度
- 神话（红色）：终极英雄，极其稀有

**专属特性**
- 每个英雄拥有独特的被动特性
- 特性直接影响打字战斗和技能系统
- 支持特性升级和强化

**灵魂链接**
- 非上场英雄可作为灵魂伙伴提供被动加成
- 链接英雄的30%基础属性加成给主战英雄
- 支持羁绊效果和共鸣机制

#### 2.3.3 技术实现

```csharp
public class HeroManager : Node
{
    private Dictionary<string, HeroInstance> _heroes = new();
    private HeroInstance _activeHero;
    private HeroInstance _linkedHero;
    
    public void SetActiveHero(string heroId) { /* 设置主战英雄 */ }
    public void SetLinkedHero(string heroId) { /* 设置链接英雄 */ }
    public HeroStats GetCombinedStats() { /* 计算综合属性 */ }
}

public class HeroInstance : Resource
{
    [Export] public string HeroId { get; set; }
    [Export] public int Level { get; set; }
    [Export] public HeroRarity Rarity { get; set; }
    [Export] public HeroStats BaseStats { get; set; }
    [Export] public SpecialTrait Trait { get; set; }
}
```

### 2.4 遗物系统

#### 2.4.1 系统概述
遗物系统为游戏提供随机性和策略深度，通过各种效果增强核心系统的表现。

#### 2.4.2 核心机制

**获取方式**
- 关卡完成后随机掉落
- 商店购买
- 特殊事件奖励
- Boss战胜利奖励

**稀有度体系**
- 普通（白色）：基础效果，常见掉落
- 稀有（蓝色）：中等效果，适中稀有度
- 史诗（紫色）：强力效果，较为稀有
- 传说（橙色）：游戏改变级效果，极其稀有

**效果类型**
- 属性增强：直接提升基础属性
- 技能增强：影响技能效果和充能
- 打字增强：改善打字体验和效率
- 特殊机制：提供独特的游戏机制

#### 2.4.3 技术实现

```csharp
public class RelicSystem : Node
{
    private List<RelicInstance> _activeRelics = new();
    private RelicDatabase _relicDatabase;
    
    public void AddRelic(string relicId) { /* 添加遗物 */ }
    public void RemoveRelic(string relicId) { /* 移除遗物 */ }
    public RelicEffect[] GetActiveEffects() { /* 获取激活效果 */ }
}

public class RelicInstance : Resource
{
    [Export] public string RelicId { get; set; }
    [Export] public RelicRarity Rarity { get; set; }
    [Export] public RelicEffect[] Effects { get; set; }
    [Export] public int Level { get; set; }
}
```

### 2.5 Buff系统

#### 2.5.1 系统概述
Buff系统是通用的效果管理框架，为所有其他系统提供状态效果支撑。

#### 2.5.2 核心机制

**Buff类型**
- 增益Buff：提升角色能力
- 减益Buff：降低角色能力
- 中性Buff：改变游戏机制但不直接影响强弱

**叠加规则**
- 同类型Buff可叠加层数
- 不同类型Buff可同时存在
- 冲突Buff按优先级覆盖

**持续时间**
- 永久Buff：直到手动移除
- 定时Buff：指定时间后自动移除
- 条件Buff：满足特定条件后移除

#### 2.5.3 技术实现

```csharp
public class BuffSystem : Node
{
    private Dictionary<string, BuffInstance> _activeBuffs = new();
    private BuffDatabase _buffDatabase;
    
    public void ApplyBuff(string buffId, float duration = -1) { /* 应用Buff */ }
    public void RemoveBuff(string buffId) { /* 移除Buff */ }
    public void UpdateBuffs(float deltaTime) { /* 更新Buff状态 */ }
}

public class BuffInstance : Resource
{
    [Export] public string BuffId { get; set; }
    [Export] public BuffType Type { get; set; }
    [Export] public float Duration { get; set; }
    [Export] public int StackCount { get; set; }
    [Export] public BuffEffect[] Effects { get; set; }
}
```

## 3. 数据管理系统

### 3.1 数据架构设计

#### 3.1.1 AutoLoad架构
所有Manager和Database类采用AutoLoad单例模式，确保全局访问和初始化顺序。

**加载顺序**：
1. GameData（核心数据，最先加载）
2. 所有Database类（配置数据库）
3. 所有Manager类（业务逻辑管理器）

#### 3.1.2 数据分层

**配置层（Database）**
- CardDatabase：技能卡牌配置
- HeroDatabase：英雄配置
- RelicDatabase：遗物配置
- BuffDatabase：Buff配置

**运行时层（Manager）**
- GameManager：游戏状态管理
- SkillTrackManager：技能轨道管理
- HeroManager：英雄实例管理
- RelicManager：遗物实例管理

**持久化层（GameData）**
- 玩家进度数据
- 解锁内容记录
- 游戏设置保存

### 3.2 配置文件管理

#### 3.2.1 文件格式
- 主要格式：JSON（便于编辑和版本控制）
- 辅助格式：Godot Resource（复杂数据结构）
- 存储位置：ResourcesData/文件夹

#### 3.2.2 热重载机制
```csharp
public class ConfigManager : Node
{
    private FileSystemWatcher _configWatcher;
    
    public override void _Ready()
    {
        SetupConfigWatcher();
        LoadAllConfigs();
    }
    
    private void OnConfigChanged(string filePath)
    {
        ReloadConfig(filePath);
        NotifySystemsConfigChanged();
    }
}
```

## 4. 系统间交互设计

### 4.1 事件系统

#### 4.1.1 事件总线
```csharp
public class EventBus : Node
{
    public static EventBus Instance { get; private set; }
    
    // 技能相关事件
    public event Action<string, int> SkillCast;
    public event Action<int, float> TrackCharged;
    
    // 战斗相关事件
    public event Action<float> DamageDealt;
    public event Action<int> ComboChanged;
    
    // 英雄相关事件
    public event Action<string> HeroChanged;
    public event Action<string> TraitActivated;
}
```

#### 4.1.2 事件流程
1. 打字输入 → 战斗系统 → 技能充能事件
2. 技能释放 → 技能系统 → 效果应用事件
3. 效果应用 → Buff系统 → 状态更新事件
4. 状态更新 → UI系统 → 界面刷新

### 4.2 数据流设计

#### 4.2.1 单向数据流
```
用户输入 → InputManager → GameManager → 具体系统 → 数据更新 → UI刷新
```

#### 4.2.2 状态管理
```csharp
public class GameState : Resource
{
    [Export] public CombatState Combat { get; set; }
    [Export] public HeroState Heroes { get; set; }
    [Export] public SkillState Skills { get; set; }
    [Export] public RelicState Relics { get; set; }
    
    public void UpdateState(StateChange change)
    {
        // 应用状态变更
        ApplyChange(change);
        
        // 通知相关系统
        NotifyStateChanged(change);
    }
}
```

## 5. 性能优化设计

### 5.1 对象池管理

```csharp
public class ObjectPool<T> where T : Node, new()
{
    private Queue<T> _pool = new();
    private int _maxSize;
    
    public T Get()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();
        return new T();
    }
    
    public void Return(T obj)
    {
        if (_pool.Count < _maxSize)
        {
            obj.Reset(); // 重置对象状态
            _pool.Enqueue(obj);
        }
        else
        {
            obj.QueueFree();
        }
    }
}
```

### 5.2 批量处理

```csharp
public class BatchProcessor : Node
{
    private List<IProcessable> _pendingOperations = new();
    private const int BATCH_SIZE = 100;
    
    public override void _Process(double delta)
    {
        ProcessBatch();
    }
    
    private void ProcessBatch()
    {
        int processed = 0;
        while (_pendingOperations.Count > 0 && processed < BATCH_SIZE)
        {
            var operation = _pendingOperations[0];
            _pendingOperations.RemoveAt(0);
            operation.Process();
            processed++;
        }
    }
}
```

### 5.3 内存管理

```csharp
public class MemoryManager : Node
{
    private Timer _gcTimer;
    
    public override void _Ready()
    {
        _gcTimer = new Timer();
        _gcTimer.WaitTime = 30.0f; // 30秒触发一次
        _gcTimer.Timeout += ForceGarbageCollection;
        AddChild(_gcTimer);
        _gcTimer.Start();
    }
    
    private void ForceGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
```

## 6. 错误处理和容错设计

### 6.1 异常处理策略

```csharp
public class ErrorHandler : Node
{
    public static void HandleError(Exception ex, string context)
    {
        // 记录错误日志
        GD.PrintErr($"Error in {context}: {ex.Message}");
        GD.PrintErr($"Stack trace: {ex.StackTrace}");
        
        // 根据错误类型决定处理策略
        switch (ex)
        {
            case FileNotFoundException:
                HandleMissingFile(ex, context);
                break;
            case NullReferenceException:
                HandleNullReference(ex, context);
                break;
            default:
                HandleGenericError(ex, context);
                break;
        }
    }
}
```

### 6.2 数据验证

```csharp
public class DataValidator
{
    public static bool ValidateSkillCard(SkillCard card)
    {
        if (string.IsNullOrEmpty(card.Id))
        {
            GD.PrintErr("Skill card ID cannot be empty");
            return false;
        }
        
        if (card.EnergyCost <= 0)
        {
            GD.PrintErr($"Skill card {card.Id} has invalid energy cost: {card.EnergyCost}");
            return false;
        }
        
        return true;
    }
}
```

## 7. 扩展性设计

### 7.1 插件系统

```csharp
public interface IGamePlugin
{
    string Name { get; }
    string Version { get; }
    void Initialize(GameManager gameManager);
    void Shutdown();
}

public class PluginManager : Node
{
    private List<IGamePlugin> _plugins = new();
    
    public void LoadPlugin(IGamePlugin plugin)
    {
        _plugins.Add(plugin);
        plugin.Initialize(GetNode<GameManager>("/root/GameManager"));
    }
}
```

### 7.2 模组支持

```csharp
public class ModLoader : Node
{
    public void LoadMod(string modPath)
    {
        var modConfig = LoadModConfig(modPath);
        
        // 加载自定义卡牌
        if (modConfig.CustomCards != null)
            LoadCustomCards(modConfig.CustomCards);
        
        // 加载自定义英雄
        if (modConfig.CustomHeroes != null)
            LoadCustomHeroes(modConfig.CustomHeroes);
    }
}
```

这个系统设计文档为Code Rogue项目提供了完整的技术架构指导，确保各个系统能够协调工作，同时保持良好的可维护性和扩展性。