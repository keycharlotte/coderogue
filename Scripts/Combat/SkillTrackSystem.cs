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
        // 轮转技能轨道：移除已使用的技能，添加新技能
        if (_skillTrack.Count > 0)
        {
            _skillTrack.Dequeue(); // 移除刚使用的技能
        }
        
        // 更新下一个技能
        _nextSkill = _skillTrack.Count > 0 ? _skillTrack.Peek() : null;
        
        // 如果轨道为空，可以从牌组中补充新技能
        if (_skillTrack.Count < TrackSize)
        {
            // TODO: 从当前牌组中获取新技能
            // var deckManager = GetNode<DeckManager>("/root/DeckManager");
            // if (deckManager != null)
            // {
            //     var newSkill = deckManager.DrawSkillCard();
            //     if (newSkill != null)
            //     {
            //         _skillTrack.Enqueue(newSkill);
            //     }
            // }
        }
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
        // 执行辅助类技能效果
        foreach (var effect in skill.Effects)
        {
            switch (effect.Type)
            {
                case SkillEffectType.Heal:
                    // 治疗玩家
                    var player = GetTree().GetFirstNodeInGroup("player");
                    if (player != null && player.HasMethod("Heal"))
                    {
                        player.Call("Heal", effect.Value);
                    }
                    break;
                case SkillEffectType.Shield:
                    // 为玩家添加护盾
                    var playerShield = GetTree().GetFirstNodeInGroup("player");
                    if (playerShield != null && playerShield.HasMethod("AddShield"))
                    {
                        playerShield.Call("AddShield", effect.Value, effect.Duration);
                    }
                    break;
                case SkillEffectType.Buff:
                    // 应用增益效果
                    var buffManager = GetNode<BuffManager>("/root/BuffManager");
                    if (buffManager != null)
                    {
                        var playerBuff = GetTree().GetFirstNodeInGroup("player");
                        if (playerBuff != null)
                        {
                            buffManager.ApplyBuff(effect.BuffId, playerBuff);
                        }
                    }
                    break;
            }
        }
    }

    private void ExecuteDefenseSkill(SkillCard skill)
    {
        // 执行防御类技能效果
        foreach (var effect in skill.Effects)
        {
            switch (effect.Type)
            {
                case SkillEffectType.Shield:
                    // 为玩家添加护盾
                    var player = GetTree().GetFirstNodeInGroup("player");
                    if (player != null && player.HasMethod("AddShield"))
                    {
                        player.Call("AddShield", effect.Value, effect.Duration);
                    }
                    break;
                case SkillEffectType.Damage:
                    // 反击伤害
                    var enemies = GetTree().GetNodesInGroup("enemies");
                    foreach (Node enemy in enemies)
                    {
                        if (enemy.HasMethod("TakeDamage"))
                        {
                            enemy.Call("TakeDamage", effect.Value);
                        }
                    }
                    break;
                case SkillEffectType.Buff:
                    // 防御增益
                    var buffManager = GetNode<BuffManager>("/root/BuffManager");
                    if (buffManager != null)
                    {
                        var playerBuff = GetTree().GetFirstNodeInGroup("player");
                        if (playerBuff != null)
                        {
                            buffManager.ApplyBuff(effect.BuffId, playerBuff);
                        }
                    }
                    break;
            }
        }
    }

    private void ExecuteAttackSkill(SkillCard skill)
    {
        // 执行攻击类技能效果
        foreach (var effect in skill.Effects)
        {
            switch (effect.Type)
            {
                case SkillEffectType.Damage:
                    // 对敌人造成伤害
                    var enemies = GetTree().GetNodesInGroup("enemies");
                    foreach (Node enemy in enemies)
                    {
                        if (enemy.HasMethod("TakeDamage"))
                        {
                            enemy.Call("TakeDamage", effect.Value);
                        }
                    }
                    break;
                case SkillEffectType.Buff:
                    // 对敌人施加负面效果
                    var buffManager = GetNode<BuffManager>("/root/BuffManager");
                    if (buffManager != null)
                    {
                        var enemyTargets = GetTree().GetNodesInGroup("enemies");
                        foreach (Node enemy in enemyTargets)
                        {
                            buffManager.ApplyBuff(effect.BuffId, enemy);
                        }
                    }
                    break;
                case SkillEffectType.TypingModifier:
                    // 影响打字体验
                    var typingSystem = GetNode<TypingCombatSystem>("/root/TypingCombatSystem");
                    if (typingSystem != null)
                    {
                        // 根据效果类型应用不同的打字修改器
                        // typingSystem.ApplyModifier(effect);
                    }
                    break;
            }
        }
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