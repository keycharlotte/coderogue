using Godot;
using Godot.Collections;
using CodeRogue.Achievements.Data;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就类型枚举
    /// </summary>
    public enum AchievementType
    {
        /// <summary>计数型成就（如击败100个敌人）</summary>
        Counter,
        /// <summary>里程碑成就（如达到某个等级）</summary>
        Milestone,
        /// <summary>收集型成就（如收集所有技能卡）</summary>
        Collection,
        /// <summary>序列型成就（如连续完成某个动作）</summary>
        Sequence,
        /// <summary>条件型成就（如在特定条件下完成任务）</summary>
        Condition
    }

    /// <summary>
    /// 成就分类枚举
    /// </summary>
    public enum AchievementCategory
    {
        /// <summary>全部成就</summary>
        All,
        /// <summary>战斗相关成就</summary>
        Combat,
        /// <summary>技能相关成就</summary>
        Skill,
        /// <summary>英雄相关成就</summary>
        Hero,
        /// <summary>收集相关成就</summary>
        Collection,
        /// <summary>进度相关成就</summary>
        Progress,
        /// <summary>特殊成就</summary>
        Special,
        /// <summary>探索相关成就</summary>
        Exploration,
        /// <summary>社交相关成就</summary>
        Social,
        /// <summary>进展相关成就</summary>
        Progression,
        /// <summary>隐藏成就</summary>
        Hidden
    }

    /// <summary>
    /// 成就稀有度枚举
    /// </summary>
    public enum AchievementRarity
    {
        /// <summary>普通成就</summary>
        Common,
        /// <summary>不常见成就</summary>
        Uncommon,
        /// <summary>稀有成就</summary>
        Rare,
        /// <summary>史诗成就</summary>
        Epic,
        /// <summary>传说成就</summary>
        Legendary
    }

    /// <summary>
    /// 游戏事件类型枚举
    /// </summary>
    public enum GameEventType
    {
        /// <summary>敌人被击败</summary>
        EnemyDefeated,
        /// <summary>技能被使用</summary>
        SkillUsed,
        /// <summary>英雄升级</summary>
        HeroLevelUp,
        /// <summary>物品被收集</summary>
        ItemCollected,
        /// <summary>关卡完成</summary>
        LevelCompleted,
        /// <summary>造成伤害</summary>
        DamageDealt,
        /// <summary>受到伤害</summary>
        DamageReceived,
        /// <summary>连击达成</summary>
        ComboAchieved,
        /// <summary>战斗胜利</summary>
        BattleWon,
        /// <summary>战斗失败</summary>
        BattleLost,
        /// <summary>技能学习</summary>
        SkillLearned,
        /// <summary>英雄解锁</summary>
        HeroUnlocked,
        /// <summary>装备获得</summary>
        EquipmentObtained,
        /// <summary>遗物获得</summary>
        RelicObtained,
        /// <summary>金币获得</summary>
        GoldEarned,
        /// <summary>经验获得</summary>
        ExperienceGained,
        /// <summary>游戏开始</summary>
        GameStarted,
        /// <summary>游戏结束</summary>
        GameEnded,
        /// <summary>存档保存</summary>
        GameSaved,
        /// <summary>存档加载</summary>
        GameLoaded,
        /// <summary>战斗开始</summary>
        BattleStarted,
        /// <summary>战斗完成</summary>
        BattleCompleted,
        /// <summary>英雄招募</summary>
        HeroRecruited,
        /// <summary>物品使用</summary>
        ItemUsed,
        /// <summary>手动触发</summary>
        Manual
    }

    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum RewardType
    {
        None = 0,
        /// <summary>经验值</summary>
        Experience = 1,
        /// <summary>金币</summary>
        Gold = 2,
        /// <summary>技能卡</summary>
        SkillCard = 3,
        /// <summary>英雄</summary>
        Hero = 4,
        /// <summary>遗物</summary>
        Relic = 5,
        /// <summary>称号</summary>
        Title = 6,
        /// <summary>头像</summary>
        Avatar = 7,
        /// <summary>徽章</summary>
        Badge = 8,
        /// <summary>解锁内容</summary>
        Unlock = 9,
        /// <summary>成就奖励</summary>
        Achievement = 10,
        Card = 11,
        Item = 12
    }

    /// <summary>
    /// 比较类型枚举
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>等于</summary>
        Equal,
        /// <summary>不等于</summary>
        NotEqual,
        /// <summary>大于</summary>
        Greater,
        /// <summary>大于等于</summary>
        GreaterOrEqual,
        /// <summary>小于</summary>
        Less,
        /// <summary>小于等于</summary>
        LessOrEqual,
        /// <summary>包含</summary>
        Contains,
        /// <summary>不包含</summary>
        NotContains
    }

    /// <summary>
    /// 条件逻辑类型枚举
    /// </summary>
    public enum ConditionLogicType
    {
        /// <summary>所有条件都必须满足</summary>
        And,
        /// <summary>任一条件满足即可</summary>
        Or,
        /// <summary>按特定顺序满足条件</summary>
        Sequence
    }

    /// <summary>
    /// 成就状态枚举
    /// </summary>
    public enum AchievementStatus
    {
        /// <summary>锁定状态</summary>
        Locked,
        /// <summary>解锁但未完成</summary>
        Unlocked,
        /// <summary>进行中</summary>
        InProgress,
        /// <summary>已完成</summary>
        Completed,
        /// <summary>奖励已领取</summary>
        RewardClaimed
    }

    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        /// <summary>成就完成通知</summary>
        AchievementCompleted,
        /// <summary>成就解锁通知</summary>
        AchievementUnlocked,
        /// <summary>进度更新通知</summary>
        ProgressUpdated,
        /// <summary>奖励获得通知</summary>
        RewardReceived
    }

    /// <summary>
    /// 通知优先级枚举
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>低优先级</summary>
        Low,
        /// <summary>普通优先级</summary>
        Normal,
        /// <summary>高优先级</summary>
        High,
        /// <summary>紧急优先级</summary>
        Critical
    }

    /// <summary>
    /// 奖励结果状态枚举
    /// </summary>
    public enum RewardResultStatus
    {
        /// <summary>成功</summary>
        Success,
        /// <summary>失败</summary>
        Failed,
        /// <summary>已经领取过</summary>
        AlreadyClaimed,
        /// <summary>条件不满足</summary>
        ConditionNotMet,
        /// <summary>奖励不存在</summary>
        RewardNotFound,
        /// <summary>背包空间不足</summary>
        InsufficientSpace
    }
    
    /// <summary>
    /// 奖励结果类
    /// </summary>
    public partial class RewardResult : RefCounted
    {
        /// <summary>是否成功</summary>
        public bool Success { get; set; } = false;
        
        /// <summary>结果消息</summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>实际发放数量</summary>
        public int GrantedAmount { get; set; } = 0;
        
        /// <summary>发放的奖励列表</summary>
        public Array<RewardConfig> GrantedRewards { get; set; } = new Array<RewardConfig>();
        
        /// <summary>失败的奖励列表</summary>
        public Array<RewardConfig> FailedRewards { get; set; } = new Array<RewardConfig>();
        
        /// <summary>结果状态</summary>
        public RewardResultStatus Status { get; set; } = RewardResultStatus.Failed;
    }
}