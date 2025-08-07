using Godot;

/// <summary>
/// 使用的遗物记录
/// </summary>
[System.Serializable]
public partial class UsedRelic : Resource
{
    [Export] public string RelicId { get; set; }
    [Export] public int ActivationCount { get; set; }
    [Export] public float ImpactScore { get; set; }
    [Export] public int BenefitProvided { get; set; }
    [Export] public float UpTime { get; set; } // 生效时间百分比
    [Export] public int SynergyBonus { get; set; } // 协同奖励
    
    /// <summary>
    /// 计算遗物价值评分
    /// </summary>
    public float CalculateValueScore()
    {
        float activationRate = ActivationCount > 0 ? 1.0f : 0.5f;
        float uptimeBonus = UpTime * 0.5f;
        float synergyBonus = SynergyBonus * 0.1f;
        
        return (ImpactScore * activationRate + uptimeBonus + synergyBonus);
    }
    
    /// <summary>
    /// 获取遗物效率评分
    /// </summary>
    public float GetEfficiencyRating()
    {
        if (ActivationCount <= 0) return ImpactScore * 0.3f;
        
        return (BenefitProvided / (float)ActivationCount) * ImpactScore;
    }
    
    /// <summary>
    /// 计算遗物影响力
    /// </summary>
    public float CalculateImpactScore()
    {
        return ImpactScore * (1.0f + UpTime) * (1.0f + SynergyBonus * 0.1f);
    }
}