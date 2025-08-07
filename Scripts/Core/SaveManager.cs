using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Godot;
using CodeRogue.Data;
using Godot.Collections;
using CodeRogue.Utils;
using GodotFileAccess = Godot.FileAccess;

namespace CodeRogue.Core
{
    /// <summary>
    /// 存档管理器 - 负责游戏存档的保存、加载、验证和备份
    /// </summary>
    public partial class SaveManager : Node
    {
        private const string SAVE_DIRECTORY = "user://saves/";
        private const string BACKUP_DIRECTORY = "user://saves/backups/";
        private const string SAVE_FILE_EXTENSION = ".save";
        private const string BACKUP_FILE_EXTENSION = ".backup";
        private const string CURRENT_SAVE_VERSION = "1.0.0";
        private const int MAX_SAVE_SLOTS = 10;
        private const int MAX_BACKUPS_PER_SLOT = 3;
        public const int AUTO_SAVE_SLOT = -1;
        
        [Export] public int AutoSaveInterval { get; set; } = 300; // 5分钟自动存档
        [Export] public bool EnableAutoSave { get; set; } = true;
        [Export] public bool EnableBackup { get; set; } = true;
        [Export] public bool EnableCompression { get; set; } = true;
        
        private Timer _autoSaveTimer;
        private Array<SaveSlotInfo> _saveSlots = new();
        private GameData _gameData;
        
        [Signal]
        public delegate void SaveCompletedEventHandler(SaveOperationResult result);
        
        [Signal]
        public delegate void LoadCompletedEventHandler(LoadOperationResult result);
        
        [Signal]
        public delegate void AutoSaveTriggeredEventHandler();
        
        public override void _Ready()
        {
            InitializeSaveSystem();
            SetupAutoSave();
            RefreshSaveSlots();
        }
        
        /// <summary>
        /// 初始化存档系统
        /// </summary>
        private void InitializeSaveSystem()
        {
            // 确保存档目录存在
            if (!DirAccess.DirExistsAbsolute(SAVE_DIRECTORY))
            {
                DirAccess.MakeDirRecursiveAbsolute(SAVE_DIRECTORY);
            }
            
            if (!DirAccess.DirExistsAbsolute(BACKUP_DIRECTORY))
            {
                DirAccess.MakeDirRecursiveAbsolute(BACKUP_DIRECTORY);
            }
            
            // 获取GameData引用
            _gameData = NodeUtils.GetGameData(this);
            if (_gameData == null)
            {
                GD.PrintErr("SaveManager: 无法找到GameData单例");
            }
        }
        
        /// <summary>
        /// 设置自动存档
        /// </summary>
        private void SetupAutoSave()
        {
            _autoSaveTimer = new Timer();
            _autoSaveTimer.WaitTime = AutoSaveInterval;
            _autoSaveTimer.Autostart = EnableAutoSave;
            _autoSaveTimer.Timeout += OnAutoSaveTimeout;
            AddChild(_autoSaveTimer);
        }
        
        /// <summary>
        /// 自动存档触发
        /// </summary>
        private void OnAutoSaveTimeout()
        {
            if (EnableAutoSave && _gameData != null)
            {
                EmitSignal(SignalName.AutoSaveTriggered);
                var result = SaveGameToSlot(0, "自动存档", true);
                GD.Print($"自动存档完成: {result.Message}");
            }
        }
        
        /// <summary>
        /// 异步保存游戏到指定槽位
        /// </summary>
        public async Task<SaveOperationResult> SaveGameToSlotAsync(int slotIndex, string saveName = "", bool isAutoSave = false)
        {
            return await Task.Run(() => SaveGameToSlot(slotIndex, saveName, isAutoSave));
        }
        
        /// <summary>
        /// 保存游戏到指定槽位
        /// </summary>
        public SaveOperationResult SaveGameToSlot(int slotIndex, string saveName = "", bool isAutoSave = false)
        {
            try
            {
                if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
                {
                    return SaveOperationResult.CreateFailure(SaveResult.DataError, "无效的存档槽位");
                }
                
                if (_gameData == null)
                {
                    return SaveOperationResult.CreateFailure(SaveResult.DataError, "游戏数据未初始化");
                }
                
                // 创建扩展游戏数据
                var extendedData = CreateExtendedGameData();
                
                // 创建存档元数据
                var metadata = CreateSaveMetadata(saveName, isAutoSave);
                
                // 生成存档文件路径
                string saveFilePath = GetSaveFilePath(slotIndex);
                
                // 备份现有存档
                if (EnableBackup && GodotFileAccess.FileExists(saveFilePath))
                {
                    CreateBackup(slotIndex);
                }
                
                // 保存数据
                var saveResult = SaveDataToFile(saveFilePath, extendedData, metadata);
                
                if (saveResult.Success)
                {
                    // 更新槽位信息
                    RefreshSaveSlot(slotIndex);
                    EmitSignal(SignalName.SaveCompleted, saveResult);
                }
                
                return saveResult;
            }
            catch (Exception ex)
            {
                var errorResult = SaveOperationResult.CreateFailure(SaveResult.FileError, "保存失败", ex.Message);
                EmitSignal(SignalName.SaveCompleted, errorResult);
                return errorResult;
            }
        }
        
        /// <summary>
        /// 从指定槽位加载游戏
        /// </summary>
        public LoadOperationResult LoadGameFromSlot(int slotIndex)
        {
            try
            {
                if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
                {
                    return LoadOperationResult.CreateFailure(LoadResult.DataError, "无效的存档槽位");
                }
                
                string saveFilePath = GetSaveFilePath(slotIndex);
                
                if (!GodotFileAccess.FileExists(saveFilePath))
                {
                    return LoadOperationResult.CreateFailure(LoadResult.FileNotFound, "存档文件不存在");
                }
                
                // 尝试加载主存档
                var loadResult = LoadDataFromFile(saveFilePath);
                
                // 如果主存档损坏，尝试从备份恢复
                if (!loadResult.Success && EnableBackup)
                {
                    loadResult = TryRestoreFromBackup(slotIndex);
                }
                
                if (loadResult.Success && loadResult.LoadedData != null)
                {
                    // 应用加载的数据到GameData
                    ApplyLoadedDataToGameData(loadResult.LoadedData);
                }
                
                EmitSignal(SignalName.LoadCompleted, loadResult);
                return loadResult;
            }
            catch (Exception ex)
            {
                var errorResult = LoadOperationResult.CreateFailure(LoadResult.FileError, "加载失败", ex.Message);
                EmitSignal(SignalName.LoadCompleted, errorResult);
                return errorResult;
            }
        }
        
        /// <summary>
        /// 删除指定槽位的存档
        /// </summary>
        public SaveOperationResult DeleteSaveSlot(int slotIndex)
        {
            try
            {
                if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
                {
                    return SaveOperationResult.CreateFailure(SaveResult.DataError, "无效的存档槽位");
                }
                
                string saveFilePath = GetSaveFilePath(slotIndex);
                
                if (GodotFileAccess.FileExists(saveFilePath))
                {
                    DirAccess.RemoveAbsolute(saveFilePath);
                }
                
                // 删除相关备份
                DeleteBackups(slotIndex);
                
                // 刷新槽位信息
                RefreshSaveSlot(slotIndex);
                
                return SaveOperationResult.CreateSuccess("存档删除成功");
            }
            catch (Exception ex)
            {
                return SaveOperationResult.CreateFailure(SaveResult.FileError, "删除存档失败", ex.Message);
            }
        }
        
        /// <summary>
        /// 获取所有存档槽位信息
        /// </summary>
        public Array<SaveSlotInfo> GetSaveSlots()
        {
            return _saveSlots;
        }
        
        /// <summary>
        /// 刷新所有存档槽位信息
        /// </summary>
        public void RefreshSaveSlots()
        {
            _saveSlots.Clear();
            
            for (int i = 0; i < MAX_SAVE_SLOTS; i++)
            {
                var slotInfo = CreateSaveSlotInfo(i);
                _saveSlots.Add(slotInfo);
            }
        }
        
        /// <summary>
        /// 刷新指定槽位信息
        /// </summary>
        private void RefreshSaveSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _saveSlots.Count)
            {
                _saveSlots[slotIndex] = CreateSaveSlotInfo(slotIndex);
            }
        }
        
        /// <summary>
        /// 创建存档槽位信息
        /// </summary>
        private SaveSlotInfo CreateSaveSlotInfo(int slotIndex)
        {
            var slotInfo = new SaveSlotInfo
            {
                SlotIndex = slotIndex,
                SaveFilePath = GetSaveFilePath(slotIndex),
                BackupFilePath = GetBackupFilePath(slotIndex, 0)
            };
            
            slotInfo.RefreshStatus();
            return slotInfo;
        }
        
        /// <summary>
        /// 创建扩展游戏数据
        /// </summary>
        private ExtendedGameData CreateExtendedGameData()
        {
            var extendedData = new ExtendedGameData();
            
            // 复制基础游戏数据
            extendedData.PlayerLevel = _gameData.PlayerLevel;
            extendedData.PlayerExperience = _gameData.PlayerExperience;
            extendedData.PlayerHealth = _gameData.PlayerHealth;
            extendedData.PlayerMaxHealth = _gameData.PlayerMaxHealth;
            extendedData.MasterVolume = _gameData.MasterVolume;
            extendedData.MusicVolume = _gameData.MusicVolume;
            extendedData.SfxVolume = _gameData.SfxVolume;
            
            // 添加扩展数据（这里可以根据实际需要添加更多数据）
            extendedData.TotalPlayTime = GetTotalPlayTime();
            extendedData.LastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            return extendedData;
        }
        
        /// <summary>
        /// 创建存档元数据
        /// </summary>
        private SaveMetadata CreateSaveMetadata(string saveName, bool isAutoSave)
        {
            return new SaveMetadata
            {
                SaveVersion = CURRENT_SAVE_VERSION,
                SaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                PlayerLevel = _gameData.PlayerLevel,
                CurrentLevel = 1, // 这里需要根据实际游戏状态获取
                TotalPlayTime = GetTotalPlayTime(),
                SaveName = string.IsNullOrEmpty(saveName) ? (isAutoSave ? "自动存档" : $"存档 {DateTime.Now:yyyy-MM-dd HH:mm}") : saveName,
                SaveDescription = isAutoSave ? "系统自动保存" : "手动保存",
                IsAutoSave = isAutoSave
            };
        }
        
        /// <summary>
        /// 保存数据到文件
        /// </summary>
        private SaveOperationResult SaveDataToFile(string filePath, ExtendedGameData gameData, SaveMetadata metadata)
        {
            try
            {
                var saveData = new Dictionary<string, Variant>
                {
                    ["metadata"] = metadata,
                    ["gameData"] = gameData,
                    ["checksum"] = ""
                };
                
                string jsonString = Json.Stringify(saveData);
                
                // 计算校验和
                string checksum = CalculateChecksum(jsonString);
                saveData["checksum"] = checksum;
                metadata.SaveChecksum = checksum;
                
                // 重新序列化包含校验和的数据
                jsonString = Json.Stringify(saveData);
                
                // 压缩数据（如果启用）
                byte[] dataToSave = Encoding.UTF8.GetBytes(jsonString);
                if (EnableCompression)
                {
                    dataToSave = CompressData(dataToSave);
                }
                
                // 写入文件
                using var file = GodotFileAccess.Open(filePath, GodotFileAccess.ModeFlags.Write);
                if (file == null)
                {
                    return SaveOperationResult.CreateFailure(SaveResult.FileError, "无法创建存档文件");
                }
                
                file.StoreBuffer(dataToSave);
                file.Close();
                
                var result = SaveOperationResult.CreateSuccess("保存成功", filePath);
                result.FileSize = (long)dataToSave.Length;
                return result;
            }
            catch (Exception ex)
            {
                return SaveOperationResult.CreateFailure(SaveResult.FileError, "保存文件时发生错误", ex.Message);
            }
        }
        
        /// <summary>
        /// 从文件加载数据
        /// </summary>
        private LoadOperationResult LoadDataFromFile(string filePath)
        {
            try
            {
                using var file = GodotFileAccess.Open(filePath, GodotFileAccess.ModeFlags.Read);
                if (file == null)
                {
                    return LoadOperationResult.CreateFailure(LoadResult.FileError, "无法打开存档文件");
                }
                
                byte[] fileData = file.GetBuffer((long)file.GetLength());
                file.Close();
                
                // 解压数据（如果需要）
                if (EnableCompression)
                {
                    fileData = DecompressData(fileData);
                }
                
                string jsonString = Encoding.UTF8.GetString(fileData);
                
                // 解析JSON
                var json = new Json();
                var parseResult = json.Parse(jsonString);
                
                if (parseResult != Error.Ok)
                {
                    return LoadOperationResult.CreateFailure(LoadResult.DataError, "存档数据格式错误");
                }
                
                var saveData = json.Data.AsGodotDictionary<string, Variant>();
                
                // 验证校验和
                if (saveData.ContainsKey("checksum"))
                {
                    string storedChecksum = saveData["checksum"].AsString();
                    saveData["checksum"] = "";
                    string calculatedChecksum = CalculateChecksum(Json.Stringify(saveData));
                    
                    if (storedChecksum != calculatedChecksum)
                    {
                        return LoadOperationResult.CreateFailure(LoadResult.CorruptedData, "存档数据校验失败");
                    }
                }
                
                // 提取游戏数据
                if (!saveData.ContainsKey("gameData"))
                {
                    return LoadOperationResult.CreateFailure(LoadResult.DataError, "存档中缺少游戏数据");
                }
                
                var gameDataDict = saveData["gameData"].AsGodotDictionary<string, Variant>();
                var extendedGameData = DictionaryToExtendedGameData(gameDataDict);
                
                return LoadOperationResult.CreateSuccess(extendedGameData, "加载成功", filePath);
            }
            catch (Exception ex)
            {
                return LoadOperationResult.CreateFailure(LoadResult.FileError, "加载文件时发生错误", ex.Message);
            }
        }
        
        /// <summary>
        /// 将字典转换为ExtendedGameData
        /// </summary>
        private ExtendedGameData DictionaryToExtendedGameData(Dictionary<string, Variant> dict)
        {
            var data = new ExtendedGameData();
            
            // 基础数据
            if (dict.ContainsKey("PlayerLevel")) data.PlayerLevel = dict["PlayerLevel"].AsInt32();
            if (dict.ContainsKey("PlayerExperience")) data.PlayerExperience = dict["PlayerExperience"].AsInt32();
            if (dict.ContainsKey("PlayerHealth")) data.PlayerHealth = dict["PlayerHealth"].AsInt32();
            if (dict.ContainsKey("PlayerMaxHealth")) data.PlayerMaxHealth = dict["PlayerMaxHealth"].AsInt32();
            if (dict.ContainsKey("MasterVolume")) data.MasterVolume = dict["MasterVolume"].AsSingle();
            if (dict.ContainsKey("MusicVolume")) data.MusicVolume = dict["MusicVolume"].AsSingle();
            if (dict.ContainsKey("SfxVolume")) data.SfxVolume = dict["SfxVolume"].AsSingle();
            
            // 扩展数据
            if (dict.ContainsKey("TotalPlayTime")) data.TotalPlayTime = dict["TotalPlayTime"].AsInt64();
            if (dict.ContainsKey("LastSaveTime")) data.LastSaveTime = dict["LastSaveTime"].AsInt64();
            
            return data;
        }
        
        /// <summary>
        /// 应用加载的数据到GameData
        /// </summary>
        private void ApplyLoadedDataToGameData(ExtendedGameData loadedData)
        {
            if (_gameData == null || loadedData == null) return;
            
            _gameData.PlayerLevel = loadedData.PlayerLevel;
            _gameData.PlayerExperience = loadedData.PlayerExperience;
            _gameData.PlayerHealth = loadedData.PlayerHealth;
            _gameData.PlayerMaxHealth = loadedData.PlayerMaxHealth;
            _gameData.MasterVolume = loadedData.MasterVolume;
            _gameData.MusicVolume = loadedData.MusicVolume;
            _gameData.SfxVolume = loadedData.SfxVolume;
        }
        
        /// <summary>
        /// 创建备份
        /// </summary>
        private void CreateBackup(int slotIndex)
        {
            try
            {
                string sourceFile = GetSaveFilePath(slotIndex);
                if (!GodotFileAccess.FileExists(sourceFile)) return;
                
                // 移动现有备份
                for (int i = MAX_BACKUPS_PER_SLOT - 1; i > 0; i--)
                {
                    string oldBackup = GetBackupFilePath(slotIndex, i - 1);
                    string newBackup = GetBackupFilePath(slotIndex, i);
                    
                    if (GodotFileAccess.FileExists(oldBackup))
                    {
                        if (GodotFileAccess.FileExists(newBackup))
                        {
                            DirAccess.RemoveAbsolute(newBackup);
                        }
                        DirAccess.RenameAbsolute(oldBackup, newBackup);
                    }
                }
                
                // 创建新备份
                string newBackupPath = GetBackupFilePath(slotIndex, 0);
                DirAccess.CopyAbsolute(sourceFile, newBackupPath);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"创建备份失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 尝试从备份恢复
        /// </summary>
        private LoadOperationResult TryRestoreFromBackup(int slotIndex)
        {
            for (int i = 0; i < MAX_BACKUPS_PER_SLOT; i++)
            {
                string backupPath = GetBackupFilePath(slotIndex, i);
                if (GodotFileAccess.FileExists(backupPath))
                {
                    var result = LoadDataFromFile(backupPath);
                    if (result.Success)
                    {
                        // 恢复主存档
                        string mainSavePath = GetSaveFilePath(slotIndex);
                        DirAccess.CopyAbsolute(backupPath, mainSavePath);
                        
                        return LoadOperationResult.CreateBackupRestored(result.LoadedData, $"从备份 {i} 恢复", mainSavePath);
                    }
                }
            }
            
            return LoadOperationResult.CreateFailure(LoadResult.CorruptedData, "所有备份都已损坏");
        }
        
        /// <summary>
        /// 删除备份文件
        /// </summary>
        private void DeleteBackups(int slotIndex)
        {
            for (int i = 0; i < MAX_BACKUPS_PER_SLOT; i++)
            {
                string backupPath = GetBackupFilePath(slotIndex, i);
                if (GodotFileAccess.FileExists(backupPath))
                {
                    DirAccess.RemoveAbsolute(backupPath);
                }
            }
        }
        
        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        private string GetSaveFilePath(int slotIndex)
        {
            return $"{SAVE_DIRECTORY}slot_{slotIndex:D2}{SAVE_FILE_EXTENSION}";
        }
        
        /// <summary>
        /// 获取备份文件路径
        /// </summary>
        private string GetBackupFilePath(int slotIndex, int backupIndex)
        {
            return $"{BACKUP_DIRECTORY}slot_{slotIndex:D2}_backup_{backupIndex:D2}{BACKUP_FILE_EXTENSION}";
        }
        
        /// <summary>
        /// 计算校验和
        /// </summary>
        private string CalculateChecksum(string data)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }
        
        /// <summary>
        /// 压缩数据
        /// </summary>
        private byte[] CompressData(byte[] data)
        {
            // 这里可以实现数据压缩，暂时返回原数据
            return data;
        }
        
        /// <summary>
        /// 解压数据
        /// </summary>
        private byte[] DecompressData(byte[] data)
        {
            // 这里可以实现数据解压，暂时返回原数据
            return data;
        }
        
        /// <summary>
        /// 获取总游戏时长
        /// </summary>
        private long GetTotalPlayTime()
        {
            // 这里需要实现实际的游戏时长统计
            return 0;
        }
        
        /// <summary>
        /// 设置自动存档间隔
        /// </summary>
        public void SetAutoSaveInterval(int seconds)
        {
            AutoSaveInterval = seconds;
            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.WaitTime = seconds;
            }
        }
        
        /// <summary>
        /// 启用/禁用自动存档
        /// </summary>
        public void SetAutoSaveEnabled(bool enabled)
        {
            EnableAutoSave = enabled;
            if (_autoSaveTimer != null)
            {
                if (enabled)
                {
                    _autoSaveTimer.Start();
                }
                else
                {
                    _autoSaveTimer.Stop();
                }
            }
        }
    }
}