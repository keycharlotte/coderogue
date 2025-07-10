using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SpecialTraitConfig : Resource
{
    [Export] public int Id { get; set; }                    // 特性ID
    [Export] public string Name { get; set; }               // 特性名称
    [Export] public string Description { get; set; }        // 特性描述
    [Export] public string IconPath { get; set; }           // 图标路径
    [Export] public SpecialTraitType Type { get; set; }     // 特性类型
    [Export] public SpecialTraitTrigger Trigger { get; set; } // 触发条件
    
    // 效果配置
    [Export] public Array<TraitEffect> Effects { get; set; } // 效果列表
    [Export] public Dictionary Parameters { get; set; }     // 参数配置
    
    // 等级相关
    [Export] public bool ScalesWithLevel { get; set; }      // 是否随等级缩放
    [Export] public float LevelScaling { get; set; }        // 等级缩放系数
    
}