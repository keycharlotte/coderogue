using Godot;
using Godot.Collections;
using CodeRogue.Achievements.Data;
using System;
using System.Linq;
using Array = Godot.Collections.Array;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就配置数据库
    /// 继承自Node，配置为AutoLoad单例
    /// 负责加载、管理和提供成就配置数据
    /// </summary>
    public partial class AchievementDatabase : Node
    {
        /// <summary>成就配置字典，键为成就ID</summary>
        private Dictionary<string, AchievementConfig> _achievementConfigs = new Dictionary<string, AchievementConfig>();
        
        /// <summary>按分类分组的成就配置</summary>
        private Dictionary<AchievementCategory, Array<AchievementConfig>> _achievementsByCategory = new Dictionary<AchievementCategory, Array<AchievementConfig>>();
        
        /// <summary>按类型分组的成就配置</summary>
        private Dictionary<AchievementType, Array<AchievementConfig>> _achievementsByType = new Dictionary<AchievementType, Array<AchievementConfig>>();
        
        /// <summary>按稀有度分组的成就配置</summary>
        private Dictionary<AchievementRarity, Array<AchievementConfig>> _achievementsByRarity = new Dictionary<AchievementRarity, Array<AchievementConfig>>();
        
        /// <summary>配置文件路径</summary>
        [Export] public string ConfigFilePath { get; set; } = "res://ResourcesData/Achievements/achievements_config.json";
        
        /// <summary>是否已加载配置</summary>
        public bool IsLoaded { get; private set; } = false;
        
        /// <summary>配置加载完成信号</summary>
        [Signal] public delegate void ConfigsLoadedEventHandler();
        
        /// <summary>配置加载失败信号</summary>
        [Signal] public delegate void ConfigsLoadFailedEventHandler(string error);
        
        public override void _Ready()
        {
            LoadConfigs();
        }
        
        /// <summary>
        /// 加载成就配置
        /// </summary>
        public void LoadConfigs()
        {
            try
            {
                GD.Print("[AchievementDatabase] 开始加载成就配置...");
                
                // 清空现有数据
                ClearConfigs();
                
                // 检查配置文件是否存在
                if (!FileAccess.FileExists(ConfigFilePath))
                {
                    GD.PrintErr($"[AchievementDatabase] 配置文件不存在: {ConfigFilePath}");
                    CreateDefaultConfig();
                    return;
                }
                
                // 读取配置文件
                using var file = FileAccess.Open(ConfigFilePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    var error = $"无法打开配置文件: {ConfigFilePath}";
                    GD.PrintErr($"[AchievementDatabase] {error}");
                    EmitSignal(SignalName.ConfigsLoadFailed, error);
                    return;
                }
                
                var jsonText = file.GetAsText();
                var json = Json.ParseString(jsonText);
                
                if (json.VariantType != Variant.Type.Dictionary)
                {
                    var error = "配置文件格式错误，根节点必须是字典";
                    GD.PrintErr($"[AchievementDatabase] {error}");
                    EmitSignal(SignalName.ConfigsLoadFailed, error);
                    return;
                }
                
                var configData = json.AsGodotDictionary();
                var typedConfigData = new Dictionary<string, Variant>();
                foreach (var kvp in configData)
                {
                    typedConfigData[kvp.Key.AsString()] = kvp.Value;
                }
                
                // 解析成就配置
                if (typedConfigData.ContainsKey("achievements"))
                {
                    var achievementsData = typedConfigData["achievements"].AsGodotArray();
                    ParseAchievements(achievementsData);
                }
                
                // 构建索引
                BuildIndexes();
                
                IsLoaded = true;
                GD.Print($"[AchievementDatabase] 成功加载 {_achievementConfigs.Count} 个成就配置");
                EmitSignal(SignalName.ConfigsLoaded);
            }
            catch (Exception ex)
            {
                var error = $"加载配置时发生异常: {ex.Message}";
                GD.PrintErr($"[AchievementDatabase] {error}");
                EmitSignal(SignalName.ConfigsLoadFailed, error);
            }
        }
        
        /// <summary>
        /// 解析成就配置数据
        /// </summary>
        /// <param name="achievementsData">成就数据数组</param>
        private void ParseAchievements(Array achievementsData)
        {
            foreach (var item in achievementsData)
            {
                if (item.VariantType != Variant.Type.Dictionary)
                    continue;
                    
                var rawAchievementData = item.AsGodotDictionary();
                    var achievementData = new Dictionary<string, Variant>();
                    foreach (var kvp in rawAchievementData)
                    {
                        achievementData[kvp.Key.AsString()] = kvp.Value;
                    }
                var config = ParseAchievementConfig(achievementData);
                
                if (config != null)
                {
                    _achievementConfigs[config.Id] = config;
                }
            }
        }
        
        /// <summary>
        /// 解析单个成就配置
        /// </summary>
        /// <param name="data">成就数据字典</param>
        /// <returns>成就配置对象</returns>
        private AchievementConfig ParseAchievementConfig(Dictionary<string, Variant> data)
        {
            try
            {
                var config = new AchievementConfig();
                
                // 基本信息
                if (data.ContainsKey("id")) config.Id = data["id"].AsString();
                if (data.ContainsKey("name")) config.Name = data["name"].AsString();
                if (data.ContainsKey("description")) config.Description = data["description"].AsString();
                if (data.ContainsKey("target_value")) config.TargetValue = data["target_value"].AsInt32();
                if (data.ContainsKey("icon_path")) config.IconPath = data["icon_path"].AsString();
                if (data.ContainsKey("is_hidden")) config.IsHidden = data["is_hidden"].AsBool();
                if (data.ContainsKey("achievement_points")) config.Points = data["achievement_points"].AsInt32();
                if (data.ContainsKey("is_repeatable")) config.IsRepeatable = data["is_repeatable"].AsBool();
                
                // 枚举类型
                if (data.ContainsKey("type"))
                {
                    if (Enum.TryParse<AchievementType>(data["type"].AsString(), out var type))
                        config.Type = type;
                }
                
                if (data.ContainsKey("category"))
                {
                    if (Enum.TryParse<AchievementCategory>(data["category"].AsString(), out var category))
                        config.Category = category;
                }
                
                if (data.ContainsKey("rarity"))
                {
                    if (Enum.TryParse<AchievementRarity>(data["rarity"].AsString(), out var rarity))
                        config.Rarity = rarity;
                }
                
                if (data.ContainsKey("condition_logic"))
                {
                    if (Enum.TryParse<ConditionLogicType>(data["condition_logic"].AsString(), out var logic))
                        config.ConditionLogic = logic;
                }
                
                if (data.ContainsKey("reset_cycle"))
                {
                    // ResetCycle枚举暂时注释，使用ResetPeriodDays代替
                    // if (Enum.TryParse<ResetCycle>(data["reset_cycle"].AsString(), out var cycle))
                    //     config.ResetCycle = cycle;
                    if (data["reset_cycle"].AsString() == "Never")
                        config.ResetPeriodDays = 0;
                    else if (data["reset_cycle"].AsString() == "Daily")
                        config.ResetPeriodDays = 1;
                    else if (data["reset_cycle"].AsString() == "Weekly")
                        config.ResetPeriodDays = 7;
                    else if (data["reset_cycle"].AsString() == "Monthly")
                        config.ResetPeriodDays = 30;
                }
                
                // 前置成就
                if (data.ContainsKey("prerequisites"))
                {
                    var prerequisites = data["prerequisites"].AsGodotArray();
                    var prerequisitesList = new System.Collections.Generic.List<string>();
                    foreach (var prereq in prerequisites)
                    {
                        prerequisitesList.Add(prereq.AsString());
                    }
                    config.Prerequisites = prerequisitesList.ToArray();
                }
                
                // 奖励配置
                if (data.ContainsKey("rewards"))
                {
                    var rewardsData = data["rewards"].AsGodotArray();
                    var rewardsList = new System.Collections.Generic.List<RewardConfig>();
                    foreach (var rewardItem in rewardsData)
                    {
                        if (rewardItem.VariantType == Variant.Type.Dictionary)
                        {
                            var rawRewardData = rewardItem.AsGodotDictionary();
                            var rewardData = new Dictionary<string, Variant>();
                            foreach (var kvp in rawRewardData)
                            {
                                rewardData[kvp.Key.AsString()] = kvp.Value;
                            }
                            var rewardConfig = ParseRewardConfig(rewardData);
                            if (rewardConfig != null)
                            {
                                rewardsList.Add(rewardConfig);
                            }
                        }
                    }
                    config.Rewards = rewardsList.ToArray();
                }
                
                // 成就条件
                if (data.ContainsKey("conditions"))
                {
                    var conditionsData = data["conditions"].AsGodotArray();
                    var conditionsList = new System.Collections.Generic.List<AchievementCondition>();
                    foreach (var conditionItem in conditionsData)
                    {
                        if (conditionItem.VariantType == Variant.Type.Dictionary)
                        {
                            var rawConditionData = conditionItem.AsGodotDictionary();
                            var conditionData = new Dictionary<string, Variant>();
                            foreach (var kvp in rawConditionData)
                            {
                                conditionData[kvp.Key.AsString()] = kvp.Value;
                            }
                            var condition = ParseCondition(conditionData);
                            if (condition != null)
                            {
                                conditionsList.Add(condition);
                            }
                        }
                    }
                    config.Conditions = conditionsList.ToArray();
                }
                
                // 额外数据
                if (data.ContainsKey("extra_data"))
                {
                    var rawExtraData = data["extra_data"].AsGodotDictionary();
                    config.ExtraData = new Dictionary<string, Variant>();
                    foreach (var kvp in rawExtraData)
                    {
                        config.ExtraData[kvp.Key.AsString()] = kvp.Value;
                    }
                }
                
                // 验证配置
                var (isValid, errorMessage) = config.Validate();
                if (!isValid)
                {
                    GD.PrintErr($"[AchievementDatabase] 成就配置验证失败 {config.Id}: {errorMessage}");
                    return null;
                }
                
                return config;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementDatabase] 解析成就配置时发生异常: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 解析奖励配置
        /// </summary>
        /// <param name="data">奖励数据字典</param>
        /// <returns>奖励配置对象</returns>
        private RewardConfig ParseRewardConfig(Dictionary<string, Variant> data)
        {
            try
            {
                var reward = new RewardConfig();
                
                if (data.ContainsKey("type"))
                {
                    if (Enum.TryParse<RewardType>(data["type"].AsString(), out var type))
                        reward.Type = type;
                }
                
                if (data.ContainsKey("item_id")) reward.ItemId = data["item_id"].AsString();
                if (data.ContainsKey("amount")) reward.Quantity = data["amount"].AsInt32();
                if (data.ContainsKey("description")) reward.Description = data["description"].AsString();
                if (data.ContainsKey("icon_path")) reward.IconPath = data["icon_path"].AsString();
                if (data.ContainsKey("is_rare")) reward.IsRare = data["is_rare"].AsBool();
                if (data.ContainsKey("weight")) reward.Weight = data["weight"].AsInt32();
                
                if (data.ContainsKey("parameters"))
                {
                    var rawParameters = data["parameters"].AsGodotDictionary();
                    reward.Parameters = new Dictionary<string, Variant>();
                    foreach (var kvp in rawParameters)
                    {
                        reward.Parameters[kvp.Key.AsString()] = kvp.Value;
                    }
                }
                
                return reward;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementDatabase] 解析奖励配置时发生异常: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 解析成就条件
        /// </summary>
        /// <param name="data">条件数据字典</param>
        /// <returns>成就条件对象</returns>
        private AchievementCondition ParseCondition(Dictionary<string, Variant> data)
        {
            try
            {
                var condition = new AchievementCondition();
                
                if (data.ContainsKey("id")) condition.Id = data["id"].AsInt32();
                if (data.ContainsKey("parameter_name")) condition.ParameterName = data["parameter_name"].AsString();
                if (data.ContainsKey("parameter_value")) condition.ParameterValue = data["parameter_value"];
                if (data.ContainsKey("description")) condition.Description = data["description"].AsString();
                if (data.ContainsKey("is_optional")) condition.IsOptional = data["is_optional"].AsBool();
                if (data.ContainsKey("weight")) condition.Weight = data["weight"].AsInt32();
                
                if (data.ContainsKey("event_type"))
                {
                    if (Enum.TryParse<GameEventType>(data["event_type"].AsString(), out var eventType))
                        condition.EventType = eventType;
                }
                
                if (data.ContainsKey("comparison_type"))
                {
                    if (Enum.TryParse<ComparisonType>(data["comparison_type"].AsString(), out var comparisonType))
                        condition.Comparison = comparisonType;
                }
                
                if (data.ContainsKey("filters"))
                {
                    var rawFilters = data["filters"].AsGodotDictionary();
                    condition.Filters = new Dictionary<string, Variant>();
                    foreach (var kvp in rawFilters)
                    {
                        condition.Filters[kvp.Key.AsString()] = kvp.Value;
                    }
                }
                
                return condition;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementDatabase] 解析成就条件时发生异常: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 构建索引
        /// </summary>
        private void BuildIndexes()
        {
            _achievementsByCategory.Clear();
            _achievementsByType.Clear();
            _achievementsByRarity.Clear();
            
            foreach (var config in _achievementConfigs.Values)
            {
                // 按分类索引
                if (!_achievementsByCategory.ContainsKey(config.Category))
                    _achievementsByCategory[config.Category] = new Array<AchievementConfig>();
                _achievementsByCategory[config.Category].Add(config);
                
                // 按类型索引
                if (!_achievementsByType.ContainsKey(config.Type))
                    _achievementsByType[config.Type] = new Array<AchievementConfig>();
                _achievementsByType[config.Type].Add(config);
                
                // 按稀有度索引
                if (!_achievementsByRarity.ContainsKey(config.Rarity))
                    _achievementsByRarity[config.Rarity] = new Array<AchievementConfig>();
                _achievementsByRarity[config.Rarity].Add(config);
            }
        }
        
        /// <summary>
        /// 创建默认配置文件
        /// </summary>
        private void CreateDefaultConfig()
        {
            try
            {
                var defaultConfig = new Dictionary<string, Variant>
                {
                    ["achievements"] = new Array<Variant>
                    {
                        new Dictionary<string, Variant>
                        {
                            ["id"] = "first_battle",
                            ["name"] = "初次战斗",
                            ["description"] = "完成第一场战斗",
                            ["type"] = "Combat",
                            ["category"] = "Combat",
                            ["rarity"] = "Common",
                            ["target_value"] = 1,
                            ["icon_path"] = "res://Icons/Achievements/first_battle.png",
                            ["is_hidden"] = false,
                            ["achievement_points"] = 10,
                            ["is_repeatable"] = false,
                            ["condition_logic"] = "And",
                            ["reset_cycle"] = "Never",
                            ["prerequisites"] = new Array<Variant>(),
                            ["rewards"] = new Array<Variant>
                            {
                                new Dictionary<string, Variant>
                                {
                                    ["type"] = "Experience",
                                    ["amount"] = 100,
                                    ["description"] = "获得100经验值"
                                }
                            },
                            ["conditions"] = new Array<Variant>
                            {
                                new Dictionary<string, Variant>
                                {
                                    ["id"] = "battle_complete",
                                    ["event_type"] = "BattleCompleted",
                                    ["parameter_name"] = "battle_count",
                                    ["parameter_value"] = "1",
                                    ["comparison_type"] = "GreaterOrEqual",
                                    ["description"] = "完成战斗次数大于等于1",
                                    ["is_optional"] = false,
                                    ["weight"] = 1
                                }
                            }
                        }
                    }
                };
                
                // 确保目录存在
                var dir = ConfigFilePath.GetBaseDir();
                if (!DirAccess.DirExistsAbsolute(dir))
                {
                    DirAccess.MakeDirRecursiveAbsolute(dir);
                }
                
                // 写入默认配置
                using var file = FileAccess.Open(ConfigFilePath, FileAccess.ModeFlags.Write);
                if (file != null)
                {
                    var jsonString = Json.Stringify(defaultConfig, "\t");
                    file.StoreString(jsonString);
                    GD.Print($"[AchievementDatabase] 已创建默认配置文件: {ConfigFilePath}");
                    
                    // 重新加载配置
                    LoadConfigs();
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementDatabase] 创建默认配置文件时发生异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 清空配置数据
        /// </summary>
        private void ClearConfigs()
        {
            _achievementConfigs.Clear();
            _achievementsByCategory.Clear();
            _achievementsByType.Clear();
            _achievementsByRarity.Clear();
            IsLoaded = false;
        }
        
        /// <summary>
        /// 获取成就配置
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>成就配置，如果不存在返回null</returns>
        public AchievementConfig GetAchievementConfig(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId))
                return null;
                
            return _achievementConfigs.ContainsKey(achievementId) ? _achievementConfigs[achievementId] : null;
        }
        
        /// <summary>
        /// 根据ID获取成就配置（GetAchievementConfig的别名）
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>成就配置，如果不存在返回null</returns>
        public AchievementConfig GetAchievementById(string achievementId)
        {
            return GetAchievementConfig(achievementId);
        }
        
        /// <summary>
        /// 获取所有成就配置
        /// </summary>
        /// <returns>成就配置数组</returns>
        public Array<AchievementConfig> GetAllAchievements()
        {
            var result = new Array<AchievementConfig>();
            foreach (var config in _achievementConfigs.Values)
            {
                result.Add(config);
            }
            return result;
        }
        
        /// <summary>
        /// 根据分类获取成就配置
        /// </summary>
        /// <param name="category">成就分类</param>
        /// <returns>成就配置数组</returns>
        public Array<AchievementConfig> GetAchievementsByCategory(AchievementCategory category)
        {
            return _achievementsByCategory.ContainsKey(category) ? _achievementsByCategory[category] : new Array<AchievementConfig>();
        }
        
        /// <summary>
        /// 根据类型获取成就配置
        /// </summary>
        /// <param name="type">成就类型</param>
        /// <returns>成就配置数组</returns>
        public Array<AchievementConfig> GetAchievementsByType(AchievementType type)
        {
            return _achievementsByType.ContainsKey(type) ? _achievementsByType[type] : new Array<AchievementConfig>();
        }
        
        /// <summary>
        /// 根据稀有度获取成就配置
        /// </summary>
        /// <param name="rarity">成就稀有度</param>
        /// <returns>成就配置数组</returns>
        public Array<AchievementConfig> GetAchievementsByRarity(AchievementRarity rarity)
        {
            return _achievementsByRarity.ContainsKey(rarity) ? _achievementsByRarity[rarity] : new Array<AchievementConfig>();
        }
        
        /// <summary>
        /// 检查成就是否存在
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>是否存在</returns>
        public bool HasAchievement(string achievementId)
        {
            return !string.IsNullOrEmpty(achievementId) && _achievementConfigs.ContainsKey(achievementId);
        }
        
        /// <summary>
        /// 获取成就总数
        /// </summary>
        /// <returns>成就总数</returns>
        public int GetAchievementCount()
        {
            return _achievementConfigs.Count;
        }
        
        /// <summary>
        /// 获取指定分类的成就数量
        /// </summary>
        /// <param name="category">成就分类</param>
        /// <returns>成就数量</returns>
        public int GetAchievementCountByCategory(AchievementCategory category)
        {
            return _achievementsByCategory.ContainsKey(category) ? _achievementsByCategory[category].Count : 0;
        }
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void ReloadConfigs()
        {
            GD.Print("[AchievementDatabase] 重新加载配置...");
            LoadConfigs();
        }
    }
}