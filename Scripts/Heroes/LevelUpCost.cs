using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LevelUpCost : Resource
{
    [Export] public int Level { get; set; }                 // 目标等级
    [Export] public int Gold { get; set; }                  // 金币消耗
    [Export] public int Experience { get; set; }            // 经验消耗
    [Export] public Dictionary<int, int> Materials { get; set; } // 材料消耗
    
}