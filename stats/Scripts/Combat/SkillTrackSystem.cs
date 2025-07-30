using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class SkillTrackSystem : Node
{
    [Export] public int TrackSize = 5; // 轨道上同时存在的技能数量
    
    private Queue<SkillCard> _skillTrack = new Queue<SkillCard>();
    private SkillCard _nextSkill; // 即将释放的技能
    
    public struct SkillActivation
    {
        public SkillCard Skill;
        public int ChargeCost;
        public bool CanActivate;
        // public string[] TriggerConditions;
    }
    
    public SkillActivation GetNextSkillActivation()
    {
        if (_nextSkill == null) return default;
        
        var activation = new SkillActivation
        {
            Skill = _nextSkill,
            ChargeCost = _nextSkill.Cost,
            CanActivate = TypingCombatSystem.Instance.CurrentCharge >= _nextSkill.Cost,
            // TriggerConditions = _nextSkill.TriggerConditions
        };
        
        return activation;
    }
    
    public void ActivateNextSkill()
    {
        var activation = GetNextSkillActivation();
        if (!activation.CanActivate) return;
        
        // 消耗充能
        // TypingCombatSystem.Instance.ConsumeCharge(activation.ChargeCost);
        
        // 执行技能效果
        ExecuteSkillEffect(activation.Skill);
        
        // 轮转技能轨道
        RotateSkillTrack();
    }

    private void RotateSkillTrack()
    {
        throw new NotImplementedException();
    }

    private void ExecuteSkillEffect(SkillCard skill)
    {
        switch (skill.SkillType)
        {
            case SkillType.Attack:
                ExecuteAttackSkill(skill);
                break;
            case SkillType.Defense:
                ExecuteDefenseSkill(skill);
                break;
            case SkillType.Utility:
                ExecuteUtilitySkill(skill);
                break;
            case SkillType.TypingEnhancement:
                ExecuteTypingEnhancement(skill);
                break;
        }
    }

    private void ExecuteUtilitySkill(SkillCard skill)
    {
        throw new NotImplementedException();
    }

    private void ExecuteDefenseSkill(SkillCard skill)
    {
        throw new NotImplementedException();
    }

    private void ExecuteAttackSkill(SkillCard skill)
    {
        throw new NotImplementedException();
    }

    private void ExecuteTypingEnhancement(SkillCard skill)
    {
        // 影响打字体验的技能
        // foreach (var effect in skill.Effects)
        // {
        //     switch (effect.Type)
        //     {
        //         case "ReduceWordLength":
        //             // 所有敌人单词长度-1
        //             EnemyManager.Instance.ApplyWordLengthModifier(-1, effect.Duration);
        //             break;
        //         case "DoubleCharge":
        //             // 双倍充能获取
        //             TypingCombatSystem.Instance.ApplyChargeMultiplier(2f, effect.Duration);
        //             break;
        //         case "SlowMotion":
        //             // 慢动作效果，给更多输入时间
        //             TimeManager.Instance.SetTimeScale(0.5f, effect.Duration);
        //             break;
        //         case "AutoComplete":
        //             // 自动完成部分字母
        //             InputManager.Instance.EnableAutoComplete(effect.Value, effect.Duration);
        //             break;
        //     }
        // }
    }
}