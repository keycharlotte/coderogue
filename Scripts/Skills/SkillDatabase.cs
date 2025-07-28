using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class SkillDatabase : Node
{
	[Export] public Array<SkillCard> AllSkills { get; set; }
	[Export] public string SkillConfigPath { get; set; } = "res://ResourcesData/SkillCard/";
	// 移除SkillDeck相关属性，现在使用UnifiedDeck
	
	private Godot.Collections.Dictionary<int, SkillCard> _skillsById;
	private Godot.Collections.Dictionary<CardRarity, Array<SkillCard>> _skillsByRarity;
	private Godot.Collections.Dictionary<SkillType, Array<SkillCard>> _skillsByType;
	private Godot.Collections.Dictionary<string, Array<SkillCard>> _skillsByTag;
	
	public override void _Ready()
	{
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
		
	}
	
	// 移除LoadDeckConfigs方法，现在使用UnifiedDeck
	
	private void IndexSkills()
	{
		_skillsById = [];
		_skillsByRarity = [];
		_skillsByType = [];
		_skillsByTag = [];
		
		foreach (var skill in AllSkills)
		{
			// 按ID索引
			_skillsById[skill.Id] = skill;
			
			// 按稀有度索引
			if (!_skillsByRarity.ContainsKey(skill.SkillRarity))
				_skillsByRarity[skill.SkillRarity] = new Array<SkillCard>();
			_skillsByRarity[skill.SkillRarity].Add(skill);
			
			// 按类型索引
			if (!_skillsByType.ContainsKey(skill.SkillType))
				_skillsByType[skill.SkillType] = new Array<SkillCard>();
			_skillsByType[skill.SkillType].Add(skill);
			
			// 按标签索引
			if (skill.Tags != null)
			{
				foreach (var tag in skill.Tags)
				{
					if (!_skillsByTag.ContainsKey(tag))
						_skillsByTag[tag] = new Array<SkillCard>();
					_skillsByTag[tag].Add(skill);
				}
			}
		}

		GD.Print($"Indexed {_skillsById.Count} skills by ID");
		GD.Print($"Indexed {_skillsByRarity.Count} skills by rarity");
		GD.Print($"Indexed {_skillsByType.Count} skills by type");
		GD.Print($"Indexed {_skillsByTag.Count} skills by tag");
	}
	
	// 移除IndexDecks方法，现在使用UnifiedDeck
	
	// 查询方法
	public SkillCard GetSkillById(int id)
	{
		return _skillsById.GetValueOrDefault(id);
	}
	
	public Array<SkillCard> GetSkillsByRarity(CardRarity rarity)
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
	
	// 移除卡组查询方法，现在使用UnifiedDeck
}
