using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class HeroManager : Node
{
	[Signal] public delegate void HeroObtainedEventHandler(HeroInstance hero);
	[Signal] public delegate void HeroLevelUpEventHandler(HeroInstance hero, int newLevel);
	[Signal] public delegate void HeroStarUpEventHandler(HeroInstance hero, int newStar);
	[Signal] public delegate void HeroAwakenedEventHandler(HeroInstance hero, int newAwakening);
	
	private Godot.Collections.Dictionary<string, HeroInstance> _ownedHeroes;
	private Godot.Collections.Dictionary<int, HeroConfig> _heroConfigs;
	private SoulLinkSystem _soulLinkSystem;
	private SpecialTraitProcessor _traitProcessor;
	private RaritySystem _raritySystem;
	
	public override void _Ready()
	{
		InitializeSystem();
	}
	
	private void InitializeSystem()
	{
		_ownedHeroes = new Godot.Collections.Dictionary<string, HeroInstance>();
		_heroConfigs = new Godot.Collections.Dictionary<int, HeroConfig>();
		
		// 获取子系统
		_soulLinkSystem = GetNode<SoulLinkSystem>("SoulLinkSystem");
		_traitProcessor = GetNode<SpecialTraitProcessor>("SpecialTraitProcessor");
		_raritySystem = RaritySystem.Instance;
		
		LoadHeroDatabase();
		LoadPlayerHeroes();
	}
	
	private void LoadHeroDatabase()
	{
		var database = GetNode<HeroDatabase>("/root/HeroDatabase");
		if (database == null)
		{
			GD.PrintErr("HeroDatabase autoload not found!");
			return;
		}
		var allHeroes = database.GetAllHeroes();
		
		foreach (var hero in allHeroes)
		{
			_heroConfigs[hero.Id] = hero;
		}
	}
	
	private void LoadPlayerHeroes()
	{
		// TODO: 从存档加载玩家拥有的英雄
		// 这里先创建一些测试英雄
		CreateTestHeroes();
	}
	
	private void CreateTestHeroes()
	{
		// 创建测试英雄实例
		var testHero = ObtainHero(1001);
		if (testHero != null)
		{
			testHero.Level = 10;
			testHero.Star = 3;
		}
	}
	
	// 获得英雄
	public HeroInstance ObtainHero(int configId)
	{
		var config = GetHeroConfig(configId);
		if (config == null) return null;
		
		var hero = new HeroInstance();
		hero.InstanceId = Guid.NewGuid().ToString();
		hero.ConfigId = configId;
		hero.Config = config;
		hero.Level = 1;
		hero.Star = 1;
		hero.Awakening = 0;
		hero.IsUnlocked = true;
		hero.IsSoulLinked = false;
		hero.ObtainTime = Time.GetUnixTimeFromSystem();
		hero.LastUsedTime = 0;
		
		_ownedHeroes[hero.InstanceId] = hero;
		
		EmitSignal(SignalName.HeroObtained, hero);
		return hero;
	}
	
	// 英雄升级
	public bool LevelUpHero(string heroId)
	{
		var hero = GetHeroInstance(heroId);
		if (hero == null) return false;
		
		var rarityConfig = _raritySystem.GetRarityConfig(hero.Config.Rarity);
		if (hero.Level >= rarityConfig.MaxLevel) return false;
		
		// 检查升级消耗
		if (!CheckLevelUpCost(hero)) return false;
		
		// 消耗资源
		ConsumeLevelUpCost(hero);
		
		// 升级
		hero.Level++;
		
		EmitSignal(SignalName.HeroLevelUp, hero, hero.Level);
		return true;
	}
	
	// 英雄升星
	public bool StarUpHero(string heroId)
	{
		var hero = GetHeroInstance(heroId);
		if (hero == null) return false;
		
		var rarityConfig = _raritySystem.GetRarityConfig(hero.Config.Rarity);
		if (hero.Star >= rarityConfig.MaxStar) return false;
		
		// 检查升星消耗
		if (!CheckStarUpCost(hero)) return false;
		
		// 消耗资源
		ConsumeStarUpCost(hero);
		
		// 升星
		hero.Star++;
		
		EmitSignal(SignalName.HeroStarUp, hero, hero.Star);
		return true;
	}
	
	// 英雄觉醒
	public bool AwakenHero(string heroId)
	{
		var hero = GetHeroInstance(heroId);
		if (hero == null) return false;
		
		var rarityConfig = _raritySystem.GetRarityConfig(hero.Config.Rarity);
		if (hero.Awakening >= rarityConfig.MaxAwakening) return false;
		
		// 检查觉醒条件
		if (!CheckAwakeningRequirements(hero)) return false;
		
		// 消耗资源
		ConsumeAwakeningCost(hero);
		
		// 觉醒
		hero.Awakening++;
		
		EmitSignal(SignalName.HeroAwakened, hero, hero.Awakening);
		return true;
	}
	
	// 获取英雄实例
	public HeroInstance GetHeroInstance(string instanceId)
	{
		return _ownedHeroes.GetValueOrDefault(instanceId);
	}
	
	// 获取英雄配置
	public HeroConfig GetHeroConfig(int configId)
	{
		return _heroConfigs.GetValueOrDefault(configId);
	}
	
	// 获取所有拥有的英雄
	public Array<HeroInstance> GetOwnedHeroes()
	{
		return new Array<HeroInstance>(_ownedHeroes.Values);
	}
	
	// 按品级筛选英雄
	public Array<HeroInstance> GetHeroesByRarity(HeroRarity rarity)
	{
		return new Array<HeroInstance>(GetOwnedHeroes().Where(h => h.Config.Rarity == rarity).ToArray());
	}
	
	// 按职业筛选英雄
	public Array<HeroInstance> GetHeroesByClass(HeroClass heroClass)
	{
		return new Array<HeroInstance>(GetOwnedHeroes().Where(h => h.Config.Class == heroClass).ToArray());
	}
	
	// 获取未被灵魂链接的英雄
	public Array<HeroInstance> GetAvailableForSoulLink()
	{
		return new Array<HeroInstance>(GetOwnedHeroes().Where(h => !h.IsSoulLinked).ToArray());
	}
	
	// 检查升级消耗
	private bool CheckLevelUpCost(HeroInstance hero)
	{
		// TODO: 实现升级消耗检查逻辑
		return true;
	}
	
	// 消耗升级资源
	private void ConsumeLevelUpCost(HeroInstance hero)
	{
		// TODO: 实现升级资源消耗逻辑
	}
	
	// 检查升星消耗
	private bool CheckStarUpCost(HeroInstance hero)
	{
		// TODO: 实现升星消耗检查逻辑
		return true;
	}
	
	// 消耗升星资源
	private void ConsumeStarUpCost(HeroInstance hero)
	{
		// TODO: 实现升星资源消耗逻辑
	}
	
	// 检查觉醒条件
	private bool CheckAwakeningRequirements(HeroInstance hero)
	{
		// TODO: 实现觉醒条件检查逻辑
		return true;
	}
	
	// 消耗觉醒资源
	private void ConsumeAwakeningCost(HeroInstance hero)
	{
		// TODO: 实现觉醒资源消耗逻辑
	}
	
	// 保存英雄数据
	public void SaveHeroData()
	{
		// TODO: 实现英雄数据保存逻辑
	}
	
	// 加载英雄数据
	public void LoadHeroData()
	{
		// TODO: 实现英雄数据加载逻辑
	}

	public HeroInstance GetActiveHero()
	{
		throw new NotImplementedException();
	}
}
