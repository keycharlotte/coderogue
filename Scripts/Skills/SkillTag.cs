using Godot;

[GlobalClass]
public partial class SkillTag : Resource
{
    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public Color Color { get; set; }
}