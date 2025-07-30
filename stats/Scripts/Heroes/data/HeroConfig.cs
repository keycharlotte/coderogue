using Godot;
using Godot.Collections;

[GlobalClass]
public partial class HeroConfig : Resource
{
    [Export] public int Id { get; set; }                    // 英雄唯一ID
    [Export] public string Name { get; set; }               // 英雄名称
    [Export] public string Description { get; set; }        // 英雄描述
    [Export] public HeroRarity Rarity { get; set; }         // 英雄品级
    [Export] public HeroClass Class { get; set; }           // 英雄职业
    [Export] public string AvatarPath { get; set; }         // 头像路径
    [Export] public string ModelPath { get; set; }          // 模型路径
    
    // 基础属性
    [Export] public HeroStats BaseStats { get; set; }       // 基础属性
    [Export] public HeroStats GrowthStats { get; set; }     // 成长属性
    
    // 专属特性
    [Export] public SpecialTraitConfig SpecialTrait { get; set; } // 专属特性
    
    // 技能配置
    [Export] public Array<int> SkillIds { get; set; }       // 技能ID列表
    
    // 灵魂链接
    [Export] public SoulLinkConfig SoulLink { get; set; }   // 灵魂链接配置
    
    // 获取途径
    [Export] public Array<HeroObtainMethod> ObtainMethods { get; set; } // 获取方式
    
    // 召唤师相关配置
    [Export] public int MaxColorSlots { get; set; } = 5;    // 最大颜色槽位数
    [Export] public MagicColor PrimaryColor { get; set; }   // 主要颜色
    
    // 种族和羁绊加成配置
    [Export] public Godot.Collections.Dictionary<MonsterRace, float> RaceBonus { get; set; } = new Godot.Collections.Dictionary<MonsterRace, float>();
    [Export] public Godot.Collections.Dictionary<BondType, float> BondBonus { get; set; } = new Godot.Collections.Dictionary<BondType, float>();
    
    // 打字战斗基础配置
    [Export] public float TypingDamageBase { get; set; } = 100f;
    [Export] public float TypingDamageDecayRate { get; set; } = 0.1f; // 每关卡衰减率
    [Export] public float TypingSpeedBonus { get; set; } = 1.0f;
    [Export] public float TypingAccuracyBonus { get; set; } = 1.0f;
    [Export] public float TypingDecayResistance { get; set; } = 0.0f; // 打字衰减抗性
    
    // 召唤师技能配置
    [Export] public Array<SummonerSkillType> DefaultSummonerSkills { get; set; } = new Array<SummonerSkillType>();
    [Export] public Array<float> DefaultSkillValues { get; set; } = new Array<float>();
    
}