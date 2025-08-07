using Godot;
using Godot.Collections;
using CodeRogue.Achievements;
using System;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 成就事件数据模型
    /// 继承自Resource，用于封装游戏事件的相关数据
    /// </summary>
    [GlobalClass]
    public partial class AchievementEventData : Resource
    {
        /// <summary>事件类型</summary>
        [Export] public GameEventType EventType { get; set; } = GameEventType.EnemyDefeated;
        
        /// <summary>事件参数字典</summary>
        [Export] public Dictionary<string, Variant> Parameters { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>事件发生时间（ISO 8601格式字符串）</summary>
        [Export] public string Timestamp { get; set; } = string.Empty;
        
        /// <summary>事件来源（触发事件的对象或系统）</summary>
        [Export] public string Source { get; set; } = string.Empty;
        
        /// <summary>事件优先级</summary>
        [Export] public int Priority { get; set; } = 0;
        
        /// <summary>是否为批量事件</summary>
        [Export] public bool IsBatch { get; set; } = false;
        
        /// <summary>批量事件数量</summary>
        [Export] public int BatchCount { get; set; } = 1;
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public AchievementEventData()
        {
            Timestamp = DateTime.UtcNow.ToString("O");
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="parameters">事件参数</param>
        /// <param name="source">事件来源</param>
        public AchievementEventData(GameEventType eventType, Dictionary<string, Variant> parameters = null, string source = "")
        {
            EventType = eventType;
            Parameters = parameters ?? new Dictionary<string, Variant>();
            Source = source;
            Timestamp = DateTime.UtcNow.ToString("O");
        }
        
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="value">参数值</param>
        public void AddParameter(string key, Variant value)
        {
            Parameters[key] = value;
        }
        
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>参数值</returns>
        public T GetParameter<[MustBeVariant] T>(string key, T defaultValue = default)
        {
            if (!Parameters.TryGetValue(key, out var value))
                return defaultValue;
                
            try
            {
                return value.As<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// 获取整数参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>整数值</returns>
        public int GetIntParameter(string key, int defaultValue = 0)
        {
            if (!Parameters.TryGetValue(key, out var value))
                return defaultValue;
                
            return value.AsInt32();
        }
        
        /// <summary>
        /// 获取浮点数参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>浮点数值</returns>
        public float GetFloatParameter(string key, float defaultValue = 0f)
        {
            if (!Parameters.TryGetValue(key, out var value))
                return defaultValue;
                
            return value.AsSingle();
        }
        
        /// <summary>
        /// 获取字符串参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>字符串值</returns>
        public string GetStringParameter(string key, string defaultValue = "")
        {
            if (!Parameters.TryGetValue(key, out var value))
                return defaultValue;
                
            return value.AsString();
        }
        
        /// <summary>
        /// 获取布尔参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>布尔值</returns>
        public bool GetBoolParameter(string key, bool defaultValue = false)
        {
            if (!Parameters.TryGetValue(key, out var value))
                return defaultValue;
                
            return value.AsBool();
        }
        
        /// <summary>
        /// 检查是否包含指定参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <returns>是否包含参数</returns>
        public bool HasParameter(string key)
        {
            return Parameters.ContainsKey(key);
        }
        
        /// <summary>
        /// 移除参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveParameter(string key)
        {
            return Parameters.Remove(key);
        }
        
        /// <summary>
        /// 清空所有参数
        /// </summary>
        public void ClearParameters()
        {
            Parameters.Clear();
        }
        
        /// <summary>
        /// 获取事件发生时间的DateTime对象
        /// </summary>
        /// <returns>事件发生时间</returns>
        public DateTime GetTimestamp()
        {
            if (DateTime.TryParse(Timestamp, out var dateTime))
                return dateTime;
                
            return DateTime.UtcNow;
        }
        
        /// <summary>
        /// 设置事件发生时间
        /// </summary>
        /// <param name="dateTime">时间</param>
        public void SetTimestamp(DateTime dateTime)
        {
            Timestamp = dateTime.ToString("O");
        }
        
        /// <summary>
        /// 合并另一个事件数据（用于批量处理）
        /// </summary>
        /// <param name="other">另一个事件数据</param>
        public void Merge(AchievementEventData other)
        {
            if (other.EventType != EventType)
                return;
                
            // 合并参数
            foreach (var param in other.Parameters)
            {
                if (param.Key == "count" || param.Key == "amount" || param.Key == "value")
                {
                    // 对于数值类型的参数，进行累加
                    var currentValue = GetIntParameter(param.Key, 0);
                    var otherValue = param.Value.AsInt32();
                    Parameters[param.Key] = currentValue + otherValue;
                }
                else
                {
                    // 其他参数直接覆盖
                    Parameters[param.Key] = param.Value;
                }
            }
            
            // 更新批量信息
            IsBatch = true;
            BatchCount += other.BatchCount;
        }
        
        /// <summary>
        /// 创建事件数据的副本
        /// </summary>
        /// <returns>事件数据的副本</returns>
        public AchievementEventData Clone()
        {
            var clone = new AchievementEventData
            {
                EventType = EventType,
                Parameters = new Dictionary<string, Variant>(Parameters),
                Timestamp = Timestamp,
                Source = Source,
                Priority = Priority,
                IsBatch = IsBatch,
                BatchCount = BatchCount
            };
            
            return clone;
        }
        
        /// <summary>
        /// 转换为字典格式（用于序列化）
        /// </summary>
        /// <returns>字典格式的事件数据</returns>
        public Dictionary<string, Variant> ToDictionary()
        {
            var dict = new Dictionary<string, Variant>
            {
                ["eventType"] = (int)EventType,
                ["parameters"] = Parameters,
                ["timestamp"] = Timestamp,
                ["source"] = Source,
                ["priority"] = Priority,
                ["isBatch"] = IsBatch,
                ["batchCount"] = BatchCount
            };
            
            return dict;
        }
        
        /// <summary>
        /// 从字典格式创建事件数据（用于反序列化）
        /// </summary>
        /// <param name="dict">字典格式的事件数据</param>
        /// <returns>事件数据对象</returns>
        public static AchievementEventData FromDictionary(Dictionary<string, Variant> dict)
        {
            var eventData = new AchievementEventData();
            
            if (dict.TryGetValue("eventType", out var eventType))
                eventData.EventType = (GameEventType)eventType.AsInt32();
                
            if (dict.TryGetValue("parameters", out var parameters))
                eventData.Parameters = parameters.AsGodotDictionary<string, Variant>();
                
            if (dict.TryGetValue("timestamp", out var timestamp))
                eventData.Timestamp = timestamp.AsString();
                
            if (dict.TryGetValue("source", out var source))
                eventData.Source = source.AsString();
                
            if (dict.TryGetValue("priority", out var priority))
                eventData.Priority = priority.AsInt32();
                
            if (dict.TryGetValue("isBatch", out var isBatch))
                eventData.IsBatch = isBatch.AsBool();
                
            if (dict.TryGetValue("batchCount", out var batchCount))
                eventData.BatchCount = batchCount.AsInt32();
                
            return eventData;
        }
        
        /// <summary>
        /// 获取事件的显示文本
        /// </summary>
        /// <returns>事件显示文本</returns>
        public string GetDisplayText()
        {
            var eventText = EventType switch
            {
                GameEventType.EnemyDefeated => "击败敌人",
                GameEventType.SkillUsed => "使用技能",
                GameEventType.HeroLevelUp => "英雄升级",
                GameEventType.ItemCollected => "收集物品",
                GameEventType.LevelCompleted => "完成关卡",
                GameEventType.DamageDealt => "造成伤害",
                GameEventType.DamageReceived => "受到伤害",
                GameEventType.ComboAchieved => "达成连击",
                GameEventType.BattleWon => "战斗胜利",
                GameEventType.BattleLost => "战斗失败",
                _ => EventType.ToString()
            };
            
            if (IsBatch && BatchCount > 1)
            {
                eventText += $" (批量: {BatchCount}次)";
            }
            
            return eventText;
        }
        
        /// <summary>
        /// 验证事件数据的有效性
        /// </summary>
        /// <returns>验证结果和错误信息</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrEmpty(Timestamp))
                return (false, "事件时间戳不能为空");
                
            if (!DateTime.TryParse(Timestamp, out _))
                return (false, "事件时间戳格式无效");
                
            if (BatchCount <= 0)
                return (false, "批量事件数量必须大于0");
                
            return (true, string.Empty);
        }
    }
}