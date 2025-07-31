using Godot;
using CodeRogue.Data;
using CodeRogue.UI;
using CodeRogue.Utils;
using CodeRogue.Level;
using CodeRogue.Player; // 添加LevelManager命名空间

namespace CodeRogue.Core
{
	/// <summary>
	/// 游戏主管理器 - 负责游戏状态管理和核心系统协调
	/// </summary>
	public partial class GameManager : Node
	{
		[Signal]
		public delegate void GameStateChangedEventHandler(GameState newState);
		
		[Signal]
		public delegate void PlayerDeathEventHandler();
		
		[Signal]
		public delegate void LevelCompletedEventHandler();
		
		[Signal]
		public delegate void OnStartLevelEventHandler(int levelNumber);
		
		[Signal]
		public delegate void OnQuitLevelEventHandler(int levelNumber);
		
		[Export]
		public GameData GameData { get; set; }
		
		private GameState _currentState = GameState.Menu;
		private UIManager _uiManager;
		private AudioManager _audioManager;
		private LevelManager _levelManager; // 添加LevelManager引用
		
		public GameState CurrentState 
		{
			get => _currentState;
			private set
			{
				if (_currentState != value)
				{
					_currentState = value;
					EmitSignal(SignalName.GameStateChanged, (int)value);
				}
			}
		}
		
		public override void _Ready()
		{
			InitializeGame();
			InitializeCoreGameplay(); // 新增
		}
		
		private void InitializeCoreGameplay()
		{
			// WordManager和InputManager现在是autoload单例，不需要手动创建
			// 它们会在游戏启动时自动初始化
			GD.Print("Core gameplay systems initialized (autoload singletons)");
		}
		
		private void InitializeGame()
		{
			// 修改：使用相对于父节点的路径
			// _uiManager = GetNode<UIManager>("../UIManager");
			// _audioManager = GetNode<AudioManager>("../AudioManager");
			
			// 或者使用绝对路径（更安全）
			_uiManager = NodeUtils.GetUIManager(this);
			_audioManager = NodeUtils.GetAudioManager(this);
			
			// 激活LevelManager
			ActivateLevelManager();
			
			// 初始化游戏数据
			if (GameData == null)
			{
				GameData = new GameData();
			}
			
			GD.Print("GameManager initialized");
		}
		
		public void StartGame()
		{
			CurrentState = GameState.Playing;
			_uiManager?.ShowGameUI();
					
			// 启动关卡
			_levelManager?.StartLevel();
		}
		
		/// <summary>
		/// 激活LevelManager实例
		/// </summary>
		private void ActivateLevelManager()
		{
			// 通过AutoLoad路径获取LevelManager实例
			_levelManager = GetNode<LevelManager>("/root/LevelManager");
			
			if (_levelManager != null)
			{
				// 连接LevelManager的信号
				ConnectLevelManagerSignals();
				
				GD.Print("LevelManager activated by GameManager");
			}
			else
			{
				GD.PrintErr("Failed to get LevelManager from AutoLoad");
			}
		}
		
		/// <summary>
		/// 重置LevelManager实例
		/// </summary>
		private void ResetLevelManager()
		{
			if (_levelManager != null)
			{
				// 断开信号连接
				DisconnectLevelManagerSignals();
				
				// 重置LevelManager状态（如果有重置方法的话）
				// _levelManager.Reset();
				
				_levelManager = null;
				
				GD.Print("LevelManager reset by GameManager");
			}
		}
		
		/// <summary>
		/// 连接LevelManager的信号
		/// </summary>
		private void ConnectLevelManagerSignals()
		{
			if (_levelManager != null)
			{
				_levelManager.Connect(LevelManager.SignalName.LevelCompleted, new Callable(this, nameof(OnLevelCompleted)));
				_levelManager.Connect(LevelManager.SignalName.PlayerCreated, new Callable(this, nameof(OnPlayerCreated)));
				// 可以根据需要连接更多信号
			}
		}
		
		/// <summary>
		/// 断开LevelManager的信号连接
		/// </summary>
		private void DisconnectLevelManagerSignals()
		{
			if (_levelManager != null)
			{
				if (_levelManager.IsConnected(LevelManager.SignalName.LevelCompleted, new Callable(this, nameof(OnLevelCompleted))))
				{
					_levelManager.Disconnect(LevelManager.SignalName.LevelCompleted, new Callable(this, nameof(OnLevelCompleted)));
				}
				
				if (_levelManager.IsConnected(LevelManager.SignalName.PlayerCreated, new Callable(this, nameof(OnPlayerCreated))))
				{
					_levelManager.Disconnect(LevelManager.SignalName.PlayerCreated, new Callable(this, nameof(OnPlayerCreated)));
				}
			}
		}
		
		/// <summary>
		/// 关卡完成回调
		/// </summary>
		private void OnLevelCompleted(int level)
		{
			GD.Print($"Level {level} completed!");
			EmitSignal(SignalName.LevelCompleted);
			
			// 可以在这里处理关卡完成逻辑，比如加载下一关
			// LoadNextLevel();
		}
		
		/// <summary>
		/// 玩家创建回调
		/// </summary>
		private void OnPlayerCreated(PlayerController player)
		{
			GD.Print("Player created in level");
			// 可以在这里处理玩家创建后的逻辑
		}
		
		public void GameOver()
		{
			CurrentState = GameState.GameOver;
			EmitSignal(SignalName.PlayerDeath);
			_uiManager?.ShowGameOverScreen();
			
			// 游戏结束时重置LevelManager
			ResetLevelManager();
		}
		
		public void RestartGame()
		{
			// 重置游戏状态
			CurrentState = GameState.Playing;
			GetTree().Paused = false;
			
			// 重置当前LevelManager
			ResetLevelManager();
			
			// 重新激活LevelManager
			ActivateLevelManager();
			_levelManager?.StartLevel();
			
			// 显示游戏UI
			_uiManager?.ShowGameUI();
			
			GD.Print("Game restarted with new LevelManager");
		}
		
		public void ReturnToMainMenu()
		{
			CurrentState = GameState.Menu;
			GetTree().Paused = false;
			
			// 返回主菜单时重置LevelManager
			ResetLevelManager();
			
			GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
			
			GD.Print("Returned to main menu, LevelManager destroyed");
		}
		
		public void ReturnToMenu()
		{
			CurrentState = GameState.Menu;
			GetTree().Paused = false;
			
			// 重置LevelManager
			ResetLevelManager();
			
			GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
		}
		
		/// <summary>
		/// 获取当前LevelManager实例
		/// </summary>
		public LevelManager GetLevelManager()
		{
			return _levelManager;
		}
		
		public void QuitGame()
		{
			GetTree().Quit();
		}

		public void PauseGame()
		{
			GetTree().Paused = true;
		}

		public void ResumeGame()
		{
			GetTree().Paused = false;
		}
		
		/// <summary>
		/// 触发开始关卡事件
		/// </summary>
		/// <param name="levelNumber">关卡编号</param>
		public void TriggerStartLevel(int levelNumber)
		{
			EmitSignal(SignalName.OnStartLevel, levelNumber);
			GD.Print($"Level {levelNumber} started - global event triggered");
		}
		
		/// <summary>
		/// 触发退出关卡事件
		/// </summary>
		/// <param name="levelNumber">关卡编号</param>
		public void TriggerQuitLevel(int levelNumber)
		{
			EmitSignal(SignalName.OnQuitLevel, levelNumber);
			GD.Print($"Level {levelNumber} quit - global event triggered");
		}
	}
}
