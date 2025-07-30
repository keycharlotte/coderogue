using System;
using Godot;
using CodeRogue.Data;

namespace CodeRogue.Core
{
    /// <summary>
    /// 自动存档管理器 - 处理自动存档功能
    /// </summary>
    public partial class AutoSaveManager : Node
    {
        private SaveManager _saveManager;
        private SaveSystemConfig _config;
        private SaveSystemEvents _events;
        
        private float _timeSinceLastAutoSave = 0;
        private bool _autoSaveEnabled = true;
        private int _autoSaveInterval = 300; // 默认5分钟
        private bool _isGamePaused = false;
        private bool _isAutoSaving = false;
        
        [Signal]
        public delegate void AutoSaveScheduledEventHandler(float secondsRemaining);
        
        public override void _Ready()
        {
            // 获取依赖的组件
            _saveManager = GetNode<SaveManager>("/root/SaveManager");
            _config = GetNode<SaveSystemConfig>("/root/SaveSystemConfig");
            _events = GetNode<SaveSystemEvents>("/root/SaveSystemEvents");
            
            if (_saveManager == null || _config == null || _events == null)
            {
                GD.PrintErr("AutoSaveManager: 无法获取必要的依赖组件");
                SetProcess(false);
                return;
            }
            
            // 初始化配置
            _autoSaveEnabled = _config.EnableAutoSave;
            _autoSaveInterval = _config.AutoSaveInterval;
            
            // 订阅事件
            _config.ConfigChanged += OnConfigChanged;
            
            // 暂停模式设置为处理
            ProcessMode = ProcessModeEnum.Always;
            
            GD.Print("AutoSaveManager: 初始化完成");
        }
        
        public override void _ExitTree()
        {
            // 取消事件订阅
            if (_config != null)
            {
                _config.ConfigChanged -= OnConfigChanged;
            }
        }
        
        public override void _Process(double delta)
        {
            if (!_autoSaveEnabled || _isGamePaused || _isAutoSaving)
            {
                return;
            }
            
            _timeSinceLastAutoSave += (float)delta;
            
            // 检查是否需要自动存档
            if (_timeSinceLastAutoSave >= _autoSaveInterval)
            {
                TriggerAutoSave();
            }
            else if (_timeSinceLastAutoSave >= _autoSaveInterval - 30 && _timeSinceLastAutoSave % 5 < 0.1f)
            {
                // 在自动存档前30秒，每5秒发出一次通知
                float remainingTime = _autoSaveInterval - _timeSinceLastAutoSave;
                EmitSignal(SignalName.AutoSaveScheduled, remainingTime);
            }
        }
        
        /// <summary>
        /// 触发自动存档
        /// </summary>
        public async void TriggerAutoSave()
        {
            if (_isAutoSaving || _saveManager == null)
            {
                return;
            }
            
            _isAutoSaving = true;
            _events?.TriggerAutoSaveTriggered();
            
            GD.Print("AutoSaveManager: 开始自动存档");
            
            // 执行自动存档
            var result = await _saveManager.SaveGameToSlotAsync(SaveManager.AUTO_SAVE_SLOT, "自动存档", true);
            OnAutoSaveCompleted(result);
        }
        
        /// <summary>
        /// 自动存档完成回调
        /// </summary>
        private void OnAutoSaveCompleted(SaveOperationResult result)
        {
            _isAutoSaving = false;
            _timeSinceLastAutoSave = 0;
            
            if (result.Success)
            {
                _events?.TriggerAutoSaveCompleted(result);
                GD.Print($"AutoSaveManager: 自动存档完成 - {result.Message}");
            }
            else
            {
                _events?.TriggerAutoSaveFailed(result);
                GD.PrintErr($"AutoSaveManager: 自动存档失败 - {result.Message}");
            }
        }
        
        /// <summary>
        /// 配置变更处理
        /// </summary>
        private void OnConfigChanged(string configKey, Variant newValue)
        {
            switch (configKey)
            {
                case "enable_auto_save":
                    _autoSaveEnabled = newValue.AsBool();
                    GD.Print($"AutoSaveManager: 自动存档已{(_autoSaveEnabled ? "启用" : "禁用")}");
                    break;
                    
                case "auto_save_interval":
                    _autoSaveInterval = newValue.AsInt32();
                    GD.Print($"AutoSaveManager: 自动存档间隔已设置为{_autoSaveInterval}秒");
                    break;
            }
        }
        
        /// <summary>
        /// 设置游戏暂停状态
        /// </summary>
        public void SetGamePaused(bool isPaused)
        {
            _isGamePaused = isPaused;
        }
        
        /// <summary>
        /// 重置自动存档计时器
        /// </summary>
        public void ResetAutoSaveTimer()
        {
            _timeSinceLastAutoSave = 0;
            GD.Print("AutoSaveManager: 自动存档计时器已重置");
        }
        
        /// <summary>
        /// 获取距离下次自动存档的剩余时间
        /// </summary>
        public float GetTimeUntilNextAutoSave()
        {
            if (!_autoSaveEnabled)
            {
                return -1;
            }
            
            return Math.Max(0, _autoSaveInterval - _timeSinceLastAutoSave);
        }
        
        /// <summary>
        /// 获取自动存档状态信息
        /// </summary>
        public string GetAutoSaveStatus()
        {
            if (!_autoSaveEnabled)
            {
                return "自动存档已禁用";
            }
            
            if (_isAutoSaving)
            {
                return "正在自动存档...";
            }
            
            float timeRemaining = GetTimeUntilNextAutoSave();
            TimeSpan time = TimeSpan.FromSeconds(timeRemaining);
            
            if (time.TotalMinutes >= 1)
            {
                return $"下次自动存档: {time.Minutes}分{time.Seconds}秒后";
            }
            else
            {
                return $"下次自动存档: {time.Seconds}秒后";
            }
        }
    }
}