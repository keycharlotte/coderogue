using Godot;
using Godot.Collections;

public enum BuffType
{
    Buff,      // 正面效果
    Debuff,    // 负面效果
    Neutral    // 中性效果
}

public enum BuffCategory
{
    AttributeModifier,  // 属性修改
    StatusEffect,       // 状态效果
    Trigger,           // 触发效果
    Aura,              // 光环效果
    Shield,            // 护盾效果
    DoT,               // 持续伤害
    HoT,               // 持续治疗
    Control            // 控制效果
}

public enum BuffStackRule
{
    None,           // 不可叠加
    Replace,        // 替换旧的
    Refresh,        // 刷新时间
    Stack,          // 叠加层数
    Extend,         // 延长时间
    Strongest       // 保留最强的
}

public enum BuffEffectType
{
    AddValue,           // 加法
    MultiplyValue,      // 乘法
    SetValue,           // 设置值
    PercentIncrease,    // 百分比增加
    PercentDecrease,    // 百分比减少
    Custom              // 自定义
}

public enum BuffValueType
{
    Flat,               // 固定值
    Percentage,         // 百分比
    Formula             // 公式计算
}

public enum BuffCalculationType
{
    Additive,           // 加法计算
    Multiplicative,     // 乘法计算
    Final,              // 最终计算
    Override            // 覆盖计算
}

public enum BuffTriggerTiming
{
    OnApply,            // 应用时
    OnRemove,           // 移除时
    Continuous,         // 持续
    Periodic,           // 周期性
    OnEvent,            // 事件触发
    OnCondition         // 条件触发
}

[GlobalClass]
public partial class BuffEffectData : Resource
{
    [Export] public BuffEffectType EffectType { get; set; }
    [Export] public string TargetProperty { get; set; }
    [Export] public BuffValueType ValueType { get; set; }
    [Export] public float BaseValue { get; set; }
    [Export] public float PerStackValue { get; set; }
    [Export] public BuffCalculationType CalculationType { get; set; }
    [Export] public BuffTriggerTiming TriggerTiming { get; set; }
    [Export] public float TriggerInterval { get; set; }
    [Export] public string Formula { get; set; }
    [Export] public string EventName { get; set; }
    [Export] public string Condition { get; set; }
    [Export] public Dictionary CustomData { get; set; } = new();
}

[GlobalClass]
public partial class BuffConfig : Resource
{
    [Export] public int Id { get; set; }
    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public BuffType Type { get; set; }
    [Export] public BuffCategory Category { get; set; }
    [Export] public int MaxStack { get; set; } = 1;
    [Export] public BuffStackRule StackRule { get; set; } = BuffStackRule.None;
    [Export] public float BaseDuration { get; set; }
    [Export] public int Priority { get; set; } = 100;
    [Export] public string IconPath { get; set; }
    [Export] public Color DisplayColor { get; set; } = Colors.White;
    [Export] public bool CanDispel { get; set; } = true;
    [Export] public bool IsPersistent { get; set; } = false;
    [Export] public Array<string> Tags { get; set; } = new();
    [Export] public Array<BuffEffectData> Effects { get; set; } = new();
    [Export] public Dictionary CustomData { get; set; } = new();
}