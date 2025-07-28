using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

[GlobalClass]
public partial class CardDatabase : Node
{
    [Export] public Array<MonsterCard> AllMonsterCards { get; set; }
    [Export] public Array<SkillCard> AllSkillCards { get; set; }
    [Export] public string ConfigPath { get; set; } = "res://ResourcesData/CardConfigs.json";
    [Export] public string MonsterCardResourcePath { get; set; } = "res://ResourcesData/MonsterCard/";
    [Export] public string SkillCardResourcePath { get; set; } = "res://ResourcesData/SkillCard/";
    
    // 稀有度和颜色权重
    private Godot.Collections.Dictionary<CardRarity, float> _rarityWeights;
    private Godot.Collections.Dictionary<MagicColor, float> _colorWeights;
    
    // 索引字典
    private Godot.Collections.Dictionary<int, MonsterCard> _monsterCardsById;
    private Godot.Collections.Dictionary<int, SkillCard> _skillCardsById;
    private Godot.Collections.Dictionary<CardRarity, Array<MonsterCard>> _monsterCardsByRarity;
    private Godot.Collections.Dictionary<CardRarity, Array<SkillCard>> _skillCardsByRarity;
    private Godot.Collections.Dictionary<MonsterRace, Array<MonsterCard>> _monsterCardsByRace;
    private Godot.Collections.Dictionary<SkillType, Array<SkillCard>> _skillCardsByType;
    
    public override void _Ready()
    {
        LoadCardConfigs();
        LoadCardResourceConfigs();
        IndexCards();
        GD.Print($"CardDatabase initialized: {AllMonsterCards.Count} monster cards, {AllSkillCards.Count} skill cards");
    }
    
    /// <summary>
    /// 从JSON配置文件加载卡牌数据
    /// </summary>
    private void LoadCardConfigs()
    {
        AllMonsterCards = new Array<MonsterCard>();
        AllSkillCards = new Array<SkillCard>();
        _rarityWeights = new Godot.Collections.Dictionary<CardRarity, float>();
        _colorWeights = new Godot.Collections.Dictionary<MagicColor, float>();
        
        if (!FileAccess.FileExists(ConfigPath))
        {
            GD.PrintErr($"Card config file not found: {ConfigPath}");
            CreateDefaultCards();
            return;
        }
        
        using var file = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"Failed to open card config file: {ConfigPath}");
            CreateDefaultCards();
            return;
        }
        
        string jsonText = file.GetAsText();
        
        try
        {
            var jsonDoc = JsonDocument.Parse(jsonText);
            var root = jsonDoc.RootElement;
            
            // 加载怪物卡牌
            if (root.TryGetProperty("monster_cards", out var monsterCards))
            {
                LoadMonsterCards(monsterCards);
            }
            
            // 加载技能卡牌
            if (root.TryGetProperty("skill_cards", out var skillCards))
            {
                LoadSkillCards(skillCards);
            }
            
            // 加载稀有度权重
            if (root.TryGetProperty("rarity_weights", out var rarityWeights))
            {
                LoadRarityWeights(rarityWeights);
            }
            
            // 加载颜色权重
            if (root.TryGetProperty("color_weights", out var colorWeights))
            {
                LoadColorWeights(colorWeights);
            }
        }
        catch (JsonException ex)
        {
            GD.PrintErr($"Failed to parse card config JSON: {ex.Message}");
            CreateDefaultCards();
        }
    }
    
    /// <summary>
    /// 从Resource文件夹加载卡牌数据
    /// </summary>
    private void LoadCardResourceConfigs()
    {
        LoadMonsterCardResources();
        LoadSkillCardResources();
    }
    
    /// <summary>
    /// 从Resource文件夹加载怪物卡牌
    /// </summary>
    private void LoadMonsterCardResources()
    {
        var dir = DirAccess.Open(MonsterCardResourcePath);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            
            while (fileName != "")
            {
                if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
                {
                    var monsterCardResource = GD.Load<MonsterCard>(MonsterCardResourcePath + fileName);
                    if (monsterCardResource != null)
                    {
                        // 检查是否已存在相同ID的卡牌，避免重复添加
                        bool exists = false;
                        foreach (var existingCard in AllMonsterCards)
                        {
                            if (existingCard.Id == monsterCardResource.Id)
                            {
                                exists = true;
                                break;
                            }
                        }
                        
                        if (!exists)
                        {
                            AllMonsterCards.Add(monsterCardResource);
                            GD.Print($"Loaded monster card from resource: {monsterCardResource.CardName}");
                        }
                    }
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.Print($"Monster card resource directory not found: {MonsterCardResourcePath}");
        }
    }
    
    /// <summary>
    /// 从Resource文件夹加载技能卡牌
    /// </summary>
    private void LoadSkillCardResources()
    {
        var dir = DirAccess.Open(SkillCardResourcePath);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            
            while (fileName != "")
            {
                if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
                {
                    var skillCardResource = GD.Load<SkillCard>(SkillCardResourcePath + fileName);
                    if (skillCardResource != null)
                    {
                        // 检查是否已存在相同ID的卡牌，避免重复添加
                        bool exists = false;
                        foreach (var existingCard in AllSkillCards)
                        {
                            if (existingCard.Id == skillCardResource.Id)
                            {
                                exists = true;
                                break;
                            }
                        }
                        
                        if (!exists)
                        {
                            AllSkillCards.Add(skillCardResource);
                            GD.Print($"Loaded skill card from resource: {skillCardResource.CardName}");
                        }
                    }
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.Print($"Skill card resource directory not found: {SkillCardResourcePath}");
        }
    }
    
    private void LoadMonsterCards(JsonElement monsterCardsElement)
    {
        foreach (var cardElement in monsterCardsElement.EnumerateArray())
        {
            var card = CreateMonsterCardFromJson(cardElement);
            if (card != null)
            {
                AllMonsterCards.Add(card);
            }
        }
    }
    
    private void LoadSkillCards(JsonElement skillCardsElement)
    {
        foreach (var cardElement in skillCardsElement.EnumerateArray())
        {
            var card = CreateSkillCardFromJson(cardElement);
            if (card != null)
            {
                AllSkillCards.Add(card);
            }
        }
    }
    
    private MonsterCard CreateMonsterCardFromJson(JsonElement cardElement)
    {
        var card = new MonsterCard();
        
        try
        {
            card.Id = cardElement.GetProperty("id").GetString().GetHashCode();
            card.CardName = cardElement.GetProperty("name").GetString();
            card.MonsterName = cardElement.GetProperty("name").GetString();
            card.Description = cardElement.GetProperty("description").GetString();
            card.Attack = cardElement.GetProperty("attack").GetInt32();
            card.Health = cardElement.GetProperty("health").GetInt32();
            card.Cost = cardElement.GetProperty("summon_cost").GetInt32();
            
            // 解析稀有度
            var rarityStr = cardElement.GetProperty("rarity").GetString();
            if (System.Enum.TryParse<CardRarity>(rarityStr, true, out var rarity))
            {
                card.Rarity = rarity;
            }
            
            // 解析种族
            var raceStr = cardElement.GetProperty("race").GetString();
            if (System.Enum.TryParse<MonsterRace>(raceStr, true, out var race))
            {
                card.Race = race;
            }
            
            // 解析颜色需求
            if (cardElement.TryGetProperty("color_requirements", out var colorReqs))
            {
                var colorArray = new Godot.Collections.Array<MagicColor>();
                foreach (var colorProperty in colorReqs.EnumerateObject())
                {
                    var colorStr = colorProperty.Name;
                    var colorCount = colorProperty.Value.GetInt32();
                    if (System.Enum.TryParse<MagicColor>(colorStr, true, out var color))
                    {
                        // 根据数量添加颜色需求
                        for (int i = 0; i < colorCount; i++)
                        {
                            colorArray.Add(color);
                        }
                    }
                }
                card.ColorRequirements = colorArray;
            }
            
            // 解析技能
            if (cardElement.TryGetProperty("skills", out var skillsElement))
            {
                var skillsArray = new Godot.Collections.Array<MonsterSkillType>();
                foreach (var skillElement in skillsElement.EnumerateArray())
                {
                    var skillStr = skillElement.GetString();
                    // 映射技能名称到枚举值
                    var mappedSkill = MapSkillNameToEnum(skillStr);
                    if (mappedSkill.HasValue)
                    {
                        skillsArray.Add(mappedSkill.Value);
                    }
                }
                card.Skills = skillsArray;
            }
            
            // 初始化其他必需的数组字段
            card.BondTypes = new Godot.Collections.Array<BondType>();
            card.SkillValues = new Godot.Collections.Array<float>();
            
            // 设置图标路径
            if (cardElement.TryGetProperty("icon_path", out var iconElement))
            {
                card.IconPath = iconElement.GetString();
            }
            
            return card;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to create monster card from JSON: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 将技能名称映射到MonsterSkillType枚举
    /// </summary>
    private MonsterSkillType? MapSkillNameToEnum(string skillName)
    {
        return skillName.ToLower() switch
        {
            "heal" => MonsterSkillType.Heal,
            "protect" => MonsterSkillType.Protect,
            "shield" => MonsterSkillType.Shield,
            "rush" => MonsterSkillType.Rush,
            "burn" => MonsterSkillType.Burn,
            "poison" => MonsterSkillType.Poison,
            "freeze" => MonsterSkillType.Freeze,
            "criticalstrike" => MonsterSkillType.CriticalStrike,
            "aoeattack" => MonsterSkillType.AoEAttack,
            "magicattack" => MonsterSkillType.MagicAttack,
            "taunt" => MonsterSkillType.Taunt,
            "stealth" => MonsterSkillType.Stealth,
            "regeneration" => MonsterSkillType.Regeneration,
            "stun" => MonsterSkillType.Stun,
            "silence" => MonsterSkillType.Silence,
            "fear" => MonsterSkillType.Fear,
            "buff" => MonsterSkillType.Buff,
            "pack" => MonsterSkillType.Pack,
            "summon" => MonsterSkillType.Summon,
            "transform" => MonsterSkillType.Transform,
            "sacrifice" => MonsterSkillType.Sacrifice,
            _ => null
        };
    }
    
    private SkillCard CreateSkillCardFromJson(JsonElement cardElement)
    {
        var card = new SkillCard();
        
        try
        {
            card.Id = cardElement.GetProperty("id").GetString().GetHashCode();
            card.CardName = cardElement.GetProperty("name").GetString();
            card.Description = cardElement.GetProperty("description").GetString();
            card.Cost = cardElement.GetProperty("charge_cost").GetInt32();
            
            // 解析稀有度
            var rarityStr = cardElement.GetProperty("rarity").GetString();
            if (System.Enum.TryParse<CardRarity>(rarityStr, true, out var rarity))
            {
                card.Rarity = rarity;
            }
            
            // 解析技能类型
            var typeStr = cardElement.GetProperty("skill_type").GetString();
            if (System.Enum.TryParse<SkillType>(typeStr, true, out var skillType))
            {
                card.SkillType = skillType;
            }
            
            // 解析颜色需求
            if (cardElement.TryGetProperty("color_requirements", out var colorReqs))
            {
                var colorArray = new Godot.Collections.Array<MagicColor>();
                foreach (var colorProperty in colorReqs.EnumerateObject())
                {
                    var colorStr = colorProperty.Name;
                    var colorCount = colorProperty.Value.GetInt32();
                    if (System.Enum.TryParse<MagicColor>(colorStr, true, out var color))
                    {
                        // 根据数量添加颜色需求
                        for (int i = 0; i < colorCount; i++)
                        {
                            colorArray.Add(color);
                        }
                    }
                }
                card.ColorRequirements = colorArray;
            }
            
            return card;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to create skill card from JSON: {ex.Message}");
            return null;
        }
    }
    
    private void LoadRarityWeights(JsonElement rarityWeightsElement)
    {
        foreach (var property in rarityWeightsElement.EnumerateObject())
        {
            if (System.Enum.TryParse<CardRarity>(property.Name, true, out var rarity))
            {
                _rarityWeights[rarity] = property.Value.GetSingle();
            }
        }
    }
    
    private void LoadColorWeights(JsonElement colorWeightsElement)
    {
        foreach (var property in colorWeightsElement.EnumerateObject())
        {
            if (System.Enum.TryParse<MagicColor>(property.Name, true, out var color))
            {
                _colorWeights[color] = property.Value.GetSingle();
            }
        }
    }
    
    private void CreateDefaultCards()
    {
        // 创建默认怪物卡
        var defaultMonster = new MonsterCard
        {
            Id = 1,
            CardName = "默认怪物",
            MonsterName = "默认怪物",
            Description = "一个基础的怪物卡牌",
            Attack = 2,
            Health = 3,
            Cost = 1,
            Rarity = CardRarity.Common,
            Race = MonsterRace.Beast,
            ColorRequirements = new Godot.Collections.Array<MagicColor> { MagicColor.White },
            Skills = new Godot.Collections.Array<MonsterSkillType>()
        };
        AllMonsterCards.Add(defaultMonster);
        
        // 创建默认技能卡
        var defaultSkill = new SkillCard
        {
            Id = 1,
            CardName = "默认技能",
            Description = "一个基础的技能卡牌",
            Cost = 10,
			Rarity = CardRarity.Common,
            SkillType = SkillType.Attack,
            ColorRequirements = new Godot.Collections.Array<MagicColor> { MagicColor.White }
        };
        AllSkillCards.Add(defaultSkill);
        
        // 设置默认权重
        _rarityWeights[CardRarity.Common] = 0.6f;
        _rarityWeights[CardRarity.Uncommon] = 0.25f;
        _rarityWeights[CardRarity.Rare] = 0.12f;
        _rarityWeights[CardRarity.Epic] = 0.03f;
        
        foreach (MagicColor color in System.Enum.GetValues<MagicColor>())
        {
            _colorWeights[color] = 1.0f;
        }
    }
    
    private void IndexCards()
    {
        _monsterCardsById = new Godot.Collections.Dictionary<int, MonsterCard>();
        _skillCardsById = new Godot.Collections.Dictionary<int, SkillCard>();
        _monsterCardsByRarity = new Godot.Collections.Dictionary<CardRarity, Array<MonsterCard>>();
        _skillCardsByRarity = new Godot.Collections.Dictionary<CardRarity, Array<SkillCard>>();
        _monsterCardsByRace = new Godot.Collections.Dictionary<MonsterRace, Array<MonsterCard>>();
        _skillCardsByType = new Godot.Collections.Dictionary<SkillType, Array<SkillCard>>();
        
        // 索引怪物卡
        foreach (var card in AllMonsterCards)
        {
            _monsterCardsById[card.Id] = card;
            
            if (!_monsterCardsByRarity.ContainsKey(card.Rarity))
                _monsterCardsByRarity[card.Rarity] = new Array<MonsterCard>();
            _monsterCardsByRarity[card.Rarity].Add(card);
            
            if (!_monsterCardsByRace.ContainsKey(card.Race))
                _monsterCardsByRace[card.Race] = new Array<MonsterCard>();
            _monsterCardsByRace[card.Race].Add(card);
        }
        
        // 索引技能卡
        foreach (var card in AllSkillCards)
        {
            _skillCardsById[card.Id] = card;
            
            if (!_skillCardsByRarity.ContainsKey(card.Rarity))
				_skillCardsByRarity[card.Rarity] = new Array<SkillCard>();
			_skillCardsByRarity[card.Rarity].Add(card);
            
            if (!_skillCardsByType.ContainsKey(card.SkillType))
                _skillCardsByType[card.SkillType] = new Array<SkillCard>();
            _skillCardsByType[card.SkillType].Add(card);
        }
    }
    
    // 查询方法
    public MonsterCard GetMonsterCardById(int id)
    {
        return _monsterCardsById.GetValueOrDefault(id);
    }
    
    public SkillCard GetSkillCardById(int id)
    {
        return _skillCardsById.GetValueOrDefault(id);
    }
    
    public Array<MonsterCard> GetMonsterCardsByRarity(CardRarity rarity)
    {
        return _monsterCardsByRarity.GetValueOrDefault(rarity, new Array<MonsterCard>());
    }
    
    public Array<SkillCard> GetSkillCardsByRarity(CardRarity rarity)
    {
        return _skillCardsByRarity.GetValueOrDefault(rarity, new Array<SkillCard>());
    }
    
    public Array<MonsterCard> GetMonsterCardsByRace(MonsterRace race)
    {
        return _monsterCardsByRace.GetValueOrDefault(race, new Array<MonsterCard>());
    }
    
    public Array<SkillCard> GetSkillCardsByType(SkillType type)
    {
        return _skillCardsByType.GetValueOrDefault(type, new Array<SkillCard>());
    }
    
    public Array<MonsterCard> GetAllMonsterCards()
    {
        return AllMonsterCards;
    }
    
    public Array<SkillCard> GetAllSkillCards()
    {
        return AllSkillCards;
    }
    
    public float GetRarityWeight(CardRarity rarity)
    {
        return _rarityWeights.GetValueOrDefault(rarity, 1.0f);
    }
    
    public float GetColorWeight(MagicColor color)
    {
        return _colorWeights.GetValueOrDefault(color, 1.0f);
    }
    
    public Godot.Collections.Dictionary<CardRarity, float> GetAllRarityWeights()
    {
        return _rarityWeights;
    }
    
    public Godot.Collections.Dictionary<MagicColor, float> GetAllColorWeights()
    {
        return _colorWeights;
    }
}