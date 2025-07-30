using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AwakeningRequirement : Resource
{
    [Export] public AwakeningRequirementType Type { get; set; }
    [Export] public int Value { get; set; }
    [Export] public Array<int> MaterialIds { get; set; }
    

}