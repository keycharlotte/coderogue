using Godot;

namespace CodeRogue.Data
{
    /// <summary>
    /// 游戏数据类 - 存储游戏进度和设置
    /// </summary>
    [System.Serializable]
    public partial class GameData : Resource
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
        
        public GameData()
        {
            // 默认构造函数
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
        
        public void SaveGame()
        {
            ResourceSaver.Save(this, "user://savegame.tres");
        }
        
        public static GameData LoadGame()
        {
            if (ResourceLoader.Exists("user://savegame.tres"))
            {
                return ResourceLoader.Load<GameData>("user://savegame.tres");
            }
            return new GameData();
        }
    }
}