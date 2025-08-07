using Godot;
using Godot.Collections;
using CodeRogue.Achievements;
using System;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 成就进度数据模型
    /// 继承自Resource，用于存储成就的进度和完成状态
    /// </summary>
    [GlobalClass]
    public partial class AchievementProgress : Resource
    {
        /// <summary>成就ID</summary>
        [Export] public string AchievementId { get; set; } = string.Empty;
        
        /// <summary>当前进度值</summary>
        [Export] public int CurrentValue { get; set; } = 0;
        
        /// <summary>目标值</summary>
        [Export] public int TargetValue { get; set; } = 1;
        
        /// <summary>是否已完成</summary>
        [Export] public bool IsCompleted { get; set; } = false;
        
        /// <summary>完成时间（ISO 8601格式字符串）</summary>
        [Export] public string CompletedAt { get; set; } = string.Empty;
        
        /// <summary>完成时间（DateTime属性，用于兼容性）</summary>
        public DateTime? CompletionTime => GetCompletedDateTime();
        
        /// <summary>是否已领取奖励</summary>
        [Export] public bool IsRewardClaimed { get; set; } = false;
        
        /// <summary>成就状态</summary>
        [Export] public AchievementStatus Status { get; set; } = AchievementStatus.Locked;
        
        /// <summary>解锁时间（ISO 8601格式字符串）</summary>
        [Export] public string UnlockedAt { get; set; } = string.Empty;
        
        /// <summary>完成次数（用于可重复成就）</summary>
        [Export] public int CompletionCount { get; set; } = 0;
        
        /// <summary>最后重置时间（ISO 8601格式字符串）</summary>
        [Export] public string LastResetAt { get; set; } = string.Empty;
        
        /// <summary>额外数据字典</summary>
        [Export] public Dictionary<string, Variant> ExtraData { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>
        /// 获取进度百分比
        /// </summary>
        /// <returns>进度百分比（0-100）</returns>
        public float GetProgressPercentage()
        {
            if (TargetValue <= 0)
                return 0f;
                
            return Math.Min(100f, (float)CurrentValue / TargetValue * 100f);
        }
        
        /// <summary>
        /// 检查是否达到完成条件
        /// </summary>
        /// <returns>是否达到完成条件</returns>
        public bool CheckCompletionCondition()
        {
            return CurrentValue >= TargetValue;
        }
        
        /// <summary>
        /// 更新进度值
        /// </summary>
        /// <param name="value">新的进度值</param>
        /// <param name="isIncrement">是否为增量更新</param>
        /// <returns>是否触发了完成条件</returns>
        public bool UpdateProgress(int value, bool isIncrement = true)
        {
            var oldValue = CurrentValue;
            
            if (isIncrement)
            {
                CurrentValue += value;
            }
            else
            {
                CurrentValue = value;
            }
            
            // 确保进度值不超过目标值（除非是可重复成就）
            CurrentValue = Math.Max(0, CurrentValue);
            
            // 检查是否刚刚达到完成条件
            bool wasCompleted = oldValue >= TargetValue;
            bool isNowCompleted = CheckCompletionCondition();
            
            return !wasCompleted && isNowCompleted;
        }
        
        /// <summary>
        /// 标记成就为已完成
        /// </summary>
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            Status = AchievementStatus.Completed;
            CompletedAt = DateTime.UtcNow.ToString("O"); // ISO 8601格式
            CompletionCount++;
        }
        
        /// <summary>
        /// 标记奖励为已领取
        /// </summary>
        public void MarkRewardAsClaimed()
        {
            IsRewardClaimed = true;
            Status = AchievementStatus.RewardClaimed;
        }
        
        /// <summary>
        /// 解锁成就
        /// </summary>
        public void Unlock()
        {
            if (Status == AchievementStatus.Locked)
            {
                Status = AchievementStatus.Unlocked;
                UnlockedAt = DateTime.UtcNow.ToString("O");
            }
        }
        
        /// <summary>
        /// 重置成就进度（用于可重复成就）
        /// </summary>
        public void Reset()
        {
            CurrentValue = 0;
            IsCompleted = false;
            IsRewardClaimed = false;
            Status = AchievementStatus.Unlocked;
            CompletedAt = string.Empty;
            LastResetAt = DateTime.UtcNow.ToString("O");
        }
        
        /// <summary>
        /// 检查是否需要重置（基于重置周期）
        /// </summary>
        /// <param name="resetPeriodDays">重置周期（天数）</param>
        /// <returns>是否需要重置</returns>
        public bool ShouldReset(int resetPeriodDays)
        {
            if (resetPeriodDays <= 0 || string.IsNullOrEmpty(LastResetAt))
                return false;
                
            if (DateTime.TryParse(LastResetAt, out var lastReset))
            {
                var daysSinceReset = (DateTime.UtcNow - lastReset).TotalDays;
                return daysSinceReset >= resetPeriodDays;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取完成时间的DateTime对象
        /// </summary>
        /// <returns>完成时间，如果未完成则返回null</returns>
        public DateTime? GetCompletedDateTime()
        {
            if (string.IsNullOrEmpty(CompletedAt))
                return null;
                
            if (DateTime.TryParse(CompletedAt, out var dateTime))
                return dateTime;
                
            return null;
        }
        
        /// <summary>
        /// 获取解锁时间的DateTime对象
        /// </summary>
        /// <returns>解锁时间，如果未解锁则返回null</returns>
        public DateTime? GetUnlockedDateTime()
        {
            if (string.IsNullOrEmpty(UnlockedAt))
                return null;
                
            if (DateTime.TryParse(UnlockedAt, out var dateTime))
                return dateTime;
                
            return null;
        }
        
        /// <summary>
        /// 获取进度显示文本
        /// </summary>
        /// <returns>进度显示文本</returns>
        public string GetProgressText()
        {
            if (IsCompleted)
                return "已完成";
                
            return $"{CurrentValue}/{TargetValue}";
        }
        
        /// <summary>
        /// 验证进度数据的有效性
        /// </summary>
        /// <returns>验证结果和错误信息</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrEmpty(AchievementId))
                return (false, "成就ID不能为空");
                
            if (TargetValue <= 0)
                return (false, "目标值必须大于0");
                
            if (CurrentValue < 0)
                return (false, "当前进度值不能为负数");
                
            if (CompletionCount < 0)
                return (false, "完成次数不能为负数");
                
            return (true, string.Empty);
        }
    }
}