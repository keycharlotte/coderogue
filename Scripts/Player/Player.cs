using Godot;
using CodeRogue.Core;
using CodeRogue.Data;
using CodeRogue.Components;

namespace CodeRogue.Player
{
    /// <summary>
    /// 玩家控制器 - 处理玩家输入和移动
    /// </summary>
    public partial class Player : CharacterBody2D
    {
        [Signal]
        public delegate void HealthChangedEventHandler(int newHealth, int maxHealth);
        
        [Signal]
        public delegate void PlayerDiedEventHandler();
        
        [Export]
        public float Speed { get; set; } = 300.0f;
        
        [Export]
        public int MaxHealth { get; set; } = 100;
        
        private int _currentHealth;
        private HealthComponent _healthComponent;
        private AnimatedSprite2D _animatedSprite;
        private CollisionShape2D _collisionShape;
        
        public int CurrentHealth 
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
                
                if (_currentHealth <= 0)
                {
                    Die();
                }
            }
        }
        
        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            _healthComponent = GetNode<HealthComponent>("HealthComponent");
            
            CurrentHealth = MaxHealth;
            
            // 连接健康组件信号
            if (_healthComponent != null)
            {
                _healthComponent.HealthChanged += OnHealthChanged;
                _healthComponent.Died += OnDied;
            }
        }
        
        public override void _PhysicsProcess(double delta)
        {
            HandleInput();
            MoveAndSlide();
            UpdateAnimation();
        }
        
        private void HandleInput()
        {
            Vector2 inputVector = Vector2.Zero;
            
            if (Input.IsActionPressed("move_left"))
                inputVector.X -= 1;
            if (Input.IsActionPressed("move_right"))
                inputVector.X += 1;
            if (Input.IsActionPressed("move_up"))
                inputVector.Y -= 1;
            if (Input.IsActionPressed("move_down"))
                inputVector.Y += 1;
            
            Velocity = inputVector.Normalized() * Speed;
        }
        
        private void UpdateAnimation()
        {
            if (_animatedSprite == null) return;
            
            if (Velocity.Length() > 0)
            {
                _animatedSprite.Play("walk");
                
                // 翻转精灵面向移动方向
                if (Velocity.X < 0)
                    _animatedSprite.FlipH = true;
                else if (Velocity.X > 0)
                    _animatedSprite.FlipH = false;
            }
            else
            {
                _animatedSprite.Play("idle");
            }
        }
        
        public void TakeDamage(int damage)
        {
            _healthComponent?.TakeDamage(damage);
        }
        
        public void Heal(int amount)
        {
            _healthComponent?.Heal(amount);
        }
        
        private void OnHealthChanged(int newHealth, int maxHealth)
        {
            CurrentHealth = newHealth;
        }
        
        private void OnDied()
        {
            Die();
        }
        
        private void Die()
        {
            EmitSignal(SignalName.PlayerDied);
            GameManager.Instance?.GameOver();
            
            // 播放死亡动画
            if (_animatedSprite != null)
            {
                _animatedSprite.Play("death");
            }
            
            // 禁用碰撞
            if (_collisionShape != null)
            {
                _collisionShape.SetDeferred("disabled", true);
            }
        }
    }
}