using Godot;
using CodeRogue.Rebirth;

/// <summary>
/// 组合数据
/// </summary>
[System.Serializable]
public partial class CombinationData : Resource
{
    [Export] public string CombinationHash { get; set; }
    [Export] public string HeroId { get; set; }
    [Export] public Godot.Collections.Array<string> CardIds { get; set; } = new();
    [Export] public Godot.Collections.Array<string> RelicIds { get; set; } = new();
    [Export] public int GlobalUsageCount { get; set; }
    [Export] public float BaseNoveltyScore { get; set; } = 1.0f;
    [Export] public CombinationRating AverageRating { get; set; }
    [Export] public float SuccessRate { get; set; }
    [Export] public int TotalVictories { get; set; }
    [Export] public int TotalAttempts { get; set; }
    [Export] public float AverageFloorReached { get; set; }
    [Export] public float AverageCurrencyEarned { get; set; }
    
    /// <summary>
    /// 添加使用记录
    /// </summary>
    public void AddUsage(bool isVictory, int floorsReached, float currencyEarned)
    {
        GlobalUsageCount++;
        TotalAttempts++;
        
        if (isVictory)
        {
            TotalVictories++;
        }
        
        // 更新平均值
        UpdateAverageFloorReached(floorsReached);
        UpdateAverageCurrencyEarned(currencyEarned);
        UpdateSuccessRate();
    }
    
    /// <summary>
    /// 更新平均到达层数
    /// </summary>
    private void UpdateAverageFloorReached(int newFloor)
    {
        if (TotalAttempts == 1)
        {
            AverageFloorReached = newFloor;
        }
        else
        {
            AverageFloorReached = ((AverageFloorReached * (TotalAttempts - 1)) + newFloor) / TotalAttempts;
        }
    }
    
    /// <summary>
    /// 更新平均货币收益
    /// </summary>
    private void UpdateAverageCurrencyEarned(float newCurrency)
    {
        if (TotalAttempts == 1)
        {
            AverageCurrencyEarned = newCurrency;
        }
        else
        {
            AverageCurrencyEarned = ((AverageCurrencyEarned * (TotalAttempts - 1)) + newCurrency) / TotalAttempts;
        }
    }
    
    /// <summary>
    /// 更新成功率
    /// </summary>
    private void UpdateSuccessRate()
    {
        SuccessRate = TotalAttempts > 0 ? (float)TotalVictories / TotalAttempts : 0f;
    }
    
    /// <summary>
    /// 计算组合强度评分
    /// </summary>
    public float CalculateStrengthScore()
    {
        float successWeight = SuccessRate * 40f;
        float floorWeight = AverageFloorReached * 2f;
        float currencyWeight = AverageCurrencyEarned * 0.1f;
        float usageWeight = Mathf.Min(GlobalUsageCount * 0.5f, 10f);
        
        return successWeight + floorWeight + currencyWeight + usageWeight;
    }
    
    /// <summary>
    /// 获取组合稀有度
    /// </summary>
    public NoveltyLevel GetRarityLevel()
    {
        if (GlobalUsageCount == 0)
            return NoveltyLevel.Legendary;
            
        if (GlobalUsageCount <= 5)
            return NoveltyLevel.High;
            
        if (GlobalUsageCount <= 20)
            return NoveltyLevel.Medium;
            
        if (GlobalUsageCount <= 50)
            return NoveltyLevel.Low;
            
        return NoveltyLevel.None;
    }
    
    /// <summary>
    /// 检查是否为强力组合
    /// </summary>
    public bool IsStrongCombination()
    {
        return SuccessRate >= 0.7f && AverageFloorReached >= 15f;
    }
    
    /// <summary>
    /// 获取组合描述
    /// </summary>
    public string GetCombinationDescription()
    {
        var strength = CalculateStrengthScore();
        var rarity = GetRarityLevel();
        
        string strengthDesc = strength switch
        {
            >= 80f => "极强",
            >= 60f => "强力",
            >= 40f => "中等",
            >= 20f => "一般",
            _ => "较弱"
        };
        
        return $"{rarity}组合 - {strengthDesc} (胜率: {SuccessRate:P1})";
    }
    
    /// <summary>
    /// 获取卡牌数量
    /// </summary>
    public int GetCardCount()
    {
        return CardIds.Count;
    }
    
    /// <summary>
    /// 获取遗物数量
    /// </summary>
    public int GetRelicCount()
    {
        return RelicIds.Count;
    }
}