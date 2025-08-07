using Godot;
using System;
using CodeRogue.Rebirth;

namespace CodeRogue.Rebirth.Data
{
    /// <summary>
    /// 投胎货币数据
    /// </summary>
    [System.Serializable]
    public partial class RebirthCurrency : Resource
{
    [Export] public string PlayerId { get; set; }
    [Export] public int TotalCurrency { get; set; }
    [Export] public int LifetimeEarned { get; set; }
    [Export] public int LifetimeSpent { get; set; }
    [Export] public string LastUpdated { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public CurrencyType CurrencyType { get; set; } = CurrencyType.RebirthCoin;
    [Export] public int SessionEarned { get; set; } // 本次会话获得
    [Export] public float NoveltyBonusTotal { get; set; } // 新颖度奖励总计
    
    /// <summary>
    /// 添加货币
    /// </summary>
    public void AddCurrency(int amount, bool isNoveltyBonus = false)
    {
        if (amount <= 0) return;
        
        TotalCurrency += amount;
        LifetimeEarned += amount;
        SessionEarned += amount;
        
        if (isNoveltyBonus)
        {
            NoveltyBonusTotal += amount;
        }
        
        LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// 消费货币
    /// </summary>
    public bool SpendCurrency(int amount)
    {
        if (amount <= 0 || TotalCurrency < amount)
            return false;
            
        TotalCurrency -= amount;
        LifetimeSpent += amount;
        LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        return true;
    }
    
    /// <summary>
    /// 检查是否有足够货币
    /// </summary>
    public bool CanAfford(int amount)
    {
        return TotalCurrency >= amount;
    }
    
    /// <summary>
    /// 检查是否有足够货币（别名方法）
    /// </summary>
    public bool HasEnoughCurrency(int amount)
    {
        return CanAfford(amount);
    }
    
    /// <summary>
    /// 获取净收益
    /// </summary>
    public int GetNetEarnings()
    {
        return LifetimeEarned - LifetimeSpent;
    }
    
    /// <summary>
    /// 获取消费比率
    /// </summary>
    public float GetSpendingRatio()
    {
        return LifetimeEarned > 0 ? (float)LifetimeSpent / LifetimeEarned : 0f;
    }
    
    /// <summary>
    /// 重置会话数据
    /// </summary>
    public void ResetSessionData()
    {
        SessionEarned = 0;
    }
}
}