using Godot;
using Godot.Collections;
using System.Linq;

[GlobalClass]
public partial class RelicDatabase : RefCounted
{
    private Dictionary<int, RelicConfig> _configs = new Dictionary<int, RelicConfig>();
    private Array<RelicConfig> _configList = new Array<RelicConfig>();
    
    public void LoadConfigs()
    {
        // 从资源文件加载配置
        LoadFromResources();
        
        // 也可以从JSON文件加载
        // LoadFromJson("res://ResourcesData/RelicConfigs.json");
        
        GD.Print($"RelicDatabase: Loaded {_configs.Count} relic configurations");
    }
    
    private void LoadFromResources()
    {
        // 扫描资源目录中的RelicConfig文件
        var dir = DirAccess.Open("res://ResourcesData/Relics/");
        if (dir != null)
        {
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            
            while (fileName != "")
            {
                if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
                {
                    var config = GD.Load<RelicConfig>($"res://ResourcesData/Relics/{fileName}");
                    if (config != null)
                    {
                        AddConfig(config);
                    }
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            // 如果目录不存在，创建一些示例配置
            CreateExampleConfigs();
        }
    }
    
    private void CreateExampleConfigs()
    {
        // 创建一些示例遗物配置
        var configs = new RelicConfig[]
        {
            // 普通遗物 - 锋利之刃
            new RelicConfig
            {
                Id = 1001,
                Name = "锋利之刃",
                Description = "攻击力提升15%",
                FlavorText = "经过千锤百炼的利刃，削铁如泥。",
                Rarity = RelicRarity.Common,
                Category = RelicCategory.Attack,
                IconPath = "res://icons/relics/sharp_blade.png",
                RarityColor = Colors.Gray,
                TriggerType = RelicTriggerType.Passive,
                Effects = new Array<RelicEffectData>
                {
                    new RelicEffectData
                    {
                        EffectType = RelicEffectType.StatModifier,
                        TargetProperty = "AttackPower",
                        Value = 15,
                        IsPercentage = true
                    }
                },
                DropWeight = 1.0f,
                MinLevel = 1
            },
            
            // 稀有遗物 - 充能核心
            new RelicConfig
            {
                Id = 2001,
                Name = "充能核心",
                Description = "每次完美输入额外获得1点充能",
                FlavorText = "蕴含着纯净能量的水晶核心。",
                Rarity = RelicRarity.Rare,
                Category = RelicCategory.Charge,
                IconPath = "res://icons/relics/charge_core.png",
                RarityColor = Colors.Blue,
                TriggerType = RelicTriggerType.OnChargeGained,
                TriggerChance = 1.0f,
                Effects = new Array<RelicEffectData>
                {
                    new RelicEffectData
                    {
                        EffectType = RelicEffectType.ChargeBoost,
                        Value = 1
                    }
                },
                DropWeight = 0.6f,
                MinLevel = 3
            },
            
            // 史诗遗物 - 连击大师
            new RelicConfig
            {
                Id = 3001,
                Name = "连击大师",
                Description = "连击数达到10时，下次攻击造成双倍伤害",
                FlavorText = "只有真正的大师才能掌握连击的奥义。",
                Rarity = RelicRarity.Epic,
                Category = RelicCategory.Special,
                IconPath = "res://icons/relics/combo_master.png",
                RarityColor = Colors.Purple,
                TriggerType = RelicTriggerType.OnComboReached,
                Effects = new Array<RelicEffectData>
                {
                    new RelicEffectData
                    {
                        EffectType = RelicEffectType.SpecialAbility,
                        TargetProperty = "NextAttackMultiplier",
                        Value = 2.0f,
                        Parameters = new Dictionary { ["comboThreshold"] = 10 }
                    }
                },
                DropWeight = 0.3f,
                MinLevel = 5
            },
            
            // 传说遗物 - 时间扭曲
            new RelicConfig
            {
                Id = 4001,
                Name = "时间扭曲",
                Description = "击败敌人时有20%概率重置所有技能冷却",
                FlavorText = "传说中能够操控时间流速的神秘遗物。",
                Rarity = RelicRarity.Legendary,
                Category = RelicCategory.Special,
                IconPath = "res://icons/relics/time_warp.png",
                RarityColor = Colors.Gold,
                TriggerType = RelicTriggerType.OnEnemyKilled,
                TriggerChance = 0.2f,
                Effects = new Array<RelicEffectData>
                {
                    new RelicEffectData
                    {
                        EffectType = RelicEffectType.SpecialAbility,
                        TargetProperty = "ResetSkillCooldowns",
                        Value = 1
                    }
                },
                DropWeight = 0.1f,
                MinLevel = 8,
                IsUnique = true
            }
        };
        
        foreach (var config in configs)
        {
            AddConfig(config);
        }
    }
    
    public void AddConfig(RelicConfig config)
    {
        if (config == null || _configs.ContainsKey(config.Id))
            return;
            
        _configs[config.Id] = config;
        _configList.Add(config);
    }
    
    public RelicConfig GetConfig(int id)
    {
        return _configs.TryGetValue(id, out var config) ? config : null;
    }
    
    public Array<RelicConfig> GetConfigsByRarity(RelicRarity rarity)
    {
        var result = new Array<RelicConfig>();
        foreach (var config in _configList)
        {
            if (config.Rarity == rarity)
                result.Add(config);
        }
        return result;
    }
    
    public Array<RelicConfig> GetConfigsByCategory(RelicCategory category)
    {
        var result = new Array<RelicConfig>();
        foreach (var config in _configList)
        {
            if (config.Category == category)
                result.Add(config);
        }
        return result;
    }
    
    public Array<RelicConfig> GetAvailableConfigs(int currentLevel, Array<RelicInstance> ownedRelics = null)
    {
        var result = new Array<RelicConfig>();
        var ownedIds = ownedRelics?.Select(r => r.ConfigId).ToHashSet() ?? new System.Collections.Generic.HashSet<int>();
        
        foreach (var config in _configList)
        {
            // 检查等级要求
            if (config.MinLevel > currentLevel)
                continue;
                
            // 检查唯一性
            if (config.IsUnique && ownedIds.Contains(config.Id))
                continue;
                
            // 检查冲突
            bool hasConflict = false;
            foreach (int conflictId in config.ConflictRelics)
            {
                if (ownedIds.Contains(conflictId))
                {
                    hasConflict = true;
                    break;
                }
            }
            
            if (!hasConflict)
                result.Add(config);
        }
        
        return result;
    }
    
    public Array<RelicConfig> GetAllConfigs()
    {
        return new Array<RelicConfig>(_configList);
    }
}