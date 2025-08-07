using Godot;
using CodeRogue.Components;
using CodeRogue.Core;
using CodeRogue.Utils;
using CodeRogue.Player;
public partial class EnemyController : CharacterBody2D
{
	[Export] public int EnemyId { get; set; } = 1;
	
	// 添加CurrentWord属性
	public string CurrentWord { get; private set; } = "";
	
	private EnemyModel _model;
	[Export] public EnemyView _view;
	private PlayerController _player;
	private Timer _attackTimer;
	private Timer _aiTimer;
	
	// AI状态
	public enum EnemyState
	{
		Idle,
		Patrol,
		Chase,
		Attack,
		Dead
	}
	
	private EnemyState _currentState = EnemyState.Idle;
	private Vector2 _patrolTarget;
	private Vector2 _spawnPosition;
	
	public bool IsAlive => _model != null && _model.IsAlive;
	
	public EnemyView GetEnemyView()
	{
		return _view;
	}
	
	public override void _Ready()
	{
		// Enemy可以检测Player和环境
		CollisionLayer = 2;      // Enemy层
		CollisionMask = 5;       // 检测Player(1) + Environment(4) = 5
		
		InitializeEnemy();
		AssignRandomWord(); // 添加随机单词分配
		SetupTimers();
		FindPlayer();
		_spawnPosition = GlobalPosition;
		_patrolTarget = _spawnPosition;
		
		// 注册到输入管理器
		var inputManager = GetNode<InputManager>("/root/InputManager");
		if (inputManager != null)
		{
			inputManager.RegisterEnemy(this);
			inputManager.WordMatched += OnWordMatched;
		}
	}
	
	public override void _ExitTree()
	{
		// 从输入管理器注销
		var inputManager = GetNode<InputManager>("/root/InputManager");
		if (inputManager != null)
		{
			inputManager.UnregisterEnemy(this);
			inputManager.WordMatched -= OnWordMatched;
		}
	}
	
	private void OnWordMatched(string word, EnemyController targetEnemy)
	{
		if (targetEnemy == this)
		{
			// 这个敌人被攻击
			TakeDamage(25);
			_view?.OnWordMatched();
			ShowAttackEffect();
		}
	}

	// 删除这个重复的私有方法（第65-75行）
	// private void TakeDamage(int damage)
	// {
	//     if (_model != null && _model.IsAlive)
	//     {
	//         _model.CurrentHealth -= damage;
	//         _view?.UpdateHealthBar(_model.CurrentHealth, _model.MaxHealth);
	//         
	//         if (_model.CurrentHealth <= 0)
	//         {
	//             Die();
	//         }
	//     }
	// }

	// private void Die()
	// {
	//     _currentState = EnemyState.Dead;
	//     _model.IsAlive = false;
		
	//     // 播放死亡效果
	//     ShowDeathEffect();
		
	//     // 延迟移除
	//     var timer = GetTree().CreateTimer(1.0f);
	//     timer.Timeout += () => QueueFree();
	// }
	
	private void ShowAttackEffect()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", Colors.Red, 0.1f);
		tween.TweenProperty(this, "modulate", Colors.White, 0.1f);
	}
	
	private void ShowDeathEffect()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
	}
	
	private void InitializeEnemy()
	{
		// 初始化模型
		_model = new EnemyModel(EnemyId);
		_model.Position = GlobalPosition;
		
		// 初始化视图
		// _view = new EnemyView();
		// AddChild(_view);
		_view.UpdateView(_model);
	}
	
	private void SetupTimers()
	{
		// 攻击计时器
		_attackTimer = new Timer();
		_attackTimer.WaitTime = 1.0f;
		_attackTimer.OneShot = true;
		AddChild(_attackTimer);
		
		// AI计时器
		_aiTimer = new Timer();
		_aiTimer.WaitTime = 0.1f;
		_aiTimer.Timeout += UpdateAI;
		AddChild(_aiTimer);
		_aiTimer.Start();
	}
	
	private void FindPlayer()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
		if (_player == null)
		{
			GD.PrintErr("Player node not found or is not a Node2D!");
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (_model == null || !_model.IsAlive)
			return;
			
		HandleMovement(delta);
		
		// 更新模型位置
		_model.Position = GlobalPosition;
		_view?.UpdateView(_model);
	}
	
	private void HandleMovement(double delta)
	{
		Vector2 targetPosition = Vector2.Zero;
		
		switch (_currentState)
		{
			case EnemyState.Patrol:
				targetPosition = _patrolTarget;
				break;
				
			case EnemyState.Chase:
				if (_player != null)
				{
					targetPosition = _player.GlobalPosition;
				}
				break;
				
			case EnemyState.Attack:
				// 攻击时不移动
				return;
		}
		
		if (targetPosition != Vector2.Zero)
		{
			Vector2 direction = (targetPosition - GlobalPosition).Normalized();
			Velocity = direction * _model.MoveSpeed;
			Move();
		}
	}
	
	/// <summary>
	/// 处理敌人的具体移动逻辑，包括碰撞检测和位置更新
	/// </summary>
	private void Move()
	{
		// 使用Godot的内置移动函数处理碰撞
		MoveAndSlide();
		
		// 更新敌人的全局位置
		GlobalPosition = GlobalPosition;
		
		// 可选：添加移动边界检查
		ClampToMovementBounds();
		
		// 可选：更新朝向
		UpdateFacing();
	}
	
	/// <summary>
	/// 限制敌人在指定区域内移动
	/// </summary>
	private void ClampToMovementBounds()
	{
		// 获取屏幕边界或关卡边界
		var viewport = GetViewport();
		if (viewport != null)
		{
			var screenSize = viewport.GetVisibleRect().Size;
			var margin = 50.0f; // 边界边距
			
			// 限制在屏幕范围内
			GlobalPosition = new Vector2(
				Mathf.Clamp(GlobalPosition.X, margin, screenSize.X - margin),
				Mathf.Clamp(GlobalPosition.Y, margin, screenSize.Y - margin)
			);
		}
	}
	
	/// <summary>
	/// 根据移动方向更新敌人朝向
	/// </summary>
	private void UpdateFacing()
	{
		if (Velocity.LengthSquared() > 0.01f)
		{
			// 根据移动方向翻转精灵
			if (_view != null && _view._sprite != null)
			{
				_view._sprite.FlipH = Velocity.X < 0;
			}
		}
	}
	
	private void UpdateAI()
	{
		if (_model == null || !_model.IsAlive)
		{
			_currentState = EnemyState.Dead;
			return;
		}
		
		float distanceToPlayer = float.MaxValue;
		if (_player != null)
		{
			// 使用通用方法计算距离
			// distanceToPlayer = NodeUtils.GetWorldDistance(this, _player);
			
			// 如果需要屏幕坐标距离，可以使用：
			distanceToPlayer = NodeUtils.GetScreenDistance(this, _player);
		}
		
		switch (_currentState)
		{
			case EnemyState.Idle:
				if (distanceToPlayer <= _model.DetectionRange)
				{
					_currentState = EnemyState.Chase;
				}
				else
				{
					_currentState = EnemyState.Patrol;
					GenerateNewPatrolTarget();
				}
				break;
				
			case EnemyState.Patrol:
				if (distanceToPlayer <= _model.DetectionRange)
				{
					_currentState = EnemyState.Chase;
				}
				break;
				
			case EnemyState.Chase:
				if (distanceToPlayer <= _model.AttackRange)
				{
					_currentState = EnemyState.Attack;
					PerformAttack();
				}
				else if (distanceToPlayer > _model.DetectionRange * 1.5f)
				{
					_currentState = EnemyState.Patrol;
				}
				break;
				
			case EnemyState.Attack:
				if (distanceToPlayer > _model.AttackRange)
				{
					_currentState = EnemyState.Chase;
				}
				else if (_attackTimer.IsStopped())
				{
					PerformAttack();
				}
				break;
		}
		// GD.Print($"Enemy {Name} - Distance to player: {distanceToPlayer}");
		// GD.Print($"Enemy {Name} - Current state: {_currentState}");
	}
	
	private void GenerateNewPatrolTarget()
	{
		var random = new RandomNumberGenerator();
		random.Randomize();
		
		float angle = random.RandfRange(0, Mathf.Tau);
		float distance = random.RandfRange(20, 50);
		
		_patrolTarget = _spawnPosition + new Vector2(
			Mathf.Cos(angle) * distance,
			Mathf.Sin(angle) * distance
		);
	}
	
	private void PerformAttack()
	{
		if (_player == null || _attackTimer.TimeLeft > 0)
			return;
			
		// 执行攻击逻辑
		GD.Print($"{_model.Name} attacks for {_model.AttackPower} damage!");
		
		// 如果玩家有HealthComponent，造成伤害
		var playerHealth = _player.GetNode<HealthComponent>("HealthComponent");
		if (playerHealth != null)
		{
			playerHealth.TakeDamage(_model.AttackPower);
		}
		
		_attackTimer.Start();
	}
	
	public void TakeDamage(int damage)
	{
		if (_model == null || !_model.IsAlive)
			return;
			
		_model.TakeDamage(damage);
		_view?.PlayDamageEffect();
		
		if (!_model.IsAlive)
		{
			Die();
		}
		
		// 受到攻击时进入追击状态
		if (_currentState != EnemyState.Dead)
		{
			_currentState = EnemyState.Chase;
		}
	}
	
	
	// 在EnemyController类中添加信号
	[Signal] public delegate void EnemyDiedEventHandler(EnemyController enemy);
	
	// 在Die()方法中发射信号
	private void Die()
	{
		_currentState = EnemyState.Dead;
		_aiTimer?.Stop();
		_attackTimer?.Stop();
		
		// 播放死亡效果
		_view?.PlayDeathEffect();
		
		// 发射死亡信号
		EmitSignal(SignalName.EnemyDied, this);
		
		// 给予玩家经验
		var gameManager = NodeUtils.GetGameManager(this);
		if (gameManager != null)
		{
			// gameManager.AddExperience(_model.ExperienceReward);
		}
		_model.IsAlive = false;
		
	}
	
	public EnemyModel GetModel()
	{
		return _model;
	}
	
	public EnemyState GetCurrentState()
	{
		return _currentState;
	}
	
	/// <summary>
	/// 为敌人分配随机单词
	/// </summary>
	private void AssignRandomWord()
	{
		var wordManager = NodeUtils.GetWordManager(this);
		if (wordManager != null)
		{
			CurrentWord = wordManager.GetRandomWord();
			GD.Print($"Enemy {EnemyId} assigned word: {CurrentWord}");
			
			// 通知视图更新显示的单词
			_view.CurrentWord = CurrentWord;
		}
		else
		{
			GD.PrintErr("WordManager instance not found!");
			CurrentWord = "word"; // 默认单词
		}
	}
}
