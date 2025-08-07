using Godot;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就类型枚举
    /// </summary>
    public enum AchievementType
    {
        /// <summary>
        /// 计数型成就（如击败100个敌人）
        /// </summary>
        Counter,
        
        /// <summary>
        /// 里程碑成就（如达到某个等级）
        /// </summary>
        Milestone,
        
        /// <summary>
        /// 收集型成就（如收集所有技能卡）
        /// </summary>
        Collection,
        
        /// <summary>
        /// 序列型成就（如连续完成某个动作）
        /// </summary>
        Sequence,
        
        /// <summary>
        /// 条件型成就（如在特定条件下完成任务）
        /// </summary>
        Condition
    }

    /// <summary>
    /// 成就分类枚举
    /// </summary>
    public enum AchievementCategory
    {
        /// <summary>
        /// 战斗相关成就
        /// </summary>
        Combat,
        
        /// <summary>
        /// 技能相关成就
        /// </summary>
        Skill,
        
        /// <summary>
        /// 英雄相关成就
        /// </summary>
        Hero,
        
        /// <summary>
        /// 收集相关成就
        /// </summary>
        Collection,
        
        /// <summary>
        /// 进度相关成就
        /// </summary>
        Progress,
        
        /// <summary>
        /// 特殊成就
        /// </summary>
        Special,
        
        /// <summary>
        /// 遗物相关成就
        /// </summary>
        Relic,
        
        /// <summary>
        /// 关卡相关成就
        /// </summary>
        Level
    }

    /// <summary>
    /// 成就稀有度枚举
    /// </summary>
    public enum AchievementRarity
    {
        /// <summary>
        /// 普通成就
        /// </summary>
        Common,
        
        /// <summary>
        /// 稀有成就
        /// </summary>
        Rare,
        
        /// <summary>
        /// 史诗成就
        /// </summary>
        Epic,
        
        /// <summary>
        /// 传说成就
        /// </summary>
        Legendary
    }

    /// <summary>
    /// 游戏事件类型枚举
    /// </summary>
    public enum GameEventType
    {
        // 战斗相关事件
        /// <summary>
        /// 敌人被击败
        /// </summary>
        EnemyDefeated,
        
        /// <summary>
        /// 战斗胜利
        /// </summary>
        BattleWon,
        
        /// <summary>
        /// 战斗失败
        /// </summary>
        BattleLost,
        
        /// <summary>
        /// 造成伤害
        /// </summary>
        DamageDealt,
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        DamageReceived,
        
        /// <summary>
        /// 连击达成
        /// </summary>
        ComboAchieved,
        
        /// <summary>
        /// 暴击
        /// </summary>
        CriticalHit,
        
        // 技能相关事件
        /// <summary>
        /// 技能使用
        /// </summary>
        SkillUsed,
        
        /// <summary>
        /// 技能学习
        /// </summary>
        SkillLearned,
        
        /// <summary>
        /// 技能升级
        /// </summary>
        SkillUpgraded,
        
        /// <summary>
        /// 技能组合使用
        /// </summary>
        SkillComboUsed,
        
        // 英雄相关事件
        /// <summary>
        /// 英雄升级
        /// </summary>
        HeroLevelUp,
        
        /// <summary>
        /// 英雄解锁
        /// </summary>
        HeroUnlocked,
        
        /// <summary>
        /// 英雄装备
        /// </summary>
        HeroEquipped,
        
        /// <summary>
        /// 英雄进化
        /// </summary>
        HeroEvolved,
        
        // 收集相关事件
        /// <summary>
        /// 物品收集
        /// </summary>
        ItemCollected,
        
        /// <summary>
        /// 卡牌获得
        /// </summary>
        CardObtained,
        
        /// <summary>
        /// 遗物获得
        /// </summary>
        RelicObtained,
        
        /// <summary>
        /// 金币获得
        /// </summary>
        GoldEarned,
        
        /// <summary>
        /// 经验获得
        /// </summary>
        ExperienceGained,
        
        // 进度相关事件
        /// <summary>
        /// 关卡完成
        /// </summary>
        LevelCompleted,
        
        /// <summary>
        /// 章节完成
        /// </summary>
        ChapterCompleted,
        
        /// <summary>
        /// 游戏完成
        /// </summary>
        GameCompleted,
        
        /// <summary>
        /// 玩家等级提升
        /// </summary>
        PlayerLevelUp,
        
        // 特殊事件
        /// <summary>
        /// 连续登录
        /// </summary>
        ConsecutiveLogin,
        
        /// <summary>
        /// 游戏时长
        /// </summary>
        PlayTime,
        
        /// <summary>
        /// 完美通关
        /// </summary>
        PerfectClear,
        
        /// <summary>
        /// 无伤通关
        /// </summary>
        NoDamageClear
    }

    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum RewardType
    {
        /// <summary>
        /// 经验值
        /// </summary>
        Experience,
        
        /// <summary>
        /// 金币
        /// </summary>
        Gold,
        
        /// <summary>
        /// 技能卡
        /// </summary>
        SkillCard,
        
        /// <summary>
        /// 英雄
        /// </summary>
        Hero,
        
        /// <summary>
        /// 遗物
        /// </summary>
        Relic,
        
        /// <summary>
        /// 称号
        /// </summary>
        Title,
        
        /// <summary>
        /// 头像
        /// </summary>
        Avatar,
        
        /// <summary>
        /// 徽章
        /// </summary>
        Badge,
        
        /// <summary>
        /// 解锁内容
        /// </summary>
        Unlock,
        
        /// <summary>
        /// 装备
        /// </summary>
        Equipment,
        
        /// <summary>
        /// 皮肤
        /// </summary>
        Skin
    }

    /// <summary>
    /// 比较类型枚举
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// 等于
        /// </summary>
        Equal,
        
        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual,
        
        /// <summary>
        /// 大于
        /// </summary>
        Greater,
        
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterOrEqual,
        
        /// <summary>
        /// 小于
        /// </summary>
        Less,
        
        /// <summary>
        /// 小于等于
        /// </summary>
        LessOrEqual,
        
        /// <summary>
        /// 包含
        /// </summary>
        Contains,
        
        /// <summary>
        /// 不包含
        /// </summary>
        NotContains
    }

    /// <summary>
    /// 成就状态枚举
    /// </summary>
    public enum AchievementStatus
    {
        /// <summary>
        /// 锁定状态（未满足前置条件）
        /// </summary>
        Locked,
        
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,
        
        /// <summary>
        /// 已完成但未领取奖励
        /// </summary>
        Completed,
        
        /// <summary>
        /// 已完成且已领取奖励
        /// </summary>
        Claimed,
        
        /// <summary>
        /// 隐藏状态
        /// </summary>
        Hidden
    }

    /// <summary>
    /// 条件逻辑类型枚举
    /// </summary>
    public enum ConditionLogicType
    {
        /// <summary>
        /// 所有条件都必须满足
        /// </summary>
        And,
        
        /// <summary>
        /// 任一条件满足即可
        /// </summary>
        Or,
        
        /// <summary>
        /// 按顺序满足条件
        /// </summary>
        Sequence
    }

    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 成就完成通知
        /// </summary>
        AchievementCompleted,
        
        /// <summary>
        /// 进度更新通知
        /// </summary>
        ProgressUpdated,
        
        /// <summary>
        /// 奖励获得通知
        /// </summary>
        RewardReceived,
        
        /// <summary>
        /// 新成就解锁通知
        /// </summary>
        AchievementUnlocked
    }
}