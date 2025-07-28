using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 怪物管理器
/// 负责管理怪物卡创建的怪物的逻辑和怪物卡牌的管理
/// </summary>
[GlobalClass]
public partial class MonsterManager : Node
{
    [Signal] public delegate void CardCollectionUpdatedEventHandler();
    [Signal] public delegate void NewCardObtainedEventHandler(MonsterCard card);
    [Signal] public delegate void CardUpgradedEventHandler(MonsterCard card);
    
    // 卡牌集合
    [Export] public Array<MonsterCard> AllCards { get; private set; } = new Array<MonsterCard>();
    [Export] public Array<MonsterCard> OwnedCards { get; private set; } = new Array<MonsterCard>();
    [Export] public Godot.Collections.Dictionary<string, int> CardCounts { get; private set; } = new Godot.Collections.Dictionary<string, int>();
    
    // 卡牌获取配置
    [Export] public Godot.Collections.Dictionary<CardRarity, float> RarityWeights { get; set; } = new Godot.Collections.Dictionary<CardRarity, float>
    {
        { CardRarity.Common, 0.6f },
        { CardRarity.Uncommon, 0.25f },
        { CardRarity.Rare, 0.12f },
        { CardRarity.Epic, 0.025f },
        { CardRarity.Legendary, 0.005f }
    };
    
    // 颜色获取权重（基于召唤师主颜色）
    private Godot.Collections.Dictionary<MagicColor, float> _colorWeights = new Godot.Collections.Dictionary<MagicColor, float>();
    
    // 当前召唤师
    private SummonerHero _currentSummoner;
    
    public override void _Ready()
    {
        InitializeDefaultCards();
        InitializeColorWeights();
    }
    
    /// <summary>
    /// 初始化默认卡牌库
    /// </summary>
    private void InitializeDefaultCards()
    {
        // 创建一些基础怪物卡牌作为示例
        CreateBasicCards();
        
        GD.Print($"Initialized {AllCards.Count} monster cards");
    }
    
    /// <summary>
    /// 创建基础卡牌
    /// </summary>
    private void CreateBasicCards()
    {
        // 白色怪物
        AllCards.Add(CreateCard("holy_knight", "圣骑士", "忠诚的圣光战士", CardRarity.Common,
            new Array<MagicColor> { MagicColor.White, MagicColor.White },
            30, 25, 3, MonsterRace.Human,
            new List<MonsterSkillType> { MonsterSkillType.Heal, MonsterSkillType.Protect }));
            
        AllCards.Add(CreateCard("healing_angel", "治疗天使", "带来希望的天使", CardRarity.Rare,
            new Array<MagicColor> { MagicColor.White, MagicColor.White, MagicColor.White },
            25, 20, 4, MonsterRace.Angel,
            new List<MonsterSkillType> { MonsterSkillType.Heal, MonsterSkillType.Buff }));
        
        // 蓝色怪物
        AllCards.Add(CreateCard("frost_elemental", "冰霜元素", "寒冷的魔法生物", CardRarity.Common,
            new Array<MagicColor> { MagicColor.Blue, MagicColor.Blue },
            20, 30, 3, MonsterRace.Elemental,
            new List<MonsterSkillType> { MonsterSkillType.Freeze, MonsterSkillType.MagicAttack }));
            
        AllCards.Add(CreateCard("water_dragon", "水龙", "古老的水系巨龙", CardRarity.Epic,
            new Array<MagicColor> { MagicColor.Blue, MagicColor.Blue, MagicColor.Blue, MagicColor.Blue },
            50, 45, 6, MonsterRace.Dragon,
            new List<MonsterSkillType> { MonsterSkillType.AoEAttack, MonsterSkillType.MagicAttack }));
        
        // 黑色怪物
        AllCards.Add(CreateCard("shadow_assassin", "暗影刺客", "来自黑暗的杀手", CardRarity.Uncommon,
            new Array<MagicColor> { MagicColor.Black, MagicColor.Black },
            25, 35, 3, MonsterRace.Human,
            new List<MonsterSkillType> { MonsterSkillType.Stealth, MonsterSkillType.CriticalStrike }));
            
        AllCards.Add(CreateCard("bone_dragon", "骨龙", "不死的龙族", CardRarity.Legendary,
            new Array<MagicColor> { MagicColor.Black, MagicColor.Black, MagicColor.Black, MagicColor.Black, MagicColor.Black },
            60, 50, 7, MonsterRace.Undead,
            new List<MonsterSkillType> { MonsterSkillType.Regeneration, MonsterSkillType.Fear }));
        
        // 红色怪物
        AllCards.Add(CreateCard("fire_goblin", "火焰哥布林", "狂暴的小恶魔", CardRarity.Common,
            new Array<MagicColor> { MagicColor.Red },
            15, 20, 2, MonsterRace.Goblin,
            new List<MonsterSkillType> { MonsterSkillType.Burn, MonsterSkillType.Rush }));
            
        AllCards.Add(CreateCard("flame_demon", "烈焰恶魔", "地狱的烈火使者", CardRarity.Rare,
            new Array<MagicColor> { MagicColor.Red, MagicColor.Red, MagicColor.Red },
            40, 45, 5, MonsterRace.Demon,
            new List<MonsterSkillType> { MonsterSkillType.Burn, MonsterSkillType.AoEAttack }));
        
        // 绿色怪物
        AllCards.Add(CreateCard("forest_wolf", "森林狼", "野性的森林守护者", CardRarity.Common,
            new Array<MagicColor> { MagicColor.Green, MagicColor.Green },
            35, 30, 3, MonsterRace.Beast,
            new List<MonsterSkillType> { MonsterSkillType.Pack, MonsterSkillType.Regeneration }));
            
        AllCards.Add(CreateCard("nature_dragon", "自然巨龙", "生命之力的化身", CardRarity.Epic,
            new Array<MagicColor> { MagicColor.Green, MagicColor.Green, MagicColor.Green, MagicColor.Green },
            55, 40, 6, MonsterRace.Dragon,
            new List<MonsterSkillType> { MonsterSkillType.Regeneration, MonsterSkillType.Buff }));
        
        // 多色怪物
        AllCards.Add(CreateCard("elemental_guardian", "元素守护者", "掌控多种元素的守护者", CardRarity.Rare,
            new Array<MagicColor> { MagicColor.Blue, MagicColor.Blue, MagicColor.Green, MagicColor.Green },
            45, 35, 5, MonsterRace.Elemental,
            new List<MonsterSkillType> { MonsterSkillType.Protect, MonsterSkillType.MagicAttack }));
    }
    
    /// <summary>
    /// 创建卡牌的辅助方法
    /// </summary>
    private MonsterCard CreateCard(string id, string name, string description, CardRarity rarity,
        Array<MagicColor> colorReq, int health, int attack, int cost, MonsterRace race,
        List<MonsterSkillType> skills)
    {
        var card = new MonsterCard();
        card.Id = int.Parse(id.GetHashCode().ToString().Substring(0, 8));
        card.CardName = name;
        card.MonsterName = name;
        card.Description = description;
        card.Rarity = rarity;
        card.ColorRequirements = colorReq;
        card.Health = health;
        card.Attack = attack;
        card.SummonCost = cost;
        card.Race = race;
        card.Skills = new Array<MonsterSkillType>(skills);
        card.IconPath = $"res://icons/monsters/{id}.png";
        
        // 设置羁绊类型
        var uniqueColors = colorReq.Distinct().ToList();
        if (uniqueColors.Count == 2)
        {
            card.BondTypes = new Array<BondType> { GetDualColorBond(uniqueColors) };
        }
        else if (uniqueColors.Count >= 3)
        {
            card.BondTypes = new Array<BondType> { BondType.Multicolor };
        }
        else
        {
            card.BondTypes = new Array<BondType> { BondType.SameColor };
        }
        
        return card;
    }
    
    /// <summary>
    /// 获取双色羁绊类型
    /// </summary>
    private BondType GetDualColorBond(List<MagicColor> colors)
    {
        if (colors.Count != 2) return BondType.SameColor;
        
        var sortedColors = colors.OrderBy(c => (int)c).ToList();
        
        return (sortedColors[0], sortedColors[1]) switch
        {
            (MagicColor.White, MagicColor.Blue) => BondType.HolyWarrior,
            (MagicColor.Blue, MagicColor.Black) => BondType.Druid,
            (MagicColor.Black, MagicColor.Red) => BondType.HolyWarrior,
            (MagicColor.Red, MagicColor.Green) => BondType.Druid,
            (MagicColor.Green, MagicColor.White) => BondType.HolyWarrior,
            (MagicColor.White, MagicColor.Black) => BondType.Druid,
            (MagicColor.Blue, MagicColor.Red) => BondType.HolyWarrior,
            (MagicColor.Black, MagicColor.Green) => BondType.Druid,
            (MagicColor.Red, MagicColor.White) => BondType.HolyWarrior,
            (MagicColor.Green, MagicColor.Blue) => BondType.Druid,
            _ => BondType.SameColor
        };
    }
    
    /// <summary>
    /// 初始化颜色权重
    /// </summary>
    private void InitializeColorWeights()
    {
        foreach (MagicColor color in System.Enum.GetValues<MagicColor>())
        {
            _colorWeights[color] = 0.2f; // 默认每种颜色20%权重
        }
    }
    
    /// <summary>
    /// 设置当前召唤师
    /// </summary>
    public void SetCurrentSummoner(SummonerHero summoner)
    {
        _currentSummoner = summoner;
        UpdateColorWeights();
    }
    
    /// <summary>
    /// 更新颜色权重（基于召唤师偏好）
    /// </summary>
    private void UpdateColorWeights()
    {
        if (_currentSummoner == null) return;
        
        // 重置权重
        foreach (var color in _colorWeights.Keys.ToList())
        {
            _colorWeights[color] = 0.1f;
        }
        
        // 提高召唤师主颜色的权重
        _colorWeights[_currentSummoner.PrimaryColor] = 0.4f;
        
        // 提高召唤师颜色槽位相关颜色的权重
        foreach (var slotColor in _currentSummoner.ColorSlots)
        {
            if (slotColor != MagicColor.Colorless)
            {
                _colorWeights[slotColor] += 0.1f;
            }
        }
    }
    
    /// <summary>
    /// 获取随机怪物卡牌
    /// </summary>
    public MonsterCard GetRandomCard(CardRarity? targetRarity = null)
    {
        var availableCards = AllCards.ToList();
        
        if (targetRarity.HasValue)
        {
            availableCards = availableCards.Where(c => c.Rarity == targetRarity.Value).ToList();
        }
        
        if (availableCards.Count == 0)
            return null;
            
        // 应用颜色权重过滤
        var weightedCards = ApplyColorWeights(availableCards);
        
        // 随机选择
        return SelectRandomWeighted(weightedCards);
    }
    
    /// <summary>
    /// 应用颜色权重
    /// </summary>
    private List<(MonsterCard card, float weight)> ApplyColorWeights(List<MonsterCard> cards)
    {
        var weightedCards = new List<(MonsterCard, float)>();
        
        foreach (var card in cards)
        {
            float weight = 1f;
            
            // 计算颜色权重
            foreach (var color in card.ColorRequirements)
            {
                if (_colorWeights.ContainsKey(color))
                {
                    weight *= _colorWeights[color];
                }
            }
            
            // 稀有度权重
            if (RarityWeights.ContainsKey(card.Rarity))
            {
                weight *= RarityWeights[card.Rarity];
            }
            
            weightedCards.Add((card, weight));
        }
        
        return weightedCards;
    }
    
    /// <summary>
    /// 加权随机选择
    /// </summary>
    private MonsterCard SelectRandomWeighted(List<(MonsterCard card, float weight)> weightedCards)
    {
        if (weightedCards.Count == 0)
            return null;
            
        float totalWeight = weightedCards.Sum(w => w.weight);
        float randomValue = GD.Randf() * totalWeight;
        
        float currentWeight = 0f;
        foreach (var (card, weight) in weightedCards)
        {
            currentWeight += weight;
            if (randomValue <= currentWeight)
            {
                return card;
            }
        }
        
        return weightedCards.Last().card;
    }
    
    /// <summary>
    /// 获取卡牌包（多张卡牌）
    /// </summary>
    public List<MonsterCard> GetCardPack(int cardCount = 5)
    {
        var pack = new List<MonsterCard>();
        
        for (int i = 0; i < cardCount; i++)
        {
            var rarity = GetRandomRarity();
            var card = GetRandomCard(rarity);
            if (card != null)
            {
                pack.Add(card);
            }
        }
        
        return pack;
    }
    
    /// <summary>
    /// 获取随机稀有度
    /// </summary>
    private CardRarity GetRandomRarity()
    {
        float randomValue = GD.Randf();
        float currentWeight = 0f;
        
        foreach (var (rarity, weight) in RarityWeights)
        {
            currentWeight += weight;
            if (randomValue <= currentWeight)
            {
                return rarity;
            }
        }
        
        return CardRarity.Common;
    }
    
    /// <summary>
    /// 添加卡牌到收藏
    /// </summary>
    public void AddCardToCollection(MonsterCard card)
    {
        if (card == null) return;
        
        OwnedCards.Add(card);
        
        // 更新卡牌数量
        string cardIdStr = card.Id.ToString();
        if (CardCounts.ContainsKey(cardIdStr))
        {
            CardCounts[cardIdStr]++;
        }
        else
        {
            CardCounts[cardIdStr] = 1;
        }
        
        EmitSignal(SignalName.NewCardObtained, card);
        EmitSignal(SignalName.CardCollectionUpdated);
        
        GD.Print($"Added {card.MonsterName} to collection");
    }
    
    /// <summary>
    /// 获取指定颜色的卡牌
    /// </summary>
    public List<MonsterCard> GetCardsByColor(MagicColor color)
    {
        return OwnedCards.Where(card => card.HasColorRequirement(color)).ToList();
    }
    
    /// <summary>
    /// 获取指定稀有度的卡牌
    /// </summary>
    public List<MonsterCard> GetCardsByRarity(CardRarity rarity)
    {
        return OwnedCards.Where(card => card.Rarity == rarity).ToList();
    }
    
    /// <summary>
    /// 获取指定种族的卡牌
    /// </summary>
    public List<MonsterCard> GetCardsByRace(MonsterRace race)
    {
        return OwnedCards.Where(card => card.Race == race).ToList();
    }
    
    /// <summary>
    /// 过滤可召唤的卡牌
    /// </summary>
    public List<MonsterCard> GetSummonableCards(SummonerHero summoner)
    {
        if (summoner == null)
            return new List<MonsterCard>();
            
        return OwnedCards.Where(card => summoner.CanSummonMonster(card)).ToList();
    }
    
    /// <summary>
    /// 获取卡牌数量
    /// </summary>
    public int GetCardCount(string cardId)
    {
        return CardCounts.ContainsKey(cardId) ? CardCounts[cardId] : 0;
    }
    
    /// <summary>
    /// 获取收藏统计
    /// </summary>
    public Godot.Collections.Dictionary<string, int> GetCollectionStats()
    {
        var stats = new Godot.Collections.Dictionary<string, int>
        {
            { "total_cards", OwnedCards.Count },
            { "unique_cards", CardCounts.Count }
        };
        
        // 按稀有度统计
        foreach (CardRarity rarity in System.Enum.GetValues<CardRarity>())
        {
            int count = OwnedCards.Count(card => card.Rarity == rarity);
            stats[$"rarity_{rarity}"] = count;
        }
        
        // 按颜色统计
        foreach (MagicColor color in System.Enum.GetValues<MagicColor>())
        {
            int count = OwnedCards.Count(card => card.HasColorRequirement(color));
            stats[$"color_{color}"] = count;
        }
        
        return stats;
    }
}