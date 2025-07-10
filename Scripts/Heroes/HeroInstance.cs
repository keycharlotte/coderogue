using Godot;
using Godot.Collections;

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
}