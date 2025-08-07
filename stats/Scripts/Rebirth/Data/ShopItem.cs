using Godot;
using CodeRogue.Rebirth;

/// <summary>
/// 商店商品配置
/// </summary>
[System.Serializable]
public partial class ShopItem : Resource
{
    [Export] public string ItemId { get; set; }
    [Export] public string ItemName { get; set; }
    
    /// <summary>Id属性别名</summary>
    public string Id => ItemId;
    
    /// <summary>Name属性别名</summary>
    public string Name => ItemName;
    [Export] public ShopItemType ItemType { get; set; }
    [Export] public int Price { get; set; }
    [Export] public string Description { get; set; }
    [Export] public string EffectData { get; set; }
    [Export] public bool IsPermanent { get; set; }
    [Export] public bool IsAvailable { get; set; } = true;
    [Export] public Texture2D Icon { get; set; }
    [Export] public int MaxPurchases { get; set; } = -1; // -1表示无限制
    [Export] public ShopItemRarity Rarity { get; set; } = ShopItemRarity.Common;
    [Export] public RewardEffectType EffectType { get; set; } = RewardEffectType.NextGame;
    [Export] public int RequiredLevel { get; set; } = 1;
    [Export] public float RecommendationScore { get; set; } = 0f;
    [Export] public int PurchaseCount { get; set; } = 0;
    [Export] public bool IsOnSale { get; set; } = false;
    [Export] public float SaleDiscount { get; set; } = 0f; // 折扣百分比
    
    /// <summary>
    /// 获取实际价格（考虑折扣）
    /// </summary>
    public int GetActualPrice()
    {
        if (IsOnSale && SaleDiscount > 0f)
        {
            return Mathf.RoundToInt(Price * (1f - SaleDiscount));
        }
        return Price;
    }
    
    /// <summary>
    /// 检查是否可以购买
    /// </summary>
    public PurchaseStatus GetPurchaseStatus(int playerCurrency, int playerLevel)
    {
        if (!IsAvailable)
            return PurchaseStatus.Locked;
            
        if (playerLevel < RequiredLevel)
            return PurchaseStatus.Locked;
            
        if (MaxPurchases > 0 && PurchaseCount >= MaxPurchases)
            return PurchaseStatus.OutOfStock;
            
        if (playerCurrency < GetActualPrice())
            return PurchaseStatus.InsufficientFunds;
            
        if (IsPermanent && PurchaseCount > 0)
            return PurchaseStatus.Purchased;
            
        return PurchaseStatus.Available;
    }
    
    /// <summary>
    /// 执行购买
    /// </summary>
    public bool Purchase()
    {
        if (MaxPurchases > 0 && PurchaseCount >= MaxPurchases)
            return false;
            
        PurchaseCount++;
        
        // 永久物品购买后变为不可用
        if (IsPermanent && MaxPurchases == 1)
        {
            IsAvailable = false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取稀有度颜色
    /// </summary>
    public Color GetRarityColor()
    {
        return Rarity switch
        {
            ShopItemRarity.Common => Colors.White,
            ShopItemRarity.Uncommon => Colors.Green,
            ShopItemRarity.Rare => Colors.Blue,
            ShopItemRarity.Epic => Colors.Purple,
            ShopItemRarity.Legendary => Colors.Orange,
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// 获取剩余购买次数
    /// </summary>
    public int GetRemainingPurchases()
    {
        if (MaxPurchases < 0) return -1; // 无限制
        return Mathf.Max(0, MaxPurchases - PurchaseCount);
    }
}