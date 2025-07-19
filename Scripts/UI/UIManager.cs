using Godot;
using CodeRogue.Core;
using CodeRogue.Data;
using System.Collections.Generic;

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
        private Stack<Control> _uiStack = new Stack<Control>();  // 添加UI栈
        private MainMenu _mainMenu;
        private GameUI _gameUI;
        private PauseMenu _pauseMenu;
        private GameOverScreen _gameOverScreen;
        private SettingsMenu _settingsMenu;
        
        public override void _Ready()
        {
            // 添加到组中以便其他脚本找到
            AddToGroup("UIManager");
            
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
            // ShowMainMenu();
        }
        
        public void ShowMainMenu()
        {
            HideAllUI();
            ClearUIStack();  // 清空栈
            if (_mainMenu != null)
            {
                _mainMenu.Visible = true;
                _currentUI = _mainMenu;
            }
        }
        
        public void ShowGameUI()
        {
            HideAllUI();
            ClearUIStack();  // 清空栈
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
                // 将当前UI压入栈中（通常是GameUI）
                if (_currentUI != null && _currentUI.Visible)
                {
                    _uiStack.Push(_currentUI);
                }
                _pauseMenu.Visible = true;
                _currentUI = _pauseMenu;
            }
        }
        
        public void HidePauseMenu()
        {
            if (_pauseMenu != null)
            {
                _pauseMenu.Visible = false;
                
                // 返回到上一级UI
                if (_uiStack.Count > 0)
                {
                    var previousUI = _uiStack.Pop();
                    if (previousUI != null)
                    {
                        previousUI.Visible = true;
                        _currentUI = previousUI;
                    }
                }
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
                // 将当前UI压入栈中
                if (_currentUI != null && _currentUI.Visible)
                {
                    _uiStack.Push(_currentUI);
                }
                _settingsMenu.Visible = true;
                _currentUI = _settingsMenu;
            }
        }
        
        public void HideSettingsMenu()
        {
            if (_settingsMenu != null)
            {
                _settingsMenu.Visible = false;
                
                // 返回到上一级UI
                if (_uiStack.Count > 0)
                {
                    var previousUI = _uiStack.Pop();
                    if (previousUI != null)
                    {
                        previousUI.Visible = true;
                        _currentUI = previousUI;
                    }
                }
                else
                {
                    // 如果栈为空，默认返回主菜单
                    ShowMainMenu();
                }
            }
        }
        
        // 清空UI栈的方法（在切换到主要UI时调用）
        public void ClearUIStack()
        {
            _uiStack.Clear();
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