using Godot;
using Godot.Collections;

[GlobalClass]
public partial class TraitEffect : Resource
{
    [Export] public TraitEffectType EffectType { get; set; } // 效果类型
    [Export] public string TargetType { get; set; }         // 目标类型
    [Export] public float Value { get; set; }               // 效果数值
    [Export] public float Duration { get; set; }            // 持续时间
    [Export] public int MaxStacks { get; set; }             // 最大叠加
    [Export] public Dictionary Parameters { get; set; }     // 额外参数
    
}