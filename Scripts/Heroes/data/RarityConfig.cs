using Godot;
using Godot.Collections;

[GlobalClass]
public partial class RarityConfig : Resource
{
    [Export] public HeroRarity Rarity { get; set; }
    [Export] public string Name { get; set; }               // 品级名称
    [Export] public Color Color { get; set; }               // 品级颜色
    [Export] public string BorderTexture { get; set; }      // 边框贴图
    [Export] public string EffectTexture { get; set; }      // 特效贴图
    
    // 属性加成
    [Export] public float StatMultiplier { get; set; }      // 属性倍率
    [Export] public int MaxLevel { get; set; }              // 最大等级
    [Export] public int MaxStar { get; set; }               // 最大星级
    [Export] public int MaxAwakening { get; set; }          // 最大觉醒
    
    // 获取概率
    [Export] public float DropRate { get; set; }            // 掉落概率
    [Export] public int FragmentsToSummon { get; set; }     // 召唤所需碎片
    
    // 升级消耗
    [Export] public Array<LevelUpCost> LevelUpCosts { get; set; } // 升级消耗
    [Export] public Array<StarUpCost> StarUpCosts { get; set; }   // 升星消耗
    

}