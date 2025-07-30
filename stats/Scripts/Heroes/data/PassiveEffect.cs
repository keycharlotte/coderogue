using Godot;

[GlobalClass]
public partial class PassiveEffect : Resource
{
    [Export] public PassiveEffectType Type { get; set; }    // 效果类型
    [Export] public string TargetProperty { get; set; }     // 目标属性
    [Export] public float Value { get; set; }               // 效果数值
    [Export] public bool IsPercentage { get; set; }         // 是否为百分比
    [Export] public string Description { get; set; }        // 效果描述
}