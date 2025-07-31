using System;

namespace CodeRogue.Core
{
    /// <summary>
    /// 备份统计信息
    /// </summary>
    public class BackupStatistics
    {
        public int TotalBackups { get; set; }
        public long TotalSize { get; set; }
        public int ManualBackups { get; set; }
        public int AutoBackups { get; set; }
        public DateTime OldestBackup { get; set; }
        public DateTime NewestBackup { get; set; }
    }
}