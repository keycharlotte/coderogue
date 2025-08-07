using Godot;
using Godot.Collections;
using System;
using CodeRogue.Achievements;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 成就配置类
    /// 定义成就的基本信息、条件和奖励
    /// </summary>
    [GlobalClass]
    public partial class AchievementConfig : Resource
    {
        /// <summary>
        /// 成就唯一标识符
        /// </summary>
        [Export] public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 成就名称
        /// </summary>
        [Export] public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 成就描述
        /// </summary>
        [Export] public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 成就类型
        /// </summary>
        [Export] public AchievementType Type { get; set; } = AchievementType.Counter;
        
        /// <summary>
        /// 成就分类
        /// </summary>
        [Export] public AchievementCategory Category { get; set; } = AchievementCategory.Combat;
        
        /// <summary>
        /// 成就稀有度
        /// </summary>
        [Export] public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;
        
        /// <summary>
        /// 目标值（用于计数型成就）
        /// </summary>
        [Export] public int TargetValue { get; set; } = 1;
        
        /// <summary>
        /// 成就图标路径
        /// </summary>
        [Export] public string IconPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 奖励配置数组
        /// </summary>
        [Export] public RewardConfig[] Rewards { get; set; } = Array.Empty<RewardConfig>();
        
        /// <summary>
        /// 前置成就ID数组
        /// </summary>
        [Export] public string[] Prerequisites { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// 是否为隐藏成就
        /// </summary>
        [Export] public bool IsHidden { get; set; } = false;
        
        /// <summary>
        /// 成就条件数组
        /// </summary>
        [Export] public AchievementCondition[] Conditions { get; set; } = Array.Empty<AchievementCondition>();
        
        /// <summary>
        /// 条件逻辑类型（AND/OR/Sequence）
        /// </summary>
        [Export] public ConditionLogicType ConditionLogic { get; set; } = ConditionLogicType.And;
        
        /// <summary>
        /// 成就点数（用于成就系统积分）
        /// </summary>
        [Export] public int Points { get; set; } = 10;
        
        /// <summary>
        /// 是否可重复完成
        /// </summary>
        [Export] public bool IsRepeatable { get; set; } = false;
        
        /// <summary>
        /// 重置周期（天数，0表示不重置）
        /// </summary>
        [Export] public int ResetPeriodDays { get; set; } = 0;
    }

    /// <summary>
    /// 成就进度类
    /// 记录玩家在特定成就上的进度
    /// </summary>
    [GlobalClass]
    public partial class AchievementProgress : Resource
    {
        /// <summary>
        /// 对应的成就ID
        /// </summary>
        [Export] public string AchievementId { get; set; } = string.Empty;
        
        /// <summary>
        /// 当前进度值
        /// </summary>
        [Export] public int CurrentValue { get; set; } = 0;
        
        /// <summary>
        /// 目标值
        /// </summary>
        [Export] public int TargetValue { get; set; } = 1;
        
        /// <summary>
        /// 是否已完成
        /// </summary>
        [Export] public bool IsCompleted { get; set; } = false;
        
        /// <summary>
        /// 完成时间
        /// </summary>
        [Export] public string CompletedAt { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否已领取奖励
        /// </summary>
        [Export] public bool IsRewardClaimed { get; set; } = false;
        
        /// <summary>
        /// 成就状态
        /// </summary>
        [Export] public AchievementStatus Status { get; set; } = AchievementStatus.Locked;
        
        /// <summary>
        /// 首次解锁时间
        /// </summary>
        [Export] public string UnlockedAt { get; set; } = string.Empty;
        
        /// <summary>
        /// 完成次数（用于可重复成就）
        /// </summary>
        [Export] public int CompletionCount { get; set; } = 0;
        
        /// <summary>
        /// 上次重置时间
        /// </summary>
        [Export] public string LastResetAt { get; set; } = string.Empty;
        
        /// <summary>
        /// 额外数据（用于存储特殊条件的进度）
        /// </summary>
        [Export] public Dictionary<string, Variant> ExtraData { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>
        /// 获取进度百分比
        /// </summary>
        /// <returns>进度百分比（0-100）</returns>
        public float GetProgressPercentage()
        {
            if (TargetValue <= 0) return 0f;
            return Math.Min(100f, (float)CurrentValue / TargetValue * 100f);
        }
        
        /// <summary>
        /// 检查是否可以完成
        /// </summary>
        /// <returns>是否可以完成</returns>
        public bool CanComplete()
        {
            return !IsCompleted && CurrentValue >= TargetValue && Status == AchievementStatus.InProgress;
        }
    }

    /// <summary>
    /// 奖励配置类
    /// 定义成就完成后给予的奖励
    /// </summary>
    [GlobalClass]
    public partial class RewardConfig : Resource
    {
        /// <summary>
        /// 奖励类型
        /// </summary>
        [Export] public RewardType Type { get; set; } = RewardType.Experience;
        
        /// <summary>
        /// 物品ID（用于特定物品奖励）
        /// </summary>
        [Export] public string ItemId { get; set; } = string.Empty;
        
        /// <summary>
        /// 奖励数量
        /// </summary>
        [Export] public int Quantity { get; set; } = 1;
        
        /// <summary>
        /// 奖励描述
        /// </summary>
        [Export] public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 奖励图标路径
        /// </summary>
        [Export] public string IconPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否为稀有奖励
        /// </summary>
        [Export] public bool IsRare { get; set; } = false;
        
        /// <summary>
        /// 奖励权重（用于随机奖励）
        /// </summary>
        [Export] public float Weight { get; set; } = 1.0f;
        
        /// <summary>
        /// 额外参数
        /// </summary>
        [Export] public Dictionary<string, Variant> Parameters { get; set; } = new Dictionary<string, Variant>();
    }

    /// <summary>
    /// 成就条件类
    /// 定义成就的触发条件
    /// </summary>
    [GlobalClass]
    public partial class AchievementCondition : Resource
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        [Export] public GameEventType EventType { get; set; } = GameEventType.EnemyDefeated;
        
        /// <summary>
        /// 参数名称
        /// </summary>
        [Export] public string ParameterName { get; set; } = string.Empty;
        
        /// <summary>
        /// 参数值
        /// </summary>
        [Export] public Variant ParameterValue { get; set; } = new Variant();
        
        /// <summary>
        /// 比较类型
        /// </summary>
        [Export] public ComparisonType Comparison { get; set; } = ComparisonType.GreaterOrEqual;
        
        /// <summary>
        /// 条件描述
        /// </summary>
        [Export] public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否为可选条件
        /// </summary>
        [Export] public bool IsOptional { get; set; } = false;
        
        /// <summary>
        /// 条件权重（用于复合条件）
        /// </summary>
        [Export] public float Weight { get; set; } = 1.0f;
        
        /// <summary>
        /// 额外过滤条件
        /// </summary>
        [Export] public Dictionary<string, Variant> Filters { get; set; } = new Dictionary<string, Variant>();
    }

    /// <summary>
    /// 成就事件数据类
    /// 用于传递游戏事件信息
    /// </summary>
    [GlobalClass]
    public partial class AchievementEventData : Resource
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        [Export] public GameEventType EventType { get; set; } = GameEventType.EnemyDefeated;
        
        /// <summary>
        /// 事件参数
        /// </summary>
        [Export] public Dictionary<string, Variant> Parameters { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>
        /// 事件发生时间
        /// </summary>
        [Export] public string Timestamp { get; set; } = string.Empty;
        
        /// <summary>
        /// 事件来源
        /// </summary>
        [Export] public string Source { get; set; } = string.Empty;
        
        /// <summary>
        /// 玩家ID（多人游戏时使用）
        /// </summary>
        [Export] public string PlayerId { get; set; } = string.Empty;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public AchievementEventData()
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="parameters">事件参数</param>
        public AchievementEventData(GameEventType eventType, Dictionary<string, Variant> parameters = null)
        {
            EventType = eventType;
            Parameters = parameters ?? new Dictionary<string, Variant>();
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    /// <summary>
    /// 成就通知数据类
    /// 用于显示成就相关通知
    /// </summary>
    [GlobalClass]
    public partial class AchievementNotification : Resource
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        [Export] public NotificationType Type { get; set; } = NotificationType.AchievementCompleted;
        
        /// <summary>
        /// 成就ID
        /// </summary>
        [Export] public string AchievementId { get; set; } = string.Empty;
        
        /// <summary>
        /// 通知标题
        /// </summary>
        [Export] public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 通知内容
        /// </summary>
        [Export] public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// 图标路径
        /// </summary>
        [Export] public string IconPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 显示时长（秒）
        /// </summary>
        [Export] public float Duration { get; set; } = 3.0f;
        
        /// <summary>
        /// 是否播放音效
        /// </summary>
        [Export] public bool PlaySound { get; set; } = true;
        
        /// <summary>
        /// 音效路径
        /// </summary>
        [Export] public string SoundPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 优先级（数值越高优先级越高）
        /// </summary>
        [Export] public int Priority { get; set; } = 0;
        
        /// <summary>
        /// 额外数据
        /// </summary>
        [Export] public Dictionary<string, Variant> ExtraData { get; set; } = new Dictionary<string, Variant>();
    }

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
        [Export] public string[] RecentCompletions { get; set; } = Array.Empty<string>();
        
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