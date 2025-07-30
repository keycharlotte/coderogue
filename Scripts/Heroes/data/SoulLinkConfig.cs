using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SoulLinkConfig : Resource
{
    [Export] public int Id { get; set; }                    // 链接ID
    [Export] public string Name { get; set; }               // 链接名称
    [Export] public string Description { get; set; }        // 链接描述
    [Export] public SoulLinkType Type { get; set; }         // 链接类型
    
    // 链接条件
    [Export] public Array<SoulLinkCondition> Conditions { get; set; } // 链接条件
    
    // 提供的被动效果
    [Export] public Array<PassiveEffect> PassiveEffects { get; set; } // 被动效果
    
    // 链接限制
    [Export] public int MaxLinks { get; set; } = 1;         // 最大链接数
    [Export] public bool RequiresUnlock { get; set; }       // 是否需要解锁
    [Export] public Array<int> UnlockRequirements { get; set; } // 解锁条件
    
}