using Godot;
using CodeRogue.Core;
using CodeRogue.UI;

namespace CodeRogue.Utils
{
    /// <summary>
    /// 节点获取相关的工具类
    /// </summary>
    public static class NodeUtils
    {
        /// <summary>
        /// 获取 AudioManager 节点
        /// </summary>
        /// <param name="node">调用此方法的节点</param>
        /// <returns>AudioManager 实例，如果未找到则返回 null</returns>
        public static AudioManager GetAudioManager(Node node)
        {
            try
            {
                return node.GetNode<AudioManager>("/root/AudioManager");
            }
            catch
            {
                GD.PrintErr("无法找到 AudioManager 节点");
                return null;
            }
        }
        
        /// <summary>
        /// 获取 GameManager 节点
        /// </summary>
        /// <param name="node">调用此方法的节点</param>
        /// <returns>GameManager 实例，如果未找到则返回 null</returns>
        public static GameManager GetGameManager(Node node)
        {
            try
            {
                return node.GetNode<GameManager>("/root/GameManager");
            }
            catch
            {
                GD.PrintErr("无法找到 GameManager 节点");
                return null;
            }
        }
        
        /// <summary>
        /// 获取 UIManager 节点
        /// </summary>
        /// <param name="node">调用此方法的节点</param>
        /// <returns>UIManager 实例，如果未找到则返回 null</returns>
        public static UIManager GetUIManager(Node node)
        {
            try
            {
                return node.GetNode<UIManager>("/root/UIManager");

            }
            catch
            {
                GD.PrintErr("无法找到 UIManager 节点");
                return null;
            }
        }
        
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