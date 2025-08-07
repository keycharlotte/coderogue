using Godot;
using System;
using CodeRogue.Rebirth;

namespace CodeRogue.Rebirth.Data
{
    /// <summary>
    /// 游戏会话数据
    /// </summary>
    [System.Serializable]
    public partial class GameSession : Resource
{
    [Export] public string SessionId { get; set; } = System.Guid.NewGuid().ToString();
    [Export] public string StartTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public string EndTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public int FloorsReached { get; set; }
    [Export] public int EnemiesDefeated { get; set; }
    [Export] public string HeroId { get; set; }
    [Export] public float BaseCurrencyEarned { get; set; }
    [Export] public float NoveltyMultiplier { get; set; } = 1.0f;
    [Export] public float TotalCurrencyEarned { get; set; }
    [Export] public bool IsVictory { get; set; }
    [Export] public GameResultType ResultType { get; set; }
    [Export] public Godot.Collections.Array<UsedCard> UsedCards { get; set; } = new();
    [Export] public Godot.Collections.Array<UsedRelic> UsedRelics { get; set; } = new();
    [Export] public NoveltyLevel NoveltyLevel { get; set; }
    [Export] public CombinationRating CombinationRating { get; set; }
    
    /// <summary>
    /// 计算游戏时长（分钟）
    /// </summary>
    public double GetGameDurationMinutes()
    {
        if (DateTime.TryParse(StartTime, out var start) && DateTime.TryParse(EndTime, out var end))
        {
            return (end - start).TotalMinutes;
        }
        return 0;
    }
    
    /// <summary>
    /// 获取胜率
    /// </summary>
    public float GetWinRate()
    {
        return IsVictory ? 1.0f : 0.0f;
    }
    
    /// <summary>
    /// 获取效率评分
    /// </summary>
    public float GetEfficiencyScore()
    {
        var duration = GetGameDurationMinutes();
        if (duration <= 0) return 0f;
        
        return FloorsReached / (float)duration * 60f; // 每小时通过的层数
    }
}
}