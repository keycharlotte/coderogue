using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SkillCard : Resource
{
    [Export] public int Id { get; set; }
    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public SkillType Type { get; set; }
    [Export] public SkillRarity Rarity { get; set; }
    [Export] public int ChargeCost { get; set; }
    [Export] public int Level { get; set; } = 1;
    [Export] public Array<SkillTag> Tags { get; set; }
    [Export] public Array<SkillEffect> Effects { get; set; }
    [Export] public string IconPath { get; set; }
    [Export] public Color RarityColor { get; set; }
    
    // 技能升级数据
    [Export] public Array<SkillLevelData> LevelData { get; set; }
    
    public SkillLevelData GetCurrentLevelData()
    {
        if (LevelData == null || LevelData.Count == 0) return null;
        int index = Mathf.Clamp(Level - 1, 0, LevelData.Count - 1);
        return LevelData[index];
    }
    
    public bool CanUpgrade()
    {
        return Level < LevelData?.Count;
    }
    
    public void Upgrade()
    {
        if (CanUpgrade())
        {
            Level++;
            // 应用升级效果
            ApplyUpgradeEffects();
        }
    }
    
    private void ApplyUpgradeEffects()
    {
        var levelData = GetCurrentLevelData();
        if (levelData != null)
        {
            // 更新技能效果数值
            for (int i = 0; i < Effects.Count && i < levelData.EffectValues.Count; i++)
            {
                Effects[i].Value = levelData.EffectValues[i];
            }
            
            // 更新充能消耗
            ChargeCost = levelData.ChargeCost;
        }
    }
}

[GlobalClass]
public partial class SkillLevelData : Resource
{
    [Export] public int Level { get; set; }
    [Export] public int ChargeCost { get; set; }
    [Export] public Array<float> EffectValues { get; set; }
    [Export] public string LevelDescription { get; set; }
}

[GlobalClass]
public partial class SkillEffect : Resource
{
    [Export] public SkillEffectType Type { get; set; }
    [Export] public string TargetProperty { get; set; }
    [Export] public float Value { get; set; }
    [Export] public float Duration { get; set; }
    [Export] public Dictionary Parameters { get; set; }
}

[GlobalClass]
public partial class SkillTag : Resource
{
    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public Color Color { get; set; }
}

public enum SkillType
{
    Attack,
    Defense,
    Utility,
    Movement,
    TypingEnhancement
}

public enum SkillRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum SkillEffectType
{
    Damage,
    Heal,
    Shield,
    Buff,
    Debuff,
    Movement,
    TypingModifier,
    ChargeModifier
}