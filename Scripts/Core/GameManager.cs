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
        
        public static GameManager Instance { get; private set; }
        
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
            if (Instance == null)
            {
                Instance = this;
                InitializeGame();
                InitializeCoreGameplay(); // 新增
            }
            else
            {
                QueueFree();
            }
        }
        
        private void InitializeCoreGameplay()
        {
            // 初始化单词管理器
            var wordManager = new WordManager();
            AddChild(wordManager);
            
            // 初始化输入管理器
            var inputManager = new InputManager();
            AddChild(inputManager);
        }
        
        private void InitializeGame()
        {
            // 修改：使用相对于父节点的路径
            // _uiManager = GetNode<UIManager>("../UIManager");
            // _audioManager = GetNode<AudioManager>("../AudioManager");
            
            // 或者使用绝对路径（更安全）
            _uiManager = NodeUtils.GetUIManager(this);
            _audioManager = NodeUtils.GetAudioManager(this);
            
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
            
            // 创建并初始化LevelManager
            CreateLevelManager();
            
            // 启动关卡
            _levelManager?.StartLevel();
        }
        
        /// <summary>
        /// 创建LevelManager实例
        /// </summary>
        private void CreateLevelManager()
        {
            // 如果已存在，先销毁
            DestroyLevelManager();
            
            // 创建新的LevelManager实例
            _levelManager = new LevelManager();
            
            // 添加到场景树中
            AddChild(_levelManager);
            
            // 连接LevelManager的信号
            ConnectLevelManagerSignals();
            
            GD.Print("LevelManager created by GameManager");
        }
        
        /// <summary>
        /// 销毁LevelManager实例
        /// </summary>
        private void DestroyLevelManager()
        {
            if (_levelManager != null)
            {
                // 断开信号连接
                DisconnectLevelManagerSignals();
                
                // 从场景树中移除并释放
                _levelManager.QueueFree();
                _levelManager = null;
                
                GD.Print("LevelManager destroyed by GameManager");
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
            
            // 游戏结束时销毁LevelManager
            DestroyLevelManager();
        }
        
        public void RestartGame()
        {
            // 重置游戏状态
            CurrentState = GameState.Playing;
            GetTree().Paused = false;
            
            // 销毁当前LevelManager
            DestroyLevelManager();
            
            // 重新创建LevelManager
            CreateLevelManager();
            _levelManager?.StartLevel();
            
            // 显示游戏UI
            _uiManager?.ShowGameUI();
            
            GD.Print("Game restarted with new LevelManager");
        }
        
        public void ReturnToMainMenu()
        {
            CurrentState = GameState.Menu;
            GetTree().Paused = false;
            
            // 返回主菜单时销毁LevelManager
            DestroyLevelManager();
            
            GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
            
            GD.Print("Returned to main menu, LevelManager destroyed");
        }
        
        public void ReturnToMenu()
        {
            CurrentState = GameState.Menu;
            GetTree().Paused = false;
            
            // 销毁LevelManager
            DestroyLevelManager();
            
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