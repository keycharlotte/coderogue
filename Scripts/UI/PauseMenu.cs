using Godot;
using CodeRogue.Core;
using CodeRogue.Utils;

namespace CodeRogue.UI
{
    /// <summary>
    /// 暂停菜单 - 处理游戏暂停时的UI
    /// </summary>
    public partial class PauseMenu : Control
    {
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        private Button _quitButton;
        private Panel _backgroundPanel;
        
        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
            
            // 设置为暂停处理模式
            ProcessMode = ProcessModeEnum.WhenPaused;
        }
        
        private void InitializeUI()
        {
            _resumeButton = GetNode<Button>("CenterContainer/VBoxContainer/ResumeButton");
            _settingsButton = GetNode<Button>("CenterContainer/VBoxContainer/SettingsButton");
            _mainMenuButton = GetNode<Button>("CenterContainer/VBoxContainer/MainMenuButton");
            _quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");
            _backgroundPanel = GetNode<Panel>("BackgroundPanel");
            
            // 设置半透明背景
            if (_backgroundPanel != null)
            {
                _backgroundPanel.Modulate = new Color(0, 0, 0, 0.7f);
            }
        }
        
        private void ConnectSignals()
        {
            if (_resumeButton != null)
            {
                _resumeButton.Pressed += OnResumeButtonPressed;
            }
            
            if (_settingsButton != null)
            {
                _settingsButton.Pressed += OnSettingsButtonPressed;
            }
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.Pressed += OnMainMenuButtonPressed;
            }
            
            if (_quitButton != null)
            {
                _quitButton.Pressed += OnQuitButtonPressed;
            }
        }
        
        private void OnResumeButtonPressed()
        {
            GameManager.Instance?.ResumeGame();
        }
        
        private void OnSettingsButtonPressed()
        {
            var uiManager = NodeUtils.GetUIManager(this);
            uiManager?.ShowSettingsMenu();
        }
        
        private void OnMainMenuButtonPressed()
        {
            GameManager.Instance?.ReturnToMenu();
        }
        
        private void OnQuitButtonPressed()
        {
            GameManager.Instance?.QuitGame();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape)
                {
                    OnResumeButtonPressed();
                }
            }
        }
    }
}