using System;
using Godot;
using CodeRogue.Core;
using CodeRogue.Data;
using Godot.Collections;

namespace CodeRogue.UI
{
    /// <summary>
    /// 存档系统UI管理器 - 处理存档界面的显示和交互
    /// </summary>
    public partial class SaveSystemUI : Control
    {
        [Export] private VBoxContainer _saveSlotContainer;
        [Export] private Button _saveButton;
        [Export] private Button _loadButton;
        [Export] private Button _deleteButton;
        [Export] private Button _backButton;
        [Export] private LineEdit _saveNameInput;
        [Export] private Label _statusLabel;
        [Export] private ConfirmationDialog _confirmDialog;
        [Export] private AcceptDialog _messageDialog;
        [Export] private PackedScene _saveSlotScene;
        
        private SaveManager _saveManager;
        private Array<SaveSlotUI> _slotUIs = new();
        private int _selectedSlotIndex = -1;
        private bool _isLoadMode = false;
        
        [Signal]
        public delegate void SaveSystemClosedEventHandler();
        
        [Signal]
        public delegate void GameLoadedEventHandler(int slotIndex);
        
        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
            GetSaveManager();
        }
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            if (_saveSlotContainer == null)
            {
                GD.PrintErr("SaveSystemUI: SaveSlotContainer未设置");
                return;
            }
            
            // 设置初始状态
            UpdateButtonStates();
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "请选择存档槽位";
            }
        }
        
        /// <summary>
        /// 连接信号
        /// </summary>
        private void ConnectSignals()
        {
            if (_saveButton != null)
                _saveButton.Pressed += OnSaveButtonPressed;
                
            if (_loadButton != null)
                _loadButton.Pressed += OnLoadButtonPressed;
                
            if (_deleteButton != null)
                _deleteButton.Pressed += OnDeleteButtonPressed;
                
            if (_backButton != null)
                _backButton.Pressed += OnBackButtonPressed;
        }
        
        /// <summary>
        /// 获取SaveManager引用
        /// </summary>
        private void GetSaveManager()
        {
            _saveManager = GetNode<SaveManager>("/root/SaveManager");
            if (_saveManager == null)
            {
                GD.PrintErr("SaveSystemUI: 无法找到SaveManager");
                return;
            }
            
            // 连接SaveManager信号
            _saveManager.SaveCompleted += OnSaveCompleted;
            _saveManager.LoadCompleted += OnLoadCompleted;
            
            // 刷新存档槽位
            RefreshSaveSlots();
        }
        
        /// <summary>
        /// 显示存档界面
        /// </summary>
        public void ShowSaveInterface()
        {
            _isLoadMode = false;
            Visible = true;
            RefreshSaveSlots();
            UpdateUIForMode();
        }
        
        /// <summary>
        /// 显示读档界面
        /// </summary>
        public void ShowLoadInterface()
        {
            _isLoadMode = true;
            Visible = true;
            RefreshSaveSlots();
            UpdateUIForMode();
        }
        
        /// <summary>
        /// 隐藏界面
        /// </summary>
        public void HideInterface()
        {
            Visible = false;
            _selectedSlotIndex = -1;
            UpdateButtonStates();
        }
        
        /// <summary>
        /// 根据模式更新UI
        /// </summary>
        private void UpdateUIForMode()
        {
            if (_saveButton != null)
                _saveButton.Visible = !_isLoadMode;
                
            if (_saveNameInput != null)
                _saveNameInput.Visible = !_isLoadMode;
                
            if (_loadButton != null)
                _loadButton.Visible = _isLoadMode;
                
            if (_statusLabel != null)
            {
                _statusLabel.Text = _isLoadMode ? "请选择要加载的存档" : "请选择存档槽位";
            }
        }
        
        /// <summary>
        /// 刷新存档槽位显示
        /// </summary>
        private void RefreshSaveSlots()
        {
            if (_saveManager == null || _saveSlotContainer == null) return;
            
            // 清除现有槽位UI
            foreach (Node child in _saveSlotContainer.GetChildren())
            {
                child.QueueFree();
            }
            _slotUIs.Clear();
            
            // 获取存档槽位信息
            var saveSlots = _saveManager.GetSaveSlots();
            
            // 创建槽位UI
            for (int i = 0; i < saveSlots.Count; i++)
            {
                var slotInfo = saveSlots[i];
                var slotUI = CreateSaveSlotUI(slotInfo);
                if (slotUI != null)
                {
                    _saveSlotContainer.AddChild(slotUI);
                    _slotUIs.Add(slotUI);
                }
            }
        }
        
        /// <summary>
        /// 创建存档槽位UI
        /// </summary>
        private SaveSlotUI CreateSaveSlotUI(SaveSlotInfo slotInfo)
        {
            if (_saveSlotScene == null)
            {
                // 如果没有预设场景，创建简单的UI
                return CreateSimpleSaveSlotUI(slotInfo);
            }
            
            var slotInstance = _saveSlotScene.Instantiate<SaveSlotUI>();
            if (slotInstance != null)
            {
                slotInstance.SetupSlot(slotInfo);
                slotInstance.SlotSelected += OnSlotSelected;
                return slotInstance;
            }
            
            return CreateSimpleSaveSlotUI(slotInfo);
        }
        
        /// <summary>
        /// 创建简单的存档槽位UI
        /// </summary>
        private SaveSlotUI CreateSimpleSaveSlotUI(SaveSlotInfo slotInfo)
        {
            // UI组件应通过场景文件创建，而不是在代码中动态创建
            // 如果没有预设场景，应该在.tscn文件中预定义SaveSlotUI组件
            if (_saveSlotScene != null)
            {
                var slotUI = _saveSlotScene.Instantiate<SaveSlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetupSlot(slotInfo);
                    slotUI.SlotSelected += OnSlotSelected;
                    return slotUI;
                }
            }
            
            // 临时回退方案，应该通过.tscn场景文件替代
            GD.PrintErr("SaveSystemUI: 缺少SaveSlotUI场景文件，请在.tscn中预定义");
            return null;
        }
        
        /// <summary>
        /// 槽位选择事件
        /// </summary>
        private void OnSlotSelected(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;
            
            // 更新槽位UI选中状态
            for (int i = 0; i < _slotUIs.Count; i++)
            {
                _slotUIs[i].SetSelected(i == slotIndex);
            }
            
            UpdateButtonStates();
            
            if (_statusLabel != null)
            {
                var slotInfo = _saveManager.GetSaveSlots()[slotIndex];
                string statusText = slotInfo.Status == SaveSlotStatus.Empty 
                    ? $"槽位 {slotIndex + 1}: 空槽位" 
                    : $"槽位 {slotIndex + 1}: {slotInfo.Metadata?.SaveName ?? "未知存档"}";
                _statusLabel.Text = statusText;
            }
        }
        
        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            bool hasSelection = _selectedSlotIndex >= 0;
            bool hasValidSave = false;
            
            if (hasSelection && _saveManager != null)
            {
                var slots = _saveManager.GetSaveSlots();
                if (_selectedSlotIndex < slots.Count)
                {
                    hasValidSave = slots[_selectedSlotIndex].Status != SaveSlotStatus.Empty;
                }
            }
            
            if (_saveButton != null)
                _saveButton.Disabled = !hasSelection;
                
            if (_loadButton != null)
                _loadButton.Disabled = !hasValidSave;
                
            if (_deleteButton != null)
                _deleteButton.Disabled = !hasValidSave;
        }
        
        /// <summary>
        /// 保存按钮点击
        /// </summary>
        private void OnSaveButtonPressed()
        {
            if (_selectedSlotIndex < 0 || _saveManager == null) return;
            
            string saveName = _saveNameInput?.Text ?? "";
            if (string.IsNullOrWhiteSpace(saveName))
            {
                saveName = $"存档 {DateTime.Now:yyyy-MM-dd HH:mm}";
            }
            
            // 检查是否覆盖现有存档
            var slots = _saveManager.GetSaveSlots();
            if (_selectedSlotIndex < slots.Count && slots[_selectedSlotIndex].Status != SaveSlotStatus.Empty)
            {
                ShowConfirmDialog("确定要覆盖现有存档吗？", () => PerformSave(saveName));
            }
            else
            {
                PerformSave(saveName);
            }
        }
        
        /// <summary>
        /// 执行保存
        /// </summary>
        private void PerformSave(string saveName)
        {
            if (_statusLabel != null)
                _statusLabel.Text = "正在保存...";
                
            _saveManager.SaveGameToSlot(_selectedSlotIndex, saveName);
        }
        
        /// <summary>
        /// 读档按钮点击
        /// </summary>
        private void OnLoadButtonPressed()
        {
            if (_selectedSlotIndex < 0 || _saveManager == null) return;
            
            ShowConfirmDialog("确定要加载此存档吗？当前进度将会丢失。", () => PerformLoad());
        }
        
        /// <summary>
        /// 执行读档
        /// </summary>
        private void PerformLoad()
        {
            if (_statusLabel != null)
                _statusLabel.Text = "正在加载...";
                
            _saveManager.LoadGameFromSlot(_selectedSlotIndex);
        }
        
        /// <summary>
        /// 删除按钮点击
        /// </summary>
        private void OnDeleteButtonPressed()
        {
            if (_selectedSlotIndex < 0 || _saveManager == null) return;
            
            ShowConfirmDialog("确定要删除此存档吗？此操作无法撤销。", () => PerformDelete());
        }
        
        /// <summary>
        /// 执行删除
        /// </summary>
        private void PerformDelete()
        {
            var result = _saveManager.DeleteSaveSlot(_selectedSlotIndex);
            
            if (result.Success)
            {
                RefreshSaveSlots();
                _selectedSlotIndex = -1;
                UpdateButtonStates();
                
                if (_statusLabel != null)
                    _statusLabel.Text = "存档删除成功";
            }
            else
            {
                ShowMessageDialog($"删除失败: {result.Message}");
            }
        }
        
        /// <summary>
        /// 返回按钮点击
        /// </summary>
        private void OnBackButtonPressed()
        {
            HideInterface();
            EmitSignal(SignalName.SaveSystemClosed);
        }
        
        /// <summary>
        /// 保存完成事件
        /// </summary>
        private void OnSaveCompleted(SaveOperationResult result)
        {
            if (result.Success)
            {
                RefreshSaveSlots();
                if (_statusLabel != null)
                    _statusLabel.Text = "保存成功";
                    
                if (_saveNameInput != null)
                    _saveNameInput.Text = "";
            }
            else
            {
                if (_statusLabel != null)
                    _statusLabel.Text = "保存失败";
                    
                ShowMessageDialog($"保存失败: {result.Message}");
            }
        }
        
        /// <summary>
        /// 读档完成事件
        /// </summary>
        private void OnLoadCompleted(LoadOperationResult result)
        {
            if (result.Success)
            {
                if (_statusLabel != null)
                    _statusLabel.Text = result.UsedBackup ? "从备份加载成功" : "加载成功";
                    
                EmitSignal(SignalName.GameLoaded, _selectedSlotIndex);
                HideInterface();
            }
            else
            {
                if (_statusLabel != null)
                    _statusLabel.Text = "加载失败";
                    
                ShowMessageDialog($"加载失败: {result.Message}");
            }
        }
        
        /// <summary>
        /// 显示确认对话框
        /// </summary>
        private void ShowConfirmDialog(string message, Action onConfirm)
        {
            if (_confirmDialog != null)
            {
                _confirmDialog.DialogText = message;
                _confirmDialog.PopupCentered();
                
                // 断开之前的连接
                if (_confirmDialog.IsConnected(ConfirmationDialog.SignalName.Confirmed, Callable.From(onConfirm)))
                {
                    _confirmDialog.Disconnect(ConfirmationDialog.SignalName.Confirmed, Callable.From(onConfirm));
                }
                
                _confirmDialog.Confirmed += onConfirm;
            }
        }
        
        /// <summary>
        /// 显示消息对话框
        /// </summary>
        private void ShowMessageDialog(string message)
        {
            if (_messageDialog != null)
            {
                _messageDialog.DialogText = message;
                _messageDialog.PopupCentered();
            }
        }
    }
}