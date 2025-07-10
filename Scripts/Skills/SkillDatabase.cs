using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class SkillDatabase : Node
{
	private static SkillDatabase _instance;
	public static SkillDatabase Instance => _instance;
	
	[Export] public Array<SkillCard> AllSkills { get; set; }
	[Export] public string SkillConfigPath { get; set; } = "res://ResourcesData/Skills/";
	
	private Godot.Collections.Dictionary<int, SkillCard> _skillsById;
	private Godot.Collections.Dictionary<SkillRarity, Array<SkillCard>> _skillsByRarity;
	private Godot.Collections.Dictionary<SkillType, Array<SkillCard>> _skillsByType;
	private Godot.Collections.Dictionary<string, Array<SkillCard>> _skillsByTag;
	
	public override void _Ready()
	{
		_instance = this;
		LoadSkillConfigs();
		IndexSkills();
	}
	
	private void LoadSkillConfigs()
	{
		AllSkills = new Array<SkillCard>();
		
		// 从资源文件夹加载所有技能配置
		var dir = DirAccess.Open(SkillConfigPath);
		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			
			while (fileName != "")
			{
				if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
				{
					var skillResource = GD.Load<SkillCard>(SkillConfigPath + fileName);
					if (skillResource != null)
					{
						AllSkills.Add(skillResource);
					}
				}
				fileName = dir.GetNext();
			}
		}
		
		// 如果没有找到配置文件，创建默认技能
		if (AllSkills.Count == 0)
		{
			CreateDefaultSkills();
		}
	}
	
	private void CreateDefaultSkills()
	{
		// 创建一些默认技能用于测试
		var fireball = CreateFireballSkill();
		var heal = CreateHealSkill();
		var shield = CreateShieldSkill();
		var lightning = CreateLightningSkill();
		
		AllSkills.Add(fireball);
		AllSkills.Add(heal);
		AllSkills.Add(shield);
		AllSkills.Add(lightning);
	}
	
	private SkillCard CreateFireballSkill()
	{
		var skill = new SkillCard();
		skill.Id = 1;
		skill.Name = "火球术";
		skill.Description = "发射火球，造成范围伤害";
		skill.Type = SkillType.Attack;
		skill.Rarity = SkillRarity.Common;
		skill.ChargeCost = 2;
		skill.IconPath = "res://Icons/fireball.png";
		skill.RarityColor = Colors.White;
		
		// 创建技能效果
		var damageEffect = new SkillEffect();
		damageEffect.Type = SkillEffectType.Damage;
		damageEffect.Value = 50f;
		damageEffect.TargetProperty = "Health";
		
		skill.Effects = new Array<SkillEffect> { damageEffect };
		
		// 创建技能标签
		var attackTag = new SkillTag { Name = "攻击", Color = Colors.Red };
		var fireTag = new SkillTag { Name = "火焰", Color = Colors.Orange };
		skill.Tags = new Array<SkillTag> { attackTag, fireTag };
		
		// 创建升级数据
		skill.LevelData = new Array<SkillLevelData>
		{
			new SkillLevelData { Level = 1, ChargeCost = 2, EffectValues = new Array<float> { 50f } },
			new SkillLevelData { Level = 2, ChargeCost = 2, EffectValues = new Array<float> { 62.5f } },
			new SkillLevelData { Level = 3, ChargeCost = 2, EffectValues = new Array<float> { 75f } },
			new SkillLevelData { Level = 4, ChargeCost = 1, EffectValues = new Array<float> { 87.5f } },
			new SkillLevelData { Level = 5, ChargeCost = 1, EffectValues = new Array<float> { 100f } }
		};
		
		return skill;
	}
	
	private SkillCard CreateHealSkill()
	{
		var skill = new SkillCard();
		skill.Id = 2;
		skill.Name = "治疗术";
		skill.Description = "恢复生命值";
		skill.Type = SkillType.Utility;
		skill.Rarity = SkillRarity.Common;
		skill.ChargeCost = 2;
		skill.IconPath = "res://Icons/heal.png";
		skill.RarityColor = Colors.White;
		
		var healEffect = new SkillEffect();
		healEffect.Type = SkillEffectType.Heal;
		healEffect.Value = 30f;
		healEffect.TargetProperty = "Health";
		
		skill.Effects = new Array<SkillEffect> { healEffect };
		
		var utilityTag = new SkillTag { Name = "辅助", Color = Colors.Green };
		var healTag = new SkillTag { Name = "治疗", Color = Colors.LightGreen };
		skill.Tags = new Array<SkillTag> { utilityTag, healTag };
		
		skill.LevelData = new Array<SkillLevelData>
		{
			new SkillLevelData { Level = 1, ChargeCost = 2, EffectValues = new Array<float> { 30f } },
			new SkillLevelData { Level = 2, ChargeCost = 2, EffectValues = new Array<float> { 37.5f } },
			new SkillLevelData { Level = 3, ChargeCost = 2, EffectValues = new Array<float> { 45f } },
			new SkillLevelData { Level = 4, ChargeCost = 1, EffectValues = new Array<float> { 52.5f } },
			new SkillLevelData { Level = 5, ChargeCost = 1, EffectValues = new Array<float> { 60f } }
		};
		
		return skill;
	}
	
	private SkillCard CreateShieldSkill()
	{
		var skill = new SkillCard();
		skill.Id = 3;
		skill.Name = "护盾术";
		skill.Description = "获得临时护盾，吸收伤害";
		skill.Type = SkillType.Defense;
		skill.Rarity = SkillRarity.Common;
		skill.ChargeCost = 2;
		skill.IconPath = "res://Icons/shield.png";
		skill.RarityColor = Colors.White;
		
		var shieldEffect = new SkillEffect();
		shieldEffect.Type = SkillEffectType.Shield;
		shieldEffect.Value = 40f;
		shieldEffect.Duration = 10f;
		shieldEffect.TargetProperty = "Shield";
		
		skill.Effects = new Array<SkillEffect> { shieldEffect };
		
		var defenseTag = new SkillTag { Name = "防御", Color = Colors.Blue };
		var shieldTag = new SkillTag { Name = "护盾", Color = Colors.LightBlue };
		skill.Tags = new Array<SkillTag> { defenseTag, shieldTag };
		
		skill.LevelData = new Array<SkillLevelData>
		{
			new SkillLevelData { Level = 1, ChargeCost = 2, EffectValues = new Array<float> { 40f } },
			new SkillLevelData { Level = 2, ChargeCost = 2, EffectValues = new Array<float> { 50f } },
			new SkillLevelData { Level = 3, ChargeCost = 2, EffectValues = new Array<float> { 60f } },
			new SkillLevelData { Level = 4, ChargeCost = 1, EffectValues = new Array<float> { 70f } },
			new SkillLevelData { Level = 5, ChargeCost = 1, EffectValues = new Array<float> { 80f } }
		};
		
		return skill;
	}
	
	private SkillCard CreateLightningSkill()
	{
		var skill = new SkillCard();
		skill.Id = 4;
		skill.Name = "闪电链";
		skill.Description = "闪电在敌人间跳跃，最多5个目标";
		skill.Type = SkillType.Attack;
		skill.Rarity = SkillRarity.Rare;
		skill.ChargeCost = 3;
		skill.IconPath = "res://Icons/lightning.png";
		skill.RarityColor = Colors.Blue;
		
		var lightningEffect = new SkillEffect();
		lightningEffect.Type = SkillEffectType.Damage;
		lightningEffect.Value = 35f;
		lightningEffect.TargetProperty = "Health";
		lightningEffect.Parameters = new Dictionary { ["MaxTargets"] = 5, ["ChainDamageReduction"] = 0.8f };
		
		skill.Effects = new Array<SkillEffect> { lightningEffect };
		
		var attackTag = new SkillTag { Name = "攻击", Color = Colors.Red };
		var lightningTag = new SkillTag { Name = "闪电", Color = Colors.Yellow };
		skill.Tags = new Array<SkillTag> { attackTag, lightningTag };
		
		skill.LevelData = new Array<SkillLevelData>
		{
			new SkillLevelData { Level = 1, ChargeCost = 3, EffectValues = new Array<float> { 35f } },
			new SkillLevelData { Level = 2, ChargeCost = 3, EffectValues = new Array<float> { 43.75f } },
			new SkillLevelData { Level = 3, ChargeCost = 3, EffectValues = new Array<float> { 52.5f } },
			new SkillLevelData { Level = 4, ChargeCost = 2, EffectValues = new Array<float> { 61.25f } },
			new SkillLevelData { Level = 5, ChargeCost = 2, EffectValues = new Array<float> { 70f } }
		};
		
		return skill;
	}
	
	private void IndexSkills()
	{
		_skillsById = new Godot.Collections.Dictionary<int, SkillCard>();
		_skillsByRarity = new Godot.Collections.Dictionary<SkillRarity, Array<SkillCard>>();
		_skillsByType = new Godot.Collections.Dictionary<SkillType, Array<SkillCard>>();
		_skillsByTag = new Godot.Collections.Dictionary<string, Array<SkillCard>>();
		
		foreach (var skill in AllSkills)
		{
			// 按ID索引
			_skillsById[skill.Id] = skill;
			
			// 按稀有度索引
			if (!_skillsByRarity.ContainsKey(skill.Rarity))
				_skillsByRarity[skill.Rarity] = new Array<SkillCard>();
			_skillsByRarity[skill.Rarity].Add(skill);
			
			// 按类型索引
			if (!_skillsByType.ContainsKey(skill.Type))
				_skillsByType[skill.Type] = new Array<SkillCard>();
			_skillsByType[skill.Type].Add(skill);
			
			// 按标签索引
			if (skill.Tags != null)
			{
				foreach (var tag in skill.Tags)
				{
					if (!_skillsByTag.ContainsKey(tag.Name))
						_skillsByTag[tag.Name] = new Array<SkillCard>();
					_skillsByTag[tag.Name].Add(skill);
				}
			}
		}
	}
	
	// 查询方法
	public SkillCard GetSkillById(int id)
	{
		return _skillsById.GetValueOrDefault(id);
	}
	
	public Array<SkillCard> GetSkillsByRarity(SkillRarity rarity)
	{
		return _skillsByRarity.GetValueOrDefault(rarity, new Array<SkillCard>());
	}
	
	public Array<SkillCard> GetSkillsByType(SkillType type)
	{
		return _skillsByType.GetValueOrDefault(type, new Array<SkillCard>());
	}
	
	public Array<SkillCard> GetSkillsByTag(string tagName)
	{
		return _skillsByTag.GetValueOrDefault(tagName, new Array<SkillCard>());
	}
	
	public Array<SkillCard> GetAllSkills()
	{
		return AllSkills;
	}
}
