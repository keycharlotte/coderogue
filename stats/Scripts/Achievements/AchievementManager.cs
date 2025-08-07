using Godot;
using Godot.Collections;
using CodeRogue.Achievements.Data;
using System;
using System.Linq;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就管理器
    /// 继承自Node，配置为AutoLoad单例
    /// 负责成就系统的核心业务逻辑，包括进度追踪、完成检测、奖励发放等
    /// </summary>
    public partial class AchievementManager : Node
    {
        /// <summary>成就进度字典，键为成就ID</summary>
        private Dictionary<string, AchievementProgress> _achievementProgress = new Dictionary<string, AchievementProgress>();
        
        /// <summary>成就数据库引用</summary>
        private AchievementDatabase _database;
        
        /// <summary>成就追踪器引用</summary>
        private AchievementTracker _tracker;
        
        /// <summary>奖励系统引用</summary>
        private AchievementRewardSystem _rewardSystem;
        
        /// <summary>通知系统引用</summary>
        private AchievementNotificationSystem _notificationSystem;
        
        /// <summary>存档文件路径</summary>
        [Export] public string SaveFilePath { get; set; } = "user://achievement_progress.save";
        
        /// <summary>是否自动保存</summary>
        [Export] public bool AutoSave { get; set; } = true;
        
        /// <summary>自动保存间隔（秒）</summary>
        [Export] public float AutoSaveInterval { get; set; } = 30.0f;
        
        /// <summary>是否已初始化</summary>
        public bool IsInitialized { get; private set; } = false;
        
        /// <summary>自动保存计时器</summary>
        private Timer _autoSaveTimer;
        
        /// <summary>成就完成信号</summary>
        [Signal] public delegate void AchievementCompletedEventHandler(string achievementId);
        
        /// <summary>成就解锁信号</summary>
        [Signal] public delegate void AchievementUnlockedEventHandler(string achievementId);
        
        /// <summary>成就进度更新信号</summary>
        [Signal] public delegate void AchievementProgressUpdatedEventHandler(string achievementId, float currentValue, float targetValue);
        
        /// <summary>奖励获得信号</summary>
        [Signal] public delegate void RewardReceivedEventHandler(string achievementId, Array<RewardConfig> rewards);
        
        /// <summary>奖励发放信号（用于兼容性）</summary>
        [Signal] public delegate void RewardGrantedEventHandler(string achievementId, Array<RewardConfig> rewards);
        
        /// <summary>系统初始化完成信号</summary>
        [Signal] public delegate void SystemInitializedEventHandler();
        
        public override void _Ready()
        {
            InitializeSystem();
        }
        
        /// <summary>
        /// 初始化成就系统
        /// </summary>
        private async void InitializeSystem()
        {
            try
            {
                GD.Print("[AchievementManager] 开始初始化成就系统...");
                
                // 获取依赖组件
                await GetDependencies();
                
                // 加载成就进度
                LoadProgress();
                
                // 初始化子系统
                InitializeSubSystems();
                
                // 设置自动保存
                SetupAutoSave();
                
                // 连接信号
                ConnectSignals();
                
                IsInitialized = true;
                GD.Print("[AchievementManager] 成就系统初始化完成");
                EmitSignal(SignalName.SystemInitialized);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementManager] 初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取依赖组件
        /// </summary>
        private async System.Threading.Tasks.Task GetDependencies()
        {
            // 等待数据库加载完成
            _database = GetNode<AchievementDatabase>("/root/AchievementDatabase");
            if (_database == null)
            {
                throw new InvalidOperationException("AchievementDatabase未找到，请确保已配置为AutoLoad");
            }
            
            // 等待数据库加载完成
            while (!_database.IsLoaded)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            
            // 获取其他组件（这些组件可能还未创建，所以使用延迟获取）
            CallDeferred(nameof(GetSubSystems));
        }
        
        /// <summary>
        /// 获取子系统组件
        /// </summary>
        private void GetSubSystems()
        {
            _tracker = GetNodeOrNull<AchievementTracker>("/root/AchievementTracker");
            _rewardSystem = GetNodeOrNull<AchievementRewardSystem>("/root/AchievementRewardSystem");
            _notificationSystem = GetNodeOrNull<AchievementNotificationSystem>("/root/AchievementNotificationSystem");
        }
        
        /// <summary>
        /// 初始化子系统
        /// </summary>
        private void InitializeSubSystems()
        {
            // 初始化所有成就的进度数据
            var allAchievements = _database.GetAllAchievements();
            foreach (var config in allAchievements)
            {
                if (!_achievementProgress.ContainsKey(config.Id))
                {
                    var progress = new AchievementProgress
                    {
                        AchievementId = config.Id,
                        TargetValue = config.TargetValue,
                        Status = config.IsHidden ? AchievementStatus.Locked : AchievementStatus.Unlocked
                    };
                    
                    // 检查前置条件
                    if (!CheckPrerequisites(config))
                    {
                        progress.Status = AchievementStatus.Locked;
                    }
                    
                    _achievementProgress[config.Id] = progress;
                }
            }
        }
        
        /// <summary>
        /// 设置自动保存
        /// </summary>
        private void SetupAutoSave()
        {
            if (AutoSave && AutoSaveInterval > 0)
            {
                _autoSaveTimer = new Timer();
                _autoSaveTimer.WaitTime = AutoSaveInterval;
                _autoSaveTimer.Autostart = true;
                _autoSaveTimer.Timeout += SaveProgress;
                AddChild(_autoSaveTimer);
            }
        }
        
        /// <summary>
        /// 连接信号
        /// </summary>
        private void ConnectSignals()
        {
            if (_database != null)
            {
                _database.ConfigsLoaded += OnConfigsLoaded;
            }
        }
        
        /// <summary>
        /// 配置加载完成回调
        /// </summary>
        private void OnConfigsLoaded()
        {
            GD.Print("[AchievementManager] 配置重新加载，更新成就进度");
            InitializeSubSystems();
        }
        
        /// <summary>
        /// 处理游戏事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void HandleGameEvent(AchievementEventData eventData)
        {
            if (!IsInitialized || eventData == null)
                return;
                
            try
            {
                // 获取所有可能受影响的成就
                var affectedAchievements = GetAffectedAchievements(eventData);
                
                foreach (var achievementId in affectedAchievements)
                {
                    UpdateAchievementProgress(achievementId, eventData);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementManager] 处理游戏事件时发生异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取受事件影响的成就列表
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <returns>受影响的成就ID列表</returns>
        private Array<string> GetAffectedAchievements(AchievementEventData eventData)
        {
            var result = new Array<string>();
            var allAchievements = _database.GetAllAchievements();
            
            foreach (var config in allAchievements)
            {
                // 检查成就是否已完成
                var progress = GetAchievementProgress(config.Id);
                if (progress?.Status == AchievementStatus.Completed)
                    continue;
                    
                // 检查成就是否解锁
                if (progress?.Status == AchievementStatus.Locked)
                    continue;
                    
                // 检查事件是否匹配成就条件
                if (IsEventMatchingAchievement(eventData, config))
                {
                    result.Add(config.Id);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 检查事件是否匹配成就条件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <param name="config">成就配置</param>
        /// <returns>是否匹配</returns>
        private bool IsEventMatchingAchievement(AchievementEventData eventData, AchievementConfig config)
        {
            if (config.Conditions == null || config.Conditions.Length == 0)
                return false;
                
            foreach (var condition in config.Conditions)
            {
                if (condition.EventType == eventData.EventType)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 更新成就进度
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <param name="eventData">事件数据</param>
        private void UpdateAchievementProgress(string achievementId, AchievementEventData eventData)
        {
            var config = _database.GetAchievementConfig(achievementId);
            var progress = GetAchievementProgress(achievementId);
            
            if (config == null || progress == null)
                return;
                
            // 检查条件是否满足
            var conditionResults = new Array<bool>();
            foreach (var condition in config.Conditions)
            {
                var result = condition.CheckCondition(eventData);
                conditionResults.Add(result);
            }
            
            // 根据逻辑类型判断是否满足条件
            bool conditionsMet = config.ConditionLogic == ConditionLogicType.And 
                ? conditionResults.All(r => r)
                : conditionResults.Any(r => r);
                
            if (conditionsMet)
            {
                // 更新进度值
                var increment = CalculateProgressIncrement(config, eventData);
                var oldValue = progress.CurrentValue;
                progress.UpdateProgress(progress.CurrentValue + increment);
                
                // 发送进度更新信号
                EmitSignal(SignalName.AchievementProgressUpdated, achievementId, (float)progress.CurrentValue, (float)progress.TargetValue);
                
                // 检查是否完成
                if (progress.CheckCompletionCondition())
                {
                    CompleteAchievement(achievementId);
                }
                
                GD.Print($"[AchievementManager] 成就 {achievementId} 进度更新: {oldValue} -> {progress.CurrentValue}/{progress.TargetValue}");
            }
        }
        
        /// <summary>
        /// 计算进度增量
        /// </summary>
        /// <param name="config">成就配置</param>
        /// <param name="eventData">事件数据</param>
        /// <returns>进度增量</returns>
        private int CalculateProgressIncrement(AchievementConfig config, AchievementEventData eventData)
        {
            // 默认增量为1
            int increment = 1;
            
            // 如果事件数据中包含批量信息
            if (eventData.IsBatch && eventData.BatchCount > 0)
            {
                increment = eventData.BatchCount;
            }
            
            // 检查事件参数中是否有增量信息
            if (eventData.Parameters.ContainsKey("increment"))
            {
                if (eventData.Parameters["increment"].VariantType == Variant.Type.Int)
                {
                    increment = eventData.Parameters["increment"].AsInt32();
                }
            }
            
            return increment;
        }
        
        /// <summary>
        /// 完成成就
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        private void CompleteAchievement(string achievementId)
        {
            var config = _database.GetAchievementConfig(achievementId);
            var progress = GetAchievementProgress(achievementId);
            
            if (config == null || progress == null)
                return;
                
            // 标记为完成
            progress.MarkAsCompleted();
            
            GD.Print($"[AchievementManager] 成就完成: {config.Name} ({achievementId})");
            
            // 发送完成信号
            EmitSignal(SignalName.AchievementCompleted, achievementId);
            
            // 发放奖励
            if (config.Rewards != null && config.Rewards.Length > 0)
            {
                GiveRewards(achievementId, config.Rewards);
            }
            
            // 发送通知
            if (_notificationSystem != null)
            {
                var notification = AchievementNotification.CreateCompletedNotification(config);
                _notificationSystem.ShowNotification(notification);
            }
            
            // 检查是否解锁新成就
            CheckUnlockNewAchievements();
            
            // 自动保存
            if (AutoSave)
            {
                SaveProgress();
            }
        }
        
        /// <summary>
        /// 发放奖励
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <param name="rewards">奖励列表</param>
        private void GiveRewards(string achievementId, RewardConfig[] rewards)
        {
            if (_rewardSystem != null)
            {
                var rewardArray = new Array<RewardConfig>();
                foreach (var reward in rewards)
                {
                    rewardArray.Add(reward);
                }
                _rewardSystem.GrantRewards(achievementId, rewardArray);
            }
            
            // 发送奖励获得信号
            EmitSignal(SignalName.RewardReceived, achievementId, rewards);
                EmitSignal(SignalName.RewardGranted, achievementId, rewards);
            
            // 发送奖励通知
            if (_notificationSystem != null)
            {
                var config = _database.GetAchievementConfig(achievementId);
                if (config != null)
                {
                    var notification = AchievementNotification.CreateRewardNotification(config, rewards);
                    _notificationSystem.ShowNotification(notification);
                }
            }
        }
        
        /// <summary>
        /// 检查解锁新成就
        /// </summary>
        private void CheckUnlockNewAchievements()
        {
            var allAchievements = _database.GetAllAchievements();
            
            foreach (var config in allAchievements)
            {
                var progress = GetAchievementProgress(config.Id);
                if (progress?.Status == AchievementStatus.Locked)
                {
                    if (CheckPrerequisites(config))
                    {
                        UnlockAchievement(config.Id);
                    }
                }
            }
        }
        
        /// <summary>
        /// 解锁成就
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        private void UnlockAchievement(string achievementId)
        {
            var config = _database.GetAchievementConfig(achievementId);
            var progress = GetAchievementProgress(achievementId);
            
            if (config == null || progress == null)
                return;
                
            progress.Unlock();
            
            GD.Print($"[AchievementManager] 成就解锁: {config.Name} ({achievementId})");
            
            // 发送解锁信号
            EmitSignal(SignalName.AchievementUnlocked, achievementId);
            
            // 发送通知
            if (_notificationSystem != null && !config.IsHidden)
            {
                var notification = AchievementNotification.CreateUnlockedNotification(config);
                _notificationSystem.ShowNotification(notification);
            }
        }
        
        /// <summary>
        /// 检查前置条件
        /// </summary>
        /// <param name="config">成就配置</param>
        /// <returns>是否满足前置条件</returns>
        private bool CheckPrerequisites(AchievementConfig config)
        {
            if (config.Prerequisites == null || config.Prerequisites.Length == 0)
                return true;
                
            foreach (var prerequisiteId in config.Prerequisites)
            {
                var prerequisiteProgress = GetAchievementProgress(prerequisiteId);
                if (prerequisiteProgress?.Status != AchievementStatus.Completed)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取成就进度
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>成就进度</returns>
        public AchievementProgress GetAchievementProgress(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId))
                return null;
                
            return _achievementProgress.ContainsKey(achievementId) ? _achievementProgress[achievementId] : null;
        }
        
        /// <summary>
        /// 获取所有成就进度
        /// </summary>
        /// <returns>成就进度字典</returns>
        public Dictionary<string, AchievementProgress> GetAllProgress()
        {
            return new Dictionary<string, AchievementProgress>(_achievementProgress);
        }
        
        /// <summary>
        /// 手动触发成就检查
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        public void TriggerAchievementCheck(string achievementId)
        {
            var config = _database.GetAchievementConfig(achievementId);
            if (config == null)
                return;
                
            // 创建一个通用事件来触发检查
            var eventData = new AchievementEventData
            {
                EventType = GameEventType.Manual,
                Source = "AchievementManager"
            };
            
            UpdateAchievementProgress(achievementId, eventData);
        }
        
        /// <summary>
        /// 重置成就进度
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        public void ResetAchievement(string achievementId)
        {
            var progress = GetAchievementProgress(achievementId);
            if (progress != null)
            {
                progress.Reset();
                GD.Print($"[AchievementManager] 重置成就进度: {achievementId}");
            }
        }
        
        /// <summary>
        /// 保存进度
        /// </summary>
        public void SaveProgress()
        {
            try
            {
                var saveData = new Dictionary<string, Variant>
                {
                    ["version"] = "1.0",
                    ["timestamp"] = DateTime.UtcNow.ToString("O"),
                    ["achievements"] = SerializeProgress()
                };
                
                using var file = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Write);
                if (file != null)
                {
                    var jsonString = Json.Stringify(saveData, "\t");
                    file.StoreString(jsonString);
                    GD.Print($"[AchievementManager] 成就进度已保存到: {SaveFilePath}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementManager] 保存进度时发生异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 加载进度
        /// </summary>
        public void LoadProgress()
        {
            try
            {
                if (!FileAccess.FileExists(SaveFilePath))
                {
                    GD.Print($"[AchievementManager] 存档文件不存在，使用默认进度: {SaveFilePath}");
                    return;
                }
                
                using var file = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[AchievementManager] 无法打开存档文件: {SaveFilePath}");
                    return;
                }
                
                var jsonText = file.GetAsText();
                var json = Json.ParseString(jsonText);
                
                if (json.VariantType == Variant.Type.Dictionary)
                {
                    var saveData = json.AsGodotDictionary();
                    if (saveData.ContainsKey("achievements"))
                    {
                        var achievementsDict = saveData["achievements"].AsGodotDictionary();
                        var typedAchievementsDict = new Dictionary<string, Variant>();
                        foreach (var kvp in achievementsDict)
                        {
                            typedAchievementsDict[kvp.Key.AsString()] = kvp.Value;
                        }
                        DeserializeProgress(typedAchievementsDict);
                        GD.Print($"[AchievementManager] 成就进度已从 {SaveFilePath} 加载");
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementManager] 加载进度时发生异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 序列化进度数据
        /// </summary>
        /// <returns>序列化后的进度数据</returns>
        private Dictionary<string, Variant> SerializeProgress()
        {
            var result = new Dictionary<string, Variant>();
            
            foreach (var kvp in _achievementProgress)
            {
                var progress = kvp.Value;
                result[kvp.Key] = new Dictionary<string, Variant>
                {
                    ["current_value"] = progress.CurrentValue,
                    ["target_value"] = progress.TargetValue,
                    ["is_completed"] = progress.IsCompleted,
                    ["completed_at"] = progress.CompletedAt,
                    ["is_reward_claimed"] = progress.IsRewardClaimed,
                    ["status"] = progress.Status.ToString(),
                    ["unlocked_at"] = progress.UnlockedAt,
                    ["completion_count"] = progress.CompletionCount,
                    ["last_reset_at"] = progress.LastResetAt,
                    ["extra_data"] = progress.ExtraData
                };
            }
            
            return result;
        }
        
        /// <summary>
        /// 反序列化进度数据
        /// </summary>
        /// <param name="data">进度数据字典</param>
        private void DeserializeProgress(Dictionary<string, Variant> data)
        {
            _achievementProgress.Clear();
            
            foreach (var key in data.Keys)
            {
                var achievementId = key;
                var progressDataDict = data[key].AsGodotDictionary();
                var progressData = new Dictionary<string, Variant>();
                foreach (var kvp in progressDataDict)
                {
                    progressData[kvp.Key.AsString()] = kvp.Value;
                }
                
                var progress = new AchievementProgress
                {
                    AchievementId = achievementId
                };
                
                if (progressData.ContainsKey("current_value"))
                    progress.CurrentValue = progressData["current_value"].AsInt32();
                if (progressData.ContainsKey("target_value"))
                    progress.TargetValue = progressData["target_value"].AsInt32();
                if (progressData.ContainsKey("is_completed"))
                    progress.IsCompleted = progressData["is_completed"].AsBool();
                if (progressData.ContainsKey("completed_at"))
                    progress.CompletedAt = progressData["completed_at"].AsString();
                if (progressData.ContainsKey("is_reward_claimed"))
                    progress.IsRewardClaimed = progressData["is_reward_claimed"].AsBool();
                if (progressData.ContainsKey("status"))
                {
                    if (Enum.TryParse<AchievementStatus>(progressData["status"].AsString(), out var status))
                        progress.Status = status;
                }
                if (progressData.ContainsKey("unlocked_at"))
                    progress.UnlockedAt = progressData["unlocked_at"].AsString();
                if (progressData.ContainsKey("completion_count"))
                    progress.CompletionCount = progressData["completion_count"].AsInt32();
                if (progressData.ContainsKey("last_reset_at"))
                    progress.LastResetAt = progressData["last_reset_at"].AsString();
                if (progressData.ContainsKey("extra_data"))
                {
                    var extraDataDict = progressData["extra_data"].AsGodotDictionary();
                    progress.ExtraData = new Dictionary<string, Variant>();
                    foreach (var kvp in extraDataDict)
                    {
                        progress.ExtraData[kvp.Key.AsString()] = kvp.Value;
                    }
                }
                    
                _achievementProgress[achievementId] = progress;
            }
        }
        
        /// <summary>
        /// 获取成就统计信息
        /// </summary>
        /// <returns>统计信息字典</returns>
        public Dictionary<string, Variant> GetStatistics()
        {
            var totalAchievements = _database.GetAchievementCount();
            var completedAchievements = _achievementProgress.Values.Count(p => p.IsCompleted);
            var unlockedAchievements = _achievementProgress.Values.Count(p => p.Status != AchievementStatus.Locked);
            var totalPoints = 0;
            
            foreach (var progress in _achievementProgress.Values)
            {
                if (progress.IsCompleted)
                {
                    var config = _database.GetAchievementConfig(progress.AchievementId);
                    if (config != null)
                    {
                        totalPoints += config.AchievementPoints;
                    }
                }
            }
            
            return new Dictionary<string, Variant>
            {
                ["total_achievements"] = totalAchievements,
                ["completed_achievements"] = completedAchievements,
                ["unlocked_achievements"] = unlockedAchievements,
                ["completion_rate"] = totalAchievements > 0 ? (float)completedAchievements / totalAchievements : 0f,
                ["total_points"] = totalPoints
            };
        }
    }
}