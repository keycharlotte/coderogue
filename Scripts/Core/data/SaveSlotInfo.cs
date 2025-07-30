using System;
using Godot;
using Godot.Collections;

namespace CodeRogue.Data
{
    /// <summary>
    /// 存档槽位信息 - 管理单个存档槽位的信息
    /// </summary>
    [System.Serializable]
    public partial class SaveSlotInfo : Resource
    {
        [Export] public int SlotIndex { get; set; }
        [Export] public SaveSlotStatus Status { get; set; } = SaveSlotStatus.Empty;
        [Export] public SaveMetadata Metadata { get; set; }
        [Export] public string SaveFilePath { get; set; } = "";
        [Export] public string BackupFilePath { get; set; } = "";
        [Export] public bool HasBackup { get; set; } = false;
        [Export] public long FileSize { get; set; } = 0;
        
        public SaveSlotInfo()
        {
        }
        
        public SaveSlotInfo(int slotIndex)
        {
            SlotIndex = slotIndex;
            SaveFilePath = $"user://saves/slot{slotIndex}/gamedata.save";
            BackupFilePath = $"user://saves/slot{slotIndex}/gamedata.backup";
        }
        
        /// <summary>
        /// 检查存档文件是否存在
        /// </summary>
        public bool SaveFileExists()
        {
            return FileAccess.FileExists(SaveFilePath);
        }
        
        /// <summary>
        /// 检查备份文件是否存在
        /// </summary>
        public bool BackupFileExists()
        {
            return FileAccess.FileExists(BackupFilePath);
        }
        
        /// <summary>
        /// 更新文件大小信息
        /// </summary>
        public void UpdateFileSize()
        {
            if (SaveFileExists())
            {
                var file = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    FileSize = (long)file.GetLength();
                    file.Close();
                }
            }
            else
            {
                FileSize = 0;
            }
        }
        
        /// <summary>
        /// 获取格式化的文件大小
        /// </summary>
        public string GetFormattedFileSize()
        {
            if (FileSize < 1024)
                return $"{FileSize} B";
            else if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024.0:F1} KB";
            else
                return $"{FileSize / (1024.0 * 1024.0):F1} MB";
        }
        
        /// <summary>
        /// 检查槽位状态
        /// </summary>
        public void RefreshStatus()
        {
            if (!SaveFileExists())
            {
                Status = SaveSlotStatus.Empty;
                return;
            }
            
            // 这里可以添加更复杂的验证逻辑
            // 比如检查文件完整性、版本兼容性等
            Status = SaveSlotStatus.Valid;
            UpdateFileSize();
            HasBackup = BackupFileExists();
        }
    }
}