using Godot;
using CodeRogue.Core;
using CodeRogue.Data;

namespace CodeRogue.UI
{
    /// <summary>
    /// UI管理器 - 负责管理所有UI界面的显示和隐藏
    /// </summary>
    public partial class UIManager : CanvasLayer
    {
        [Export]
        public PackedScene MainMenuScene { get; set; }
        
        [Export]
        public PackedScene GameUIScene { get; set; }
        
        [Export]
        public PackedScene PauseMenuScene { get; set; }
        
        [Export]
        public PackedScene GameOverScene { get; set; }
        
        [Export]
        public PackedScene SettingsScene { get; set; }
        
        private Control _currentUI;
        private MainMenu _mainMenu;
        private GameUI _gameUI;
        private PauseMenu _pauseMenu;
        private GameOverScreen _gameOverScreen;
        private SettingsMenu _settingsMenu;
        
        public override void _Ready()
        {
            // 初始化UI界面
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // 创建主菜单
            if (MainMenuScene != null)
            {
                _mainMenu = MainMenuScene.Instantiate<MainMenu>();
                AddChild(_mainMenu);
                _mainMenu.Visible = false;
            }
            
            // 创建游戏UI
            if (GameUIScene != null)
            {
                _gameUI = GameUIScene.Instantiate<GameUI>();
                AddChild(_gameUI);
                _gameUI.Visible = false;
            }
            
            // 创建暂停菜单
            if (PauseMenuScene != null)
            {
                _pauseMenu = PauseMenuScene.Instantiate<PauseMenu>();
                AddChild(_pauseMenu);
                _pauseMenu.Visible = false;
            }
            
            // 创建游戏结束界面
            if (GameOverScene != null)
            {
                _gameOverScreen = GameOverScene.Instantiate<GameOverScreen>();
                AddChild(_gameOverScreen);
                _gameOverScreen.Visible = false;
            }
            
            // 创建设置菜单
            if (SettingsScene != null)
            {
                _settingsMenu = SettingsScene.Instantiate<SettingsMenu>();
                AddChild(_settingsMenu);
                _settingsMenu.Visible = false;
            }

            GD.Print("UIManager initialized");
            ShowMainMenu();
        }
        
        public void ShowMainMenu()
        {
            HideAllUI();
            if (_mainMenu != null)
            {
                _mainMenu.Visible = true;
                _currentUI = _mainMenu;
            }
        }
        
        public void ShowGameUI()
        {
            HideAllUI();
            if (_gameUI != null)
            {
                _gameUI.Visible = true;
                _currentUI = _gameUI;
            }
        }
        
        public void ShowPauseMenu()
        {
            if (_pauseMenu != null)
            {
                _pauseMenu.Visible = true;
            }
        }
        
        public void HidePauseMenu()
        {
            if (_pauseMenu != null)
            {
                _pauseMenu.Visible = false;
            }
        }
        
        public void ShowGameOverScreen()
        {
            if (_gameOverScreen != null)
            {
                _gameOverScreen.Visible = true;
            }
        }
        
        public void ShowSettingsMenu()
        {
            if (_settingsMenu != null)
            {
                _settingsMenu.Visible = true;
            }
        }
        
        public void HideSettingsMenu()
        {
            if (_settingsMenu != null)
            {
                _settingsMenu.Visible = false;
            }
        }
        
        private void HideAllUI()
        {
            _mainMenu?.Hide();
            _gameUI?.Hide();
            _pauseMenu?.Hide();
            _gameOverScreen?.Hide();
            _settingsMenu?.Hide();
        }
    }
}