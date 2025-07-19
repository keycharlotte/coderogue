using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class SkillDatabase : Node
{
	[Export] public Array<SkillCard> AllSkills { get; set; }
	[Export] public string SkillConfigPath { get; set; } = "res://ResourcesData/SkillCard/";
	[Export] public Array<SkillDeck> AllDecks { get; set; }
	[Export] public string DeckConfigPath { get; set; } = "res://ResourcesData/SkillDeck/";
	
	private Godot.Collections.Dictionary<int, SkillCard> _skillsById;
	private Godot.Collections.Dictionary<SkillRarity, Array<SkillCard>> _skillsByRarity;
	private Godot.Collections.Dictionary<SkillType, Array<SkillCard>> _skillsByType;
	private Godot.Collections.Dictionary<string, Array<SkillCard>> _skillsByTag;
	[Export] private Godot.Collections.Dictionary<string, SkillDeck> _decksByName;
	
	public override void _Ready()
	{
		LoadSkillConfigs();
		LoadDeckConfigs();
		IndexSkills();
		IndexDecks();
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
	
	private void LoadDeckConfigs()
	{
		AllDecks = new Array<SkillDeck>();
		
		// 从资源文件夹加载所有卡组配置
		var dir = DirAccess.Open(DeckConfigPath);
		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			
			while (fileName != "")
			{
				if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
				{
					var deckResource = GD.Load<SkillDeck>(DeckConfigPath + fileName);
					if (deckResource != null)
					{
						AllDecks.Add(deckResource);
					}
				}
				fileName = dir.GetNext();
			}
		}
		
	}
	
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

		GD.Print($"Indexed {_skillsById.Count} skills by ID");
		GD.Print($"Indexed {_skillsByRarity.Count} skills by rarity");
		GD.Print($"Indexed {_skillsByType.Count} skills by type");
		GD.Print($"Indexed {_skillsByTag.Count} skills by tag");
	}
	
	private void IndexDecks()
	{
		_decksByName = [];
		
		foreach (var deck in AllDecks)
		{
			// 优先使用Name属性，如果为空则使用文件名作为备选
			string deckName = !string.IsNullOrEmpty(deck.Name) 
				? deck.Name 
				: deck.ResourcePath.GetFile().GetBaseName();
			_decksByName[deckName] = deck;
		}
		
		GD.Print($"Indexed {_decksByName.Count} decks by name");
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
	
	// 卡组查询方法
	public SkillDeck GetDeckByName(string deckName)
	{
		return _decksByName.GetValueOrDefault(deckName);
	}
	
	public Array<SkillDeck> GetAllDecks()
	{
		return AllDecks;
	}
	
	public Array<string> GetAllDeckNames()
	{
		var names = new Array<string>();
		foreach (var deckName in _decksByName.Keys)
		{
			names.Add(deckName);
		}
		return names;
	}
}
