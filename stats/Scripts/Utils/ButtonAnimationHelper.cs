using Godot;

namespace CodeRogue.Utils
{
    /// <summary>
    /// 按钮动画辅助类
    /// </summary>
    public static class ButtonAnimationHelper
    {
        /// <summary>
        /// 为按钮添加悬停和离开动效
        /// </summary>
        /// <param name="button">要添加动效的按钮</param>
        /// <param name="hoverScale">悬停时的缩放比例，默认1.1</param>
        /// <param name="animationDuration">动画持续时间，默认0.1秒</param>
        public static void AddHoverAnimation(Button button, float hoverScale = 1.1f, float animationDuration = 0.1f)
        {
            if (button == null) return;
            
            button.MouseEntered += () => {
                var tween = button.CreateTween();
                tween.TweenProperty(button, "scale", Vector2.One * hoverScale, animationDuration);
            };
            
            button.MouseExited += () => {
                var tween = button.CreateTween();
                tween.TweenProperty(button, "scale", Vector2.One, animationDuration);
            };
        }
        
        /// <summary>
        /// 为多个按钮批量添加悬停动效
        /// </summary>
        /// <param name="buttons">按钮数组</param>
        /// <param name="hoverScale">悬停时的缩放比例</param>
        /// <param name="animationDuration">动画持续时间</param>
        public static void AddHoverAnimationToButtons(Button[] buttons, float hoverScale = 1.1f, float animationDuration = 0.1f)
        {
            foreach (var button in buttons)
            {
                AddHoverAnimation(button, hoverScale, animationDuration);
            }
        }
    }
}