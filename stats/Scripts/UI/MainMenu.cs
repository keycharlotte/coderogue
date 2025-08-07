using Godot;
using CodeRogue.Core;
using CodeRogue.Utils;
using CodeRogue.Data;
using CodeRogue.Utils;
using System.Linq;

namespace CodeRogue.UI
{
    /// <summary>
    /// 主菜单控制器 - 处理主菜单的交互逻辑
    /// </summary>
    public partial class MainMenu : Control
    {
        [Export]
        public AudioStream ButtonHoverSound { get; set; }
        
        [Export]
        public AudioStream ButtonClickSound { get; set; }
        
        private Button _startButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _quitButton;
        private Label _titleLabel;
        private AudioStreamPlayer _audioPlayer;
        
        [Export] public TextureRect BackgroundRect { get; set; }

        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
            CheckSaveGame();
        }
        
        private void InitializeUI()
        {
            // 获取UI节点
            _startButton = GetNode<Button>("VBoxContainer/StartButton");
            _continueButton = GetNode<Button>("VBoxContainer/ContinueButton");
            _settingsButton = GetNode<Button>("VBoxContainer/SettingsButton");
            _quitButton = GetNode<Button>("VBoxContainer/QuitButton");
            _titleLabel = GetNode<Label>("TitleLabel");
            _audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
            
            // 设置标题
            if (_titleLabel != null)
            {
                _titleLabel.Text = "Code Rogue";
            }
        }
        
        private void ConnectSignals()
        {
            // 连接按钮点击信号
            if (_startButton != null)
                _startButton.Pressed += OnStartButtonPressed;
            if (_continueButton != null)
                _continueButton.Pressed += OnContinueButtonPressed;
            if (_settingsButton != null)
                _settingsButton.Pressed += OnSettingsButtonPressed;
            if (_quitButton != null)
                _quitButton.Pressed += OnQuitButtonPressed;
            
            // 批量添加悬停动效
            var buttons = new[] { _startButton, _continueButton, _settingsButton, _quitButton };
            ButtonAnimationHelper.AddHoverAnimationToButtons(buttons.Where(b => b != null).ToArray());
            
            // 添加悬停音效（需要单独处理）
            foreach (var button in buttons.Where(b => b != null))
            {
                button.MouseEntered += PlayButtonHoverSound;
            }
        }
        
        private void CheckSaveGame()
        {
            // 检查是否有存档
            bool hasSaveGame = ResourceLoader.Exists("user://savegame.tres");
            
            if (_continueButton != null)
            {
                _continueButton.Disabled = !hasSaveGame;
            }
        }
        
        private void OnStartButtonPressed()
        {
            PlayButtonClickSound();
            StartNewGame();
        }
        
        private void OnContinueButtonPressed()
        {
            PlayButtonClickSound();
            ContinueGame();
        }
        
        private void OnSettingsButtonPressed()
        {
            PlayButtonClickSound();
            ShowSettings();
        }
        
        private void OnQuitButtonPressed()
        {
            PlayButtonClickSound();
            QuitGame();
        }
        
        private void OnButtonHover(Button button)
        {
            PlayButtonHoverSound();
            
            // 添加悬停效果
            var tween = CreateTween();
            tween.TweenProperty(button, "scale", Vector2.One * 1.1f, 0.1f);
        }
        
        private void StartNewGame()
        {
            // 创建新游戏数据
            var gameData = new GameData();
            gameData.ResetToDefaults();
            // 切换到游戏场景
            var gameManager = NodeUtils.GetGameManager(this);
            gameManager?.StartGame();
            // GetTree().ChangeSceneToFile("res://Scenes/Level/BaseLevel.tscn");
        }
        
        private void ContinueGame()
        {
            // // 加载存档
            // var gameData = GameData.LoadGame();
            
            // // 根据存档数据切换到对应关卡
            // string levelScene = $"res://Scenes/Level/Level{gameData.CurrentLevel}.tscn";
            // GetTree().ChangeSceneToFile(levelScene);
        }
        
        private void ShowSettings()
        {
            // 显示设置菜单
            var uiManager = NodeUtils.GetUIManager(this);
            uiManager?.ShowSettingsMenu();
        }
        
        private void QuitGame()
        {
            GetTree().Quit();
        }
        
        private void PlayButtonHoverSound()
        {
            if (_audioPlayer != null && ButtonHoverSound != null)
            {
                _audioPlayer.Stream = ButtonHoverSound;
                _audioPlayer.Play();
            }
        }
        
        private void PlayButtonClickSound()
        {
            if (_audioPlayer != null && ButtonClickSound != null)
            {
                _audioPlayer.Stream = ButtonClickSound;
                _audioPlayer.Play();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            // 处理键盘导航
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Enter:
                    case Key.Space:
                        if (_startButton != null && _startButton.HasFocus())
                        {
                            OnStartButtonPressed();
                        }
                        break;
                    case Key.Escape:
                        OnQuitButtonPressed();
                        break;
                }
            }
        }
    }
}