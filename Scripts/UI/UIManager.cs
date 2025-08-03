using Godot;
using CodeRogue.Core;
using CodeRogue.Data;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace CodeRogue.UI
{
    /// <summary>
    /// UI管理器 - 负责管理所有UI界面的显示和隐藏
    /// </summary>
    public partial class UIManager : CanvasLayer
    {
        [Export]
        public PackedScene MainMenuScene { get; set; }
        
        [Export]
        public PackedScene GameUIScene { get; set; }
        
        [Export]
        public PackedScene PauseMenuScene { get; set; }
        
        [Export]
        public PackedScene GameOverScene { get; set; }
        
        [Export]
        public PackedScene SettingsScene { get; set; }
        
        private Control _currentUI;
        private Stack<Control> _uiStack = new Stack<Control>();  // 添加UI栈
        private MainMenu _mainMenu;
        private GameUI _gameUI;
        private PauseMenu _pauseMenu;
        private GameOverScreen _gameOverScreen;
        private SettingsMenu _settingsMenu;
        
        // UI缓存系统
        private Godot.Collections.Dictionary<string, Control> _uiCache = new Godot.Collections.Dictionary<string, Control>();
        private Godot.Collections.Dictionary<string, PackedScene> _uiScenes = new Godot.Collections.Dictionary<string, PackedScene>();
        private Godot.Collections.Dictionary<string, bool> _uiPersistent = new Godot.Collections.Dictionary<string, bool>();
        
        public override void _Ready()
        {
            // 添加到组中以便其他脚本找到
            AddToGroup("UIManager");
            
            // 初始化UI界面
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // 注册核心UI场景到新的管理系统
            if (MainMenuScene != null)
            {
                _uiScenes["MainMenu"] = MainMenuScene;
                _uiPersistent["MainMenu"] = true;
            }
            
            if (GameUIScene != null)
            {
                _uiScenes["GameUI"] = GameUIScene;
                _uiPersistent["GameUI"] = true;
            }
            
            if (PauseMenuScene != null)
            {
                _uiScenes["PauseMenu"] = PauseMenuScene;
                _uiPersistent["PauseMenu"] = true;
            }
            
            if (GameOverScene != null)
            {
                _uiScenes["GameOverScreen"] = GameOverScene;
                _uiPersistent["GameOverScreen"] = true;
            }
            
            if (SettingsScene != null)
            {
                _uiScenes["SettingsMenu"] = SettingsScene;
                _uiPersistent["SettingsMenu"] = true;
            }
            
            // 预加载核心UI（保持向后兼容）
            if (MainMenuScene != null)
            {
                _mainMenu = LoadUI("MainMenu", false, false) as MainMenu;
            }
            
            if (GameUIScene != null)
            {
                _gameUI = LoadUI("GameUI", false, false) as GameUI;
            }
            
            if (PauseMenuScene != null)
            {
                _pauseMenu = LoadUI("PauseMenu", false, false) as PauseMenu;
            }
            
            if (GameOverScene != null)
            {
                _gameOverScreen = LoadUI("GameOverScreen", false, false) as GameOverScreen;
            }
            
            if (SettingsScene != null)
            {
                _settingsMenu = LoadUI("SettingsMenu", false, false) as SettingsMenu;
            }
            
            // 隐藏所有UI
            HideAllUI();

            GD.Print("UIManager initialized");
            // ShowMainMenu();
        }
        
        public void ShowMainMenu()
        {
            HideAllUI();
            ClearUIStack();  // 清空栈
            if (_mainMenu != null)
            {
                _mainMenu.Visible = true;
                _currentUI = _mainMenu;
            }
        }
        
        public void ShowGameUI()
        {
            HideAllUI();
            ClearUIStack();  // 清空栈
            if (_gameUI != null)
            {
                _gameUI.Visible = true;
                _currentUI = _gameUI;
            }
        }
        
        public void ShowPauseMenu()
        {
            if (_pauseMenu != null)
            {
                // 将当前UI压入栈中（通常是GameUI）
                if (_currentUI != null && _currentUI.Visible)
                {
                    _uiStack.Push(_currentUI);
                }
                _pauseMenu.Visible = true;
                _currentUI = _pauseMenu;
            }
        }
        
        public void HidePauseMenu()
        {
            if (_pauseMenu != null)
            {
                _pauseMenu.Visible = false;
                
                // 返回到上一级UI
                if (_uiStack.Count > 0)
                {
                    var previousUI = _uiStack.Pop();
                    if (previousUI != null)
                    {
                        previousUI.Visible = true;
                        _currentUI = previousUI;
                    }
                }
            }
        }
        
        public void ShowGameOverScreen()
        {
            if (_gameOverScreen != null)
            {
                _gameOverScreen.Visible = true;
            }
        }
        
        public void ShowSettingsMenu()
        {
            if (_settingsMenu != null)
            {
                // 将当前UI压入栈中
                if (_currentUI != null && _currentUI.Visible)
                {
                    _uiStack.Push(_currentUI);
                }
                _settingsMenu.Visible = true;
                _currentUI = _settingsMenu;
            }
        }
        
        public void HideSettingsMenu()
        {
            if (_settingsMenu != null)
            {
                _settingsMenu.Visible = false;
                
                // 返回到上一级UI
                if (_uiStack.Count > 0)
                {
                    var previousUI = _uiStack.Pop();
                    if (previousUI != null)
                    {
                        previousUI.Visible = true;
                        _currentUI = previousUI;
                    }
                }
                else
                {
                    // 如果栈为空，默认返回主菜单
                    ShowMainMenu();
                }
            }
        }
        
        // 清空UI栈的方法（在切换到主要UI时调用）
        public void ClearUIStack()
        {
            _uiStack.Clear();
        }
        
        private void HideAllUI()
        {
            _mainMenu?.Hide();
            _gameUI?.Hide();
            _pauseMenu?.Hide();
            _gameOverScreen?.Hide();
            _settingsMenu?.Hide();
            
            // 隐藏所有缓存的UI
            foreach (var ui in _uiCache.Values)
            {
                if (ui != null && ui.IsInsideTree())
                {
                    ui.Hide();
                }
            }
        }
        
        #region UI管理核心接口
        
        /// <summary>
        /// 注册UI场景
        /// </summary>
        /// <param name="uiId">UI唯一标识</param>
        /// <param name="scenePath">场景文件路径</param>
        /// <param name="isPersistent">是否持久化（不自动释放）</param>
        public void RegisterUI(string uiId, string scenePath, bool isPersistent = false)
        {
            if (string.IsNullOrEmpty(uiId) || string.IsNullOrEmpty(scenePath))
            {
                GD.PrintErr($"UIManager: 注册UI失败，uiId或scenePath为空");
                return;
            }
            
            var scene = GD.Load<PackedScene>(scenePath);
            if (scene == null)
            {
                GD.PrintErr($"UIManager: 无法加载UI场景: {scenePath}");
                return;
            }
            
            _uiScenes[uiId] = scene;
            _uiPersistent[uiId] = isPersistent;
            
            GD.Print($"UIManager: 已注册UI - {uiId} ({scenePath})");
        }
        
        /// <summary>
        /// 加载并显示UI
        /// </summary>
        /// <param name="uiId">UI唯一标识</param>
        /// <param name="hideOthers">是否隐藏其他UI</param>
        /// <param name="addToStack">是否添加到UI栈</param>
        /// <returns>加载的UI控件</returns>
        public Control LoadUI(string uiId, bool hideOthers = true, bool addToStack = true)
        {
            if (string.IsNullOrEmpty(uiId))
            {
                GD.PrintErr("UIManager: LoadUI失败，uiId为空");
                return null;
            }
            
            // 检查是否已缓存
             if (_uiCache.ContainsKey(uiId) && _uiCache[uiId] != null && _uiCache[uiId].IsInsideTree())
             {
                 var cachedUI = _uiCache[uiId];
                 ShowCachedUI(cachedUI, hideOthers, addToStack);
                 return cachedUI;
             }
            
            // 检查是否已注册场景
            if (!_uiScenes.ContainsKey(uiId))
            {
                GD.PrintErr($"UIManager: UI未注册 - {uiId}");
                return null;
            }
            
            // 实例化UI
            var scene = _uiScenes[uiId];
            var uiInstance = scene.Instantiate<Control>();
            if (uiInstance == null)
            {
                GD.PrintErr($"UIManager: 无法实例化UI - {uiId}");
                return null;
            }
            
            // 添加到场景树
            AddChild(uiInstance);
            
            // 缓存UI
            _uiCache[uiId] = uiInstance;
            
            // 显示UI
            ShowCachedUI(uiInstance, hideOthers, addToStack);
            
            GD.Print($"UIManager: 已加载UI - {uiId}");
            return uiInstance;
        }
        
        /// <summary>
        /// 显示已缓存的UI
        /// </summary>
        private void ShowCachedUI(Control ui, bool hideOthers, bool addToStack)
        {
            if (hideOthers)
            {
                // 将当前UI压入栈
                if (addToStack && _currentUI != null && _currentUI.Visible && _currentUI != ui)
                {
                    _uiStack.Push(_currentUI);
                }
                
                HideAllUI();
            }
            
            ui.Show();
            _currentUI = ui;
        }
        
        /// <summary>
        /// 隐藏指定UI
        /// </summary>
        /// <param name="uiId">UI唯一标识</param>
        public void HideUI(string uiId)
        {
            if (_uiCache.ContainsKey(uiId) && _uiCache[uiId] != null)
            {
                _uiCache[uiId].Hide();
                
                // 如果隐藏的是当前UI，尝试返回上一级
                if (_currentUI == _uiCache[uiId])
                {
                    ReturnToPreviousUI();
                }
            }
        }
        
        /// <summary>
        /// 释放UI资源
        /// </summary>
        /// <param name="uiId">UI唯一标识</param>
        public void UnloadUI(string uiId)
        {
            if (!_uiCache.ContainsKey(uiId))
                return;
                
            var ui = _uiCache[uiId];
             if (ui != null && ui.IsInsideTree())
             {
                 // 如果是当前UI，先返回上一级
                 if (_currentUI == ui)
                 {
                     ReturnToPreviousUI();
                 }
                 
                 ui.QueueFree();
             }
            
            _uiCache.Remove(uiId);
            GD.Print($"UIManager: 已释放UI - {uiId}");
        }
        
        /// <summary>
        /// 返回上一级UI
        /// </summary>
        public void ReturnToPreviousUI()
        {
            if (_uiStack.Count > 0)
            {
                var previousUI = _uiStack.Pop();
                if (previousUI != null && previousUI.IsInsideTree())
                {
                    previousUI.Show();
                    _currentUI = previousUI;
                    return;
                }
            }
            
            // 如果栈为空或上一级UI无效，显示主菜单
            ShowMainMenu();
        }
        
        /// <summary>
        /// 清理非持久化UI
        /// </summary>
        public void CleanupNonPersistentUI()
        {
            var uisToRemove = new List<string>();
            
            foreach (var kvp in _uiCache)
            {
                var uiId = kvp.Key;
                var ui = kvp.Value;
                
                // 检查是否为非持久化UI
                if (!_uiPersistent.GetValueOrDefault(uiId, false))
                {
                    if (ui != null && ui.IsInsideTree())
                {
                    ui.QueueFree();
                }
                    uisToRemove.Add(uiId);
                }
            }
            
            foreach (var uiId in uisToRemove)
            {
                _uiCache.Remove(uiId);
                GD.Print($"UIManager: 已清理非持久化UI - {uiId}");
            }
        }
        
        /// <summary>
        /// 获取已加载的UI
        /// </summary>
        /// <param name="uiId">UI唯一标识</param>
        /// <returns>UI控件，如果未加载则返回null</returns>
        public Control GetLoadedUI(string uiId)
        {
            if (_uiCache.ContainsKey(uiId) && _uiCache[uiId] != null && _uiCache[uiId].IsInsideTree())
            {
                return _uiCache[uiId];
            }
            return null;
        }
        
        /// <summary>
        /// 检查UI是否已加载
        /// </summary>
        /// <param name="uiId">UI唯一标识</param>
        /// <returns>是否已加载</returns>
        public bool IsUILoaded(string uiId)
        {
            return _uiCache.ContainsKey(uiId) && _uiCache[uiId] != null && _uiCache[uiId].IsInsideTree();
        }
        
        /// <summary>
        /// 检查UI是否可见
        /// </summary>
        /// <param name="uiId">UI唯一标识</param>
        /// <returns>是否可见</returns>
        public bool IsUIVisible(string uiId)
        {
            return IsUILoaded(uiId) && _uiCache[uiId].Visible;
        }
        
        #endregion
         
         #region 便捷方法和向后兼容
         
         /// <summary>
         /// 显示指定UI（便捷方法）
         /// </summary>
         /// <param name="uiId">UI唯一标识</param>
         /// <param name="hideOthers">是否隐藏其他UI</param>
         public void ShowUI(string uiId, bool hideOthers = true)
         {
             LoadUI(uiId, hideOthers, true);
         }
         
         /// <summary>
         /// 切换UI显示状态
         /// </summary>
         /// <param name="uiId">UI唯一标识</param>
         public void ToggleUI(string uiId)
         {
             if (IsUIVisible(uiId))
             {
                 HideUI(uiId);
             }
             else
             {
                 ShowUI(uiId);
             }
         }
         
         /// <summary>
         /// 获取UI的强类型引用
         /// </summary>
         /// <typeparam name="T">UI类型</typeparam>
         /// <param name="uiId">UI唯一标识</param>
         /// <returns>强类型UI引用</returns>
         public T GetUI<T>(string uiId) where T : Control
         {
             var ui = GetLoadedUI(uiId);
             return ui as T;
         }
         
         /// <summary>
         /// 加载并获取UI的强类型引用
         /// </summary>
         /// <typeparam name="T">UI类型</typeparam>
         /// <param name="uiId">UI唯一标识</param>
         /// <param name="hideOthers">是否隐藏其他UI</param>
         /// <returns>强类型UI引用</returns>
         public T LoadAndGetUI<T>(string uiId, bool hideOthers = true) where T : Control
         {
             var ui = LoadUI(uiId, hideOthers);
             return ui as T;
         }
         
         /// <summary>
         /// 批量注册UI
         /// </summary>
         /// <param name="uiConfigs">UI配置列表</param>
         public void RegisterUIs(System.Collections.Generic.Dictionary<string, (string scenePath, bool isPersistent)> uiConfigs)
         {
             foreach (var kvp in uiConfigs)
             {
                 RegisterUI(kvp.Key, kvp.Value.scenePath, kvp.Value.isPersistent);
             }
         }
         
         /// <summary>
         /// 获取所有已注册的UI ID
         /// </summary>
         /// <returns>UI ID列表</returns>
         public List<string> GetRegisteredUIIds()
         {
             return new List<string>(_uiScenes.Keys);
         }
         
         /// <summary>
         /// 获取所有已加载的UI ID
         /// </summary>
         /// <returns>已加载的UI ID列表</returns>
         public List<string> GetLoadedUIIds()
         {
             var loadedIds = new List<string>();
             foreach (var kvp in _uiCache)
             {
                 if (kvp.Value != null && kvp.Value.IsInsideTree())
                 {
                     loadedIds.Add(kvp.Key);
                 }
             }
             return loadedIds;
         }
         
         /// <summary>
         /// 获取UI缓存统计信息
         /// </summary>
         /// <returns>缓存统计信息</returns>
         public System.Collections.Generic.Dictionary<string, object> GetCacheStats()
         {
             var stats = new System.Collections.Generic.Dictionary<string, object>
             {
                 ["RegisteredCount"] = _uiScenes.Count,
                 ["LoadedCount"] = GetLoadedUIIds().Count,
                 ["PersistentCount"] = _uiPersistent.Values.Count(p => p),
                 ["StackDepth"] = _uiStack.Count,
                 ["CurrentUI"] = _currentUI?.Name ?? "None"
             };
             return stats;
         }
         
         #endregion
    }
}