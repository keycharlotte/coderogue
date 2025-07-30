using System;
using Godot;
using Godot.Collections;

namespace CodeRogue.Data
{
    /// <summary>
    /// 扩展游戏数据结构 - 包含完整的游戏进度和收集数据
    /// </summary>
    [System.Serializable]
    public partial class ExtendedGameData : Resource
    {
        // 基础玩家数据
        [Export] public int PlayerLevel { get; set; } = 1;
        [Export] public int PlayerExperience { get; set; } = 0;
        [Export] public int PlayerHealth { get; set; } = 100;
        [Export] public int PlayerMaxHealth { get; set; } = 100;
        
        // 游戏进度
        [Export] public int CurrentLevel { get; set; } = 1;
        [Export] public Array<int> UnlockedLevels { get; set; } = new Array<int>();
        [Export] public Godot.Collections.Dictionary<int, int> LevelBestScores { get; set; } = new Godot.Collections.Dictionary<int, int>();
        [Export] public Godot.Collections.Dictionary<int, float> LevelBestTimes { get; set; } = new Godot.Collections.Dictionary<int, float>();
        [Export] public Godot.Collections.Dictionary<int, int> LevelCompletionCount { get; set; } = new Godot.Collections.Dictionary<int, int>();
        
        // 收集数据
        [Export] public Array<string> UnlockedCards { get; set; } = new Array<string>();
        [Export] public Array<string> UnlockedHeroes { get; set; } = new Array<string>();
        [Export] public Array<string> UnlockedRelics { get; set; } = new Array<string>();
        [Export] public Godot.Collections.Dictionary<string, int> CardUsageCount { get; set; } = new Godot.Collections.Dictionary<string, int>();
        [Export] public Godot.Collections.Dictionary<string, int> HeroUsageCount { get; set; } = new Godot.Collections.Dictionary<string, int>();
        [Export] public Godot.Collections.Dictionary<string, int> RelicFoundCount { get; set; } = new Godot.Collections.Dictionary<string, int>();
        
        // 成就和统计
        [Export] public Array<string> UnlockedAchievements { get; set; } = new Array<string>();
        [Export] public Godot.Collections.Dictionary<string, int> GameStatistics { get; set; } = new Godot.Collections.Dictionary<string, int>();
        [Export] public Godot.Collections.Dictionary<string, float> GameStatisticsFloat { get; set; } = new Godot.Collections.Dictionary<string, float>();
        
        // 战斗统计
        [Export] public int TotalBattlesWon { get; set; } = 0;
        [Export] public int TotalBattlesLost { get; set; } = 0;
        [Export] public int TotalEnemiesDefeated { get; set; } = 0;
        [Export] public float TotalDamageDealt { get; set; } = 0f;
        [Export] public float TotalDamageTaken { get; set; } = 0f;
        [Export] public int TotalSkillsUsed { get; set; } = 0;
        
        // 游戏设置
        [Export] public float MasterVolume { get; set; } = 1.0f;
        [Export] public float SfxVolume { get; set; } = 1.0f;
        [Export] public float MusicVolume { get; set; } = 1.0f;
        [Export] public bool FullScreen { get; set; } = false;
        [Export] public int ResolutionWidth { get; set; } = 1920;
        [Export] public int ResolutionHeight { get; set; } = 1080;
        [Export] public bool VSync { get; set; } = true;
        [Export] public int GraphicsQuality { get; set; } = 2; // 0=Low, 1=Medium, 2=High
        
        // 控制设置
        [Export] public Godot.Collections.Dictionary<string, string> KeyBindings { get; set; } = new Godot.Collections.Dictionary<string, string>();
        [Export] public float MouseSensitivity { get; set; } = 1.0f;
        [Export] public bool InvertMouse { get; set; } = false;
        
        // 游戏状态快照
        [Export] public Godot.Collections.Dictionary<string, Variant> CurrentGameState { get; set; } = new Godot.Collections.Dictionary<string, Variant>();
        [Export] public bool HasActiveGame { get; set; } = false;
        [Export] public string CurrentScenePath { get; set; } = "";
        [Export] public Vector2 PlayerPosition { get; set; } = Vector2.Zero;
        
        // 元数据
        [Export] public string SaveVersion { get; set; } = "1.0.0";
        [Export] public long LastSaveTime { get; set; } = 0;
        [Export] public float TotalPlayTime { get; set; } = 0f;
        [Export] public long FirstPlayTime { get; set; } = 0;
        [Export] public int SaveCount { get; set; } = 0;
        [Export] public string GameVersion { get; set; } = "";
        
        public ExtendedGameData()
        {
            InitializeDefaults();
        }
        
        /// <summary>
        /// 初始化默认值
        /// </summary>
        private void InitializeDefaults()
        {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastSaveTime = currentTime;
            if (FirstPlayTime == 0)
                FirstPlayTime = currentTime;
                
            // 初始化默认解锁内容
            if (UnlockedLevels.Count == 0)
                UnlockedLevels.Add(1);
                
            // 初始化统计数据
            if (GameStatistics.Count == 0)
            {
                GameStatistics["gamesStarted"] = 0;
                GameStatistics["gamesCompleted"] = 0;
                GameStatistics["deathCount"] = 0;
                GameStatistics["itemsCollected"] = 0;
                GameStatistics["secretsFound"] = 0;
            }
            
            if (GameStatisticsFloat.Count == 0)
            {
                GameStatisticsFloat["averageGameTime"] = 0f;
                GameStatisticsFloat["bestSpeedrunTime"] = 0f;
                GameStatisticsFloat["totalIdleTime"] = 0f;
            }
        }
        
        /// <summary>
        /// 重置到默认值
        /// </summary>
        public void ResetToDefaults()
        {
            PlayerLevel = 1;
            PlayerExperience = 0;
            PlayerHealth = 100;
            PlayerMaxHealth = 100;
            CurrentLevel = 1;
            
            UnlockedLevels.Clear();
            LevelBestScores.Clear();
            LevelBestTimes.Clear();
            LevelCompletionCount.Clear();
            
            UnlockedCards.Clear();
            UnlockedHeroes.Clear();
            UnlockedRelics.Clear();
            CardUsageCount.Clear();
            HeroUsageCount.Clear();
            RelicFoundCount.Clear();
            
            UnlockedAchievements.Clear();
            GameStatistics.Clear();
            GameStatisticsFloat.Clear();
            
            TotalBattlesWon = 0;
            TotalBattlesLost = 0;
            TotalEnemiesDefeated = 0;
            TotalDamageDealt = 0f;
            TotalDamageTaken = 0f;
            TotalSkillsUsed = 0;
            
            MasterVolume = 1.0f;
            SfxVolume = 1.0f;
            MusicVolume = 1.0f;
            FullScreen = false;
            ResolutionWidth = 1920;
            ResolutionHeight = 1080;
            VSync = true;
            GraphicsQuality = 2;
            
            KeyBindings.Clear();
            MouseSensitivity = 1.0f;
            InvertMouse = false;
            
            CurrentGameState.Clear();
            HasActiveGame = false;
            CurrentScenePath = "";
            PlayerPosition = Vector2.Zero;
            
            TotalPlayTime = 0f;
            SaveCount = 0;
            
            InitializeDefaults();
        }
        
        /// <summary>
        /// 更新游戏统计
        /// </summary>
        public void UpdateStatistic(string key, int value)
        {
            if (GameStatistics.ContainsKey(key))
            {
                GameStatistics[key] = GameStatistics[key] + value;
            }
            else
            {
                GameStatistics[key] = value;
            }
        }
        
        /// <summary>
        /// 更新浮点统计
        /// </summary>
        public void UpdateStatisticFloat(string key, float value)
        {
            if (GameStatisticsFloat.ContainsKey(key))
            {
                GameStatisticsFloat[key] = GameStatisticsFloat[key] + value;
            }
            else
            {
                GameStatisticsFloat[key] = value;
            }
        }
        
        /// <summary>
        /// 解锁关卡
        /// </summary>
        public void UnlockLevel(int levelId)
        {
            if (!UnlockedLevels.Contains(levelId))
            {
                UnlockedLevels.Add(levelId);
            }
        }
        
        /// <summary>
        /// 更新关卡最佳成绩
        /// </summary>
        public void UpdateLevelBestScore(int levelId, int score)
        {
            if (!LevelBestScores.ContainsKey(levelId) || LevelBestScores[levelId] < score)
            {
                LevelBestScores[levelId] = score;
            }
        }
        
        /// <summary>
        /// 解锁成就
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!UnlockedAchievements.Contains(achievementId))
            {
                UnlockedAchievements.Add(achievementId);
                UpdateStatistic("achievementsUnlocked", 1);
            }
        }
    }
}