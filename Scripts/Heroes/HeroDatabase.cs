using System.Collections.Generic;
using Godot;
using Godot.Collections;
using CodeRogue.Data;

[GlobalClass]
public partial class HeroDatabase : Node
{
    private Godot.Collections.Dictionary<int, HeroConfig> _heroConfigs;
    private Array<HeroConfig> _allHeroes;
    
    public override void _Ready()
    {
        _heroConfigs = new Godot.Collections.Dictionary<int, HeroConfig>();
        _allHeroes = new Array<HeroConfig>();
        LoadHeroConfigs();
    }
    
    private void LoadHeroConfigs()
    {
        // 尝试从JSON文件加载英雄配置
        string configPath = "res://ResourcesData/Heroes/hero_configs.json";
        
        if (FileAccess.FileExists(configPath))
        {
            LoadFromJsonFile(configPath);
        }
        else
        {
            GD.PrintErr($"英雄配置文件不存在: {configPath}，使用示例配置");
            CreateExampleHeroes();
        }
    }
    
    private void LoadFromJsonFile(string filePath)
    {
        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"无法打开英雄配置文件: {filePath}");
            CreateExampleHeroes();
            return;
        }
        
        string jsonContent = file.GetAsText();
        var json = new Json();
        var parseResult = json.Parse(jsonContent);
        
        if (parseResult != Error.Ok)
        {
            GD.PrintErr($"解析英雄配置JSON失败: {json.GetErrorMessage()}");
            CreateExampleHeroes();
            return;
        }
        
        var jsonData = json.Data.AsGodotDictionary();
        if (jsonData.ContainsKey("heroes"))
        {
            var heroesArray = jsonData["heroes"].AsGodotArray();
            foreach (var heroData in heroesArray)
            {
                var heroDict = heroData.AsGodotDictionary();
                var heroConfig = ParseHeroConfig(heroDict);
                if (heroConfig != null)
                {
                    AddHero(heroConfig);
                }
            }
            
            GD.Print($"成功加载 {_allHeroes.Count} 个英雄配置");
        }
        else
        {
            GD.PrintErr("JSON文件中未找到heroes数组");
            CreateExampleHeroes();
        }
    }
    
    private HeroConfig ParseHeroConfig(Godot.Collections.Dictionary heroDict)
    {
        try
        {
            var hero = new HeroConfig();
            hero.Id = heroDict.GetValueOrDefault("id", 0).AsInt32();
            hero.Name = heroDict.GetValueOrDefault("name", "").AsString();
            hero.Description = heroDict.GetValueOrDefault("description", "").AsString();
            hero.Rarity = (HeroRarity)heroDict.GetValueOrDefault("rarity", 0).AsInt32();
            hero.Class = (HeroClass)heroDict.GetValueOrDefault("class", 0).AsInt32();
            hero.AvatarPath = heroDict.GetValueOrDefault("avatarPath", "").AsString();
            hero.ModelPath = heroDict.GetValueOrDefault("modelPath", "").AsString();
            
            // 解析基础属性
            if (heroDict.ContainsKey("baseStats"))
            {
                var statsDict = heroDict["baseStats"].AsGodotDictionary();
                hero.BaseStats = ParseHeroStats(statsDict);
            }
            
            // 解析成长属性
            if (heroDict.ContainsKey("growthStats"))
            {
                var growthDict = heroDict["growthStats"].AsGodotDictionary();
                hero.GrowthStats = ParseHeroStats(growthDict);
            }
            
            // 解析召唤师配置
            hero.MaxColorSlots = heroDict.GetValueOrDefault("maxColorSlots", 6).AsInt32();
            hero.PrimaryColor = (MagicColor)heroDict.GetValueOrDefault("primaryColor", 0).AsInt32();
            hero.TypingDamageBase = heroDict.GetValueOrDefault("typingDamageBase", 15f).AsSingle();
            hero.TypingSpeedBonus = heroDict.GetValueOrDefault("typingSpeedBonus", 0.05f).AsSingle();
            hero.TypingAccuracyBonus = heroDict.GetValueOrDefault("typingAccuracyBonus", 0.03f).AsSingle();
            hero.TypingDecayResistance = heroDict.GetValueOrDefault("typingDecayResistance", 0.1f).AsSingle();
            
            return hero;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"解析英雄配置失败: {ex.Message}");
            return null;
        }
    }
    
    private HeroStats ParseHeroStats(Godot.Collections.Dictionary statsDict)
    {
        var stats = new HeroStats();
        stats.Health = statsDict.GetValueOrDefault("health", 100).AsInt32();
        stats.Attack = statsDict.GetValueOrDefault("attack", 20).AsInt32();
        stats.Defense = statsDict.GetValueOrDefault("defense", 10).AsInt32();
        stats.CritRate = statsDict.GetValueOrDefault("critRate", 0.05f).AsSingle();
        stats.CritDamage = statsDict.GetValueOrDefault("critDamage", 1.5f).AsSingle();
        return stats;
    }
    
    private void CreateExampleHeroes()
    {
        // 示例英雄1：稀有战士
        var warrior = new HeroConfig();
        warrior.Id = 1001;
        warrior.Name = "钢铁战士";
        warrior.Description = "拥有坚韧防御的近战战士";
        warrior.Rarity = HeroRarity.Rare;
        warrior.Class = HeroClass.Warrior;
        warrior.AvatarPath = "res://heroes/avatars/warrior_001.png";
        warrior.ModelPath = "res://heroes/models/warrior_001.tscn";
        
        warrior.BaseStats = new HeroStats();
        warrior.BaseStats.Health = 100;
        warrior.BaseStats.Attack = 20;
        warrior.BaseStats.Defense = 15;
        warrior.BaseStats.CritRate = 0.05f;
        warrior.BaseStats.CritDamage = 1.5f;
        
        warrior.GrowthStats = new HeroStats();
        warrior.GrowthStats.Health = 5;
        warrior.GrowthStats.Attack = 1;
        warrior.GrowthStats.Defense = 1;
        warrior.GrowthStats.CritRate = 0.001f;
        warrior.GrowthStats.CritDamage = 0.01f;
        
        // 召唤师配置
        warrior.MaxColorSlots = 6;
        warrior.PrimaryColor = MagicColor.Red;
        warrior.RaceBonus = new Godot.Collections.Dictionary<MonsterRace, float> { { MonsterRace.Human, 0.1f } };
        warrior.BondBonus = new Godot.Collections.Dictionary<BondType, float> { { BondType.HolyWarrior, 0.05f } };
        warrior.TypingDamageBase = 15f;
        // warrior.TypingDecayRate = 0.1f; // 属性不存在，暂时注释
        warrior.TypingSpeedBonus = 0.05f;
        warrior.TypingAccuracyBonus = 0.03f;
        warrior.TypingDecayResistance = 0.1f;
        warrior.DefaultSummonerSkills = new Godot.Collections.Array<SummonerSkillType> { SummonerSkillType.CombatExpertise, SummonerSkillType.DamageAmplifier };
        warrior.DefaultSkillValues = new Godot.Collections.Array<float> { 1.0f, 0.1f };
        
        AddHero(warrior);
        
        // 示例英雄2：史诗法师
        var mage = new HeroConfig();
        mage.Id = 2001;
        mage.Name = "元素法师";
        mage.Description = "掌控元素力量的强大法师";
        mage.Rarity = HeroRarity.Epic;
        mage.Class = HeroClass.Mage;
        mage.AvatarPath = "res://heroes/avatars/mage_001.png";
        mage.ModelPath = "res://heroes/models/mage_001.tscn";
        
        mage.BaseStats = new HeroStats();
        mage.BaseStats.Health = 80;
        mage.BaseStats.Attack = 30;
        mage.BaseStats.Defense = 10;
        mage.BaseStats.CritRate = 0.08f;
        mage.BaseStats.CritDamage = 2.0f;
        
        mage.GrowthStats = new HeroStats();
        mage.GrowthStats.Health = 3;
        mage.GrowthStats.Attack = 2;
        mage.GrowthStats.Defense = 0.5f;
        mage.GrowthStats.CritRate = 0.002f;
        mage.GrowthStats.CritDamage = 0.02f;
        
        // 召唤师配置
        mage.MaxColorSlots = 8;
        mage.PrimaryColor = MagicColor.Blue;
        mage.RaceBonus = new Godot.Collections.Dictionary<MonsterRace, float> { { MonsterRace.Elemental, 0.15f } };
        mage.BondBonus = new Godot.Collections.Dictionary<BondType, float> { { BondType.OrderMage, 0.1f } };
        mage.TypingDamageBase = 20f;
        // mage.TypingDecayRate = 0.08f; // 属性不存在，暂时注释
        mage.TypingSpeedBonus = 0.1f;
        mage.TypingAccuracyBonus = 0.05f;
        mage.TypingDecayResistance = 0.15f;
        mage.DefaultSummonerSkills = new Godot.Collections.Array<SummonerSkillType> { SummonerSkillType.ElementalMastery, SummonerSkillType.DamageAmplifier };
        mage.DefaultSkillValues = new Godot.Collections.Array<float> { 1.2f, 0.15f };
        
        AddHero(mage);
    }
    
    private void AddHero(HeroConfig hero)
    {
        _heroConfigs[hero.Id] = hero;
        _allHeroes.Add(hero);
    }
    
    // 根据ID获取英雄配置
    public HeroConfig GetHeroConfig(int heroId)
    {
        return _heroConfigs.GetValueOrDefault(heroId);
    }
    
    // 获取所有英雄配置
    public Array<HeroConfig> GetAllHeroes()
    {
        return _allHeroes;
    }
    
    // 根据品级筛选英雄
    public Array<HeroConfig> GetHeroesByRarity(HeroRarity rarity)
    {
        var result = new Array<HeroConfig>();
        foreach (var hero in _allHeroes)
        {
            if (hero.Rarity == rarity)
            {
                result.Add(hero);
            }
        }
        return result;
    }
    
    // 根据职业筛选英雄
    public Array<HeroConfig> GetHeroesByClass(HeroClass heroClass)
    {
        var result = new Array<HeroConfig>();
        foreach (var hero in _allHeroes)
        {
            if (hero.Class == heroClass)
            {
                result.Add(hero);
            }
        }
        return result;
    }
    
    // 根据获取方式筛选英雄
    public Array<HeroConfig> GetHeroesByObtainMethod(HeroObtainMethod method)
    {
        var result = new Array<HeroConfig>();
        foreach (var hero in _allHeroes)
        {
            if (hero.ObtainMethods.Contains(method))
            {
                result.Add(hero);
            }
        }
        return result;
    }
}