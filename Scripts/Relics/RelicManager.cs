using Godot;
using Godot.Collections;
using System;
using System.Linq;

[GlobalClass]
public partial class RelicManager : Node
{
    private static RelicManager _instance;
    public static RelicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new RelicManager();
            }
            return _instance;
        }
    }

    // 信号
    [Signal] public delegate void RelicObtainedEventHandler(RelicInstance relic);
    [Signal] public delegate void RelicTriggeredEventHandler(RelicInstance relic, string triggerContext);
    [Signal] public delegate void SynergyActivatedEventHandler(RelicSynergy synergy);

    // 数据
    private Array<RelicInstance> _ownedRelics = new Array<RelicInstance>();
    private RelicDatabase _database;
    private RelicDropSystem _dropSystem;
    
    // 缓存
    private Dictionary<RelicTriggerType, Array<RelicInstance>> _triggerCache = new Dictionary<RelicTriggerType, Array<RelicInstance>>();
    private Array<RelicSynergy> _activeSynergies = new Array<RelicSynergy>();

    public override void _Ready()
    {
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }
        _instance = this;
        
        _database = new RelicDatabase();
        _dropSystem = new RelicDropSystem();
        
        // 初始化数据库
        _database.LoadConfigs();
        
        // 连接BuffManager信号（如果需要）
        if (BuffManager.Instance != null)
        {
            // 可以连接相关信号
        }
    }

    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    #region 遗物获取
    
    /// <summary>
    /// 获得遗物
    /// </summary>
    public bool ObtainRelic(int configId)
    {
        var config = _database.GetConfig(configId);
        if (config == null)
        {
            GD.PrintErr($"RelicManager: Config not found for ID {configId}");
            return false;
        }

        // 检查唯一性
        if (config.IsUnique && HasRelic(configId))
        {
            GD.Print($"RelicManager: Unique relic {config.Name} already owned");
            return false;
        }

        // 检查冲突
        foreach (int conflictId in config.ConflictRelics)
        {
            if (HasRelic(conflictId))
            {
                var conflictConfig = _database.GetConfig(conflictId);
                GD.Print($"RelicManager: Relic {config.Name} conflicts with {conflictConfig?.Name}");
                return false;
            }
        }

        // 创建实例
        var instance = new RelicInstance
        {
            ConfigId = configId,
            Config = config
        };

        _ownedRelics.Add(instance);
        RefreshTriggerCache();
        CheckSynergies();
        
        // 应用被动效果
        if (config.TriggerType == RelicTriggerType.Passive)
        {
            ApplyRelicEffects(instance, "passive");
        }

        EmitSignal(SignalName.RelicObtained, instance);
        GD.Print($"RelicManager: Obtained relic {config.Name}");
        return true;
    }

    /// <summary>
    /// 随机获得遗物
    /// </summary>
    public RelicInstance ObtainRandomRelic(RelicRarity? targetRarity = null, int currentLevel = 1)
    {
        var configId = _dropSystem.GetRandomRelicId(targetRarity, currentLevel, _ownedRelics);
        if (configId != -1 && ObtainRelic(configId))
        {
            return _ownedRelics.LastOrDefault(r => r.ConfigId == configId);
        }
        return null;
    }

    #endregion

    #region 遗物触发

    /// <summary>
    /// 触发指定类型的遗物
    /// </summary>
    public void TriggerRelics(RelicTriggerType triggerType, Dictionary context = null)
    {
        if (!_triggerCache.ContainsKey(triggerType))
            return;

        var relicsToTrigger = _triggerCache[triggerType];
        foreach (var relic in relicsToTrigger)
        {
            if (!relic.IsActive || relic.IsOnCooldown)
                continue;

            // 检查触发概率
            if (relic.Config.TriggerChance < 1.0f)
            {
                var random = GD.Randf();
                if (random > relic.Config.TriggerChance)
                    continue;
            }

            // 触发效果
            ApplyRelicEffects(relic, context?.ToString() ?? triggerType.ToString());
            
            // 更新触发数据
            relic.TriggerCount++;
            relic.LastTriggerTime = Time.GetUnixTimeFromSystem();
            
            EmitSignal(SignalName.RelicTriggered, relic, triggerType.ToString());
        }
    }

    /// <summary>
    /// 应用遗物效果
    /// </summary>
    private void ApplyRelicEffects(RelicInstance relic, string context)
    {
        foreach (var effect in relic.Config.Effects)
        {
            switch (effect.EffectType)
            {
                case RelicEffectType.StatModifier:
                    ApplyStatModifier(relic, effect, context);
                    break;
                case RelicEffectType.SkillEnhancement:
                    ApplySkillEnhancement(relic, effect, context);
                    break;
                case RelicEffectType.ChargeBoost:
                    ApplyChargeBoost(relic, effect, context);
                    break;
                case RelicEffectType.BuffGeneration:
                    ApplyBuffGeneration(relic, effect, context);
                    break;
                case RelicEffectType.SpecialAbility:
                    ApplySpecialAbility(relic, effect, context);
                    break;
                case RelicEffectType.RuleModification:
                    ApplyRuleModification(relic, effect, context);
                    break;
            }
        }
    }

    #endregion

    #region 效果应用

    private void ApplyStatModifier(RelicInstance relic, RelicEffectData effect, string context)
    {
        // // 创建属性修改Buff
        // var buffConfig = new BuffConfig
        // {
        //     Id = $"relic_{relic.Config.Id}_{effect.TargetProperty}",
        //     Name = $"{relic.Config.Name} - {effect.TargetProperty}",
        //     Description = $"来自遗物 {relic.Config.Name} 的属性修改",
        //     Duration = effect.Duration,
        //     StackingRule = BuffStackingRule.Replace,
        //     Effects = new Array<BuffEffect>
        //     {
        //         new BuffEffect
        //         {
        //             Type = BuffEffectType.PropertyModification,
        //             TargetProperty = effect.TargetProperty,
        //             Value = effect.Value,
        //             IsPercentage = effect.IsPercentage
        //         }
        //     }
        // };

        // var buffInstance = new BuffInstance(buffConfig, null);
        // BuffManager.Instance?.ApplyBuff(GetTree().CurrentScene, buffInstance);
        // relic.GeneratedBuffs.Add(buffInstance);
    }

    private void ApplySkillEnhancement(RelicInstance relic, RelicEffectData effect, string context)
    {
        // 技能增强逻辑
        GD.Print($"RelicManager: Applied skill enhancement from {relic.Config.Name}");
    }

    private void ApplyChargeBoost(RelicInstance relic, RelicEffectData effect, string context)
    {
        // 充能提升逻辑
        if (TypingCombatSystem.Instance != null)
        {
            // 可以调用TypingCombatSystem的方法来增加充能
            GD.Print($"RelicManager: Applied charge boost from {relic.Config.Name}");
        }
    }

    private void ApplyBuffGeneration(RelicInstance relic, RelicEffectData effect, string context)
    {
        // 生成特定Buff
        if (effect.Parameters.ContainsKey("buffId"))
        {
            var buffId = effect.Parameters["buffId"].AsString();
            // 从BuffDatabase获取配置并应用
            GD.Print($"RelicManager: Generated buff {buffId} from {relic.Config.Name}");
        }
    }

    private void ApplySpecialAbility(RelicInstance relic, RelicEffectData effect, string context)
    {
        // 特殊能力逻辑
        GD.Print($"RelicManager: Applied special ability from {relic.Config.Name}");
    }

    private void ApplyRuleModification(RelicInstance relic, RelicEffectData effect, string context)
    {
        // 规则修改逻辑
        GD.Print($"RelicManager: Applied rule modification from {relic.Config.Name}");
    }

    #endregion

    #region 协同效果

    /// <summary>
    /// 检查并激活协同效果
    /// </summary>
    private void CheckSynergies()
    {
        _activeSynergies.Clear();
        
        foreach (var relic in _ownedRelics)
        {
            foreach (var synergy in relic.Config.Synergies)
            {
                bool canActivate = true;
                foreach (int requiredId in synergy.RequiredRelics)
                {
                    if (!HasRelic(requiredId))
                    {
                        canActivate = false;
                        break;
                    }
                }
                
                if (canActivate && !_activeSynergies.Contains(synergy))
                {
                    _activeSynergies.Add(synergy);
                    ApplySynergyEffects(synergy);
                    EmitSignal(SignalName.SynergyActivated, synergy);
                }
            }
        }
    }

    private void ApplySynergyEffects(RelicSynergy synergy)
    {
        foreach (var effect in synergy.BonusEffects)
        {
            // 应用协同效果（类似于普通遗物效果）
            GD.Print($"RelicManager: Applied synergy effect {synergy.SynergyName}");
        }
    }

    #endregion

    #region 查询方法

    public bool HasRelic(int configId)
    {
        return _ownedRelics.Any(r => r.ConfigId == configId);
    }

    public RelicInstance GetRelic(int configId)
    {
        return _ownedRelics.FirstOrDefault(r => r.ConfigId == configId);
    }

    public Array<RelicInstance> GetRelicsByRarity(RelicRarity rarity)
    {
        var result = new Array<RelicInstance>();
        foreach (var relic in _ownedRelics)
        {
            if (relic.Config.Rarity == rarity)
                result.Add(relic);
        }
        return result;
    }

    public Array<RelicInstance> GetRelicsByCategory(RelicCategory category)
    {
        var result = new Array<RelicInstance>();
        foreach (var relic in _ownedRelics)
        {
            if (relic.Config.Category == category)
                result.Add(relic);
        }
        return result;
    }

    public Array<RelicInstance> GetOwnedRelics()
    {
        return new Array<RelicInstance>(_ownedRelics);
    }

    public Array<RelicSynergy> GetActiveSynergies()
    {
        return new Array<RelicSynergy>(_activeSynergies);
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 刷新触发缓存
    /// </summary>
    private void RefreshTriggerCache()
    {
        _triggerCache.Clear();
        
        foreach (var relic in _ownedRelics)
        {
            var triggerType = relic.Config.TriggerType;
            if (!_triggerCache.ContainsKey(triggerType))
            {
                _triggerCache[triggerType] = new Array<RelicInstance>();
            }
            _triggerCache[triggerType].Add(relic);
        }
    }

    #endregion
}