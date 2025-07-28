using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 怪物游戏管理器
/// 整合所有怪物系统组件，提供统一的游戏逻辑接口
/// </summary>
[GlobalClass]
public partial class MonsterGameManager : Node
{
    [Signal] public delegate void GameStateChangedEventHandler(GameState newState);
    [Signal] public delegate void BattleStartedEventHandler();
    [Signal] public delegate void BattleEndedEventHandler(bool victory);
    [Signal] public delegate void SummonerChangedEventHandler(SummonerHero newSummoner);
    [Signal] public delegate void GameProgressUpdatedEventHandler(float progress);
    
    // 游戏状态
    public enum GameState
    {
        MainMenu,
        DeckBuilding,
        Battle,
        CardCollection,
        Settings
    }
    
    [Export] public GameState CurrentState { get; private set; } = GameState.MainMenu;
    [Export] public SummonerHero CurrentSummoner { get; private set; }
    [Export] public float GameProgress { get; private set; } = 0f; // 0-1 游戏进度
    
    // 系统组件引用
    private SummonSystem _summonSystem;
    private TypingCombatSystem _typingCombatSystem;
    private MonsterManager _cardManager;
    private DeckManager _deckManager;
    
    // 战斗状态
    private bool _inBattle = false;
    private float _battleStartTime;
    private Godot.Collections.Dictionary<string, float> _battleStats = new Godot.Collections.Dictionary<string, float>();
    
    // 游戏配置
    [Export] public Godot.Collections.Dictionary<string, Variant> GameConfig { get; set; } = new Godot.Collections.Dictionary<string, Variant>
    {
        { "auto_save_interval", 30f }, // 自动保存间隔（秒）
        { "max_summoned_monsters", 7 },
        { "starting_summon_points", 10 },
        { "typing_damage_decay_rate", 0.02f },
        { "monster_synergy_multiplier", 1.0f }
    };
    
    public override void _Ready()
    {
        InitializeComponents();
        InitializeDefaultSummoner();
        SetupAutoSave();
        
        GD.Print("MonsterGameManager initialized");
    }
    
    /// <summary>
    /// 初始化系统组件
    /// </summary>
    private void InitializeComponents()
    {
        // 获取或创建系统组件
        _summonSystem = GetNodeOrCreate<SummonSystem>("SummonSystem");
        _typingCombatSystem = GetNodeOrCreate<TypingCombatSystem>("TypingCombatSystem");
        _cardManager = GetNodeOrCreate<MonsterManager>("MonsterManager");
        _deckManager = GetNodeOrCreate<DeckManager>("DeckManager");
        
        // 连接信号
        ConnectSystemSignals();
        
        // 应用配置
        ApplyGameConfig();
    }
    
    /// <summary>
    /// 获取节点或创建新节点
    /// </summary>
    private T GetNodeOrCreate<T>(string nodeName) where T : Node, new()
    {
        var node = GetNodeOrNull<T>(nodeName);
        if (node == null)
        {
            node = new T();
            node.Name = nodeName;
            AddChild(node);
            GD.Print($"Created {nodeName} component");
        }
        return node;
    }
    
    /// <summary>
    /// 连接系统信号
    /// </summary>
    private void ConnectSystemSignals()
    {
        // 召唤系统信号
        if (_summonSystem != null)
        {
            _summonSystem.MonsterSummoned += OnMonsterSummoned;
            _summonSystem.MonsterReplaced += OnMonsterReplaced;
            _summonSystem.BondsUpdated += OnBondsUpdated;
        }
        
        // 打字战斗系统信号
        if (_typingCombatSystem != null)
        {
            _typingCombatSystem.DamageCalculated += OnDamageCalculated;
            _typingCombatSystem.SynergyTriggered += OnSynergyTriggered;
            _typingCombatSystem.TypingStatsUpdated += OnTypingStatsUpdated;
        }
        
        // 卡牌管理器信号
        if (_cardManager != null)
        {
            _cardManager.NewCardObtained += OnNewCardObtained;
        }
        
        // 卡组管理器信号
        if (_deckManager != null)
        {
            _deckManager.DeckChanged += OnCurrentDeckChanged;
        }
    }
    
    /// <summary>
    /// 应用游戏配置
    /// </summary>
    private void ApplyGameConfig()
    {
        if (_summonSystem != null && GameConfig.ContainsKey("starting_summon_points"))
        {
            _summonSystem.MaxSummonPoints = GameConfig["starting_summon_points"].AsInt32();
            _summonSystem.CurrentSummonPoints = GameConfig["starting_summon_points"].AsInt32();
        }
        
        if (_typingCombatSystem != null && GameConfig.ContainsKey("typing_damage_decay_rate"))
        {
            _typingCombatSystem.BaseDecayRate = GameConfig["typing_damage_decay_rate"].AsSingle();
        }
    }
    
    /// <summary>
    /// 初始化默认召唤师
    /// </summary>
    private void InitializeDefaultSummoner()
    {
        // 创建默认召唤师
        var defaultSummoner = CreateDefaultSummoner();
        SetCurrentSummoner(defaultSummoner);
    }
    
    /// <summary>
    /// 创建默认召唤师
    /// </summary>
    private SummonerHero CreateDefaultSummoner()
    {
        var summoner = new SummonerHero();
        summoner.Id = 1;
        summoner.HeroName = "新手召唤师";
        summoner.Description = "刚开始冒险的召唤师";
        summoner.PrimaryColor = MagicColor.White;
        summoner.TypingDamageBase = 10f;
        summoner.TypingSpeedBonus = 0.1f;
        summoner.TypingAccuracyBonus = 0.05f;
        summoner.TypingDecayResistance = 0.1f;
        
        // 初始化颜色槽位
        summoner.InitializeDefaultColorSlots();
        
        // 添加基础技能
        summoner.AddSummonerSkill(SummonerSkillType.TypingEnhancement, 0.1f);
        summoner.AddSummonerSkill(SummonerSkillType.SummonBonus, 0.05f);
        
        return summoner;
    }
    
    /// <summary>
    /// 设置当前召唤师
    /// </summary>
    public void SetCurrentSummoner(SummonerHero summoner)
    {
        if (summoner == null)
            return;
            
        CurrentSummoner = summoner;
        
        // 更新各系统的召唤师引用
        _summonSystem?.SetCurrentSummoner(summoner);
        _typingCombatSystem?.SetCurrentSummoner(summoner);
        _cardManager?.SetCurrentSummoner(summoner);
        
        EmitSignal(SignalName.SummonerChanged, summoner);
        
        GD.Print($"Current summoner set to: {summoner.HeroName}");
    }
    
    /// <summary>
    /// 改变游戏状态
    /// </summary>
    public void ChangeGameState(GameState newState)
    {
        if (CurrentState == newState)
            return;
            
        var oldState = CurrentState;
        CurrentState = newState;
        
        // 处理状态转换
        OnGameStateChanged(oldState, newState);
        
        EmitSignal(SignalName.GameStateChanged, (int)newState);
        
        GD.Print($"Game state changed from {oldState} to {newState}");
    }
    
    /// <summary>
    /// 处理游戏状态改变
    /// </summary>
    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Battle:
                StartBattle();
                break;
            case GameState.DeckBuilding:
                // 可以在这里添加卡组构建相关逻辑
                break;
            case GameState.CardCollection:
                // 可以在这里添加卡牌收集相关逻辑
                break;
        }
        
        // 离开战斗状态时的清理
        if (oldState == GameState.Battle && newState != GameState.Battle)
        {
            EndBattle(false);
        }
    }
    
    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle()
    {
        if (_inBattle)
            return;
            
        _inBattle = true;
        _battleStartTime = (float)Time.GetUnixTimeFromSystem();
        _battleStats.Clear();
        
        // 重置战斗相关系统
        _typingCombatSystem?.ResetTypingStats();
        _summonSystem?.ClearAllMonsters();
        
        // 恢复召唤点数
        if (_summonSystem != null)
        {
            _summonSystem.CurrentSummonPoints = _summonSystem.MaxSummonPoints;
        }
        
        EmitSignal(SignalName.BattleStarted);
        
        GD.Print("Battle started");
    }
    
    /// <summary>
    /// 结束战斗
    /// </summary>
    public void EndBattle(bool victory)
    {
        if (!_inBattle)
            return;
            
        _inBattle = false;
        
        // 计算战斗统计
        float battleDuration = (float)Time.GetUnixTimeFromSystem() - _battleStartTime;
        _battleStats["duration"] = battleDuration;
        _battleStats["victory"] = victory ? 1f : 0f;
        
        if (_typingCombatSystem != null)
        {
            _battleStats["total_characters"] = _typingCombatSystem.TotalCharactersTyped;
            _battleStats["final_wpm"] = _typingCombatSystem.CurrentWPM;
            _battleStats["final_accuracy"] = _typingCombatSystem.CurrentAccuracy;
        }
        
        if (_summonSystem != null)
        {
            _battleStats["monsters_summoned"] = _summonSystem.GetSummonedCount();
        }
        
        // 更新游戏进度
        if (victory)
        {
            UpdateGameProgress(0.01f); // 每次胜利增加1%进度
        }
        
        EmitSignal(SignalName.BattleEnded, victory);
        
        GD.Print($"Battle ended - Victory: {victory}, Duration: {battleDuration:F1}s");
    }
    
    /// <summary>
    /// 处理打字输入
    /// </summary>
    public float ProcessTypingInput(string inputText)
    {
        if (!_inBattle || _typingCombatSystem == null)
            return 0f;
            
        float currentTime = (float)Time.GetUnixTimeFromSystem();
        return _typingCombatSystem.ProcessTypingInput(inputText, currentTime);
    }
    
    /// <summary>
    /// 召唤怪物
    /// </summary>
    public bool SummonMonster(MonsterCard monster, int position = -1)
    {
        if (!_inBattle || _summonSystem == null)
            return false;
            
        return _summonSystem.SummonMonster(monster, position);
    }
    
    /// <summary>
    /// 获取可召唤的卡牌
    /// </summary>
    public List<MonsterCard> GetAvailableCards()
    {
        var currentDeck = _deckManager?.GetCurrentDeck();
        if (currentDeck == null || _summonSystem == null)
            return new List<MonsterCard>();
            
        return currentDeck.GetAffordableCards(_summonSystem.CurrentSummonPoints);
    }
    
    /// <summary>
    /// 更新游戏进度
    /// </summary>
    private void UpdateGameProgress(float increment)
    {
        GameProgress = Mathf.Clamp(GameProgress + increment, 0f, 1f);
        EmitSignal(SignalName.GameProgressUpdated, GameProgress);
    }
    
    /// <summary>
    /// 设置自动保存
    /// </summary>
    private void SetupAutoSave()
    {
        if (GameConfig.ContainsKey("auto_save_interval"))
        {
            float interval = GameConfig["auto_save_interval"].AsSingle();
            var timer = new Timer();
            timer.WaitTime = interval;
            timer.Timeout += AutoSave;
            timer.Autostart = true;
            AddChild(timer);
        }
    }
    
    /// <summary>
    /// 自动保存
    /// </summary>
    private void AutoSave()
    {
        _deckManager?.SaveAllDecks();
        // 这里可以添加其他需要保存的数据
        GD.Print("Auto-saved game data");
    }
    
    /// <summary>
    /// 获取游戏统计
    /// </summary>
    public Godot.Collections.Dictionary<string, Variant> GetGameStats()
    {
        var stats = new Godot.Collections.Dictionary<string, Variant>
        {
            { "game_progress", GameProgress },
            { "current_state", CurrentState.ToString() },
            { "in_battle", _inBattle },
            { "current_summoner", CurrentSummoner?.HeroName ?? "None" }
        };
        
        // 添加各系统统计
        if (_cardManager != null)
        {
            var collectionStats = _cardManager.GetCollectionStats();
            foreach (var kvp in collectionStats)
            {
                stats[$"collection_{kvp.Key}"] = kvp.Value;
            }
        }
        
        if (_deckManager != null)
        {
            var deckStats = _deckManager.GetAllDecksStats();
            foreach (var kvp in deckStats)
            {
                stats[$"deck_{kvp.Key}"] = kvp.Value;
            }
        }
        
        if (_inBattle)
        {
            foreach (var kvp in _battleStats)
            {
                stats[$"battle_{kvp.Key}"] = kvp.Value;
            }
        }
        
        return stats;
    }
    
    // 信号处理方法
    private void OnMonsterSummoned(MonsterCard monster, int position)
    {
        GD.Print($"Monster summoned: {monster.MonsterName} at position {position}");
    }
    
    private void OnMonsterReplaced(MonsterCard oldMonster, MonsterCard newMonster, int position)
    {
        GD.Print($"Monster replaced: {oldMonster.MonsterName} -> {newMonster.MonsterName} at position {position}");
    }
    
    private void OnBondsUpdated(Godot.Collections.Dictionary<BondType, int> activeBonds)
    {
        if (activeBonds.Count > 0)
        {
            var bondInfo = string.Join(", ", activeBonds.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            GD.Print($"Bonds updated: {bondInfo}");
        }
    }
    
    private void OnDamageCalculated(float totalDamage, float typingDamage, float monsterDamage)
    {
        if (_inBattle)
        {
            if (!_battleStats.ContainsKey("total_damage"))
                _battleStats["total_damage"] = 0f;
            if (!_battleStats.ContainsKey("typing_damage"))
                _battleStats["typing_damage"] = 0f;
            if (!_battleStats.ContainsKey("monster_damage"))
                _battleStats["monster_damage"] = 0f;
                
            _battleStats["total_damage"] += totalDamage;
            _battleStats["typing_damage"] += typingDamage;
            _battleStats["monster_damage"] += monsterDamage;
        }
    }
    
    private void OnSynergyTriggered(MonsterCard monster, float synergyBonus)
    {
        GD.Print($"Synergy triggered: {monster.MonsterName} (+{synergyBonus:P0})");
    }
    
    private void OnTypingStatsUpdated(float wpm, float accuracy)
    {
        // 可以在这里更新UI或其他系统
    }
    
    private void OnNewCardObtained(MonsterCard card)
    {
        GD.Print($"New card obtained: {card.MonsterName} ({card.Rarity})");
    }
    
    private void OnCurrentDeckChanged(UnifiedDeck newDeck)
    {
        GD.Print($"Current deck changed to: {newDeck?.DeckName ?? "None"}");
    }
    
    /// <summary>
    /// 获取系统组件引用（供外部访问）
    /// </summary>
    public SummonSystem GetSummonSystem() => _summonSystem;
    public TypingCombatSystem GetTypingCombatSystem() => _typingCombatSystem;
    public MonsterManager GetCardManager() => _cardManager;
    public DeckManager GetDeckManager() => _deckManager;
}