using Godot;
using Godot.Collections;

[GlobalClass]
public partial class StarUpCost : Resource
{
    [Export] public int Star { get; set; }                  // 目标星级
    [Export] public int Gold { get; set; }                  // 金币消耗
    [Export] public int HeroFragments { get; set; }         // 英雄碎片
    [Export] public Godot.Collections.Dictionary<int, int> Materials { get; set; } // 材料消耗
    
}