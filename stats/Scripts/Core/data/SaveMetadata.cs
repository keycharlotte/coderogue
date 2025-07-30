using System;
using Godot;
using Godot.Collections;

namespace CodeRogue.Data
{
    /// <summary>
    /// 存档元数据 - 存储存档的基本信息
    /// </summary>
    [System.Serializable]
    public partial class SaveMetadata : Resource
    {
        [Export] public string SaveVersion { get; set; } = "1.0.0";
        [Export] public long SaveTimestamp { get; set; }
        [Export] public string SaveChecksum { get; set; } = "";
        [Export] public int PlayerLevel { get; set; }
        [Export] public int CurrentLevel { get; set; }
        [Export] public float TotalPlayTime { get; set; }
        [Export] public string SaveName { get; set; } = "";
        [Export] public string SaveDescription { get; set; } = "";
        [Export] public bool IsAutoSave { get; set; } = false;
        
        public SaveMetadata()
        {
            SaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
        /// <summary>
        /// 获取格式化的保存时间
        /// </summary>
        public string GetFormattedSaveTime()
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(SaveTimestamp);
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        /// <summary>
        /// 获取格式化的游戏时长
        /// </summary>
        public string GetFormattedPlayTime()
        {
            var timeSpan = TimeSpan.FromSeconds(TotalPlayTime);
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}