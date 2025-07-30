using Godot;

[GlobalClass]
public partial class AwakeningBonus : Resource
{
    [Export] public string BonusType { get; set; }          // 奖励类型
    [Export] public float Value { get; set; }               // 奖励数值
    [Export] public string Description { get; set; }        // 奖励描述
}