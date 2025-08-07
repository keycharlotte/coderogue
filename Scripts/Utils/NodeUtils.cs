using Godot;
using System;
using System.Collections.Generic;
using CodeRogue.Core;
using CodeRogue.UI;
using CodeRogue.Skills;
using CodeRogue.Buffs;
using CodeRogue.Heroes;
using CodeRogue.Tower;
using CodeRogue.Level;
using CodeRogue.Data;

namespace CodeRogue.Utils
{
    /// <summary>
    /// 节点获取相关的工具类 - 统一Manager获取标准
    /// </summary>
    public static class NodeUtils
    {
        // Manager缓存（可选优化）
        private static readonly Dictionary<string, WeakReference> _managerCache = new();
        /// <summary>
        /// 获取指定Manager的通用方法
        /// </summary>
        /// <typeparam name="T">Manager类型</typeparam>
        /// <param name="node">调用节点</param>
        /// <param name="managerName">Manager名称</param>
        /// <returns>Manager实例或null</returns>
        public static T GetManager<T>(Node node, string managerName) where T : Node
        {
            try
            {
                if (node == null)
                {
                    GD.PrintErr($"调用节点为空，无法获取{managerName}");
                    return null;
                }
                
                var manager = node.GetNode<T>($"/root/{managerName}");
                if (manager == null)
                {
                    GD.PrintErr($"未找到{managerName}节点");
                }
                return manager;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"获取{managerName}时发生错误: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 从Resource类中获取Manager的通用方法
        /// </summary>
        /// <typeparam name="T">Manager类型</typeparam>
        /// <param name="managerName">Manager名称</param>
        /// <returns>Manager实例或null</returns>
        public static T GetManagerFromResource<T>(string managerName) where T : Node
        {
            try
            {
                var sceneTree = Engine.GetMainLoop() as SceneTree;
                if (sceneTree?.Root == null)
                {
                    GD.PrintErr($"无法获取场景树，无法获取{managerName}");
                    return null;
                }
                
                var manager = sceneTree.Root.GetNode<T>($"/root/{managerName}");
                if (manager == null)
                {
                    GD.PrintErr($"未找到{managerName}节点");
                }
                return manager;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"从Resource获取{managerName}时发生错误: {ex.Message}");
                return null;
            }
        }
        
        // ===========================================
        // 具体Manager获取方法 - Node类使用
        // ===========================================
        
        /// <summary>
        /// 获取 GameManager 节点
        /// </summary>
        public static GameManager GetGameManager(Node node) => GetManager<GameManager>(node, "GameManager");
        
        /// <summary>
        /// 获取 AudioManager 节点
        /// </summary>
        public static AudioManager GetAudioManager(Node node) => GetManager<AudioManager>(node, "AudioManager");
        
        /// <summary>
        /// 获取 UIManager 节点
        /// </summary>
        public static UIManager GetUIManager(Node node) => GetManager<UIManager>(node, "UIManager");
        
        /// <summary>
        /// 获取 DeckManager 节点
        /// </summary>
        public static DeckManager GetDeckManager(Node node) => GetManager<DeckManager>(node, "DeckManager");
        
        /// <summary>
        /// 获取 BuffManager 节点
        /// </summary>
        public static BuffManager GetBuffManager(Node node) => GetManager<BuffManager>(node, "BuffManager");
        
        /// <summary>
        /// 获取 SkillTrackManager 节点
        /// </summary>
        public static SkillTrackManager GetSkillTrackManager(Node node) => GetManager<SkillTrackManager>(node, "SkillTrackManager");
        
        /// <summary>
        /// 获取 InputManager 节点
        /// </summary>
        public static InputManager GetInputManager(Node node) => GetManager<InputManager>(node, "InputManager");
        
        /// <summary>
        /// 获取 WordManager 节点
        /// </summary>
        public static WordManager GetWordManager(Node node) => GetManager<WordManager>(node, "WordManager");
        
        /// <summary>
        /// 获取 RelicManager 节点
        /// </summary>
        public static RelicManager GetRelicManager(Node node) => GetManager<RelicManager>(node, "RelicManager");
        
        /// <summary>
        /// 获取 LevelManager 节点
        /// </summary>
        public static LevelManager GetLevelManager(Node node) => GetManager<LevelManager>(node, "LevelManager");
        
        /// <summary>
        /// 获取 HeroManager 节点
        /// </summary>
        public static HeroManager GetHeroManager(Node node) => GetManager<HeroManager>(node, "HeroManager");
        
        /// <summary>
        /// 获取 TowerManager 节点
        /// </summary>
        public static TowerManager GetTowerManager(Node node) => GetManager<TowerManager>(node, "TowerManager");
        
        /// <summary>
        /// 获取 ConfigManager 节点
        /// </summary>
        public static ConfigManager GetConfigManager(Node node) => GetManager<ConfigManager>(node, "ConfigManager");
        
        /// <summary>
        /// 获取 SaveManager 节点
        /// </summary>
        public static SaveManager GetSaveManager(Node node) => GetManager<SaveManager>(node, "SaveManager");
        
        /// <summary>
        /// 获取 SaveSystemConfig 节点
        /// </summary>
        public static SaveSystemConfig GetSaveSystemConfig(Node node) => GetManager<SaveSystemConfig>(node, "SaveSystemConfig");
        
        /// <summary>
        /// 获取 SaveSystemEvents 节点
        /// </summary>
        public static SaveSystemEvents GetSaveSystemEvents(Node node) => GetManager<SaveSystemEvents>(node, "SaveSystemEvents");
        
        /// <summary>
        /// 获取 CardDatabase 节点
        /// </summary>
        public static CardDatabase GetCardDatabase(Node node) => GetManager<CardDatabase>(node, "CardDatabase");
        
        /// <summary>
        /// 获取 GameData 节点
        /// </summary>
        public static GameData GetGameData(Node node) => GetManager<GameData>(node, "GameData");
        
        /// <summary>
        /// 获取 BuffDatabase 节点
        /// </summary>
        public static BuffDatabase GetBuffDatabase(Node node) => GetManager<BuffDatabase>(node, "BuffDatabase");
        
        /// <summary>
        /// 获取 HeroDatabase 节点
        /// </summary>
        public static HeroDatabase GetHeroDatabase(Node node) => GetManager<HeroDatabase>(node, "HeroDatabase");
        
        /// <summary>
        /// 获取 RelicDatabase 节点
        /// </summary>
        public static RelicDatabase GetRelicDatabase(Node node) => GetManager<RelicDatabase>(node, "RelicDatabase");
        
        // ===========================================
        // Resource类专用方法
        // ===========================================
        
        /// <summary>
        /// 从Resource获取 GameManager 节点
        /// </summary>
        public static GameManager GetGameManagerFromResource() => GetManagerFromResource<GameManager>("GameManager");
        
        /// <summary>
        /// 从Resource获取 BuffManager 节点
        /// </summary>
        public static BuffManager GetBuffManagerFromResource() => GetManagerFromResource<BuffManager>("BuffManager");
        
        /// <summary>
        /// 从Resource获取 SkillTrackManager 节点
        /// </summary>
        public static SkillTrackManager GetSkillTrackManagerFromResource() => GetManagerFromResource<SkillTrackManager>("SkillTrackManager");
        
        /// <summary>
        /// 从Resource获取 DeckManager 节点
        /// </summary>
        public static DeckManager GetDeckManagerFromResource() => GetManagerFromResource<DeckManager>("DeckManager");
        
        /// <summary>
        /// 从Resource获取 HeroManager 节点
        /// </summary>
        public static HeroManager GetHeroManagerFromResource() => GetManagerFromResource<HeroManager>("HeroManager");
        
        /// <summary>
        /// 从Resource获取 RelicManager 节点
        /// </summary>
        public static RelicManager GetRelicManagerFromResource() => GetManagerFromResource<RelicManager>("RelicManager");
        
        /// <summary>
        /// 从Resource获取 AudioManager 节点
        /// </summary>
        public static AudioManager GetAudioManagerFromResource() => GetManagerFromResource<AudioManager>("AudioManager");
        
        /// <summary>
        /// 从Resource获取 UIManager 节点
        /// </summary>
        public static UIManager GetUIManagerFromResource() => GetManagerFromResource<UIManager>("UIManager");
        
        /// <summary>
        /// 从Resource获取 InputManager 节点
        /// </summary>
        public static InputManager GetInputManagerFromResource() => GetManagerFromResource<InputManager>("InputManager");
        
        /// <summary>
        /// 从Resource获取 WordManager 节点
        /// </summary>
        public static WordManager GetWordManagerFromResource() => GetManagerFromResource<WordManager>("WordManager");
        
        /// <summary>
        /// 从Resource获取 LevelManager 节点
        /// </summary>
        public static LevelManager GetLevelManagerFromResource() => GetManagerFromResource<LevelManager>("LevelManager");
        
        /// <summary>
        /// 从Resource获取 TowerManager 节点
        /// </summary>
        public static TowerManager GetTowerManagerFromResource() => GetManagerFromResource<TowerManager>("TowerManager");
        
        /// <summary>
        /// 从Resource获取 ConfigManager 节点
        /// </summary>
        public static ConfigManager GetConfigManagerFromResource() => GetManagerFromResource<ConfigManager>("ConfigManager");
        
        /// <summary>
        /// 安全获取子节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="parent">父节点</param>
        /// <param name="path">节点路径</param>
        /// <returns>节点实例，如果未找到则返回 null</returns>
        public static T GetChildSafe<T>(Node parent, string path) where T : Node
        {
            try
            {
                return parent.GetNode<T>(path);
            }
            catch
            {
                GD.PrintErr($"无法找到子节点: {path}");
                return null;
            }
        }
        
        /// <summary>
        /// 计算两个节点之间的世界坐标距离
        /// </summary>
        /// <param name="node1">第一个节点</param>
        /// <param name="node2">第二个节点</param>
        /// <returns>世界坐标距离</returns>
        public static float GetWorldDistance(Node2D node1, Node2D node2)
        {
            if (node1 == null || node2 == null)
                return float.MaxValue;
                
            return node1.GlobalPosition.DistanceTo(node2.GlobalPosition);
        }
        
        /// <summary>
        /// 计算两个节点之间的屏幕坐标距离
        /// </summary>
        /// <param name="node1">第一个节点</param>
        /// <param name="node2">第二个节点</param>
        /// <param name="viewport">视口，如果为null则自动获取</param>
        /// <returns>屏幕坐标距离</returns>
        public static float GetScreenDistance(Node2D node1, Node2D node2, Viewport viewport = null)
        {
            if (node1 == null || node2 == null)
                return float.MaxValue;
                
            viewport ??= node1.GetViewport();
            if (viewport == null)
                return GetWorldDistance(node1, node2);
                
            var camera = viewport.GetCamera2D();
            if (camera == null)
                return GetWorldDistance(node1, node2);
                
            // 将世界坐标转换为屏幕坐标
            Vector2 node1ScreenPos = GetScreenPosition(node1, camera);
            Vector2 node2ScreenPos = GetScreenPosition(node2, camera);
            
            return node1ScreenPos.DistanceTo(node2ScreenPos);
        }
        
        /// <summary>
        /// 将节点的世界坐标转换为屏幕坐标
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="camera">摄像机</param>
        /// <returns>屏幕坐标</returns>
        public static Vector2 GetScreenPosition(Node2D node, Camera2D camera)
        {
            if (node == null || camera == null)
                return Vector2.Zero;
                
            // 计算相对于摄像机的位置
            Vector2 relativePos = node.GlobalPosition - camera.GlobalPosition;
            
            // 应用摄像机缩放
            Vector2 screenPos = relativePos * camera.Zoom;
            
            // 添加屏幕中心偏移
            Vector2 screenCenter = camera.GetViewport().GetVisibleRect().Size / 2;
            
            return screenCenter + screenPos;
        }
        
        /// <summary>
        /// 计算两个位置之间的距离（支持世界坐标和屏幕坐标）
        /// </summary>
        /// <param name="pos1">位置1</param>
        /// <param name="pos2">位置2</param>
        /// <param name="useScreenCoordinates">是否使用屏幕坐标</param>
        /// <param name="camera">摄像机（仅在使用屏幕坐标时需要）</param>
        /// <returns>距离</returns>
        public static float GetDistance(Vector2 pos1, Vector2 pos2, bool useScreenCoordinates = false, Camera2D camera = null)
        {
            if (!useScreenCoordinates || camera == null)
            {
                return pos1.DistanceTo(pos2);
            }
            
            // 转换为屏幕坐标
            Vector2 screenPos1 = (pos1 - camera.GlobalPosition) * camera.Zoom;
            Vector2 screenPos2 = (pos2 - camera.GlobalPosition) * camera.Zoom;
            
            return screenPos1.DistanceTo(screenPos2);
        }
    }
}