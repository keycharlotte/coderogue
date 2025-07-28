using Godot;
using Godot.Collections;
using System.Linq;

/// <summary>
/// 怪物卡牌数据结构
/// 基于万智牌颜色体系的怪物召唤卡牌
/// </summary>
[GlobalClass]
public partial class MonsterCard : BaseCard
{
    [Export] public string MonsterName { get; set; } = "";
    [Export] public CardRarity MonsterRarity { get; set; }
    
    // 基础属性
    [Export] public int Health { get; set; } = 100;
    [Export] public int Attack { get; set; } = 50;
    
    // 怪物特性
    [Export] public MonsterRace Race { get; set; }
    [Export] public Array<MonsterSkillType> Skills { get; set; } = new Array<MonsterSkillType>();
    [Export] public Array<BondType> BondTypes { get; set; } = new Array<BondType>();
    

    
    // 技能效果数值
    [Export] public Array<float> SkillValues { get; set; } = new Array<float>();
    
    public MonsterCard()
    {
        CardType = CardType.Monster;
        Skills = new Array<MonsterSkillType>();
        BondTypes = new Array<BondType>();
        SkillValues = new Array<float>();
    }
    
    /// <summary>
    /// 获取召唤费用（使用基类的Cost属性）
    /// </summary>
    public int SummonCost
    {
        get => Cost;
        set => Cost = value;
    }
    
    /// <summary>
    /// 获取怪物稀有度（映射到基类的Rarity）
    /// </summary>
    public new CardRarity Rarity
    {
        get => MonsterRarity;
        set => MonsterRarity = value;
    }
    
    /// <summary>
    /// 检查是否包含指定颜色需求
    /// </summary>
    public bool HasColorRequirement(MagicColor color)
    {
        return ColorRequirements.Contains(color);
    }
    
    /// <summary>
    /// 获取颜色需求数量
    /// </summary>
    public int GetColorRequirementCount()
    {
        return ColorRequirements.Count;
    }
    
    /// <summary>
    /// 检查是否包含指定技能
    /// </summary>
    public bool HasSkill(MonsterSkillType skillType)
    {
        return Skills.Contains(skillType);
    }
    
    /// <summary>
    /// 获取指定技能的效果数值
    /// </summary>
    public float GetSkillValue(MonsterSkillType skillType)
    {
        int index = Skills.IndexOf(skillType);
        if (index >= 0 && index < SkillValues.Count)
        {
            return SkillValues[index];
        }
        return 0f;
    }
    
    /// <summary>
    /// 检查是否包含指定羁绊类型
    /// </summary>
    public bool HasBondType(BondType bondType)
    {
        return BondTypes.Contains(bondType);
    }
    
    /// <summary>
    /// 获取怪物的主要颜色（第一个颜色需求）
    /// </summary>
    public MagicColor GetPrimaryColor()
    {
        if (ColorRequirements.Count > 0)
            return ColorRequirements[0];
        return MagicColor.White; // 默认白色
    }
    
    /// <summary>
    /// 检查是否为单色怪物
    /// </summary>
    public bool IsSingleColor()
    {
        return ColorRequirements.Count == 1;
    }
    
    /// <summary>
    /// 检查是否为多色怪物
    /// </summary>
    public bool IsMultiColor()
    {
        return ColorRequirements.Count > 1;
    }
    
    /// <summary>
    /// 获取稀有度对应的颜色
    /// </summary>
    public Color GetRarityColor()
    {
        return MonsterRarity switch
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
    /// 获取怪物的战斗力评估
    /// </summary>
    public override float GetPowerRating()
    {
        float baseRating = (Health + Attack * 2) / 10f;
        float skillBonus = Skills.Count * 10f;
        float rarityMultiplier = MonsterRarity switch
        {
            CardRarity.Common => 1.0f,
            CardRarity.Uncommon => 1.1f,
            CardRarity.Rare => 1.2f,
            CardRarity.Epic => 1.5f,
            CardRarity.Legendary => 2.0f,
            _ => 1.0f
        };
        
        return (baseRating + skillBonus) * rarityMultiplier;
    }
    
    /// <summary>
    /// 检查是否可以升级
    /// </summary>
    public override bool CanUpgrade()
    {
        return Level < 10; // 怪物最多升级到10级
    }
    
    /// <summary>
    /// 升级怪物
    /// </summary>
    public override void Upgrade()
    {
        if (CanUpgrade())
        {
            Level++;
            // 升级时提升基础属性
            Health = (int)(Health * 1.1f);
            Attack = (int)(Attack * 1.1f);
        }
    }
    
    /// <summary>
    /// 创建怪物副本
    /// </summary>
    public override BaseCard CreateCopy()
    {
        var copy = new MonsterCard();
        copy.Id = Id;
        copy.CardName = CardName;
        copy.MonsterName = MonsterName;
        copy.Description = Description;
        copy.MonsterRarity = MonsterRarity;
        copy.ColorRequirements = new Array<MagicColor>(ColorRequirements);
        copy.Health = Health;
        copy.Attack = Attack;
        copy.Cost = Cost;
        copy.Level = Level;
        copy.Race = Race;
        copy.Skills = new Array<MonsterSkillType>(Skills);
        copy.BondTypes = new Array<BondType>(BondTypes);
        copy.IconPath = IconPath;
        copy.RarityColor = RarityColor;
        copy.SkillValues = new Array<float>(SkillValues);
        copy.Tags = new Array<string>(Tags);
        return copy;
    }
    
    /// <summary>
    /// 创建怪物副本（类型安全版本）
    /// </summary>
    public MonsterCard CreateMonsterCopy()
    {
        return (MonsterCard)CreateCopy();
    }
    
    public override string ToString()
    {
        return $"{MonsterName} ({Rarity}) - {Health}HP/{Attack}ATK - Cost:{SummonCost}";
    }
}