using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SkillEffect : Resource
{
    [Export] public SkillEffectType Type { get; set; }
    [Export] public string TargetProperty { get; set; }
    [Export] public float Value { get; set; }
    [Export] public float Duration { get; set; }
    [Export] public Dictionary Parameters { get; set; }
}