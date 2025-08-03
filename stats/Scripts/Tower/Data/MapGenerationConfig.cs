using Godot;
using Godot.Collections;

namespace CodeRogue.Tower
{
    /// <summary>
    /// 地图生成配置类 - 用于配置地图生成的各种参数
    /// </summary>
    [GlobalClass]
    public partial class MapGenerationConfig : Resource
    {
        [Export] public string ConfigId { get; set; } = "";
        [Export] public string ConfigName { get; set; } = "";
        [Export] public string Description { get; set; } = "";
        
        // 基础配置
        [Export] public int FloorLevel { get; set; } = 1;
        [Export] public TowerFloorType FloorType { get; set; } = TowerFloorType.UrbanEnvironment;
        [Export] public MapTopologyType TopologyType { get; set; } = MapTopologyType.Branching;
        [Export] public MapDifficulty Difficulty { get; set; } = MapDifficulty.Normal;
        [Export] public MapGenerationRule GenerationRule { get; set; } = MapGenerationRule.Balanced;
        
        // 地图尺寸和布局
        [Export] public Vector2 MapSize { get; set; } = new Vector2(800, 600);
        [Export] public int MapLayers { get; set; } = 5;
        [Export] public int NodesPerLayer { get; set; } = 3;
        [Export] public float LayerSpacing { get; set; } = 120.0f;
        [Export] public float NodeSpacing { get; set; } = 80.0f;
        
        // 随机种子
        [Export] public int RandomSeed { get; set; } = 0;
        [Export] public bool UseRandomSeed { get; set; } = true;
        
        // 节点类型权重配置
        [Export] public Dictionary<MapNodeType, float> NodeTypeWeights { get; set; } = new Dictionary<MapNodeType, float>();
        
        // 连接配置
        [Export] public float ConnectionDensity { get; set; } = 0.6f; // 连接密度 0-1
        [Export] public int MinConnectionsPerNode { get; set; } = 1;
        [Export] public int MaxConnectionsPerNode { get; set; } = 3;
        [Export] public float CrossLayerConnectionChance { get; set; } = 0.1f; // 跨层连接概率
        [Export] public float BackwardConnectionChance { get; set; } = 0.05f; // 向后连接概率
        
        // 特殊节点配置
        [Export] public float BossNodeChance { get; set; } = 0.2f;
        [Export] public float EliteNodeChance { get; set; } = 0.3f;
        [Export] public float RestNodeChance { get; set; } = 0.15f;
        [Export] public float TreasureNodeChance { get; set; } = 0.1f;
        [Export] public float EventNodeChance { get; set; } = 0.2f;
        [Export] public float ShopNodeChance { get; set; } = 0.1f;
        
        // 楼层特定配置
        [Export] public Dictionary<TowerFloorType, Dictionary<string, Variant>> FloorSpecificConfigs { get; set; } = new Dictionary<TowerFloorType, Dictionary<string, Variant>>();
        
        // 难度调整参数
        [Export] public float DifficultyScaling { get; set; } = 1.0f;
        [Export] public float EnemyLevelVariance { get; set; } = 0.2f;
        [Export] public float RewardMultiplier { get; set; } = 1.0f;
        
        // 地图变体配置
        [Export] public Array<string> EnabledVariants { get; set; } = new Array<string>();
        [Export] public Dictionary<string, Variant> VariantParameters { get; set; } = new Dictionary<string, Variant>();
        
        // 验证规则
        [Export] public bool RequireStartNode { get; set; } = true;
        [Export] public bool RequireExitNode { get; set; } = true;
        [Export] public bool RequireConnectivity { get; set; } = true;
        [Export] public int MinPathLength { get; set; } = 3;
        [Export] public int MaxPathLength { get; set; } = 10;
        
        public MapGenerationConfig()
        {
            NodeTypeWeights = new Dictionary<MapNodeType, float>();
            FloorSpecificConfigs = new Dictionary<TowerFloorType, Dictionary<string, Variant>>();
            EnabledVariants = new Array<string>();
            VariantParameters = new Dictionary<string, Variant>();
            
            InitializeDefaultWeights();
            InitializeFloorConfigs();
        }
        
        /// <summary>
        /// 初始化默认配置值
        /// </summary>
        public void InitializeDefaults()
        {
            // 重新初始化所有默认值
            InitializeDefaultWeights();
            InitializeFloorConfigs();
            
            // 设置基础默认值
            ConfigId = "default_config";
            ConfigName = "默认配置";
            Description = "默认地图生成配置";
            FloorLevel = 1;
            FloorType = TowerFloorType.UrbanEnvironment;
            TopologyType = MapTopologyType.Branching;
            Difficulty = MapDifficulty.Normal;
            GenerationRule = MapGenerationRule.Balanced;
            
            // 地图尺寸和布局
            MapSize = new Vector2(800, 600);
            MapLayers = 5;
            NodesPerLayer = 3;
            LayerSpacing = 120.0f;
            NodeSpacing = 80.0f;
            
            // 随机种子
            RandomSeed = 0;
            UseRandomSeed = true;
            
            // 连接配置
            ConnectionDensity = 0.6f;
            MinConnectionsPerNode = 1;
            MaxConnectionsPerNode = 3;
            CrossLayerConnectionChance = 0.1f;
            BackwardConnectionChance = 0.05f;
            
            // 特殊节点配置
            BossNodeChance = 0.2f;
            EliteNodeChance = 0.3f;
            RestNodeChance = 0.15f;
            TreasureNodeChance = 0.1f;
            EventNodeChance = 0.2f;
            ShopNodeChance = 0.1f;
            
            // 难度调整参数
            DifficultyScaling = 1.0f;
            EnemyLevelVariance = 0.2f;
            RewardMultiplier = 1.0f;
            
            // 验证规则
            RequireStartNode = true;
            RequireExitNode = true;
            RequireConnectivity = true;
            MinPathLength = 3;
            MaxPathLength = 10;
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
        /// 初始化楼层特定配置
        /// </summary>
        private void InitializeFloorConfigs()
        {
            // 城市环境配置
            var urbanConfig = new Dictionary<string, Variant>
            {
                ["TechMarketChance"] = 0.3f,
                ["CultivatorTavernChance"] = 0.2f,
                ["OpenLibraryChance"] = 0.15f,
                ["NetworkConsciousnessChance"] = 0.1f
            };
            FloorSpecificConfigs[TowerFloorType.UrbanEnvironment] = urbanConfig;
            
            // 灵能领域配置
            var spiritualConfig = new Dictionary<string, Variant>
            {
                ["SpiritualChargeChance"] = 0.4f,
                ["MemoryFragmentChance"] = 0.3f,
                ["TeleportGateChance"] = 0.2f,
                ["SectTrialChance"] = 0.25f
            };
            FloorSpecificConfigs[TowerFloorType.SpiritualRealm] = spiritualConfig;
            
            // 科技融合配置
            var techConfig = new Dictionary<string, Variant>
            {
                ["TechFusionChance"] = 0.5f,
                ["NetworkConsciousnessChance"] = 0.3f,
                ["CardConstructChance"] = 0.35f,
                ["TechMarketChance"] = 0.4f
            };
            FloorSpecificConfigs[TowerFloorType.TechFusion] = techConfig;
        }
        
        /// <summary>
        /// 根据生成规则调整权重
        /// </summary>
        public void ApplyGenerationRule(MapGenerationRule rule)
        {
            GenerationRule = rule;
            
            switch (rule)
            {
                case MapGenerationRule.CombatFocused:
                    ApplyCombatFocusedWeights();
                    break;
                    
                case MapGenerationRule.ExplorationFocused:
                    ApplyExplorationFocusedWeights();
                    break;
                    
                case MapGenerationRule.ResourceFocused:
                    ApplyResourceFocusedWeights();
                    break;
                    
                case MapGenerationRule.EventFocused:
                    ApplyEventFocusedWeights();
                    break;
                    
                case MapGenerationRule.Challenging:
                    ApplyChallengingWeights();
                    break;
                    
                case MapGenerationRule.Relaxed:
                    ApplyRelaxedWeights();
                    break;
                    
                case MapGenerationRule.Balanced:
                default:
                    InitializeDefaultWeights();
                    break;
            }
        }
        
        private void ApplyCombatFocusedWeights()
        {
            NodeTypeWeights[MapNodeType.NormalBattle] = 0.5f;
            NodeTypeWeights[MapNodeType.EliteBattle] = 0.25f;
            NodeTypeWeights[MapNodeType.BossBattle] = 0.1f;
            NodeTypeWeights[MapNodeType.RestSite] = 0.1f;
            NodeTypeWeights[MapNodeType.RandomEvent] = 0.05f;
        }
        
        private void ApplyExplorationFocusedWeights()
        {
            NodeTypeWeights[MapNodeType.NormalBattle] = 0.25f;
            NodeTypeWeights[MapNodeType.RandomEvent] = 0.25f;
            NodeTypeWeights[MapNodeType.Mystery] = 0.2f;
            NodeTypeWeights[MapNodeType.Treasure] = 0.15f;
            NodeTypeWeights[MapNodeType.Choice] = 0.15f;
        }
        
        private void ApplyResourceFocusedWeights()
        {
            NodeTypeWeights[MapNodeType.NormalBattle] = 0.3f;
            NodeTypeWeights[MapNodeType.CardConstruct] = 0.2f;
            NodeTypeWeights[MapNodeType.TechMarket] = 0.15f;
            NodeTypeWeights[MapNodeType.Treasure] = 0.15f;
            NodeTypeWeights[MapNodeType.CultivatorTavern] = 0.1f;
            NodeTypeWeights[MapNodeType.OpenLibrary] = 0.1f;
        }
        
        private void ApplyEventFocusedWeights()
        {
            NodeTypeWeights[MapNodeType.NormalBattle] = 0.3f;
            NodeTypeWeights[MapNodeType.RandomEvent] = 0.3f;
            NodeTypeWeights[MapNodeType.SectTrial] = 0.15f;
            NodeTypeWeights[MapNodeType.Choice] = 0.15f;
            NodeTypeWeights[MapNodeType.Mystery] = 0.1f;
        }
        
        private void ApplyChallengingWeights()
        {
            NodeTypeWeights[MapNodeType.NormalBattle] = 0.35f;
            NodeTypeWeights[MapNodeType.EliteBattle] = 0.3f;
            NodeTypeWeights[MapNodeType.BossBattle] = 0.15f;
            NodeTypeWeights[MapNodeType.SectTrial] = 0.1f;
            NodeTypeWeights[MapNodeType.RestSite] = 0.05f;
            NodeTypeWeights[MapNodeType.Treasure] = 0.05f;
        }
        
        private void ApplyRelaxedWeights()
        {
            NodeTypeWeights[MapNodeType.NormalBattle] = 0.3f;
            NodeTypeWeights[MapNodeType.RestSite] = 0.2f;
            NodeTypeWeights[MapNodeType.CardConstruct] = 0.15f;
            NodeTypeWeights[MapNodeType.Treasure] = 0.15f;
            NodeTypeWeights[MapNodeType.RandomEvent] = 0.1f;
            NodeTypeWeights[MapNodeType.EliteBattle] = 0.1f;
        }
        
        /// <summary>
        /// 根据难度调整配置
        /// </summary>
        public void ApplyDifficulty(MapDifficulty difficulty)
        {
            Difficulty = difficulty;
            
            switch (difficulty)
            {
                case MapDifficulty.Easy:
                    DifficultyScaling = 0.8f;
                    RewardMultiplier = 0.9f;
                    RestNodeChance = 0.25f;
                    BossNodeChance = 0.1f;
                    EliteNodeChance = 0.2f;
                    break;
                    
                case MapDifficulty.Normal:
                    DifficultyScaling = 1.0f;
                    RewardMultiplier = 1.0f;
                    RestNodeChance = 0.15f;
                    BossNodeChance = 0.2f;
                    EliteNodeChance = 0.3f;
                    break;
                    
                case MapDifficulty.Hard:
                    DifficultyScaling = 1.3f;
                    RewardMultiplier = 1.2f;
                    RestNodeChance = 0.1f;
                    BossNodeChance = 0.3f;
                    EliteNodeChance = 0.4f;
                    break;
                    
                case MapDifficulty.Expert:
                    DifficultyScaling = 1.6f;
                    RewardMultiplier = 1.5f;
                    RestNodeChance = 0.05f;
                    BossNodeChance = 0.4f;
                    EliteNodeChance = 0.5f;
                    break;
                    
                case MapDifficulty.Master:
                    DifficultyScaling = 2.0f;
                    RewardMultiplier = 2.0f;
                    RestNodeChance = 0.03f;
                    BossNodeChance = 0.5f;
                    EliteNodeChance = 0.6f;
                    break;
            }
        }
        
        /// <summary>
        /// 根据楼层类型调整配置
        /// </summary>
        public void ApplyFloorType(TowerFloorType floorType)
        {
            FloorType = floorType;
            
            if (FloorSpecificConfigs.ContainsKey(floorType))
            {
                var config = FloorSpecificConfigs[floorType];
                
                // 应用楼层特定的节点生成概率
                foreach (var kvp in config)
                {
                    VariantParameters[kvp.Key] = kvp.Value;
                }
            }
            
            // 根据楼层类型调整基础参数
            switch (floorType)
            {
                case TowerFloorType.UrbanEnvironment:
                    ApplyUrbanEnvironmentConfig();
                    break;
                    
                case TowerFloorType.SpiritualRealm:
                    ApplySpiritualRealmConfig();
                    break;
                    
                case TowerFloorType.TechFusion:
                    ApplyTechFusionConfig();
                    break;
                    
                case TowerFloorType.DataStream:
                    ApplyDataStreamConfig();
                    break;
                    
                case TowerFloorType.QuantumSpace:
                    ApplyQuantumSpaceConfig();
                    break;
                    
                case TowerFloorType.ConsciousnessNet:
                    ApplyConsciousnessNetConfig();
                    break;
                    
                case TowerFloorType.HiddenRuins:
                    ApplyHiddenRuinsConfig();
                    break;
            }
        }
        
        private void ApplyUrbanEnvironmentConfig()
        {
            // 城市环境：更多商店和社交节点
            ShopNodeChance = 0.2f;
            EventNodeChance = 0.25f;
            TopologyType = MapTopologyType.Grid; // 城市网格布局
        }
        
        private void ApplySpiritualRealmConfig()
        {
            // 灵能领域：更多神秘和修炼节点
            EventNodeChance = 0.3f;
            RestNodeChance = 0.2f;
            TopologyType = MapTopologyType.Tree; // 树形灵脉布局
        }
        
        private void ApplyTechFusionConfig()
        {
            // 科技融合：更多实验和升级节点
            TreasureNodeChance = 0.15f;
            EventNodeChance = 0.25f;
            TopologyType = MapTopologyType.Network; // 网络连接布局
        }
        
        private void ApplyDataStreamConfig()
        {
            // 数据流：线性快速通道
            TopologyType = MapTopologyType.Linear;
            ConnectionDensity = 0.8f;
            LayerSpacing = 100.0f;
        }
        
        private void ApplyQuantumSpaceConfig()
        {
            // 量子空间：复杂的多维连接
            TopologyType = MapTopologyType.Network;
            CrossLayerConnectionChance = 0.3f;
            BackwardConnectionChance = 0.2f;
        }
        
        private void ApplyConsciousnessNetConfig()
        {
            // 意识网络：环形连接结构
            TopologyType = MapTopologyType.Circular;
            ConnectionDensity = 0.9f;
            MaxConnectionsPerNode = 5;
        }
        
        private void ApplyHiddenRuinsConfig()
        {
            // 隐藏废墟：迷宫式布局
            TopologyType = MapTopologyType.Maze;
            BossNodeChance = 0.6f;
            EliteNodeChance = 0.8f;
            RestNodeChance = 0.05f;
        }
        
        /// <summary>
        /// 启用地图变体
        /// </summary>
        public void EnableVariant(string variantName, Dictionary<string, Variant> parameters = null)
        {
            if (!EnabledVariants.Contains(variantName))
            {
                EnabledVariants.Add(variantName);
            }
            
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    VariantParameters[$"{variantName}_{kvp.Key}"] = kvp.Value;
                }
            }
        }
        
        /// <summary>
        /// 禁用地图变体
        /// </summary>
        public void DisableVariant(string variantName)
        {
            EnabledVariants.Remove(variantName);
            
            // 移除相关参数
            var keysToRemove = new Array<string>();
            foreach (var key in VariantParameters.Keys)
            {
                if (key.StartsWith($"{variantName}_"))
                {
                    keysToRemove.Add(key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                VariantParameters.Remove(key);
            }
        }
        
        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool ValidateConfig()
        {
            // 检查基础参数
            if (MapLayers <= 0 || NodesPerLayer <= 0)
                return false;
                
            if (LayerSpacing <= 0 || NodeSpacing <= 0)
                return false;
                
            if (MapSize.X <= 0 || MapSize.Y <= 0)
                return false;
                
            // 检查概率值
            if (ConnectionDensity < 0 || ConnectionDensity > 1)
                return false;
                
            if (CrossLayerConnectionChance < 0 || CrossLayerConnectionChance > 1)
                return false;
                
            if (BackwardConnectionChance < 0 || BackwardConnectionChance > 1)
                return false;
                
            // 检查节点数量限制
            if (MinConnectionsPerNode < 0 || MaxConnectionsPerNode < MinConnectionsPerNode)
                return false;
                
            // 检查路径长度
            if (MinPathLength <= 0 || MaxPathLength < MinPathLength)
                return false;
                
            return true;
        }
        
        /// <summary>
        /// 创建配置副本
        /// </summary>
        public MapGenerationConfig Clone()
        {
            var clone = new MapGenerationConfig
            {
                ConfigId = ConfigId,
                ConfigName = ConfigName,
                Description = Description,
                FloorLevel = FloorLevel,
                FloorType = FloorType,
                TopologyType = TopologyType,
                Difficulty = Difficulty,
                GenerationRule = GenerationRule,
                MapSize = MapSize,
                MapLayers = MapLayers,
                NodesPerLayer = NodesPerLayer,
                LayerSpacing = LayerSpacing,
                NodeSpacing = NodeSpacing,
                RandomSeed = RandomSeed,
                UseRandomSeed = UseRandomSeed,
                ConnectionDensity = ConnectionDensity,
                MinConnectionsPerNode = MinConnectionsPerNode,
                MaxConnectionsPerNode = MaxConnectionsPerNode,
                CrossLayerConnectionChance = CrossLayerConnectionChance,
                BackwardConnectionChance = BackwardConnectionChance,
                BossNodeChance = BossNodeChance,
                EliteNodeChance = EliteNodeChance,
                RestNodeChance = RestNodeChance,
                TreasureNodeChance = TreasureNodeChance,
                EventNodeChance = EventNodeChance,
                ShopNodeChance = ShopNodeChance,
                DifficultyScaling = DifficultyScaling,
                EnemyLevelVariance = EnemyLevelVariance,
                RewardMultiplier = RewardMultiplier,
                RequireStartNode = RequireStartNode,
                RequireExitNode = RequireExitNode,
                RequireConnectivity = RequireConnectivity,
                MinPathLength = MinPathLength,
                MaxPathLength = MaxPathLength
            };
            
            // 深拷贝字典和数组
            foreach (var kvp in NodeTypeWeights)
            {
                clone.NodeTypeWeights[kvp.Key] = kvp.Value;
            }
            
            foreach (var kvp in FloorSpecificConfigs)
            {
                var configCopy = new Dictionary<string, Variant>();
                foreach (var configKvp in kvp.Value)
                {
                    configCopy[configKvp.Key] = configKvp.Value;
                }
                clone.FloorSpecificConfigs[kvp.Key] = configCopy;
            }
            
            foreach (var variant in EnabledVariants)
            {
                clone.EnabledVariants.Add(variant);
            }
            
            foreach (var kvp in VariantParameters)
            {
                clone.VariantParameters[kvp.Key] = kvp.Value;
            }
            
            return clone;
        }
    }
}