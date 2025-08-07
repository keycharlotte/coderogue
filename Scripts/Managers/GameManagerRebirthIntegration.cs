using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Rebirth;
using CodeRogue.Rebirth.Data;
using CodeRogue.Core;

/// <summary>
/// GameManager的死亡重来系统集成扩展
/// </summary>
public partial class GameManagerRebirthIntegration : Node
{
    private RebirthManager _rebirthManager;
    private RebirthResultsUI _resultsUI;
    private RebirthShopUI _shopUI;
    private GameManager _gameManager;
    
    // 当前游戏会话数据
    private DateTime _gameStartTime;
    private string _currentHeroId;
    private List<string> _usedCardIds = new();
    private List<string> _usedRelicIds = new();
    
    [Signal] public delegate void RebirthSystemReadyEventHandler();
    [Signal] public delegate void GameSessionStartedEventHandler();
    [Signal] public delegate void GameSessionEndedEventHandler(GameSession session);
    
    public override void _Ready()
    {
        // 获取必要的组件引用
        _gameManager = GetParent<GameManager>();
        
        // 初始化死亡重来系统
        InitializeRebirthSystem();
        
        // 连接游戏管理器信号
        ConnectGameManagerSignals();
        
        GD.Print("死亡重来系统集成初始化完成");
    }
    
    /// <summary>
    /// 初始化死亡重来系统
    /// </summary>
    private void InitializeRebirthSystem()
    {
        // 加载死亡重来系统场景
        var rebirthSystemScene = GD.Load<PackedScene>("res://Scenes/RebirthSystem.tscn");
        var rebirthSystemInstance = rebirthSystemScene.Instantiate();
        GetTree().Root.AddChild(rebirthSystemInstance);
        
        _rebirthManager = rebirthSystemInstance as RebirthManager;
        
        // 加载UI组件
        InitializeUI();
        
        // 连接死亡重来系统信号
        ConnectRebirthSignals();
        
        EmitSignal(SignalName.RebirthSystemReady);
    }
    
    /// <summary>
    /// 初始化UI组件
    /// </summary>
    private void InitializeUI()
    {
        // 加载结算UI
        var resultsScene = GD.Load<PackedScene>("res://Scenes/UI/RebirthResultsUI.tscn");
        _resultsUI = resultsScene.Instantiate<RebirthResultsUI>();
        GetTree().Root.AddChild(_resultsUI);
        
        // 获取商店UI引用
        _shopUI = _rebirthManager?.GetNode<RebirthShopUI>("UI/RebirthShop");
        
        // 连接UI信号
        if (_resultsUI != null)
        {
            _resultsUI.ShopRequested += OnShopRequested;
            _resultsUI.ContinueGameRequested += OnContinueGameRequested;
            _resultsUI.ResultsClosed += OnResultsClosed;
        }
        
        if (_shopUI != null)
        {
            _shopUI.ShopClosed += OnShopClosed;
            _shopUI.ItemPurchased += OnShopItemPurchased;
        }
    }
    
    /// <summary>
    /// 连接游戏管理器信号
    /// </summary>
    private void ConnectGameManagerSignals()
    {
        if (_gameManager != null)
        {
            // 这些信号需要在实际的GameManager中定义
            // _gameManager.GameStarted += OnGameStarted;
            // _gameManager.GameEnded += OnGameEnded;
            // _gameManager.HeroSelected += OnHeroSelected;
            // _gameManager.CardUsed += OnCardUsed;
            // _gameManager.RelicObtained += OnRelicObtained;
        }
    }
    
    /// <summary>
    /// 连接死亡重来系统信号
    /// </summary>
    private void ConnectRebirthSignals()
    {
        if (_rebirthManager != null)
        {
            _rebirthManager.GameSessionCompleted += OnRebirthSessionCompleted;
            _rebirthManager.PlayerLevelUp += OnPlayerLevelUp;
            _rebirthManager.NoveltyBonusEarned += OnNoveltyBonusEarned;
        }
    }
    
    /// <summary>
    /// 游戏开始事件处理
    /// </summary>
    public void OnGameStarted(string heroId)
    {
        _gameStartTime = DateTime.Now;
        _currentHeroId = heroId;
        _usedCardIds.Clear();
        _usedRelicIds.Clear();
        
        EmitSignal(SignalName.GameSessionStarted);
        GD.Print($"游戏会话开始 - 英雄: {heroId}");
    }
    
    /// <summary>
    /// 游戏结束事件处理
    /// </summary>
    public void OnGameEnded(bool isVictory, int floorsReached, int enemiesDefeated)
    {
        var endTime = DateTime.Now;
        
        // 处理游戏会话完成
        _rebirthManager?.OnGameSessionCompleted(
            _currentHeroId,
            floorsReached,
            enemiesDefeated,
            isVictory,
            _usedCardIds,
            _usedRelicIds,
            _gameStartTime,
            endTime
        );
        
        GD.Print($"游戏会话结束 - 胜利: {isVictory}, 层数: {floorsReached}");
    }
    
    /// <summary>
    /// 英雄选择事件处理
    /// </summary>
    public void OnHeroSelected(string heroId)
    {
        _currentHeroId = heroId;
        GD.Print($"选择英雄: {heroId}");
    }
    
    /// <summary>
    /// 卡牌使用事件处理
    /// </summary>
    public void OnCardUsed(string cardId)
    {
        if (!_usedCardIds.Contains(cardId))
        {
            _usedCardIds.Add(cardId);
        }
    }
    
    /// <summary>
    /// 遗物获得事件处理
    /// </summary>
    public void OnRelicObtained(string relicId)
    {
        if (!_usedRelicIds.Contains(relicId))
        {
            _usedRelicIds.Add(relicId);
        }
    }
    
    /// <summary>
    /// 死亡重来会话完成事件处理
    /// </summary>
    private void OnRebirthSessionCompleted(GameSession session, int currencyEarned)
    {
        // 计算经验值
        var experienceGained = CalculateExperienceGained(session);
        var currentLevel = _rebirthManager.PlayerData.PlayerLevel;
        
        // 显示结算界面
        _resultsUI?.ShowResults(session, experienceGained, false, currentLevel);
        
        EmitSignal(SignalName.GameSessionEnded, session);
        GD.Print($"死亡重来会话完成 - 货币: {currencyEarned}, 经验: {experienceGained}");
    }
    
    /// <summary>
    /// 玩家升级事件处理
    /// </summary>
    private void OnPlayerLevelUp(int newLevel, int experienceGained)
    {
        GD.Print($"玩家升级到等级 {newLevel}!");
        
        // 可以在这里添加升级特效或通知
        ShowLevelUpNotification(newLevel);
    }
    
    /// <summary>
    /// 新颖度奖励事件处理
    /// </summary>
    private void OnNoveltyBonusEarned(float multiplier, NoveltyLevel level)
    {
        GD.Print($"获得新颖度奖励: {multiplier:F2}x ({level})");
        
        // 可以在这里添加新颖度奖励特效
        ShowNoveltyBonusNotification(multiplier, level);
    }
    
    /// <summary>
    /// 商店请求事件处理
    /// </summary>
    private void OnShopRequested()
    {
        _shopUI?.ShowShop();
        _resultsUI?.HideResults();
    }
    
    /// <summary>
    /// 继续游戏请求事件处理
    /// </summary>
    private void OnContinueGameRequested()
    {
        // 返回主菜单或重新开始游戏
        _gameManager?.ReturnToMainMenu();
    }
    
    /// <summary>
    /// 结算界面关闭事件处理
    /// </summary>
    private void OnResultsClosed()
    {
        // 可以在这里添加额外的清理逻辑
    }
    
    /// <summary>
    /// 商店关闭事件处理
    /// </summary>
    private void OnShopClosed()
    {
        // 商店关闭后可以返回结算界面或主菜单
        _resultsUI?.Show();
    }
    
    /// <summary>
    /// 商店物品购买事件处理
    /// </summary>
    private void OnShopItemPurchased(ShopItem item)
    {
        GD.Print($"购买了商店物品: {item.Name}");
        
        // 可以在这里添加购买特效或应用物品效果
        ApplyShopItemEffect(item);
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
    /// 显示升级通知
    /// </summary>
    private void ShowLevelUpNotification(int newLevel)
    {
        // 创建临时的升级通知UI
        var notification = new Label();
        notification.Text = $"升级到等级 {newLevel}!";
        notification.AddThemeStyleboxOverride("normal", CreateNotificationStyleBox());
        notification.Position = new Vector2(GetViewport().GetVisibleRect().Size.X / 2 - 100, 150);
        notification.Modulate = Colors.Gold;
        GetTree().Root.AddChild(notification);
        
        // 动画效果
        var tween = GetTree().CreateTween();
        tween.TweenProperty(notification, "position:y", notification.Position.Y - 50, 1.0f);
        tween.Parallel().TweenProperty(notification, "modulate:a", 0.0f, 1.0f);
        tween.TweenCallback(Callable.From(() => notification.QueueFree()));
    }
    
    /// <summary>
    /// 显示新颖度奖励通知
    /// </summary>
    private void ShowNoveltyBonusNotification(float multiplier, NoveltyLevel level)
    {
        var notification = new Label();
        notification.Text = $"新颖度奖励: {multiplier:F2}x!";
        notification.AddThemeStyleboxOverride("normal", CreateNotificationStyleBox());
        notification.Position = new Vector2(GetViewport().GetVisibleRect().Size.X / 2 - 120, 200);
        notification.Modulate = GetNoveltyLevelColor(level);
        GetTree().Root.AddChild(notification);
        
        var tween = GetTree().CreateTween();
        tween.TweenProperty(notification, "position:y", notification.Position.Y - 50, 1.5f);
        tween.Parallel().TweenProperty(notification, "modulate:a", 0.0f, 1.5f);
        tween.TweenCallback(Callable.From(() => notification.QueueFree()));
    }
    
    /// <summary>
    /// 应用商店物品效果
    /// </summary>
    private void ApplyShopItemEffect(ShopItem item)
    {
        // 根据物品类型应用不同的效果
        switch (item.EffectType)
        {
            case RewardEffectType.Permanent:
                ApplyStatBoostToGame(item.EffectData);
                break;
            case RewardEffectType.Immediate:
                GrantItemToPlayer(item.EffectData);
                break;
            case RewardEffectType.Temporary:
                ApplyExperienceBoostToPlayer(item.EffectData);
                break;
            case RewardEffectType.NextGame:
                // 下局游戏生效的处理
                break;
            // 其他效果类型的处理
        }
    }
    
    /// <summary>
    /// 应用属性提升到游戏
    /// </summary>
    private void ApplyStatBoostToGame(string effectData)
    {
        // 这里需要与游戏的属性系统集成
        // 解析effectData并应用到玩家属性
        GD.Print($"应用属性提升: {effectData}");
    }
    
    /// <summary>
    /// 给予物品给玩家
    /// </summary>
    private void GrantItemToPlayer(string effectData)
    {
        // 这里需要与游戏的物品系统集成
        GD.Print($"给予物品: {effectData}");
    }
    
    /// <summary>
    /// 应用经验提升给玩家
    /// </summary>
    private void ApplyExperienceBoostToPlayer(string effectData)
    {
        // 这里需要与游戏的经验系统集成
        GD.Print($"应用经验提升: {effectData}");
    }
    
    /// <summary>
    /// 创建通知样式框
    /// </summary>
    private StyleBox CreateNotificationStyleBox()
    {
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0, 0, 0, 0.8f);
        styleBox.BorderColor = Colors.White;
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.CornerRadiusTopLeft = 8;
        styleBox.CornerRadiusTopRight = 8;
        styleBox.CornerRadiusBottomLeft = 8;
        styleBox.CornerRadiusBottomRight = 8;
        
        return styleBox;
    }
    
    /// <summary>
    /// 获取新颖度等级颜色
    /// </summary>
    private Color GetNoveltyLevelColor(NoveltyLevel level)
    {
        return level switch
        {
            NoveltyLevel.Common => Colors.White,
            NoveltyLevel.Uncommon => Colors.LightGreen,
            NoveltyLevel.Rare => Colors.LightBlue,
            NoveltyLevel.Epic => Colors.Purple,
            NoveltyLevel.Legendary => Colors.Orange,
            NoveltyLevel.Mythical => Colors.Gold,
            _ => Colors.Gray
        };
    }
    
    /// <summary>
    /// 获取死亡重来管理器
    /// </summary>
    public RebirthManager GetRebirthManager()
    {
        return _rebirthManager;
    }
    
    /// <summary>
    /// 手动触发商店显示
    /// </summary>
    public void ShowShop()
    {
        _shopUI?.ShowShop();
    }
    
    /// <summary>
    /// 手动触发结算界面显示
    /// </summary>
    public void ShowResults(GameSession session, int experienceGained, bool leveledUp = false, int newLevel = 0)
    {
        _resultsUI?.ShowResults(session, experienceGained, leveledUp, newLevel);
    }
    
    /// <summary>
    /// 获取玩家统计摘要
    /// </summary>
    public Dictionary<string, object> GetPlayerStatistics()
    {
        return _rebirthManager?.GetStatisticsSummary() ?? new Dictionary<string, object>();
    }
}