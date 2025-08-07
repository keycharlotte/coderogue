using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Utils;
using Random = System.Random;

[GlobalClass]
public partial class SkillSelector : Node
{
	private static SkillSelector _instance;
	public static SkillSelector Instance => _instance;
	
	[Signal] public delegate void SkillSelectionStartedEventHandler(Array<SkillCard> options);
	[Signal] public delegate void SkillSelectedEventHandler(SkillCard selectedSkill);
	[Signal] public delegate void SkillUpgradedEventHandler(SkillCard upgradedSkill);
	
	[Export] public int OptionsPerSelection { get; set; } = 3;
	
	private CardDatabase _database;
	private Godot.Collections.Dictionary<CardRarity, float> _selectionWeights;
	private Random _random;
	
	public override void _Ready()
	{
		_instance = this;
		_database = NodeUtils.GetCardDatabase(this);
		if (_database == null)
		{
			GD.PrintErr("CardDatabase autoload not found!");
			return;
		}
		_random = new Random();
		InitializeSelectionWeights();
	}
	
	private void InitializeSelectionWeights()
	{
		_selectionWeights = new Godot.Collections.Dictionary<CardRarity, float>
		{
			{ CardRarity.Common, 60f },
			{ CardRarity.Uncommon, 25f },
			{ CardRarity.Rare, 12f },
			{ CardRarity.Epic, 2.5f },
			{ CardRarity.Legendary, 0.5f }
		};
	}
	
	public void TriggerSkillSelection(int playerLevel)
	{
		var options = GenerateSkillOptions(playerLevel);
		EmitSignal(SignalName.SkillSelectionStarted, new Array<SkillCard>(options));
	}
	
	private List<SkillCard> GenerateSkillOptions(int playerLevel)
	{
		var options = new List<SkillCard>();
		var deckManager = NodeUtils.GetDeckManager(this);
		var currentDeck = deckManager?.GetCurrentDeck();
		var ownedSkills = currentDeck?.Cards?.OfType<SkillCard>().ToList() ?? new List<SkillCard>();
		
		// 调整权重基于游戏进度
		var adjustedWeights = AdjustWeightsByProgress(playerLevel);
		
		for (int i = 0; i < OptionsPerSelection; i++)
		{
			var skill = GenerateSingleOption(ownedSkills, adjustedWeights, options);
			if (skill != null)
			{
				options.Add(skill);
			}
		}
		
		return options;
	}
	
	private SkillCard GenerateSingleOption(List<SkillCard> ownedSkills, Godot.Collections.Dictionary<CardRarity, float> weights, List<SkillCard> existingOptions)
	{
		// 随机选择稀有度
		var rarity = SelectRandomRarity(weights);
		
		// 获取该稀有度的所有技能卡
		var availableSkills = _database.GetSkillCardsByRarity(rarity);
		
		// 过滤已选择的选项 - 修复：显式转换
		availableSkills = new Godot.Collections.Array<SkillCard>(availableSkills.Where(s => !existingOptions.Any(o => o.Id == s.Id)));
		
		if (availableSkills.Count == 0) return null;
		
		// 检查是否已拥有该技能（用于升级）
		var ownedSkill = ownedSkills.FirstOrDefault(owned => 
			availableSkills.Any(available => available.Id == owned.Id));
		
		if (ownedSkill != null && ownedSkill.CanUpgrade())
		{
			// 返回升级版本
			var upgradedSkill = ownedSkill.CreateUpgradedCopy();
			return upgradedSkill;
		}
		else
		{
			// 返回新技能
			var randomIndex = _random.Next(availableSkills.Count);
			return availableSkills[randomIndex];
		}
	}
	
	private CardRarity SelectRandomRarity(Godot.Collections.Dictionary<CardRarity, float> weights)
	{
		float totalWeight = 0f;
		foreach (var weight in weights.Values)
		{
			totalWeight += weight;
		}
		
		float randomValue = (float)_random.NextDouble() * totalWeight;
		
		float currentWeight = 0f;
		foreach (var kvp in weights)
		{
			currentWeight += kvp.Value;
			if (randomValue <= currentWeight)
			{
				return kvp.Key;
			}
		}
		
		return CardRarity.Common; // 默认返回普通
	}
	
	private Godot.Collections.Dictionary<CardRarity, float> AdjustWeightsByProgress(int playerLevel)
	{
		var adjustedWeights = new Godot.Collections.Dictionary<CardRarity, float>();
		
		// 复制原始权重
		foreach (var kvp in _selectionWeights)
		{
			adjustedWeights[kvp.Key] = kvp.Value;
		}
		
		// 随着等级提升，高稀有度技能概率增加
		float progressMultiplier = 1f + (playerLevel * 0.05f);
		
		adjustedWeights[CardRarity.Uncommon] *= progressMultiplier;
		adjustedWeights[CardRarity.Rare] *= progressMultiplier * 1.2f;
		adjustedWeights[CardRarity.Epic] *= progressMultiplier * 1.5f;
		adjustedWeights[CardRarity.Legendary] *= progressMultiplier * 2f;
		
		// 根据当前构建倾向调整权重
		AdjustWeightsByBuildTendency(adjustedWeights);
		
		return adjustedWeights;
	}
	
	private void AdjustWeightsByBuildTendency(Godot.Collections.Dictionary<CardRarity, float> weights)
	{
		var deckManager = NodeUtils.GetDeckManager(this);
		var currentDeck = deckManager?.GetCurrentDeck();
		if (currentDeck?.Cards == null) return;
		
		// 分析当前构建倾向
		var attackRatio = currentDeck.GetSkillRatio(SkillType.Attack);
		var defenseRatio = currentDeck.GetSkillRatio(SkillType.Defense);
		var utilityRatio = currentDeck.GetSkillRatio(SkillType.Utility);
		
		// 如果某种类型技能过多，降低该类型的权重
		if (attackRatio > 0.6f)
		{
			// 攻击技能过多，提升防御和辅助技能权重
			// 这里可以进一步细化权重调整逻辑
		}
	}
	
	public void SelectSkill(SkillCard selectedSkill)
	{
		var deckManager = NodeUtils.GetDeckManager(this);
		var currentDeck = deckManager?.GetCurrentDeck();
		if (currentDeck == null) return;
		
		// 检查是否是升级现有技能
		var existingSkill = currentDeck.Cards?.FirstOrDefault(c => c.Id == selectedSkill.Id);
		if (existingSkill != null)
		{
			// 升级现有技能
			existingSkill.Upgrade();
			EmitSignal(SignalName.SkillUpgraded, existingSkill);
		}
		else
		{
			// 添加新技能到卡组
			currentDeck.AddCard(selectedSkill);
			EmitSignal(SignalName.SkillSelected, selectedSkill);
		}
		
		// 更新技能轨道
		var skillTrackManager = NodeUtils.GetSkillTrackManager(this);
		skillTrackManager?.SetDeck(currentDeck);
	}
}

// 扩展SkillCard类以支持升级
public partial class SkillCard
{
	public SkillCard CreateUpgradedCopy()
	{
		var upgradedSkill = (SkillCard)Duplicate();
		upgradedSkill.Level = Level + 1;
		upgradedSkill.ApplyUpgradeEffects();
		return upgradedSkill;
	}
}
