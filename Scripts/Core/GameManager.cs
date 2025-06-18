using Godot;
using CodeRogue.Data;
using CodeRogue.UI;

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
        
        [Export]
        public GameData GameData { get; set; }
        
        public static GameManager Instance { get; private set; }
        
        private GameState _currentState = GameState.Menu;
        private UIManager _uiManager;
        private AudioManager _audioManager;
        
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
            _uiManager = GetNode<UIManager>("../UIManager");
            _audioManager = GetNode<AudioManager>("../AudioManager");
            
            // 或者使用绝对路径（更安全）
            // _uiManager = GetNode<UIManager>("/root/Main/UIManager");
            // _audioManager = GetNode<AudioManager>("/root/Main/AudioManager");
            
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
        }
        
        public void PauseGame()
        {
            CurrentState = GameState.Paused;
            GetTree().Paused = true;
            _uiManager?.ShowPauseMenu();
        }
        
        
        public void ResumeGame()
        {
            CurrentState = GameState.Playing;
            GetTree().Paused = false;
            _uiManager?.ShowGameUI();
        }
        
        public void GameOver()
        {
            CurrentState = GameState.GameOver;
            EmitSignal(SignalName.PlayerDeath);
            _uiManager?.ShowGameOverScreen();
        }
        
        // 添加重新开始游戏方法
        public void RestartGame()
        {
            // 重置游戏状态
            CurrentState = GameState.Playing;
            GetTree().Paused = false;
            
            // 重新加载当前场景
            GetTree().ReloadCurrentScene();
            
            // 显示游戏UI
            _uiManager?.ShowGameUI();
            
            GD.Print("Game restarted");
        }
        
        // 添加返回主菜单方法
        public void ReturnToMainMenu()
        {
            CurrentState = GameState.Menu;
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
            
            GD.Print("Returned to main menu");
        }
        
        public void ReturnToMenu()
        {
            CurrentState = GameState.Menu;
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
        }
        
        public void QuitGame()
        {
            GetTree().Quit();
        }
    }
}