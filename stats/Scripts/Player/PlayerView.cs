using Godot;
using CodeRogue.Components;

namespace CodeRogue.Player
{
    public partial class PlayerView : Node2D, IHealthDisplay
    {
        [Export] public Sprite2D _sprite;
        [Export] public ProgressBar _healthBar;
        [Export] public Label _healthLabel;
        [Export] public Label _levelLabel;
        [Export] public ProgressBar _experienceBar;
        
        private PlayerModel _model;
        
        public override void _Ready()
        {
            SetupVisualComponents();
        }
        
        private void SetupVisualComponents()
        {
            // 如果需要动态创建UI组件，在这里实现
            // 当前假设组件已在场景中设置
        }
        
        public void UpdateView(PlayerModel model)
        {
            _model = model;
            
            UpdateHealthDisplay(model.CurrentHealth, model.MaxHealth);
            UpdateLevelDisplay(model.Level);
            UpdateExperienceDisplay(model.Experience, model.ExperienceToNextLevel);
            
            Position = model.Position;
        }
        
        public void UpdateHealthDisplay(int currentHealth, int maxHealth)
        {
            if (_healthBar != null)
            {
                _healthBar.Value = (float)currentHealth / maxHealth * 100;
                _healthBar.MaxValue = 100;
            }
            
            if (_healthLabel != null)
            {
                _healthLabel.Text = $"{currentHealth}/{maxHealth}";
            }
        }
        
        private void UpdateLevelDisplay(int level)
        {
            if (_levelLabel != null)
            {
                _levelLabel.Text = $"Level {level}";
            }
        }
        
        private void UpdateExperienceDisplay(int currentExp, int expToNext)
        {
            if (_experienceBar != null)
            {
                _experienceBar.Value = (float)currentExp / expToNext * 100;
                _experienceBar.MaxValue = 100;
            }
        }
        
        public void PlayDamageEffect()
        {
            if (_sprite != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(_sprite, "modulate", Colors.Red, 0.1f);
                tween.TweenProperty(_sprite, "modulate", Colors.White, 0.1f);
            }
        }
        
        public void PlayHealEffect()
        {
            if (_sprite != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(_sprite, "modulate", Colors.Green, 0.1f);
                tween.TweenProperty(_sprite, "modulate", Colors.White, 0.1f);
            }
        }
        
        public void PlayLevelUpEffect()
        {
            // 升级特效
            var tween = CreateTween();
            tween.TweenProperty(this, "scale", Vector2.One * 1.2f, 0.2f);
            tween.TweenProperty(this, "scale", Vector2.One, 0.2f);
            
            if (_sprite != null)
            {
                var colorTween = CreateTween();
                colorTween.TweenProperty(_sprite, "modulate", Colors.Gold, 0.3f);
                colorTween.TweenProperty(_sprite, "modulate", Colors.White, 0.3f);
            }
        }
        
        public void PlayDeathEffect()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
        }
        
        public void SetVisible(bool visible)
        {
            Visible = visible;
        }
    }
}