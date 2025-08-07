using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Rebirth;
using CodeRogue.Rebirth.Data;

/// <summary>
/// 死亡重来商店管理器
/// </summary>
public partial class RebirthShopManager : Node
{
    private List<ShopItem> _shopItems = new();
    private List<PurchaseRecord> _purchaseHistory = new();
    private RebirthCurrency _playerCurrency;
    private PlayerData _playerData;
    private NoveltyCalculator _noveltyCalculator;
    
    [Signal] public delegate void ShopItemPurchasedEventHandler(ShopItem item, PurchaseRecord record);
    [Signal] public delegate void ShopRefreshedEventHandler(Godot.Collections.Array items);
    [Signal] public delegate void CurrencyUpdatedEventHandler(RebirthCurrency currency);
    [Signal] public delegate void ItemUnlockedEventHandler(ShopItem item);
    
    public override void _Ready()
    {
        _noveltyCalculator = GetNode<NoveltyCalculator>("../NoveltyCalculator");
        InitializeShop();
    }
    
    /// <summary>
    /// 初始化商店
    /// </summary>
    private void InitializeShop()
    {
        LoadShopItems();
        RefreshShopAvailability();
    }
    
    /// <summary>
    /// 加载商店商品
    /// </summary>
    private void LoadShopItems()
    {
        _shopItems.Clear();
        
        // 永久升级类商品
        AddPermanentUpgrades();
        
        // 临时增益类商品
        AddTemporaryBoosts();
        
        // 解锁类商品
        AddUnlockItems();
        
        // 特殊商品
        AddSpecialItems();
        
        // 根据玩家等级和进度排序
        SortItemsByRecommendation();
    }
    
    /// <summary>
    /// 添加永久升级商品
    /// </summary>
    private void AddPermanentUpgrades()
    {
        // 基础属性提升
        _shopItems.Add(new ShopItem
        {
            ItemId = "health_boost_1",
            ItemName = "生命强化 I",
            ItemType = ShopItemType.PermanentUpgrade,
            Price = 50,
            Description = "永久增加10点最大生命值",
            EffectData = "{\"health_bonus\": 10}",
            IsPermanent = true,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/health_boost.png"),
            MaxPurchases = 5,
            Rarity = ShopItemRarity.Common,
            EffectType = RewardEffectType.Permanent,
            RequiredLevel = 1,
            RecommendationScore = 8.5f
        });
        
        _shopItems.Add(new ShopItem
        {
            ItemId = "damage_boost_1",
            ItemName = "攻击强化 I",
            ItemType = ShopItemType.PermanentUpgrade,
            Price = 75,
            Description = "永久增加5点攻击力",
            EffectData = "{\"damage_bonus\": 5}",
            IsPermanent = true,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/damage_boost.png"),
            MaxPurchases = 5,
            Rarity = ShopItemRarity.Common,
            EffectType = RewardEffectType.Permanent,
            RequiredLevel = 1,
            RecommendationScore = 9.0f
        });
        
        _shopItems.Add(new ShopItem
        {
            ItemId = "currency_multiplier_1",
            ItemName = "财富增幅 I",
            ItemType = ShopItemType.PermanentUpgrade,
            Price = 100,
            Description = "永久增加10%投胎币获得量",
            EffectData = "{\"currency_multiplier\": 0.1}",
            IsPermanent = true,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/currency_boost.png"),
            MaxPurchases = 3,
            Rarity = ShopItemRarity.Uncommon,
            EffectType = RewardEffectType.Permanent,
            RequiredLevel = 3,
            RecommendationScore = 9.5f
        });
    }
    
    /// <summary>
    /// 添加临时增益商品
    /// </summary>
    private void AddTemporaryBoosts()
    {
        _shopItems.Add(new ShopItem
        {
            ItemId = "lucky_charm",
            ItemName = "幸运护符",
            ItemType = ShopItemType.TemporaryBoost,
            Price = 30,
            Description = "下次游戏开始时获得一个随机稀有遗物",
            EffectData = "{\"relic_rarity\": \"rare\", \"duration\": 1}",
            IsPermanent = false,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/lucky_charm.png"),
            MaxPurchases = 1,
            Rarity = ShopItemRarity.Uncommon,
            EffectType = RewardEffectType.Immediate,
            RequiredLevel = 2,
            RecommendationScore = 7.0f
        });
        
        _shopItems.Add(new ShopItem
        {
            ItemId = "exp_boost",
            ItemName = "经验加速器",
            ItemType = ShopItemType.TemporaryBoost,
            Price = 25,
            Description = "下次游戏获得双倍经验值",
            EffectData = "{\"exp_multiplier\": 2.0, \"duration\": 1}",
            IsPermanent = false,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/exp_boost.png"),
            MaxPurchases = 1,
            Rarity = ShopItemRarity.Common,
            EffectType = RewardEffectType.Temporary,
            RequiredLevel = 1,
            RecommendationScore = 6.5f
        });
    }
    
    /// <summary>
    /// 添加解锁类商品
    /// </summary>
    private void AddUnlockItems()
    {
        _shopItems.Add(new ShopItem
        {
            ItemId = "unlock_hero_warrior",
            ItemName = "解锁：战士",
            ItemType = ShopItemType.Unlock,
            Price = 200,
            Description = "解锁新英雄：战士。拥有强大的近战能力和防御技能。",
            EffectData = "{\"hero_id\": \"warrior\"}",
            IsPermanent = true,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/hero_warrior.png"),
            MaxPurchases = 1,
            Rarity = ShopItemRarity.Rare,
            EffectType = RewardEffectType.Permanent,
            RequiredLevel = 5,
            RecommendationScore = 10.0f
        });
        
        _shopItems.Add(new ShopItem
        {
            ItemId = "unlock_card_pack_fire",
            ItemName = "解锁：火焰卡包",
            ItemType = ShopItemType.Unlock,
            Price = 150,
            Description = "解锁火焰系列卡牌，包含强力的AOE攻击卡牌。",
            EffectData = "{\"card_pack\": \"fire\"}",
            IsPermanent = true,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/card_pack_fire.png"),
            MaxPurchases = 1,
            Rarity = ShopItemRarity.Rare,
            EffectType = RewardEffectType.Permanent,
            RequiredLevel = 4,
            RecommendationScore = 8.8f
        });
    }
    
    /// <summary>
    /// 添加特殊商品
    /// </summary>
    private void AddSpecialItems()
    {
        _shopItems.Add(new ShopItem
        {
            ItemId = "reroll_shop",
            ItemName = "商店重置",
            ItemType = ShopItemType.Special,
            Price = 20,
            Description = "重新生成商店商品列表",
            EffectData = "{\"action\": \"reroll_shop\"}",
            IsPermanent = false,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/reroll.png"),
            MaxPurchases = 999,
            Rarity = ShopItemRarity.Common,
            EffectType = RewardEffectType.Permanent,
            RequiredLevel = 1,
            RecommendationScore = 5.0f
        });
        
        _shopItems.Add(new ShopItem
        {
            ItemId = "mystery_box",
            ItemName = "神秘宝箱",
            ItemType = ShopItemType.Special,
            Price = 80,
            Description = "打开后获得随机奖励，可能是卡牌、遗物或货币",
            EffectData = "{\"action\": \"mystery_reward\"}",
            IsPermanent = false,
            IsAvailable = true,
            // Icon = GD.Load<Texture2D>("res://icons/mystery_box.png"),
            MaxPurchases = 3,
            Rarity = ShopItemRarity.Epic,
            EffectType = RewardEffectType.Permanent,
            RequiredLevel = 6,
            RecommendationScore = 7.5f
        });
    }
    
    /// <summary>
    /// 根据推荐度排序商品
    /// </summary>
    private void SortItemsByRecommendation()
    {
        _shopItems = _shopItems.OrderByDescending(item => item.RecommendationScore).ToList();
    }
    
    /// <summary>
    /// 刷新商店可用性
    /// </summary>
    public void RefreshShopAvailability()
    {
        foreach (var item in _shopItems)
        {
            UpdateItemAvailability(item);
        }
        
        EmitSignal(SignalName.ShopRefreshed, new Godot.Collections.Array(_shopItems.ToArray()));
    }
    
    /// <summary>
    /// 更新商品可用性
    /// </summary>
    private void UpdateItemAvailability(ShopItem item)
    {
        // 检查等级要求
        if (_playerData != null && _playerData.PlayerLevel < item.RequiredLevel)
        {
            item.IsAvailable = false;
            return;
        }
        
        // 检查购买次数限制
        if (item.PurchaseCount >= item.MaxPurchases)
        {
            item.IsAvailable = false;
            return;
        }
        
        // 检查解锁状态
        if (item.ItemType == ShopItemType.Unlock)
        {
            if (IsItemAlreadyUnlocked(item))
            {
                item.IsAvailable = false;
                return;
            }
        }
        
        item.IsAvailable = true;
    }
    
    /// <summary>
    /// 检查物品是否已解锁
    /// </summary>
    private bool IsItemAlreadyUnlocked(ShopItem item)
    {
        if (_playerData == null) return false;
        
        return item.EffectType switch
        {
            RewardEffectType.Permanent => CheckPermanentEffectPurchased(item.EffectData),
            RewardEffectType.Immediate => false, // Immediate effects can be purchased multiple times
            RewardEffectType.NextGame => false, // Next game effects can be purchased multiple times
            _ => false
        };
    }
    
    /// <summary>
    /// 检查永久效果是否已购买
    /// </summary>
    private bool CheckPermanentEffectPurchased(string effectData)
    {
        // 检查购买历史中是否已有相同的永久效果
        return _purchaseHistory.Any(record => 
            record.EffectType == RewardEffectType.Permanent && 
            record.IsActive && 
            record.ItemType == ShopItemType.Unlock);
    }
    
    /// <summary>
    /// 购买商品
    /// </summary>
    public bool PurchaseItem(string itemId)
    {
        var item = _shopItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null || !item.IsAvailable)
        {
            GD.PrintErr($"商品不可用: {itemId}");
            return false;
        }
        
        var actualPrice = item.GetActualPrice();
        if (!_playerCurrency.HasEnoughCurrency(actualPrice))
        {
            GD.PrintErr($"货币不足，需要: {actualPrice}, 拥有: {_playerCurrency.TotalCurrency}");
            return false;
        }
        
        // 执行购买
        var success = item.Purchase();
        if (!success)
        {
            GD.PrintErr($"购买失败: {itemId}");
            return false;
        }
        
        // 扣除货币
        _playerCurrency.SpendCurrency(actualPrice);
        
        // 创建购买记录
        var record = new PurchaseRecord
        {
            PurchaseId = Guid.NewGuid().ToString(),
            PlayerId = _playerData?.PlayerId ?? "unknown",
            ItemId = itemId,
            PricePaid = actualPrice,
            PurchaseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            IsActive = true,
            ItemType = item.ItemType,
            EffectType = item.EffectType,
            ItemName = item.Name
        };
        
        _purchaseHistory.Add(record);
        
        // 应用商品效果
        ApplyItemEffect(item, record);
        
        // 更新商店状态
        UpdateItemAvailability(item);
        
        // 发送信号
        EmitSignal(SignalName.ShopItemPurchased, item, record);
        EmitSignal(SignalName.CurrencyUpdated, _playerCurrency);
        
        GD.Print($"成功购买商品: {item.Name}, 花费: {actualPrice}");
        return true;
    }
    
    /// <summary>
    /// 应用商品效果
    /// </summary>
    private void ApplyItemEffect(ShopItem item, PurchaseRecord record)
    {
        switch (item.EffectType)
        {
            case RewardEffectType.Temporary:
                ApplyStatBoost(item.EffectData);
                GrantItem(item.EffectData);
                ApplyExperienceBoost(item.EffectData);
                break;
            case RewardEffectType.Immediate:
                ApplyCurrencyBoost(item.EffectData);
                UnlockHero(item.EffectData);
                break;
            case RewardEffectType.NextGame:
                UnlockCards(item.EffectData);
                break;
            case RewardEffectType.Permanent:
                HandleSpecialEffect(item.EffectData);
                break;
        }
    }
    
    /// <summary>
    /// 应用属性提升
    /// </summary>
    private void ApplyStatBoost(string effectData)
    {
        // 解析效果数据并应用到玩家数据
        // 这里需要与游戏的属性系统集成
        GD.Print($"应用属性提升: {effectData}");
    }
    
    /// <summary>
    /// 应用货币提升
    /// </summary>
    private void ApplyCurrencyBoost(string effectData)
    {
        // 解析并应用货币倍率提升
        GD.Print($"应用货币提升: {effectData}");
    }
    
    /// <summary>
    /// 解锁英雄
    /// </summary>
    private void UnlockHero(string effectData)
    {
        var heroId = GetHeroIdFromEffect(effectData);
        if (!string.IsNullOrEmpty(heroId) && _playerData != null)
        {
            _playerData.UnlockHero(heroId);
            EmitSignal(SignalName.ItemUnlocked, _shopItems.First(i => i.EffectData == effectData));
            GD.Print($"解锁英雄: {heroId}");
        }
    }
    
    /// <summary>
    /// 解锁卡牌
    /// </summary>
    private void UnlockCards(string effectData)
    {
        var cardPack = GetCardPackFromEffect(effectData);
        if (!string.IsNullOrEmpty(cardPack) && _playerData != null)
        {
            // 这里需要根据卡包解锁对应的卡牌
            GD.Print($"解锁卡包: {cardPack}");
        }
    }
    
    /// <summary>
    /// 解锁遗物
    /// </summary>
    private void UnlockRelic(string effectData)
    {
        var relicId = GetRelicIdFromEffect(effectData);
        if (!string.IsNullOrEmpty(relicId) && _playerData != null)
        {
            _playerData.UnlockRelic(relicId);
            GD.Print($"解锁遗物: {relicId}");
        }
    }
    
    /// <summary>
    /// 给予物品
    /// </summary>
    private void GrantItem(string effectData)
    {
        // 处理物品给予逻辑
        GD.Print($"给予物品: {effectData}");
    }
    
    /// <summary>
    /// 应用经验提升
    /// </summary>
    private void ApplyExperienceBoost(string effectData)
    {
        // 处理经验提升逻辑
        GD.Print($"应用经验提升: {effectData}");
    }
    
    /// <summary>
    /// 处理特殊效果
    /// </summary>
    private void HandleSpecialEffect(string effectData)
    {
        // 解析特殊效果
        if (effectData.Contains("reroll_shop"))
        {
            RerollShop();
        }
        else if (effectData.Contains("mystery_reward"))
        {
            GrantMysteryReward();
        }
    }
    
    /// <summary>
    /// 重置商店
    /// </summary>
    private void RerollShop()
    {
        // 重新生成部分商品
        LoadShopItems();
        RefreshShopAvailability();
        GD.Print("商店已重置");
    }
    
    /// <summary>
    /// 给予神秘奖励
    /// </summary>
    private void GrantMysteryReward()
    {
        var random = new Random();
        var rewardType = random.Next(0, 3);
        
        switch (rewardType)
        {
            case 0: // 货币奖励
                var currencyReward = random.Next(50, 200);
                _playerCurrency.AddCurrency(currencyReward);
                GD.Print($"神秘奖励：获得 {currencyReward} 投胎币");
                break;
            case 1: // 经验奖励
                var expReward = random.Next(100, 500);
                _playerData?.AddExperience(expReward);
                GD.Print($"神秘奖励：获得 {expReward} 经验值");
                break;
            case 2: // 临时增益
                GD.Print("神秘奖励：获得临时增益效果");
                break;
        }
    }
    
    /// <summary>
    /// 从效果数据中提取英雄ID
    /// </summary>
    private string GetHeroIdFromEffect(string effectData)
    {
        // 简单的JSON解析，实际项目中应使用JSON库
        if (effectData.Contains("hero_id"))
        {
            var start = effectData.IndexOf("hero_id\": \"") + 11;
            var end = effectData.IndexOf("\"", start);
            if (start > 10 && end > start)
            {
                return effectData.Substring(start, end - start);
            }
        }
        return string.Empty;
    }
    
    /// <summary>
    /// 从效果数据中提取卡包名称
    /// </summary>
    private string GetCardPackFromEffect(string effectData)
    {
        if (effectData.Contains("card_pack"))
        {
            var start = effectData.IndexOf("card_pack\": \"") + 13;
            var end = effectData.IndexOf("\"", start);
            if (start > 12 && end > start)
            {
                return effectData.Substring(start, end - start);
            }
        }
        return string.Empty;
    }
    
    /// <summary>
    /// 从效果数据中提取遗物ID
    /// </summary>
    private string GetRelicIdFromEffect(string effectData)
    {
        if (effectData.Contains("relic_id"))
        {
            var start = effectData.IndexOf("relic_id\": \"") + 12;
            var end = effectData.IndexOf("\"", start);
            if (start > 11 && end > start)
            {
                return effectData.Substring(start, end - start);
            }
        }
        return string.Empty;
    }
    
    /// <summary>
    /// 获取商店商品列表
    /// </summary>
    public List<ShopItem> GetShopItems(ShopPageType pageType = ShopPageType.All)
    {
        if (pageType == ShopPageType.All)
        {
            return _shopItems.Where(item => item.IsAvailable).ToList();
        }
        
        return pageType switch
        {
            ShopPageType.Upgrades => _shopItems.Where(item => item.ItemType == ShopItemType.PermanentUpgrade && item.IsAvailable).ToList(),
            ShopPageType.Unlocks => _shopItems.Where(item => item.ItemType == ShopItemType.Unlock && item.IsAvailable).ToList(),
            ShopPageType.Boosts => _shopItems.Where(item => item.ItemType == ShopItemType.TemporaryBoost && item.IsAvailable).ToList(),
            ShopPageType.Special => _shopItems.Where(item => item.ItemType == ShopItemType.Special && item.IsAvailable).ToList(),
            _ => new List<ShopItem>()
        };
    }
    
    /// <summary>
    /// 获取购买历史
    /// </summary>
    public List<PurchaseRecord> GetPurchaseHistory()
    {
        return _purchaseHistory.OrderByDescending(r => r.PurchaseTime).ToList();
    }
    
    /// <summary>
    /// 设置玩家数据
    /// </summary>
    public void SetPlayerData(PlayerData playerData)
    {
        _playerData = playerData;
        RefreshShopAvailability();
    }
    
    /// <summary>
    /// 设置玩家货币
    /// </summary>
    public void SetPlayerCurrency(RebirthCurrency currency)
    {
        _playerCurrency = currency;
    }
    
    /// <summary>
    /// 获取推荐商品
    /// </summary>
    public List<ShopItem> GetRecommendedItems(int count = 3)
    {
        return _shopItems
            .Where(item => item.IsAvailable)
            .OrderByDescending(item => item.RecommendationScore)
            .Take(count)
            .ToList();
    }
}