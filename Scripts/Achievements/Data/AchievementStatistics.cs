using Godot;
using Godot.Collections;
using System;
using CodeRogue.Achievements;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 成就统计数据类
    /// 用于记录成就系统的统计信息
    /// </summary>
    [GlobalClass]
    public partial class AchievementStatistics : Resource
    {
        /// <summary>
        /// 总成就数量
        /// </summary>
        [Export] public int TotalAchievements { get; set; } = 0;
        
        /// <summary>
        /// 已完成成就数量
        /// </summary>
        [Export] public int CompletedAchievements { get; set; } = 0;
        
        /// <summary>
        /// 总成就点数
        /// </summary>
        [Export] public int TotalPoints { get; set; } = 0;
        
        /// <summary>
        /// 已获得成就点数
        /// </summary>
        [Export] public int EarnedPoints { get; set; } = 0;
        
        /// <summary>
        /// 各稀有度成就完成数量
        /// </summary>
        [Export] public Dictionary<int, int> CompletedByRarity { get; set; } = new Dictionary<int, int>();
        
        /// <summary>
        /// 各分类成就完成数量
        /// </summary>
        [Export] public Dictionary<int, int> CompletedByCategory { get; set; } = new Dictionary<int, int>();
        
        /// <summary>
        /// 最近完成的成就ID列表
        /// </summary>
        [Export] public string[] RecentCompletions { get; set; } = System.Array.Empty<string>();
        
        /// <summary>
        /// 上次更新时间
        /// </summary>
        [Export] public string LastUpdated { get; set; } = string.Empty;
        
        /// <summary>
        /// 获取完成百分比
        /// </summary>
        /// <returns>完成百分比（0-100）</returns>
        public float GetCompletionPercentage()
        {
            if (TotalAchievements <= 0) return 0f;
            return (float)CompletedAchievements / TotalAchievements * 100f;
        }
        
        /// <summary>
        /// 获取点数百分比
        /// </summary>
        /// <returns>点数百分比（0-100）</returns>
        public float GetPointsPercentage()
        {
            if (TotalPoints <= 0) return 0f;
            return (float)EarnedPoints / TotalPoints * 100f;
        }
    }
}