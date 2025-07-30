using System;
using Godot;
using CodeRogue.Data;

namespace CodeRogue.Core
{
    /// <summary>
    /// 存档系统配置管理器 - 管理存档系统的各种设置和配置
    /// </summary>
    public partial class SaveSystemConfig : Node
    {
        private const string CONFIG_FILE_PATH = "user://save_system_config.cfg";
        
        [Export] public bool EnableAutoSave { get; set; } = true;
        [Export] public int AutoSaveInterval { get; set; } = 300; // 5分钟
        [Export] public bool EnableBackup { get; set; } = true;
        [Export] public int MaxBackupsPerSlot { get; set; } = 3;
        [Export] public bool EnableCompression { get; set; } = true;
        [Export] public bool EnableEncryption { get; set; } = false;
        [Export] public bool ShowSaveNotifications { get; set; } = true;
        [Export] public bool ShowLoadNotifications { get; set; } = true;
        [Export] public bool ConfirmOverwrite { get; set; } = true;
        [Export] public bool ConfirmDelete { get; set; } = true;
        [Export] public int MaxSaveSlots { get; set; } = 10;
        [Export] public long MaxSaveFileSize { get; set; } = 10485760; // 10MB
        [Export] public bool EnableCloudSync { get; set; } = false;
        [Export] public string CloudSyncProvider { get; set; } = "";
        
        [Signal]
        public delegate void ConfigChangedEventHandler(string configKey, Variant newValue);
        
        public override void _Ready()
        {
            LoadConfig();
        }
        
        /// <summary>
        /// 加载配置
        /// </summary>
        public void LoadConfig()
        {
            var config = new ConfigFile();
            var error = config.Load(CONFIG_FILE_PATH);
            
            if (error != Error.Ok)
            {
                GD.Print("SaveSystemConfig: 配置文件不存在，使用默认配置");
                SaveConfig(); // 创建默认配置文件
                return;
            }
            
            // 加载各项配置
            EnableAutoSave = config.GetValue("general", "enable_auto_save", true).AsBool();
            AutoSaveInterval = config.GetValue("general", "auto_save_interval", 300).AsInt32();
            EnableBackup = config.GetValue("general", "enable_backup", true).AsBool();
            MaxBackupsPerSlot = config.GetValue("general", "max_backups_per_slot", 3).AsInt32();
            EnableCompression = config.GetValue("general", "enable_compression", true).AsBool();
            EnableEncryption = config.GetValue("general", "enable_encryption", false).AsBool();
            
            ShowSaveNotifications = config.GetValue("ui", "show_save_notifications", true).AsBool();
            ShowLoadNotifications = config.GetValue("ui", "show_load_notifications", true).AsBool();
            ConfirmOverwrite = config.GetValue("ui", "confirm_overwrite", true).AsBool();
            ConfirmDelete = config.GetValue("ui", "confirm_delete", true).AsBool();
            
            MaxSaveSlots = config.GetValue("limits", "max_save_slots", 10).AsInt32();
            MaxSaveFileSize = config.GetValue("limits", "max_save_file_size", 10485760).AsInt64();
            
            EnableCloudSync = config.GetValue("cloud", "enable_cloud_sync", false).AsBool();
            CloudSyncProvider = config.GetValue("cloud", "cloud_sync_provider", "").AsString();
            
            GD.Print("SaveSystemConfig: 配置加载完成");
        }
        
        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveConfig()
        {
            var config = new ConfigFile();
            
            // 保存各项配置
            config.SetValue("general", "enable_auto_save", EnableAutoSave);
            config.SetValue("general", "auto_save_interval", AutoSaveInterval);
            config.SetValue("general", "enable_backup", EnableBackup);
            config.SetValue("general", "max_backups_per_slot", MaxBackupsPerSlot);
            config.SetValue("general", "enable_compression", EnableCompression);
            config.SetValue("general", "enable_encryption", EnableEncryption);
            
            config.SetValue("ui", "show_save_notifications", ShowSaveNotifications);
            config.SetValue("ui", "show_load_notifications", ShowLoadNotifications);
            config.SetValue("ui", "confirm_overwrite", ConfirmOverwrite);
            config.SetValue("ui", "confirm_delete", ConfirmDelete);
            
            config.SetValue("limits", "max_save_slots", MaxSaveSlots);
            config.SetValue("limits", "max_save_file_size", MaxSaveFileSize);
            
            config.SetValue("cloud", "enable_cloud_sync", EnableCloudSync);
            config.SetValue("cloud", "cloud_sync_provider", CloudSyncProvider);
            
            var error = config.Save(CONFIG_FILE_PATH);
            if (error != Error.Ok)
            {
                GD.PrintErr($"SaveSystemConfig: 保存配置失败 - {error}");
            }
            else
            {
                GD.Print("SaveSystemConfig: 配置保存完成");
            }
        }
        
        /// <summary>
        /// 设置自动存档启用状态
        /// </summary>
        public void SetAutoSaveEnabled(bool enabled)
        {
            if (EnableAutoSave != enabled)
            {
                EnableAutoSave = enabled;
                EmitSignal(SignalName.ConfigChanged, "enable_auto_save", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置自动存档间隔
        /// </summary>
        public void SetAutoSaveInterval(int interval)
        {
            if (interval < 60) interval = 60; // 最小1分钟
            if (interval > 3600) interval = 3600; // 最大1小时
            
            if (AutoSaveInterval != interval)
            {
                AutoSaveInterval = interval;
                EmitSignal(SignalName.ConfigChanged, "auto_save_interval", interval);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置备份启用状态
        /// </summary>
        public void SetBackupEnabled(bool enabled)
        {
            if (EnableBackup != enabled)
            {
                EnableBackup = enabled;
                EmitSignal(SignalName.ConfigChanged, "enable_backup", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置每槽位最大备份数
        /// </summary>
        public void SetMaxBackupsPerSlot(int maxBackups)
        {
            if (maxBackups < 1) maxBackups = 1;
            if (maxBackups > 10) maxBackups = 10;
            
            if (MaxBackupsPerSlot != maxBackups)
            {
                MaxBackupsPerSlot = maxBackups;
                EmitSignal(SignalName.ConfigChanged, "max_backups_per_slot", maxBackups);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置压缩启用状态
        /// </summary>
        public void SetCompressionEnabled(bool enabled)
        {
            if (EnableCompression != enabled)
            {
                EnableCompression = enabled;
                EmitSignal(SignalName.ConfigChanged, "enable_compression", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置加密启用状态
        /// </summary>
        public void SetEncryptionEnabled(bool enabled)
        {
            if (EnableEncryption != enabled)
            {
                EnableEncryption = enabled;
                EmitSignal(SignalName.ConfigChanged, "enable_encryption", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置保存通知显示状态
        /// </summary>
        public void SetSaveNotificationsEnabled(bool enabled)
        {
            if (ShowSaveNotifications != enabled)
            {
                ShowSaveNotifications = enabled;
                EmitSignal(SignalName.ConfigChanged, "show_save_notifications", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置加载通知显示状态
        /// </summary>
        public void SetLoadNotificationsEnabled(bool enabled)
        {
            if (ShowLoadNotifications != enabled)
            {
                ShowLoadNotifications = enabled;
                EmitSignal(SignalName.ConfigChanged, "show_load_notifications", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置覆盖确认状态
        /// </summary>
        public void SetConfirmOverwriteEnabled(bool enabled)
        {
            if (ConfirmOverwrite != enabled)
            {
                ConfirmOverwrite = enabled;
                EmitSignal(SignalName.ConfigChanged, "confirm_overwrite", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置删除确认状态
        /// </summary>
        public void SetConfirmDeleteEnabled(bool enabled)
        {
            if (ConfirmDelete != enabled)
            {
                ConfirmDelete = enabled;
                EmitSignal(SignalName.ConfigChanged, "confirm_delete", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置最大存档槽位数
        /// </summary>
        public void SetMaxSaveSlots(int maxSlots)
        {
            if (maxSlots < 1) maxSlots = 1;
            if (maxSlots > 50) maxSlots = 50;
            
            if (MaxSaveSlots != maxSlots)
            {
                MaxSaveSlots = maxSlots;
                EmitSignal(SignalName.ConfigChanged, "max_save_slots", maxSlots);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置最大存档文件大小
        /// </summary>
        public void SetMaxSaveFileSize(long maxSize)
        {
            if (maxSize < 1048576) maxSize = 1048576; // 最小1MB
            if (maxSize > 104857600) maxSize = 104857600; // 最大100MB
            
            if (MaxSaveFileSize != maxSize)
            {
                MaxSaveFileSize = maxSize;
                EmitSignal(SignalName.ConfigChanged, "max_save_file_size", maxSize);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置云同步启用状态
        /// </summary>
        public void SetCloudSyncEnabled(bool enabled)
        {
            if (EnableCloudSync != enabled)
            {
                EnableCloudSync = enabled;
                EmitSignal(SignalName.ConfigChanged, "enable_cloud_sync", enabled);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 设置云同步提供商
        /// </summary>
        public void SetCloudSyncProvider(string provider)
        {
            if (CloudSyncProvider != provider)
            {
                CloudSyncProvider = provider;
                EmitSignal(SignalName.ConfigChanged, "cloud_sync_provider", provider);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public void ResetToDefaults()
        {
            EnableAutoSave = true;
            AutoSaveInterval = 300;
            EnableBackup = true;
            MaxBackupsPerSlot = 3;
            EnableCompression = true;
            EnableEncryption = false;
            ShowSaveNotifications = true;
            ShowLoadNotifications = true;
            ConfirmOverwrite = true;
            ConfirmDelete = true;
            MaxSaveSlots = 10;
            MaxSaveFileSize = 10485760;
            EnableCloudSync = false;
            CloudSyncProvider = "";
            
            SaveConfig();
            GD.Print("SaveSystemConfig: 已重置为默认配置");
        }
        
        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public ValidationResult ValidateConfig()
        {
            var issues = new Godot.Collections.Array<string>();
            
            if (AutoSaveInterval < 60 || AutoSaveInterval > 3600)
            {
                issues.Add("自动存档间隔应在60-3600秒之间");
            }
            
            if (MaxBackupsPerSlot < 1 || MaxBackupsPerSlot > 10)
            {
                issues.Add("每槽位最大备份数应在1-10之间");
            }
            
            if (MaxSaveSlots < 1 || MaxSaveSlots > 50)
            {
                issues.Add("最大存档槽位数应在1-50之间");
            }
            
            if (MaxSaveFileSize < 1048576 || MaxSaveFileSize > 104857600)
            {
                issues.Add("最大存档文件大小应在1MB-100MB之间");
            }
            
            if (EnableCloudSync && string.IsNullOrWhiteSpace(CloudSyncProvider))
            {
                issues.Add("启用云同步时必须指定提供商");
            }
            
            return issues.Count == 0 ? ValidationResult.Valid : ValidationResult.Invalid;
        }
        
        /// <summary>
        /// 获取配置摘要
        /// </summary>
        public string GetConfigSummary()
        {
            return $"自动存档: {(EnableAutoSave ? "启用" : "禁用")} ({AutoSaveInterval}秒)\n" +
                   $"备份: {(EnableBackup ? "启用" : "禁用")} (每槽位{MaxBackupsPerSlot}个)\n" +
                   $"压缩: {(EnableCompression ? "启用" : "禁用")}\n" +
                   $"加密: {(EnableEncryption ? "启用" : "禁用")}\n" +
                   $"存档槽位: {MaxSaveSlots}个\n" +
                   $"最大文件大小: {MaxSaveFileSize / 1048576}MB\n" +
                   $"云同步: {(EnableCloudSync ? "启用" : "禁用")} ({CloudSyncProvider})";
        }
    }
}