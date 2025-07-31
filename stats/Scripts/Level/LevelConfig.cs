using Godot;
using System.Collections.Generic;
using CodeRogue.Level;

namespace CodeRogue.Level
{
    public partial class LevelConfig : RefCounted
{
    public int Level { get; private set; }
    public List<WaveConfig> Waves { get; private set; } = new List<WaveConfig>();
    
    public LevelConfig(int level)
    {
        Level = level;
        LoadConfig();
    }
    
    private void LoadConfig()
    {
        var configPath = "res://ResourcesData/LevelConfig.json";
        if (FileAccess.FileExists(configPath))
        {
            var file = FileAccess.Open(configPath, FileAccess.ModeFlags.Read);
            var jsonString = file.GetAsText();
            file.Close();
            
            var json = Json.ParseString(jsonString);
            var levelData = json.AsGodotDictionary();
            
            if (levelData.ContainsKey(Level.ToString()))
            {
                var levelConfig = levelData[Level.ToString()].AsGodotDictionary();
                var wavesArray = levelConfig["waves"].AsGodotArray();
                
                foreach (var waveData in wavesArray)
                {
                    var wave = new WaveConfig();
                    var waveDict = waveData.AsGodotDictionary();
                    
                    wave.MaxEnemies = waveDict.GetValueOrDefault("maxEnemies", 5).AsInt32();
                    wave.SpawnInterval = waveDict.GetValueOrDefault("spawnInterval", 2.0f).AsSingle();
                    wave.Duration = waveDict.GetValueOrDefault("duration", 30.0f).AsSingle();
                    
                    var enemyTypesDict = waveDict["enemyTypes"].AsGodotDictionary();
                    foreach (var kvp in enemyTypesDict)
                    {
                        wave.EnemyTypes[kvp.Key.AsInt32()] = kvp.Value.AsSingle();
                    }
                    
                    Waves.Add(wave);
                }
            }
        }
        else
        {
            // 生成默认配置
            GenerateDefaultConfig();
        }
    }
    
    private void GenerateDefaultConfig()
    {
        // 根据关卡等级生成默认配置
        int waveCount = Mathf.Min(3 + Level / 2, 8);
        
        for (int i = 1; i <= waveCount; i++)
        {
            var wave = new WaveConfig();
            wave.MaxEnemies = 3 + i + Level;
            wave.SpawnInterval = Mathf.Max(1.0f, 3.0f - Level * 0.1f);
            wave.Duration = 20.0f + i * 5.0f;
            
            // 根据关卡和波次调整敌人类型
            if (Level == 1)
            {
                wave.EnemyTypes[1] = 1.0f; // 只有史莱姆
            }
            else if (Level <= 3)
            {
                wave.EnemyTypes[1] = 0.7f; // 史莱姆
                wave.EnemyTypes[2] = 0.3f; // 哥布林
            }
            else
            {
                wave.EnemyTypes[1] = 0.4f; // 史莱姆
                wave.EnemyTypes[2] = 0.4f; // 哥布林
                wave.EnemyTypes[3] = 0.2f; // 骷髅战士
            }
            
            Waves.Add(wave);
        }
    }
    
    public WaveConfig GetWaveConfig(int waveNumber)
    {
        if (waveNumber <= 0 || waveNumber > Waves.Count)
            return null;
            
        return Waves[waveNumber - 1];
    }
    
    public int GetTotalWaves()
    {
        return Waves.Count;
    }
    }
}