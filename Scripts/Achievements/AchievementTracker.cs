using Godot;
using Godot.Collections;
using CodeRogue.Achievements.Data;
using System;
using System.Linq;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就追踪器
    /// 负责监听游戏中的各种事件，并将相关事件转发给成就管理器进行处理
    /// 实现IAchievementTracker接口
    /// </summary>
    public partial class AchievementTracker : Node, IAchievementTracker
    {
        /// <summary>成就管理器引用</summary>
        private AchievementManager _achievementManager;
        
        /// <summary>事件缓冲区</summary>
        private Array<AchievementEventData> _eventBuffer = new Array<AchievementEventData>();
        
        /// <summary>是否启用事件缓冲</summary>
        [Export] public bool EnableEventBuffering { get; set; } = true;
        
        /// <summary>缓冲区最大大小</summary>
        [Export] public int MaxBufferSize { get; set; } = 100;
        
        /// <summary>批处理间隔（秒）</summary>
        [Export] public float BatchProcessInterval { get; set; } = 1.0f;
        
        /// <summary>是否启用调试日志</summary>
        [Export] public bool EnableDebugLog { get; set; } = false;
        
        /// <summary>批处理计时器</summary>
        private Timer _batchTimer;
        
        /// <summary>是否已初始化</summary>
        public bool IsInitialized { get; private set; } = false;
        
        /// <summary>事件统计</summary>
        private Dictionary<GameEventType, int> _eventStats = new Dictionary<GameEventType, int>();
        
        public override void _Ready()
        {
            InitializeTracker();
        }
        
        /// <summary>
        /// 初始化追踪器
        /// </summary>
        private void InitializeTracker()
        {
            try
            {
                GD.Print("[AchievementTracker] 初始化成就追踪器...");
                
                // 获取成就管理器引用
                _achievementManager = GetNode<AchievementManager>("/root/AchievementManager");
                if (_achievementManager == null)
                {
                    GD.PrintErr("[AchievementTracker] AchievementManager未找到，请确保已配置为AutoLoad");
                    return;
                }
                
                // 设置批处理计时器
                SetupBatchTimer();
                
                // 连接游戏事件信号
                ConnectGameEvents();
                
                IsInitialized = true;
                GD.Print("[AchievementTracker] 成就追踪器初始化完成");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementTracker] 初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 设置批处理计时器
        /// </summary>
        private void SetupBatchTimer()
        {
            if (EnableEventBuffering && BatchProcessInterval > 0)
            {
                _batchTimer = new Timer();
                _batchTimer.WaitTime = BatchProcessInterval;
                _batchTimer.Autostart = true;
                _batchTimer.Timeout += ProcessEventBuffer;
                AddChild(_batchTimer);
            }
        }
        
        /// <summary>
        /// 连接游戏事件信号
        /// </summary>
        private void ConnectGameEvents()
        {
            // 连接各种游戏系统的信号
            ConnectCombatEvents();
            ConnectSkillEvents();
            ConnectHeroEvents();
            ConnectItemEvents();
            ConnectProgressEvents();
        }
        
        /// <summary>
        /// 连接战斗相关事件
        /// </summary>
        private void ConnectCombatEvents()
        {
            // 这里需要根据实际的游戏系统来连接信号
            // 示例代码，实际使用时需要替换为真实的信号连接
            
            // var battleManager = GetNodeOrNull("/root/BattleManager");
            // if (battleManager != null)
            // {
            //     battleManager.Connect("BattleStarted", new Callable(this, nameof(OnBattleStarted)));
            //     battleManager.Connect("BattleCompleted", new Callable(this, nameof(OnBattleCompleted)));
            //     battleManager.Connect("EnemyDefeated", new Callable(this, nameof(OnEnemyDefeated)));
            // }
        }
        
        /// <summary>
        /// 连接技能相关事件
        /// </summary>
        private void ConnectSkillEvents()
        {
            // var skillManager = GetNodeOrNull("/root/SkillManager");
            // if (skillManager != null)
            // {
            //     skillManager.Connect("SkillUsed", new Callable(this, nameof(OnSkillUsed)));
            //     skillManager.Connect("SkillLearned", new Callable(this, nameof(OnSkillLearned)));
            // }
        }
        
        /// <summary>
        /// 连接英雄相关事件
        /// </summary>
        private void ConnectHeroEvents()
        {
            // var heroManager = GetNodeOrNull("/root/HeroManager");
            // if (heroManager != null)
            // {
            //     heroManager.Connect("HeroLevelUp", new Callable(this, nameof(OnHeroLevelUp)));
            //     heroManager.Connect("HeroRecruited", new Callable(this, nameof(OnHeroRecruited)));
            // }
        }
        
        /// <summary>
        /// 连接物品相关事件
        /// </summary>
        private void ConnectItemEvents()
        {
            // var inventoryManager = GetNodeOrNull("/root/InventoryManager");
            // if (inventoryManager != null)
            // {
            //     inventoryManager.Connect("ItemCollected", new Callable(this, nameof(OnItemCollected)));
            //     inventoryManager.Connect("ItemUsed", new Callable(this, nameof(OnItemUsed)));
            // }
        }
        
        /// <summary>
        /// 连接进度相关事件
        /// </summary>
        private void ConnectProgressEvents()
        {
            // var gameManager = GetNodeOrNull("/root/GameManager");
            // if (gameManager != null)
            // {
            //     gameManager.Connect("LevelCompleted", new Callable(this, nameof(OnLevelCompleted)));
            //     gameManager.Connect("GameStarted", new Callable(this, nameof(OnGameStarted)));
            // }
        }
        
        /// <summary>
        /// 追踪游戏事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="parameters">事件参数</param>
        public void TrackEvent(GameEventType eventType, Dictionary<string, Variant> parameters = null)
        {
            if (!IsInitialized)
                return;
                
            var eventData = new AchievementEventData
            {
                EventType = eventType,
                Parameters = parameters ?? new Dictionary<string, Variant>(),
                Source = "AchievementTracker"
            };
            
            TrackEvent(eventData);
        }
        
        /// <summary>
        /// 追踪游戏事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void TrackEvent(AchievementEventData eventData)
        {
            if (!IsInitialized || eventData == null)
                return;
                
            try
            {
                // 更新事件统计
                UpdateEventStats(eventData.EventType);
                
                if (EnableDebugLog)
                {
                    GD.Print($"[AchievementTracker] 追踪事件: {eventData.EventType} from {eventData.Source}");
                }
                
                if (EnableEventBuffering)
                {
                    // 添加到缓冲区
                    AddToBuffer(eventData);
                }
                else
                {
                    // 直接处理
                    ProcessEvent(eventData);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementTracker] 追踪事件时发生异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 批量追踪事件
        /// </summary>
        /// <param name="events">事件列表</param>
        public void TrackEvents(Array<AchievementEventData> events)
        {
            if (!IsInitialized || events == null)
                return;
                
            foreach (var eventData in events)
            {
                TrackEvent(eventData);
            }
        }
        
        /// <summary>
        /// 添加事件到缓冲区
        /// </summary>
        /// <param name="eventData">事件数据</param>
        private void AddToBuffer(AchievementEventData eventData)
        {
            _eventBuffer.Add(eventData);
            
            // 检查缓冲区大小
            if (_eventBuffer.Count >= MaxBufferSize)
            {
                ProcessEventBuffer();
            }
        }
        
        /// <summary>
        /// 处理事件缓冲区
        /// </summary>
        private void ProcessEventBuffer()
        {
            if (_eventBuffer.Count == 0)
                return;
                
            try
            {
                if (EnableDebugLog)
                {
                    GD.Print($"[AchievementTracker] 处理事件缓冲区，共 {_eventBuffer.Count} 个事件");
                }
                
                // 合并相同类型的事件
                var mergedEvents = MergeEvents(_eventBuffer);
                
                // 处理合并后的事件
                foreach (var eventData in mergedEvents)
                {
                    ProcessEvent(eventData);
                }
                
                // 清空缓冲区
                _eventBuffer.Clear();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementTracker] 处理事件缓冲区时发生异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 合并相同类型的事件
        /// </summary>
        /// <param name="events">事件列表</param>
        /// <returns>合并后的事件列表</returns>
        private Array<AchievementEventData> MergeEvents(Array<AchievementEventData> events)
        {
            var mergedEvents = new Dictionary<string, AchievementEventData>();
            
            foreach (var eventData in events)
            {
                var key = GetEventMergeKey(eventData);
                
                if (mergedEvents.ContainsKey(key))
                {
                    // 合并事件
                    var existingEvent = mergedEvents[key];
                    existingEvent.Merge(eventData);
                }
                else
                {
                    // 添加新事件
                    mergedEvents[key] = eventData.Clone();
                }
            }
            
            var result = new Array<AchievementEventData>();
            foreach (var eventData in mergedEvents.Values)
            {
                result.Add(eventData);
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取事件合并键
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <returns>合并键</returns>
        private string GetEventMergeKey(AchievementEventData eventData)
        {
            // 基于事件类型和关键参数生成合并键
            var key = eventData.EventType.ToString();
            
            // 添加关键参数到键中
            if (eventData.Parameters.ContainsKey("type"))
            {
                key += "_" + eventData.Parameters["type"].AsString();
            }
            
            if (eventData.Parameters.ContainsKey("id"))
            {
                key += "_" + eventData.Parameters["id"].AsString();
            }
            
            return key;
        }
        
        /// <summary>
        /// 处理单个事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        private void ProcessEvent(AchievementEventData eventData)
        {
            if (_achievementManager != null)
            {
                _achievementManager.HandleGameEvent(eventData);
            }
        }
        
        /// <summary>
        /// 更新事件统计
        /// </summary>
        /// <param name="eventType">事件类型</param>
        private void UpdateEventStats(GameEventType eventType)
        {
            if (_eventStats.ContainsKey(eventType))
            {
                _eventStats[eventType]++;
            }
            else
            {
                _eventStats[eventType] = 1;
            }
        }
        
        /// <summary>
        /// 立即处理所有缓冲的事件
        /// </summary>
        public void FlushBuffer()
        {
            if (_eventBuffer.Count > 0)
            {
                ProcessEventBuffer();
            }
        }
        
        /// <summary>
        /// 获取事件统计信息
        /// </summary>
        /// <returns>事件统计字典</returns>
        public Dictionary<GameEventType, int> GetEventStats()
        {
            return new Dictionary<GameEventType, int>(_eventStats);
        }
        
        /// <summary>
        /// 重置事件统计
        /// </summary>
        public void ResetEventStats()
        {
            _eventStats.Clear();
        }
        
        /// <summary>
        /// 获取缓冲区状态
        /// </summary>
        /// <returns>缓冲区信息</returns>
        public Dictionary<string, Variant> GetBufferStatus()
        {
            return new Dictionary<string, Variant>
            {
                ["buffer_size"] = _eventBuffer.Count,
                ["max_buffer_size"] = MaxBufferSize,
                ["buffering_enabled"] = EnableEventBuffering,
                ["batch_interval"] = BatchProcessInterval
            };
        }
        
        // 以下是具体的事件处理方法，需要根据实际游戏系统进行连接
        
        /// <summary>
        /// 战斗开始事件处理
        /// </summary>
        private void OnBattleStarted()
        {
            TrackEvent(GameEventType.BattleStarted);
        }
        
        /// <summary>
        /// 战斗完成事件处理
        /// </summary>
        /// <param name="result">战斗结果</param>
        private void OnBattleCompleted(string result)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["result"] = result,
                ["battle_count"] = 1
            };
            
            TrackEvent(GameEventType.BattleCompleted, parameters);
        }
        
        /// <summary>
        /// 敌人击败事件处理
        /// </summary>
        /// <param name="enemyType">敌人类型</param>
        /// <param name="count">击败数量</param>
        private void OnEnemyDefeated(string enemyType, int count = 1)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["enemy_type"] = enemyType,
                ["count"] = count
            };
            
            TrackEvent(GameEventType.EnemyDefeated, parameters);
        }
        
        /// <summary>
        /// 技能使用事件处理
        /// </summary>
        /// <param name="skillId">技能ID</param>
        /// <param name="count">使用次数</param>
        private void OnSkillUsed(string skillId, int count = 1)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["skill_id"] = skillId,
                ["count"] = count
            };
            
            TrackEvent(GameEventType.SkillUsed, parameters);
        }
        
        /// <summary>
        /// 技能学习事件处理
        /// </summary>
        /// <param name="skillId">技能ID</param>
        private void OnSkillLearned(string skillId)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["skill_id"] = skillId
            };
            
            TrackEvent(GameEventType.SkillLearned, parameters);
        }
        
        /// <summary>
        /// 英雄升级事件处理
        /// </summary>
        /// <param name="heroId">英雄ID</param>
        /// <param name="newLevel">新等级</param>
        private void OnHeroLevelUp(string heroId, int newLevel)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["hero_id"] = heroId,
                ["level"] = newLevel
            };
            
            TrackEvent(GameEventType.HeroLevelUp, parameters);
        }
        
        /// <summary>
        /// 英雄招募事件处理
        /// </summary>
        /// <param name="heroId">英雄ID</param>
        private void OnHeroRecruited(string heroId)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["hero_id"] = heroId
            };
            
            TrackEvent(GameEventType.HeroRecruited, parameters);
        }
        
        /// <summary>
        /// 物品收集事件处理
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="count">数量</param>
        private void OnItemCollected(string itemId, int count = 1)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["item_id"] = itemId,
                ["count"] = count
            };
            
            TrackEvent(GameEventType.ItemCollected, parameters);
        }
        
        /// <summary>
        /// 物品使用事件处理
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="count">数量</param>
        private void OnItemUsed(string itemId, int count = 1)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["item_id"] = itemId,
                ["count"] = count
            };
            
            TrackEvent(GameEventType.ItemUsed, parameters);
        }
        
        /// <summary>
        /// 关卡完成事件处理
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        private void OnLevelCompleted(string levelId)
        {
            var parameters = new Dictionary<string, Variant>
            {
                ["level_id"] = levelId
            };
            
            TrackEvent(GameEventType.LevelCompleted, parameters);
        }
        
        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        private void OnGameStarted()
        {
            TrackEvent(GameEventType.GameStarted);
        }
        
        /// <summary>
        /// 手动触发事件（用于测试）
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="parameters">事件参数</param>
        public void TriggerTestEvent(GameEventType eventType, Dictionary<string, Variant> parameters = null)
        {
            if (EnableDebugLog)
            {
                GD.Print($"[AchievementTracker] 触发测试事件: {eventType}");
            }
            
            TrackEvent(eventType, parameters);
        }
    }
    
    /// <summary>
    /// 成就追踪器接口
    /// </summary>
    public interface IAchievementTracker
    {
        /// <summary>追踪游戏事件</summary>
        void TrackEvent(GameEventType eventType, Dictionary<string, Variant> parameters = null);
        
        /// <summary>追踪游戏事件</summary>
        void TrackEvent(AchievementEventData eventData);
        
        /// <summary>批量追踪事件</summary>
        void TrackEvents(Array<AchievementEventData> events);
        
        /// <summary>立即处理所有缓冲的事件</summary>
        void FlushBuffer();
        
        /// <summary>获取事件统计信息</summary>
        Dictionary<GameEventType, int> GetEventStats();
        
        /// <summary>重置事件统计</summary>
        void ResetEventStats();
    }
}