using Godot;
using Godot.Collections;
using System;

namespace CodeRogue.Tower
{
	/// <summary>
	/// 地图生成器 - 负责随机生成地图拓扑结构
	/// </summary>
	public partial class MapGenerator : Node
	{
		[Export] public MapGenerationRule DefaultGenerationRule { get; set; } = MapGenerationRule.Balanced;
		[Export] public int DefaultMapLayers { get; set; } = 5;
		[Export] public int DefaultNodesPerLayer { get; set; } = 3;
		[Export] public float DefaultLayerSpacing { get; set; } = 120.0f;
		[Export] public float DefaultNodeSpacing { get; set; } = 80.0f;
		[Export] public Vector2 DefaultMapSize { get; set; } = new Vector2(800, 600);
		
		private Random _random;
		private System.Collections.Generic.Dictionary<MapTopologyType, System.Func<MapGenerationConfig, MapTopology>> _generatorMethods;
		
		public override void _Ready()
		{
			_random = new Random();
			InitializeGeneratorMethods();
		}
		
		/// <summary>
		/// 初始化生成器方法映射
		/// </summary>
		private void InitializeGeneratorMethods()
		{
			_generatorMethods = new System.Collections.Generic.Dictionary<MapTopologyType, System.Func<MapGenerationConfig, MapTopology>>
			{
				{ MapTopologyType.Linear, GenerateLinearMap },
				{ MapTopologyType.Branching, GenerateBranchingMap },
				{ MapTopologyType.Grid, GenerateGridMap },
				{ MapTopologyType.Circular, GenerateCircularMap },
				{ MapTopologyType.Tree, GenerateTreeMap },
				{ MapTopologyType.Network, GenerateNetworkMap },
				{ MapTopologyType.Maze, GenerateMazeMap },
				{ MapTopologyType.Spiral, GenerateSpiralMap },
				{ MapTopologyType.Random, GenerateRandomMap }
			};
		}
		
		/// <summary>
		/// 生成地图拓扑结构
		/// </summary>
		public MapTopology GenerateMap(MapGenerationConfig config)
		{
			if (config == null)
			{
				config = CreateDefaultConfig();
			}
			
			// 设置随机种子
			if (config.RandomSeed != 0)
			{
				_random = new Random(config.RandomSeed);
			}
			
			// 根据拓扑类型生成地图
			if (_generatorMethods.TryGetValue(config.TopologyType, out Func<MapGenerationConfig, MapTopology> value))
			{
				var map = value(config);
				
				// 后处理
				PostProcessMap(map, config);
				
				return map;
			}
			
			GD.PrintErr($"不支持的地图拓扑类型: {config.TopologyType}");
			return GenerateLinearMap(config);
		}
		
		/// <summary>
		/// 创建默认配置
		/// </summary>
		private MapGenerationConfig CreateDefaultConfig()
		{
			var config = new MapGenerationConfig
			{
				TopologyType = MapTopologyType.Branching,
				Difficulty = MapDifficulty.Normal,
				GenerationRule = DefaultGenerationRule,
				MapLayers = DefaultMapLayers,
				NodesPerLayer = DefaultNodesPerLayer,
				LayerSpacing = DefaultLayerSpacing,
				NodeSpacing = DefaultNodeSpacing,
				MapSize = DefaultMapSize,
				RandomSeed = (int)DateTime.Now.Ticks
			};
			
			return config;
		}
		
		/// <summary>
		/// 生成线性地图
		/// </summary>
		private MapTopology GenerateLinearMap(MapGenerationConfig config)
		{
			var map = CreateBaseMap(config);
			
			MapNode previousNode = null;
			
			for (int layer = 0; layer < config.MapLayers; layer++)
			{
				var nodeType = SelectNodeType(config, layer);
				var position = new Vector2(config.MapSize.X / 2, layer * config.LayerSpacing + 50);
				
				var node = CreateNode($"node_{layer}", nodeType, position, config);
				map.AddNode(node);
				
				if (layer == 0)
				{
					map.StartNodeId = node.NodeId;
					node.State = MapState.Available;
				}
				else if (layer == config.MapLayers - 1)
				{
					map.ExitNodeIds.Add(node.NodeId);
					node.NodeType = MapNodeType.Exit;
				}
				
				previousNode?.AddConnection(node.NodeId);
				
				previousNode = node;
			}
			
			return map;
		}
		
		/// <summary>
		/// 生成分支地图
		/// </summary>
		private MapTopology GenerateBranchingMap(MapGenerationConfig config)
		{
			var map = CreateBaseMap(config);
			var layerNodes = new Array<Array<MapNode>>();
			
			// 生成每层的节点
			for (int layer = 0; layer < config.MapLayers; layer++)
			{
				var currentLayerNodes = new Array<MapNode>();
				int nodesInLayer = GetNodesInLayer(config, layer);
				
				for (int nodeIndex = 0; nodeIndex < nodesInLayer; nodeIndex++)
				{
					var nodeType = SelectNodeType(config, layer);
					var position = CalculateNodePosition(config, layer, nodeIndex, nodesInLayer);
					
					var node = CreateNode($"node_{layer}_{nodeIndex}", nodeType, position, config);
					map.AddNode(node);
					currentLayerNodes.Add(node);
					
					if (layer == 0)
					{
						map.StartNodeId = node.NodeId;
						node.State = MapState.Available;
					}
					else if (layer == config.MapLayers - 1)
					{
						map.ExitNodeIds.Add(node.NodeId);
						node.NodeType = MapNodeType.Exit;
					}
				}
				
				layerNodes.Add(currentLayerNodes);
			}
			
			// 创建层间连接
			CreateLayerConnections(layerNodes, config);
			
			return map;
		}
		
		/// <summary>
		/// 生成网格地图
		/// </summary>
		private MapTopology GenerateGridMap(MapGenerationConfig config)
		{
			var map = CreateBaseMap(config);
			var nodeGrid = new MapNode[config.MapLayers, config.NodesPerLayer];
			
			// 生成网格节点
			for (int layer = 0; layer < config.MapLayers; layer++)
			{
				for (int col = 0; col < config.NodesPerLayer; col++)
				{
					var nodeType = SelectNodeType(config, layer);
					var position = new Vector2(
						col * config.NodeSpacing + 100,
						layer * config.LayerSpacing + 50
					);
					
					var node = CreateNode($"node_{layer}_{col}", nodeType, position, config);
					map.AddNode(node);
					nodeGrid[layer, col] = node;
					
					if (layer == 0)
					{
						if (col == config.NodesPerLayer / 2)
						{
							map.StartNodeId = node.NodeId;
							node.State = MapState.Available;
						}
					}
					else if (layer == config.MapLayers - 1)
					{
						map.ExitNodeIds.Add(node.NodeId);
						node.NodeType = MapNodeType.Exit;
					}
				}
			}
			
			// 创建网格连接
			CreateGridConnections(nodeGrid, config);
			
			return map;
		}
		
		/// <summary>
		/// 生成环形地图
		/// </summary>
		private MapTopology GenerateCircularMap(MapGenerationConfig config)
		{
			var map = CreateBaseMap(config);
			var centerX = config.MapSize.X / 2;
			var centerY = config.MapSize.Y / 2;
			var radius = Math.Min(centerX, centerY) - 100;
			
			var nodes = new Array<MapNode>();
			int totalNodes = config.MapLayers * config.NodesPerLayer;
			
			for (int i = 0; i < totalNodes; i++)
			{
				float angle = (float)(2 * Math.PI * i / totalNodes);
				var position = new Vector2(
					centerX + radius * Mathf.Cos(angle),
					centerY + radius * Mathf.Sin(angle)
				);
				
				var nodeType = SelectNodeType(config, i / config.NodesPerLayer);
				var node = CreateNode($"node_{i}", nodeType, position, config);
				map.AddNode(node);
				nodes.Add(node);
				
				if (i == 0)
				{
					map.StartNodeId = node.NodeId;
					node.State = MapState.Available;
				}
				else if (i == totalNodes - 1)
				{
					map.ExitNodeIds.Add(node.NodeId);
					node.NodeType = MapNodeType.Exit;
				}
			}
			
			// 创建环形连接
			CreateCircularConnections(nodes, config);
			
			return map;
		}
		
		/// <summary>
		/// 生成树形地图
		/// </summary>
		private MapTopology GenerateTreeMap(MapGenerationConfig config)
		{
			var map = CreateBaseMap(config);
			
			// 创建根节点
			var rootPosition = new Vector2(config.MapSize.X / 2, 50);
			var rootNode = CreateNode("root", MapNodeType.Start, rootPosition, config);
			map.AddNode(rootNode);
			map.StartNodeId = rootNode.NodeId;
			rootNode.State = MapState.Available;
			
			// 递归生成树结构
			GenerateTreeBranches(map, rootNode, config, 1, config.MapSize.X / 2, 100);
			
			return map;
		}
		
		/// <summary>
		/// 生成网络地图
		/// </summary>
		private MapTopology GenerateNetworkMap(MapGenerationConfig config)
		{
			var map = CreateBaseMap(config);
			var nodes = new Array<MapNode>();
			
			// 随机分布节点
			int totalNodes = config.MapLayers * config.NodesPerLayer;
			for (int i = 0; i < totalNodes; i++)
			{
				var position = new Vector2(
					_random.Next(100, (int)config.MapSize.X - 100),
					_random.Next(50, (int)config.MapSize.Y - 50)
				);
				
				var nodeType = SelectNodeType(config, i / config.NodesPerLayer);
				var node = CreateNode($"node_{i}", nodeType, position, config);
				map.AddNode(node);
				nodes.Add(node);
				
				if (i == 0)
				{
					map.StartNodeId = node.NodeId;
					node.State = MapState.Available;
				}
				else if (i == totalNodes - 1)
				{
					map.ExitNodeIds.Add(node.NodeId);
					node.NodeType = MapNodeType.Exit;
				}
			}
			
			// 创建网络连接
			CreateNetworkConnections(nodes, config);
			
			return map;
		}
		
		/// <summary>
		/// 生成迷宫地图
		/// </summary>
		private MapTopology GenerateMazeMap(MapGenerationConfig config)
		{
			// 简化的迷宫生成，实际可以使用更复杂的迷宫算法
			return GenerateGridMap(config);
		}
		
		/// <summary>
		/// 生成螺旋地图
		/// </summary>
		private MapTopology GenerateSpiralMap(MapGenerationConfig config)
		{
			var map = CreateBaseMap(config);
			var centerX = config.MapSize.X / 2;
			var centerY = config.MapSize.Y / 2;
			
			int totalNodes = config.MapLayers * config.NodesPerLayer;
			MapNode previousNode = null;
			
			for (int i = 0; i < totalNodes; i++)
			{
				float angle = (float)(i * 0.5f);
				float radius = i * 10f;
				
				var position = new Vector2(
					centerX + radius * Mathf.Cos(angle),
					centerY + radius * Mathf.Sin(angle)
				);
				
				var nodeType = SelectNodeType(config, i / config.NodesPerLayer);
				var node = CreateNode($"node_{i}", nodeType, position, config);
				map.AddNode(node);
				
				if (i == 0)
				{
					map.StartNodeId = node.NodeId;
					node.State = MapState.Available;
				}
				else if (i == totalNodes - 1)
				{
					map.ExitNodeIds.Add(node.NodeId);
					node.NodeType = MapNodeType.Exit;
				}
				
				previousNode?.AddConnection(node.NodeId);
				
				previousNode = node;
			}
			
			return map;
		}
		
		/// <summary>
		/// 生成随机地图
		/// </summary>
		private MapTopology GenerateRandomMap(MapGenerationConfig config)
		{
			// 随机选择一种拓扑类型
			var topologyTypes = new MapTopologyType[]
			{
				MapTopologyType.Linear,
				MapTopologyType.Branching,
				MapTopologyType.Grid,
				MapTopologyType.Circular,
				MapTopologyType.Tree,
				MapTopologyType.Network
			};
			
			var randomType = topologyTypes[_random.Next(topologyTypes.Length)];
			config.TopologyType = randomType;
			
			return _generatorMethods[randomType](config);
		}
		
		// 辅助方法
		
		private MapTopology CreateBaseMap(MapGenerationConfig config)
		{
			var map = new MapTopology
			{
				MapId = Guid.NewGuid().ToString(),
				MapName = $"第{config.FloorLevel}层地图",
				FloorLevel = config.FloorLevel,
				FloorType = config.FloorType,
				TopologyType = config.TopologyType,
				Difficulty = config.Difficulty,
				MapSize = config.MapSize,
				MaxLayers = config.MapLayers,
				NodesPerLayer = config.NodesPerLayer,
				LayerSpacing = config.LayerSpacing,
				NodeSpacing = config.NodeSpacing,
				GenerationRule = config.GenerationRule
			};
			
			return map;
		}
		
		private MapNode CreateNode(string nodeId, MapNodeType nodeType, Vector2 position, MapGenerationConfig config)
		{
			var node = new MapNode
			{
				NodeId = nodeId,
				NodeType = nodeType,
				Position = position,
				FloorLevel = config.FloorLevel,
				FloorType = config.FloorType,
				State = MapState.Locked
			};
			
			// 根据节点类型设置特定配置
			ConfigureNodeByType(node, config);
			
			return node;
		}
		
		private void ConfigureNodeByType(MapNode node, MapGenerationConfig config)
		{
			switch (node.NodeType)
			{
				case MapNodeType.NormalBattle:
					node.EnemyLevel = config.FloorLevel;
					node.DifficultyMultiplier = GetDifficultyMultiplier(config.Difficulty);
					break;
					
				case MapNodeType.EliteBattle:
					node.EnemyLevel = config.FloorLevel + 1;
					node.DifficultyMultiplier = GetDifficultyMultiplier(config.Difficulty) * 1.5f;
					break;
					
				case MapNodeType.BossBattle:
					node.EnemyLevel = config.FloorLevel + 2;
					node.DifficultyMultiplier = GetDifficultyMultiplier(config.Difficulty) * 2.0f;
					break;
					
				case MapNodeType.RestSite:
					node.NodeConfig["HealAmount"] = 30;
					break;
					
				case MapNodeType.Treasure:
					node.NodeConfig["TreasureLevel"] = config.FloorLevel;
					break;
			}
		}
		
		private float GetDifficultyMultiplier(MapDifficulty difficulty)
		{
			return difficulty switch
			{
				MapDifficulty.Easy => 0.8f,
				MapDifficulty.Normal => 1.0f,
				MapDifficulty.Hard => 1.3f,
				MapDifficulty.Expert => 1.6f,
				MapDifficulty.Master => 2.0f,
				_ => 1.0f
			};
		}
		
		private MapNodeType SelectNodeType(MapGenerationConfig config, int layer)
		{
			// 根据生成规则和层级选择节点类型
			if (layer == 0) return MapNodeType.Start;
			if (layer == config.MapLayers - 1) return MapNodeType.Exit;
			
			// Boss节点通常在中间层
			if (layer == config.MapLayers / 2 && _random.NextDouble() < 0.3)
			{
				return MapNodeType.BossBattle;
			}
			
			// 根据权重随机选择
			var weights = GetNodeTypeWeights(config.GenerationRule);
			return SelectWeightedNodeType(weights);
		}
		
		private Dictionary<MapNodeType, float> GetNodeTypeWeights(MapGenerationRule rule)
		{
			return rule switch
			{
				MapGenerationRule.CombatFocused => new Dictionary<MapNodeType, float>
				{
					{ MapNodeType.NormalBattle, 0.5f },
					{ MapNodeType.EliteBattle, 0.25f },
					{ MapNodeType.BossBattle, 0.1f },
					{ MapNodeType.RestSite, 0.1f },
					{ MapNodeType.Treasure, 0.05f }
				},
				MapGenerationRule.ExplorationFocused => new Dictionary<MapNodeType, float>
				{
					{ MapNodeType.NormalBattle, 0.3f },
					{ MapNodeType.RandomEvent, 0.2f },
					{ MapNodeType.Treasure, 0.15f },
					{ MapNodeType.Mystery, 0.15f },
					{ MapNodeType.RestSite, 0.1f },
					{ MapNodeType.EliteBattle, 0.1f }
				},
				_ => new Dictionary<MapNodeType, float>
				{
					{ MapNodeType.NormalBattle, 0.4f },
					{ MapNodeType.EliteBattle, 0.15f },
					{ MapNodeType.RandomEvent, 0.15f },
					{ MapNodeType.RestSite, 0.1f },
					{ MapNodeType.Treasure, 0.1f },
					{ MapNodeType.CardConstruct, 0.1f }
				}
			};
		}
		
		private MapNodeType SelectWeightedNodeType(Dictionary<MapNodeType, float> weights)
		{
			float totalWeight = 0;
			foreach (var weight in weights.Values)
			{
				totalWeight += weight;
			}
			
			float randomValue = (float)_random.NextDouble() * totalWeight;
			float currentWeight = 0;
			
			foreach (var kvp in weights)
			{
				currentWeight += kvp.Value;
				if (randomValue <= currentWeight)
				{
					return kvp.Key;
				}
			}
			
			return MapNodeType.NormalBattle;
		}
		
		private int GetNodesInLayer(MapGenerationConfig config, int layer)
		{
			// 根据层级调整节点数量
			if (layer == 0 || layer == config.MapLayers - 1)
			{
				return 1; // 起始和结束层只有一个节点
			}
			
			return config.NodesPerLayer + _random.Next(-1, 2); // 随机变化
		}
		
		private Vector2 CalculateNodePosition(MapGenerationConfig config, int layer, int nodeIndex, int totalNodesInLayer)
		{
			float layerY = layer * config.LayerSpacing + 50;
			float totalWidth = (totalNodesInLayer - 1) * config.NodeSpacing;
			float startX = (config.MapSize.X - totalWidth) / 2;
			float nodeX = startX + nodeIndex * config.NodeSpacing;
			
			return new Vector2(nodeX, layerY);
		}
		
		private void CreateLayerConnections(Array<Array<MapNode>> layerNodes, MapGenerationConfig config)
		{
			for (int layer = 0; layer < layerNodes.Count - 1; layer++)
			{
				var currentLayer = layerNodes[layer];
				var nextLayer = layerNodes[layer + 1];
				
				foreach (var currentNode in currentLayer)
				{
					// 每个节点至少连接到下一层的一个节点
					int connectionsCount = _random.Next(1, Math.Min(3, nextLayer.Count + 1));
					var availableTargets = new Array<MapNode>(nextLayer);
					
					for (int i = 0; i < connectionsCount && availableTargets.Count > 0; i++)
					{
						int targetIndex = _random.Next(availableTargets.Count);
						var targetNode = availableTargets[targetIndex];
						currentNode.AddConnection(targetNode.NodeId);
						availableTargets.RemoveAt(targetIndex);
					}
				}
			}
		}
		
		private void CreateGridConnections(MapNode[,] nodeGrid, MapGenerationConfig config)
		{
			for (int layer = 0; layer < config.MapLayers; layer++)
			{
				for (int col = 0; col < config.NodesPerLayer; col++)
				{
					var currentNode = nodeGrid[layer, col];
					
					// 连接到下一层
					if (layer < config.MapLayers - 1)
					{
						for (int nextCol = Math.Max(0, col - 1); nextCol <= Math.Min(config.NodesPerLayer - 1, col + 1); nextCol++)
						{
							currentNode.AddConnection(nodeGrid[layer + 1, nextCol].NodeId);
						}
					}
					
					// 连接到同层相邻节点
					if (col > 0)
					{
						currentNode.AddConnection(nodeGrid[layer, col - 1].NodeId, NodeConnectionType.Lateral);
					}
					if (col < config.NodesPerLayer - 1)
					{
						currentNode.AddConnection(nodeGrid[layer, col + 1].NodeId, NodeConnectionType.Lateral);
					}
				}
			}
		}
		
		private void CreateCircularConnections(Array<MapNode> nodes, MapGenerationConfig config)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				var currentNode = nodes[i];
				
				// 连接到下一个节点
				if (i < nodes.Count - 1)
				{
					currentNode.AddConnection(nodes[i + 1].NodeId);
				}
				
				// 可选的跳跃连接
				if (_random.NextDouble() < 0.3 && i < nodes.Count - 2)
				{
					currentNode.AddConnection(nodes[i + 2].NodeId, NodeConnectionType.Skip);
				}
			}
		}
		
		private void GenerateTreeBranches(MapTopology map, MapNode parentNode, MapGenerationConfig config, int currentLayer, float centerX, float width)
		{
			if (currentLayer >= config.MapLayers)
				return;
				
			int branchCount = _random.Next(2, 4); // 2-3个分支
			float branchWidth = width / branchCount;
			
			for (int i = 0; i < branchCount; i++)
			{
				float branchX = centerX - width / 2 + (i + 0.5f) * branchWidth;
				var position = new Vector2(branchX, currentLayer * config.LayerSpacing + 50);
				
				var nodeType = SelectNodeType(config, currentLayer);
				var childNode = CreateNode($"node_{currentLayer}_{i}", nodeType, position, config);
				map.AddNode(childNode);
				
				parentNode.AddConnection(childNode.NodeId);
				
				if (currentLayer == config.MapLayers - 1)
				{
					map.ExitNodeIds.Add(childNode.NodeId);
					childNode.NodeType = MapNodeType.Exit;
				}
				else
				{
					GenerateTreeBranches(map, childNode, config, currentLayer + 1, branchX, branchWidth);
				}
			}
		}
		
		private void CreateNetworkConnections(Array<MapNode> nodes, MapGenerationConfig config)
		{
			float maxDistance = config.NodeSpacing * 2;
			
			foreach (var node in nodes)
			{
				int connectionCount = 0;
				int maxConnections = _random.Next(2, 5);
				
				foreach (var otherNode in nodes)
				{
					if (node == otherNode || connectionCount >= maxConnections)
						continue;
						
					float distance = node.Position.DistanceTo(otherNode.Position);
					if (distance <= maxDistance && _random.NextDouble() < 0.4)
					{
						node.AddConnection(otherNode.NodeId);
						connectionCount++;
					}
				}
			}
		}
		
		private void PostProcessMap(MapTopology map, MapGenerationConfig config)
		{
			// 确保地图连通性
			EnsureMapConnectivity(map);
			
			// 添加特殊节点
			AddSpecialNodes(map, config);
			
			// 验证地图
			if (!map.ValidateMap())
			{
				GD.PrintErr("生成的地图验证失败");
			}
		}
		
		private void EnsureMapConnectivity(MapTopology map)
		{
			// 简单的连通性检查和修复
			// 确保每个节点至少有一个连接
			foreach (var node in map.Nodes)
			{
				if (node.ConnectedNodeIds.Count == 0 && node.NodeType != MapNodeType.Exit)
				{
					// 找到最近的节点并连接
					MapNode nearestNode = null;
					float minDistance = float.MaxValue;
					
					foreach (var otherNode in map.Nodes)
					{
						if (otherNode != node)
						{
							float distance = node.Position.DistanceTo(otherNode.Position);
							if (distance < minDistance)
							{
								minDistance = distance;
								nearestNode = otherNode;
							}
						}
					}
					
					if (nearestNode != null)
					{
						node.AddConnection(nearestNode.NodeId);
					}
				}
			}
		}
		
		private void AddSpecialNodes(MapTopology map, MapGenerationConfig config)
		{
			// 根据楼层类型添加特殊节点
			switch (config.FloorType)
			{
				case TowerFloorType.UrbanEnvironment:
					AddUrbanSpecialNodes(map, config);
					break;
				case TowerFloorType.SpiritualRealm:
					AddSpiritualSpecialNodes(map, config);
					break;
				case TowerFloorType.TechFusion:
					AddTechSpecialNodes(map, config);
					break;
			}
		}
		
		private void AddUrbanSpecialNodes(MapTopology map, MapGenerationConfig config)
		{
			// 添加城市环境特有的节点
			var techMarketNodes = map.GetNodesByType(MapNodeType.TechMarket);
			if (techMarketNodes.Count == 0 && _random.NextDouble() < 0.3)
			{
				// 随机选择一个普通节点转换为技术市场
				var normalNodes = map.GetNodesByType(MapNodeType.NormalBattle);
				if (normalNodes.Count > 0)
				{
					var selectedNode = normalNodes[_random.Next(normalNodes.Count)];
					selectedNode.NodeType = MapNodeType.TechMarket;
				}
			}
		}
		
		private void AddSpiritualSpecialNodes(MapTopology map, MapGenerationConfig config)
		{
			// 添加灵能领域特有的节点
			var chargeNodes = map.GetNodesByType(MapNodeType.SpiritualCharge);
			if (chargeNodes.Count == 0 && _random.NextDouble() < 0.4)
			{
				var normalNodes = map.GetNodesByType(MapNodeType.NormalBattle);
				if (normalNodes.Count > 0)
				{
					var selectedNode = normalNodes[_random.Next(normalNodes.Count)];
					selectedNode.NodeType = MapNodeType.SpiritualCharge;
				}
			}
		}
		
		private void AddTechSpecialNodes(MapTopology map, MapGenerationConfig config)
		{
			// 添加科技融合特有的节点
			var fusionNodes = map.GetNodesByType(MapNodeType.TechFusion);
			if (fusionNodes.Count == 0 && _random.NextDouble() < 0.5)
			{
				var normalNodes = map.GetNodesByType(MapNodeType.NormalBattle);
				if (normalNodes.Count > 0)
				{
					var selectedNode = normalNodes[_random.Next(normalNodes.Count)];
					selectedNode.NodeType = MapNodeType.TechFusion;
				}
			}
		}
	}
}
