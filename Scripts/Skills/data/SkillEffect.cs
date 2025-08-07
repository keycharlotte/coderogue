using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SkillEffect : Resource
{
    [Export] public SkillEffectType Type { get; set; }
    [Export] public SkillTargetType TargetType { get; set; } = SkillTargetType.Enemy;
    [Export] public string TargetProperty { get; set; }
    [Export] public float Value { get; set; }
    [Export] public float Duration { get; set; }
    [Export] public int BuffId { get; set; }
    [Export] public Dictionary Parameters { get; set; }
}