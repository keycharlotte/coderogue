using Godot;
using Godot.Collections;

namespace CodeRogue.Tower
{
    /// <summary>
    /// 地图拓扑结构数据类 - 表示整个地图的结构和配置
    /// </summary>
    [GlobalClass]
    public partial class MapTopology : Resource
    {
        [Export] public string MapId { get; set; } = "";
        [Export] public string MapName { get; set; } = "";
        [Export] public string Description { get; set; } = "";
        [Export] public int FloorLevel { get; set; } = 1;
        [Export] public TowerFloorType FloorType { get; set; } = TowerFloorType.UrbanEnvironment;
        
        // 地图配置
        [Export] public MapTopologyType TopologyType { get; set; } = MapTopologyType.Linear;
        [Export] public MapDifficulty Difficulty { get; set; } = MapDifficulty.Normal;
        [Export] public MapLayer Layer { get; set; } = MapLayer.Surface;
        [Export] public MapState State { get; set; } = MapState.Locked;
        
        // 地图尺寸和布局
        [Export] public Vector2 MapSize { get; set; } = new Vector2(800, 600);
        [Export] public int MaxLayers { get; set; } = 5;
        [Export] public int NodesPerLayer { get; set; } = 3;
        [Export] public float LayerSpacing { get; set; } = 120.0f;
        [Export] public float NodeSpacing { get; set; } = 80.0f;
        
        // 节点集合
        [Export] public Array<MapNode> Nodes { get; set; } = new Array<MapNode>();
        [Export] public Dictionary<string, MapNode> NodeMap { get; set; } = new Dictionary<string, MapNode>();
        
        // 起始和结束节点
        [Export] public string StartNodeId { get; set; } = "";
        [Export] public Array<string> ExitNodeIds { get; set; } = new Array<string>();
        [Export] public string CurrentNodeId { get; set; } = "";
        
        // 生成规则
        [Export] public MapGenerationRule GenerationRule { get; set; } = MapGenerationRule.Balanced;
        [Export] public Dictionary<MapNodeType, float> NodeTypeWeights { get; set; } = new Dictionary<MapNodeType, float>();
        [Export] public Dictionary<string, Variant> GenerationParameters { get; set; } = new Dictionary<string, Variant>();
        
        // 地图元数据
        [Export] public Dictionary<string, Variant> Metadata { get; set; } = new Dictionary<string, Variant>();
        [Export] public Array<string> RequiredConditions { get; set; } = new Array<string>();
        [Export] public Array<string> UnlockConditions { get; set; } = new Array<string>();
        
        public MapTopology()
        {
            Nodes = new Array<MapNode>();
            NodeMap = new Dictionary<string, MapNode>();
            ExitNodeIds = new Array<string>();
            NodeTypeWeights = new Dictionary<MapNodeType, float>();
            GenerationParameters = new Dictionary<string, Variant>();
            Metadata = new Dictionary<string, Variant>();
            RequiredConditions = new Array<string>();
            UnlockConditions = new Array<string>();
            
            InitializeDefaultWeights();
        }
        
        /// <summary>
        /// 初始化默认的节点类型权重
        /// </summary>
        private void InitializeDefaultWeights()
        {
            NodeTypeWeights[MapNodeType.NormalBattle] = 0.4f;
            NodeTypeWeights[MapNodeType.EliteBattle] = 0.15f;
            NodeTypeWeights[MapNodeType.BossBattle] = 0.05f;
            NodeTypeWeights[MapNodeType.CardConstruct] = 0.1f;
            NodeTypeWeights[MapNodeType.SpiritualCharge] = 0.08f;
            NodeTypeWeights[MapNodeType.MemoryFragment] = 0.06f;
            NodeTypeWeights[MapNodeType.TeleportGate] = 0.04f;
            NodeTypeWeights[MapNodeType.RandomEvent] = 0.08f;
            NodeTypeWeights[MapNodeType.RestSite] = 0.04f;
        }
        
        /// <summary>
        /// 添加节点到地图
        /// </summary>
        public void AddNode(MapNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.NodeId))
                return;
                
            if (!NodeMap.ContainsKey(node.NodeId))
            {
                Nodes.Add(node);
                NodeMap[node.NodeId] = node;
                
                // 设置节点的楼层信息
                node.FloorLevel = FloorLevel;
                node.FloorType = FloorType;
            }
        }
        
        /// <summary>
        /// 移除节点
        /// </summary>
        public void RemoveNode(string nodeId)
        {
            if (NodeMap.ContainsKey(nodeId))
            {
                var node = NodeMap[nodeId];
                Nodes.Remove(node);
                NodeMap.Remove(nodeId);
                
                // 移除所有指向该节点的连接
                foreach (var otherNode in Nodes)
                {
                    otherNode.RemoveConnection(nodeId);
                }
            }
        }
        
        /// <summary>
        /// 获取节点
        /// </summary>
        public MapNode GetNode(string nodeId)
        {
            return NodeMap.ContainsKey(nodeId) ? NodeMap[nodeId] : null;
        }
        
        /// <summary>
        /// 获取起始节点
        /// </summary>
        public MapNode GetStartNode()
        {
            return GetNode(StartNodeId);
        }
        
        /// <summary>
        /// 获取当前节点
        /// </summary>
        public MapNode GetCurrentNode()
        {
            return GetNode(CurrentNodeId);
        }
        
        /// <summary>
        /// 获取可访问的节点
        /// </summary>
        public Array<MapNode> GetAccessibleNodes(Dictionary<string, Variant> playerState)
        {
            var accessibleNodes = new Array<MapNode>();
            
            foreach (var node in Nodes)
            {
                if (node.CanAccess(playerState))
                {
                    accessibleNodes.Add(node);
                }
            }
            
            return accessibleNodes;
        }
        
        /// <summary>
        /// 获取指定类型的节点
        /// </summary>
        public Array<MapNode> GetNodesByType(MapNodeType nodeType)
        {
            var filteredNodes = new Array<MapNode>();
            
            foreach (var node in Nodes)
            {
                if (node.NodeType == nodeType)
                {
                    filteredNodes.Add(node);
                }
            }
            
            return filteredNodes;
        }
        
        /// <summary>
        /// 获取指定层级的节点
        /// </summary>
        public Array<MapNode> GetNodesByLayer(int layer)
        {
            var layerNodes = new Array<MapNode>();
            
            foreach (var node in Nodes)
            {
                // 根据Y坐标计算层级
                int nodeLayer = Mathf.FloorToInt(node.Position.Y / LayerSpacing);
                if (nodeLayer == layer)
                {
                    layerNodes.Add(node);
                }
            }
            
            return layerNodes;
        }
        
        /// <summary>
        /// 设置当前节点
        /// </summary>
        public void SetCurrentNode(string nodeId)
        {
            // 清除之前的当前节点标记
            if (!string.IsNullOrEmpty(CurrentNodeId))
            {
                var previousNode = GetNode(CurrentNodeId);
                if (previousNode != null)
                {
                    previousNode.IsCurrentNode = false;
                }
            }
            
            // 设置新的当前节点
            CurrentNodeId = nodeId;
            var currentNode = GetNode(nodeId);
            if (currentNode != null)
            {
                currentNode.IsCurrentNode = true;
                currentNode.IsVisited = true;
                
                // 解锁连接的节点
                UnlockConnectedNodes(currentNode);
            }
        }
        
        /// <summary>
        /// 解锁连接的节点
        /// </summary>
        private void UnlockConnectedNodes(MapNode currentNode)
        {
            foreach (string connectedNodeId in currentNode.ConnectedNodeIds)
            {
                var connectedNode = GetNode(connectedNodeId);
                if (connectedNode != null && connectedNode.State == MapState.Locked)
                {
                    connectedNode.State = MapState.Available;
                }
            }
        }
        
        /// <summary>
        /// 验证地图完整性
        /// </summary>
        public bool ValidateMap()
        {
            // 检查起始节点
            if (string.IsNullOrEmpty(StartNodeId) || !NodeMap.ContainsKey(StartNodeId))
                return false;
                
            // 检查出口节点
            if (ExitNodeIds.Count == 0)
                return false;
                
            foreach (string exitId in ExitNodeIds)
            {
                if (!NodeMap.ContainsKey(exitId))
                    return false;
            }
            
            // 检查连接完整性
            foreach (var node in Nodes)
            {
                foreach (string connectedId in node.ConnectedNodeIds)
                {
                    if (!NodeMap.ContainsKey(connectedId))
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取地图统计信息
        /// </summary>
        public Dictionary<string, Variant> GetMapStatistics()
        {
            var stats = new Dictionary<string, Variant>();
            
            stats["TotalNodes"] = Nodes.Count;
            stats["CompletedNodes"] = GetCompletedNodeCount();
            stats["VisitedNodes"] = GetVisitedNodeCount();
            stats["AvailableNodes"] = GetAvailableNodeCount();
            
            // 按类型统计节点数量
            var nodeTypeCounts = new Dictionary<string, int>();
            foreach (var node in Nodes)
            {
                string typeName = node.NodeType.ToString();
                if (nodeTypeCounts.ContainsKey(typeName))
                    nodeTypeCounts[typeName]++;
                else
                    nodeTypeCounts[typeName] = 1;
            }
            stats["NodeTypeCounts"] = nodeTypeCounts;
            
            return stats;
        }
        
        private int GetCompletedNodeCount()
        {
            int count = 0;
            foreach (var node in Nodes)
            {
                if (node.IsCompleted) count++;
            }
            return count;
        }
        
        private int GetVisitedNodeCount()
        {
            int count = 0;
            foreach (var node in Nodes)
            {
                if (node.IsVisited) count++;
            }
            return count;
        }
        
        private int GetAvailableNodeCount()
        {
            int count = 0;
            foreach (var node in Nodes)
            {
                if (node.State == MapState.Available) count++;
            }
            return count;
        }
        
        /// <summary>
        /// 清理地图状态
        /// </summary>
        public void ResetMapState()
        {
            CurrentNodeId = "";
            
            foreach (var node in Nodes)
            {
                node.IsVisited = false;
                node.IsCompleted = false;
                node.IsCurrentNode = false;
                node.State = MapState.Locked;
            }
            
            // 解锁起始节点
            var startNode = GetStartNode();
            if (startNode != null)
            {
                startNode.State = MapState.Available;
            }
        }
    }
}