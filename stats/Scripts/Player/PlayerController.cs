using Godot;
using CodeRogue.Core;
using CodeRogue.Data;
using CodeRogue.Components;
using CodeRogue.Utils;

namespace CodeRogue.Player
{
    /// <summary>
    /// 玩家控制器 - 处理玩家输入和游戏逻辑
    /// </summary>
    public partial class PlayerController : CharacterBody2D
    {
        [Signal]
        public delegate void HealthChangedEventHandler(int newHealth, int maxHealth);
        
        [Signal]
        public delegate void PlayerDiedEventHandler();
        
        [Signal]
        public delegate void LevelUpEventHandler(int newLevel);
        
        [Export] public PlayerView _view;
        
        private PlayerModel _model;
        [Export] public HealthComponent _healthComponent;
        // [Export] public AnimatedSprite2D _animatedSprite;

        // [Export] public Sprite2D _sprite;
        [Export] public CollisionShape2D _collisionShape;
        
        public bool IsAlive => _model != null && _model.IsAlive;
        
        public PlayerModel GetPlayerModel()
        {
            return _model;
        }
        
        public override void _Ready()
        {
            // Player忽略Enemy层的碰撞
            CollisionLayer = 1;      // Player层
            CollisionMask = 4;       // 只检测Environment层
            
            InitializePlayer();
            SetupComponents();
        }
        
        private void InitializePlayer()
        {
            _model = new PlayerModel();
            _model.Position = GlobalPosition;
        }
        
        private void SetupComponents()
        {
            // _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            // _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            // _healthComponent = GetNode<HealthComponent>("HealthComponent");
            // _view = GetNode<PlayerView>("PlayerView");
            
            // 设置健康组件的初始值
            if (_healthComponent != null)
            {
                _healthComponent.SetMaxHealth(_model.MaxHealth);
                _healthComponent.HealthChanged += OnHealthChanged;
                _healthComponent.Died += OnDied;
            }
            
            // 初始化视图
            if (_view != null)
            {
                _view.UpdateView(_model);
            }
        }
        
        public override void _PhysicsProcess(double delta)
        {
            HandleInput();
            MoveAndSlide();
            UpdateAnimation();
            UpdateModel();
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
            
            Velocity = inputVector.Normalized() * _model.Speed;
        }
        
        private void UpdateAnimation()
        {
            // if (_animatedSprite == null) return;
            
            // if (Velocity.Length() > 0)
            // {
            //     _animatedSprite.Play("walk");
                
            //     // 翻转精灵面向移动方向
            //     if (Velocity.X < 0)
            //         _animatedSprite.FlipH = true;
            //     else if (Velocity.X > 0)
            //         _animatedSprite.FlipH = false;
            // }
            // else
            // {
            //     _animatedSprite.Play("idle");
            // }
        }
        
        private void UpdateModel()
        {
            if (_model != null)
            {
                _model.Position = GlobalPosition;
            }
        }
        
        public void TakeDamage(int damage)
        {
            _healthComponent?.TakeDamage(damage);
            _view?.PlayDamageEffect();
        }
        
        public void Heal(int amount)
        {
            _healthComponent?.Heal(amount);
            _view?.PlayHealEffect();
        }
        
        public void GainExperience(int exp)
        {
            int oldLevel = _model.Level;
            _model.GainExperience(exp);
            
            if (_model.Level > oldLevel)
            {
                // 升级了，更新健康组件
                _healthComponent?.SetMaxHealth(_model.MaxHealth);
                EmitSignal(SignalName.LevelUp, _model.Level);
                _view?.PlayLevelUpEffect();
            }
            
            _view?.UpdateView(_model);
        }
        
        private void OnHealthChanged(int newHealth, int maxHealth)
        {
            _model.CurrentHealth = newHealth;
            EmitSignal(SignalName.HealthChanged, newHealth, maxHealth);
            _view?.UpdateHealthDisplay(newHealth, maxHealth);
        }
        
        private void OnDied()
        {
            Die();
        }
        
        private void Die()
        {
            _model.IsAlive = false;
            EmitSignal(SignalName.PlayerDied);
            var gameManager = NodeUtils.GetGameManager(this);
		gameManager?.GameOver();
            
            // 播放死亡动画
            // if (_animatedSprite != null)
            // {
            //     _animatedSprite.Play("death");
            // }
            
            // 禁用碰撞
            if (_collisionShape != null)
            {
                _collisionShape.SetDeferred("disabled", true);
            }
            
            _view?.PlayDeathEffect();
        }
    }
}