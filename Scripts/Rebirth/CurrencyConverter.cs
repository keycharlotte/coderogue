using Godot;
using System;
using CodeRogue.Rebirth;
using CodeRogue.Rebirth.Data;

/// <summary>
/// 货币转换器
/// </summary>
public partial class CurrencyConverter : Node
{
    private const int BASE_CURRENCY_PER_FLOOR = 10;
    private const int BASE_CURRENCY_PER_ENEMY = 2;
    private const int VICTORY_BONUS = 50;
    private const int MAX_TIME_BONUS = 50;
    private const float TIME_BONUS_PER_HOUR = 20f;
    
    [Signal] public delegate void CurrencyCalculatedEventHandler(float baseCurrency, float finalCurrency, float multiplier);
    
    /// <summary>
    /// 计算基础货币
    /// </summary>
    public float CalculateBaseCurrency(GameSession session)
    {
        float baseCurrency = 0f;
        
        // 基础层数奖励
        baseCurrency += session.FloorsReached * BASE_CURRENCY_PER_FLOOR;
        
        // 击败敌人奖励
        baseCurrency += session.EnemiesDefeated * BASE_CURRENCY_PER_ENEMY;
        
        // 胜利奖励
        if (session.IsVictory)
        {
            baseCurrency += VICTORY_BONUS;
        }
        
        // 游戏时长奖励（防止速通，鼓励探索）
        var gameTime = session.GetGameDurationMinutes();
        float timeBonus = CalculateTimeBonus(gameTime);
        baseCurrency += timeBonus;
        
        // 效率奖励
        float efficiencyBonus = CalculateEfficiencyBonus(session);
        baseCurrency += efficiencyBonus;
        
        // 卡牌使用奖励
        float cardBonus = CalculateCardUsageBonus(session);
        baseCurrency += cardBonus;
        
        // 遗物激活奖励
        float relicBonus = CalculateRelicActivationBonus(session);
        baseCurrency += relicBonus;
        
        return Mathf.Max(0f, baseCurrency);
    }
    
    /// <summary>
    /// 计算最终货币
    /// </summary>
    public int CalculateFinalCurrency(float baseCurrency, float noveltyMultiplier)
    {
        float finalCurrency = baseCurrency * noveltyMultiplier;
        int result = Mathf.RoundToInt(finalCurrency);
        
        EmitSignal(SignalName.CurrencyCalculated, baseCurrency, finalCurrency, noveltyMultiplier);
        
        return result;
    }
    
    /// <summary>
    /// 计算时间奖励
    /// </summary>
    private float CalculateTimeBonus(double gameTimeMinutes)
    {
        if (gameTimeMinutes <= 0) return 0f;
        
        float timeBonus = (float)(gameTimeMinutes / 60.0 * TIME_BONUS_PER_HOUR);
        return Mathf.Min(MAX_TIME_BONUS, timeBonus);
    }
    
    /// <summary>
    /// 计算效率奖励
    /// </summary>
    private float CalculateEfficiencyBonus(GameSession session)
    {
        float efficiencyScore = session.GetEfficiencyScore();
        
        // 高效率玩家获得额外奖励
        if (efficiencyScore >= 20f) // 每小时20层以上
        {
            return 30f;
        }
        else if (efficiencyScore >= 15f)
        {
            return 20f;
        }
        else if (efficiencyScore >= 10f)
        {
            return 10f;
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 计算卡牌使用奖励
    /// </summary>
    private float CalculateCardUsageBonus(GameSession session)
    {
        if (session.UsedCards.Count == 0) return 0f;
        
        float totalBonus = 0f;
        
        foreach (var card in session.UsedCards)
        {
            // 基于卡牌使用次数和效果的奖励
            float cardValue = card.CalculateValueScore();
            totalBonus += cardValue * 0.5f;
        }
        
        // 卡牌多样性奖励
        if (session.UsedCards.Count >= 10)
        {
            totalBonus += 15f;
        }
        else if (session.UsedCards.Count >= 7)
        {
            totalBonus += 10f;
        }
        else if (session.UsedCards.Count >= 5)
        {
            totalBonus += 5f;
        }
        
        return totalBonus;
    }
    
    /// <summary>
    /// 计算遗物激活奖励
    /// </summary>
    private float CalculateRelicActivationBonus(GameSession session)
    {
        if (session.UsedRelics.Count == 0) return 0f;
        
        float totalBonus = 0f;
        
        foreach (var relic in session.UsedRelics)
        {
            // 基于遗物激活次数和影响的奖励
            float relicValue = relic.CalculateValueScore();
            totalBonus += relicValue * 0.3f;
        }
        
        // 遗物收集奖励
        if (session.UsedRelics.Count >= 5)
        {
            totalBonus += 20f;
        }
        else if (session.UsedRelics.Count >= 3)
        {
            totalBonus += 10f;
        }
        
        return totalBonus;
    }
    
    /// <summary>
    /// 计算难度奖励
    /// </summary>
    public float CalculateDifficultyBonus(int difficultyLevel)
    {
        return difficultyLevel switch
        {
            1 => 1.0f,  // 简单
            2 => 1.2f,  // 普通
            3 => 1.5f,  // 困难
            4 => 2.0f,  // 专家
            5 => 3.0f,  // 大师
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// 计算连胜奖励
    /// </summary>
    public float CalculateWinStreakBonus(int winStreak)
    {
        if (winStreak <= 1) return 1.0f;
        
        // 连胜奖励，最高50%
        float bonus = 1.0f + (winStreak - 1) * 0.1f;
        return Mathf.Min(1.5f, bonus);
    }
    
    /// <summary>
    /// 计算首次通关奖励
    /// </summary>
    public float CalculateFirstClearBonus(bool isFirstClear)
    {
        return isFirstClear ? 100f : 0f;
    }
    
    /// <summary>
    /// 获取货币转换详情
    /// </summary>
    public CurrencyBreakdown GetCurrencyBreakdown(GameSession session, float noveltyMultiplier)
    {
        var breakdown = new CurrencyBreakdown
        {
            FloorBonus = session.FloorsReached * BASE_CURRENCY_PER_FLOOR,
            EnemyBonus = session.EnemiesDefeated * BASE_CURRENCY_PER_ENEMY,
            VictoryBonus = session.IsVictory ? VICTORY_BONUS : 0,
            TimeBonus = CalculateTimeBonus(session.GetGameDurationMinutes()),
            EfficiencyBonus = CalculateEfficiencyBonus(session),
            CardBonus = CalculateCardUsageBonus(session),
            RelicBonus = CalculateRelicActivationBonus(session),
            NoveltyMultiplier = noveltyMultiplier
        };
        
        breakdown.BaseCurrency = breakdown.FloorBonus + breakdown.EnemyBonus + 
                               breakdown.VictoryBonus + breakdown.TimeBonus + 
                               breakdown.EfficiencyBonus + breakdown.CardBonus + 
                               breakdown.RelicBonus;
                               
        breakdown.FinalCurrency = breakdown.BaseCurrency * noveltyMultiplier;
        
        return breakdown;
    }
}

/// <summary>
/// 货币分解详情
/// </summary>
public class CurrencyBreakdown
{
    public float FloorBonus { get; set; }
    public float EnemyBonus { get; set; }
    public float VictoryBonus { get; set; }
    public float TimeBonus { get; set; }
    public float EfficiencyBonus { get; set; }
    public float CardBonus { get; set; }
    public float RelicBonus { get; set; }
    public float BaseCurrency { get; set; }
    public float NoveltyMultiplier { get; set; }
    public float FinalCurrency { get; set; }
}