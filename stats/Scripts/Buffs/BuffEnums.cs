using Godot;

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