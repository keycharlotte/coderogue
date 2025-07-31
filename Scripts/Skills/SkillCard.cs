using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SkillCard : BaseCard
{
	[Export] public SkillType SkillType { get; set; }
	[Export] public Array<SkillEffect> Effects { get; set; }
	
	public SkillCard()
	{
		CardType = CardType.Skill;
		Effects = new Array<SkillEffect>();
	}
	
	/// <summary>
	/// 获取技能名称（映射到基类的CardName）
	/// </summary>
	public string Name
	{
		get => CardName;
		set => CardName = value;
	}
	
	/// <summary>
	/// 获取技能稀有度（映射到基类的Rarity）
	/// </summary>
	public new CardRarity Rarity
	{
		get => base.Rarity;
		set => base.Rarity = value;
	}
	
	/// <summary>
	/// 获取充能费用（映射到基类的Cost）
	/// </summary>
	public new int Cost
	{
		get => base.Cost;
		set => base.Cost = value;
	}
	
	// 技能升级数据
	[Export] public Array<SkillLevelData> LevelData { get; set; }
	
	public SkillLevelData GetCurrentLevelData()
	{
		if (LevelData == null || LevelData.Count == 0) return null;
		int index = Mathf.Clamp(Level - 1, 0, LevelData.Count - 1);
		return LevelData[index];
	}
	
	public override bool CanUpgrade()
	{
		return Level < LevelData?.Count;
	}
	
	public override void Upgrade()
	{
		if (CanUpgrade())
		{
			Level++;
			// 应用升级效果
			ApplyUpgradeEffects();
		}
	}
	
	private void ApplyUpgradeEffects()
	{
		var levelData = GetCurrentLevelData();
		if (levelData != null)
		{
			// 更新技能效果数值
			for (int i = 0; i < Effects.Count && i < levelData.EffectValues.Count; i++)
			{
				Effects[i].Value = levelData.EffectValues[i];
			}
			
			// 更新充能消耗
			Cost = levelData.ChargeCost;
		}
	}
	
	/// <summary>
	/// 获取技能的战斗力评估
	/// </summary>
	public override float GetPowerRating()
	{
		float baseRating = 0f;
		
		// 根据技能效果计算基础评分
		foreach (var effect in Effects)
		{
			baseRating += effect.Value * GetEffectMultiplier(effect.Type);
		}
		
		// 考虑充能消耗（消耗越低评分越高）
		float costEfficiency = 10f / Mathf.Max(1f, Cost);
		
		// 稀有度加成
		float rarityMultiplier = Rarity switch
		{
			CardRarity.Common => 1.0f,
			CardRarity.Uncommon => 1.1f,
			CardRarity.Rare => 1.2f,
			CardRarity.Epic => 1.5f,
			CardRarity.Legendary => 2.0f,
			_ => 1.0f
		};
		
		return (baseRating + costEfficiency) * rarityMultiplier;
	}
	
	/// <summary>
	/// 获取效果类型的倍数
	/// </summary>
	private float GetEffectMultiplier(SkillEffectType effectType)
	{
		return effectType switch
		{
			SkillEffectType.Damage => 1.0f,
			SkillEffectType.Heal => 0.8f,
			SkillEffectType.Shield => 0.6f,
			SkillEffectType.Buff => 0.5f,
			_ => 0.3f
		};
	}
	
	/// <summary>
	/// 创建技能卡副本
	/// </summary>
	public override BaseCard CreateCopy()
	{
		var copy = new SkillCard();
		copy.Id = Id;
		copy.CardName = CardName;
		copy.Description = Description;
		copy.SkillType = SkillType;
		copy.Rarity = Rarity;
		copy.Cost = Cost;
		copy.Level = Level;
		copy.IconPath = IconPath;
		copy.RarityColor = RarityColor;
		copy.ColorRequirements = new Array<MagicColor>(ColorRequirements);
		copy.Tags = new Array<CardTag>(Tags);
		
		// 复制技能效果
		copy.Effects = new Array<SkillEffect>();
		foreach (var effect in Effects)
		{
			copy.Effects.Add(effect);
		}
		
		// 复制升级数据
		copy.LevelData = new Array<SkillLevelData>();
		foreach (var levelData in LevelData)
		{
			copy.LevelData.Add(levelData);
		}
		
		return copy;
	}
	
	/// <summary>
	/// 创建技能卡副本（类型安全版本）
	/// </summary>
	public SkillCard CreateSkillCopy()
	{
		return (SkillCard)CreateCopy();
	}
}
