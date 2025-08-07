using Godot;
using CodeRogue.Achievements.Data;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 通知处理器抽象基类
    /// 替代INotificationHandler接口以解决Godot Variant兼容性问题
    /// </summary>
    public abstract partial class NotificationHandlerBase : RefCounted
    {
        /// <summary>
        /// 配置通知UI
        /// </summary>
        /// <param name="notificationUI">通知UI控件</param>
        /// <param name="notification">通知数据</param>
        public abstract void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification);
        
        /// <summary>
        /// 配置基础通知UI（通用实现）
        /// </summary>
        /// <param name="notificationUI">通知UI控件</param>
        /// <param name="notification">通知数据</param>
        protected virtual void ConfigureBasicNotificationUI(Control notificationUI, AchievementNotification notification)
        {
            try
            {
                // 设置标题
                var titleLabel = notificationUI.GetNode<Label>("Title");
                if (titleLabel != null)
                {
                    titleLabel.Text = notification.Title;
                }
                
                // 设置内容
                var contentLabel = notificationUI.GetNode<Label>("Content");
                if (contentLabel != null)
                {
                    contentLabel.Text = notification.Message;
                }
                
                // 设置图标
                var iconTexture = notificationUI.GetNode<TextureRect>("Icon");
                if (iconTexture != null && !string.IsNullOrEmpty(notification.IconPath))
                {
                    var texture = GD.Load<Texture2D>(notification.IconPath);
                    if (texture != null)
                    {
                        iconTexture.Texture = texture;
                    }
                }
                
                // 设置优先级样式
                ApplyPriorityStyle(notificationUI, notification.Priority);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[NotificationHandlerBase] 配置基础UI时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 应用优先级样式
        /// </summary>
        /// <param name="notificationUI">通知UI控件</param>
        /// <param name="priority">优先级</param>
        protected virtual void ApplyPriorityStyle(Control notificationUI, int priority)
        {
            var styleClass = priority switch
            {
                <= 2 => "notification-low",
                <= 5 => "notification-normal",
                <= 8 => "notification-high",
                _ => "notification-critical"
            };
            
            // 添加样式类（如果支持）
            if (notificationUI.HasMethod("add_theme_stylebox_override"))
            {
                // 可以在这里添加主题样式覆盖
            }
        }
        
        /// <summary>
        /// 获取处理器类型名称
        /// </summary>
        /// <returns>处理器类型名称</returns>
        public virtual string GetHandlerTypeName()
        {
            return GetType().Name;
        }
        
        /// <summary>
        /// 验证通知UI是否有效
        /// </summary>
        /// <param name="notificationUI">通知UI控件</param>
        /// <returns>是否有效</returns>
        protected virtual bool ValidateNotificationUI(Control notificationUI)
        {
            if (notificationUI == null)
            {
                GD.PrintErr("[NotificationHandlerBase] 通知UI控件为空");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 验证通知数据是否有效
        /// </summary>
        /// <param name="notification">通知数据</param>
        /// <returns>是否有效</returns>
        protected virtual bool ValidateNotification(AchievementNotification notification)
        {
            if (notification == null)
            {
                GD.PrintErr("[NotificationHandlerBase] 通知数据为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(notification.Title))
            {
                GD.PrintErr("[NotificationHandlerBase] 通知标题为空");
                return false;
            }
            
            return true;
        }
    }
}