using Godot;
using Godot.Collections;
using System.Linq;

[GlobalClass]
public partial class RelicDropSystem : RefCounted
{
    // 稀有度权重配置
    private readonly Dictionary<RelicRarity, float> _baseRarityWeights = new Dictionary<RelicRarity, float>
    {
        { RelicRarity.Common, 60.0f },
        { RelicRarity.Rare, 30.0f },
        { RelicRarity.Epic, 8.0f },
        { RelicRarity.Legendary, 2.0f }
    };
    
    // 层数对稀有度的影响
    private readonly Dictionary<RelicRarity, float> _levelMultipliers = new Dictionary<RelicRarity, float>
    {
        { RelicRarity.Common, 0.95f },    // 每层减少5%
        { RelicRarity.Rare, 1.02f },      // 每层增加2%
        { RelicRarity.Epic, 1.05f },      // 每层增加5%
        { RelicRarity.Legendary, 1.08f }  // 每层增加8%
    }
    ;
    
    /// <summary>
    /// 获取随机遗物ID
    /// </summary>
    public int GetRandomRelicId(RelicRarity? targetRarity, int currentLevel, Array<RelicInstance> ownedRelics)
    {
        var database = new RelicDatabase();
        database.LoadConfigs();
        
        var availableConfigs = database.GetAvailableConfigs(currentLevel, ownedRelics);
        if (availableConfigs.Count == 0)
            return -1;
        
        RelicConfig selectedConfig;
        
        if (targetRarity.HasValue)
        {
            // 指定稀有度
            var rarityConfigs = availableConfigs.Where(c => c.Rarity == targetRarity.Value).ToArray();
            if (rarityConfigs.Length == 0)
                return -1;
                
            selectedConfig = SelectByWeight(rarityConfigs, currentLevel);
        }
        else
        {
            // 随机稀有度
            var rarity = SelectRandomRarity(currentLevel);
            var rarityConfigs = availableConfigs.Where(c => c.Rarity == rarity).ToArray();
            
            if (rarityConfigs.Length == 0)
            {
                // 如果该稀有度没有可用遗物，从所有可用中选择
                selectedConfig = SelectByWeight(availableConfigs.ToArray(), currentLevel);
            }
            else
            {
                selectedConfig = SelectByWeight(rarityConfigs, currentLevel);
            }
        }
        
        return selectedConfig?.Id ?? -1;
    }
    
    /// <summary>
    /// 选择随机稀有度
    /// </summary>
    private RelicRarity SelectRandomRarity(int currentLevel)
    {
        var weights = new Dictionary<RelicRarity, float>();
        
        foreach (var kvp in _baseRarityWeights)
        {
            var rarity = kvp.Key;
            var baseWeight = kvp.Value;
            var multiplier = Mathf.Pow(_levelMultipliers[rarity], currentLevel - 1);
            weights[rarity] = baseWeight * multiplier;
        }
        
        return SelectWeightedRandom(weights);
    }
    
    /// <summary>
    /// 根据权重选择遗物
    /// </summary>
    private RelicConfig SelectByWeight(RelicConfig[] configs, int currentLevel)
    {
        if (configs.Length == 0)
            return null;
            
        if (configs.Length == 1)
            return configs[0];
        
        var weights = new float[configs.Length];
        for (int i = 0; i < configs.Length; i++)
        {
            weights[i] = CalculateAdjustedWeight(configs[i], currentLevel);
        }
        
        var totalWeight = weights.Sum();
        var random = GD.Randf() * totalWeight;
        
        float currentWeight = 0;
        for (int i = 0; i < configs.Length; i++)
        {
            currentWeight += weights[i];
            if (random <= currentWeight)
                return configs[i];
        }
        
        return configs[configs.Length - 1];
    }
    
    /// <summary>
    /// 计算调整后的权重
    /// </summary>
    private float CalculateAdjustedWeight(RelicConfig config, int currentLevel)
    {
        var baseWeight = config.DropWeight;
        
        // 根据层数调整权重
        var levelDiff = currentLevel - config.MinLevel;
        var levelBonus = 1.0f + (levelDiff * 0.1f); // 每超过最低层数10%加成
        
        return baseWeight * levelBonus;
    }
    
    /// <summary>
    /// 加权随机选择
    /// </summary>
    private T SelectWeightedRandom<T>(Dictionary<T, float> weights)
    {
        var totalWeight = weights.Values.Sum();
        var random = GD.Randf() * totalWeight;
        
        float currentWeight = 0;
        foreach (var kvp in weights)
        {
            currentWeight += kvp.Value;
            if (random <= currentWeight)
                return kvp.Key;
        }
        
        return weights.Keys.First();
    }
    
    /// <summary>
    /// 获取稀有度权重信息（用于调试）
    /// </summary>
    public Dictionary<RelicRarity, float> GetRarityWeights(int currentLevel)
    {
        var weights = new Dictionary<RelicRarity, float>();
        
        foreach (var kvp in _baseRarityWeights)
        {
            var rarity = kvp.Key;
            var baseWeight = kvp.Value;
            var multiplier = Mathf.Pow(_levelMultipliers[rarity], currentLevel - 1);
            weights[rarity] = baseWeight * multiplier;
        }
        
        return weights;
    }
}