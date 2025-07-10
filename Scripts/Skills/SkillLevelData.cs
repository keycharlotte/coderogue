using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SkillLevelData : Resource
{
    [Export] public int Level { get; set; }
    [Export] public int ChargeCost { get; set; }
    [Export] public Array<float> EffectValues { get; set; }
    [Export] public string LevelDescription { get; set; }
}