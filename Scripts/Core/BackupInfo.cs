using System;
using CodeRogue.Data;

namespace CodeRogue.Core
{
    /// <summary>
    /// 备份信息
    /// </summary>
    public class BackupInfo
    {
        public string FilePath { get; set; }
        public BackupType BackupType { get; set; }
        public DateTime Timestamp { get; set; }
        public long FileSize { get; set; }
        public string DisplayName { get; set; }
    }
}