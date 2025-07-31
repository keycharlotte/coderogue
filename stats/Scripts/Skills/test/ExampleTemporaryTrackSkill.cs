using Godot;
using CodeRogue.Buffs;
using Godot.Collections;

namespace CodeRogue.Skills
{
    /// <summary>
    /// 示例：创建临时轨道的技能卡
    /// </summary>
    [GlobalClass]
    public partial class ExampleTemporaryTrackSkill : SkillCard
    {
        public void Ready()
        {
            // 设置技能基本信息
            Name = "召唤临时轨道";
            Description = "创建一个持续10秒的临时轨道，装载火球术";
            SkillType = SkillType.Utility;
            Rarity = CardRarity.Rare;
            
            // 创建临时轨道效果
            var tempTrackEffect = new TemporaryTrackSkillEffect
            {
                Type = SkillEffectType.TemporaryTrack,
                DestroyCondition = TemporaryTrackDestroyCondition.Timer,
                Duration = 10f,
                TriggerSkillId = "", // 空字符串表示不基于特定技能激活销毁
                BuffConfigId = 1001 // 需要在BuffDatabase中配置对应的Buff
            };
            
            // 创建要装载到临时轨道的技能（这里假设有一个火球术技能）
            var fireball = CreateFireballSkill();
            tempTrackEffect.TrackSkill = fireball;
            
            // 添加效果到技能
            Effects = new Array<SkillEffect> { tempTrackEffect };
        }
        
        private SkillCard CreateFireballSkill()
        {
            // 这里创建一个简单的火球术技能作为示例
            var fireball = new SkillCard
            {
                Name = "火球术",
                Description = "发射一个火球造成伤害",
                SkillType = SkillType.Attack,
                Rarity = CardRarity.Common,
            };
            
            // 添加伤害效果
            var damageEffect = new SkillEffect
            {
                Type = SkillEffectType.Damage,
                Value = 100f
            };
            
            fireball.Effects = new Array<SkillEffect> { damageEffect };
            return fireball;
        }
    }
}