using Godot;
using System.Linq;

public static class GodotDictionaryExtensions
{
	public static Variant GetValueOrDefault(this Godot.Collections.Dictionary dict, string key, Variant defaultValue)
	{
		return dict.ContainsKey(key) ? dict[key] : defaultValue;
	}
}

[System.Serializable]
public partial class EnemyModel : RefCounted
{
	[Export] public int Id { get; set; }
	[Export] public string Name { get; set; } = "";
	[Export] public int MaxHealth { get; set; } = 100;
	[Export] public int CurrentHealth { get; set; } = 100;
	[Export] public int AttackPower { get; set; } = 10;
	[Export] public float MoveSpeed { get; set; } = 50.0f;
	[Export] public float AttackRange { get; set; } = 32.0f;
	[Export] public float DetectionRange { get; set; } = 64.0f;
	[Export] public string SpritePath { get; set; } = "";
	[Export] public int ExperienceReward { get; set; } = 10;
	[Export] public Vector2 Position { get; set; } = Vector2.Zero;
	[Export] public bool IsAlive { get; set; } = true;
	
	public EnemyModel()
	{
	}
	
	public EnemyModel(int id)
	{
		LoadFromConfig(id);
	}
	
	private void LoadFromConfig(int enemyId)
	{
		// 从配置表加载敌人数据
		var configPath = "res://ResourcesData/EnemyConfig.json";
		if (FileAccess.FileExists(configPath))
		{
			var file = FileAccess.Open(configPath, FileAccess.ModeFlags.Read);
			var jsonString = file.GetAsText();
			file.Close();
			
			var json = Json.ParseString(jsonString);
			if (json.AsGodotDictionary().ContainsKey(enemyId.ToString()))
			{
				var enemyData = json.AsGodotDictionary()[enemyId.ToString()].AsGodotDictionary();
				
				Id = enemyId;
				// 现有的代码可以保持不变
				Name = enemyData.GetValueOrDefault("name", "").AsString();
				MaxHealth = enemyData.GetValueOrDefault("maxHealth", 100).AsInt32();
				CurrentHealth = MaxHealth;
				AttackPower = enemyData.GetValueOrDefault("attackPower", 10).AsInt32();
				MoveSpeed = enemyData.GetValueOrDefault("moveSpeed", 50.0f).AsSingle();
				AttackRange = enemyData.GetValueOrDefault("attackRange", 32.0f).AsSingle();
				DetectionRange = enemyData.GetValueOrDefault("detectionRange", 64.0f).AsSingle();
				SpritePath = enemyData.GetValueOrDefault("spritePath", "").AsString();
				ExperienceReward = enemyData.GetValueOrDefault("experienceReward", 10).AsInt32();
			}
		}
		else
		{
			// 默认数据
			SetDefaultData(enemyId);
		}
	}
	
	private void SetDefaultData(int enemyId)
	{
		Id = enemyId;
		Name = $"Enemy_{enemyId}";
		MaxHealth = 100;
		CurrentHealth = MaxHealth;
		AttackPower = 10;
		MoveSpeed = 50.0f;
		AttackRange = 32.0f;
		DetectionRange = 64.0f;
		// SpritePath = "res://AssetsTextures/enemy_default.png";
		ExperienceReward = 10;
	}
	
	public void TakeDamage(int damage)
	{
		CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
		if (CurrentHealth <= 0)
		{
			IsAlive = false;
		}
	}
	
	public void Heal(int amount)
	{
		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
	}
	
	public float GetHealthPercentage()
	{
		return MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0.0f;
	}
}
