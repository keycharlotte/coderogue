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
