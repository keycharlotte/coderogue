using Godot;
using Godot.Collections;
using System;
using System.Linq;
using CodeRogue.Achievements.Data;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就通知系统
    /// 负责处理成就通知的显示、队列管理和动画效果
    /// </summary>
    public partial class AchievementNotificationSystem : Node, INotificationSystem
    {
        [Signal]
        public delegate void NotificationShownEventHandler(AchievementNotification notification);
        
        [Signal]
        public delegate void NotificationHiddenEventHandler(AchievementNotification notification);
        
        [Signal]
        public delegate void NotificationQueueUpdatedEventHandler(int queueCount);
        
        [Signal]
        public delegate void NotificationClickedEventHandler(AchievementNotification notification);
        
        [Signal]
        public delegate void NotificationExpiredEventHandler(AchievementNotification notification);

        // 通知队列
        private Array<AchievementNotification> _notificationQueue;
        
        // 当前显示的通知
        private Array<AchievementNotification> _activeNotifications;
        
        // 通知历史
        private Array<AchievementNotification> _notificationHistory;
        
        // 通知UI容器
        [Export] public Control NotificationContainer { get; set; }
        
        // 通知预制体场景
        [Export] public PackedScene NotificationPrefab { get; set; }
        
        // 音频播放器
        [Export] public AudioStreamPlayer NotificationAudioPlayer { get; set; }
        
        // 配置参数
        [Export] public int MaxActiveNotifications { get; set; } = 3;
        [Export] public float DefaultDisplayDuration { get; set; } = 5.0f;
        [Export] public float NotificationSpacing { get; set; } = 10.0f;
        [Export] public bool EnableSounds { get; set; } = true;
        [Export] public bool EnableAnimations { get; set; } = true;
        [Export] public bool AutoHideNotifications { get; set; } = true;
        
        // 通知处理器
        private Dictionary<NotificationType, NotificationHandlerBase> _notificationHandlers;
        
        // 通知过滤器
        private Array<NotificationFilterBase> _notificationFilters;
        
        // 通知统计
        private NotificationStatistics _statistics;
        
        // 定时器
        private Timer _cleanupTimer;
        
        public override void _Ready()
        {
            InitializeNotificationSystem();
        }
        
        /// <summary>
        /// 初始化通知系统
        /// </summary>
        private void InitializeNotificationSystem()
        {
            _notificationQueue = new Array<AchievementNotification>();
            _activeNotifications = new Array<AchievementNotification>();
            _notificationHistory = new Array<AchievementNotification>();
            _notificationHandlers = new Dictionary<NotificationType, NotificationHandlerBase>();
            _notificationFilters = new Array<NotificationFilterBase>();
            _statistics = new NotificationStatistics();
            
            // 注册默认通知处理器
            RegisterDefaultHandlers();
            
            // 设置清理定时器
            SetupCleanupTimer();
            
            // 验证必要组件
            ValidateComponents();
            
            GD.Print("[AchievementNotificationSystem] 通知系统初始化完成");
        }
        
        /// <summary>
        /// 注册默认通知处理器
        /// </summary>
        private void RegisterDefaultHandlers()
        {
            RegisterNotificationHandler(NotificationType.AchievementCompleted, new AchievementCompletedHandler());
            RegisterNotificationHandler(NotificationType.AchievementUnlocked, new AchievementUnlockedHandler());
            RegisterNotificationHandler(NotificationType.ProgressUpdated, new ProgressUpdateHandler());
            RegisterNotificationHandler(NotificationType.RewardReceived, new RewardReceivedHandler());
        }
        
        /// <summary>
        /// 设置清理定时器
        /// </summary>
        private void SetupCleanupTimer()
        {
            _cleanupTimer = new Timer();
            _cleanupTimer.WaitTime = 60.0f; // 每分钟清理一次
            _cleanupTimer.Autostart = true;
            _cleanupTimer.Timeout += OnCleanupTimer;
            AddChild(_cleanupTimer);
        }
        
        /// <summary>
        /// 验证必要组件
        /// </summary>
        private void ValidateComponents()
        {
            if (NotificationContainer == null)
            {
                GD.PrintErr("[AchievementNotificationSystem] NotificationContainer 未设置");
            }
            
            if (NotificationPrefab == null)
            {
                GD.PrintErr("[AchievementNotificationSystem] NotificationPrefab 未设置");
            }
            
            if (NotificationAudioPlayer == null)
            {
                GD.PrintErr("[AchievementNotificationSystem] NotificationAudioPlayer 未设置");
            }
        }
        
        /// <summary>
        /// 显示通知
        /// </summary>
        public void ShowNotification(AchievementNotification notification)
        {
            if (notification == null)
            {
                GD.PrintErr("[AchievementNotificationSystem] 尝试显示空通知");
                return;
            }
            
            // 应用过滤器
            if (!ShouldShowNotification(notification))
            {
                GD.Print($"[AchievementNotificationSystem] 通知被过滤器拦截: {notification.Title}");
                return;
            }
            
            // 添加到队列
            _notificationQueue.Add(notification);
            _statistics.IncrementQueuedCount(notification.Type);
            
            // 处理队列
            ProcessNotificationQueue();
            
            // 更新队列信号
            EmitSignal(SignalName.NotificationQueueUpdated, _notificationQueue.Count);
        }
        
        /// <summary>
        /// 处理通知队列
        /// </summary>
        private void ProcessNotificationQueue()
        {
            while (_notificationQueue.Count > 0 && _activeNotifications.Count < MaxActiveNotifications)
            {
                var notification = _notificationQueue[0];
                _notificationQueue.RemoveAt(0);
                
                DisplayNotification(notification);
            }
        }
        
        /// <summary>
        /// 显示单个通知
        /// </summary>
        private void DisplayNotification(AchievementNotification notification)
        {
            if (NotificationContainer == null || NotificationPrefab == null)
            {
                GD.PrintErr("[AchievementNotificationSystem] 通知容器或预制体未设置");
                return;
            }
            
            // 获取通知处理器
            if (!_notificationHandlers.ContainsKey(notification.Type))
            {
                GD.PrintErr($"[AchievementNotificationSystem] 未找到通知类型 {notification.Type} 的处理器");
                return;
            }
            
            var handler = _notificationHandlers[notification.Type];
            
            try
            {
                // 创建通知UI
                var notificationUI = NotificationPrefab.Instantiate() as Control;
                if (notificationUI == null)
                {
                    GD.PrintErr("[AchievementNotificationSystem] 无法实例化通知预制体");
                    return;
                }
                
                // 配置通知UI
                handler.ConfigureNotificationUI(notificationUI, notification);
                
                // 设置位置
                SetNotificationPosition(notificationUI);
                
                // 添加到容器
                NotificationContainer.AddChild(notificationUI);
                
                // 播放音效
                PlayNotificationSound(notification);
                
                // 播放动画
                if (EnableAnimations)
                {
                    PlayShowAnimation(notificationUI, notification);
                }
                
                // 添加到活动列表
                _activeNotifications.Add(notification);
                notification.MarkAsShown();
                
                // 设置自动隐藏
                if (AutoHideNotifications)
                {
                    SetupAutoHide(notificationUI, notification);
                }
                
                // 连接点击事件
                ConnectNotificationEvents(notificationUI, notification);
                
                // 更新统计
                _statistics.IncrementShownCount(notification.Type);
                
                // 发送信号
                EmitSignal(SignalName.NotificationShown, notification);
                
                GD.Print($"[AchievementNotificationSystem] 显示通知: {notification.Title}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[AchievementNotificationSystem] 显示通知时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 设置通知位置
        /// </summary>
        private void SetNotificationPosition(Control notificationUI)
        {
            var yOffset = _activeNotifications.Count * (notificationUI.Size.Y + NotificationSpacing);
            notificationUI.Position = new Vector2(0, yOffset);
        }
        
        /// <summary>
        /// 播放通知音效
        /// </summary>
        private void PlayNotificationSound(AchievementNotification notification)
        {
            if (!EnableSounds || NotificationAudioPlayer == null)
                return;
            
            var soundPath = notification.GetSoundPath();
            if (!string.IsNullOrEmpty(soundPath))
            {
                var audioStream = GD.Load<AudioStream>(soundPath);
                if (audioStream != null)
                {
                    NotificationAudioPlayer.Stream = audioStream;
                    NotificationAudioPlayer.Play();
                }
            }
        }
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        private void PlayShowAnimation(Control notificationUI, AchievementNotification notification)
        {
            var tween = CreateTween();
            
            // 从右侧滑入
            var startPos = notificationUI.Position + new Vector2(300, 0);
            notificationUI.Position = startPos;
            notificationUI.Modulate = new Color(1, 1, 1, 0);
            
            tween.Parallel().TweenProperty(notificationUI, "position", notificationUI.Position - new Vector2(300, 0), 0.5f);
            tween.Parallel().TweenProperty(notificationUI, "modulate:a", 1.0f, 0.3f);
            
            // 根据稀有度添加特效
            if (notification.AchievementId != null)
            {
                // TODO: 根据成就稀有度添加特殊动画效果
            }
        }
        
        /// <summary>
        /// 设置自动隐藏
        /// </summary>
        private void SetupAutoHide(Control notificationUI, AchievementNotification notification)
        {
            var timer = new Timer();
            timer.WaitTime = notification.DisplayDuration > 0 ? notification.DisplayDuration : DefaultDisplayDuration;
            timer.OneShot = true;
            timer.Timeout += () => HideNotification(notificationUI, notification);
            
            notificationUI.AddChild(timer);
            timer.Start();
        }
        
        /// <summary>
        /// 连接通知事件
        /// </summary>
        private void ConnectNotificationEvents(Control notificationUI, AchievementNotification notification)
        {
            // 连接点击事件
            if (notificationUI.HasSignal("gui_input"))
            {
                notificationUI.GuiInput += (inputEvent) => OnNotificationClicked(inputEvent, notification);
            }
        }
        
        /// <summary>
        /// 隐藏通知
        /// </summary>
        public void HideNotification(Control notificationUI, AchievementNotification notification)
        {
            if (notificationUI == null || !IsInstanceValid(notificationUI))
                return;
            
            // 播放隐藏动画
            if (EnableAnimations)
            {
                PlayHideAnimation(notificationUI, () => {
                    RemoveNotification(notificationUI, notification);
                });
            }
            else
            {
                RemoveNotification(notificationUI, notification);
            }
        }
        
        /// <summary>
        /// 播放隐藏动画
        /// </summary>
        private void PlayHideAnimation(Control notificationUI, Action onComplete)
        {
            var tween = CreateTween();
            
            // 向右滑出并淡出
            tween.Parallel().TweenProperty(notificationUI, "position", notificationUI.Position + new Vector2(300, 0), 0.3f);
            tween.Parallel().TweenProperty(notificationUI, "modulate:a", 0.0f, 0.3f);
            
            tween.TweenCallback(Callable.From(onComplete));
        }
        
        /// <summary>
        /// 移除通知
        /// </summary>
        private void RemoveNotification(Control notificationUI, AchievementNotification notification)
        {
            // 从活动列表移除
            _activeNotifications.Remove(notification);
            
            // 添加到历史
            _notificationHistory.Add(notification);
            
            // 移除UI
            if (IsInstanceValid(notificationUI))
            {
                notificationUI.QueueFree();
            }
            
            // 重新排列其他通知
            RearrangeNotifications();
            
            // 处理队列中的下一个通知
            ProcessNotificationQueue();
            
            // 发送信号
            EmitSignal(SignalName.NotificationHidden, notification);
            EmitSignal(SignalName.NotificationQueueUpdated, _notificationQueue.Count);
            
            GD.Print($"[AchievementNotificationSystem] 隐藏通知: {notification.Title}");
        }
        
        /// <summary>
        /// 重新排列通知
        /// </summary>
        private void RearrangeNotifications()
        {
            if (NotificationContainer == null)
                return;
            
            var children = NotificationContainer.GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Control notificationUI)
                {
                    var targetY = i * (notificationUI.Size.Y + NotificationSpacing);
                    
                    if (EnableAnimations)
                    {
                        var tween = CreateTween();
                        tween.TweenProperty(notificationUI, "position:y", targetY, 0.2f);
                    }
                    else
                    {
                        notificationUI.Position = new Vector2(notificationUI.Position.X, targetY);
                    }
                }
            }
        }
        
        /// <summary>
        /// 通知点击事件
        /// </summary>
        private void OnNotificationClicked(InputEvent inputEvent, AchievementNotification notification)
        {
            if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                EmitSignal(SignalName.NotificationClicked, notification);
                GD.Print($"[AchievementNotificationSystem] 通知被点击: {notification.Title}");
            }
        }
        
        /// <summary>
        /// 清理定时器事件
        /// </summary>
        private void OnCleanupTimer()
        {
            CleanupExpiredNotifications();
        }
        
        /// <summary>
        /// 清理过期通知
        /// </summary>
        private void CleanupExpiredNotifications()
        {
            var expiredNotifications = new Array<AchievementNotification>();
            
            foreach (var notification in _notificationHistory)
            {
                if (notification.IsExpired())
                {
                    expiredNotifications.Add(notification);
                }
            }
            
            foreach (var expired in expiredNotifications)
            {
                _notificationHistory.Remove(expired);
                EmitSignal(SignalName.NotificationExpired, expired);
            }
            
            if (expiredNotifications.Count > 0)
            {
                GD.Print($"[AchievementNotificationSystem] 清理了 {expiredNotifications.Count} 个过期通知");
            }
        }
        
        /// <summary>
        /// 检查是否应该显示通知
        /// </summary>
        private bool ShouldShowNotification(AchievementNotification notification)
        {
            foreach (var filter in _notificationFilters)
            {
                if (!filter.ShouldShow(notification))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 注册通知处理器
        /// </summary>
        public void RegisterNotificationHandler(NotificationType type, NotificationHandlerBase handler)
        {
            if (handler == null)
            {
                GD.PrintErr($"[AchievementNotificationSystem] 尝试注册空的通知处理器: {type}");
                return;
            }
            
            _notificationHandlers[type] = handler;
            GD.Print($"[AchievementNotificationSystem] 注册通知处理器: {type}");
        }
        
        /// <summary>
        /// 添加通知过滤器
        /// </summary>
        public void AddNotificationFilter(NotificationFilterBase filter)
        {
            if (filter != null && !_notificationFilters.Contains(filter))
            {
                _notificationFilters.Add(filter);
            }
        }
        
        /// <summary>
        /// 移除通知过滤器
        /// </summary>
        public void RemoveNotificationFilter(NotificationFilterBase filter)
        {
            _notificationFilters.Remove(filter);
        }
        
        /// <summary>
        /// 清空所有通知
        /// </summary>
        public void ClearAllNotifications()
        {
            // 清空队列
            _notificationQueue.Clear();
            
            // 隐藏所有活动通知
            var activeNotificationsCopy = new Array<AchievementNotification>(_activeNotifications);
            foreach (var notification in activeNotificationsCopy)
            {
                var notificationUI = FindNotificationUI(notification);
                if (notificationUI != null)
                {
                    HideNotification(notificationUI, notification);
                }
            }
            
            EmitSignal(SignalName.NotificationQueueUpdated, 0);
            GD.Print("[AchievementNotificationSystem] 清空所有通知");
        }
        
        /// <summary>
        /// 查找通知UI
        /// </summary>
        private Control FindNotificationUI(AchievementNotification notification)
        {
            if (NotificationContainer == null)
                return null;
            
            // TODO: 实现通知UI查找逻辑
            // 这里需要根据实际的UI结构来实现
            return null;
        }
        
        /// <summary>
        /// 获取通知统计信息
        /// </summary>
        public NotificationStatistics GetNotificationStatistics()
        {
            return _statistics;
        }
        
        /// <summary>
        /// 获取通知历史
        /// </summary>
        public Array<AchievementNotification> GetNotificationHistory()
        {
            return new Array<AchievementNotification>(_notificationHistory);
        }
        
        /// <summary>
        /// 获取活动通知
        /// </summary>
        public Array<AchievementNotification> GetActiveNotifications()
        {
            return new Array<AchievementNotification>(_activeNotifications);
        }
        
        /// <summary>
        /// 获取队列中的通知
        /// </summary>
        public Array<AchievementNotification> GetQueuedNotifications()
        {
            return new Array<AchievementNotification>(_notificationQueue);
        }
    }
    
    /// <summary>
    /// 通知系统接口
    /// </summary>
    public interface INotificationSystem
    {
        void ShowNotification(AchievementNotification notification);
        void RegisterNotificationHandler(NotificationType type, NotificationHandlerBase handler);
        void AddNotificationFilter(NotificationFilterBase filter);
        void RemoveNotificationFilter(NotificationFilterBase filter);
        void ClearAllNotifications();
    }
    
    /// <summary>
    /// 通知处理器接口
    /// </summary>
    public interface INotificationHandler
    {
        void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification);
    }
    
    /// <summary>
    /// 通知过滤器接口
    /// </summary>
    public interface INotificationFilter
    {
        bool ShouldShow(AchievementNotification notification);
    }
    
    /// <summary>
    /// 通知统计信息
    /// </summary>
    public partial class NotificationStatistics : RefCounted
    {
        private Dictionary<NotificationType, int> _queuedCounts = new Dictionary<NotificationType, int>();
        private Dictionary<NotificationType, int> _shownCounts = new Dictionary<NotificationType, int>();
        
        public void IncrementQueuedCount(NotificationType type)
        {
            if (!_queuedCounts.ContainsKey(type))
                _queuedCounts[type] = 0;
            _queuedCounts[type]++;
        }
        
        public void IncrementShownCount(NotificationType type)
        {
            if (!_shownCounts.ContainsKey(type))
                _shownCounts[type] = 0;
            _shownCounts[type]++;
        }
        
        public int GetQueuedCount(NotificationType type)
        {
            return _queuedCounts.ContainsKey(type) ? _queuedCounts[type] : 0;
        }
        
        public int GetShownCount(NotificationType type)
        {
            return _shownCounts.ContainsKey(type) ? _shownCounts[type] : 0;
        }
        
        public int GetTotalQueuedCount()
        {
            return _queuedCounts.Values.Sum();
        }
        
        public int GetTotalShownCount()
        {
            return _shownCounts.Values.Sum();
        }
    }
    
    // 默认通知处理器实现
    public partial class AchievementCompletedHandler : NotificationHandlerBase
    {
        public override void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification)
        {
            if (!ValidateNotificationUI(notificationUI) || !ValidateNotification(notification))
            {
                return;
            }
            
            // 使用基类的通用配置
            ConfigureBasicNotificationUI(notificationUI, notification);
            
            // 成就完成特定的配置
            ConfigureAchievementSpecificUI(notificationUI, notification);
        }
        
        private void ConfigureAchievementSpecificUI(Control notificationUI, AchievementNotification notification)
        {
            // 可以添加成就完成特有的UI配置
            // 例如：特殊动画、音效、样式等
        }
    }
    
    public partial class AchievementUnlockedHandler : NotificationHandlerBase
    {
        public override void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification)
        {
            if (!ValidateNotificationUI(notificationUI) || !ValidateNotification(notification))
            {
                return;
            }
            
            // 使用基类的通用配置
            ConfigureBasicNotificationUI(notificationUI, notification);
            
            // 成就解锁特定的配置
            ConfigureUnlockSpecificUI(notificationUI, notification);
        }
        
        private void ConfigureUnlockSpecificUI(Control notificationUI, AchievementNotification notification)
        {
            // 可以添加成就解锁特有的UI配置
            // 例如：解锁动画、特殊图标等
        }
    }
    
    public partial class ProgressUpdateHandler : NotificationHandlerBase
    {
        public override void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification)
        {
            if (!ValidateNotificationUI(notificationUI) || !ValidateNotification(notification))
            {
                return;
            }
            
            // 使用基类的通用配置
            ConfigureBasicNotificationUI(notificationUI, notification);
            
            // 进度更新特定的配置
            ConfigureProgressSpecificUI(notificationUI, notification);
        }
        
        private void ConfigureProgressSpecificUI(Control notificationUI, AchievementNotification notification)
        {
            // 可以添加进度更新特有的UI配置
            // 例如：进度条、百分比显示等
        }
    }
    
    public partial class RewardReceivedHandler : NotificationHandlerBase
    {
        public override void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification)
        {
            if (!ValidateNotificationUI(notificationUI) || !ValidateNotification(notification))
            {
                return;
            }
            
            // 使用基类的通用配置
            ConfigureBasicNotificationUI(notificationUI, notification);
            
            // 奖励获得特定的配置
            ConfigureRewardSpecificUI(notificationUI, notification);
        }
        
        private void ConfigureRewardSpecificUI(Control notificationUI, AchievementNotification notification)
        {
            // 可以添加奖励获得特有的UI配置
            // 例如：奖励图标、数量显示等
        }
    }
}