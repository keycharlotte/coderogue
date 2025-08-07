using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Rebirth.Data;
using Godot.Collections;
using Dictionary = Godot.Collections.Dictionary;
using GodotDictionary = Godot.Collections.Dictionary;

namespace CodeRogue.Rebirth
{
    /// <summary>
    /// 新颖度数据库
    /// 继承自Node，配置为AutoLoad单例
    /// 负责管理新颖度记录数据
    /// </summary>
    public partial class NoveltyDatabase : Node
    {
        private Godot.Collections.Dictionary<string, NoveltyRecord> _noveltyRecords = new();
        private const string SAVE_PATH = "user://novelty_records.save";
        
        public override void _Ready()
        {
            LoadNoveltyRecords();
            GD.Print("[NoveltyDatabase] 新颖度数据库初始化完成");
        }
        
        /// <summary>
        /// 获取新颖度记录
        /// </summary>
        public NoveltyRecord GetNoveltyRecord(string combinationHash)
        {
            return _noveltyRecords.GetValueOrDefault(combinationHash);
        }
        
        /// <summary>
        /// 更新新颖度记录
        /// </summary>
        public void UpdateNoveltyRecord(string combinationHash, float score, float multiplier)
        {
            if (_noveltyRecords.ContainsKey(combinationHash))
            {
                _noveltyRecords[combinationHash].UpdateUsage(score, multiplier);
            }
            else
            {
                var newRecord = new NoveltyRecord
                {
                    CombinationHash = combinationHash,
                    NoveltyScore = score,
                    BestMultiplier = multiplier,
                    UsageCount = 1,
                    FirstUsed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    LastUsed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                _noveltyRecords[combinationHash] = newRecord;
            }
            
            SaveNoveltyRecords();
        }
        
        /// <summary>
        /// 获取所有新颖度记录
        /// </summary>
        public List<NoveltyRecord> GetAllRecords()
        {
            return _noveltyRecords.Values.ToList();
        }
        
        /// <summary>
        /// 获取新颖度历史
        /// </summary>
        public List<NoveltyRecord> GetNoveltyHistory()
        {
            return _noveltyRecords.Values.OrderByDescending(r => r.LastUsed).ToList();
        }
        
        /// <summary>
        /// 清理过期记录
        /// </summary>
        public void CleanupOldRecords(int maxDaysOld = 365)
        {
            var cutoffDate = DateTime.Now.AddDays(-maxDaysOld);
            var toRemove = new List<string>();
            
            foreach (var kvp in _noveltyRecords)
            {
                if (DateTime.TryParse(kvp.Value.LastUsed, out var lastUsed) && lastUsed < cutoffDate)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in toRemove)
            {
                _noveltyRecords.Remove(key);
            }
            
            if (toRemove.Count > 0)
            {
                SaveNoveltyRecords();
                GD.Print($"[NoveltyDatabase] 清理了 {toRemove.Count} 条过期记录");
            }
        }
        
        /// <summary>
        /// 加载新颖度记录
        /// </summary>
        private void LoadNoveltyRecords()
        {
            if (!FileAccess.FileExists(SAVE_PATH))
            {
                GD.Print("[NoveltyDatabase] 新颖度记录文件不存在，使用默认数据");
                return;
            }
            
            try
            {
                using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr("[NoveltyDatabase] 无法打开新颖度记录文件");
                    return;
                }
                
                string jsonString = file.GetAsText();
                var json = new Json();
                var parseResult = json.Parse(jsonString);
                
                if (parseResult != Error.Ok)
                {
                    GD.PrintErr($"[NoveltyDatabase] JSON解析失败: {parseResult}");
                    return;
                }
                
                var data = json.Data.AsGodotDictionary();
                if (data.ContainsKey("records"))
                {
                    var recordsArray = data["records"].AsGodotArray();
                    foreach (var recordData in recordsArray)
                    {
                        var recordDict = recordData.AsGodotDictionary();
                        var record = ParseNoveltyRecord(recordDict);
                        if (record != null)
                        {
                            _noveltyRecords[record.CombinationHash] = record;
                        }
                    }
                }
                
                GD.Print($"[NoveltyDatabase] 加载了 {_noveltyRecords.Count} 条新颖度记录");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[NoveltyDatabase] 加载新颖度记录失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存新颖度记录
        /// </summary>
        private void SaveNoveltyRecords()
        {
            try
            {
                var data = new Godot.Collections.Dictionary();
                var recordsArray = new Godot.Collections.Array();
                
                foreach (var record in _noveltyRecords.Values)
                {
                    var recordDict = new Godot.Collections.Dictionary
                    {
                        ["combination_hash"] = record.CombinationHash,
                        ["usage_count"] = record.UsageCount,
                        ["first_used"] = record.FirstUsed,
                        ["last_used"] = record.LastUsed,
                        ["novelty_score"] = record.NoveltyScore,
                        ["best_multiplier"] = record.BestMultiplier,
                        ["total_currency_earned"] = record.TotalCurrencyEarned
                    };
                    recordsArray.Add(recordDict);
                }
                
                data["records"] = recordsArray;
                
                using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr("[NoveltyDatabase] 无法创建新颖度记录文件");
                    return;
                }
                
                file.StoreString(Json.Stringify(data));
                GD.Print($"[NoveltyDatabase] 保存了 {_noveltyRecords.Count} 条新颖度记录");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[NoveltyDatabase] 保存新颖度记录失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 解析新颖度记录
        /// </summary>
        private NoveltyRecord ParseNoveltyRecord(Godot.Collections.Dictionary data)
        {
            try
            {
                var record = new NoveltyRecord();
                
                if (data.ContainsKey("combination_hash"))
                    record.CombinationHash = data["combination_hash"].AsString();
                if (data.ContainsKey("usage_count"))
                    record.UsageCount = data["usage_count"].AsInt32();
                if (data.ContainsKey("first_used"))
                    record.FirstUsed = data["first_used"].AsString();
                if (data.ContainsKey("last_used"))
                    record.LastUsed = data["last_used"].AsString();
                if (data.ContainsKey("novelty_score"))
                    record.NoveltyScore = data["novelty_score"].AsSingle();
                if (data.ContainsKey("best_multiplier"))
                    record.BestMultiplier = data["best_multiplier"].AsSingle();
                if (data.ContainsKey("total_currency_earned"))
                    record.TotalCurrencyEarned = data["total_currency_earned"].AsInt32();
                
                return record;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[NoveltyDatabase] 解析新颖度记录失败: {ex.Message}");
                return null;
            }
        }
    }
}