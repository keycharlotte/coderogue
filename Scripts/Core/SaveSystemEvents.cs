using System;
using Godot;
using CodeRogue.Data;

namespace CodeRogue.Core
{
    /// <summary>
    /// 存档系统事件管理器 - 处理存档相关的事件和通知
    /// </summary>
    public partial class SaveSystemEvents : Node
    {
        // 存档操作事件
        [Signal]
        public delegate void SaveStartedEventHandler(int slotIndex, string saveName);
        
        [Signal]
        public delegate void SaveCompletedEventHandler(SaveOperationResult result);
        
        [Signal]
        public delegate void SaveFailedEventHandler(SaveOperationResult result);
        
        [Signal]
        public delegate void LoadStartedEventHandler(int slotIndex);
        
        [Signal]
        public delegate void LoadCompletedEventHandler(LoadOperationResult result);
        
        [Signal]
        public delegate void LoadFailedEventHandler(LoadOperationResult result);
        
        [Signal]
        public delegate void DeleteStartedEventHandler(int slotIndex);
        
        [Signal]
        public delegate void DeleteCompletedEventHandler(int slotIndex, bool success);
        
        [Signal]
        public delegate void DeleteFailedEventHandler(int slotIndex, string error);
        
        // 自动存档事件
        [Signal]
        public delegate void AutoSaveTriggeredEventHandler();
        
        [Signal]
        public delegate void AutoSaveCompletedEventHandler(SaveOperationResult result);
        
        [Signal]
        public delegate void AutoSaveFailedEventHandler(SaveOperationResult result);
        
        // 备份事件
        [Signal]
        public delegate void BackupCreatedEventHandler(int slotIndex, string backupPath);
        
        [Signal]
        public delegate void BackupRestoredEventHandler(int slotIndex, string backupPath);
        
        [Signal]
        public delegate void BackupFailedEventHandler(int slotIndex, string error);
        
        // 验证事件
        [Signal]
        public delegate void ValidationStartedEventHandler(int slotIndex);
        
        [Signal]
        public delegate void ValidationCompletedEventHandler(int slotIndex, ValidationResult result);
        
        [Signal]
        public delegate void ValidationFailedEventHandler(int slotIndex, string error);
        
        // 存档槽位事件
        [Signal]
        public delegate void SlotStatusChangedEventHandler(int slotIndex, SaveSlotStatus oldStatus, SaveSlotStatus newStatus);
        
        [Signal]
        public delegate void SlotInfoUpdatedEventHandler(int slotIndex, SaveSlotInfo slotInfo);
        
        // 系统事件
        [Signal]
        public delegate void SaveSystemInitializedEventHandler();
        
        [Signal]
        public delegate void SaveSystemShutdownEventHandler();
        
        [Signal]
        public delegate void ConfigurationChangedEventHandler(string configKey, Variant newValue);
        
        [Signal]
        public delegate void ErrorOccurredEventHandler(string errorMessage, string details);
        
        [Signal]
        public delegate void WarningOccurredEventHandler(string warningMessage, string details);
        
        // 进度事件
        [Signal]
        public delegate void ProgressUpdatedEventHandler(string operation, float progress, string status);
        
        // 云同步事件
        [Signal]
        public delegate void CloudSyncStartedEventHandler(int slotIndex);
        
        [Signal]
        public delegate void CloudSyncCompletedEventHandler(int slotIndex, bool success);
        
        [Signal]
        public delegate void CloudSyncFailedEventHandler(int slotIndex, string error);
        
        [Signal]
        public delegate void CloudSyncConflictEventHandler(int slotIndex, string localVersion, string cloudVersion);
        
        private static SaveSystemEvents _instance;
        public static SaveSystemEvents Instance => _instance;
        
        public override void _Ready()
        {
            if (_instance == null)
            {
                _instance = this;
                GD.Print("SaveSystemEvents: 事件管理器初始化完成");
            }
            else
            {
                GD.PrintErr("SaveSystemEvents: 实例已存在，删除重复实例");
                QueueFree();
            }
        }
        
        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        /// <summary>
        /// 触发存档开始事件
        /// </summary>
        public void TriggerSaveStarted(int slotIndex, string saveName)
        {
            EmitSignal(SignalName.SaveStarted, slotIndex, saveName);
            GD.Print($"SaveSystemEvents: 存档开始 - 槽位{slotIndex}, 名称: {saveName}");
        }
        
        /// <summary>
        /// 触发存档完成事件
        /// </summary>
        public void TriggerSaveCompleted(SaveOperationResult result)
        {
            EmitSignal(SignalName.SaveCompleted, result);
            GD.Print($"SaveSystemEvents: 存档完成 - {result.Message}");
        }
        
        /// <summary>
        /// 触发存档失败事件
        /// </summary>
        public void TriggerSaveFailed(SaveOperationResult result)
        {
            EmitSignal(SignalName.SaveFailed, result);
            GD.PrintErr($"SaveSystemEvents: 存档失败 - {result.Message}");
        }
        
        /// <summary>
        /// 触发加载开始事件
        /// </summary>
        public void TriggerLoadStarted(int slotIndex)
        {
            EmitSignal(SignalName.LoadStarted, slotIndex);
            GD.Print($"SaveSystemEvents: 加载开始 - 槽位{slotIndex}");
        }
        
        /// <summary>
        /// 触发加载完成事件
        /// </summary>
        public void TriggerLoadCompleted(LoadOperationResult result)
        {
            EmitSignal(SignalName.LoadCompleted, result);
            GD.Print($"SaveSystemEvents: 加载完成 - {result.Message}");
        }
        
        /// <summary>
        /// 触发加载失败事件
        /// </summary>
        public void TriggerLoadFailed(LoadOperationResult result)
        {
            EmitSignal(SignalName.LoadFailed, result);
            GD.PrintErr($"SaveSystemEvents: 加载失败 - {result.Message}");
        }
        
        /// <summary>
        /// 触发删除开始事件
        /// </summary>
        public void TriggerDeleteStarted(int slotIndex)
        {
            EmitSignal(SignalName.DeleteStarted, slotIndex);
            GD.Print($"SaveSystemEvents: 删除开始 - 槽位{slotIndex}");
        }
        
        /// <summary>
        /// 触发删除完成事件
        /// </summary>
        public void TriggerDeleteCompleted(int slotIndex, bool success)
        {
            EmitSignal(SignalName.DeleteCompleted, slotIndex, success);
            GD.Print($"SaveSystemEvents: 删除完成 - 槽位{slotIndex}, 成功: {success}");
        }
        
        /// <summary>
        /// 触发删除失败事件
        /// </summary>
        public void TriggerDeleteFailed(int slotIndex, string error)
        {
            EmitSignal(SignalName.DeleteFailed, slotIndex, error);
            GD.PrintErr($"SaveSystemEvents: 删除失败 - 槽位{slotIndex}, 错误: {error}");
        }
        
        /// <summary>
        /// 触发自动存档触发事件
        /// </summary>
        public void TriggerAutoSaveTriggered()
        {
            EmitSignal(SignalName.AutoSaveTriggered);
            GD.Print("SaveSystemEvents: 自动存档触发");
        }
        
        /// <summary>
        /// 触发自动存档完成事件
        /// </summary>
        public void TriggerAutoSaveCompleted(SaveOperationResult result)
        {
            EmitSignal(SignalName.AutoSaveCompleted, result);
            GD.Print($"SaveSystemEvents: 自动存档完成 - {result.Message}");
        }
        
        /// <summary>
        /// 触发自动存档失败事件
        /// </summary>
        public void TriggerAutoSaveFailed(SaveOperationResult result)
        {
            EmitSignal(SignalName.AutoSaveFailed, result);
            GD.PrintErr($"SaveSystemEvents: 自动存档失败 - {result.Message}");
        }
        
        /// <summary>
        /// 触发备份创建事件
        /// </summary>
        public void TriggerBackupCreated(int slotIndex, string backupPath)
        {
            EmitSignal(SignalName.BackupCreated, slotIndex, backupPath);
            GD.Print($"SaveSystemEvents: 备份创建 - 槽位{slotIndex}, 路径: {backupPath}");
        }
        
        /// <summary>
        /// 触发备份恢复事件
        /// </summary>
        public void TriggerBackupRestored(int slotIndex, string backupPath)
        {
            EmitSignal(SignalName.BackupRestored, slotIndex, backupPath);
            GD.Print($"SaveSystemEvents: 备份恢复 - 槽位{slotIndex}, 路径: {backupPath}");
        }
        
        /// <summary>
        /// 触发备份失败事件
        /// </summary>
        public void TriggerBackupFailed(int slotIndex, string error)
        {
            EmitSignal(SignalName.BackupFailed, slotIndex, error);
            GD.PrintErr($"SaveSystemEvents: 备份失败 - 槽位{slotIndex}, 错误: {error}");
        }
        
        /// <summary>
        /// 触发验证开始事件
        /// </summary>
        public void TriggerValidationStarted(int slotIndex)
        {
            EmitSignal(SignalName.ValidationStarted, slotIndex);
            GD.Print($"SaveSystemEvents: 验证开始 - 槽位{slotIndex}");
        }
        
        /// <summary>
        /// 触发验证完成事件
        /// </summary>
        public void TriggerValidationCompleted(int slotIndex, ValidationResult result)
        {
            EmitSignal(SignalName.ValidationCompleted, slotIndex, (int)result);
            GD.Print($"SaveSystemEvents: 验证完成 - 槽位{slotIndex}, 结果: {result}");
        }
        
        /// <summary>
        /// 触发验证失败事件
        /// </summary>
        public void TriggerValidationFailed(int slotIndex, string error)
        {
            EmitSignal(SignalName.ValidationFailed, slotIndex, error);
            GD.PrintErr($"SaveSystemEvents: 验证失败 - 槽位{slotIndex}, 错误: {error}");
        }
        
        /// <summary>
        /// 触发槽位状态改变事件
        /// </summary>
        public void TriggerSlotStatusChanged(int slotIndex, SaveSlotStatus oldStatus, SaveSlotStatus newStatus)
        {
            EmitSignal(SignalName.SlotStatusChanged, slotIndex, (int)oldStatus, (int)newStatus);
            GD.Print($"SaveSystemEvents: 槽位状态改变 - 槽位{slotIndex}, {oldStatus} -> {newStatus}");
        }
        
        /// <summary>
        /// 触发槽位信息更新事件
        /// </summary>
        public void TriggerSlotInfoUpdated(int slotIndex, SaveSlotInfo slotInfo)
        {
            EmitSignal(SignalName.SlotInfoUpdated, slotIndex, slotInfo);
            GD.Print($"SaveSystemEvents: 槽位信息更新 - 槽位{slotIndex}");
        }
        
        /// <summary>
        /// 触发系统初始化事件
        /// </summary>
        public void TriggerSaveSystemInitialized()
        {
            EmitSignal(SignalName.SaveSystemInitialized);
            GD.Print("SaveSystemEvents: 存档系统初始化完成");
        }
        
        /// <summary>
        /// 触发系统关闭事件
        /// </summary>
        public void TriggerSaveSystemShutdown()
        {
            EmitSignal(SignalName.SaveSystemShutdown);
            GD.Print("SaveSystemEvents: 存档系统关闭");
        }
        
        /// <summary>
        /// 触发配置改变事件
        /// </summary>
        public void TriggerConfigurationChanged(string configKey, Variant newValue)
        {
            EmitSignal(SignalName.ConfigurationChanged, configKey, newValue);
            GD.Print($"SaveSystemEvents: 配置改变 - {configKey}: {newValue}");
        }
        
        /// <summary>
        /// 触发错误事件
        /// </summary>
        public void TriggerError(string errorMessage, string details = "")
        {
            EmitSignal(SignalName.ErrorOccurred, errorMessage, details);
            GD.PrintErr($"SaveSystemEvents: 错误 - {errorMessage}");
            if (!string.IsNullOrEmpty(details))
            {
                GD.PrintErr($"SaveSystemEvents: 错误详情 - {details}");
            }
        }
        
        /// <summary>
        /// 触发警告事件
        /// </summary>
        public void TriggerWarning(string warningMessage, string details = "")
        {
            EmitSignal(SignalName.WarningOccurred, warningMessage, details);
            GD.PrintRich($"[color=yellow]SaveSystemEvents: 警告 - {warningMessage}[/color]");
            if (!string.IsNullOrEmpty(details))
            {
                GD.PrintRich($"[color=yellow]SaveSystemEvents: 警告详情 - {details}[/color]");
            }
        }
        
        /// <summary>
        /// 触发进度更新事件
        /// </summary>
        public void TriggerProgressUpdated(string operation, float progress, string status)
        {
            EmitSignal(SignalName.ProgressUpdated, operation, progress, status);
            GD.Print($"SaveSystemEvents: 进度更新 - {operation}: {progress:P0} - {status}");
        }
        
        /// <summary>
        /// 触发云同步开始事件
        /// </summary>
        public void TriggerCloudSyncStarted(int slotIndex)
        {
            EmitSignal(SignalName.CloudSyncStarted, slotIndex);
            GD.Print($"SaveSystemEvents: 云同步开始 - 槽位{slotIndex}");
        }
        
        /// <summary>
        /// 触发云同步完成事件
        /// </summary>
        public void TriggerCloudSyncCompleted(int slotIndex, bool success)
        {
            EmitSignal(SignalName.CloudSyncCompleted, slotIndex, success);
            GD.Print($"SaveSystemEvents: 云同步完成 - 槽位{slotIndex}, 成功: {success}");
        }
        
        /// <summary>
        /// 触发云同步失败事件
        /// </summary>
        public void TriggerCloudSyncFailed(int slotIndex, string error)
        {
            EmitSignal(SignalName.CloudSyncFailed, slotIndex, error);
            GD.PrintErr($"SaveSystemEvents: 云同步失败 - 槽位{slotIndex}, 错误: {error}");
        }
        
        /// <summary>
        /// 触发云同步冲突事件
        /// </summary>
        public void TriggerCloudSyncConflict(int slotIndex, string localVersion, string cloudVersion)
        {
            EmitSignal(SignalName.CloudSyncConflict, slotIndex, localVersion, cloudVersion);
            GD.PrintRich($"[color=orange]SaveSystemEvents: 云同步冲突 - 槽位{slotIndex}, 本地: {localVersion}, 云端: {cloudVersion}[/color]");
        }
    }
}