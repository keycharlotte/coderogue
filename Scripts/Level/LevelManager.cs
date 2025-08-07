using Godot;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Player;
using CodeRogue.Core;
using CodeRogue.Utils;

namespace CodeRogue.Level
{
	public partial class LevelManager : Node2D
{

	
	[Export] public int CurrentLevel { get; set; } = 1;
	[Export] public float SpawnRadius { get; set; } = 200.0f;
	[Export] public float MinSpawnDistance { get; set; } = 100.0f;
	[Export] public int MaxEnemiesOnScreen { get; set; } = 10;
	[Export] public PackedScene PlayerScene { get; set; }
	
	private PlayerController _player;
	private List<EnemyController> _activeEnemies = new List<EnemyController>();
	private Timer _spawnTimer;
	private Timer _waveTimer;
	private RandomNumberGenerator _random = new RandomNumberGenerator();
	
	// 关卡配置
	private LevelConfig _currentLevelConfig;
	private int _currentWave = 0;
	private int _enemiesSpawnedInWave = 0;
	private bool _waveInProgress = false;
	private bool _levelActive = false;
	
	// 信号
	[Signal] public delegate void LevelStartedEventHandler(int level);
	[Signal] public delegate void LevelCompletedEventHandler(int level);
	[Signal] public delegate void WaveStartedEventHandler(int wave);
	[Signal] public delegate void WaveCompletedEventHandler(int wave);
	[Signal] public delegate void EnemySpawnedEventHandler(EnemyController enemy);
	[Signal] public delegate void EnemyDefeatedEventHandler(EnemyController enemy);
	[Signal] public delegate void PlayerCreatedEventHandler(PlayerController player);
	
	public override void _Ready()
	{
		_random.Randomize();
		SetupTimers();
	}
	
	/// <summary>
	/// 初始化关卡（由GameManager调用）
	/// </summary>
	public void InitializeLevel()
	{
		LoadLevelConfig();
		CreatePlayer(); // 在初始化时创建玩家
		GD.Print("LevelManager initialized by GameManager - Player created");
	}
	
	private void CreatePlayer()
	{
		// 如果没有设置玩家场景，使用默认路径
		if (PlayerScene == null)
		{
			PlayerScene = GD.Load<PackedScene>("res://ScenesPlayer/Player.tscn");
		}
		
		if (PlayerScene == null)
		{
			GD.PrintErr("LevelManager: Player scene not found!");
			return;
		}
		
		_player = PlayerScene.Instantiate<PlayerController>();
		
		// 设置玩家初始位置（可以从关卡配置中获取）
		_player.GlobalPosition = GetPlayerSpawnPosition();
		
		// 将玩家添加到场景
		GetParent().AddChild(_player);
		
		// 连接玩家信号
		_player.Connect(PlayerController.SignalName.PlayerDied, new Callable(this, nameof(OnPlayerDied)));
		
		// 将玩家添加到组中，方便其他系统查找
		_player.AddToGroup("player");
		
		EmitSignal(SignalName.PlayerCreated, _player);
		
		GD.Print("Player created and added to scene");
	}
	
	private Vector2 GetPlayerSpawnPosition()
	{
		// 获取视口大小并返回屏幕中心点
		var viewport = GetViewport();
		if (viewport != null)
		{
			return viewport.GetVisibleRect().Size / 2;
		}
		return Vector2.Zero;
	}
	
	private void SetupTimers()
	{
		// 获取场景中预配置的Timer节点
		_spawnTimer = GetNode<Timer>("SpawnTimer");
		_spawnTimer.WaitTime = 2.0f;
		_spawnTimer.Timeout += SpawnEnemy;
		
		_waveTimer = GetNode<Timer>("WaveTimer");
		_waveTimer.WaitTime = 30.0f;
		_waveTimer.OneShot = true;
		_waveTimer.Timeout += StartNextWave;
	}
	
	private void LoadLevelConfig()
	{
		_currentLevelConfig = new LevelConfig(CurrentLevel);
		GD.Print($"Loaded config for Level {CurrentLevel}");
	}
	
	public void StartLevel()
	{
		if (!_levelActive)
		{
			_levelActive = true;
			
			// 确保已初始化
			if (_currentLevelConfig == null)
			{
				InitializeLevel();
			}
			
			// 确保玩家已创建
			if (_player == null)
			{
				GD.PrintErr("Player not created during initialization!");
				return;
			}
			
			// 只开始刷怪流程
			StartNextWave();
			EmitSignal(SignalName.LevelStarted, CurrentLevel);
			var gameManager = NodeUtils.GetGameManager(this);
			gameManager?.TriggerStartLevel(CurrentLevel);
			GD.Print($"Level {CurrentLevel} started - Wave spawning begins");
		}
	}
	
	private void StartNextWave()
	{
		if (!_levelActive) return;
		
		_currentWave++;
		_enemiesSpawnedInWave = 0;
		_waveInProgress = true;
		
		var waveConfig = _currentLevelConfig.GetWaveConfig(_currentWave);
		if (waveConfig == null)
		{
			// 关卡完成
			CompleteLevel();
			return;
		}
		
		GD.Print($"Starting Wave {_currentWave}");
		EmitSignal(SignalName.WaveStarted, _currentWave);
		
		// 设置生成间隔
		_spawnTimer.WaitTime = waveConfig.SpawnInterval;
		_spawnTimer.Start();
		
		// 设置波次持续时间
		if (waveConfig.Duration > 0)
		{
			_waveTimer.WaitTime = waveConfig.Duration;
			_waveTimer.Start();
		}
	}
	
	private void SpawnEnemy()
	{
		if (!_waveInProgress || _player == null || !_levelActive)
			return;
			
		var waveConfig = _currentLevelConfig.GetWaveConfig(_currentWave);
		if (waveConfig == null)
			return;
			
		// 检查是否达到本波次敌人数量上限
		if (_enemiesSpawnedInWave >= waveConfig.MaxEnemies)
		{
			_spawnTimer.Stop();
			CheckWaveCompletion();
			return;
		}
		
		// 检查屏幕上敌人数量
		CleanupDeadEnemies();
		if (_activeEnemies.Count >= MaxEnemiesOnScreen)
			return;
			
		// 选择敌人类型
		int enemyId = SelectEnemyType(waveConfig);
		Vector2 spawnPosition = GetSpawnPosition();
		
		if (spawnPosition != Vector2.Zero)
		{
			CreateEnemy(enemyId, spawnPosition);
			_enemiesSpawnedInWave++;
		}
	}
	
	private int SelectEnemyType(WaveConfig waveConfig)
	{
		float totalWeight = waveConfig.EnemyTypes.Values.Sum();
		float randomValue = _random.RandfRange(0, totalWeight);
		
		float currentWeight = 0;
		foreach (var kvp in waveConfig.EnemyTypes)
		{
			currentWeight += kvp.Value;
			if (randomValue <= currentWeight)
			{
				return kvp.Key;
			}
		}
		
		// 默认返回第一个敌人类型
		return waveConfig.EnemyTypes.Keys.First();
	}
	
	private Vector2 GetSpawnPosition()
	{
		if (_player == null)
			return Vector2.Zero;
			
		Vector2 playerPos = _player.GlobalPosition;
		
		// 尝试多次找到合适的生成位置
		for (int i = 0; i < 10; i++)
		{
			float angle = _random.RandfRange(0, Mathf.Tau);
			float distance = _random.RandfRange(MinSpawnDistance, SpawnRadius);
			
			Vector2 spawnPos = playerPos + new Vector2(
				Mathf.Cos(angle) * distance,
				Mathf.Sin(angle) * distance
			);
			
			if (IsValidSpawnPosition(spawnPos))
			{
				return spawnPos;
			}
		}
		
		return Vector2.Zero;
	}
	
	private bool IsValidSpawnPosition(Vector2 position)
	{
		if (_player != null && position.DistanceTo(_player.GlobalPosition) < MinSpawnDistance)
		{
			return false;
		}
		
		return true;
	}
	
	private void CreateEnemy(int enemyId, Vector2 position)
	{
		var enemyScene = GD.Load<PackedScene>("res://ScenesEnemy/Enemy.tscn");
		if (enemyScene == null)
		{
			GD.PrintErr("LevelManager: Enemy scene not found!");
			return;
		}
		
		var enemy = enemyScene.Instantiate<EnemyController>();
		enemy.EnemyId = enemyId;
		enemy.GlobalPosition = position;
		
		// 添加到场景
		GetParent().AddChild(enemy);
		_activeEnemies.Add(enemy);
		
		// 连接敌人死亡信号 - 修正信号名称
		enemy.Connect(EnemyController.SignalName.EnemyDied, new Callable(this, nameof(OnEnemyDied)));
		
		EmitSignal(SignalName.EnemySpawned, enemy);
		
		GD.Print($"Spawned enemy {enemyId} at {position}");
	}
	
	private void OnEnemyDied(EnemyController enemy)
	{
		_activeEnemies.Remove(enemy);
		EmitSignal(SignalName.EnemyDefeated, enemy);
		
		CheckWaveCompletion();
	}
	
	private void OnPlayerDied()
	{
		_levelActive = false;
		_spawnTimer.Stop();
		_waveTimer.Stop();
		
		GD.Print("Player died - Level stopped");
		
		// 通知GameManager玩家死亡
		// var gameManager = GameManager.Instance;
		// if (gameManager != null)
		// {
		// 	gameManager.OnPlayerDied();
		// }
	}
	
	private void CleanupDeadEnemies()
	{
		_activeEnemies.RemoveAll(enemy => enemy == null || !IsInstanceValid(enemy));
	}
	
	private void CheckWaveCompletion()
	{
		var waveConfig = _currentLevelConfig.GetWaveConfig(_currentWave);
		if (waveConfig == null)
			return;
			
		// 检查是否所有敌人都已死亡
		if (_enemiesSpawnedInWave >= waveConfig.MaxEnemies && _activeEnemies.Count == 0)
		{
			CompleteWave();
		}
	}
	
	private void CompleteWave()
	{
		_waveInProgress = false;
		_spawnTimer.Stop();
		_waveTimer.Stop();
		
		GD.Print($"Wave {_currentWave} completed!");
		EmitSignal(SignalName.WaveCompleted, _currentWave);
		
		// 等待一段时间后开始下一波
		GetTree().CreateTimer(3.0f).Timeout += StartNextWave;
	}
	
	private void CompleteLevel()
	{
		_levelActive = false;
		_spawnTimer.Stop();
		_waveTimer.Stop();
		
		GD.Print($"Level {CurrentLevel} completed!");
		EmitSignal(SignalName.LevelCompleted, CurrentLevel);
		
		// 通知GameManager关卡完成
		// var gameManager = GameManager.Instance;
		// if (gameManager != null)
		// {
		// 	gameManager.OnLevelCompleted(CurrentLevel);
		// }
	}
	
	public void NextLevel()
	{
		CurrentLevel++;
		LoadLevelConfig();
		StartLevel();
	}
	
	public void RestartLevel()
	{
		// 清理所有敌人
		foreach (var enemy in _activeEnemies)
		{
			if (IsInstanceValid(enemy))
			{
				enemy.QueueFree();
			}
		}
		_activeEnemies.Clear();
		
		// 重置玩家状态
		if (_player != null)
		{
			_player.GlobalPosition = GetPlayerSpawnPosition();
			// 重置玩家血量等状态
		}
		
		// 重新开始关卡
		StartLevel();
	}
	
	/// <summary>
	/// 停止关卡（由GameManager调用）
	/// </summary>
	public void StopLevel()
	{
		if (_levelActive)
		{
			_levelActive = false;
			_waveInProgress = false;
			
			// 停止计时器
			_spawnTimer?.Stop();
			_waveTimer?.Stop();
			
			// 清理敌人
			ClearAllEnemies();
			
			// 移除玩家
			if (_player != null)
			{
				_player.QueueFree();
				_player = null;
			}
			
			var gameManager = NodeUtils.GetGameManager(this);
			gameManager?.TriggerQuitLevel(CurrentLevel);
			GD.Print($"Level {CurrentLevel} stopped");
		}
	}
	/// <summary>
	/// 清理所有敌人
	/// </summary>
	private void ClearAllEnemies()
	{
		foreach (var enemy in _activeEnemies.ToList())
		{
			if (enemy != null && IsInstanceValid(enemy))
			{
				enemy.QueueFree();
			}
		}
		_activeEnemies.Clear();
	}
	// 公共接口
	public PlayerController GetPlayer() => _player;
	public int GetActiveEnemyCount()
	{
		CleanupDeadEnemies();
		return _activeEnemies.Count;
	}
	public int GetCurrentWave() => _currentWave;
	public bool IsWaveInProgress() => _waveInProgress;
	public bool IsLevelActive() => _levelActive;
	
	// 重写_ExitTree以确保清理
	public override void _ExitTree()
	{
		StopLevel();
		base._ExitTree();
	}
	}
}
