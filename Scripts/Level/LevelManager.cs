using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class LevelManager : Node
{
	[Export] public int CurrentLevel { get; set; } = 1;
	[Export] public float SpawnRadius { get; set; } = 200.0f;
	[Export] public float MinSpawnDistance { get; set; } = 100.0f;
	[Export] public int MaxEnemiesOnScreen { get; set; } = 10;
	
	private Node2D _player;
	private List<EnemyController> _activeEnemies = new List<EnemyController>();
	private Timer _spawnTimer;
	private Timer _waveTimer;
	private RandomNumberGenerator _random = new RandomNumberGenerator();
	
	// 关卡配置
	private LevelConfig _currentLevelConfig;
	private int _currentWave = 0;
	private int _enemiesSpawnedInWave = 0;
	private bool _waveInProgress = false;
	
	// 信号
	[Signal] public delegate void LevelStartedEventHandler(int level);
	[Signal] public delegate void LevelCompletedEventHandler(int level);
	[Signal] public delegate void WaveStartedEventHandler(int wave);
	[Signal] public delegate void WaveCompletedEventHandler(int wave);
	[Signal] public delegate void EnemySpawnedEventHandler(EnemyController enemy);
	[Signal] public delegate void EnemyDefeatedEventHandler(EnemyController enemy);
	
	public override void _Ready()
	{
		_random.Randomize();
		SetupTimers();
		FindPlayer();
		LoadLevelConfig();
		
		// 延迟启动关卡
		CallDeferred(nameof(StartLevel));
	}
	
	private void SetupTimers()
	{
		// 敌人生成计时器
		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = 2.0f;
		_spawnTimer.Timeout += SpawnEnemy;
		AddChild(_spawnTimer);
		
		// 波次计时器
		_waveTimer = new Timer();
		_waveTimer.WaitTime = 30.0f;
		_waveTimer.OneShot = true;
		_waveTimer.Timeout += StartNextWave;
		AddChild(_waveTimer);
	}
	
	private void FindPlayer()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
		if (_player == null)
		{
			GD.PrintErr("LevelManager: Player not found!");
		}
	}
	
	private void LoadLevelConfig()
	{
		_currentLevelConfig = new LevelConfig(CurrentLevel);
	}
	
	public void StartLevel()
	{
		GD.Print($"Starting Level {CurrentLevel}");
		EmitSignal(SignalName.LevelStarted, CurrentLevel);
		
		_currentWave = 0;
		StartNextWave();
	}
	
	private void StartNextWave()
	{
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
		if (!_waveInProgress || _player == null)
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
			
			// 检查位置是否有效（可以添加更多检查，如地形碰撞等）
			if (IsValidSpawnPosition(spawnPos))
			{
				return spawnPos;
			}
		}
		
		return Vector2.Zero;
	}
	
	private bool IsValidSpawnPosition(Vector2 position)
	{
		// 基本检查：确保不在玩家太近的位置
		if (_player != null && position.DistanceTo(_player.GlobalPosition) < MinSpawnDistance)
		{
			return false;
		}
		
		// 可以添加更多检查，如地形碰撞、其他敌人位置等
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
		GetTree().CurrentScene.AddChild(enemy);
		_activeEnemies.Add(enemy);
		
		// 连接敌人死亡信号
		enemy.Connect("enemy_died", new Callable(this, nameof(OnEnemyDied)));
		
		EmitSignal(SignalName.EnemySpawned, enemy);
		
		GD.Print($"Spawned enemy {enemyId} at {position}");
	}
	
	private void OnEnemyDied(EnemyController enemy)
	{
		_activeEnemies.Remove(enemy);
		EmitSignal(SignalName.EnemyDefeated, enemy);
		
		CheckWaveCompletion();
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
			
		// 检查是否所有敌人都已生成且被击败
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
		_spawnTimer.Stop();
		_waveTimer.Stop();
		
		GD.Print($"Level {CurrentLevel} completed!");
		EmitSignal(SignalName.LevelCompleted, CurrentLevel);
		
		// 可以在这里处理关卡完成逻辑，如奖励、下一关卡等
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
		
		// 重新开始关卡
		StartLevel();
	}
	
	public int GetActiveEnemyCount()
	{
		CleanupDeadEnemies();
		return _activeEnemies.Count;
	}
	
	public int GetCurrentWave()
	{
		return _currentWave;
	}
	
	public bool IsWaveInProgress()
	{
		return _waveInProgress;
	}
}
