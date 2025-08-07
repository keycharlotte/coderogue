using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Rebirth;
using CodeRogue.Rebirth.Data;

/// <summary>
/// 死亡重来结算UI控制器
/// </summary>
public partial class RebirthResultsUI : Control
{
    private RebirthManager _rebirthManager;
    private GameSession _currentSession;
    private CurrencyBreakdown _currencyBreakdown;
    
    // UI节点引用
    private Label _resultTitle;
    private Label _floorLabel;
    private Label _enemyLabel;
    private Label _timeLabel;
    private Label _heroLabel;
    private Label _victoryLabel;
    private Label _efficiencyLabel;
    private Label _combinationLabel;
    private Label _noveltyLevelLabel;
    private Label _multiplierLabel;
    private Label _noveltyDescription;
    private Label _baseCurrencyLabel;
    private Label _noveltyBonusLabel;
    private Label _totalCurrencyLabel;
    private Label _experienceLabel;
    private Label _levelUpLabel;
    private Button _shopButton;
    private Button _continueButton;
    private AnimationPlayer _animationPlayer;
    
    [Signal] public delegate void ShopRequestedEventHandler();
    [Signal] public delegate void ContinueGameRequestedEventHandler();
    [Signal] public delegate void ResultsClosedEventHandler();
    
    public override void _Ready()
    {
        InitializeNodes();
        
        // 获取死亡重来管理器
        _rebirthManager = GetNode<RebirthManager>("/root/RebirthManager");
        if (_rebirthManager == null)
        {
            GD.PrintErr("无法找到RebirthManager");
        }
        
        // 初始时隐藏
        Hide();
    }
    
    /// <summary>
    /// 初始化UI节点
    /// </summary>
    private void InitializeNodes()
    {
        _resultTitle = GetNode<Label>("MainContainer/GameResultContainer/ResultTitle");
        _floorLabel = GetNode<Label>("MainContainer/GameResultContainer/ResultDetails/LeftColumn/FloorLabel");
        _enemyLabel = GetNode<Label>("MainContainer/GameResultContainer/ResultDetails/LeftColumn/EnemyLabel");
        _timeLabel = GetNode<Label>("MainContainer/GameResultContainer/ResultDetails/LeftColumn/TimeLabel");
        _heroLabel = GetNode<Label>("MainContainer/GameResultContainer/ResultDetails/RightColumn/HeroLabel");
        _victoryLabel = GetNode<Label>("MainContainer/GameResultContainer/ResultDetails/RightColumn/VictoryLabel");
        _efficiencyLabel = GetNode<Label>("MainContainer/GameResultContainer/ResultDetails/RightColumn/EfficiencyLabel");
        _combinationLabel = GetNode<Label>("MainContainer/NoveltyContainer/NoveltyDetails/CombinationLabel");
        _noveltyLevelLabel = GetNode<Label>("MainContainer/NoveltyContainer/NoveltyDetails/NoveltyLevelLabel");
        _multiplierLabel = GetNode<Label>("MainContainer/NoveltyContainer/NoveltyDetails/MultiplierLabel");
        _noveltyDescription = GetNode<Label>("MainContainer/NoveltyContainer/NoveltyDetails/NoveltyDescription");
        _baseCurrencyLabel = GetNode<Label>("MainContainer/CurrencyContainer/CurrencyBreakdown/BaseCurrencyLabel");
        _noveltyBonusLabel = GetNode<Label>("MainContainer/CurrencyContainer/CurrencyBreakdown/NoveltyBonusLabel");
        _totalCurrencyLabel = GetNode<Label>("MainContainer/CurrencyContainer/CurrencyBreakdown/TotalCurrencyLabel");
        _experienceLabel = GetNode<Label>("MainContainer/ExperienceContainer/ExperienceLabel");
        _levelUpLabel = GetNode<Label>("MainContainer/ExperienceContainer/LevelUpLabel");
        _shopButton = GetNode<Button>("MainContainer/ButtonContainer/ShopButton");
        _continueButton = GetNode<Button>("MainContainer/ButtonContainer/ContinueButton");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }
    
    /// <summary>
    /// 显示游戏结算结果
    /// </summary>
    public void ShowResults(GameSession session, int experienceGained, bool leveledUp = false, int newLevel = 0)
    {
        _currentSession = session;
        
        // 获取货币分解详情
        if (_rebirthManager?.GetNode<CurrencyConverter>("CurrencyConverter") != null)
        {
            var converter = _rebirthManager.GetNode<CurrencyConverter>("CurrencyConverter");
            _currencyBreakdown = converter.GetCurrencyBreakdown(session, session.NoveltyMultiplier);
        }
        
        UpdateGameResultDisplay();
        UpdateNoveltyDisplay();
        UpdateCurrencyDisplay();
        UpdateExperienceDisplay(experienceGained, leveledUp, newLevel);
        
        Show();
        PlayShowAnimation();
    }
    
    /// <summary>
    /// 更新游戏结果显示
    /// </summary>
    private void UpdateGameResultDisplay()
    {
        if (_currentSession == null) return;
        
        // 设置结果标题
        _resultTitle.Text = _currentSession.IsVictory ? "胜利!" : "失败";
        _resultTitle.Modulate = _currentSession.IsVictory ? Colors.Gold : Colors.LightCoral;
        
        // 基础信息
        _floorLabel.Text = $"到达层数: {_currentSession.FloorsReached}";
        _enemyLabel.Text = $"击败敌人: {_currentSession.EnemiesDefeated}";
        _timeLabel.Text = $"游戏时长: {_currentSession.GetGameDurationMinutes():F1}分钟";
        
        // 英雄和结果信息
        _heroLabel.Text = $"使用英雄: {GetHeroDisplayName(_currentSession.HeroId)}";
        _victoryLabel.Text = $"游戏结果: {(_currentSession.IsVictory ? "胜利" : "失败")}";
        _victoryLabel.Modulate = _currentSession.IsVictory ? Colors.LightGreen : Colors.LightCoral;
        
        // 效率评分
        var efficiency = _currentSession.GetEfficiencyScore();
        _efficiencyLabel.Text = $"效率评分: {efficiency:F1}";
        _efficiencyLabel.Modulate = GetEfficiencyColor(efficiency);
    }
    
    /// <summary>
    /// 更新新颖度显示
    /// </summary>
    private void UpdateNoveltyDisplay()
    {
        if (_currentSession == null) return;
        
        // 组合评估
        var combinationRating = _currentSession.CombinationRating;
        _combinationLabel.Text = $"组合评估: {GetCombinationRatingText(combinationRating)}";
        _combinationLabel.Modulate = GetCombinationRatingColor(combinationRating);
        
        // 新颖度等级
        var noveltyLevel = _currentSession.NoveltyLevel;
        _noveltyLevelLabel.Text = $"新颖度等级: {GetNoveltyLevelText(noveltyLevel)}";
        _noveltyLevelLabel.Modulate = GetNoveltyLevelColor(noveltyLevel);
        
        // 新颖度倍率
        var multiplier = _currentSession.NoveltyMultiplier;
        _multiplierLabel.Text = $"新颖度倍率: {multiplier:F2}x";
        _multiplierLabel.Modulate = GetMultiplierColor(multiplier);
        
        // 新颖度描述
        _noveltyDescription.Text = GetNoveltyDescription(noveltyLevel, multiplier);
    }
    
    /// <summary>
    /// 更新货币显示
    /// </summary>
    private void UpdateCurrencyDisplay()
    {
        if (_currentSession == null) return;
        
        var baseCurrency = _currentSession.BaseCurrencyEarned;
        var totalCurrency = _currentSession.TotalCurrencyEarned;
        var noveltyBonus = totalCurrency - baseCurrency;
        
        _baseCurrencyLabel.Text = $"基础货币: {baseCurrency:F0}";
        _noveltyBonusLabel.Text = $"新颖度奖励: +{noveltyBonus:F0}";
        _noveltyBonusLabel.Modulate = noveltyBonus > 0 ? Colors.LightGreen : Colors.White;
        
        _totalCurrencyLabel.Text = $"总获得: {totalCurrency} 投胎币";
        
        // 如果有详细分解信息，可以显示更多细节
        if (_currencyBreakdown != null)
        {
            var tooltip = CreateCurrencyTooltip(_currencyBreakdown);
            _totalCurrencyLabel.TooltipText = tooltip;
        }
    }
    
    /// <summary>
    /// 更新经验显示
    /// </summary>
    private void UpdateExperienceDisplay(int experienceGained, bool leveledUp, int newLevel)
    {
        _experienceLabel.Text = $"获得经验: {experienceGained}";
        
        if (leveledUp)
        {
            _levelUpLabel.Text = $"恭喜升级! 当前等级: {newLevel}";
            _levelUpLabel.Show();
        }
        else
        {
            _levelUpLabel.Hide();
        }
    }
    
    /// <summary>
    /// 播放显示动画
    /// </summary>
    private void PlayShowAnimation()
    {
        if (_animationPlayer != null && _animationPlayer.HasAnimation("show_results"))
        {
            _animationPlayer.Play("show_results");
        }
        else
        {
            // 简单的淡入效果
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", Colors.White, 0.5f);
        }
    }
    
    /// <summary>
    /// 获取英雄显示名称
    /// </summary>
    private string GetHeroDisplayName(string heroId)
    {
        return heroId switch
        {
            "default_hero" => "默认英雄",
            "warrior" => "战士",
            "mage" => "法师",
            "rogue" => "盗贼",
            _ => heroId
        };
    }
    
    /// <summary>
    /// 获取效率评分颜色
    /// </summary>
    private Color GetEfficiencyColor(float efficiency)
    {
        return efficiency switch
        {
            >= 20f => Colors.Gold,
            >= 15f => Colors.LightGreen,
            >= 10f => Colors.Yellow,
            >= 5f => Colors.Orange,
            _ => Colors.LightCoral
        };
    }
    
    /// <summary>
    /// 获取组合评估文本
    /// </summary>
    private string GetCombinationRatingText(CombinationRating rating)
    {
        return rating switch
        {
            CombinationRating.Poor => "较差",
            CombinationRating.Fair => "一般",
            CombinationRating.Good => "良好",
            CombinationRating.Excellent => "优秀",
            CombinationRating.Perfect => "完美",
            _ => "未知"
        };
    }
    
    /// <summary>
    /// 获取组合评估颜色
    /// </summary>
    private Color GetCombinationRatingColor(CombinationRating rating)
    {
        return rating switch
        {
            CombinationRating.Poor => Colors.Red,
            CombinationRating.Fair => Colors.Yellow,
            CombinationRating.Good => Colors.LightGreen,
            CombinationRating.Excellent => Colors.Cyan,
            CombinationRating.Perfect => Colors.Gold,
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// 获取新颖度等级文本
    /// </summary>
    private string GetNoveltyLevelText(NoveltyLevel level)
    {
        return level switch
        {
            NoveltyLevel.Common => "普通",
            NoveltyLevel.Uncommon => "稀有",
            NoveltyLevel.Rare => "史诗",
            NoveltyLevel.Epic => "传说",
            NoveltyLevel.Legendary => "神话",
            NoveltyLevel.Mythical => "神话+",
            _ => "未知"
        };
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
    /// 获取倍率颜色
    /// </summary>
    private Color GetMultiplierColor(float multiplier)
    {
        return multiplier switch
        {
            >= 3.0f => Colors.Gold,
            >= 2.0f => Colors.Orange,
            >= 1.5f => Colors.Purple,
            >= 1.2f => Colors.LightBlue,
            >= 1.1f => Colors.LightGreen,
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// 获取新颖度描述
    /// </summary>
    private string GetNoveltyDescription(NoveltyLevel level, float multiplier)
    {
        return level switch
        {
            NoveltyLevel.Common => "这是一个常见的组合，没有特殊奖励。",
            NoveltyLevel.Uncommon => "这个组合有些新意，获得了小幅奖励。",
            NoveltyLevel.Rare => "这是一个创新的组合，获得了不错的奖励!",
            NoveltyLevel.Epic => "这个组合非常独特，获得了丰厚的奖励!",
            NoveltyLevel.Legendary => "这是一个传说级的组合，获得了巨额奖励!",
            NoveltyLevel.Mythical => "这个组合前所未见，获得了史诗级奖励!",
            _ => "未知的组合类型。"
        };
    }
    
    /// <summary>
    /// 创建货币详情提示
    /// </summary>
    private string CreateCurrencyTooltip(CurrencyBreakdown breakdown)
    {
        var tooltip = "货币详细分解:\n";
        tooltip += $"层数奖励: {breakdown.FloorBonus:F0}\n";
        tooltip += $"敌人奖励: {breakdown.EnemyBonus:F0}\n";
        tooltip += $"胜利奖励: {breakdown.VictoryBonus:F0}\n";
        tooltip += $"时间奖励: {breakdown.TimeBonus:F0}\n";
        tooltip += $"效率奖励: {breakdown.EfficiencyBonus:F0}\n";
        tooltip += $"卡牌奖励: {breakdown.CardBonus:F0}\n";
        tooltip += $"遗物奖励: {breakdown.RelicBonus:F0}\n";
        tooltip += $"基础总计: {breakdown.BaseCurrency:F0}\n";
        tooltip += $"新颖度倍率: {breakdown.NoveltyMultiplier:F2}x\n";
        tooltip += $"最终总计: {breakdown.FinalCurrency:F0}";
        
        return tooltip;
    }
    
    /// <summary>
    /// 商店按钮点击事件
    /// </summary>
    private void _on_shop_button_pressed()
    {
        EmitSignal(SignalName.ShopRequested);
    }
    
    /// <summary>
    /// 继续游戏按钮点击事件
    /// </summary>
    private void _on_continue_button_pressed()
    {
        EmitSignal(SignalName.ContinueGameRequested);
        HideResults();
    }
    
    /// <summary>
    /// 隐藏结算界面
    /// </summary>
    public void HideResults()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 0.3f);
        tween.TweenCallback(Callable.From(() => {
            Hide();
            Modulate = Colors.White;
            EmitSignal(SignalName.ResultsClosed);
        }));
    }
    
    /// <summary>
    /// 处理输入事件
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // ESC键关闭界面
            if (keyEvent.Keycode == Key.Escape)
            {
                _on_continue_button_pressed();
            }
            // 空格键继续游戏
            else if (keyEvent.Keycode == Key.Space)
            {
                _on_continue_button_pressed();
            }
            // S键打开商店
            else if (keyEvent.Keycode == Key.S)
            {
                _on_shop_button_pressed();
            }
        }
    }
}