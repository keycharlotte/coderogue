using Godot;
using Godot.Collections;
using System;
using System.Linq;

/// <summary>
/// 卡牌基类 - 统一怪物卡和技能卡的共同属性
/// </summary>
[GlobalClass]
public abstract partial class BaseCard : Resource
{
    [Export] public int Id { get; set; }
    [Export] public string CardName { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public CardType CardType { get; set; }
    [Export] public CardRarity Rarity { get; set; }
    [Export] public int Cost { get; set; } = 1;
    [Export] public int Level { get; set; } = 1;
    
    // 视觉资源
    [Export] public string IconPath { get; set; } = "";
    [Export] public Color RarityColor { get; set; } = Colors.White;
    
    // 颜色需求（技能卡可能不需要，但保持统一）
    [Export] public Array<MagicColor> ColorRequirements { get; set; } = new Array<MagicColor>();
    
    // 标签系统（用于分类和搜索）
    [Export] public Array<string> Tags { get; set; } = new Array<string>();
    
    public BaseCard()
    {
        ColorRequirements = new Array<MagicColor>();
        Tags = new Array<string>();
    }
    
    /// <summary>
    /// 检查是否包含指定颜色需求
    /// </summary>
    public virtual bool HasColorRequirement(MagicColor color)
    {
        return ColorRequirements.Contains(color);
    }
    
    /// <summary>
    /// 获取指定颜色的需求数量
    /// </summary>
    public virtual int GetColorRequirementCount(MagicColor color)
    {
        return ColorRequirements.Count(c => c == color);
    }
    
    /// <summary>
    /// 检查是否包含指定标签
    /// </summary>
    public bool HasTag(string tag)
    {
        return Tags.Contains(tag);
    }
    
    /// <summary>
    /// 添加标签
    /// </summary>
    public void AddTag(string tag)
    {
        if (!HasTag(tag))
        {
            Tags.Add(tag);
        }
    }
    
    /// <summary>
    /// 移除标签
    /// </summary>
    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }
    
    /// <summary>
    /// 获取卡牌的稀有度对应颜色
    /// </summary>
    public virtual Color GetRarityColor()
    {
        return Rarity switch
        {
            CardRarity.Common => Colors.Gray,
            CardRarity.Uncommon => Colors.Green,
            CardRarity.Rare => Colors.Blue,
            CardRarity.Epic => Colors.Purple,
            CardRarity.Legendary => Colors.Orange,
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// 检查卡牌是否可以升级
    /// </summary>
    public abstract bool CanUpgrade();
    
    /// <summary>
    /// 升级卡牌
    /// </summary>
    public abstract void Upgrade();
    
    /// <summary>
    /// 获取卡牌的战斗力评估
    /// </summary>
    public abstract float GetPowerRating();
    
    /// <summary>
    /// 创建卡牌副本
    /// </summary>
    public abstract BaseCard CreateCopy();
    
    /// <summary>
    /// 获取卡牌详细信息
    /// </summary>
    public virtual string GetDetailedInfo()
    {
        return $"{CardName} ({Rarity})\n{Description}\nCost: {Cost}";
    }
}