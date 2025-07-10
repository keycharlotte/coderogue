using System.Collections.Generic;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BuffDatabase : Resource
{
    [Export] public Array<BuffConfig> Buffs { get; set; } = new();
    
    private Godot.Collections.Dictionary<int, BuffConfig> _buffLookup = new();
    
    public void Initialize()
    {
        _buffLookup.Clear();
        foreach (var buff in Buffs)
        {
            _buffLookup[buff.Id] = buff;
        }
        
        // 如果没有配置，创建一些默认的buff
        if (Buffs.Count == 0)
        {
            CreateDefaultBuffs();
        }
    }
    
    public BuffConfig GetBuffConfig(int id)
    {
        return _buffLookup.GetValueOrDefault(id);
    }
    
    private void CreateDefaultBuffs()
    {
        // 力量提升
        var strengthBuff = new BuffConfig
        {
            Id = 1001,
            Name = "力量提升",
            Description = "提升攻击力{0}点，持续{1}秒",
            Type = BuffType.Buff,
            Category = BuffCategory.AttributeModifier,
            MaxStack = 5,
            StackRule = BuffStackRule.Stack,
            BaseDuration = 30.0f,
            Priority = 100,
            IconPath = "res://icons/buff_strength.png",
            DisplayColor = Colors.Red,
            CanDispel = true
        };
        
        var strengthEffect = new BuffEffectData
        {
            EffectType = BuffEffectType.AddValue,
            TargetProperty = "Attack",
            ValueType = BuffValueType.Flat,
            BaseValue = 10.0f,
            PerStackValue = 5.0f,
            CalculationType = BuffCalculationType.Additive,
            TriggerTiming = BuffTriggerTiming.Continuous
        };
        
        strengthBuff.Effects.Add(strengthEffect);
        Buffs.Add(strengthBuff);
        _buffLookup[strengthBuff.Id] = strengthBuff;
        
        // 速度提升
        var speedBuff = new BuffConfig
        {
            Id = 1002,
            Name = "速度提升",
            Description = "提升移动速度{0}%，持续{1}秒",
            Type = BuffType.Buff,
            Category = BuffCategory.AttributeModifier,
            MaxStack = 3,
            StackRule = BuffStackRule.Stack,
            BaseDuration = 20.0f,
            Priority = 100,
            IconPath = "res://icons/buff_speed.png",
            DisplayColor = Colors.Blue,
            CanDispel = true
        };
        
        var speedEffect = new BuffEffectData
        {
            EffectType = BuffEffectType.PercentIncrease,
            TargetProperty = "Speed",
            ValueType = BuffValueType.Percentage,
            BaseValue = 20.0f,
            PerStackValue = 10.0f,
            CalculationType = BuffCalculationType.Multiplicative,
            TriggerTiming = BuffTriggerTiming.Continuous
        };
        
        speedBuff.Effects.Add(speedEffect);
        Buffs.Add(speedBuff);
        _buffLookup[speedBuff.Id] = speedBuff;
        
        // 中毒debuff
        var poisonDebuff = new BuffConfig
        {
            Id = 2001,
            Name = "中毒",
            Description = "每秒受到{0}点毒素伤害，持续{1}秒",
            Type = BuffType.Debuff,
            Category = BuffCategory.DoT,
            MaxStack = 10,
            StackRule = BuffStackRule.Stack,
            BaseDuration = 15.0f,
            Priority = 200,
            IconPath = "res://icons/debuff_poison.png",
            DisplayColor = Colors.Green,
            CanDispel = true
        };
        
        var poisonEffect = new BuffEffectData
        {
            EffectType = BuffEffectType.Custom,
            TargetProperty = "Health",
            ValueType = BuffValueType.Flat,
            BaseValue = 5.0f,
            PerStackValue = 2.0f,
            CalculationType = BuffCalculationType.Final,
            TriggerTiming = BuffTriggerTiming.Periodic,
            TriggerInterval = 1.0f
        };
        
        poisonDebuff.Effects.Add(poisonEffect);
        Buffs.Add(poisonDebuff);
        _buffLookup[poisonDebuff.Id] = poisonDebuff;
    }
}