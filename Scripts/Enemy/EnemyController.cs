using Godot;
using CodeRogue.Components;
using CodeRogue.Core;

public partial class EnemyController : CharacterBody2D
{
	[Export] public int EnemyId { get; set; } = 1;
	
	private EnemyModel _model;
	private EnemyView _view;
	private Node2D _player;
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
		InitializeEnemy();
		SetupTimers();
		FindPlayer();
		_spawnPosition = GlobalPosition;
		_patrolTarget = _spawnPosition;
		
		// 注册到输入管理器
		if (CodeRogue.Core.InputManager.Instance != null)
		{
			CodeRogue.Core.InputManager.Instance.RegisterEnemy(this);
			CodeRogue.Core.InputManager.Instance.WordMatched += OnWordMatched;
		}
	}
	
	public override void _ExitTree()
	{
		// 从输入管理器注销
		if (CodeRogue.Core.InputManager.Instance != null)
		{
			CodeRogue.Core.InputManager.Instance.UnregisterEnemy(this);
			CodeRogue.Core.InputManager.Instance.WordMatched -= OnWordMatched;
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
		_view = new EnemyView();
		AddChild(_view);
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
		_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
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
				if (GlobalPosition.DistanceTo(_patrolTarget) < 10.0f)
				{
					GenerateNewPatrolTarget();
				}
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
			MoveAndSlide();
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
			distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);
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
		var gameManager = GetNode<GameManager>("/root/GameManager");
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
}
