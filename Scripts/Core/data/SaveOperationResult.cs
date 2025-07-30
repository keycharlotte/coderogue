using System;
using Godot;

namespace CodeRogue.Data
{
    /// <summary>
    /// 存档操作结果 - 包含操作结果和详细信息
    /// </summary>
    [System.Serializable]
    public partial class SaveOperationResult : Resource
    {
        [Export] public SaveResult Result { get; set; } = SaveResult.Unknown;
        [Export] public string Message { get; set; } = "";
        [Export] public string ErrorDetails { get; set; } = "";
        public bool Success => Result == SaveResult.Success;
        [Export] public long OperationTime { get; set; } = 0;
        [Export] public string FilePath { get; set; } = "";
        [Export] public long FileSize { get; set; } = 0;
        
        public SaveOperationResult()
        {
        }
        
        public SaveOperationResult(SaveResult result, string message = "")
        {
            Result = result;
            Message = message;
            OperationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static SaveOperationResult CreateSuccess(string message = "保存成功", string filePath = "")
        {
            return new SaveOperationResult(SaveResult.Success, message)
            {
                FilePath = filePath
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static SaveOperationResult CreateFailure(SaveResult result, string message, string errorDetails = "")
        {
            return new SaveOperationResult(result, message)
            {
                ErrorDetails = errorDetails
            };
        }
        
        /// <summary>
        /// 获取本地化的错误消息
        /// </summary>
        public string GetLocalizedMessage()
        {
            return Result switch
            {
                SaveResult.Success => "操作成功",
                SaveResult.FileError => "文件操作失败",
                SaveResult.DataError => "数据格式错误",
                SaveResult.PermissionError => "权限不足",
                SaveResult.CorruptedData => "数据已损坏",
                SaveResult.VersionMismatch => "版本不匹配",
                _ => "未知错误"
            };
        }
    }
    
    /// <summary>
    /// 读档操作结果 - 包含读档结果和详细信息
    /// </summary>
    [System.Serializable]
    public partial class LoadOperationResult : Resource
    {
        [Export] public LoadResult Result { get; set; } = LoadResult.Unknown;
        [Export] public string Message { get; set; } = "";
        [Export] public string ErrorDetails { get; set; } = "";
        public bool Success => Result == LoadResult.Success || Result == LoadResult.BackupRestored;
        [Export] public long OperationTime { get; set; } = 0;
        [Export] public string FilePath { get; set; } = "";
        public bool UsedBackup => Result == LoadResult.BackupRestored;
        [Export] public ExtendedGameData LoadedData { get; set; }
        
        public LoadOperationResult()
        {
        }
        
        public LoadOperationResult(LoadResult result, string message = "")
        {
            Result = result;
            Message = message;
            OperationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static LoadOperationResult CreateSuccess(ExtendedGameData data, string message = "读取成功", string filePath = "")
        {
            return new LoadOperationResult(LoadResult.Success, message)
            {
                LoadedData = data,
                FilePath = filePath
            };
        }
        
        /// <summary>
        /// 创建备份恢复结果
        /// </summary>
        public static LoadOperationResult CreateBackupRestored(ExtendedGameData data, string message = "从备份恢复", string filePath = "")
        {
            return new LoadOperationResult(LoadResult.BackupRestored, message)
            {
                LoadedData = data,
                FilePath = filePath
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static LoadOperationResult CreateFailure(LoadResult result, string message, string errorDetails = "")
        {
            return new LoadOperationResult(result, message)
            {
                ErrorDetails = errorDetails
            };
        }
        
        /// <summary>
        /// 获取本地化的错误消息
        /// </summary>
        public string GetLocalizedMessage()
        {
            return Result switch
            {
                LoadResult.Success => "读取成功",
                LoadResult.FileNotFound => "存档文件不存在",
                LoadResult.FileError => "文件读取失败",
                LoadResult.DataError => "数据格式错误",
                LoadResult.CorruptedData => "存档数据已损坏",
                LoadResult.VersionMismatch => "存档版本不兼容",
                LoadResult.BackupRestored => "主存档损坏，已从备份恢复",
                _ => "未知错误"
            };
        }
    }
}