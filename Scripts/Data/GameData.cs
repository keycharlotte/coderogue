using System.Collections.Generic;
using Godot;

namespace CodeRogue.Data
{
	/// <summary>
	/// 游戏数据类 - 存储游戏进度和设置
	/// </summary>
	public partial class GameData : Node
	{
		[Export]
		public int PlayerLevel { get; set; } = 1;
		
		[Export]
		public int PlayerExperience { get; set; } = 0;
		
		[Export]
		public int PlayerHealth { get; set; } = 100;
		
		[Export]
		public int PlayerMaxHealth { get; set; } = 100;
		
		[Export]
		public int CurrentLevel { get; set; } = 1;
		
		[Export]
		public float MasterVolume { get; set; } = 1.0f;
		
		[Export]
		public float SfxVolume { get; set; } = 1.0f;
		
		[Export]
		public float MusicVolume { get; set; } = 1.0f;
		
		[Export]
		public bool FullScreen { get; set; } = false;
		
		public override void _Ready()
		{
			// 自动加载游戏数据
			LoadGameData();
		}
		
		public void ResetToDefaults()
		{
			PlayerLevel = 1;
			PlayerExperience = 0;
			PlayerHealth = 100;
			PlayerMaxHealth = 100;
			CurrentLevel = 1;
			MasterVolume = 1.0f;
			SfxVolume = 1.0f;
			MusicVolume = 1.0f;
			FullScreen = false;
		}
		
		public void SaveGameData()
		{
			var saveData = new Godot.Collections.Dictionary<string, Variant>()
			{
				["PlayerLevel"] = PlayerLevel,
				["PlayerExperience"] = PlayerExperience,
				["PlayerHealth"] = PlayerHealth,
				["PlayerMaxHealth"] = PlayerMaxHealth,
				["CurrentLevel"] = CurrentLevel,
				["MasterVolume"] = MasterVolume,
				["SfxVolume"] = SfxVolume,
				["MusicVolume"] = MusicVolume,
				["FullScreen"] = FullScreen
			};
			
			var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
			if (saveFile != null)
			{
				saveFile.StoreString(Json.Stringify(saveData));
				saveFile.Close();
			}
		}
		
		public void LoadGameData()
		{
			if (!FileAccess.FileExists("user://savegame.save"))
			{
				return; // 使用默认值
			}
			
			var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);
			if (saveFile != null)
			{
				var jsonString = saveFile.GetAsText();
				saveFile.Close();
				
				var json = new Json();
				var parseResult = json.Parse(jsonString);
				if (parseResult == Error.Ok)
				{
					var saveData = json.Data.AsGodotDictionary<string, Variant>();
					
					PlayerLevel = saveData.GetValueOrDefault("PlayerLevel", 1).AsInt32();
					PlayerExperience = saveData.GetValueOrDefault("PlayerExperience", 0).AsInt32();
					PlayerHealth = saveData.GetValueOrDefault("PlayerHealth", 100).AsInt32();
					PlayerMaxHealth = saveData.GetValueOrDefault("PlayerMaxHealth", 100).AsInt32();
					CurrentLevel = saveData.GetValueOrDefault("CurrentLevel", 1).AsInt32();
					MasterVolume = saveData.GetValueOrDefault("MasterVolume", 1.0f).AsSingle();
					SfxVolume = saveData.GetValueOrDefault("SfxVolume", 1.0f).AsSingle();
					MusicVolume = saveData.GetValueOrDefault("MusicVolume", 1.0f).AsSingle();
					FullScreen = saveData.GetValueOrDefault("FullScreen", false).AsBool();
				}
			}
		}
	}
}
