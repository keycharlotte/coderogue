using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AwakeningConfig : Resource
{
    [Export] public int AwakeningLevel { get; set; }         // 觉醒等级
    [Export] public Array<AwakeningRequirement> Requirements { get; set; } // 觉醒条件
    [Export] public Array<AwakeningBonus> Bonuses { get; set; } // 觉醒奖励
    [Export] public SpecialTraitConfig NewTrait { get; set; } // 新增特性
    
}