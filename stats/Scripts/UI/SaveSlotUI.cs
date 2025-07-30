using System;
using Godot;
using CodeRogue.Data;

namespace CodeRogue.UI
{
    /// <summary>
    /// 存档槽位UI组件 - ViewModel层，只处理数据绑定和逻辑
    /// UI布局和样式应在对应的.tscn文件中定义
    /// </summary>
    public partial class SaveSlotUI : Control
    {
        [Export] private Label _slotIndexLabel;
        [Export] private Label _saveNameLabel;
        [Export] private Label _saveTimeLabel;
        [Export] private Label _playerLevelLabel;
        [Export] private Label _playTimeLabel;
        [Export] private Button _slotButton;
        [Export] private Panel _backgroundPanel;
        
        private SaveSlotInfo _slotInfo;
        private bool _isSelected = false;
        
        [Signal]
        public delegate void SlotSelectedEventHandler(int slotIndex);
        
        public override void _Ready()
        {
            ConnectSignals();
        }
        
        /// <summary>
        /// 连接信号
        /// </summary>
        private void ConnectSignals()
        {
            if (_slotButton != null)
            {
                _slotButton.Pressed += OnSlotButtonPressed;
            }
        }
        
        /// <summary>
        /// 设置槽位信息
        /// </summary>
        public void SetupSlot(SaveSlotInfo slotInfo)
        {
            _slotInfo = slotInfo;
            UpdateDisplay();
        }
        
        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (_slotInfo == null) return;
            
            if (_slotIndexLabel != null)
                _slotIndexLabel.Text = $"槽位 {_slotInfo.SlotIndex + 1}";
            
            if (_slotInfo.Status == SaveSlotStatus.Empty)
            {
                if (_saveNameLabel != null) _saveNameLabel.Text = "空槽位";
                if (_saveTimeLabel != null) _saveTimeLabel.Text = "";
                if (_playerLevelLabel != null) _playerLevelLabel.Text = "";
                if (_playTimeLabel != null) _playTimeLabel.Text = "";
            }
            else if (_slotInfo.Metadata != null)
            {
                if (_saveNameLabel != null) _saveNameLabel.Text = _slotInfo.Metadata.SaveName;
                if (_saveTimeLabel != null) _saveTimeLabel.Text = _slotInfo.Metadata.GetFormattedSaveTime();
                if (_playerLevelLabel != null) _playerLevelLabel.Text = $"等级 {_slotInfo.Metadata.PlayerLevel}";
                if (_playTimeLabel != null) _playTimeLabel.Text = _slotInfo.Metadata.GetFormattedPlayTime();
            }
            
            // 更新视觉状态
            UpdateVisualState();
        }
        
        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisualState();
        }
        
        /// <summary>
        /// 更新视觉状态 - 通过修改现有组件的属性而非创建新组件
        /// </summary>
        private void UpdateVisualState()
        {
            if (_backgroundPanel == null) return;
            
            // 根据状态设置颜色 - 这是数据绑定，不是UI创建
            Color backgroundColor = _isSelected ? Colors.LightBlue : Colors.Gray;
            
            if (_slotInfo?.Status == SaveSlotStatus.Corrupted)
            {
                backgroundColor = Colors.Red;
            }
            else if (_slotInfo?.Status == SaveSlotStatus.Empty)
            {
                backgroundColor = Colors.DarkGray;
            }
            
            _backgroundPanel.Modulate = backgroundColor;
        }
        
        /// <summary>
        /// 槽位按钮点击
        /// </summary>
        private void OnSlotButtonPressed()
        {
            if (_slotInfo != null)
            {
                EmitSignal(SignalName.SlotSelected, _slotInfo.SlotIndex);
            }
        }
    }
}