using Godot;
using CodeRogue.Skills;

namespace CodeRogue.Buffs
{
    /// <summary>
    /// 创建临时技能轨道的Buff配置
    /// </summary>
    public partial class TemporaryTrackBuffConfig : BuffConfig
    {
        [Export] public TemporaryTrackDestroyCondition DestroyCondition { get; set; } = TemporaryTrackDestroyCondition.Timer;
        [Export] public string TriggerSkillId { get; set; } = "";
        [Export] public SkillCard TrackSkill { get; set; } // 要装载到临时轨道的技能
        
        public TemporaryTrackBuffConfig()
        {
            // 设置基本属性
            Name = "临时轨道";
            Description = "创建一个临时技能轨道";
            Type = BuffType.Buff;
            Category = BuffCategory.Trigger;
        }
    }
}