using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class HeroInstance : Resource
{
    [Export] public string InstanceId { get; set; }         // 实例ID
    [Export] public int ConfigId { get; set; }              // 配置ID
    [Export] public HeroConfig Config { get; set; }         // 配置引用
    
    // 动态属性
    [Export] public int Level { get; set; } = 1;            // 等级
    [Export] public int Experience { get; set; }            // 经验值
    [Export] public int Star { get; set; } = 1;             // 星级
    [Export] public int Awakening { get; set; }             // 觉醒等级
    
    // 装备
    [Export] public Array<int> EquippedItems { get; set; }  // 装备ID列表
    
    // 状态
    [Export] public bool IsUnlocked { get; set; }           // 是否解锁
    [Export] public bool IsSoulLinked { get; set; }         // 是否被灵魂链接
    
    // 时间戳
    [Export] public double ObtainTime { get; set; }         // 获得时间
    [Export] public double LastUsedTime { get; set; }       // 最后使用时间
    
    // 召唤师动态属性
    [Export] public Array<MagicColor> ColorSlots { get; set; } = new Array<MagicColor>();
    [Export] public Array<SummonerSkillType> SummonerSkills { get; set; } = new Array<SummonerSkillType>();
    [Export] public Array<float> SkillValues { get; set; } = new Array<float>();
    
    // 计算最终属性
    public HeroStats GetFinalStats()
    {
        var stats = new HeroStats();
        
        // 基础属性 + 等级成长
        stats.Health = Config.BaseStats.Health + Config.GrowthStats.Health * (Level - 1);
        stats.Attack = Config.BaseStats.Attack + Config.GrowthStats.Attack * (Level - 1);
        stats.Defense = Config.BaseStats.Defense + Config.GrowthStats.Defense * (Level - 1);
        stats.CritRate = Config.BaseStats.CritRate + Config.GrowthStats.CritRate * (Level - 1);
        stats.CritDamage = Config.BaseStats.CritDamage + Config.GrowthStats.CritDamage * (Level - 1);
        
        // 星级加成
        ApplyStarBonus(stats);
        
        // 觉醒加成
        ApplyAwakeningBonus(stats);
        
        // 装备加成
        ApplyEquipmentBonus(stats);
        
        return stats;
    }
    
    private void ApplyStarBonus(HeroStats stats)
    {
        // 每星级提供5%属性加成
        float starBonus = 1.0f + (Star - 1) * 0.05f;
        stats.Health *= starBonus;
        stats.Attack *= starBonus;
        stats.Defense *= starBonus;
    }
    
    private void ApplyAwakeningBonus(HeroStats stats)
    {
        // 每觉醒等级提供10%属性加成
        float awakeningBonus = 1.0f + Awakening * 0.1f;
        stats.Health *= awakeningBonus;
        stats.Attack *= awakeningBonus;
        stats.Defense *= awakeningBonus;
    }
    
    private void ApplyEquipmentBonus(HeroStats stats)
    {
        // TODO: 实现装备加成逻辑
    }
    
    // 升级
    public bool LevelUp()
    {
        // TODO: 检查升级条件和消耗
        Level++;
        return true;
    }
    
    // 升星
    public bool StarUp()
    {
        // TODO: 检查升星条件和消耗
        Star++;
        return true;
    }
    
    // 觉醒
    public bool Awaken()
    {
        // TODO: 检查觉醒条件和消耗
        Awakening++;
        return true;
    }
    
    #region 召唤师功能
    
    /// <summary>
    /// 初始化默认颜色槽位
    /// </summary>
    public void InitializeDefaultColorSlots()
    {
        if (ColorSlots.Count == 0 && Config != null)
        {
            // 默认给予3个主颜色槽位
            for (int i = 0; i < 3; i++)
            {
                ColorSlots.Add(Config.PrimaryColor);
            }
        }
    }
    
    /// <summary>
    /// 检查是否能召唤指定怪物
    /// </summary>
    public bool CanSummonMonster(MonsterCard monster)
    {
        return CanSatisfyColorRequirements(monster.ColorRequirements);
    }
    
    /// <summary>
    /// 检查颜色需求是否满足（Array版本）
    /// </summary>
    public bool CanSatisfyColorRequirements(Godot.Collections.Array<MagicColor> requirements)
    {
        var availableColors = new System.Collections.Generic.Dictionary<MagicColor, int>();
        
        // 统计可用颜色
        foreach (var color in ColorSlots)
        {
            availableColors[color] = availableColors.GetValueOrDefault(color, 0) + 1;
        }
        
        // 统计需求颜色
        var requiredColors = new System.Collections.Generic.Dictionary<MagicColor, int>();
        foreach (var color in requirements)
        {
            requiredColors[color] = requiredColors.GetValueOrDefault(color, 0) + 1;
        }
        
        // 检查每个颜色需求
        foreach (var requirement in requiredColors)
        {
            var requiredColor = requirement.Key;
            var requiredCount = requirement.Value;
            var availableCount = availableColors.GetValueOrDefault(requiredColor, 0);
            
            if (availableCount < requiredCount)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查颜色需求是否满足（Dictionary版本）
    /// </summary>
    public bool CanSatisfyColorRequirements(Godot.Collections.Dictionary<MagicColor, int> requirements)
    {
        var availableColors = new System.Collections.Generic.Dictionary<MagicColor, int>();
        
        // 统计可用颜色
        foreach (var color in ColorSlots)
        {
            availableColors[color] = availableColors.GetValueOrDefault(color, 0) + 1;
        }
        
        // 检查每个颜色需求
        foreach (var requirement in requirements)
        {
            var requiredColor = requirement.Key;
            var requiredCount = requirement.Value;
            var availableCount = availableColors.GetValueOrDefault(requiredColor, 0);
            
            if (availableCount < requiredCount)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取召唤加成
    /// </summary>
    public float GetSummonBonus(MonsterCard monster)
    {
        if (Config == null) return 0f;
        
        float bonus = 0f;
        
        // 种族加成
        if (Config.RaceBonus.ContainsKey(monster.Race))
            bonus += Config.RaceBonus[monster.Race];
            
        // 颜色亲和加成
        foreach (var color in monster.ColorRequirements)
        {
            if (color == Config.PrimaryColor)
            {
                bonus += 0.2f;
                break;
            }
        }
        
        return bonus;
    }
    
    /// <summary>
    /// 计算打字伤害
    /// </summary>
    public float CalculateTypingDamage(int currentLevel, float typingSpeed, float accuracy)
    {
        if (Config == null) return 0f;
        
        // 基础伤害随关卡衰减
        float levelDecay = Mathf.Pow(1f - Config.TypingDamageDecayRate, currentLevel - 1);
        float baseDamage = Config.TypingDamageBase * levelDecay;
        
        // 应用打字速度和准确度加成
        float speedMultiplier = 1f + (typingSpeed - 1f) * Config.TypingSpeedBonus;
        float accuracyMultiplier = 1f + (accuracy - 1f) * Config.TypingAccuracyBonus;
        
        return baseDamage * speedMultiplier * accuracyMultiplier;
    }
    
    /// <summary>
    /// 添加颜色槽位
    /// </summary>
    public bool AddColorSlot(MagicColor color)
    {
        if (Config == null || ColorSlots.Count >= Config.MaxColorSlots)
            return false;
            
        ColorSlots.Add(color);
        return true;
    }
    
    /// <summary>
    /// 移除颜色槽位
    /// </summary>
    public bool RemoveColorSlot(MagicColor color)
    {
        return ColorSlots.Remove(color);
    }
    
    /// <summary>
    /// 获取指定颜色的槽位数量
    /// </summary>
    public int GetColorSlotCount(MagicColor color)
    {
        int count = 0;
        foreach (var slot in ColorSlots)
        {
            if (slot == color)
                count++;
        }
        return count;
    }
    
    /// <summary>
    /// 检查是否拥有召唤师技能
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
        var colorCounts = new System.Collections.Generic.Dictionary<MagicColor, int>();
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
    
    #endregion
}