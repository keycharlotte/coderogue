using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 卡组基类 - 统一怪物卡组和技能卡组的共同功能
/// </summary>
[GlobalClass]
public abstract partial class BaseDeck : Resource
{
    [Export] public string DeckName { get; set; } = "";
    [Export] public Array<BaseCard> Cards { get; set; } = new Array<BaseCard>();
    [Export] public int MaxDeckSize { get; set; } = 30;
    [Export] public int MinDeckSize { get; set; } = 15;
    
    // 运行时数据（不序列化）
    protected List<BaseCard> _drawPile;
    protected List<BaseCard> _discardPile;
    protected Random _random;
    
    // 统计缓存
    private Godot.Collections.Dictionary<string, Variant> _statsCache;
    private bool _statsCacheValid = false;
    
    public BaseDeck()
    {
        Cards = new Array<BaseCard>();
        _drawPile = new List<BaseCard>();
        _discardPile = new List<BaseCard>();
        _random = new Random();
        _statsCache = new Godot.Collections.Dictionary<string, Variant>();
    }
    
    /// <summary>
    /// 初始化卡组
    /// </summary>
    public virtual void Initialize()
    {
        _drawPile = new List<BaseCard>(Cards.ToList());
        _discardPile = new List<BaseCard>();
        _random = new Random();
        ShuffleDeck();
    }
    
    /// <summary>
    /// 抽取随机卡牌
    /// </summary>
    public virtual BaseCard DrawRandomCard()
    {
        if (_drawPile.Count == 0)
        {
            ReshuffleDiscardPile();
        }
        
        if (_drawPile.Count == 0) return null;
        
        var card = _drawPile[0];
        _drawPile.RemoveAt(0);
        return card;
    }
    
    /// <summary>
    /// 弃牌
    /// </summary>
    public virtual void DiscardCard(BaseCard card)
    {
        if (card != null)
        {
            _discardPile.Add(card);
        }
    }
    
    /// <summary>
    /// 洗牌
    /// </summary>
    protected virtual void ShuffleDeck()
    {
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = _random.Next(i + 1);
            var temp = _drawPile[i];
            _drawPile[i] = _drawPile[randomIndex];
            _drawPile[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// 重新洗牌弃牌堆
    /// </summary>
    protected virtual void ReshuffleDiscardPile()
    {
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        ShuffleDeck();
    }
    
    /// <summary>
    /// 添加卡牌
    /// </summary>
    public virtual bool AddCard(BaseCard card)
    {
        if (card == null || Cards.Count >= MaxDeckSize)
            return false;
            
        Cards.Add(card);
        _drawPile?.Add(card);
        InvalidateStatsCache();
        return true;
    }
    
    /// <summary>
    /// 移除卡牌
    /// </summary>
    public virtual bool RemoveCard(BaseCard card)
    {
        if (card == null)
            return false;
            
        bool removed = Cards.Remove(card);
        if (removed)
        {
            _drawPile?.Remove(card);
            _discardPile?.Remove(card);
            InvalidateStatsCache();
        }
        return removed;
    }
    
    /// <summary>
    /// 清空卡组
    /// </summary>
    public virtual void ClearDeck()
    {
        Cards.Clear();
        _drawPile?.Clear();
        _discardPile?.Clear();
        InvalidateStatsCache();
    }
    
    /// <summary>
    /// 检查卡组是否有效
    /// </summary>
    public virtual bool IsValidDeck()
    {
        return Cards.Count >= MinDeckSize && Cards.Count <= MaxDeckSize;
    }
    
    /// <summary>
    /// 获取卡组统计信息
    /// </summary>
    public virtual Godot.Collections.Dictionary<string, Variant> GetDeckStats()
    {
        if (_statsCacheValid)
            return _statsCache;
            
        _statsCache.Clear();
        
        // 基础统计
        _statsCache["total_cards"] = Cards.Count;
        _statsCache["average_cost"] = (float)(Cards.Count > 0 ? Cards.Average(c => c.Cost) : 0f);
        _statsCache["min_cost"] = Cards.Count > 0 ? Cards.Min(c => c.Cost) : 0;
        _statsCache["max_cost"] = Cards.Count > 0 ? Cards.Max(c => c.Cost) : 0;
        
        // // 稀有度分布
        // var rarityDistribution = new Godot.Collections.Dictionary<CardRarity, int>();
        // foreach (CardRarity rarity in Enum.GetValues<CardRarity>())
        // {
        //     rarityDistribution[rarity] = Cards.Count(c => c.Rarity == rarity);
        // }
        // _statsCache["rarity_distribution"] = rarityDistribution;
        
        // // 费用分布
        // var costDistribution = new Godot.Collections.Dictionary<int, int>();
        // foreach (var card in Cards)
        // {
        //     if (!costDistribution.ContainsKey(card.Cost))
        //         costDistribution[card.Cost] = 0;
        //     costDistribution[card.Cost]++;
        // }
        // _statsCache["cost_distribution"] = costDistribution;
        
        // // 颜色分布
        // var colorDistribution = new Godot.Collections.Dictionary<MagicColor, int>();
        // foreach (MagicColor color in Enum.GetValues<MagicColor>())
        // {
        //     colorDistribution[color] = Cards.Count(c => c.HasColorRequirement(color));
        // }
        // _statsCache["color_distribution"] = colorDistribution;
        
        // 计算特定统计
        CalculateSpecificStats(_statsCache);
        
        _statsCacheValid = true;
        return _statsCache;
    }
    
    /// <summary>
    /// 计算特定类型的统计信息（由子类实现）
    /// </summary>
    protected abstract void CalculateSpecificStats(Godot.Collections.Dictionary<string, Variant> stats);
    
    /// <summary>
    /// 无效化统计缓存
    /// </summary>
    protected void InvalidateStatsCache()
    {
        _statsCacheValid = false;
    }
    
    /// <summary>
    /// 获取费用曲线
    /// </summary>
    public virtual Godot.Collections.Dictionary<int, float> GetCostCurve()
    {
        var curve = new Godot.Collections.Dictionary<int, float>();
        if (Cards.Count == 0) return curve;
        
        var maxCost = Cards.Max(c => c.Cost);
        for (int i = 0; i <= maxCost; i++)
        {
            curve[i] = (float)Cards.Count(c => c.Cost == i) / Cards.Count;
        }
        
        return curve;
    }
    
    /// <summary>
    /// 获取颜色平衡度
    /// </summary>
    public virtual float GetColorBalance()
    {
        if (Cards.Count == 0) return 1f;
        
        var colorCounts = new Godot.Collections.Dictionary<MagicColor, int>();
        foreach (MagicColor color in Enum.GetValues<MagicColor>())
        {
            colorCounts[color] = Cards.Count(c => c.HasColorRequirement(color));
        }
        
        var nonZeroCounts = colorCounts.Values.Where(c => c > 0).ToList();
        if (nonZeroCounts.Count <= 1) return 1f;
        
        float average = (float)nonZeroCounts.Average();
        float variance = nonZeroCounts.Sum(c => (c - average) * (c - average)) / nonZeroCounts.Count;
        
        return 1f / (1f + variance / (average * average));
    }
    
    /// <summary>
    /// 按费用获取卡牌
    /// </summary>
    public virtual List<BaseCard> GetCardsByCost(int cost)
    {
        return Cards.Where(c => c.Cost == cost).ToList();
    }
    
    /// <summary>
    /// 按稀有度获取卡牌
    /// </summary>
    public virtual List<BaseCard> GetCardsByRarity(CardRarity rarity)
    {
        return Cards.Where(c => c.Rarity == rarity).ToList();
    }
    
    /// <summary>
    /// 按颜色获取卡牌
    /// </summary>
    public virtual List<BaseCard> GetCardsByColor(MagicColor color)
    {
        return Cards.Where(c => c.HasColorRequirement(color)).ToList();
    }
    
    /// <summary>
    /// 创建卡组副本
    /// </summary>
    public abstract BaseDeck CreateCopy();
    
    /// <summary>
    /// 获取卡组描述
    /// </summary>
    public virtual string GetDeckDescription()
    {
        var stats = GetDeckStats();
        return $"{DeckName} ({Cards.Count} cards)\nAvg Cost: {stats["average_cost"]:F1}";
    }
    
    public override string ToString()
    {
        return GetDeckDescription();
    }
}