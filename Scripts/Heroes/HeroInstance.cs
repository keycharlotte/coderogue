using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Data;

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
        if (EquippedItems == null || EquippedItems.Count == 0)
            return;
            
        // 获取装备数据库
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        if (sceneTree?.Root == null)
            return;
            
        // TODO: 当实现装备系统后，取消注释以下代码
        /*
        var equipmentDatabase = sceneTree.Root.GetNode<EquipmentDatabase>("/root/EquipmentDatabase");
        if (equipmentDatabase == null)
            return;
            
        foreach (int equipmentId in EquippedItems)
        {
            var equipment = equipmentDatabase.GetEquipmentById(equipmentId);
            if (equipment != null)
            {
                // 应用装备属性加成
                stats.Health += equipment.HealthBonus;
                stats.Attack += equipment.AttackBonus;
                stats.Defense += equipment.DefenseBonus;
                stats.CritRate += equipment.CritRateBonus;
                stats.CritDamage += equipment.CritDamageBonus;
                
                // 应用装备套装效果
                ApplyEquipmentSetBonus(equipment, stats);
            }
        }
        */
        
        // 临时实现：为每件装备提供固定加成
        foreach (int equipmentId in EquippedItems)
        {
            // 假设每件装备提供基础属性的10%加成
            stats.Health *= 1.1f;
            stats.Attack *= 1.1f;
            stats.Defense *= 1.1f;
            stats.CritRate += 0.02f; // 增加2%暴击率
        }
    }
    
    // 应用装备套装效果（预留方法）
    private void ApplyEquipmentSetBonus(object equipment, HeroStats stats)
    {
        // TODO: 实现套装效果逻辑
        // 检查装备套装，应用相应的套装加成
    }
    
    // 装备道具
    public bool EquipItem(int itemId)
    {
        if (EquippedItems == null)
            EquippedItems = new Array<int>();
            
        // 检查是否已装备
        if (EquippedItems.Contains(itemId))
        {
            GD.Print("该装备已经装备");
            return false;
        }
        
        // TODO: 检查装备槽位限制和装备类型兼容性
        const int maxEquipmentSlots = 6; // 假设最多6个装备槽位
        if (EquippedItems.Count >= maxEquipmentSlots)
        {
            GD.Print("装备槽位已满");
            return false;
        }
        
        EquippedItems.Add(itemId);
        GD.Print($"装备道具 {itemId}");
        return true;
    }
    
    // 卸下装备
    public bool UnequipItem(int itemId)
    {
        if (EquippedItems == null || !EquippedItems.Contains(itemId))
        {
            GD.Print("未装备该道具");
            return false;
        }
        
        EquippedItems.Remove(itemId);
        GD.Print($"卸下装备 {itemId}");
        return true;
    }
    
    // 获取装备列表
    public Array<int> GetEquippedItems()
    {
        return EquippedItems ?? new Array<int>();
    }
    
    // 检查是否装备了指定道具
    public bool IsItemEquipped(int itemId)
    {
        return EquippedItems?.Contains(itemId) ?? false;
    }
    
    // 升级
    public bool LevelUp()
    {
        const int maxLevel = 100;
        if (Level >= maxLevel)
        {
            GD.Print("英雄已达到最大等级");
            return false;
        }
        
        // 计算升级所需经验
        int requiredExp = GetRequiredExperienceForLevel(Level + 1);
        if (Experience < requiredExp)
        {
            GD.Print($"经验不足，需要 {requiredExp}，当前 {Experience}");
            return false;
        }
        
        // 消耗经验并升级
        Experience -= requiredExp;
        Level++;
        GD.Print($"英雄 {Config?.Name} 升级到 {Level} 级");
        return true;
    }
    
    // 升星
    public bool StarUp()
    {
        const int maxStar = 6;
        if (Star >= maxStar)
        {
            GD.Print("英雄已达到最大星级");
            return false;
        }
        
        // 升星需要消耗相同英雄碎片
        int requiredFragments = GetRequiredFragmentsForStar(Star + 1);
        
        // 获取游戏数据检查碎片数量
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        if (sceneTree?.Root != null)
        {
            var gameData = sceneTree.Root.GetNode<GameData>("/root/GameData");
            if (gameData != null)
            {
                // TODO: 当GameData实现英雄碎片系统后，在这里检查碎片数量
                // if (gameData.GetHeroFragments(ConfigId) < requiredFragments)
                // {
                //     GD.Print($"英雄碎片不足，需要 {requiredFragments}");
                //     return false;
                // }
                // gameData.ConsumeHeroFragments(ConfigId, requiredFragments);
            }
        }
        
        Star++;
        GD.Print($"英雄 {Config?.Name} 升星到 {Star} 星");
        return true;
    }
    
    // 觉醒
    public bool Awaken()
    {
        const int maxAwakening = 5;
        if (Awakening >= maxAwakening)
        {
            GD.Print("英雄已达到最大觉醒等级");
            return false;
        }
        
        // 觉醒需要特定材料和金币
        var requiredMaterials = GetRequiredMaterialsForAwakening(Awakening + 1);
        int requiredGold = GetRequiredGoldForAwakening(Awakening + 1);
        
        // 获取游戏数据检查材料和金币
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        if (sceneTree?.Root != null)
        {
            var gameData = sceneTree.Root.GetNode<GameData>("/root/GameData");
            if (gameData != null)
            {
                // TODO: 当GameData实现材料和金币系统后，在这里检查和消耗资源
                // if (gameData.Gold < requiredGold)
                // {
                //     GD.Print($"金币不足，需要 {requiredGold}");
                //     return false;
                // }
                // foreach (var material in requiredMaterials)
                // {
                //     if (gameData.GetMaterialCount(material.Key) < material.Value)
                //     {
                //         GD.Print($"材料 {material.Key} 不足，需要 {material.Value}");
                //         return false;
                //     }
                // }
                // gameData.ConsumeGold(requiredGold);
                // foreach (var material in requiredMaterials)
                // {
                //     gameData.ConsumeMaterial(material.Key, material.Value);
                // }
            }
        }
        
        Awakening++;
        GD.Print($"英雄 {Config?.Name} 觉醒到 {Awakening} 级");
        return true;
    }
    
    // 计算升级所需经验
    private int GetRequiredExperienceForLevel(int targetLevel)
    {
        // 经验需求公式：基础经验 * 等级系数
        int baseExp = 100;
        float levelMultiplier = 1.2f;
        return Mathf.RoundToInt(baseExp * Mathf.Pow(levelMultiplier, targetLevel - 1));
    }
    
    // 计算升星所需碎片
    private int GetRequiredFragmentsForStar(int targetStar)
    {
        // 升星碎片需求：2星需要10个，3星需要20个，以此类推
        return (targetStar - 1) * 10;
    }
    
    // 计算觉醒所需材料
    private Godot.Collections.Dictionary<string, int> GetRequiredMaterialsForAwakening(int targetAwakening)
    {
        var materials = new Godot.Collections.Dictionary<string, int>();
        
        switch (targetAwakening)
        {
            case 1:
                materials["awakening_stone_1"] = 5;
                materials["magic_essence"] = 10;
                break;
            case 2:
                materials["awakening_stone_2"] = 3;
                materials["magic_essence"] = 20;
                materials["hero_soul"] = 1;
                break;
            case 3:
                materials["awakening_stone_3"] = 2;
                materials["magic_essence"] = 30;
                materials["hero_soul"] = 2;
                break;
            case 4:
                materials["awakening_stone_4"] = 1;
                materials["magic_essence"] = 50;
                materials["hero_soul"] = 3;
                materials["divine_crystal"] = 1;
                break;
            case 5:
                materials["awakening_stone_5"] = 1;
                materials["magic_essence"] = 100;
                materials["hero_soul"] = 5;
                materials["divine_crystal"] = 3;
                break;
        }
        
        return materials;
    }
    
    // 计算觉醒所需金币
    private int GetRequiredGoldForAwakening(int targetAwakening)
    {
        return targetAwakening * 10000; // 每级觉醒需要1万金币递增
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