using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 召唤师英雄数据结构
/// 管理英雄的颜色槽位、召唤能力和打字战斗
/// </summary>
[GlobalClass]
public partial class SummonerHero : Resource
{
    [Export] public int Id { get; set; }
    [Export] public string HeroName { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public Array<MagicColor> ColorSlots { get; set; } = new Array<MagicColor>();
    [Export] public int MaxColorSlots { get; set; } = 5;
    [Export] public MagicColor PrimaryColor { get; set; }
    
    // 种族和羁绊加成
    [Export] public Godot.Collections.Dictionary<MonsterRace, float> RaceBonus { get; set; } = new Godot.Collections.Dictionary<MonsterRace, float>();
    [Export] public Godot.Collections.Dictionary<BondType, float> BondBonus { get; set; } = new Godot.Collections.Dictionary<BondType, float>();
    
    // 打字伤害相关属性
    [Export] public float TypingDamageBase { get; set; } = 100f;
    [Export] public float TypingDamageDecayRate { get; set; } = 0.1f; // 每关卡衰减率
    [Export] public float TypingSpeedBonus { get; set; } = 1.0f;
    [Export] public float TypingAccuracyBonus { get; set; } = 1.0f;
    [Export] public float TypingDecayResistance { get; set; } = 0.0f; // 打字衰减抗性
    
    // 召唤师技能
    [Export] public Array<SummonerSkillType> SummonerSkills { get; set; } = new Array<SummonerSkillType>();
    [Export] public Array<float> SkillValues { get; set; } = new Array<float>();
    
    // 视觉资源
    [Export] public string IconPath { get; set; } = "";
    [Export] public string PortraitPath { get; set; } = "";
    
    public SummonerHero()
    {
        ColorSlots = new Array<MagicColor>();
        RaceBonus = new Godot.Collections.Dictionary<MonsterRace, float>();
        BondBonus = new Godot.Collections.Dictionary<BondType, float>();
        SummonerSkills = new Array<SummonerSkillType>();
        SkillValues = new Array<float>();
        
        // 初始化默认颜色槽位（3个主颜色）
        InitializeDefaultColorSlots();
    }
    
    /// <summary>
    /// 初始化默认颜色槽位
    /// </summary>
    public void InitializeDefaultColorSlots()
    {
        if (ColorSlots.Count == 0)
        {
            // 默认给予3个主颜色槽位
            for (int i = 0; i < 3; i++)
            {
                ColorSlots.Add(PrimaryColor);
            }
        }
    }
    
    /// <summary>
    /// 检查是否可以召唤指定怪物
    /// </summary>
    public bool CanSummonMonster(MonsterCard monster)
    {
        return CanSatisfyColorRequirements(monster.ColorRequirements);
    }
    
    /// <summary>
    /// 检查颜色槽位是否满足怪物的颜色需求
    /// </summary>
    private bool CanSatisfyColorRequirements(Array<MagicColor> requirements)
    {
        var availableSlots = new List<MagicColor>(ColorSlots.ToList());
        
        foreach (var requiredColor in requirements)
        {
            if (availableSlots.Contains(requiredColor))
            {
                availableSlots.Remove(requiredColor);
            }
            else
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取对指定怪物的召唤加成
    /// </summary>
    public float GetSummonBonus(MonsterCard monster)
    {
        float bonus = 0f;
        
        // 种族加成
        if (RaceBonus.ContainsKey(monster.Race))
            bonus += RaceBonus[monster.Race];
            
        // 颜色亲和加成
        if (monster.HasColorRequirement(PrimaryColor))
            bonus += 0.2f;
            
        // 羁绊加成
        foreach (var bondType in monster.BondTypes)
        {
            if (BondBonus.ContainsKey(bondType))
                bonus += BondBonus[bondType];
        }
        
        return bonus;
    }
    
    /// <summary>
    /// 计算召唤师的打字伤害
    /// </summary>
    public float CalculateTypingDamage(int currentLevel, float typingSpeed, float accuracy)
    {
        // 基础伤害随关卡衰减
        float levelDecay = Mathf.Pow(1f - TypingDamageDecayRate, currentLevel - 1);
        float baseDamage = TypingDamageBase * levelDecay;
        
        // 打字速度和准确度加成
        float speedMultiplier = 1f + (typingSpeed - 1f) * TypingSpeedBonus;
        float accuracyMultiplier = 1f + (accuracy - 1f) * TypingAccuracyBonus;
        
        return baseDamage * speedMultiplier * accuracyMultiplier;
    }
    
    /// <summary>
    /// 添加颜色槽位
    /// </summary>
    public bool AddColorSlot(MagicColor color)
    {
        if (ColorSlots.Count < MaxColorSlots)
        {
            ColorSlots.Add(color);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 移除颜色槽位
    /// </summary>
    public bool RemoveColorSlot(int index)
    {
        if (index >= 0 && index < ColorSlots.Count && ColorSlots.Count > 1)
        {
            ColorSlots.RemoveAt(index);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取指定颜色的槽位数量
    /// </summary>
    public int GetColorSlotCount(MagicColor color)
    {
        return ColorSlots.Count(slot => slot == color);
    }
    
    /// <summary>
    /// 检查是否有指定的召唤师技能
    /// </summary>
    public bool HasSummonerSkill(SummonerSkillType skillType)
    {
        return SummonerSkills.Contains(skillType);
    }
    
    /// <summary>
    /// 获取召唤师技能的效果数值
    /// </summary>
    public float GetSummonerSkillValue(SummonerSkillType skillType)
    {
        int index = SummonerSkills.IndexOf(skillType);
        if (index >= 0 && index < SkillValues.Count)
        {
            return SkillValues[index];
        }
        return 0f;
    }
    
    /// <summary>
    /// 添加召唤师技能
    /// </summary>
    public void AddSummonerSkill(SummonerSkillType skillType, float value)
    {
        if (!HasSummonerSkill(skillType))
        {
            SummonerSkills.Add(skillType);
            SkillValues.Add(value);
        }
    }
    
    /// <summary>
    /// 获取英雄的颜色倾向描述
    /// </summary>
    public string GetColorAffinityDescription()
    {
        var colorCounts = new Godot.Collections.Dictionary<MagicColor, int>();
        foreach (var color in ColorSlots)
        {
            colorCounts[color] = colorCounts.GetValueOrDefault(color, 0) + 1;
        }
        
        var dominantColors = colorCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(2)
            .Select(kvp => $"{kvp.Key}({kvp.Value})")
            .ToArray();
            
        return string.Join(", ", dominantColors);
    }
    
    /// <summary>
    /// 创建英雄的副本
    /// </summary>
    public SummonerHero CreateCopy()
    {
        var copy = new SummonerHero
        {
            Id = Id,
            HeroName = HeroName,
            Description = Description,
            MaxColorSlots = MaxColorSlots,
            PrimaryColor = PrimaryColor,
            TypingDamageBase = TypingDamageBase,
            TypingDamageDecayRate = TypingDamageDecayRate,
            TypingSpeedBonus = TypingSpeedBonus,
            TypingAccuracyBonus = TypingAccuracyBonus,
            IconPath = IconPath,
            PortraitPath = PortraitPath
        };
        
        // 复制数组和字典
        foreach (var slot in ColorSlots)
            copy.ColorSlots.Add(slot);
        foreach (var skill in SummonerSkills)
            copy.SummonerSkills.Add(skill);
        foreach (var value in SkillValues)
            copy.SkillValues.Add(value);
        foreach (var kvp in RaceBonus)
            copy.RaceBonus[kvp.Key] = kvp.Value;
        foreach (var kvp in BondBonus)
            copy.BondBonus[kvp.Key] = kvp.Value;
            
        return copy;
    }
    
    public override string ToString()
    {
        return $"{HeroName} - {GetColorAffinityDescription()} - Primary: {PrimaryColor}";
    }
}