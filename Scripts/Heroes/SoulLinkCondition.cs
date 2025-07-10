using Godot;

[GlobalClass]
public partial class SoulLinkCondition : Resource
{
    [Export] public SoulLinkConditionType Type { get; set; } // 条件类型
    [Export] public Variant Value { get; set; }             // 条件值
    [Export] public string Description { get; set; }        // 条件描述
}