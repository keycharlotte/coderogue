public enum SkillType
{
    Attack,
    Defense,
    Movement,
    Heal,           // 治疗技能
    Buff,           // 增益技能
    Debuff,         // 减益技能
    Control,        // 控制技能
    Utility,        // 实用技能
    TypingEnhancement, // 打字增强
    Special         // 特殊技能
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