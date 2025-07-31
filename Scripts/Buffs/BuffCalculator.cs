using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using CodeRogue.Buffs;
public partial class BuffCalculator : RefCounted
{
    public static float CalculateProperty(Node target, string property)
    {
        var buffManager = target.GetNode<BuffManager>("/root/BuffManager");
        var buffs = buffManager?.GetTargetBuffs(target) ?? new Array<BuffInstance>();
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
    
    public static float CalculateEffectValue(BuffInstance buff, BuffEffectData effect)
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
    
    private static float GetBasePropertyValue(Node target, string property)
    {
        if (target is IBuffTarget buffTarget)
        {
            return buffTarget.GetPropertyValue(property);
        }
        return 0f;
    }
    
    private static float ApplyAdditiveEffects(float baseValue, IEnumerable<BuffInstance> buffs, string property)
    {
        float additiveValue = 0f;
        
        foreach (var buff in buffs)
        {
            foreach (var effectInstance in buff.EffectInstances)
            {
                var effect = effectInstance.EffectData;
                if (effect.TargetProperty == property && effect.CalculationType == BuffCalculationType.Additive)
                {
                    additiveValue += CalculateEffectValue(buff, effect);
                }
            }
        }
        
        return baseValue + additiveValue;
    }
    
    private static float ApplyMultiplicativeEffects(float currentValue, IEnumerable<BuffInstance> buffs, string property)
    {
        float multiplier = 1f;
        
        foreach (var buff in buffs)
        {
            foreach (var effectInstance in buff.EffectInstances)
            {
                var effect = effectInstance.EffectData;
                if (effect.TargetProperty == property && effect.CalculationType == BuffCalculationType.Multiplicative)
                {
                    multiplier *= (1f + CalculateEffectValue(buff, effect) / 100f);
                }
            }
        }
        
        return currentValue * multiplier;
    }
    
    private static float ApplyFinalEffects(float currentValue, IEnumerable<BuffInstance> buffs, string property)
    {
        float finalValue = currentValue;
        
        foreach (var buff in buffs)
        {
            foreach (var effectInstance in buff.EffectInstances)
            {
                var effect = effectInstance.EffectData;
                if (effect.TargetProperty == property)
                {
                    if (effect.CalculationType == BuffCalculationType.Final)
                    {
                        finalValue += CalculateEffectValue(buff, effect);
                    }
                    else if (effect.CalculationType == BuffCalculationType.Override)
                    {
                        finalValue = CalculateEffectValue(buff, effect);
                    }
                }
            }
        }
        
        return finalValue;
    }
    
    private static float EvaluateFormula(string formula, BuffInstance buff)
    {
        // 简单的公式计算实现
        // 可以使用更复杂的表达式解析器
        formula = formula.Replace("{stack}", buff.CurrentStack.ToString());
        formula = formula.Replace("{duration}", buff.RemainingTime.ToString());
        formula = formula.Replace("{progress}", buff.Progress.ToString());
        
        // 这里可以实现更复杂的公式解析
        // 暂时返回基础值
        return 0f;
    }
}