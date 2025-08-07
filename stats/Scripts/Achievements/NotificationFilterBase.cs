using Godot;
using CodeRogue.Achievements.Data;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 通知过滤器抽象基类
    /// 用于替代INotificationFilter接口，解决Godot Variant兼容性问题
    /// </summary>
    public abstract partial class NotificationFilterBase : RefCounted
    {
        /// <summary>
        /// 判断是否应该显示通知
        /// </summary>
        /// <param name="notification">通知数据</param>
        /// <returns>是否应该显示</returns>
        public abstract bool ShouldShow(AchievementNotification notification);
        
        /// <summary>
        /// 获取过滤器类型名称
        /// </summary>
        /// <returns>过滤器类型名称</returns>
        public virtual string GetFilterTypeName()
        {
            return GetType().Name;
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
                LogError("通知数据为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(notification.AchievementId))
            {
                LogError("成就ID为空");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">错误信息</param>
        protected virtual void LogError(string message)
        {
            GD.PrintErr($"[{GetFilterTypeName()}] {message}");
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">信息内容</param>
        protected virtual void LogInfo(string message)
        {
            GD.Print($"[{GetFilterTypeName()}] {message}");
        }
    }
}