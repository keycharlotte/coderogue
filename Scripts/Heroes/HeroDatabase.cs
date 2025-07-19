using System.Collections.Generic;
using Godot;
using Godot.Collections;

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
        // TODO: 从资源文件加载英雄配置
        // 这里先创建示例配置
        CreateExampleHeroes();
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