using Godot;
using Godot.Collections;
using CodeRogue.Achievements;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 成就条件数据模型
    /// 继承自Resource，用于定义成就的触发条件
    /// </summary>
    [GlobalClass]
    public partial class AchievementCondition : Resource
    {
        /// <summary>条件ID</summary>
        [Export] public int Id { get; set; } = 0;
        
        /// <summary>游戏事件类型</summary>
        [Export] public GameEventType EventType { get; set; } = GameEventType.EnemyDefeated;
        
        /// <summary>参数名称</summary>
        [Export] public string ParameterName { get; set; } = string.Empty;
        
        /// <summary>参数值</summary>
        [Export] public Variant ParameterValue { get; set; } = new Variant();
        
        /// <summary>比较类型</summary>
        [Export] public ComparisonType Comparison { get; set; } = ComparisonType.GreaterOrEqual;
        
        /// <summary>条件描述</summary>
        [Export] public string Description { get; set; } = string.Empty;
        
        /// <summary>是否为可选条件</summary>
        [Export] public bool IsOptional { get; set; } = false;
        
        /// <summary>条件权重（用于复合条件）</summary>
        [Export] public float Weight { get; set; } = 1.0f;
        
        /// <summary>过滤器字典（用于额外的条件筛选）</summary>
        [Export] public Dictionary<string, Variant> Filters { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>
        /// 检查事件数据是否满足条件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <returns>是否满足条件</returns>
        public bool CheckCondition(AchievementEventData eventData)
        {
            // 检查事件类型是否匹配
            if (eventData.EventType != EventType)
                return false;
                
            // 检查过滤器条件
            if (!CheckFilters(eventData))
                return false;
                
            // 如果没有指定参数名称，则只检查事件类型
            if (string.IsNullOrEmpty(ParameterName))
                return true;
                
            // 获取事件数据中的参数值
            if (!eventData.Parameters.TryGetValue(ParameterName, out var eventValue))
                return false;
                
            // 执行比较操作
            return CompareValues(eventValue, ParameterValue, Comparison);
        }
        
        /// <summary>
        /// 检查过滤器条件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <returns>是否通过过滤器</returns>
        private bool CheckFilters(AchievementEventData eventData)
        {
            foreach (var filter in Filters)
            {
                if (!eventData.Parameters.TryGetValue(filter.Key, out var eventValue))
                    return false;
                    
                if (!CompareValues(eventValue, filter.Value, ComparisonType.Equal))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 比较两个值
        /// </summary>
        /// <param name="value1">值1</param>
        /// <param name="value2">值2</param>
        /// <param name="comparison">比较类型</param>
        /// <returns>比较结果</returns>
        private bool CompareValues(Variant value1, Variant value2, ComparisonType comparison)
        {
            try
            {
                return comparison switch
                {
                    ComparisonType.Equal => value1.Equals(value2),
                    ComparisonType.NotEqual => !value1.Equals(value2),
                    ComparisonType.Greater => CompareNumeric(value1, value2) > 0,
                    ComparisonType.GreaterOrEqual => CompareNumeric(value1, value2) >= 0,
                    ComparisonType.Less => CompareNumeric(value1, value2) < 0,
                    ComparisonType.LessOrEqual => CompareNumeric(value1, value2) <= 0,
                    ComparisonType.Contains => value1.AsString().Contains(value2.AsString()),
                    ComparisonType.NotContains => !value1.AsString().Contains(value2.AsString()),
                    _ => false
                };
            }
            catch
            {
                // 如果比较失败，返回false
                return false;
            }
        }
        
        /// <summary>
        /// 数值比较
        /// </summary>
        /// <param name="value1">值1</param>
        /// <param name="value2">值2</param>
        /// <returns>比较结果（-1: 小于, 0: 等于, 1: 大于）</returns>
        private int CompareNumeric(Variant value1, Variant value2)
        {
            // 尝试转换为数值进行比较
            if (value1.VariantType == Variant.Type.Int && value2.VariantType == Variant.Type.Int)
            {
                return value1.AsInt32().CompareTo(value2.AsInt32());
            }
            
            if (value1.VariantType == Variant.Type.Float || value2.VariantType == Variant.Type.Float)
            {
                return value1.AsSingle().CompareTo(value2.AsSingle());
            }
            
            // 尝试解析为数值
            if (float.TryParse(value1.AsString(), out var float1) && 
                float.TryParse(value2.AsString(), out var float2))
            {
                return float1.CompareTo(float2);
            }
            
            // 如果无法转换为数值，则按字符串比较
            return string.Compare(value1.AsString(), value2.AsString(), System.StringComparison.Ordinal);
        }
        
        /// <summary>
        /// 获取条件的显示文本
        /// </summary>
        /// <returns>条件显示文本</returns>
        public string GetDisplayText()
        {
            if (!string.IsNullOrEmpty(Description))
                return Description;
                
            var eventText = GetEventTypeText();
            
            if (string.IsNullOrEmpty(ParameterName))
                return eventText;
                
            var comparisonText = GetComparisonText();
            var valueText = ParameterValue.AsString();
            
            return $"{eventText} {ParameterName} {comparisonText} {valueText}";
        }
        
        /// <summary>
        /// 获取事件类型的显示文本
        /// </summary>
        /// <returns>事件类型文本</returns>
        private string GetEventTypeText()
        {
            return EventType switch
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
                GameEventType.SkillLearned => "学习技能",
                GameEventType.HeroUnlocked => "解锁英雄",
                GameEventType.EquipmentObtained => "获得装备",
                GameEventType.RelicObtained => "获得遗物",
                GameEventType.GoldEarned => "获得金币",
                GameEventType.ExperienceGained => "获得经验",
                _ => EventType.ToString()
            };
        }
        
        /// <summary>
        /// 获取比较类型的显示文本
        /// </summary>
        /// <returns>比较类型文本</returns>
        private string GetComparisonText()
        {
            return Comparison switch
            {
                ComparisonType.Equal => "等于",
                ComparisonType.NotEqual => "不等于",
                ComparisonType.Greater => "大于",
                ComparisonType.GreaterOrEqual => "大于等于",
                ComparisonType.Less => "小于",
                ComparisonType.LessOrEqual => "小于等于",
                ComparisonType.Contains => "包含",
                ComparisonType.NotContains => "不包含",
                _ => Comparison.ToString()
            };
        }
        
        /// <summary>
        /// 创建条件的副本
        /// </summary>
        /// <returns>条件的副本</returns>
        public AchievementCondition Clone()
        {
            var clone = new AchievementCondition
            {
                Id = Id,
                EventType = EventType,
                ParameterName = ParameterName,
                ParameterValue = ParameterValue,
                Comparison = Comparison,
                Description = Description,
                IsOptional = IsOptional,
                Weight = Weight,
                Filters = new Dictionary<string, Variant>(Filters)
            };
            
            return clone;
        }
        
        /// <summary>
        /// 验证条件配置的有效性
        /// </summary>
        /// <returns>验证结果和错误信息</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (Weight < 0)
                return (false, "条件权重不能为负数");
                
            // 检查数值比较是否有效
            if (Comparison is ComparisonType.Greater or ComparisonType.GreaterOrEqual or 
                ComparisonType.Less or ComparisonType.LessOrEqual)
            {
                if (ParameterValue.VariantType != Variant.Type.Int && 
                    ParameterValue.VariantType != Variant.Type.Float &&
                    !float.TryParse(ParameterValue.AsString(), out _))
                {
                    return (false, "数值比较需要有效的数值参数");
                }
            }
            
            return (true, string.Empty);
        }
    }
}