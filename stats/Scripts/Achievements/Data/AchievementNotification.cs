using Godot;
using Godot.Collections;
using CodeRogue.Achievements;
using System;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 成就通知数据模型
    /// 继承自Resource，用于封装成就通知的相关信息
    /// </summary>
    [GlobalClass]
    public partial class AchievementNotification : Resource
    {
        /// <summary>通知ID</summary>
        [Export] public string Id { get; set; } = string.Empty;
        
        /// <summary>通知类型</summary>
        [Export] public NotificationType Type { get; set; } = NotificationType.AchievementCompleted;
        
        /// <summary>成就ID</summary>
        [Export] public string AchievementId { get; set; } = string.Empty;
        
        /// <summary>通知标题</summary>
        [Export] public string Title { get; set; } = string.Empty;
        
        /// <summary>通知内容</summary>
        [Export] public string Message { get; set; } = string.Empty;
        
        /// <summary>通知图标路径</summary>
        [Export] public string IconPath { get; set; } = string.Empty;
        
        /// <summary>显示时长（秒）</summary>
        [Export] public float Duration { get; set; } = 3.0f;
        
        /// <summary>优先级（数值越大优先级越高）</summary>
        [Export] public int Priority { get; set; } = 0;
        
        /// <summary>是否播放音效</summary>
        [Export] public bool PlaySound { get; set; } = true;
        
        /// <summary>音效路径</summary>
        [Export] public string SoundPath { get; set; } = string.Empty;
        
        /// <summary>是否显示动画</summary>
        [Export] public bool ShowAnimation { get; set; } = true;
        
        /// <summary>动画类型</summary>
        [Export] public string AnimationType { get; set; } = "slide_in";
        
        /// <summary>背景颜色</summary>
        [Export] public Color BackgroundColor { get; set; } = Colors.DarkBlue;
        
        /// <summary>文本颜色</summary>
        [Export] public Color TextColor { get; set; } = Colors.White;
        
        /// <summary>创建时间（ISO 8601格式字符串）</summary>
        [Export] public string CreatedAt { get; set; } = string.Empty;
        
        /// <summary>是否已显示</summary>
        [Export] public bool IsShown { get; set; } = false;
        
        /// <summary>额外数据字典</summary>
        [Export] public Dictionary<string, Variant> ExtraData { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public AchievementNotification()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow.ToString("O");
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">通知类型</param>
        /// <param name="achievementId">成就ID</param>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        public AchievementNotification(NotificationType type, string achievementId, string title, string message)
        {
            Id = Guid.NewGuid().ToString();
            Type = type;
            AchievementId = achievementId;
            Title = title;
            Message = message;
            CreatedAt = DateTime.UtcNow.ToString("O");
            
            // 根据通知类型设置默认属性
            SetDefaultPropertiesByType();
        }
        
        /// <summary>
        /// 根据通知类型设置默认属性
        /// </summary>
        private void SetDefaultPropertiesByType()
        {
            switch (Type)
            {
                case NotificationType.AchievementCompleted:
                    BackgroundColor = Colors.Gold;
                    TextColor = Colors.Black;
                    Duration = 4.0f;
                    Priority = 10;
                    SoundPath = "res://Audio/SFX/achievement_completed.ogg";
                    IconPath = "res://Icons/Notifications/achievement_completed.png";
                    break;
                    
                case NotificationType.AchievementUnlocked:
                    BackgroundColor = Colors.LightBlue;
                    TextColor = Colors.White;
                    Duration = 3.0f;
                    Priority = 5;
                    SoundPath = "res://Audio/SFX/achievement_unlocked.ogg";
                    IconPath = "res://Icons/Notifications/achievement_unlocked.png";
                    break;
                    
                case NotificationType.ProgressUpdated:
                    BackgroundColor = Colors.Green;
                    TextColor = Colors.White;
                    Duration = 2.0f;
                    Priority = 1;
                    PlaySound = false;
                    ShowAnimation = false;
                    IconPath = "res://Icons/Notifications/progress_updated.png";
                    break;
                    
                case NotificationType.RewardReceived:
                    BackgroundColor = Colors.Purple;
                    TextColor = Colors.White;
                    Duration = 3.5f;
                    Priority = 8;
                    SoundPath = "res://Audio/SFX/reward_received.ogg";
                    IconPath = "res://Icons/Notifications/reward_received.png";
                    break;
            }
        }
        
        /// <summary>
        /// 创建成就完成通知
        /// </summary>
        /// <param name="achievementConfig">成就配置</param>
        /// <returns>成就完成通知</returns>
        public static AchievementNotification CreateCompletedNotification(AchievementConfig achievementConfig)
        {
            var notification = new AchievementNotification(
                NotificationType.AchievementCompleted,
                achievementConfig.Id,
                "成就完成！",
                $"恭喜完成成就：{achievementConfig.Name}"
            );
            
            if (!string.IsNullOrEmpty(achievementConfig.IconPath))
            {
                notification.IconPath = achievementConfig.IconPath;
            }
            
            // 根据稀有度调整通知属性
            notification.AdjustByRarity(achievementConfig.Rarity);
            
            return notification;
        }
        
        /// <summary>
        /// 创建成就解锁通知
        /// </summary>
        /// <param name="achievementConfig">成就配置</param>
        /// <returns>成就解锁通知</returns>
        public static AchievementNotification CreateUnlockedNotification(AchievementConfig achievementConfig)
        {
            var notification = new AchievementNotification(
                NotificationType.AchievementUnlocked,
                achievementConfig.Id,
                "新成就解锁！",
                $"解锁新成就：{achievementConfig.Name}"
            );
            
            if (!string.IsNullOrEmpty(achievementConfig.IconPath))
            {
                notification.IconPath = achievementConfig.IconPath;
            }
            
            return notification;
        }
        
        /// <summary>
        /// 创建进度更新通知
        /// </summary>
        /// <param name="achievementConfig">成就配置</param>
        /// <param name="progress">成就进度</param>
        /// <returns>进度更新通知</returns>
        public static AchievementNotification CreateProgressNotification(AchievementConfig achievementConfig, AchievementProgress progress)
        {
            var percentage = progress.GetProgressPercentage();
            var notification = new AchievementNotification(
                NotificationType.ProgressUpdated,
                achievementConfig.Id,
                "成就进度更新",
                $"{achievementConfig.Name}: {progress.GetProgressText()} ({percentage:F0}%)"
            );
            
            return notification;
        }
        
        /// <summary>
        /// 创建奖励获得通知
        /// </summary>
        /// <param name="achievementConfig">成就配置</param>
        /// <param name="rewards">奖励列表</param>
        /// <returns>奖励获得通知</returns>
        public static AchievementNotification CreateRewardNotification(AchievementConfig achievementConfig, RewardConfig[] rewards)
        {
            var rewardText = string.Join(", ", System.Array.ConvertAll(rewards, r => r.GetDisplayName()));
            var notification = new AchievementNotification(
                NotificationType.RewardReceived,
                achievementConfig.Id,
                "获得奖励！",
                $"完成成就 {achievementConfig.Name} 获得：{rewardText}"
            );
            
            return notification;
        }
        
        /// <summary>
        /// 根据稀有度调整通知属性
        /// </summary>
        /// <param name="rarity">稀有度</param>
        private void AdjustByRarity(AchievementRarity rarity)
        {
            switch (rarity)
            {
                case AchievementRarity.Common:
                    Duration = 3.0f;
                    Priority = 5;
                    break;
                    
                case AchievementRarity.Rare:
                    Duration = 4.0f;
                    Priority = 10;
                    BackgroundColor = Colors.Blue;
                    break;
                    
                case AchievementRarity.Epic:
                    Duration = 5.0f;
                    Priority = 15;
                    BackgroundColor = Colors.Purple;
                    AnimationType = "bounce_in";
                    break;
                    
                case AchievementRarity.Legendary:
                    Duration = 6.0f;
                    Priority = 20;
                    BackgroundColor = Colors.Orange;
                    AnimationType = "flash_in";
                    break;
            }
        }
        
        /// <summary>
        /// 获取创建时间的DateTime对象
        /// </summary>
        /// <returns>创建时间</returns>
        public DateTime GetCreatedDateTime()
        {
            if (DateTime.TryParse(CreatedAt, out var dateTime))
                return dateTime;
                
            return DateTime.UtcNow;
        }
        
        /// <summary>
        /// 检查通知是否已过期
        /// </summary>
        /// <param name="maxAge">最大存活时间（秒）</param>
        /// <returns>是否已过期</returns>
        public bool IsExpired(float maxAge = 300f) // 默认5分钟过期
        {
            var age = (DateTime.UtcNow - GetCreatedDateTime()).TotalSeconds;
            return age > maxAge;
        }
        
        /// <summary>
        /// 标记通知为已显示
        /// </summary>
        public void MarkAsShown()
        {
            IsShown = true;
        }
        
        /// <summary>
        /// 获取通知的默认音效路径
        /// </summary>
        /// <returns>默认音效路径</returns>
        public string GetDefaultSoundPath()
        {
            if (!string.IsNullOrEmpty(SoundPath))
                return SoundPath;
                
            return Type switch
            {
                NotificationType.AchievementCompleted => "res://Audio/SFX/achievement_completed.ogg",
                NotificationType.AchievementUnlocked => "res://Audio/SFX/achievement_unlocked.ogg",
                NotificationType.ProgressUpdated => "res://Audio/SFX/progress_updated.ogg",
                NotificationType.RewardReceived => "res://Audio/SFX/reward_received.ogg",
                _ => "res://Audio/SFX/notification_default.ogg"
            };
        }
        
        /// <summary>
        /// 获取音效路径
        /// </summary>
        /// <returns>音效路径</returns>
        public string GetSoundPath()
        {
            return !string.IsNullOrEmpty(SoundPath) ? SoundPath : GetDefaultSoundPath();
        }
        
        /// <summary>
        /// 获取显示时长
        /// </summary>
        /// <returns>显示时长（秒）</returns>
        public float DisplayDuration => Duration;
        
        /// <summary>
        /// 创建通知的副本
        /// </summary>
        /// <returns>通知的副本</returns>
        public AchievementNotification Clone()
        {
            var clone = new AchievementNotification
            {
                Id = Id,
                Type = Type,
                AchievementId = AchievementId,
                Title = Title,
                Message = Message,
                IconPath = IconPath,
                Duration = Duration,
                Priority = Priority,
                PlaySound = PlaySound,
                SoundPath = SoundPath,
                ShowAnimation = ShowAnimation,
                AnimationType = AnimationType,
                BackgroundColor = BackgroundColor,
                TextColor = TextColor,
                CreatedAt = CreatedAt,
                IsShown = IsShown,
                ExtraData = new Dictionary<string, Variant>(ExtraData)
            };
            
            return clone;
        }
        
        /// <summary>
        /// 验证通知数据的有效性
        /// </summary>
        /// <returns>验证结果和错误信息</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrEmpty(Id))
                return (false, "通知ID不能为空");
                
            if (string.IsNullOrEmpty(AchievementId))
                return (false, "成就ID不能为空");
                
            if (string.IsNullOrEmpty(Title))
                return (false, "通知标题不能为空");
                
            if (Duration <= 0)
                return (false, "显示时长必须大于0");
                
            if (string.IsNullOrEmpty(CreatedAt))
                return (false, "创建时间不能为空");
                
            if (!DateTime.TryParse(CreatedAt, out _))
                return (false, "创建时间格式无效");
                
            return (true, string.Empty);
        }
    }
}