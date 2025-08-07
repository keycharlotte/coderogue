using Godot;
using Godot.Collections;
using System.Linq;
using CodeRogue.Buffs;
using CodeRogue.Utils;

[GlobalClass]
public partial class BuffManager : Node
{
	[Signal] public delegate void BuffAppliedEventHandler(BuffInstance buff);
	[Signal] public delegate void BuffRemovedEventHandler(BuffInstance buff);
	[Signal] public delegate void BuffExpiredEventHandler(BuffInstance buff);
	[Signal] public delegate void BuffStackedEventHandler(BuffInstance buff, int newStack);
	
	private BuffDatabase _database;
	private Godot.Collections.Dictionary<string, BuffInstance> _allBuffs = new();
	private Godot.Collections.Dictionary<Node, Array<BuffInstance>> _targetBuffs = new();
	
	public override void _Ready()
	{
		InitializeSystem();
	}
	
	private void InitializeSystem()
	{
		_database = NodeUtils.GetBuffDatabase(this);
		if (_database == null)
		{
			GD.PrintErr("BuffDatabase autoload not found!");
			return;
		}
	}
	
	public BuffInstance ApplyBuff(int configId, Node target, Node caster = null)
	{
		var config = _database.GetBuffConfig(configId);
		if (config == null)
		{
			GD.PrintErr($"Buff config not found: {configId}");
			return null;
		}
		
		// 检查叠加规则
		var existingBuff = GetExistingBuff(target, configId);
		if (existingBuff != null)
		{
			return HandleBuffStacking(existingBuff, config, caster);
		}
		
		var instance = CreateBuffInstance(config, target, caster);
		AddBuffToTarget(target, instance);
		
		EmitSignal(SignalName.BuffApplied, instance);
		return instance;
	}
	
	private BuffInstance CreateBuffInstance(BuffConfig config, Node target, Node caster)
	{
		var instance = new BuffInstance
		{
			ConfigId = config.Id,
			Config = config,
			Target = target,
			Caster = caster,
			TotalDuration = config.BaseDuration,
			RemainingTime = config.BaseDuration
		};
		
		// 创建效果实例
		foreach (var effectData in config.Effects)
		{
			var effectInstance = new BuffEffectInstance
			{
				EffectData = effectData,
				NextTriggerTime = effectData.TriggerInterval
			};
			instance.EffectInstances.Add(effectInstance);
		}
		
		return instance;
	}
	
	private BuffInstance GetExistingBuff(Node target, int configId)
	{
		if (!_targetBuffs.TryGetValue(target, out var buffs)) return null;
		
		return buffs.FirstOrDefault(b => b.ConfigId == configId);
	}
	
	private BuffInstance HandleBuffStacking(BuffInstance existingBuff, BuffConfig config, Node caster)
	{
		switch (config.StackRule)
		{
			case BuffStackRule.None:
				return existingBuff;
				
			case BuffStackRule.Replace:
				RemoveBuff(existingBuff.InstanceId);
				return ApplyBuff(config.Id, existingBuff.Target, caster);
				
			case BuffStackRule.Refresh:
				existingBuff.RemainingTime = config.BaseDuration;
				existingBuff.TotalDuration = config.BaseDuration;
				return existingBuff;
				
			case BuffStackRule.Stack:
				if (existingBuff.CurrentStack < config.MaxStack)
				{
					existingBuff.CurrentStack++;
					EmitSignal(SignalName.BuffStacked, existingBuff, existingBuff.CurrentStack);
				}
				return existingBuff;
				
			case BuffStackRule.Extend:
				existingBuff.RemainingTime += config.BaseDuration;
				existingBuff.TotalDuration += config.BaseDuration;
				return existingBuff;
				
			case BuffStackRule.Strongest:
				// 比较强度，保留更强的
				return existingBuff;
				
			default:
				return existingBuff;
		}
	}
	
	private void AddBuffToTarget(Node target, BuffInstance instance)
	{
		_allBuffs[instance.InstanceId] = instance;
		
		if (!_targetBuffs.ContainsKey(target))
		{
			_targetBuffs[target] = new Array<BuffInstance>();
		}
		_targetBuffs[target].Add(instance);
		
		// 应用初始效果
		ApplyBuffEffects(instance);
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
	
	private void RemoveBuffFromTarget(Node target, BuffInstance buff)
	{
		// 移除效果
		RemoveBuffEffects(buff);
		
		// 从数据结构中移除
		_allBuffs.Remove(buff.InstanceId);
		if (_targetBuffs.TryGetValue(target, out var buffs))
		{
			buffs.Remove(buff);
			if (buffs.Count == 0)
			{
				_targetBuffs.Remove(target);
			}
		}
	}
	
	public override void _Process(double delta)
	{
		UpdateBuffs((float)delta);
	}
	
	private void UpdateBuffs(float deltaTime)
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
	
	private void ApplyBuffEffects(BuffInstance buff)
	{
		foreach (var effectInstance in buff.EffectInstances)
		{
			if (effectInstance.EffectData.TriggerTiming == BuffTriggerTiming.OnApply)
			{
				ApplyEffect(buff, effectInstance);
			}
		}
	}
	
	private void UpdateBuffEffects(BuffInstance buff, float deltaTime)
	{
		foreach (var effectInstance in buff.EffectInstances)
		{
			var effectData = effectInstance.EffectData;
			
			switch (effectData.TriggerTiming)
			{
				case BuffTriggerTiming.Continuous:
					ApplyEffect(buff, effectInstance);
					break;
					
				case BuffTriggerTiming.Periodic:
					effectInstance.NextTriggerTime -= deltaTime;
					if (effectInstance.NextTriggerTime <= 0)
					{
						ApplyEffect(buff, effectInstance);
						effectInstance.NextTriggerTime = effectData.TriggerInterval;
					}
					break;
			}
		}
	}
	
	private void RemoveBuffEffects(BuffInstance buff)
	{
		foreach (var effectInstance in buff.EffectInstances)
		{
			if (effectInstance.EffectData.TriggerTiming == BuffTriggerTiming.OnRemove)
			{
				ApplyEffect(buff, effectInstance);
			}
		}
	}
	
	private void ApplyEffect(BuffInstance buff, BuffEffectInstance effectInstance)
	{
		var effectData = effectInstance.EffectData;
		var value = BuffCalculator.CalculateEffectValue(buff, effectData);
		
		// 根据效果类型应用效果
		switch (effectData.EffectType)
		{
			case BuffEffectType.AddValue:
			case BuffEffectType.MultiplyValue:
			case BuffEffectType.SetValue:
			case BuffEffectType.PercentIncrease:
			case BuffEffectType.PercentDecrease:
				ApplyPropertyModification(buff.Target, effectData.TargetProperty, value, effectData.EffectType);
				break;
				
			case BuffEffectType.Custom:
				ApplyCustomEffect(buff, effectInstance);
				break;
		}
	}
	
	private void ApplyPropertyModification(Node target, string property, float value, BuffEffectType effectType)
	{
		if (target is IBuffTarget buffTarget)
		{
			var currentValue = buffTarget.GetPropertyValue(property);
			float newValue = currentValue;
			
			switch (effectType)
			{
				case BuffEffectType.AddValue:
					newValue = currentValue + value;
					break;
				case BuffEffectType.MultiplyValue:
					newValue = currentValue * value;
					break;
				case BuffEffectType.SetValue:
					newValue = value;
					break;
				case BuffEffectType.PercentIncrease:
					newValue = currentValue * (1 + value / 100);
					break;
				case BuffEffectType.PercentDecrease:
					newValue = currentValue * (1 - value / 100);
					break;
			}
			
			buffTarget.SetPropertyValue(property, newValue);
		}
	}
	
	private void ApplyCustomEffect(BuffInstance buff, BuffEffectInstance effectInstance)
	{
		// 处理自定义效果
		// 可以通过反射或预定义的效果处理器来实现
	}
	
	public Array<BuffInstance> GetTargetBuffs(Node target)
	{
		if (_targetBuffs.TryGetValue(target, out var buffs))
		{
			return buffs;
		}
		return new Array<BuffInstance>();
	}
	
	public float CalculatePropertyValue(Node target, string property)
	{
		return BuffCalculator.CalculateProperty(target, property);
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
}
