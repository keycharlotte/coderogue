using System;
using System.IO;
using System.Linq;
using Godot;
using CodeRogue.Data;
using GodotFileAccess = Godot.FileAccess;

namespace CodeRogue.Core
{
    /// <summary>
    /// 备份管理器 - 处理存档备份功能
    /// </summary>
    public partial class BackupManager : Node
    {
        private SaveSystemConfig _config;
        private SaveSystemEvents _events;
        
        public override void _Ready()
        {
            // 获取依赖的组件
            _config = GetNode<SaveSystemConfig>("/root/SaveSystemConfig");
            _events = GetNode<SaveSystemEvents>("/root/SaveSystemEvents");
            
            if (_config == null || _events == null)
            {
                GD.PrintErr("BackupManager: 无法获取必要的依赖组件");
                return;
            }
            
            GD.Print("BackupManager: 初始化完成");
        }
        
        /// <summary>
        /// 创建存档备份
        /// </summary>
        public bool CreateBackup(int slotIndex, BackupType backupType)
        {
            if (!_config.EnableBackup)
            {
                GD.Print("BackupManager: 备份功能已禁用");
                return false;
            }
            
            string saveFilePath = SaveSystemUtils.GetSaveFilePath(slotIndex);
            if (!GodotFileAccess.FileExists(saveFilePath))
            {
                GD.PrintErr($"BackupManager: 存档文件不存在 - 槽位{slotIndex}");
                return false;
            }
            
            try
            {
                DateTime timestamp = SaveSystemUtils.GetCurrentTimestamp();
                string backupFilePath = SaveSystemUtils.GetBackupFilePath(slotIndex, backupType, timestamp);
                
                // 确保备份目录存在
                SaveSystemUtils.EnsureDirectoryExists(backupFilePath);
                
                // 创建备份
                bool success = SaveSystemUtils.CreateBackup(saveFilePath, backupFilePath);
                
                if (success)
                {
                    _events?.TriggerBackupCreated(slotIndex, backupFilePath);
                    
                    // 清理旧备份
                    CleanupOldBackups(slotIndex);
                    
                    GD.Print($"BackupManager: 备份创建成功 - 槽位{slotIndex}, 类型: {backupType}");
                    return true;
                }
                else
                {
                    _events?.TriggerBackupFailed(slotIndex, "备份文件创建失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"创建备份时发生异常: {ex.Message}";
                _events?.TriggerBackupFailed(slotIndex, errorMessage);
                GD.PrintErr($"BackupManager: {errorMessage}");
                return false;
            }
        }
        
        /// <summary>
        /// 恢复备份
        /// </summary>
        public bool RestoreBackup(int slotIndex, string backupFilePath)
        {
            if (!GodotFileAccess.FileExists(backupFilePath))
            {
                string errorMessage = "备份文件不存在";
                _events?.TriggerBackupFailed(slotIndex, errorMessage);
                GD.PrintErr($"BackupManager: {errorMessage} - {backupFilePath}");
                return false;
            }
            
            try
            {
                string saveFilePath = SaveSystemUtils.GetSaveFilePath(slotIndex);
                
                // 在恢复前创建当前存档的备份
                if (GodotFileAccess.FileExists(saveFilePath))
                {
                    CreateBackup(slotIndex, BackupType.BeforeRestore);
                }
                
                // 恢复备份
                bool success = SaveSystemUtils.RestoreBackup(backupFilePath, saveFilePath);
                
                if (success)
                {
                    _events?.TriggerBackupRestored(slotIndex, backupFilePath);
                    GD.Print($"BackupManager: 备份恢复成功 - 槽位{slotIndex}");
                    return true;
                }
                else
                {
                    _events?.TriggerBackupFailed(slotIndex, "备份恢复失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"恢复备份时发生异常: {ex.Message}";
                _events?.TriggerBackupFailed(slotIndex, errorMessage);
                GD.PrintErr($"BackupManager: {errorMessage}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取备份列表
        /// </summary>
        public BackupInfo[] GetBackupList(int slotIndex)
        {
            string[] backupFiles = SaveSystemUtils.GetBackupFiles(slotIndex);
            var backupInfos = new BackupInfo[backupFiles.Length];
            
            for (int i = 0; i < backupFiles.Length; i++)
            {
                backupInfos[i] = CreateBackupInfo(backupFiles[i]);
            }
            
            // 按时间戳降序排序（最新的在前）
            Array.Sort(backupInfos, (a, b) => b.Timestamp.CompareTo(a.Timestamp));
            
            return backupInfos;
        }
        
        /// <summary>
        /// 创建备份信息
        /// </summary>
        private BackupInfo CreateBackupInfo(string backupFilePath)
        {
            var fileInfo = new FileInfo(backupFilePath);
            string fileName = Path.GetFileNameWithoutExtension(backupFilePath);
            
            // 解析文件名获取备份类型和时间戳
            string[] parts = fileName.Split('_');
            BackupType backupType = BackupType.Manual;
            DateTime timestamp = fileInfo.CreationTime;
            
            if (parts.Length >= 2)
            {
                if (Enum.TryParse<BackupType>(parts[0], true, out BackupType parsedType))
                {
                    backupType = parsedType;
                }
                
                if (parts.Length >= 3 && DateTime.TryParseExact($"{parts[1]}_{parts[2]}", "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
                {
                    timestamp = parsedTime;
                }
            }
            
            return new BackupInfo
            {
                FilePath = backupFilePath,
                BackupType = backupType,
                Timestamp = timestamp,
                FileSize = fileInfo.Length,
                DisplayName = GetBackupDisplayName(backupType, timestamp)
            };
        }
        
        /// <summary>
        /// 获取备份显示名称
        /// </summary>
        private string GetBackupDisplayName(BackupType backupType, DateTime timestamp)
        {
            string typeText = backupType switch
            {
                BackupType.Manual => "手动备份",
                BackupType.Auto => "自动备份",
                BackupType.BeforeSave => "保存前备份",
                BackupType.BeforeLoad => "加载前备份",
                BackupType.BeforeRestore => "恢复前备份",
                _ => "未知备份"
            };
            
            return $"{typeText} - {SaveSystemUtils.FormatTimestamp(timestamp)}";
        }
        
        /// <summary>
        /// 清理旧备份
        /// </summary>
        private void CleanupOldBackups(int slotIndex)
        {
            if (_config.MaxBackupsPerSlot <= 0)
            {
                return;
            }
            
            try
            {
                SaveSystemUtils.CleanupOldBackups(slotIndex, _config.MaxBackupsPerSlot);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"BackupManager: 清理旧备份失败 - {ex.Message}");
            }
        }
        
        /// <summary>
        /// 删除备份
        /// </summary>
        public bool DeleteBackup(string backupFilePath)
        {
            if (!GodotFileAccess.FileExists(backupFilePath))
            {
                return true; // 文件不存在，视为删除成功
            }
            
            try
            {
                File.Delete(backupFilePath);
                GD.Print($"BackupManager: 备份删除成功 - {backupFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"BackupManager: 删除备份失败 - {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 删除所有备份
        /// </summary>
        public bool DeleteAllBackups(int slotIndex)
        {
            string[] backupFiles = SaveSystemUtils.GetBackupFiles(slotIndex);
            bool allSuccess = true;
            
            foreach (string backupFile in backupFiles)
            {
                if (!DeleteBackup(backupFile))
                {
                    allSuccess = false;
                }
            }
            
            if (allSuccess)
            {
                GD.Print($"BackupManager: 所有备份删除成功 - 槽位{slotIndex}");
            }
            else
            {
                GD.PrintErr($"BackupManager: 部分备份删除失败 - 槽位{slotIndex}");
            }
            
            return allSuccess;
        }
        
        /// <summary>
        /// 获取备份统计信息
        /// </summary>
        public BackupStatistics GetBackupStatistics(int slotIndex)
        {
            BackupInfo[] backups = GetBackupList(slotIndex);
            
            var statistics = new BackupStatistics
            {
                TotalBackups = backups.Length,
                TotalSize = backups.Sum(b => b.FileSize),
                ManualBackups = backups.Count(b => b.BackupType == BackupType.Manual),
                AutoBackups = backups.Count(b => b.BackupType == BackupType.Auto),
                OldestBackup = backups.Length > 0 ? backups.Min(b => b.Timestamp) : DateTime.MinValue,
                NewestBackup = backups.Length > 0 ? backups.Max(b => b.Timestamp) : DateTime.MinValue
            };
            
            return statistics;
        }
        
        /// <summary>
        /// 验证备份完整性
        /// </summary>
        public bool ValidateBackup(string backupFilePath)
        {
            if (!GodotFileAccess.FileExists(backupFilePath))
            {
                return false;
            }
            
            try
            {
                // 尝试读取备份文件
                using var file = GodotFileAccess.Open(backupFilePath, GodotFileAccess.ModeFlags.Read);
                if (file == null)
                {
                    return false;
                }
                
                // 检查文件大小
                long fileSize = (long)file.GetLength();
                if (fileSize == 0)
                {
                    return false;
                }
                
                // 尝试读取文件内容
                byte[] data = file.GetBuffer(fileSize);
                if (data == null || data.Length == 0)
                {
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"BackupManager: 验证备份失败 - {ex.Message}");
                return false;
            }
        }
    }
}