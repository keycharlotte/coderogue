using Godot;

/// <summary>
/// 死亡重来系统相关枚举定义
/// </summary>
namespace CodeRogue.Rebirth
{
    /// <summary>
    /// 商店商品类型
    /// </summary>
    public enum ShopItemType
    {
        CardUnlock,      // 卡牌解锁
        StartingBonus,   // 开局奖励
        PermanentUpgrade,// 永久升级
        HeroUnlock,      // 英雄解锁
        RelicUnlock,     // 遗物解锁
        CurrencyBonus,   // 货币奖励
        Unlock,          // 通用解锁
        TemporaryBoost,  // 临时增强
        Special          // 特殊商品
    }

    /// <summary>
    /// 货币类型
    /// </summary>
    public enum CurrencyType
    {
        RebirthCoin,     // 投胎币
        BonusCoin,       // 奖励币
        PremiumCoin      // 高级币
    }

    /// <summary>
    /// 购买状态
    /// </summary>
    public enum PurchaseStatus
    {
        Available,       // 可购买
        Purchased,       // 已购买
        Locked,          // 锁定
        OutOfStock,      // 缺货
        InsufficientFunds // 资金不足
    }

    /// <summary>
    /// 新颖度等级
    /// </summary>
    public enum NoveltyLevel
    {
        None,            // 无新颖度
        Low,             // 低新颖度
        Medium,          // 中等新颖度
        High,            // 高新颖度
        Common,          // 普通
        Uncommon,        // 不常见
        Rare,            // 稀有
        Epic,            // 史诗
        Legendary,       // 传奇新颖度
        Mythical         // 神话
    }

    /// <summary>
    /// 游戏结果类型
    /// </summary>
    public enum GameResultType
    {
        Victory,         // 胜利
        Defeat,          // 失败
        Quit,            // 退出
        Timeout          // 超时
    }

    /// <summary>
    /// 商品稀有度
    /// </summary>
    public enum ShopItemRarity
    {
        Common,          // 普通
        Uncommon,        // 不常见
        Rare,            // 稀有
        Epic,            // 史诗
        Legendary        // 传奇
    }

    /// <summary>
    /// 奖励效果类型
    /// </summary>
    public enum RewardEffectType
    {
        Immediate,       // 立即生效
        NextGame,        // 下局生效
        Permanent,       // 永久生效
        Temporary        // 临时生效
    }

    /// <summary>
    /// 商店页面类型
    /// </summary>
    public enum ShopPageType
    {
        All,             // 全部页面
        Main,            // 主页面
        Cards,           // 卡牌页面
        Heroes,          // 英雄页面
        Relics,          // 遗物页面
        Upgrades,        // 升级页面
        Unlocks,         // 解锁页面
        Boosts,          // 增强页面
        Special          // 特殊页面
    }

    /// <summary>
    /// 统计数据类型
    /// </summary>
    public enum StatisticType
    {
        TotalGames,      // 总游戏次数
        TotalVictories,  // 总胜利次数
        TotalCurrency,   // 总货币获得
        BestFloor,       // 最佳层数
        PlayTime,        // 游戏时长
        NoveltyBonus     // 新颖度奖励
    }

    /// <summary>
    /// 组合评估结果
    /// </summary>
    public enum CombinationRating
    {
        Poor,            // 差
        Fair,            // 一般
        Good,            // 好
        Excellent,       // 优秀
        Perfect          // 完美
    }
}