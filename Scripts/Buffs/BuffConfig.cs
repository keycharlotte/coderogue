using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BuffConfig : Resource
{
    [Export] public int Id { get; set; }
    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public BuffType Type { get; set; }
    [Export] public BuffCategory Category { get; set; }
    [Export] public int MaxStack { get; set; } = 1;
    [Export] public BuffStackRule StackRule { get; set; } = BuffStackRule.None;
    [Export] public float BaseDuration { get; set; }
    [Export] public int Priority { get; set; } = 100;
    [Export] public string IconPath { get; set; }
    [Export] public Color DisplayColor { get; set; } = Colors.White;
    [Export] public bool CanDispel { get; set; } = true;
    [Export] public bool IsPersistent { get; set; } = false;
    [Export] public Array<string> Tags { get; set; } = new();
    [Export] public Array<BuffEffectData> Effects { get; set; } = new();
    [Export] public Dictionary CustomData { get; set; } = new();
}