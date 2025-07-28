using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 卡组管理器 - 管理包含所有类型卡牌的统一卡组
/// 负责牌组的管理
/// </summary>
[GlobalClass]
public partial class DeckManager : Node
{
    [Signal] public delegate void DeckChangedEventHandler(UnifiedDeck newDeck);
    [Signal] public delegate void CardAddedEventHandler(BaseCard card);
    [Signal] public delegate void CardRemovedEventHandler(BaseCard card);
    [Signal] public delegate void DeckSavedEventHandler(string deckName);
    [Signal] public delegate void DeckLoadedEventHandler(string deckName);
    
    // 当前卡组
    [Export] public UnifiedDeck CurrentDeck { get; private set; }
    
    // 保存的卡组列表
    [Export] public Array<UnifiedDeck> SavedDecks { get; private set; } = new Array<UnifiedDeck>();
    
    // 默认卡组名称
    [Export] public string DefaultDeckName { get; set; } = "Default Deck";
    
    // 文件路径
    private const string DECKS_SAVE_PATH = "user://unified_decks.save";
    
    // 依赖组件
    private CardDatabase _cardDatabase;
    
    public override void _Ready()
    {
        // 获取依赖组件
        _cardDatabase = GetNode<CardDatabase>("/root/CardDatabase");
        
        if (_cardDatabase == null)
        {
            GD.PrintErr("CardDatabase not found! DeckManager requires CardDatabase.");
        }
        
        // 加载保存的卡组
        LoadDecks();
        
        // 如果没有卡组，创建默认卡组
        if (SavedDecks.Count == 0)
        {
            CreateDefaultDeck();
        }
        
        // 设置当前卡组
        if (CurrentDeck == null && SavedDecks.Count > 0)
        {
            SetCurrentDeck(SavedDecks[0]);
        }
    }
    
    /// <summary>
    /// 创建默认卡组
    /// </summary>
    private void CreateDefaultDeck()
    {
        var defaultDeck = new UnifiedDeck();
        defaultDeck.DeckName = DefaultDeckName;
        
        // 添加一些基础技能卡
        if (_cardDatabase != null)
        {
            var basicSkills = _cardDatabase.GetSkillCardsByRarity(CardRarity.Common);
            int skillsToAdd = Math.Min(5, basicSkills.Count);
            for (int i = 0; i < skillsToAdd; i++)
            {
                defaultDeck.AddSkillCard(basicSkills[i]);
            }
        }
        
        // 添加一些基础怪物卡
        if (_cardDatabase != null)
        {
            var basicMonsters = _cardDatabase.GetMonsterCardsByRarity(CardRarity.Common);
            int monstersToAdd = Math.Min(10, basicMonsters.Count);
            for (int i = 0; i < monstersToAdd; i++)
            {
                defaultDeck.AddMonsterCard(basicMonsters[i]);
            }
        }
        
        AddDeck(defaultDeck);
        GD.Print($"Created default deck: {DefaultDeckName}");
    }
    
    /// <summary>
    /// 添加新卡组
    /// </summary>
    public bool AddDeck(UnifiedDeck deck)
    {
        if (deck == null || string.IsNullOrEmpty(deck.DeckName))
            return false;
            
        // 检查名称是否重复
        if (SavedDecks.Any(d => d.DeckName == deck.DeckName))
        {
            GD.PrintErr($"Deck with name '{deck.DeckName}' already exists");
            return false;
        }
        
        SavedDecks.Add(deck);
        
        // 自动保存
        SaveDecks();
        
        GD.Print($"Added deck: {deck.DeckName}");
        return true;
    }
    
    /// <summary>
    /// 移除卡组
    /// </summary>
    public bool RemoveDeck(string deckName)
    {
        var deck = GetDeck(deckName);
        if (deck == null)
            return false;
            
        // 不能删除当前使用的卡组
        if (deck == CurrentDeck)
        {
            GD.PrintErr("Cannot remove currently active deck");
            return false;
        }
        
        SavedDecks.Remove(deck);
        
        // 自动保存
        SaveDecks();
        
        GD.Print($"Removed deck: {deckName}");
        return true;
    }
    
    /// <summary>
    /// 获取指定名称的卡组
    /// </summary>
    public UnifiedDeck GetDeck(string deckName)
    {
        return SavedDecks.FirstOrDefault(d => d.DeckName == deckName);
    }
    
    /// <summary>
    /// 设置当前卡组
    /// </summary>
    public bool SetCurrentDeck(UnifiedDeck deck)
    {
        if (deck == null || !SavedDecks.Contains(deck))
            return false;
            
        CurrentDeck = deck;
        CurrentDeck.Initialize();
        EmitSignal(SignalName.DeckChanged, CurrentDeck);
        
        GD.Print($"Current deck changed to: {deck.DeckName}");
        return true;
    }
    
    /// <summary>
    /// 设置当前卡组（通过名称）
    /// </summary>
    public bool SetCurrentDeck(string deckName)
    {
        var deck = GetDeck(deckName);
        return SetCurrentDeck(deck);
    }
    
    /// <summary>
    /// 获取当前卡组
    /// </summary>
    public UnifiedDeck GetCurrentDeck()
    {
        return CurrentDeck;
    }
    
    /// <summary>
    /// 添加卡牌到当前卡组
    /// </summary>
    public bool AddCardToDeck(BaseCard card)
    {
        if (CurrentDeck == null || card == null)
            return false;
            
        bool success = false;
        
        if (card is MonsterCard monsterCard)
        {
            success = CurrentDeck.AddMonsterCard(monsterCard);
        }
        else if (card is SkillCard skillCard)
        {
            success = CurrentDeck.AddSkillCard(skillCard);
        }
        else
        {
            success = CurrentDeck.AddCard(card);
        }
        
        if (success)
        {
            EmitSignal(SignalName.CardAdded, card);
            EmitSignal(SignalName.DeckChanged, CurrentDeck);
            SaveDecks(); // 自动保存
        }
        
        return success;
    }
    
    /// <summary>
    /// 从当前卡组移除卡牌
    /// </summary>
    public bool RemoveCardFromDeck(BaseCard card)
    {
        if (CurrentDeck == null || card == null)
            return false;
            
        bool success = false;
        
        if (card is MonsterCard monsterCard)
        {
            success = CurrentDeck.RemoveMonsterCard(monsterCard);
        }
        else if (card is SkillCard skillCard)
        {
            success = CurrentDeck.RemoveSkillCard(skillCard);
        }
        else
        {
            success = CurrentDeck.RemoveCard(card);
        }
        
        if (success)
        {
            EmitSignal(SignalName.CardRemoved, card);
            EmitSignal(SignalName.DeckChanged, CurrentDeck);
            SaveDecks(); // 自动保存
        }
        
        return success;
    }
    
    /// <summary>
    /// 创建新的空卡组
    /// </summary>
    public UnifiedDeck CreateNewDeck(string deckName, SummonerHero summoner = null)
    {
        if (string.IsNullOrEmpty(deckName) || SavedDecks.Any(d => d.DeckName == deckName))
            return null;
            
        var newDeck = new UnifiedDeck();
        newDeck.DeckName = deckName;
        newDeck.AssociatedSummoner = summoner;
        
        AddDeck(newDeck);
        return newDeck;
    }
    
    /// <summary>
    /// 复制卡组
    /// </summary>
    public UnifiedDeck DuplicateDeck(string originalDeckName, string newDeckName)
    {
        var originalDeck = GetDeck(originalDeckName);
        if (originalDeck == null || SavedDecks.Any(d => d.DeckName == newDeckName))
            return null;
            
        var duplicatedDeck = originalDeck.CreateUnifiedDeckCopy();
        duplicatedDeck.DeckName = newDeckName;
        
        AddDeck(duplicatedDeck);
        return duplicatedDeck;
    }
    
    /// <summary>
    /// 自动构建卡组
    /// </summary>
    public UnifiedDeck AutoBuildDeck(string deckName, int targetMonsters = 15, int targetSkills = 10)
    {
        if (SavedDecks.Any(d => d.DeckName == deckName))
            return null;
            
        var newDeck = new UnifiedDeck();
        newDeck.DeckName = deckName;
        
        // 添加随机怪物卡
        if (_cardDatabase != null)
        {
            var availableMonsters = _cardDatabase.GetAllMonsterCards();
            var selectedMonsters = availableMonsters.OrderBy(x => Guid.NewGuid()).Take(targetMonsters);
            
            foreach (var monster in selectedMonsters)
            {
                newDeck.AddMonsterCard(monster);
            }
        }
        
        // 添加随机技能卡
        if (_cardDatabase != null)
        {
            var availableSkills = _cardDatabase.GetAllSkillCards();
            var selectedSkills = availableSkills.OrderBy(x => Guid.NewGuid()).Take(targetSkills);
            
            foreach (var skill in selectedSkills)
            {
                newDeck.AddSkillCard(skill);
            }
        }
        
        AddDeck(newDeck);
        return newDeck;
    }
    
    /// <summary>
    /// 获取所有卡组名称
    /// </summary>
    public List<string> GetDeckNames()
    {
        return SavedDecks.Select(d => d.DeckName).ToList();
    }
    
    /// <summary>
    /// 保存所有卡组
    /// </summary>
    public void SaveAllDecks()
    {
        SaveDecks();
    }
    
    /// <summary>
    /// 保存所有卡组
    /// </summary>
    public void SaveDecks()
    {
        try
        {
            var saveFile = FileAccess.Open(DECKS_SAVE_PATH, FileAccess.ModeFlags.Write);
            if (saveFile != null)
            {
                var saveData = new Godot.Collections.Dictionary<string, Variant>
                {
                    { "decks", SavedDecks },
                    { "current_deck_name", CurrentDeck?.DeckName ?? "" }
                };
                
                saveFile.StoreString(Json.Stringify(saveData));
                saveFile.Close();
                
                GD.Print($"Saved {SavedDecks.Count} decks to {DECKS_SAVE_PATH}");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save decks: {e.Message}");
        }
    }
    
    /// <summary>
    /// 加载所有卡组
    /// </summary>
    public void LoadDecks()
    {
        try
        {
            if (!FileAccess.FileExists(DECKS_SAVE_PATH))
            {
                GD.Print("No saved decks found, will create default deck");
                return;
            }
            
            var saveFile = FileAccess.Open(DECKS_SAVE_PATH, FileAccess.ModeFlags.Read);
            if (saveFile != null)
            {
                var jsonString = saveFile.GetAsText();
                saveFile.Close();
                
                var json = new Json();
                var parseResult = json.Parse(jsonString);
                
                if (parseResult == Error.Ok)
                {
                    var saveData = json.Data.AsGodotDictionary();
                    
                    if (saveData.ContainsKey("decks"))
                    {
                        SavedDecks = saveData["decks"].AsGodotArray<UnifiedDeck>();
                    }
                    
                    if (saveData.ContainsKey("current_deck_name"))
                    {
                        var currentDeckName = saveData["current_deck_name"].AsString();
                        if (!string.IsNullOrEmpty(currentDeckName))
                        {
                            var deck = GetDeck(currentDeckName);
                            if (deck != null)
                            {
                                CurrentDeck = deck;
                            }
                        }
                    }
                    
                    GD.Print($"Loaded {SavedDecks.Count} decks from {DECKS_SAVE_PATH}");
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load decks: {e.Message}");
        }
    }
    
    /// <summary>
    /// 获取所有卡组统计信息
    /// </summary>
    public Godot.Collections.Dictionary<string, Variant> GetAllDecksStats()
    {
        return GetManagerStats();
    }
    
    /// <summary>
    /// 获取卡组管理器统计信息
    /// </summary>
    public Godot.Collections.Dictionary<string, Variant> GetManagerStats()
    {
        var stats = new Godot.Collections.Dictionary<string, Variant>
        {
            { "total_decks", SavedDecks.Count },
            { "current_deck", CurrentDeck?.DeckName ?? "None" }
        };
        
        if (SavedDecks.Count > 0)
        {
            stats["average_deck_size"] = SavedDecks.Average(d => d.Cards.Count);
            stats["largest_deck"] = SavedDecks.Max(d => d.Cards.Count);
            stats["smallest_deck"] = SavedDecks.Min(d => d.Cards.Count);
            
            // 统计各稀有度卡牌使用情况
            var rarityUsage = new Godot.Collections.Dictionary<CardRarity, int>();
            foreach (CardRarity rarity in Enum.GetValues<CardRarity>())
            {
                int count = SavedDecks.Sum(deck => deck.Cards.Count(card => card.Rarity == rarity));
                rarityUsage[rarity] = count;
            }
            stats["rarity_usage"] = rarityUsage;
            
            // 统计卡牌类型分布
            int totalMonsters = SavedDecks.Sum(deck => deck.MonsterCards.Count);
            int totalSkills = SavedDecks.Sum(deck => deck.SkillCards.Count);
            
            stats["total_monsters"] = totalMonsters;
            stats["total_skills"] = totalSkills;
            stats["monster_skill_ratio"] = totalSkills > 0 ? (float)totalMonsters / totalSkills : 0f;
        }
        
        return stats;
    }
    
    /// <summary>
    /// 验证所有卡组
    /// </summary>
    public List<string> ValidateAllDecks()
    {
        var invalidDecks = new List<string>();
        
        foreach (var deck in SavedDecks)
        {
            if (!deck.IsValidDeck())
            {
                invalidDecks.Add(deck.DeckName);
            }
        }
        
        return invalidDecks;
    }
    
    /// <summary>
    /// 清除所有保存的卡组
    /// </summary>
    public void ClearAllDecks()
    {
        SavedDecks.Clear();
        CurrentDeck = null;
        
        // 删除保存文件
        if (FileAccess.FileExists(DECKS_SAVE_PATH))
        {
            DirAccess.RemoveAbsolute(DECKS_SAVE_PATH);
        }
        
        // 重新创建默认卡组
        CreateDefaultDeck();
        
        GD.Print("Cleared all decks and created new default deck");
    }
    
    /// <summary>
    /// 打开指定卡组
    /// </summary>
    public bool OpenDeck(string deckName)
    {
        return SetCurrentDeck(deckName);
    }
    
    /// <summary>
    /// 打开卡组UI
    /// </summary>
    public void OpenDeckUI()
    {
        // 创建统一卡组UI场景
        var deckUIScene = GD.Load<PackedScene>("res://Scenes/UI/UnifiedDeckUI.tscn");
        if (deckUIScene != null)
        {
            var deckUI = deckUIScene.Instantiate();
            GetTree().CurrentScene.AddChild(deckUI);
        }
        else
        {
            GD.PrintErr("UnifiedDeckUI scene not found");
        }
    }
}