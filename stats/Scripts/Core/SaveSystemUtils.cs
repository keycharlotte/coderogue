using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Godot;
using CodeRogue.Data;
using GodotFileAccess = Godot.FileAccess;

namespace CodeRogue.Core
{
    /// <summary>
    /// 存档系统工具类 - 提供各种辅助功能
    /// </summary>
    public static class SaveSystemUtils
    {
        private const string ENCRYPTION_KEY = "CodeRogue_SaveSystem_EncryptionKey_2023";
        
        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        public static string GetSaveFilePath(int slotIndex)
        {
            return $"user://saves/slot_{slotIndex:D2}.save";
        }
        
        /// <summary>
        /// 获取备份文件路径
        /// </summary>
        public static string GetBackupFilePath(int slotIndex, BackupType backupType, DateTime timestamp)
        {
            string backupFolder = $"user://backups/slot_{slotIndex:D2}";
            string backupFileName = $"{backupType.ToString().ToLower()}_{timestamp:yyyyMMdd_HHmmss}.backup";
            return $"{backupFolder}/{backupFileName}";
        }
        
        /// <summary>
        /// 获取自动存档文件路径
        /// </summary>
        public static string GetAutoSaveFilePath()
        {
            return "user://saves/autosave.save";
        }
        
        /// <summary>
        /// 确保目录存在
        /// </summary>
        public static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        
        /// <summary>
        /// 计算文件校验和
        /// </summary>
        public static string CalculateChecksum(string filePath)
        {
            if (!GodotFileAccess.FileExists(filePath))
            {
                return string.Empty;
            }
            
            using var file = GodotFileAccess.Open(filePath, GodotFileAccess.ModeFlags.Read);
            if (file == null)
            {
                return string.Empty;
            }
            
            byte[] fileData = file.GetBuffer((long)file.GetLength());
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(fileData);
            
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// 验证文件校验和
        /// </summary>
        public static bool ValidateChecksum(string filePath, string expectedChecksum)
        {
            if (string.IsNullOrEmpty(expectedChecksum))
            {
                return false;
            }
            
            string actualChecksum = CalculateChecksum(filePath);
            return string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// 压缩数据
        /// </summary>
        public static byte[] CompressData(byte[] data)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(data, 0, data.Length);
            }
            
            return memoryStream.ToArray();
        }
        
        /// <summary>
        /// 解压数据
        /// </summary>
        public static byte[] DecompressData(byte[] compressedData)
        {
            using var compressedStream = new MemoryStream(compressedData);
            using var decompressedStream = new MemoryStream();
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(decompressedStream);
            }
            
            return decompressedStream.ToArray();
        }
        
        /// <summary>
        /// 加密数据
        /// </summary>
        public static byte[] EncryptData(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(ENCRYPTION_KEY);
            aes.IV = new byte[16]; // 使用全零IV，简化处理
            
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
            }
            
            return memoryStream.ToArray();
        }
        
        /// <summary>
        /// 解密数据
        /// </summary>
        public static byte[] DecryptData(byte[] encryptedData)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(ENCRYPTION_KEY);
            aes.IV = new byte[16]; // 使用全零IV，简化处理
            
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(new MemoryStream(encryptedData), decryptor, CryptoStreamMode.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }
            }
            
            return memoryStream.ToArray();
        }
        
        /// <summary>
        /// 从密码派生密钥
        /// </summary>
        private static byte[] DeriveKeyFromPassword(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("CodeRogue_Salt");
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32); // AES-256需要32字节密钥
        }
        
        /// <summary>
        /// 格式化文件大小
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
        
        /// <summary>
        /// 格式化时间戳
        /// </summary>
        public static string FormatTimestamp(DateTime timestamp)
        {
            return timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        /// <summary>
        /// 格式化游戏时长
        /// </summary>
        public static string FormatPlayTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            
            if (time.TotalHours >= 1)
            {
                return $"{time.Hours}小时{time.Minutes}分钟";
            }
            else
            {
                return $"{time.Minutes}分钟{time.Seconds}秒";
            }
        }
        
        /// <summary>
        /// 生成唯一ID
        /// </summary>
        public static string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        public static DateTime GetCurrentTimestamp()
        {
            return DateTime.Now;
        }
        
        /// <summary>
        /// 获取当前游戏版本
        /// </summary>
        public static string GetGameVersion()
        {
            return Engine.GetVersionInfo()["string"].AsString();
        }
        
        /// <summary>
        /// 获取存档版本兼容性
        /// </summary>
        public static VersionCompatibility CheckVersionCompatibility(string saveVersion, string currentVersion)
        {
            if (string.IsNullOrEmpty(saveVersion))
            {
                return VersionCompatibility.Unknown;
            }
            
            if (saveVersion == currentVersion)
            {
                return VersionCompatibility.Compatible;
            }
            
            // 简单版本比较，实际项目中可能需要更复杂的版本比较逻辑
            string[] saveVersionParts = saveVersion.Split('.');
            string[] currentVersionParts = currentVersion.Split('.');
            
            if (saveVersionParts.Length >= 2 && currentVersionParts.Length >= 2)
            {
                // 主版本号不同，不兼容
                if (saveVersionParts[0] != currentVersionParts[0])
                {
                    return VersionCompatibility.Incompatible;
                }
                
                // 次版本号不同，部分兼容
                if (saveVersionParts[1] != currentVersionParts[1])
                {
                    return VersionCompatibility.PartiallyCompatible;
                }
            }
            
            // 默认部分兼容
            return VersionCompatibility.PartiallyCompatible;
        }
        
        /// <summary>
        /// 获取存档文件列表
        /// </summary>
        public static string[] GetSaveFiles()
        {
            string savesDirectory = "user://saves";
            if (!Directory.Exists(savesDirectory))
            {
                return Array.Empty<string>();
            }
            
            return Directory.GetFiles(savesDirectory, "*.save");
        }
        
        /// <summary>
        /// 获取备份文件列表
        /// </summary>
        public static string[] GetBackupFiles(int slotIndex)
        {
            string backupDirectory = $"user://backups/slot_{slotIndex:D2}";
            if (!Directory.Exists(backupDirectory))
            {
                return Array.Empty<string>();
            }
            
            return Directory.GetFiles(backupDirectory, "*.backup");
        }
        
        /// <summary>
        /// 清理旧备份
        /// </summary>
        public static void CleanupOldBackups(int slotIndex, int maxBackups)
        {
            if (maxBackups <= 0)
            {
                return;
            }
            
            string[] backupFiles = GetBackupFiles(slotIndex);
            if (backupFiles.Length <= maxBackups)
            {
                return;
            }
            
            // 按文件创建时间排序
            Array.Sort(backupFiles, (a, b) =>
            {
                DateTime timeA = File.GetCreationTime(a);
                DateTime timeB = File.GetCreationTime(b);
                return timeB.CompareTo(timeA); // 降序，最新的在前
            });
            
            // 删除超出数量限制的旧备份
            for (int i = maxBackups; i < backupFiles.Length; i++)
            {
                try
                {
                    File.Delete(backupFiles[i]);
                    GD.Print($"SaveSystemUtils: 已删除旧备份 - {backupFiles[i]}");
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"SaveSystemUtils: 删除旧备份失败 - {backupFiles[i]}, 错误: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 创建备份
        /// </summary>
        public static bool CreateBackup(string sourceFilePath, string backupFilePath)
        {
            if (!GodotFileAccess.FileExists(sourceFilePath))
            {
                GD.PrintErr($"SaveSystemUtils: 创建备份失败 - 源文件不存在: {sourceFilePath}");
                return false;
            }
            
            try
            {
                EnsureDirectoryExists(backupFilePath);
                File.Copy(sourceFilePath, backupFilePath, true);
                GD.Print($"SaveSystemUtils: 备份创建成功 - {backupFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveSystemUtils: 创建备份失败 - {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 恢复备份
        /// </summary>
        public static bool RestoreBackup(string backupFilePath, string targetFilePath)
        {
            if (!GodotFileAccess.FileExists(backupFilePath))
            {
                GD.PrintErr($"SaveSystemUtils: 恢复备份失败 - 备份文件不存在: {backupFilePath}");
                return false;
            }
            
            try
            {
                EnsureDirectoryExists(targetFilePath);
                File.Copy(backupFilePath, targetFilePath, true);
                GD.Print($"SaveSystemUtils: 备份恢复成功 - {targetFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveSystemUtils: 恢复备份失败 - {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 删除存档文件
        /// </summary>
        public static bool DeleteSaveFile(string filePath)
        {
            if (!GodotFileAccess.FileExists(filePath))
            {
                return true; // 文件不存在，视为删除成功
            }
            
            try
            {
                File.Delete(filePath);
                GD.Print($"SaveSystemUtils: 存档文件删除成功 - {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveSystemUtils: 删除存档文件失败 - {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取本地化消息
        /// </summary>
        public static string GetLocalizedMessage(string messageKey, params object[] args)
        {
            // 实际项目中应使用Godot的翻译系统
            // 这里简单实现，直接返回消息键和参数
            if (args != null && args.Length > 0)
            {
                return string.Format(messageKey, args);
            }
            
            return messageKey;
        }
    }
}