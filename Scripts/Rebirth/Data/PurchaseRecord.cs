using Godot;
using System;
using CodeRogue.Rebirth;

namespace CodeRogue.Rebirth.Data
{
    /// <summary>
    /// 购买记录
    /// </summary>
    [System.Serializable]
    public partial class PurchaseRecord : Resource
{
    [Export] public string PurchaseId { get; set; } = System.Guid.NewGuid().ToString();
    [Export] public string PlayerId { get; set; }
    [Export] public string ItemId { get; set; }
    [Export] public int PricePaid { get; set; }
    [Export] public string PurchaseTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public bool IsActive { get; set; } = true;
    [Export] public ShopItemType ItemType { get; set; }
    [Export] public RewardEffectType EffectType { get; set; }
    [Export] public string ItemName { get; set; }
    [Export] public bool IsRefunded { get; set; } = false;
    [Export] public string RefundTime { get; set; } = "";
    [Export] public string RefundReason { get; set; }
    
    /// <summary>
    /// 激活购买记录
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// 停用购买记录
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
    
    /// <summary>
    /// 执行退款
    /// </summary>
    public bool Refund(string reason = "")
    {
        if (IsRefunded) return false;
        
        IsRefunded = true;
        RefundTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        RefundReason = reason;
        IsActive = false;
        
        return true;
    }
    
    /// <summary>
    /// 检查是否可以退款
    /// </summary>
    public bool CanRefund()
    {
        if (IsRefunded) return false;
        
        // 购买后24小时内可以退款
        if (DateTime.TryParse(PurchaseTime, out var purchaseTime))
        {
            var timeSincePurchase = DateTime.Now - purchaseTime;
            return timeSincePurchase.TotalHours <= 24;
        }
        return false;
    }
    
    /// <summary>
    /// 获取购买时长
    /// </summary>
    public TimeSpan GetTimeSincePurchase()
    {
        if (DateTime.TryParse(PurchaseTime, out var purchaseTime))
        {
            return DateTime.Now - purchaseTime;
        }
        return TimeSpan.Zero;
    }
    
    /// <summary>
    /// 检查是否为永久效果
    /// </summary>
    public bool IsPermanentEffect()
    {
        return EffectType == RewardEffectType.Permanent;
    }
    
    /// <summary>
    /// 检查效果是否仍然有效
    /// </summary>
    public bool IsEffectValid()
    {
        if (!IsActive || IsRefunded) return false;
        
        return EffectType switch
        {
            RewardEffectType.Permanent => true,
            RewardEffectType.Immediate => false, // 立即效果已经使用
            RewardEffectType.NextGame => true,   // 下局游戏有效
            RewardEffectType.Temporary => GetTimeSincePurchase().TotalHours <= 24, // 24小时有效
            _ => false
        };
    }
}
}