using Godot;

[GlobalClass]
public partial class HeroStats : Resource
{
    [Export] public float Health { get; set; }              // 生命值
    [Export] public float Attack { get; set; }              // 攻击力
    [Export] public float Defense { get; set; }             // 防御力
    [Export] public float CritRate { get; set; }            // 暴击率
    [Export] public float CritDamage { get; set; }          // 暴击伤害
}