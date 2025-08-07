using Godot;
using System;
using CodeRogue.Rebirth;

namespace CodeRogue.Rebirth.Data
{
    /// <summary>
    /// 新颖度记录
    /// </summary>
    [System.Serializable]
    public partial class NoveltyRecord : Resource
{
    [Export] public string RecordId { get; set; } = System.Guid.NewGuid().ToString();
    [Export] public string PlayerId { get; set; }
    [Export] public string CombinationHash { get; set; }
    [Export] public int UsageCount { get; set; }
    [Export] public string FirstUsed { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public string LastUsed { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public float NoveltyScore { get; set; }
    [Export] public NoveltyLevel NoveltyLevel { get; set; }
    [Export] public float BestMultiplier { get; set; } = 1.0f;
    [Export] public int TotalCurrencyEarned { get; set; }
    [Export] public CombinationRating BestRating { get; set; }
    
    /// <summary>
    /// 更新使用记录
    /// </summary>
    public void UpdateUsage(float newScore, float multiplier)
    {
        UsageCount++;
        LastUsed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        if (UsageCount == 1)
        {
            FirstUsed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        // 更新最佳记录
        if (multiplier > BestMultiplier)
        {
            BestMultiplier = multiplier;
            NoveltyScore = newScore;
        }
    }
    
    /// <summary>
    /// 计算当前新颖度等级
    /// </summary>
    public NoveltyLevel CalculateNoveltyLevel()
    {
        if (UsageCount == 0)
            return NoveltyLevel.Legendary;
            
        if (UsageCount == 1)
            return NoveltyLevel.High;
            
        if (UsageCount <= 3)
            return NoveltyLevel.Medium;
            
        if (UsageCount <= 7)
            return NoveltyLevel.Low;
            
        return NoveltyLevel.None;
    }
    
    /// <summary>
    /// 获取时间奖励系数
    /// </summary>
    public float GetTimeBonus()
    {
        if (UsageCount == 0) return 2.0f;
        
        if (DateTime.TryParse(LastUsed, out var lastUseTime))
        {
            var daysSinceLastUse = (DateTime.Now - lastUseTime).TotalDays;
            return Mathf.Min(2.0f, 1.0f + (float)(daysSinceLastUse / 30.0));
        }
        return 1.0f;
    }
    
    /// <summary>
    /// 计算衰减系数
    /// </summary>
    public float GetDecayFactor()
    {
        return Mathf.Max(0.1f, 1.0f - (UsageCount * 0.15f));
    }
    
    /// <summary>
    /// 获取新颖度描述
    /// </summary>
    public string GetNoveltyDescription()
    {
        return NoveltyLevel switch
        {
            NoveltyLevel.Legendary => "传奇组合！前所未见的搭配",
            NoveltyLevel.High => "高度新颖的组合",
            NoveltyLevel.Medium => "较为新颖的组合",
            NoveltyLevel.Low => "略有新意的组合",
            NoveltyLevel.None => "常见的组合",
            _ => "未知组合"
        };
    }
    
    /// <summary>
    /// 检查是否为稀有组合
    /// </summary>
    public bool IsRareCombination()
    {
        return NoveltyLevel >= NoveltyLevel.High;
    }
    
    /// <summary>
    /// 添加货币收益
    /// </summary>
    public void AddCurrencyEarned(int amount)
    {
        TotalCurrencyEarned += amount;
    }
}
}