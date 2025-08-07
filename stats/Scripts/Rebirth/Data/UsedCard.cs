using Godot;

/// <summary>
/// 使用的卡牌记录
/// </summary>
[System.Serializable]
public partial class UsedCard : Resource
{
    [Export] public string CardId { get; set; }
    [Export] public int UsageCount { get; set; }
    [Export] public float EffectivenessScore { get; set; }
    [Export] public int DamageDealt { get; set; }
    [Export] public int TimesPlayed { get; set; }
    [Export] public float AverageImpact { get; set; }
    
    /// <summary>
    /// 计算卡牌价值评分
    /// </summary>
    public float CalculateValueScore()
    {
        if (TimesPlayed <= 0) return 0f;
        
        float damagePerUse = DamageDealt / (float)TimesPlayed;
        float usageRate = UsageCount / (float)TimesPlayed;
        
        return (damagePerUse * 0.6f + EffectivenessScore * 0.3f + usageRate * 0.1f);
    }
    
    /// <summary>
    /// 获取卡牌使用效率
    /// </summary>
    public float GetUsageEfficiency()
    {
        return TimesPlayed > 0 ? UsageCount / (float)TimesPlayed : 0f;
    }
}