using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class RaritySystem : Node
{
    private static RaritySystem _instance;
    public static RaritySystem Instance => _instance;
    
    private Godot.Collections.Dictionary<HeroRarity, RarityConfig> _rarityConfigs;
    
    public override void _Ready()
    {
        if (_instance == null)
        {
            _instance = this;
            InitializeRarityConfigs();
        }
        else
        {
            QueueFree();
        }
    }
    
    private void InitializeRarityConfigs()
    {
        _rarityConfigs = new Godot.Collections.Dictionary<HeroRarity, RarityConfig>();
        LoadRarityConfigs();
    }
    
    private void LoadRarityConfigs()
    {
        // TODO: 从资源文件加载品级配置
        // 这里先创建默认配置
        CreateDefaultRarityConfigs();
    }
    
    private void CreateDefaultRarityConfigs()
    {
        // 稀有品级
        var rareConfig = new RarityConfig();
        rareConfig.Rarity = HeroRarity.Rare;
        rareConfig.Name = "稀有";
        rareConfig.Color = Colors.Blue;
        rareConfig.StatMultiplier = 1.0f;
        rareConfig.MaxLevel = 60;
        rareConfig.MaxStar = 5;
        rareConfig.MaxAwakening = 3;
        rareConfig.DropRate = 50.0f;
        rareConfig.FragmentsToSummon = 10;
        _rarityConfigs[HeroRarity.Rare] = rareConfig;
        
        // 史诗品级
        var epicConfig = new RarityConfig();
        epicConfig.Rarity = HeroRarity.Epic;
        epicConfig.Name = "史诗";
        epicConfig.Color = Colors.Purple;
        epicConfig.StatMultiplier = 1.5f;
        epicConfig.MaxLevel = 80;
        epicConfig.MaxStar = 6;
        epicConfig.MaxAwakening = 5;
        epicConfig.DropRate = 30.0f;
        epicConfig.FragmentsToSummon = 20;
        _rarityConfigs[HeroRarity.Epic] = epicConfig;
        
        // 传说品级
        var legendaryConfig = new RarityConfig();
        legendaryConfig.Rarity = HeroRarity.Legendary;
        legendaryConfig.Name = "传说";
        legendaryConfig.Color = Colors.Orange;
        legendaryConfig.StatMultiplier = 2.0f;
        legendaryConfig.MaxLevel = 100;
        legendaryConfig.MaxStar = 7;
        legendaryConfig.MaxAwakening = 7;
        legendaryConfig.DropRate = 15.0f;
        legendaryConfig.FragmentsToSummon = 50;
        _rarityConfigs[HeroRarity.Legendary] = legendaryConfig;
        
        // 神话品级
        var mythicConfig = new RarityConfig();
        mythicConfig.Rarity = HeroRarity.Mythic;
        mythicConfig.Name = "神话";
        mythicConfig.Color = Colors.Red;
        mythicConfig.StatMultiplier = 3.0f;
        mythicConfig.MaxLevel = 120;
        mythicConfig.MaxStar = 8;
        mythicConfig.MaxAwakening = 10;
        mythicConfig.DropRate = 5.0f;
        mythicConfig.FragmentsToSummon = 100;
        _rarityConfigs[HeroRarity.Mythic] = mythicConfig;
    }
    
    // 获取品级配置
    public RarityConfig GetRarityConfig(HeroRarity rarity)
    {
        return _rarityConfigs.GetValueOrDefault(rarity);
    }
    
    // 计算品级加成
    public HeroStats ApplyRarityBonus(HeroStats baseStats, HeroRarity rarity)
    {
        var config = GetRarityConfig(rarity);
        if (config == null) return baseStats;
        
        var bonusStats = new HeroStats();
        bonusStats.Health = baseStats.Health * config.StatMultiplier;
        bonusStats.Attack = baseStats.Attack * config.StatMultiplier;
        bonusStats.Defense = baseStats.Defense * config.StatMultiplier;
        bonusStats.CritRate = baseStats.CritRate * config.StatMultiplier;
        bonusStats.CritDamage = baseStats.CritDamage * config.StatMultiplier;
        
        return bonusStats;
    }
    
    // 获取品级颜色
    public Color GetRarityColor(HeroRarity rarity)
    {
        return rarity switch
        {
            HeroRarity.Rare => Colors.Blue,
            HeroRarity.Epic => Colors.Purple,
            HeroRarity.Legendary => Colors.Orange,
            HeroRarity.Mythic => Colors.Red,
            _ => Colors.White
        };
    }
    
    // 获取所有品级配置
    public Array<RarityConfig> GetAllRarityConfigs()
    {
        return new Array<RarityConfig>(_rarityConfigs.Values);
    }
}