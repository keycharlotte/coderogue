using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 统一卡组类 - 包含所有类型的卡牌（怪物卡、技能卡等）
/// 替代原有的分离式卡组设计
/// </summary>
[GlobalClass]
public partial class UnifiedDeck : BaseDeck
{
    // 类型安全的卡牌访问属性
    public Array<MonsterCard> MonsterCards
    {
        get
        {
            var monsterCards = new Array<MonsterCard>();
            foreach (var card in Cards)
            {
                if (card is MonsterCard monsterCard)
                    monsterCards.Add(monsterCard);
            }
            return monsterCards;
        }
    }
    
    public Array<SkillCard> SkillCards
    {
        get
        {
            var skillCards = new Array<SkillCard>();
            foreach (var card in Cards)
            {
                if (card is SkillCard skillCard)
                    skillCards.Add(skillCard);
            }
            return skillCards;
        }
    }
    
    // 按卡牌类型分类的抽牌堆和弃牌堆
    public Array<MonsterCard> MonsterDrawPile
    {
        get
        {
            var monsterCards = new Array<MonsterCard>();
            foreach (var card in DrawPile)
            {
                if (card is MonsterCard monsterCard)
                    monsterCards.Add(monsterCard);
            }
            return monsterCards;
        }
    }
    
    public Array<SkillCard> SkillDrawPile
    {
        get
        {
            var skillCards = new Array<SkillCard>();
            foreach (var card in DrawPile)
            {
                if (card is SkillCard skillCard)
                    skillCards.Add(skillCard);
            }
            return skillCards;
        }
    }
    
    public Array<MonsterCard> MonsterDiscardPile
    {
        get
        {
            var monsterCards = new Array<MonsterCard>();
            foreach (var card in DiscardPile)
            {
                if (card is MonsterCard monsterCard)
                    monsterCards.Add(monsterCard);
            }
            return monsterCards;
        }
    }
    
    public Array<SkillCard> SkillDiscardPile
    {
        get
        {
            var skillCards = new Array<SkillCard>();
            foreach (var card in DiscardPile)
            {
                if (card is SkillCard skillCard)
                    skillCards.Add(skillCard);
            }
            return skillCards;
        }
    }
    
    [Export] public HeroInstance AssociatedSummoner { get; set; }
    
    // 公开抽牌堆和弃牌堆的访问
    public List<BaseCard> DrawPile => _drawPile;
    public List<BaseCard> DiscardPile => _discardPile;
    
    public UnifiedDeck()
    {
        MaxDeckSize = 50; // 增加卡组容量以容纳所有类型的卡牌
        MinDeckSize = 20;
    }
    
    /// <summary>
    /// 添加怪物卡牌
    /// </summary>
    public bool AddMonsterCard(MonsterCard card)
    {
        return AddCard(card);
    }
    
    /// <summary>
    /// 添加技能卡牌
    /// </summary>
    public bool AddSkillCard(SkillCard card)
    {
        return AddCard(card);
    }
    
    /// <summary>
    /// 移除怪物卡牌
    /// </summary>
    public bool RemoveMonsterCard(MonsterCard card)
    {
        return RemoveCard(card);
    }
    
    /// <summary>
    /// 移除技能卡牌
    /// </summary>
    public bool RemoveSkillCard(SkillCard card)
    {
        return RemoveCard(card);
    }
    
    /// <summary>
    /// 抽取指定类型的卡牌
    /// </summary>
    public T DrawCardOfType<T>() where T : BaseCard
    {
        var availableCards = DrawPile.Where(c => c is T).ToList();
        if (availableCards.Count == 0)
            return null;
            
        var randomCard = availableCards[_random.Next(availableCards.Count)];
        DrawPile.Remove(randomCard);
        return randomCard as T;
    }
    
    /// <summary>
    /// 抽取怪物卡
    /// </summary>
    public MonsterCard DrawMonsterCard()
    {
        return DrawCardOfType<MonsterCard>();
    }
    
    /// <summary>
    /// 抽取技能卡
    /// </summary>
    public SkillCard DrawSkillCard()
    {
        return DrawCardOfType<SkillCard>();
    }
    
    /// <summary>
    /// 弃置怪物卡
    /// </summary>
    public void DiscardMonsterCard(MonsterCard card)
    {
        DiscardCard(card);
    }
    
    /// <summary>
    /// 弃置技能卡
    /// </summary>
    public void DiscardSkillCard(SkillCard card)
    {
        DiscardCard(card);
    }
    
    /// <summary>
    /// 获取卡牌类型分布
    /// </summary>
    public System.Collections.Generic.Dictionary<Type, int> GetCardTypeDistribution()
    {
        var distribution = new System.Collections.Generic.Dictionary<Type, int>();
        
        foreach (var card in Cards)
        {
            var cardType = card.GetType();
            if (distribution.ContainsKey(cardType))
                distribution[cardType]++;
            else
                distribution[cardType] = 1;
        }
        
        return distribution;
    }
    
    /// <summary>
    /// 获取技能类型分布
    /// </summary>
    public System.Collections.Generic.Dictionary<SkillType, int> GetSkillTypeDistribution()
    {
        var distribution = new System.Collections.Generic.Dictionary<SkillType, int>();
        
        foreach (SkillType skillType in Enum.GetValues<SkillType>())
        {
            distribution[skillType] = SkillCards.Count(c => c.SkillType == skillType);
        }
        
        return distribution;
    }
    
    /// <summary>
    /// 获取怪物种族分布
    /// </summary>
    public System.Collections.Generic.Dictionary<MonsterRace, int> GetMonsterRaceDistribution()
    {
        var distribution = new System.Collections.Generic.Dictionary<MonsterRace, int>();
        
        foreach (MonsterRace race in Enum.GetValues<MonsterRace>())
        {
            distribution[race] = MonsterCards.Count(c => c.Race == race);
        }
        
        return distribution;
    }
    
    /// <summary>
    /// 获取怪物技能分布
    /// </summary>
    public System.Collections.Generic.Dictionary<MonsterSkillType, int> GetMonsterSkillDistribution()
    {
        var distribution = new System.Collections.Generic.Dictionary<MonsterSkillType, int>();
        
        foreach (MonsterSkillType skill in Enum.GetValues<MonsterSkillType>())
        {
            distribution[skill] = MonsterCards.Count(c => c.HasSkill(skill));
        }
        
        return distribution;
    }
    
    /// <summary>
    /// 计算技能卡组效率
    /// </summary>
    public float GetSkillChargeEfficiency()
    {
        if (SkillCards.Count == 0) return 1f;
        
        float totalEfficiency = 0f;
        foreach (var card in SkillCards)
        {
            // 低消耗技能提供更高的充能效率
            float cardEfficiency = 1f + (10f - card.Cost) * 0.1f;
            totalEfficiency += cardEfficiency;
        }
        
        return totalEfficiency / SkillCards.Count;
    }
    
    /// <summary>
    /// 随机抽取一张技能卡
    /// </summary>
    public SkillCard DrawRandomSkill()
    {
        if (SkillCards.Count == 0) return null;
        
        var random = new Random();
        int index = random.Next(SkillCards.Count);
        var card = SkillCards[index];
        
        // 从卡组中移除抽取的卡牌
        RemoveCard(card);
        
        return card;
    }
    
    /// <summary>
    /// 获取技能类型比例
    /// </summary>
    public float GetSkillRatio(SkillType type)
    {
        if (SkillCards.Count == 0) return 0f;
        return (float)SkillCards.Count(c => c.SkillType == type) / SkillCards.Count;
    }
    
    /// <summary>
    /// 获取平均伤害倍率
    /// </summary>
    public float GetAverageDamageMultiplier()
    {
        var attackCards = SkillCards.Where(c => c.SkillType == SkillType.Attack);
        if (!attackCards.Any()) return 1f;
        
        return attackCards.Average(c => c.Effects
            .Where(e => e.Type == SkillEffectType.Damage)
            .Sum(e => e.Value));
    }
    
    /// <summary>
    /// 计算怪物卡组的羁绊潜力
    /// </summary>
    public float CalculateMonsterBondPotential()
    {
        if (MonsterCards.Count == 0) return 0f;
        
        float totalPotential = 0f;
        var raceGroups = MonsterCards.GroupBy(c => c.Race);
        
        foreach (var group in raceGroups)
        {
            int count = group.Count();
            if (count >= 2)
            {
                // 同种族卡牌数量越多，羁绊潜力越高
                totalPotential += count * (count - 1) * 0.5f;
            }
        }
        
        return totalPotential / MonsterCards.Count;
    }
    
    /// <summary>
    /// 计算卡组评分
    /// </summary>
    public float CalculateDeckScore()
    {
        if (Cards.Count == 0) return 0f;
        
        float score = 0f;
        
        // 基础分数：卡牌数量
        score += Cards.Count * 10f;
        
        // 稀有度加成
        foreach (var card in Cards)
        {
            score += (int)card.Rarity * 5f;
        }
        
        // 平衡性加成
        float colorBalance = GetColorBalance();
        score += colorBalance * 50f;
        
        // 技能效率加成
        if (SkillCards.Count > 0)
        {
            score += GetSkillChargeEfficiency() * 20f;
        }
        
        // 怪物羁绊加成
        if (MonsterCards.Count > 0)
        {
            score += CalculateMonsterBondPotential() * 30f;
        }
        
        return score;
    }
    
    /// <summary>
    /// 计算特定统计信息
    /// </summary>
    protected override void CalculateSpecificStats(Godot.Collections.Dictionary<string, Variant> stats)
    {
        // 卡牌类型分布
        stats["monster_count"] = MonsterCards.Count;
        stats["skill_count"] = SkillCards.Count;
        
        // 技能类型分布
        var skillTypeDistribution = new Godot.Collections.Dictionary<SkillType, int>();
        foreach (SkillType skillType in Enum.GetValues<SkillType>())
        {
            skillTypeDistribution[skillType] = SkillCards.Count(c => c.SkillType == skillType);
        }
        stats["skill_type_distribution"] = skillTypeDistribution;
        
        // 怪物种族分布
        var raceDistribution = new Godot.Collections.Dictionary<MonsterRace, int>();
        foreach (MonsterRace race in Enum.GetValues<MonsterRace>())
        {
            raceDistribution[race] = MonsterCards.Count(c => c.Race == race);
        }
        stats["race_distribution"] = raceDistribution;
        
        // 怪物技能分布
        var monsterSkillDistribution = new Godot.Collections.Dictionary<MonsterSkillType, int>();
        foreach (MonsterSkillType skill in Enum.GetValues<MonsterSkillType>())
        {
            monsterSkillDistribution[skill] = MonsterCards.Count(c => c.HasSkill(skill));
        }
        stats["monster_skill_distribution"] = monsterSkillDistribution;
        
        // 效率指标
        if (SkillCards.Count > 0)
        {
            stats["skill_charge_efficiency"] = GetSkillChargeEfficiency();
            stats["average_damage_multiplier"] = GetAverageDamageMultiplier();
        }
        
        if (MonsterCards.Count > 0)
        {
            stats["monster_bond_potential"] = CalculateMonsterBondPotential();
        }
        
        // 卡组评分
        stats["deck_score"] = CalculateDeckScore();
    }
    
    /// <summary>
    /// 创建卡组副本
    /// </summary>
    public override BaseDeck CreateCopy()
    {
        var copy = new UnifiedDeck();
        copy.DeckName = DeckName + " (Copy)";
        copy.MaxDeckSize = MaxDeckSize;
        copy.MinDeckSize = MinDeckSize;
        copy.AssociatedSummoner = AssociatedSummoner;
        
        foreach (var card in Cards)
        {
            copy.AddCard(card.CreateCopy() as BaseCard);
        }
        
        return copy;
    }
    
    /// <summary>
    /// 创建统一卡组副本（类型安全）
    /// </summary>
    public UnifiedDeck CreateUnifiedDeckCopy()
    {
        return CreateCopy() as UnifiedDeck;
    }
    
    /// <summary>
    /// 检查卡组是否有效
    /// </summary>
    public override bool IsValidDeck()
    {
        // 基础验证
        if (!base.IsValidDeck())
            return false;
            
        // 确保至少有一些基础卡牌
        if (MonsterCards.Count == 0 && SkillCards.Count == 0)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// 获取可负担的卡牌（费用不超过指定点数的怪物卡）
    /// </summary>
    public List<MonsterCard> GetAffordableCards(int availablePoints)
    {
        return MonsterCards.Where(card => card.Cost <= availablePoints).ToList();
    }

    /// <summary>
    /// 获取卡组描述
    /// </summary>
    public override string GetDeckDescription()
    {
        var stats = GetDeckStats();
        return $"{DeckName} ({Cards.Count} cards)\n" +
               $"Monsters: {MonsterCards.Count}, Skills: {SkillCards.Count}\n" +
               $"Avg Cost: {stats["average_cost"]:F1}";
    }
}