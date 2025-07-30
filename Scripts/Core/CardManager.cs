using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// 通用卡牌管理器
/// 负责所有类型卡牌的获取、存储、过滤和管理
/// 支持从JSON配置文件加载卡牌数据
/// </summary>
[GlobalClass]
public partial class CardManager : Node
{
    [Signal] public delegate void CardCollectionUpdatedEventHandler();
    [Signal] public delegate void NewCardObtainedEventHandler(BaseCard card);
    [Signal] public delegate void CardUpgradedEventHandler(BaseCard card);
    
    // 卡牌集合
    [Export] public Array<BaseCard> OwnedCards { get; private set; } = new Array<BaseCard>();
    [Export] public Godot.Collections.Dictionary<string, int> CardCounts { get; private set; } = new Godot.Collections.Dictionary<string, int>();
    
    // 配置数据
    [Export] public Godot.Collections.Dictionary<CardRarity, float> RarityWeights { get; private set; } = new Godot.Collections.Dictionary<CardRarity, float>();
    [Export] public Godot.Collections.Dictionary<MagicColor, float> ColorWeights { get; private set; } = new Godot.Collections.Dictionary<MagicColor, float>();
    
    // 卡牌数据库引用
    private CardDatabase _cardDatabase;
    
    // 当前召唤师
    private HeroInstance _currentSummoner;
    
    public override void _Ready()
    {
        // 获取CardDatabase实例
        _cardDatabase = GetNode<CardDatabase>("/root/CardDatabase");
        if (_cardDatabase == null)
        {
            GD.PrintErr("CardDatabase not found! Make sure it's added as an autoload.");
        }
        
        InitializeRarityWeights();
        InitializeColorWeights();
        GD.Print($"CardManager initialized with CardDatabase");
    }
    
    /// <summary>
    /// 获取羁绊类型
    /// </summary>
    private BondType GetBondType(Godot.Collections.Array<MagicColor> colorReqs)
    {
        var uniqueColors = colorReqs.Distinct().OrderBy(c => (int)c).ToList();
        
        if (uniqueColors.Count == 2)
        {
            return (uniqueColors[0], uniqueColors[1]) switch
            {
                (MagicColor.White, MagicColor.Blue) => BondType.OrderMage,
                (MagicColor.Blue, MagicColor.Black) => BondType.DarkMage,
                (MagicColor.Black, MagicColor.Red) => BondType.ChaosDemon,
                (MagicColor.Red, MagicColor.Green) => BondType.WildBerserker,
                (MagicColor.Green, MagicColor.White) => BondType.NatureGuard,
                (MagicColor.White, MagicColor.Black) => BondType.FallenKnight,
                (MagicColor.Blue, MagicColor.Red) => BondType.ElementalMage,
                (MagicColor.Black, MagicColor.Green) => BondType.CorruptNature,
                (MagicColor.Red, MagicColor.White) => BondType.HolyWarrior,
                (MagicColor.Green, MagicColor.Blue) => BondType.Druid,
                _ => BondType.SameColor
            };
        }
        else if (uniqueColors.Count >= 3)
        {
            return BondType.Multicolor;
        }
        else
        {
            return BondType.SameColor;
        }
    }
    
    /// <summary>
    /// 初始化稀有度权重（从CardDatabase获取）
    /// </summary>
    private void InitializeRarityWeights()
    {
        if (_cardDatabase != null)
        {
            var dbWeights = _cardDatabase.GetAllRarityWeights();
            foreach (var kvp in dbWeights)
            {
                RarityWeights[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            // 设置默认稀有度权重
            RarityWeights[CardRarity.Common] = 0.6f;
            RarityWeights[CardRarity.Uncommon] = 0.25f;
            RarityWeights[CardRarity.Rare] = 0.12f;
            RarityWeights[CardRarity.Epic] = 0.025f;
            RarityWeights[CardRarity.Legendary] = 0.005f;
        }
    }
    
    /// <summary>
    /// 初始化颜色权重（从CardDatabase获取）
    /// </summary>
    private void InitializeColorWeights()
    {
        if (_cardDatabase != null)
        {
            var dbWeights = _cardDatabase.GetAllColorWeights();
            foreach (var kvp in dbWeights)
            {
                ColorWeights[kvp.Key] = kvp.Value;
            }
        }
        
        if (ColorWeights.Count == 0)
        {
            foreach (MagicColor color in System.Enum.GetValues<MagicColor>())
            {
                ColorWeights[color] = 0.2f; // 默认每种颜色20%权重
            }
        }
    }
    
    /// <summary>
    /// 设置当前召唤师
    /// </summary>
    public void SetCurrentSummoner(HeroInstance summoner)
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
        foreach (var color in ColorWeights.Keys.ToList())
        {
            ColorWeights[color] = 0.1f;
        }
        
        // 提高召唤师主颜色的权重
        ColorWeights[_currentSummoner.Config.PrimaryColor] = 0.4f;
        
        // 为召唤师的颜色槽位增加权重
        foreach (var slotColor in _currentSummoner.ColorSlots)
        {
            if (slotColor != MagicColor.Colorless)
            {
                ColorWeights[slotColor] += 0.1f;
            }
        }
    }
    
    /// <summary>
    /// 获取随机卡牌
    /// </summary>
    public T GetRandomCard<T>(CardRarity? targetRarity = null) where T : BaseCard
    {
        if (_cardDatabase == null) return null;
        
        List<T> availableCards;
        
        if (typeof(T) == typeof(MonsterCard))
        {
            var monsterCards = targetRarity.HasValue 
                ? _cardDatabase.GetMonsterCardsByRarity(targetRarity.Value)
                : _cardDatabase.GetAllMonsterCards();
            availableCards = monsterCards.Cast<T>().ToList();
        }
        else if (typeof(T) == typeof(SkillCard))
        {
            var skillCards = targetRarity.HasValue 
                ? _cardDatabase.GetSkillCardsByRarity(targetRarity.Value)
                : _cardDatabase.GetAllSkillCards();
            availableCards = skillCards.Cast<T>().ToList();
        }
        else
        {
            // 混合类型，获取所有卡牌
            var allCards = new List<BaseCard>();
            allCards.AddRange(_cardDatabase.GetAllMonsterCards().Cast<BaseCard>());
            allCards.AddRange(_cardDatabase.GetAllSkillCards().Cast<BaseCard>());
            
            availableCards = allCards.Where(c => c is T).Cast<T>().ToList();
            
            if (targetRarity.HasValue)
            {
                availableCards = availableCards.Where(c => c.Rarity == targetRarity.Value).ToList();
            }
        }
        
        if (availableCards.Count == 0)
            return null;
            
        // 应用颜色权重过滤
        var weightedCards = ApplyWeights(availableCards.Cast<BaseCard>().ToList());
        
        // 随机选择
        return SelectRandomWeighted(weightedCards) as T;
    }
    
    /// <summary>
    /// 获取随机卡牌（任意类型）
    /// </summary>
    public BaseCard GetRandomCard(CardRarity? targetRarity = null)
    {
        if (_cardDatabase == null) return null;
        
        var availableCards = new List<BaseCard>();
        
        if (targetRarity.HasValue)
        {
            availableCards.AddRange(_cardDatabase.GetMonsterCardsByRarity(targetRarity.Value).Cast<BaseCard>());
            availableCards.AddRange(_cardDatabase.GetSkillCardsByRarity(targetRarity.Value).Cast<BaseCard>());
        }
        else
        {
            availableCards.AddRange(_cardDatabase.GetAllMonsterCards().Cast<BaseCard>());
            availableCards.AddRange(_cardDatabase.GetAllSkillCards().Cast<BaseCard>());
        }
        
        if (availableCards.Count == 0)
            return null;
            
        // 应用权重过滤
        var weightedCards = ApplyWeights(availableCards);
        
        // 随机选择
        return SelectRandomWeighted(weightedCards);
    }
    
    /// <summary>
    /// 应用权重
    /// </summary>
    private List<(BaseCard card, float weight)> ApplyWeights(List<BaseCard> cards)
    {
        var weightedCards = new List<(BaseCard, float)>();
        
        foreach (var card in cards)
        {
            float weight = 1f;
            
            // 计算颜色权重
            foreach (var color in card.ColorRequirements)
            {
                if (ColorWeights.ContainsKey(color))
                {
                    weight *= ColorWeights[color];
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
    private BaseCard SelectRandomWeighted(List<(BaseCard card, float weight)> weightedCards)
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
    public List<BaseCard> GetCardPack(int cardCount = 5)
    {
        var pack = new List<BaseCard>();
        
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
    public void AddCardToCollection(BaseCard card)
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
        
        GD.Print($"Added {card.CardName} to collection");
    }
    
    /// <summary>
    /// 获取指定颜色的卡牌
    /// </summary>
    public List<T> GetCardsByColor<T>(MagicColor color) where T : BaseCard
    {
        return OwnedCards.Where(card => card is T && card.HasColorRequirement(color)).Cast<T>().ToList();
    }
    
    /// <summary>
    /// 获取指定稀有度的卡牌
    /// </summary>
    public List<T> GetCardsByRarity<T>(CardRarity rarity) where T : BaseCard
    {
        return OwnedCards.Where(card => card is T && card.Rarity == rarity).Cast<T>().ToList();
    }
    
    /// <summary>
    /// 获取指定种族的怪物卡牌
    /// </summary>
    public List<MonsterCard> GetCardsByRace(MonsterRace race)
    {
        return OwnedCards.Where(card => card is MonsterCard mc && mc.Race == race).Cast<MonsterCard>().ToList();
    }
    
    /// <summary>
    /// 获取指定技能类型的技能卡牌
    /// </summary>
    public List<SkillCard> GetCardsBySkillType(SkillType skillType)
    {
        return OwnedCards.Where(card => card is SkillCard sc && sc.SkillType == skillType).Cast<SkillCard>().ToList();
    }
    
    /// <summary>
    /// 过滤可召唤的怪物卡牌
    /// </summary>
    public List<MonsterCard> GetSummonableCards(HeroInstance summoner)
    {
        if (summoner == null)
            return new List<MonsterCard>();
            
        return OwnedCards.Where(card => card is MonsterCard mc && summoner.CanSummonMonster(mc)).Cast<MonsterCard>().ToList();
    }
    
    /// <summary>
    /// 获取卡牌数量
    /// </summary>
    public int GetCardCount(string cardId)
    {
        return CardCounts.ContainsKey(cardId) ? CardCounts[cardId] : 0;
    }
    
    /// <summary>
    /// 获取所有怪物卡牌
    /// </summary>
    public List<MonsterCard> GetAllMonsterCards()
    {
        if (_cardDatabase == null)
        {
            GD.PrintErr("CardDatabase not available");
            return new List<MonsterCard>();
        }
        return _cardDatabase.GetAllMonsterCards().ToList();
    }
    
    /// <summary>
    /// 根据稀有度获取怪物卡牌
    /// </summary>
    public List<MonsterCard> GetMonsterCardsByRarity(CardRarity rarity)
    {
        if (_cardDatabase == null)
        {
            GD.PrintErr("CardDatabase not available");
            return new List<MonsterCard>();
        }
        return _cardDatabase.GetMonsterCardsByRarity(rarity).ToList();
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
        
        // 按卡牌类型统计
        int monsterCount = OwnedCards.Count(card => card is MonsterCard);
        int skillCount = OwnedCards.Count(card => card is SkillCard);
        stats["monster_cards"] = monsterCount;
        stats["skill_cards"] = skillCount;
        
        return stats;
    }
}