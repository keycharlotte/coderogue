using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using CodeRogue.Data;
using CodeRogue.Utils;

namespace CodeRogue.Tower
{
	/// <summary>
	/// 爬塔系统核心管理器 - 负责管理整个爬塔系统的运行
	/// </summary>
	public partial class TowerManager : Node
	{
		[Signal] public delegate void MapGeneratedEventHandler(MapTopology mapTopology);
		[Signal] public delegate void NodeEnteredEventHandler(MapNode node);
		[Signal] public delegate void NodeCompletedEventHandler(MapNode node, bool success);
		[Signal] public delegate void FloorCompletedEventHandler(int floorLevel, TowerFloorType floorType);
		[Signal] public delegate void TowerProgressChangedEventHandler(int currentFloor, int totalFloors);
		[Signal] public delegate void MapStateChangedEventHandler(MapState oldState, MapState newState);
		
		// 导出属性 - 在编辑器中配置
		[Export] public Godot.Collections.Array<MapGenerationConfig> DefaultConfigs { get; set; } = new Godot.Collections.Array<MapGenerationConfig>();
		[Export] public int TotalTowerFloors { get; set; } = 51;
		[Export] public bool EnableAutoSave { get; set; } = true;
		[Export] public float AutoSaveInterval { get; set; } = 30.0f;
		[Export] public bool EnableDebugMode { get; set; } = false;
		
		// 私有字段
		private MapTopology _currentMap;
		private MapNode _currentNode;
		private MapGenerator _mapGenerator;
		private Godot.Collections.Dictionary<int, MapTopology> _floorMaps;
		private Godot.Collections.Dictionary<int, MapGenerationConfig> _floorConfigs;
		private TowerProgress _towerProgress;
		private Timer _autoSaveTimer;
		
		// 当前状态
		private int _currentFloor = 1;
		private TowerFloorType _currentFloorType = TowerFloorType.UrbanEnvironment;
		private MapState _currentMapState = MapState.NotGenerated;
		
		public override void _Ready()
		{
			InitializeTowerManager();
			SetupAutoSave();
			LoadTowerProgress();
			
			if (EnableDebugMode)
			{
				GD.Print("[TowerManager] 爬塔系统初始化完成");
			}
		}
		
		/// <summary>
		/// 初始化爬塔管理器
		/// </summary>
		private void InitializeTowerManager()
		{
			_mapGenerator = new MapGenerator();
			_floorMaps = new Godot.Collections.Dictionary<int, MapTopology>();
			_floorConfigs = new Godot.Collections.Dictionary<int, MapGenerationConfig>();
			_towerProgress = new TowerProgress();
			
			InitializeFloorConfigs();
		}
		
		/// <summary>
		/// 初始化楼层配置
		/// </summary>
		private void InitializeFloorConfigs()
		{
			for (int floor = 1; floor <= TotalTowerFloors; floor++)
			{
				var floorType = GetFloorType(floor);
				var config = CreateFloorConfig(floor, floorType);
				_floorConfigs[floor] = config;
			}
			
			// 特殊处理隐藏楼层（第0层）
			var hiddenConfig = CreateFloorConfig(0, TowerFloorType.HiddenRuins);
			_floorConfigs[0] = hiddenConfig;
		}
		
		/// <summary>
		/// 根据楼层数获取楼层类型
		/// </summary>
		private TowerFloorType GetFloorType(int floor)
		{
			if (floor == 0) return TowerFloorType.HiddenRuins;
			
			// 51层修仙高塔的楼层分布
			if (floor >= 1 && floor <= 17)
				return TowerFloorType.UrbanEnvironment; // 凡俗层 1-17
			else if (floor >= 18 && floor <= 34)
				return TowerFloorType.SpiritualRealm; // 灵能层 18-34
			else if (floor >= 35 && floor <= 51)
				return TowerFloorType.TechFusion; // 天机层 35-51
			else
				return TowerFloorType.UrbanEnvironment; // 默认
		}
		
		/// <summary>
		/// 创建楼层配置
		/// </summary>
		private MapGenerationConfig CreateFloorConfig(int floor, TowerFloorType floorType)
		{
			var config = new MapGenerationConfig
			{
				ConfigId = $"floor_{floor}",
				ConfigName = $"第{floor}层",
				Description = $"51层修仙高塔 - 第{floor}层 ({GetFloorTypeName(floorType)})",
				FloorLevel = floor,
				FloorType = floorType
			};
			
			// 根据楼层调整难度
			var difficulty = CalculateFloorDifficulty(floor);
			config.ApplyDifficulty(difficulty);
			config.ApplyFloorType(floorType);
			
			// 根据楼层位置调整生成规则
			var generationRule = GetFloorGenerationRule(floor, floorType);
			config.ApplyGenerationRule(generationRule);
			
			return config;
		}
		
		/// <summary>
		/// 计算楼层难度
		/// </summary>
		private MapDifficulty CalculateFloorDifficulty(int floor)
		{
			if (floor == 0) return MapDifficulty.Master; // 隐藏层最高难度
			
			float difficultyRatio = (float)floor / TotalTowerFloors;
			
			if (difficultyRatio <= 0.2f) return MapDifficulty.Easy;
			if (difficultyRatio <= 0.4f) return MapDifficulty.Normal;
			if (difficultyRatio <= 0.6f) return MapDifficulty.Hard;
			if (difficultyRatio <= 0.8f) return MapDifficulty.Expert;
			return MapDifficulty.Master;
		}
		
		/// <summary>
		/// 获取楼层生成规则
		/// </summary>
		private MapGenerationRule GetFloorGenerationRule(int floor, TowerFloorType floorType)
		{
			// 每个大境界的最后一层（17, 34, 51）使用挑战规则
			if (floor == 17 || floor == 34 || floor == 51)
				return MapGenerationRule.Challenging;
				
			// 隐藏层使用挑战规则
			if (floor == 0)
				return MapGenerationRule.Challenging;
				
			// 根据楼层类型选择规则
			switch (floorType)
			{
				case TowerFloorType.UrbanEnvironment:
					return floor % 5 == 0 ? MapGenerationRule.ResourceFocused : MapGenerationRule.Balanced;
					
				case TowerFloorType.SpiritualRealm:
					return floor % 3 == 0 ? MapGenerationRule.ExplorationFocused : MapGenerationRule.EventFocused;
					
				case TowerFloorType.TechFusion:
					return floor % 4 == 0 ? MapGenerationRule.CombatFocused : MapGenerationRule.Balanced;
					
				default:
					return MapGenerationRule.Balanced;
			}
		}
		
		/// <summary>
		/// 获取楼层类型名称
		/// </summary>
		private string GetFloorTypeName(TowerFloorType floorType)
		{
			return floorType switch
			{
				TowerFloorType.UrbanEnvironment => "凡俗层",
				TowerFloorType.SpiritualRealm => "灵能层",
				TowerFloorType.TechFusion => "天机层",
				TowerFloorType.HiddenRuins => "塔底废墟",
				_ => "未知层"
			};
		}
		
		/// <summary>
		/// 设置自动保存
		/// </summary>
		private void SetupAutoSave()
		{
			if (!EnableAutoSave) return;
			
			_autoSaveTimer = new Timer();
			_autoSaveTimer.WaitTime = AutoSaveInterval;
			_autoSaveTimer.Autostart = true;
			_autoSaveTimer.Timeout += SaveTowerProgress;
			AddChild(_autoSaveTimer);
		}
		
		/// <summary>
		/// 生成指定楼层的地图
		/// </summary>
		public MapTopology GenerateFloorMap(int floor, MapGenerationConfig customConfig = null)
		{
			if (EnableDebugMode)
			{
				GD.Print($"[TowerManager] 开始生成第{floor}层地图");
			}
			
			var config = customConfig ?? _floorConfigs.GetValueOrDefault(floor);
			if (config == null)
			{
				GD.PrintErr($"[TowerManager] 找不到第{floor}层的配置");
				return null;
			}
			
			// 验证配置
			if (!config.ValidateConfig())
			{
				GD.PrintErr($"[TowerManager] 第{floor}层配置无效");
				return null;
			}
			
			// 生成地图
			var map = _mapGenerator.GenerateMap(config);
			if (map == null)
			{
				GD.PrintErr($"[TowerManager] 第{floor}层地图生成失败");
				return null;
			}
			
			// 缓存地图
			_floorMaps[floor] = map;
			
			// 如果是当前楼层，设置为当前地图
			if (floor == _currentFloor)
			{
				SetCurrentMap(map);
			}
			
			EmitSignal(SignalName.MapGenerated, map);
			
			if (EnableDebugMode)
			{
				GD.Print($"[TowerManager] 第{floor}层地图生成完成，包含{map.Nodes.Count}个节点");
			}
			
			return map;
		}
		
		/// <summary>
		/// 设置当前地图
		/// </summary>
		private void SetCurrentMap(MapTopology map)
		{
			var oldState = _currentMapState;
			_currentMap = map;
			_currentMapState = map?.State ?? MapState.NotGenerated;
			
			if (oldState != _currentMapState)
			{
				EmitSignal(SignalName.MapStateChanged, (int)oldState, (int)_currentMapState);
			}
		}
		
		/// <summary>
		/// 进入指定楼层
		/// </summary>
		public bool EnterFloor(int floor)
		{
			if (floor < 0 || floor > TotalTowerFloors)
			{
				GD.PrintErr($"[TowerManager] 无效的楼层: {floor}");
				return false;
			}
			
			// 检查是否可以进入该楼层
			if (!CanEnterFloor(floor))
			{
				GD.PrintErr($"[TowerManager] 无法进入第{floor}层，条件不满足");
				return false;
			}
			
			_currentFloor = floor;
			_currentFloorType = GetFloorType(floor);
			
			// 获取或生成该楼层的地图
			var map = GetFloorMap(floor);
			if (map == null)
			{
				map = GenerateFloorMap(floor);
				if (map == null)
				{
					return false;
				}
			}
			
			SetCurrentMap(map);
			
			// 设置起始节点为当前节点
			var startNode = map.GetStartNode();
			if (startNode != null)
			{
				SetCurrentNode(startNode);
			}
			
			// 更新塔进度
			_towerProgress.CurrentFloor = floor;
			_towerProgress.UnlockedFloors.Add(floor);
			
			EmitSignal(SignalName.TowerProgressChanged, _currentFloor, TotalTowerFloors);
			
			if (EnableDebugMode)
			{
				GD.Print($"[TowerManager] 进入第{floor}层 ({GetFloorTypeName(_currentFloorType)})");
			}
			
			return true;
		}
		
		/// <summary>
		/// 检查是否可以进入指定楼层
		/// </summary>
		private bool CanEnterFloor(int floor)
		{
			// 第1层总是可以进入
			if (floor == 1) return true;
			
			// 隐藏层需要特殊条件
			if (floor == 0)
			{
				return _towerProgress.HasCompletedFloor(51); // 需要完成最高层
			}
			
			// 其他楼层需要完成前一层
			return _towerProgress.HasCompletedFloor(floor - 1);
		}
		
		/// <summary>
		/// 获取楼层地图
		/// </summary>
		public MapTopology GetFloorMap(int floor)
		{
			return _floorMaps.GetValueOrDefault(floor);
		}
		
		/// <summary>
		/// 获取当前玩家状态
		/// </summary>
		public Godot.Collections.Dictionary<string, Variant> GetPlayerState()
		{
			var playerState = new Godot.Collections.Dictionary<string, Variant>();
			
			// 添加基础玩家状态信息
			if (_towerProgress != null)
			{
				playerState["CurrentFloor"] = _currentFloor;
				playerState["CompletedFloors"] = _towerProgress.CompletedFloors.Count;
				playerState["UnlockedFloors"] = _towerProgress.UnlockedFloors.Count;
				playerState["TotalPlayTime"] = _towerProgress.TotalPlayTime;
			}
			
			// 可以从GameData获取更多玩家信息
			var gameData = NodeUtils.GetGameData(this);
			if (gameData != null)
			{
				playerState["PlayerLevel"] = gameData.PlayerLevel;
				playerState["PlayerExperience"] = gameData.PlayerExperience;
				playerState["PlayerHealth"] = gameData.PlayerHealth;
				playerState["PlayerMaxHealth"] = gameData.PlayerMaxHealth;
			}
			
			return playerState;
		}
		
		/// <summary>
		/// 进入指定节点
		/// </summary>
		public bool EnterNode(string nodeId)
		{
			if (_currentMap == null)
			{
				GD.PrintErr("[TowerManager] 当前没有活动地图");
				return false;
			}
			
			var node = _currentMap.GetNode(nodeId);
			if (node == null)
			{
				GD.PrintErr($"[TowerManager] 找不到节点: {nodeId}");
				return false;
			}
			
			// 检查节点是否可访问
			var playerState = GetPlayerState();
			if (!node.CanAccess(playerState))
			{
				GD.PrintErr($"[TowerManager] 节点{nodeId}不可访问");
				return false;
			}
			
			SetCurrentNode(node);
			EmitSignal(SignalName.NodeEntered, node);
			
			if (EnableDebugMode)
			{
				GD.Print($"[TowerManager] 进入节点: {node.NodeName} ({node.NodeType})");
			}
			
			return true;
		}
		
		/// <summary>
		/// 设置当前节点
		/// </summary>
		private void SetCurrentNode(MapNode node)
		{
			_currentNode = node;
			
			if (node != null)
			{
				node.IsVisited = true;
				_currentMap?.SetCurrentNode(node.NodeId);
			}
		}
		
		/// <summary>
		/// 完成当前节点
		/// </summary>
		public void CompleteCurrentNode(bool success = true)
		{
			if (_currentNode == null)
			{
				GD.PrintErr("[TowerManager] 没有当前节点可完成");
				return;
			}
			
			if (success)
			{
				_currentNode.IsCompleted = true;
				
				// 解锁连接的节点
				var connectedNodes = _currentNode.ConnectedNodeIds;
				foreach (var nodeId in connectedNodes)
				{
					var connectedNode = _currentMap?.GetNode(nodeId);
					if (connectedNode != null)
					{
						if (connectedNode.State == MapState.Locked)
						{
							connectedNode.State = MapState.Available;
						}
					}
				}
			}
			
			EmitSignal(SignalName.NodeCompleted, _currentNode, success);
			
			// 检查是否完成了楼层
			CheckFloorCompletion();
			
			if (EnableDebugMode)
			{
				GD.Print($"[TowerManager] 节点完成: {_currentNode.NodeName} (成功: {success})");
			}
		}
		
		/// <summary>
		/// 检查楼层完成状态
		/// </summary>
		private void CheckFloorCompletion()
		{
			if (_currentMap == null) return;
			
			var exitNodes = _currentMap.ExitNodeIds;
			var exitNode = exitNodes.Count > 0 ? _currentMap.GetNode(exitNodes[0]) : null;
			if (exitNode != null && exitNode.IsCompleted)
			{
				CompleteFloor(_currentFloor);
			}
		}
		
		/// <summary>
		/// 完成楼层
		/// </summary>
		private void CompleteFloor(int floor)
		{
			_towerProgress.CompleteFloor(floor);
			
			// 解锁下一层
			if (floor < TotalTowerFloors)
			{
				_towerProgress.UnlockedFloors.Add(floor + 1);
			}
			
			// 如果完成了第51层，解锁隐藏层
			if (floor == 51)
			{
				_towerProgress.UnlockedFloors.Add(0);
			}
			
			EmitSignal(SignalName.FloorCompleted, floor, (int)_currentFloorType);
			
			if (EnableDebugMode)
			{
				GD.Print($"[TowerManager] 完成第{floor}层");
			}
		}
		
		/// <summary>
		/// 获取可用的连接节点
		/// </summary>
		public Godot.Collections.Array<MapNode> GetAvailableConnections()
		{
			var availableNodes = new Godot.Collections.Array<MapNode>();
			
			if (_currentNode == null || _currentMap == null)
				return availableNodes;
			
			var playerState = GetPlayerState();
			var connectedIds = _currentNode.ConnectedNodeIds;
			foreach (var nodeId in connectedIds)
			{
				var node = _currentMap.GetNode(nodeId);
				if (node != null && node.CanAccess(playerState))
				{
					availableNodes.Add(node);
				}
			}
			
			return availableNodes;
		}
		
		/// <summary>
		/// 重新生成当前楼层地图
		/// </summary>
		public bool RegenerateCurrentFloor(MapGenerationConfig customConfig = null)
		{
			if (_currentFloor <= 0)
			{
				GD.PrintErr("[TowerManager] 无效的当前楼层");
				return false;
			}
			
			// 清除缓存的地图
			_floorMaps.Remove(_currentFloor);
			
			// 重新生成
			var newMap = GenerateFloorMap(_currentFloor, customConfig);
			if (newMap == null)
			{
				return false;
			}
			
			// 重置到起始节点
			var startNode = newMap.GetStartNode();
			if (startNode != null)
			{
				SetCurrentNode(startNode);
			}
			
			if (EnableDebugMode)
			{
				GD.Print($"[TowerManager] 重新生成第{_currentFloor}层地图");
			}
			
			return true;
		}
		
		/// <summary>
		/// 保存塔进度
		/// </summary>
		public void SaveTowerProgress()
		{
			if (_towerProgress == null) return;
			
			try
			{
				var saveData = new Godot.Collections.Dictionary<string, Variant>
				{
					["CurrentFloor"] = _towerProgress.CurrentFloor,
					["CompletedFloors"] = _towerProgress.CompletedFloors.ToArray(),
					["UnlockedFloors"] = _towerProgress.UnlockedFloors.ToArray(),
					["TotalPlayTime"] = _towerProgress.TotalPlayTime,
					["LastSaveTime"] = Time.GetUnixTimeFromSystem()
				};
				
				var json = Json.Stringify(saveData);
				var file = FileAccess.Open("user://tower_progress.save", FileAccess.ModeFlags.Write);
				if (file != null)
				{
					file.StoreString(json);
					file.Close();
					
					if (EnableDebugMode)
					{
						GD.Print("[TowerManager] 塔进度已保存");
					}
				}
			}
			catch (System.Exception e)
			{
				GD.PrintErr($"[TowerManager] 保存塔进度失败: {e.Message}");
			}
		}
		
		/// <summary>
		/// 加载塔进度
		/// </summary>
		public void LoadTowerProgress()
		{
			try
			{
				var file = FileAccess.Open("user://tower_progress.save", FileAccess.ModeFlags.Read);
				if (file == null)
				{
					// 文件不存在，创建新的进度
					_towerProgress = new TowerProgress();
					_towerProgress.UnlockedFloors.Add(1); // 解锁第一层
					return;
				}
				
				var json = file.GetAsText();
				file.Close();
				
				var jsonParser = new Json();
				var parseResult = jsonParser.Parse(json);
				
				if (parseResult == Error.Ok && jsonParser.Data.AsGodotDictionary() is Godot.Collections.Dictionary saveData)
				{
					_towerProgress = new TowerProgress
					{
						CurrentFloor = saveData.GetValueOrDefault("CurrentFloor", 1).AsInt32(),
						TotalPlayTime = saveData.GetValueOrDefault("TotalPlayTime", 0.0f).AsSingle()
					};
					
					// 加载完成的楼层
					if (saveData.ContainsKey("CompletedFloors"))
					{
						var completedArray = saveData["CompletedFloors"].AsGodotArray();
						if (completedArray != null)
						{
							foreach (Variant floor in completedArray)
							{
								_towerProgress.CompletedFloors.Add(floor.AsInt32());
							}
						}
					}
					
					// 加载解锁的楼层
					if (saveData.ContainsKey("UnlockedFloors"))
					{
						var unlockedArray = saveData["UnlockedFloors"].AsGodotArray();
						if (unlockedArray != null)
						{
							foreach (Variant floor in unlockedArray)
							{
								_towerProgress.UnlockedFloors.Add(floor.AsInt32());
							}
						}
					}
					
					// 确保至少解锁第一层
					if (_towerProgress.UnlockedFloors.Count == 0)
					{
						_towerProgress.UnlockedFloors.Add(1);
					}
					
					if (EnableDebugMode)
					{
						GD.Print($"[TowerManager] 塔进度已加载，当前楼层: {_towerProgress.CurrentFloor}");
					}
				}
				else
				{
					GD.PrintErr("[TowerManager] 塔进度文件格式错误");
					_towerProgress = new TowerProgress();
					_towerProgress.UnlockedFloors.Add(1);
				}
			}
			catch (System.Exception e)
			{
				GD.PrintErr($"[TowerManager] 加载塔进度失败: {e.Message}");
				_towerProgress = new TowerProgress();
				_towerProgress.UnlockedFloors.Add(1);
			}
		}
		
		/// <summary>
		/// 获取塔统计信息
		/// </summary>
		public Godot.Collections.Dictionary<string, Variant> GetTowerStats()
		{
			var stats = new Godot.Collections.Dictionary<string, Variant>
			{
				["CurrentFloor"] = _currentFloor,
				["TotalFloors"] = TotalTowerFloors,
				["CompletedFloors"] = _towerProgress?.CompletedFloors.Count ?? 0,
				["UnlockedFloors"] = _towerProgress?.UnlockedFloors.Count ?? 0,
				["TotalPlayTime"] = _towerProgress?.TotalPlayTime ?? 0.0f,
				["CurrentFloorType"] = GetFloorTypeName(_currentFloorType),
				["MapState"] = _currentMapState.ToString(),
				["CurrentNodeId"] = _currentNode?.NodeId ?? "",
				["CurrentNodeType"] = _currentNode?.NodeType.ToString() ?? ""
			};
			
			if (_currentMap != null)
			{
				var mapStats = _currentMap.GetMapStatistics();
				foreach (var kvp in mapStats)
				{
					stats[$"Map_{kvp.Key}"] = kvp.Value;
				}
			}
			
			return stats;
		}
		
		/// <summary>
		/// 重置塔进度
		/// </summary>
		public void ResetTowerProgress()
		{
			_towerProgress = new TowerProgress();
			_towerProgress.UnlockedFloors.Add(1);
			_currentFloor = 1;
			_currentFloorType = TowerFloorType.UrbanEnvironment;
			_currentNode = null;
			_currentMap = null;
			_currentMapState = MapState.NotGenerated;
			
			// 清除所有缓存的地图
			_floorMaps.Clear();
			
			SaveTowerProgress();
			
			if (EnableDebugMode)
			{
				GD.Print("[TowerManager] 塔进度已重置");
			}
		}
		
		// 公共属性访问器
		public int CurrentFloor => _currentFloor;
		public TowerFloorType CurrentFloorType => _currentFloorType;
		public MapTopology CurrentMap => _currentMap;
		public MapNode CurrentNode => _currentNode;
		public MapState CurrentMapState => _currentMapState;
		public TowerProgress TowerProgress => _towerProgress;
		
		public override void _ExitTree()
		{
			// 退出时保存进度
			SaveTowerProgress();
		}
	}
	
	/// <summary>
	/// 塔进度数据类
	/// </summary>
	public partial class TowerProgress : RefCounted
	{
		public int CurrentFloor { get; set; } = 1;
		public HashSet<int> CompletedFloors { get; set; } = new HashSet<int>();
		public HashSet<int> UnlockedFloors { get; set; } = new HashSet<int>();
		public float TotalPlayTime { get; set; } = 0.0f;
		
		public bool HasCompletedFloor(int floor)
		{
			return CompletedFloors.Contains(floor);
		}
		
		public bool IsFloorUnlocked(int floor)
		{
			return UnlockedFloors.Contains(floor);
		}
		
		public void CompleteFloor(int floor)
		{
			CompletedFloors.Add(floor);
			UnlockedFloors.Add(floor);
		}
		
		public void UnlockFloor(int floor)
		{
			UnlockedFloors.Add(floor);
		}
		
		public float GetCompletionPercentage(int totalFloors)
		{
			return (float)CompletedFloors.Count / totalFloors * 100.0f;
		}
	}
}
