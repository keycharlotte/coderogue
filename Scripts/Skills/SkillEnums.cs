public enum SkillType
{
    Attack,
    Defense,
    Utility,
    Movement,
    TypingEnhancement
}

public enum SkillRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum SkillEffectType
{
    Damage,
    Heal,
    Shield,
    Buff,
    Debuff,
    Movement,
    TypingModifier,
    ChargeModifier,
    TemporaryTrack
}

/// <summary>
/// 临时轨道销毁条件
/// </summary>
public enum TemporaryTrackDestroyCondition
{
    Timer,          // 计时器到期
    TrackEmpty,     // 轨道为空时
    SkillActivated  // 特定技能激活时
}