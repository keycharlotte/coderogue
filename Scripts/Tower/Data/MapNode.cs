using Godot;
using Godot.Collections;

namespace CodeRogue.Tower
{
    /// <summary>
    /// 地图节点数据类 - 表示爬塔系统中的单个节点
    /// </summary>
    [GlobalClass]
    public partial class MapNode : Resource
    {
        [Export] public string NodeId { get; set; } = "";
        [Export] public string NodeName { get; set; } = "";
        [Export] public string Description { get; set; } = "";
        [Export] public MapNodeType NodeType { get; set; } = MapNodeType.NormalBattle;
        [Export] public Vector2 Position { get; set; } = Vector2.Zero;
        [Export] public int FloorLevel { get; set; } = 1;
        [Export] public TowerFloorType FloorType { get; set; } = TowerFloorType.UrbanEnvironment;
        
        // 节点状态
        [Export] public MapState State { get; set; } = MapState.Locked;
        [Export] public bool IsVisited { get; set; } = false;
        [Export] public bool IsCompleted { get; set; } = false;
        [Export] public bool IsCurrentNode { get; set; } = false;
        
        // 连接信息
        [Export] public Array<string> ConnectedNodeIds { get; set; } = new Array<string>();
        [Export] public Dictionary<string, NodeConnectionType> ConnectionTypes { get; set; } = new Dictionary<string, NodeConnectionType>();
        
        // 节点配置
        [Export] public Dictionary<string, Variant> NodeConfig { get; set; } = new Dictionary<string, Variant>();
        [Export] public Array<string> RequiredConditions { get; set; } = new Array<string>();
        [Export] public Array<string> UnlockConditions { get; set; } = new Array<string>();
        
        // 奖励和资源
        [Export] public Dictionary<string, int> Rewards { get; set; } = new Dictionary<string, int>();
        [Export] public Array<string> AvailableCards { get; set; } = new Array<string>();
        [Export] public Array<string> AvailableRelics { get; set; } = new Array<string>();
        
        // 敌人配置（战斗节点专用）
        [Export] public Array<string> EnemyIds { get; set; } = new Array<string>();
        [Export] public int EnemyLevel { get; set; } = 1;
        [Export] public float DifficultyMultiplier { get; set; } = 1.0f;
        
        // 事件配置（事件节点专用）
        [Export] public string EventId { get; set; } = "";
        [Export] public Dictionary<string, Variant> EventParameters { get; set; } = new Dictionary<string, Variant>();
        
        // 商店配置（商店节点专用）
        [Export] public Array<string> ShopItems { get; set; } = new Array<string>();
        [Export] public Dictionary<string, int> ShopPrices { get; set; } = new Dictionary<string, int>();
        
        public MapNode()
        {
            ConnectedNodeIds = new Array<string>();
            ConnectionTypes = new Dictionary<string, NodeConnectionType>();
            NodeConfig = new Dictionary<string, Variant>();
            RequiredConditions = new Array<string>();
            UnlockConditions = new Array<string>();
            Rewards = new Dictionary<string, int>();
            AvailableCards = new Array<string>();
            AvailableRelics = new Array<string>();
            EnemyIds = new Array<string>();
            EventParameters = new Dictionary<string, Variant>();
            ShopItems = new Array<string>();
            ShopPrices = new Dictionary<string, int>();
        }
        
        /// <summary>
        /// 检查节点是否可以访问
        /// </summary>
        public bool CanAccess(Dictionary<string, Variant> playerState)
        {
            if (State == MapState.Locked)
                return false;
                
            // 检查必要条件
            foreach (string condition in RequiredConditions)
            {
                if (!CheckCondition(condition, playerState))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查解锁条件
        /// </summary>
        public bool CanUnlock(Dictionary<string, Variant> playerState)
        {
            if (State != MapState.Locked)
                return false;
                
            // 检查解锁条件
            foreach (string condition in UnlockConditions)
            {
                if (!CheckCondition(condition, playerState))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查特定条件
        /// </summary>
        private bool CheckCondition(string condition, Dictionary<string, Variant> playerState)
        {
            // 简单的条件检查实现
            // 可以扩展为更复杂的条件系统
            if (playerState.ContainsKey(condition))
            {
                return playerState[condition].AsBool();
            }
            
            return false;
        }
        
        /// <summary>
        /// 添加连接到其他节点
        /// </summary>
        public void AddConnection(string nodeId, NodeConnectionType connectionType = NodeConnectionType.Normal)
        {
            if (!ConnectedNodeIds.Contains(nodeId))
            {
                ConnectedNodeIds.Add(nodeId);
                ConnectionTypes[nodeId] = connectionType;
            }
        }
        
        /// <summary>
        /// 移除连接
        /// </summary>
        public void RemoveConnection(string nodeId)
        {
            ConnectedNodeIds.Remove(nodeId);
            if (ConnectionTypes.ContainsKey(nodeId))
            {
                ConnectionTypes.Remove(nodeId);
            }
        }
        
        /// <summary>
        /// 获取节点的显示名称
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(NodeName))
                return NodeName;
                
            return NodeType switch
            {
                MapNodeType.NormalBattle => "普通战斗",
                MapNodeType.EliteBattle => "精英战斗",
                MapNodeType.BossBattle => "Boss战斗",
                MapNodeType.CardConstruct => "卡牌构筑室",
                MapNodeType.SpiritualCharge => "灵能充电站",
                MapNodeType.MemoryFragment => "记忆碎片库",
                MapNodeType.TeleportGate => "试炼传送门",
                MapNodeType.RandomEvent => "随机事件",
                MapNodeType.SectTrial => "宗门试炼",
                MapNodeType.TechFusion => "科技融合",
                MapNodeType.NetworkConsciousness => "网络神识",
                MapNodeType.TechMarket => "技术交易市场",
                MapNodeType.CultivatorTavern => "修炼者酒馆",
                MapNodeType.OpenLibrary => "开源图书馆",
                MapNodeType.RestSite => "休息点",
                MapNodeType.Treasure => "宝藏",
                MapNodeType.Mystery => "神秘事件",
                MapNodeType.Choice => "选择事件",
                MapNodeType.Start => "起始点",
                MapNodeType.Exit => "出口",
                _ => "未知节点"
            };
        }
        
        /// <summary>
        /// 获取节点图标路径
        /// </summary>
        public string GetIconPath()
        {
            return NodeType switch
            {
                MapNodeType.NormalBattle => "res://Assets/Icons/battle_normal.svg",
                MapNodeType.EliteBattle => "res://Assets/Icons/battle_elite.svg",
                MapNodeType.BossBattle => "res://Assets/Icons/battle_boss.svg",
                MapNodeType.CardConstruct => "res://Assets/Icons/card_construct.svg",
                MapNodeType.SpiritualCharge => "res://Assets/Icons/spiritual_charge.svg",
                MapNodeType.MemoryFragment => "res://Assets/Icons/memory_fragment.svg",
                MapNodeType.TeleportGate => "res://Assets/Icons/teleport_gate.svg",
                MapNodeType.RandomEvent => "res://Assets/Icons/random_event.svg",
                MapNodeType.SectTrial => "res://Assets/Icons/sect_trial.svg",
                MapNodeType.TechFusion => "res://Assets/Icons/tech_fusion.svg",
                MapNodeType.NetworkConsciousness => "res://Assets/Icons/network_consciousness.svg",
                MapNodeType.TechMarket => "res://Assets/Icons/tech_market.svg",
                MapNodeType.CultivatorTavern => "res://Assets/Icons/cultivator_tavern.svg",
                MapNodeType.OpenLibrary => "res://Assets/Icons/open_library.svg",
                MapNodeType.RestSite => "res://Assets/Icons/rest_site.svg",
                MapNodeType.Treasure => "res://Assets/Icons/treasure.svg",
                MapNodeType.Mystery => "res://Assets/Icons/mystery.svg",
                MapNodeType.Choice => "res://Assets/Icons/choice.svg",
                MapNodeType.Start => "res://Assets/Icons/start.svg",
                MapNodeType.Exit => "res://Assets/Icons/exit.svg",
                _ => "res://Assets/Icons/unknown.svg"
            };
        }
    }
}