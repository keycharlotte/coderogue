# Buff系统设计文档 (Godot 4.4+)

## 📋 目录
1. [系统概述](#系统概述)
2. [核心架构](#核心架构)
3. [数据结构设计](#数据结构设计)
4. [Buff分类体系](#buff分类体系)
5. [管理机制](#管理机制)
6. [效果计算系统](#效果计算系统)
7. [UI表现设计](#ui表现设计)
8. [配置系统](#配置系统)
9. [技术实现](#技术实现)
10. [性能优化](#性能优化)
11. [扩展性设计](#扩展性设计)

## 🎯 系统概述

### 设计目标
- **通用性**：支持各种类型的buff效果
- **可扩展性**：易于添加新的buff类型和效果
- **高性能**：优化的计算和存储机制
- **易维护**：清晰的代码结构和配置系统
- **用户友好**：直观的UI表现和反馈

### 核心特性
- 支持多种buff类型（增益、减益、中性）
- 灵活的持续时间管理
- 可叠加的buff效果
- 优先级和冲突处理
- 丰富的触发条件
- 实时效果计算

## 🏗️ 核心架构

### 系统组件图
─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   BuffManager   │◄──►│   BuffInstance  │◄──►│   BuffConfig    │
│   (管理器)      │    │   (实例)        │    │   (配置)        │
└─────────────────┘    └─────────────────┘    └─────────────────┘
│                       │                       │
▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  BuffCalculator │    │   BuffEffect    │    │   BuffDatabase  │
│  (计算器)       │    │   (效果)        │    │   (数据库)      │
└─────────────────┘    └─────────────────┘    └─────────────────


### 核心类关系
- **BuffManager**: 全局buff管理器，负责所有buff的生命周期
- **BuffInstance**: buff实例，包含具体的buff数据和状态
- **BuffConfig**: buff配置数据，定义buff的基础属性
- **BuffEffect**: buff效果处理器，负责具体效果的计算和应用
- **BuffCalculator**: 效果计算器，处理复杂的数值计算
- **BuffDatabase**: buff数据库，管理所有buff配置

## 📊 数据结构设计

### BuffConfig (Buff配置资源)
```csharp
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BuffConfig : Resource
{
    [Export] public int Id { get; set; }                    // Buff唯一ID
    [Export] public string Name { get; set; }               // Buff名称
    [Export] public string Description { get; set; }        // Buff描述
    [Export] public BuffType Type { get; set; }             // Buff类型
    [Export] public BuffCategory Category { get; set; }     // Buff分类
    [Export] public int MaxStack { get; set; }              // 最大叠加层数
    [Export] public float BaseDuration { get; set; }        // 基础持续时间
    [Export] public int Priority { get; set; }              // 优先级
    [Export] public Array<BuffEffectData> Effects { get; set; } // 效果列表
    [Export] public BuffTriggerCondition TriggerCondition { get; set; } // 触发条件
    [Export] public string IconPath { get; set; }           // 图标路径
    [Export] public Color DisplayColor { get; set; }        // 显示颜色
    [Export] public Array<int> ConflictBuffs { get; set; }  // 冲突buff列表
    [Export] public bool CanDispel { get; set; }            // 是否可驱散
    [Export] public bool IsPersistent { get; set; }         // 是否持久化
}

[GlobalClass]
public partial class BuffInstance : RefCounted
{
    [Export] public string InstanceId { get; set; }         // 实例唯一ID
    [Export] public int ConfigId { get; set; }              // 配置ID
    [Export] public BuffConfig Config { get; set; }         // 配置引用
    [Export] public Node Target { get; set; }               // 目标节点
    [Export] public Node Caster { get; set; }               // 施法者节点
    [Export] public int CurrentStack { get; set; }          // 当前叠加层数
    [Export] public float RemainingTime { get; set; }       // 剩余时间
    [Export] public float TotalDuration { get; set; }       // 总持续时间
    [Export] public BuffState State { get; set; }           // 当前状态
    [Export] public double CreateTime { get; set; }         // 创建时间
    [Export] public double LastUpdateTime { get; set; }     // 最后更新时间
    [Export] public Dictionary CustomData { get; set; }     // 自定义数据
    [Export] public Array<BuffEffectInstance> EffectInstances { get; set; } // 效果实例
}

[GlobalClass]
public partial class BuffEffectInstance : RefCounted
{
    [Export] public BuffEffectConfig Config { get; set; }
    [Export] public BuffInstance Parent { get; set; }
    [Export] public BuffEffectState State { get; set; }
    [Export] public double CreateTime { get; set; }
    [Export] public double LastUpdateTime { get; set; }
    [Export] public Dictionary CustomData { get; set; }
}

[GlobalClass]
public partial class BuffEffectData : Resource
{
    [Export] public BuffEffectType EffectType { get; set; } // 效果类型
    [Export] public string TargetProperty { get; set; }     // 目标属性
    [Export] public BuffValueType ValueType { get; set; }   // 数值类型
    [Export] public float BaseValue { get; set; }           // 基础数值
    [Export] public float PerStackValue { get; set; }       // 每层数值
    [Export] public BuffCalculationType CalculationType { get; set; } // 计算类型
    [Export] public string Formula { get; set; }            // 自定义公式
    [Export] public BuffTriggerTiming TriggerTiming { get; set; } // 触发时机
    [Export] public float TriggerInterval { get; set; }     // 触发间隔
    [Export] public int TriggerCount { get; set; }          // 触发次数
    [Export] public Dictionary Parameters { get; set; }     // 参数
}

public enum BuffType
{
    Buff,       // 增益效果
    Debuff,     // 减益效果
    Neutral     // 中性效果
}
public enum BuffCategory
{
    // 属性类
    AttributeModifier,  // 属性修改
    StatBoost,         // 数值提升
    StatReduction,     // 数值降低
    
    // 状态类
    StatusEffect,      // 状态效果
    ControlEffect,     // 控制效果
    ProtectionEffect,  // 保护效果
    
    // 行为类
    BehaviorModifier,  // 行为修改
    SkillModifier,     // 技能修改
    MovementModifier,  // 移动修改
    
    // 特殊类
    Transformation,    // 变身效果
    Aura,             // 光环效果
    Trigger,          // 触发效果
    Custom            // 自定义效果
}
public enum BuffEffectType
{
    // 数值修改
    AddValue,          // 加法修改
    MultiplyValue,     // 乘法修改
    SetValue,          // 设置数值
    
    // 持续效果
    PeriodicDamage,    // 持续伤害
    PeriodicHeal,      // 持续治疗
    PeriodicRestore,   // 持续恢复
    
    // 状态控制
    Stun,              // 眩晕
    Silence,           // 沉默
    Root,              // 定身
    Slow,              // 减速
    Haste,             // 加速
    
    // 特殊效果
    Immunity,          // 免疫
    Reflection,        // 反射
    Absorption,        // 吸收
    Invisibility,      // 隐身
    
    // 自定义
    CustomEffect       // 自定义效果
}
using Godot;
using Godot.Collections;

public partial class BuffManager : Node
{
    private static BuffManager _instance;
    public static BuffManager Instance => _instance;
    
    [Signal] public delegate void BuffAppliedEventHandler(BuffInstance buff);
    [Signal] public delegate void BuffRemovedEventHandler(BuffInstance buff);
    [Signal] public delegate void BuffExpiredEventHandler(BuffInstance buff);
    [Signal] public delegate void BuffStackedEventHandler(BuffInstance buff, int newStack);
    
    private Dictionary<Node, Array<BuffInstance>> _targetBuffs;
    private Dictionary<string, BuffInstance> _allBuffs;
    private BuffDatabase _database;
    private BuffCalculator _calculator;
    
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
    
    // 核心方法
    public BuffInstance ApplyBuff(int configId, Node target, Node caster = null)
    {
        var config = _database.GetBuffConfig(configId);
        if (config == null) return null;
        
        var instance = CreateBuffInstance(config, target, caster);
        AddBuffToTarget(target, instance);
        
        EmitSignal(SignalName.BuffApplied, instance);
        return instance;
    }
    
    public bool RemoveBuff(string instanceId)
    {
        if (_allBuffs.TryGetValue(instanceId, out var buff))
        {
            RemoveBuffFromTarget(buff.Target, buff);
            EmitSignal(SignalName.BuffRemoved, buff);
            return true;
        }
        return false;
    }
    
    public bool RemoveBuffByType(Node target, BuffType type)
    {
        if (!_targetBuffs.TryGetValue(target, out var buffs)) return false;
        
        bool removed = false;
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            if (buffs[i].Config.Type == type)
            {
                RemoveBuff(buffs[i].InstanceId);
                removed = true;
            }
        }
        return removed;
    }
    
    public override void _Process(double delta)
    {
        UpdateBuffs((float)delta);
    }
    
    public void UpdateBuffs(float deltaTime)
    {
        var buffsToRemove = new Array<BuffInstance>();
        
        foreach (var buff in _allBuffs.Values)
        {
            buff.RemainingTime -= deltaTime;
            buff.LastUpdateTime = Time.GetUnixTimeFromSystem();
            
            // 更新效果
            UpdateBuffEffects(buff, deltaTime);
            
            // 检查是否过期
            if (buff.RemainingTime <= 0)
            {
                buffsToRemove.Add(buff);
            }
        }
        
        // 移除过期buff
        foreach (var buff in buffsToRemove)
        {
            RemoveBuff(buff.InstanceId);
            EmitSignal(SignalName.BuffExpired, buff);
        }
    }
    
    public Array<BuffInstance> GetTargetBuffs(Node target)
    {
        return _targetBuffs.GetValueOrDefault(target, new Array<BuffInstance>());
    }
    
    public float CalculatePropertyValue(Node target, string property)
    {
        return _calculator.CalculateProperty(target, property);
    }
}

### 生命周期管理
1. 创建阶段
   
   - 验证配置有效性
   - 检查冲突和叠加规则
   - 创建buff实例
   - 应用初始效果
2. 更新阶段
   
   - 时间倒计时
   - 周期性效果触发
   - 状态检查和更新
   - 效果重新计算
3. 销毁阶段
   
   - 移除所有效果
   - 清理资源引用
   - 触发结束事件
   - 更新UI显示
### 叠加规则
public enum BuffStackRule
{
    None,           // 不可叠加
    Replace,        // 替换旧的
    Refresh,        // 刷新时间
    Stack,          // 叠加层数
    Extend,         // 延长时间
    Strongest       // 保留最强的
}
using Godot;
using Godot.Collections;

public partial class BuffCalculator : RefCounted
{
    // 属性计算
    public float CalculateProperty(Node target, string property)
    {
        var buffs = BuffManager.Instance.GetTargetBuffs(target);
        float baseValue = GetBasePropertyValue(target, property);
        float finalValue = baseValue;
        
        // 按优先级排序
        var sortedBuffs = buffs.OrderBy(b => b.Config.Priority);
        
        // 分阶段计算
        finalValue = ApplyAdditiveEffects(finalValue, sortedBuffs, property);
        finalValue = ApplyMultiplicativeEffects(finalValue, sortedBuffs, property);
        finalValue = ApplyFinalEffects(finalValue, sortedBuffs, property);
        
        return finalValue;
    }
    
    // 效果强度计算
    public float CalculateEffectValue(BuffInstance buff, BuffEffectData effect)
    {
        float value = effect.BaseValue;
        
        // 叠加层数影响
        if (buff.CurrentStack > 1)
        {
            value += effect.PerStackValue * (buff.CurrentStack - 1);
        }
        
        // 自定义公式
        if (!string.IsNullOrEmpty(effect.Formula))
        {
            value = EvaluateFormula(effect.Formula, buff);
        }
        
        return value;
    }
    
    private float GetBasePropertyValue(Node target, string property)
    {
        // 通过反射或接口获取基础属性值
        if (target is IBuffTarget buffTarget)
        {
            return buffTarget.GetPropertyValue(property);
        }
        return 0f;
    }
}

### 计算优先级
1. 基础值 (Base Value)
2. 加法修改 (Additive Modifiers)
3. 乘法修改 (Multiplicative Modifiers)
4. 最终修改 (Final Modifiers)
5. 绝对设置 (Absolute Set)

using Godot;
using Godot.Collections;

public partial class BuffUI : Control
{
    [Export] public Container BuffContainer { get; set; }     // Buff容器
    [Export] public PackedScene BuffIconScene { get; set; }   // Buff图标场景
    [Export] public RichTextLabel BuffTooltip { get; set; }   // Buff提示文本
    
    private Dictionary<string, BuffIconUI> _buffIcons = new();
    
    public override void _Ready()
    {
        // 连接BuffManager信号
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.BuffApplied += OnBuffApplied;
            BuffManager.Instance.BuffRemoved += OnBuffRemoved;
        }
    }
    
    public void UpdateBuffDisplay(Node target)
    {
        var buffs = BuffManager.Instance.GetTargetBuffs(target);
        
        // 更新图标显示
        foreach (var buff in buffs)
        {
            UpdateBuffIcon(buff);
        }
        
        // 清理过期图标
        CleanupExpiredIcons(buffs);
    }
    
    private void OnBuffApplied(BuffInstance buff)
    {
        CreateBuffIcon(buff);
    }
    
    private void OnBuffRemoved(BuffInstance buff)
    {
        RemoveBuffIcon(buff.InstanceId);
    }
}

public partial class BuffIconUI : Control
{
    [Export] public TextureRect IconImage { get; set; }      // 图标
    [Export] public Label StackText { get; set; }           // 层数文本
    [Export] public TextureProgressBar DurationProgress { get; set; } // 持续时间进度
    [Export] public NinePatchRect BorderImage { get; set; }  // 边框
    
    private BuffInstance _buffInstance;
    private Tween _tween;
    
    public override void _Ready()
    {
        _tween = CreateTween();
    }
    
    public void SetBuffInstance(BuffInstance buffInstance)
    {
        _buffInstance = buffInstance;
        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        if (_buffInstance == null) return;
        
        // 更新图标
        var texture = GD.Load<Texture2D>(_buffInstance.Config.IconPath);
        if (texture != null)
            IconImage.Texture = texture;
        
        // 更新层数
        StackText.Text = _buffInstance.CurrentStack > 1 ? 
            _buffInstance.CurrentStack.ToString() : "";
        
        // 更新持续时间
        float progress = _buffInstance.RemainingTime / _buffInstance.TotalDuration;
        DurationProgress.Value = progress * 100;
        
        // 更新边框颜色
        BorderImage.Modulate = GetTypeColor(_buffInstance.Config.Type);
        
        // 即将结束时闪烁
        if (progress < 0.2f)
        {
            StartBlinkAnimation();
        }
    }
    
    private Color GetTypeColor(BuffType type)
    {
        return type switch
        {
            BuffType.Buff => Colors.Green,
            BuffType.Debuff => Colors.Red,
            BuffType.Neutral => Colors.Gray,
            _ => Colors.White
        };
    }
    
    private void StartBlinkAnimation()
    {
        _tween.TweenProperty(this, "modulate:a", 0.5f, 0.5f);
        _tween.TweenProperty(this, "modulate:a", 1.0f, 0.5f);
        _tween.SetLoops();
    }
}

### 视觉效果
- 图标动画 : 获得/失去buff时的缩放动画
- 颜色编码 : 不同类型buff使用不同颜色边框
- 持续时间 : 圆形或条形进度条显示剩余时间
- 叠加显示 : 数字显示叠加层数
- 闪烁效果 : 即将结束时的闪烁提醒
{
  "buffs": [
    {
      "id": 1001,
      "name": "力量提升",
      "description": "提升攻击力{0}点，持续{1}秒",
      "type": "Buff",
      "category": "AttributeModifier",
      "maxStack": 5,
      "baseDuration": 30.0,
      "priority": 100,
      "iconPath": "res://icons/buff_strength.png",
      "displayColor": "#FF6B6B",
      "canDispel": true,
      "effects": [
        {
          "effectType": "AddValue",
          "targetProperty": "Attack",
          "valueType": "Flat",
          "baseValue": 10.0,
          "perStackValue": 5.0,
          "calculationType": "Additive",
          "triggerTiming": "Continuous"
        }
      ]
    }
  ]
}
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BuffDatabase : Resource
{
    [Export] public Array<BuffConfig> Buffs { get; set; } = new();
    
    private Dictionary<int, BuffConfig> _buffLookup = new();
    
    public void Initialize()
    {
        _buffLookup.Clear();
        foreach (var buff in Buffs)
        {
            _buffLookup[buff.Id] = buff;
        }
    }
    
    public BuffConfig GetBuffConfig(int id)
    {
        return _buffLookup.GetValueOrDefault(id);
    }
    
    public void LoadFromJson(string filePath)
    {
        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) return;
        
        var jsonString = file.GetAsText();
        var json = new Json();
        var parseResult = json.Parse(jsonString);
        
        if (parseResult != Error.Ok) return;
        
        var data = json.Data.AsGodotDictionary();
        // 解析JSON数据到BuffConfig
    }
    
    public void SaveToJson(string filePath)
    {
        var data = new Dictionary();
        var buffsArray = new Array();
        
        foreach (var buff in Buffs)
        {
            // 将BuffConfig转换为Dictionary
            buffsArray.Add(BuffConfigToDict(buff));
        }
        
        data["buffs"] = buffsArray;
        
        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(Json.Stringify(data));
        }
    }
}
#if TOOLS
using Godot;

[Tool]
public partial class BuffDatabaseEditor : EditorPlugin
{
    private Control _dockContainer;
    
    public override void _EnterTree()
    {
        // 创建编辑器界面
        _dockContainer = GD.Load<PackedScene>("res://addons/buff_system/BuffEditor.tscn").Instantiate<Control>();
        AddControlToDock(DockSlot.LeftBr, _dockContainer);
    }
    
    public override void _ExitTree()
    {
        RemoveControlFromDocks(_dockContainer);
    }
}

[Tool]
public partial class BuffEditorDock : Control
{
    [Export] public Button AddBuffButton { get; set; }
    [Export] public ItemList BuffList { get; set; }
    [Export] public Button SaveButton { get; set; }
    
    private BuffDatabase _database;
    
    public override void _Ready()
    {
        AddBuffButton.Pressed += OnAddBuffPressed;
        SaveButton.Pressed += OnSavePressed;
        
        LoadDatabase();
    }
    
    private void OnAddBuffPressed()
    {
        var newBuff = new BuffConfig();
        newBuff.Id = GetNextBuffId();
        newBuff.Name = "新Buff";
        
        _database.Buffs.Add(newBuff);
        RefreshBuffList();
    }
    
    private void OnSavePressed()
    {
        ResourceSaver.Save(_database, "res://data/BuffDatabase.tres");
        GD.Print("Buff数据库已保存");
    }
}
#endif
public interface IBuffEffect
{
    void OnApply(BuffInstance buff);
    void OnUpdate(BuffInstance buff, float deltaTime);
    void OnRemove(BuffInstance buff);
    void OnStack(BuffInstance buff, int newStack);
}

public interface IBuffTarget
{
    void OnBuffApplied(BuffInstance buff);
    void OnBuffRemoved(BuffInstance buff);
    float GetPropertyValue(string property);
    void SetPropertyValue(string property, float value);
}

public interface IBuffTrigger
{
    bool ShouldTrigger(BuffInstance buff, Variant context);
    void OnTrigger(BuffInstance buff, Variant context);
}

[GlobalClass]
public partial class BuffSaveData : Resource
{
    [Export] public string InstanceId { get; set; }
    [Export] public int ConfigId { get; set; }
    [Export] public int CurrentStack { get; set; }
    [Export] public float RemainingTime { get; set; }
    [Export] public string TargetPath { get; set; }
    [Export] public string CasterPath { get; set; }
    [Export] public Dictionary CustomData { get; set; }
    
    public static BuffSaveData FromInstance(BuffInstance instance)
    {
        return new BuffSaveData
        {
            InstanceId = instance.InstanceId,
            ConfigId = instance.ConfigId,
            CurrentStack = instance.CurrentStack,
            RemainingTime = instance.RemainingTime,
            TargetPath = instance.Target?.GetPath(),
            CasterPath = instance.Caster?.GetPath(),
            CustomData = instance.CustomData
        };
    }
    
    public BuffInstance ToInstance(Node sceneRoot)
    {
        var instance = new BuffInstance();
        instance.InstanceId = InstanceId;
        instance.ConfigId = ConfigId;
        instance.CurrentStack = CurrentStack;
        instance.RemainingTime = RemainingTime;
        instance.CustomData = CustomData;
        
        // 恢复节点引用
        if (!string.IsNullOrEmpty(TargetPath))
            instance.Target = sceneRoot.GetNode(TargetPath);
        if (!string.IsNullOrEmpty(CasterPath))
            instance.Caster = sceneRoot.GetNode(CasterPath);
            
        return instance;
    }
}

public static class BuffEvents
{
    public static event System.Action<BuffInstance> OnBuffApplied;
    public static event System.Action<BuffInstance> OnBuffRemoved;
    public static event System.Action<BuffInstance> OnBuffExpired;
    public static event System.Action<BuffInstance, int> OnBuffStacked;
    public static event System.Action<BuffInstance> OnBuffTriggered;
    
    public static void TriggerBuffApplied(BuffInstance buff)
    {
        OnBuffApplied?.Invoke(buff);
    }
}

public partial class BuffInstancePool : RefCounted
{
    private Queue<BuffInstance> _pool = new();
    private int _maxPoolSize = 100;
    
    public BuffInstance Get()
    {
        if (_pool.Count > 0)
        {
            var instance = _pool.Dequeue();
            instance.Reset();
            return instance;
        }
        return new BuffInstance();
    }
    
    public void Return(BuffInstance instance)
    {
        if (_pool.Count < _maxPoolSize)
        {
            _pool.Enqueue(instance);
        }
    }
}

public partial class BuffUpdateBatch : RefCounted
{
    private Array<BuffInstance> _buffsToUpdate = new();
    private Array<BuffInstance> _buffsToRemove = new();
    
    public void Update(float deltaTime)
    {
        // 批量更新
        for (int i = 0; i < _buffsToUpdate.Count; i++)
        {
            var buff = _buffsToUpdate[i];
            UpdateBuff(buff, deltaTime);
            
            if (buff.RemainingTime <= 0)
            {
                _buffsToRemove.Add(buff);
            }
        }
        
        // 批量移除
        foreach (var buff in _buffsToRemove)
        {
            RemoveBuff(buff);
        }
        _buffsToRemove.Clear();
    }
}

public partial class BuffPropertyCache : RefCounted
{
    private Dictionary<string, CachedProperty> _cache = new();
    
    public float GetCachedProperty(Node target, string property)
    {
        string key = $"{target.GetInstanceId()}_{property}";
        
        if (_cache.TryGetValue(key, out var cached))
        {
            if (Time.GetUnixTimeFromSystem() - cached.LastUpdateTime < 0.1) // 100ms缓存
            {
                return cached.Value;
            }
        }
        
        float value = CalculateProperty(target, property);
        _cache[key] = new CachedProperty { Value = value, LastUpdateTime = Time.GetUnixTimeFromSystem() };
        return value;
    }
}

public partial class CachedProperty : RefCounted
{
    public float Value { get; set; }
    public double LastUpdateTime { get; set; }
}
public abstract partial class CustomBuffEffect : RefCounted, IBuffEffect
{
    public abstract string EffectName { get; }
    public abstract void OnApply(BuffInstance buff);
    public abstract void OnUpdate(BuffInstance buff, float deltaTime);
    public abstract void OnRemove(BuffInstance buff);
    public virtual void OnStack(BuffInstance buff, int newStack) { }
}

// 示例：自定义吸血效果
public partial class LifestealEffect : CustomBuffEffect
{
    public override string EffectName => "Lifesteal";
    
    public override void OnApply(BuffInstance buff)
    {
        // 连接攻击信号
        if (buff.Target is IBuffTarget target)
        {
            // 注册攻击事件监听
        }
    }
    
    public override void OnUpdate(BuffInstance buff, float deltaTime)
    {
        // 持续更新逻辑
    }
    
    public override void OnRemove(BuffInstance buff)
    {
        // 断开信号连接
    }
}

public interface IBuffModule
{
    string ModuleName { get; }
    void Initialize(BuffManager manager);
    void Update(float deltaTime);
    void Cleanup();
}

public partial class BuffPersistenceModule : RefCounted, IBuffModule
{
    public string ModuleName => "Persistence";
    
    public void Initialize(BuffManager manager)
    {
        // 初始化持久化系统
    }
    
    public void Update(float deltaTime)
    {
        // 定期保存数据
    }
    
    public void Cleanup()
    {
        // 清理资源
    }
    
    public void SaveBuffs(Node target)
    {
        var buffs = BuffManager.Instance.GetTargetBuffs(target);
        var saveData = new Array<BuffSaveData>();
        
        foreach (var buff in buffs)
        {
            if (buff.Config.IsPersistent)
            {
                saveData.Add(BuffSaveData.FromInstance(buff));
            }
        }
        
        // 保存到文件
        var savePath = $"user://buff_save_{target.GetInstanceId()}.tres";
        ResourceSaver.Save(saveData, savePath);
    }
    
    public void LoadBuffs(Node target)
    {
        var savePath = $"user://buff_save_{target.GetInstanceId()}.tres";
        if (ResourceLoader.Exists(savePath))
        {
            var saveData = ResourceLoader.Load<Array<BuffSaveData>>(savePath);
            foreach (var data in saveData)
            {
                var instance = data.ToInstance(target.GetTree().CurrentScene);
                BuffManager.Instance.RestoreBuffInstance(instance);
            }
        }
    }
}