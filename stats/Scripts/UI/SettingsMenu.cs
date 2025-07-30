using Godot;
using CodeRogue.Core;
using CodeRogue.Utils;

namespace CodeRogue.UI
{
    /// <summary>
    /// 设置菜单界面
    /// </summary>
    public partial class SettingsMenu : Control
    {
        [Export]
        public Slider MasterVolumeSlider { get; set; }
        
        [Export]
        public Slider MusicVolumeSlider { get; set; }
        
        [Export]
        public Slider SfxVolumeSlider { get; set; }
        
        [Export]
        public CheckBox FullscreenCheckBox { get; set; }
        
        [Export]
        public OptionButton ResolutionOption { get; set; }
        
        [Export]
        public Button BackButton { get; set; }
        
        [Export]
        public Button ResetButton { get; set; }

        public override void _Ready()
        {
            // 连接控件信号
            if (MasterVolumeSlider != null)
                MasterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
                
            if (MusicVolumeSlider != null)
                MusicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
                
            if (SfxVolumeSlider != null)
                SfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;
                
            if (FullscreenCheckBox != null)
                FullscreenCheckBox.Toggled += OnFullscreenToggled;
                
            if (ResolutionOption != null)
                ResolutionOption.ItemSelected += OnResolutionSelected;
                
            if (BackButton != null)
                BackButton.Pressed += OnBackPressed;
                
            if (ResetButton != null)
                ResetButton.Pressed += OnResetPressed;
                
            // 加载当前设置
            LoadSettings();
        }
        
        /// <summary>
        /// 显示设置菜单
        /// </summary>
        public void ShowSettings()
        {
            Visible = true;
            LoadSettings();
        }
        
        /// <summary>
        /// 隐藏设置菜单
        /// </summary>
        public void HideSettings()
        {
            Visible = false;
            
        }
        
        private void LoadSettings()
        {
            var audioManager = NodeUtils.GetAudioManager(this);
            if (audioManager != null)
            {
                if (MasterVolumeSlider != null)
                    MasterVolumeSlider.Value = audioManager.MasterVolume;
                    
                if (MusicVolumeSlider != null)
                    MusicVolumeSlider.Value = audioManager.MusicVolume;
                    
                if (SfxVolumeSlider != null)
                    SfxVolumeSlider.Value = audioManager.SfxVolume;
            }
            
            // 加载显示设置
            if (FullscreenCheckBox != null)
                FullscreenCheckBox.ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        }
        
        private void OnMasterVolumeChanged(double value)
        {
            var audioManager = NodeUtils.GetAudioManager(this);
            audioManager?.SetMasterVolume((float)value);
        }
        
        private void OnMusicVolumeChanged(double value)
        {
            var audioManager = NodeUtils.GetAudioManager(this);
            audioManager?.SetMusicVolume((float)value);
        }
        
        private void OnSfxVolumeChanged(double value)
        {
            var audioManager = NodeUtils.GetAudioManager(this);
            audioManager?.SetSFXVolume((float)value);
        }
        
        private void OnFullscreenToggled(bool pressed)
        {
            if (pressed)
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            else
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
        
        private void OnResolutionSelected(long index)
        {
            // 处理分辨率选择
            Vector2I[] resolutions = {
                new Vector2I(1920, 1080),
                new Vector2I(1600, 900),
                new Vector2I(1366, 768),
                new Vector2I(1280, 720)
            };
            
            if (index >= 0 && index < resolutions.Length)
            {
                DisplayServer.WindowSetSize(resolutions[index]);
            }
        }
        
        private void OnBackPressed()
        {
            // 通过UIManager返回上一级UI
            var uiManager = NodeUtils.GetUIManager(this);
            uiManager?.HideSettingsMenu();
        }
        
        private void OnResetPressed()
        {
            // 重置所有设置到默认值
            if (MasterVolumeSlider != null)
                MasterVolumeSlider.Value = 1.0;
                
            if (MusicVolumeSlider != null)
                MusicVolumeSlider.Value = 0.8;
                
            if (SfxVolumeSlider != null)
                SfxVolumeSlider.Value = 1.0;
                
            if (FullscreenCheckBox != null)
                FullscreenCheckBox.ButtonPressed = false;
                
            // 应用默认设置
            OnMasterVolumeChanged(1.0);
            OnMusicVolumeChanged(0.8);
            OnSfxVolumeChanged(1.0);
            OnFullscreenToggled(false);
        }
    }
}