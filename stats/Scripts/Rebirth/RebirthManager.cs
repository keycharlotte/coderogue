using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Rebirth;
using CodeRogue.Rebirth.Data;

/// <summary>
/// 死亡重来系统主管理器
/// </summary>
public partial class RebirthManager : Node
{
    private PlayerData _playerData;
    private RebirthCurrency _playerCurrency;
    private List<GameSession> _sessionHistory = new();
    private NoveltyCalculator _noveltyCalculator;
    private CurrencyConverter _currencyConverter;
    private RebirthShopManager _shopManager;
    
    [Signal] public delegate void GameSessionCompletedEventHandler(GameSession session, int currencyEarned);
    [Signal] public delegate void PlayerLevelUpEventHandler(int newLevel, int experienceGained);
    [Signal] public delegate void NoveltyBonusEarnedEventHandler(float multiplier, NoveltyLevel level);
    [Signal] public delegate void RebirthSystemInitializedEventHandler();
    
    public PlayerData PlayerData => _playerData;
    public RebirthCurrency PlayerCurrency => _playerCurrency;
    public RebirthShopManager ShopManager => _shopManager;
    
    public override void _Ready()
    {
        InitializeComponents();
        LoadPlayerData();
        ConnectSignals();
        
        EmitSignal(SignalName.RebirthSystemInitialized);
        GD.Print("死亡重来系统初始化完成");
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        _noveltyCalculator = GetNode<NoveltyCalculator>("NoveltyCalculator");
        _currencyConverter = GetNode<CurrencyConverter>("CurrencyConverter");
        _shopManager = GetNode<RebirthShopManager>("RebirthShopManager");
    }
    
    /// <summary>
    /// 连接信号
    /// </summary>
    private void ConnectSignals()
    {
        _shopManager.ShopItemPurchased += OnShopItemPurchased;
        _shopManager.CurrencyUpdated += OnCurrencyUpdated;
        _currencyConverter.CurrencyCalculated += OnCurrencyCalculated;
    }
    
    /// <summary>
    /// 加载玩家数据
    /// </summary>
    private void LoadPlayerData()
    {
        // 尝试从存档加载，如果没有则创建新玩家
        _playerData = LoadPlayerDataFromSave() ?? CreateNewPlayer();
        _playerCurrency = LoadPlayerCurrencyFromSave() ?? CreateNewCurrency();
        
        // 设置到子管理器
        _shopManager.SetPlayerData(_playerData);
        _shopManager.SetPlayerCurrency(_playerCurrency);
        
        GD.Print($"玩家数据加载完成: {_playerData.PlayerName}, 等级: {_playerData.PlayerLevel}, 货币: {_playerCurrency.TotalCurrency}");
    }
    
    /// <summary>
    /// 从存档加载玩家数据
    /// </summary>
    private PlayerData LoadPlayerDataFromSave()
    {
        // 这里应该从实际的存档系统加载
        // 暂时返回null，表示没有存档
        return null;
    }
    
    /// <summary>
    /// 从存档加载货币数据
    /// </summary>
    private RebirthCurrency LoadPlayerCurrencyFromSave()
    {
        // 这里应该从实际的存档系统加载
        // 暂时返回null，表示没有存档
        return null;
    }
    
    /// <summary>
    /// 创建新玩家
    /// </summary>
    private PlayerData CreateNewPlayer()
    {
        var playerId = Guid.NewGuid().ToString();
        return new PlayerData
        {
            PlayerId = playerId,
            PlayerName = "新玩家",
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            LastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            PlayerLevel = 1,
            Experience = 0,
            UnlockedCards = new Godot.Collections.Array<string> { "basic_attack", "basic_defend", "basic_skill" },
            UnlockedHeroes = new Godot.Collections.Array<string> { "default_hero" },
            UnlockedRelics = new Godot.Collections.Array<string>(),
            Statistics = new Godot.Collections.Dictionary<string, int>
            {
                { StatisticType.TotalGames.ToString(), 0 },
                { StatisticType.TotalVictories.ToString(), 0 },
                { StatisticType.TotalCurrency.ToString(), 0 },
                { StatisticType.NoveltyBonus.ToString(), 0 },
                { StatisticType.BestFloor.ToString(), 0 },
                { StatisticType.PlayTime.ToString(), 0 },
                // WinRate will be calculated dynamically
                // AverageFloorReached will be calculated dynamically
                // TotalEnemiesDefeated will be tracked separately
            }
        };
    }
    
    /// <summary>
    /// 创建新货币数据
    /// </summary>
    private RebirthCurrency CreateNewCurrency()
    {
        return new RebirthCurrency
        {
            PlayerId = _playerData.PlayerId,
            TotalCurrency = 0,
            LifetimeEarned = 0,
            LifetimeSpent = 0,
            LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            CurrencyType = CurrencyType.RebirthCoin
        };
    }
    
    /// <summary>
    /// 处理游戏会话完成
    /// </summary>
    public void OnGameSessionCompleted(string heroId, int floorsReached, int enemiesDefeated, 
        bool isVictory, List<string> usedCardIds, List<string> usedRelicIds, 
        DateTime startTime, DateTime endTime)
    {
        // 创建游戏会话数据
        var session = CreateGameSession(heroId, floorsReached, enemiesDefeated, 
            isVictory, usedCardIds, usedRelicIds, startTime, endTime);
        
        // 计算新颖度
        var noveltyDb = GetNode<NoveltyDatabase>("/root/NoveltyDatabase");
        var noveltyMultiplier = _noveltyCalculator.CalculateNoveltyMultiplier(session, noveltyDb);
        
        session.NoveltyMultiplier = noveltyMultiplier;
        session.NoveltyLevel = NoveltyLevel.Common; // 默认值，应该从NoveltyCalculator获取
        
        // 计算货币奖励
        var baseCurrency = _currencyConverter.CalculateBaseCurrency(session);
        var finalCurrency = _currencyConverter.CalculateFinalCurrency(baseCurrency, noveltyMultiplier);
        
        session.BaseCurrencyEarned = baseCurrency;
        session.TotalCurrencyEarned = finalCurrency;
        
        // 更新玩家货币
        _playerCurrency.AddCurrency((int)finalCurrency);
        _playerCurrency.AddCurrency((int)(finalCurrency - baseCurrency), true); // 新颖度奖励
        
        // 计算经验值
        var experienceGained = CalculateExperienceGained(session);
        bool leveledUp = _playerData.AddExperience(experienceGained);
        
        // 更新玩家统计
        UpdatePlayerStatistics(session);
        
        // 添加到会话历史
        _sessionHistory.Add(session);
        _playerData.AddGameSession(session.IsVictory, session.FloorsReached, (float)session.GetGameDurationMinutes(), (int)session.TotalCurrencyEarned);
        
        // 保存数据
        SavePlayerData();
        
        // 发送信号
        EmitSignal(SignalName.GameSessionCompleted, session, finalCurrency);
        
        if (leveledUp)
        {
            EmitSignal(SignalName.PlayerLevelUp, _playerData.PlayerLevel, experienceGained);
        }
        
        if (noveltyMultiplier > 1.0f)
        {
            EmitSignal(SignalName.NoveltyBonusEarned, noveltyMultiplier, (int)session.NoveltyLevel);
        }
        
        GD.Print($"游戏会话完成 - 层数: {floorsReached}, 货币: {finalCurrency}, 新颖度: {noveltyMultiplier:F2}x");
    }
    
    /// <summary>
    /// 创建游戏会话
    /// </summary>
    private GameSession CreateGameSession(string heroId, int floorsReached, int enemiesDefeated,
        bool isVictory, List<string> usedCardIds, List<string> usedRelicIds,
        DateTime startTime, DateTime endTime)
    {
        var session = new GameSession
        {
            SessionId = Guid.NewGuid().ToString(),
            HeroId = heroId,
            FloorsReached = floorsReached,
            EnemiesDefeated = enemiesDefeated,
            IsVictory = isVictory,
            StartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
            EndTime = endTime.ToString("yyyy-MM-dd HH:mm:ss"),
            ResultType = isVictory ? GameResultType.Victory : GameResultType.Defeat,
            UsedCards = new Godot.Collections.Array<UsedCard>(),
            UsedRelics = new Godot.Collections.Array<UsedRelic>()
        };
        
        // 创建使用的卡牌记录
        foreach (var cardId in usedCardIds)
        {
            session.UsedCards.Add(new UsedCard
            {
                CardId = cardId,
                UsageCount = 1, // 这里应该从实际游戏数据获取
                EffectivenessScore = 5.0f, // 默认值，应该从实际数据计算
                DamageDealt = 100, // 默认值
                TimesPlayed = 1,
                AverageImpact = 5.0f
            });
        }
        
        // 创建使用的遗物记录
        foreach (var relicId in usedRelicIds)
        {
            session.UsedRelics.Add(new UsedRelic
            {
                RelicId = relicId,
                ActivationCount = 5, // 默认值，应该从实际数据获取
                ImpactScore = 7.0f,
                BenefitProvided = 50,
                UpTime = 80f,
                SynergyBonus = 10
            });
        }
        
        return session;
    }
    
    /// <summary>
    /// 计算经验值获得
    /// </summary>
    private int CalculateExperienceGained(GameSession session)
    {
        int baseExp = 10;
        int floorExp = session.FloorsReached * 5;
        int enemyExp = session.EnemiesDefeated * 2;
        int victoryExp = session.IsVictory ? 50 : 0;
        
        // 新颖度奖励经验
        float noveltyExpBonus = session.NoveltyMultiplier > 1.0f ? 
            (session.NoveltyMultiplier - 1.0f) * 30f : 0f;
        
        int totalExp = baseExp + floorExp + enemyExp + victoryExp + (int)noveltyExpBonus;
        return Math.Max(1, totalExp);
    }
    
    /// <summary>
    /// 更新玩家统计
    /// </summary>
    private void UpdatePlayerStatistics(GameSession session)
    {
        _playerData.UpdateStatistic(StatisticType.TotalGames.ToString(), 1);
        _playerData.UpdateStatistic(StatisticType.TotalCurrency.ToString(), (int)session.TotalCurrencyEarned);
        // Track enemies defeated separately - not in StatisticType enum
        _playerData.UpdateStatistic(StatisticType.PlayTime.ToString(), (int)session.GetGameDurationMinutes());
        
        if (session.IsVictory)
        {
            _playerData.UpdateStatistic(StatisticType.TotalVictories.ToString(), 1);
        }
        
        if (session.FloorsReached > _playerData.GetStatistic(StatisticType.BestFloor.ToString()))
        {
            _playerData.UpdateStatistic(StatisticType.BestFloor.ToString(), session.FloorsReached);
        }
        
        // 更新平均值统计
        var totalGames = _playerData.GetStatistic(StatisticType.TotalGames.ToString());
        var totalVictories = _playerData.GetStatistic(StatisticType.TotalVictories.ToString());
        var totalPlayTime = _playerData.GetStatistic(StatisticType.PlayTime.ToString());
        
        // Calculate win rate and average floor dynamically - not stored in StatisticType enum
        // Removed SetStatistic call as method doesn't exist
        
        var totalFloorsReached = _sessionHistory.Sum(s => s.FloorsReached) + session.FloorsReached;
        // Calculate average floor reached dynamically - not stored in StatisticType enum
    }
    
    /// <summary>
    /// 保存玩家数据
    /// </summary>
    private void SavePlayerData()
    {
        // 这里应该保存到实际的存档系统
        // 暂时只打印日志
        GD.Print("玩家数据已保存");
    }
    
    /// <summary>
    /// 获取会话历史
    /// </summary>
    public List<GameSession> GetSessionHistory(int count = 10)
    {
        return _sessionHistory
            .OrderByDescending(s => s.EndTime)
            .Take(count)
            .ToList();
    }
    
    /// <summary>
    /// 获取最佳会话
    /// </summary>
    public GameSession GetBestSession()
    {
        return _sessionHistory
            .OrderByDescending(s => s.FloorsReached)
            .ThenByDescending(s => s.TotalCurrencyEarned)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// 获取统计摘要
    /// </summary>
    public Dictionary<string, object> GetStatisticsSummary()
    {
        return new Dictionary<string, object>
        {
            ["player_level"] = _playerData.PlayerLevel,
            ["total_currency"] = _playerCurrency.TotalCurrency,
            ["total_games"] = _playerData.GetStatistic(StatisticType.TotalGames.ToString()),
            ["win_rate"] = _playerData.GetStatistic(StatisticType.TotalVictories.ToString()) / _playerData.GetStatistic(StatisticType.TotalGames.ToString()) * 100f, // Calculate dynamically
            ["highest_floor"] = _playerData.GetStatistic(StatisticType.BestFloor.ToString()),
            ["total_play_time"] = _playerData.GetStatistic(StatisticType.PlayTime.ToString()),
            ["unlocked_heroes"] = _playerData.UnlockedHeroes.Count,
            ["unlocked_cards"] = _playerData.UnlockedCards.Count,
            ["unlocked_relics"] = _playerData.UnlockedRelics.Count
        };
    }
    
    /// <summary>
    /// 重置玩家进度
    /// </summary>
    public void ResetPlayerProgress(bool keepUnlocks = false)
    {
        if (!keepUnlocks)
        {
            _playerData = CreateNewPlayer();
        }
        else
        {
            // 保留解锁内容，重置其他数据
            var unlockedCards = _playerData.UnlockedCards;
            var unlockedHeroes = _playerData.UnlockedHeroes;
            var unlockedRelics = _playerData.UnlockedRelics;
            
            _playerData = CreateNewPlayer();
            _playerData.UnlockedCards = unlockedCards;
            _playerData.UnlockedHeroes = unlockedHeroes;
            _playerData.UnlockedRelics = unlockedRelics;
        }
        
        _playerCurrency = CreateNewCurrency();
        _sessionHistory.Clear();
        
        _shopManager.SetPlayerData(_playerData);
        _shopManager.SetPlayerCurrency(_playerCurrency);
        
        SavePlayerData();
        GD.Print("玩家进度已重置");
    }
    
    /// <summary>
    /// 商店物品购买事件处理
    /// </summary>
    private void OnShopItemPurchased(ShopItem item, PurchaseRecord record)
    {
        GD.Print($"购买了商店物品: {item.Name}");
        SavePlayerData();
    }
    
    /// <summary>
    /// 货币更新事件处理
    /// </summary>
    private void OnCurrencyUpdated(RebirthCurrency currency)
    {
        _playerCurrency = currency;
    }
    
    /// <summary>
    /// 货币计算事件处理
    /// </summary>
    private void OnCurrencyCalculated(float baseCurrency, float finalCurrency, float multiplier)
    {
        GD.Print($"货币计算完成 - 基础: {baseCurrency}, 最终: {finalCurrency}, 倍率: {multiplier:F2}x");
    }
    
    /// <summary>
    /// 获取新颖度历史
    /// </summary>
    public List<NoveltyRecord> GetNoveltyHistory()
    {
        return _noveltyCalculator.GetNoveltyHistory();
    }
    
    /// <summary>
    /// 预测组合新颖度
    /// </summary>
    public float PredictCombinationNovelty(string heroId, List<string> cardIds, List<string> relicIds)
    {
        return _noveltyCalculator.PredictCombinationPotential(heroId, cardIds, relicIds);
    }
    
    /// <summary>
    /// 获取推荐组合
    /// </summary>
    public List<string> GetRecommendedCombinations(int count = 5)
    {
        // GetRecommendedCombinations method not implemented yet
        return new List<string>();
    }
}