using Godot;

namespace CodeRogue.Data
{
    /// <summary>
    /// 存档系统相关枚举定义
    /// </summary>
    
    /// <summary>
    /// 存档操作结果
    /// </summary>
    public enum SaveResult
    {
        Success,        // 成功
        FileError,      // 文件错误
        DataError,      // 数据错误
        PermissionError,// 权限错误
        CorruptedData,  // 数据损坏
        VersionMismatch,// 版本不匹配
        Unknown         // 未知错误
    }
    
    /// <summary>
    /// 读档操作结果
    /// </summary>
    public enum LoadResult
    {
        Success,        // 成功
        FileNotFound,   // 文件不存在
        FileError,      // 文件错误
        DataError,      // 数据错误
        CorruptedData,  // 数据损坏
        VersionMismatch,// 版本不匹配
        BackupRestored, // 从备份恢复
        Unknown         // 未知错误
    }
    
    /// <summary>
    /// 存档槽位状态
    /// </summary>
    public enum SaveSlotStatus
    {
        Empty,          // 空槽位
        Valid,          // 有效存档
        Corrupted,      // 损坏存档
        Outdated        // 过期存档
    }
    
    /// <summary>
    /// 数据验证结果
    /// </summary>
    public enum ValidationResult
    {
        Valid,          // 数据有效
        Invalid,        // 数据无效
        InvalidFormat,  // 格式无效
        InvalidRange,   // 数值范围无效
        MissingFields,  // 缺少字段
        CorruptedData   // 数据损坏
    }
    
    /// <summary>
    /// 存档版本兼容性
    /// </summary>
    public enum VersionCompatibility
    {
        Unknown,        // 未知
        Compatible,     // 兼容
        PartiallyCompatible, // 部分兼容
        NeedsMigration, // 需要迁移
        Incompatible    // 不兼容
    }
    
    /// <summary>
    /// 备份类型
    /// </summary>
    public enum BackupType
    {
        Auto,           // 自动备份
        Manual,         // 手动备份
        Emergency,      // 紧急备份
        BeforeSave,     // 保存前备份
        BeforeLoad,     // 加载前备份
        BeforeRestore   // 恢复前备份
    }
}