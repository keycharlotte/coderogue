using Godot;
using CodeRogue.Tower;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using CodeRogue.Tower.UI;
using CodeRogue.Data;

namespace CodeRogue.Tower.Test
{
	/// <summary>
	/// 爬塔系统测试场景
	/// </summary>
	public partial class TowerTestScene : Control
	{
		[Export] public TowerUI TowerUI { get; set; }
		[Export] public VBoxContainer DebugPanel { get; set; }
		[Export] public Button GenerateTreeMapButton { get; set; }
		[Export] public Button NextFloorButton { get; set; }
		[Export] public Button PrevFloorButton { get; set; }
		[Export] public Button ResetProgressButton { get; set; }
		[Export] public Label DebugInfoLabel { get; set; }
		[Export] public SpinBox FloorSpinBox { get; set; }
		[Export] public OptionButton DifficultySelector { get; set; }
		[Export] public CheckBox DebugModeCheckBox { get; set; }
		
		private TowerManager _towerManager;
		private MapGenerationConfig _testConfig;
		
		public override void _Ready()
		{
			InitializeTestScene();
			ConnectSignals();
			SetupDebugControls();
		}
		
		/// <summary>
		/// 初始化测试场景
		/// </summary>
		private void InitializeTestScene()
		{
			// 获取TowerManager引用
			_towerManager = GetNode<TowerManager>("/root/TowerManager");
			if (_towerManager == null)
			{
				GD.PrintErr("[TowerTestScene] 找不到TowerManager，请确保已配置为AutoLoad");
				return;
			}
			
			// 创建测试配置
			_testConfig = new MapGenerationConfig();
			_testConfig.InitializeDefaults();
			
			// 设置TowerUI的调试模式
			TowerUI?.SetDebugMode(true);
			
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 连接信号
		/// </summary>
		private void ConnectSignals()
		{
			if (_towerManager != null)
			{
				_towerManager.MapGenerated += OnMapGenerated;
				_towerManager.NodeEntered += OnNodeEntered;
				_towerManager.NodeCompleted += OnNodeCompleted;
				_towerManager.FloorCompleted += OnFloorCompleted;
				_towerManager.TowerProgressChanged += OnTowerProgressChanged;
			}
			
			if (TowerUI != null)
			{
				TowerUI.NodeSelected += OnNodeSelected;
				TowerUI.FloorChangeRequested += OnFloorChangeRequested;
				TowerUI.MapRegenerateRequested += OnMapRegenerateRequested;
			}
			
			// 连接调试按钮
			if (GenerateTreeMapButton != null)
				GenerateTreeMapButton.Pressed += GenerateTestMap;
			
			if (NextFloorButton != null)
				NextFloorButton.Pressed += OnNextFloorPressed;
			
			if (PrevFloorButton != null)
				PrevFloorButton.Pressed += OnPrevFloorPressed;
			
			if (ResetProgressButton != null)
				ResetProgressButton.Pressed += OnResetProgressPressed;
			
			if (DebugModeCheckBox != null)
				DebugModeCheckBox.Toggled += OnDebugModeToggled;
		}
		
		/// <summary>
		/// 设置调试控件
		/// </summary>
		private void SetupDebugControls()
		{
			// 设置楼层选择器
			if (FloorSpinBox != null)
			{
				FloorSpinBox.MinValue = 0;
				FloorSpinBox.MaxValue = 51;
				FloorSpinBox.Value = 1;
				FloorSpinBox.ValueChanged += OnFloorSpinBoxChanged;
			}
			

			// 设置难度选择器
			if (DifficultySelector != null)
			{
				DifficultySelector.Clear();
				DifficultySelector.AddItem("简单", (int)MapDifficulty.Easy);
				DifficultySelector.AddItem("普通", (int)MapDifficulty.Normal);
				DifficultySelector.AddItem("困难", (int)MapDifficulty.Hard);
				DifficultySelector.AddItem("专家", (int)MapDifficulty.Expert);
				DifficultySelector.AddItem("大师", (int)MapDifficulty.Master);
				DifficultySelector.ItemSelected += OnDifficultySelected;
			}
		}
		
		/// <summary>
		/// 生成测试地图（树状地图）
		/// </summary>
		private void GenerateTestMap()
		{
			if (_towerManager == null || _testConfig == null) return;
			
			_testConfig.TopologyType = MapTopologyType.Tree;
			_testConfig.FloorLevel = (int)(FloorSpinBox?.Value ?? 1);
			
			// 根据楼层设置难度和类型
			var floorLevel = _testConfig.FloorLevel;
			_testConfig.FloorType = GetFloorType(floorLevel);
			_testConfig.Difficulty = GetFloorDifficulty(floorLevel);
			
			GD.Print($"[TowerTestScene] 生成测试地图: 楼层{floorLevel}, 类型树状, 难度{_testConfig.Difficulty}");
			
			_towerManager.GenerateFloorMap(floorLevel, _testConfig);
			
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 获取楼层类型
		/// </summary>
		private TowerFloorType GetFloorType(int floorLevel)
		{
			if (floorLevel == 0)
				return TowerFloorType.HiddenRuins;
			else if (floorLevel <= 17)
				return TowerFloorType.UrbanEnvironment;
			else if (floorLevel <= 34)
				return TowerFloorType.SpiritualRealm;
			else
				return TowerFloorType.TechFusion;
		}
		
		/// <summary>
		/// 获取楼层难度
		/// </summary>
		private MapDifficulty GetFloorDifficulty(int floorLevel)
		{
			if (floorLevel == 0)
				return MapDifficulty.Master;
			else if (floorLevel <= 10)
				return MapDifficulty.Easy;
			else if (floorLevel <= 25)
				return MapDifficulty.Normal;
			else if (floorLevel <= 40)
				return MapDifficulty.Hard;
			else if (floorLevel <= 50)
				return MapDifficulty.Expert;
			else
				return MapDifficulty.Master;
		}
		
		/// <summary>
		/// 获取当前玩家状态
		/// </summary>
		private Godot.Collections.Dictionary<string, Variant> GetPlayerState()
		{
			var playerState = new Godot.Collections.Dictionary<string, Variant>();
			
			if (_towerManager != null)
			{
				var towerProgress = _towerManager.TowerProgress;
				if (towerProgress != null)
				{
					playerState["CurrentFloor"] = _towerManager.CurrentFloor;
					playerState["CompletedFloors"] = towerProgress.CompletedFloors.Count;
					playerState["UnlockedFloors"] = towerProgress.UnlockedFloors.Count;
					playerState["TotalPlayTime"] = towerProgress.TotalPlayTime;
				}
			}
			
			// 可以从GameData获取更多玩家信息
			var gameData = GetNode<GameData>("/root/GameData");
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
		/// 更新调试信息
		/// </summary>
		private void UpdateDebugInfo()
		{
			if (DebugInfoLabel == null || _towerManager == null) return;
			
			var info = "=== 爬塔系统调试信息 ===\n";
			info += $"当前楼层: {_towerManager.CurrentFloor}\n";
			info += $"楼层类型: {_towerManager.CurrentFloorType}\n";
			
			var progress = _towerManager.TowerProgress;
			if (progress != null)
			{
				info += $"解锁楼层: {progress.UnlockedFloors.Count}\n";
				info += $"完成楼层: {progress.CompletedFloors.Count}\n";
				info += $"总进度: {progress.GetCompletionPercentage(_towerManager.TotalTowerFloors):F1}%\n";
			}
			
			var currentMap = _towerManager.CurrentMap;
			if (currentMap != null)
			{
				var nodes = currentMap.Nodes;
				var completedNodes = nodes.Where(n => n.IsCompleted).Count();
				info += $"\n=== 当前地图信息 ===\n";
				info += $"地图ID: {currentMap.MapId}\n";
				info += $"拓扑类型: {currentMap.TopologyType}\n";
				info += $"难度: {currentMap.Difficulty}\n";
				info += $"节点总数: {nodes.Count}\n";
				info += $"已完成节点: {completedNodes}\n";
				info += $"地图状态: {currentMap.State}\n";
			}
			
			var currentNode = _towerManager.CurrentNode;
			if (currentNode != null)
			{
				info += $"\n=== 当前节点信息 ===\n";
				info += $"节点ID: {currentNode.NodeId}\n";
				info += $"节点类型: {currentNode.NodeType}\n";
				info += $"节点状态: {(currentNode.IsCompleted ? "已完成" : currentNode.IsVisited ? "已访问" : "未访问")}\n";
				var playerState = GetPlayerState();
				info += $"可访问: {currentNode.CanAccess(playerState)}\n";
			}
			
			DebugInfoLabel.Text = info;
		}
		
		/// <summary>
		/// 地图生成事件处理
		/// </summary>
		private void OnMapGenerated(MapTopology mapTopology)
		{
			GD.Print($"[TowerTestScene] 地图已生成: {mapTopology.MapId}, 节点数: {mapTopology.Nodes.Count}");
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 节点进入事件处理
		/// </summary>
		private void OnNodeEntered(MapNode node)
		{
			GD.Print($"[TowerTestScene] 进入节点: {node.NodeId} ({node.NodeType})");
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 节点完成事件处理
		/// </summary>
		private void OnNodeCompleted(MapNode node, bool success)
		{
			GD.Print($"[TowerTestScene] 节点完成: {node.NodeId}, 成功: {success}");
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 楼层完成事件处理
		/// </summary>
		private void OnFloorCompleted(int floorLevel, TowerFloorType floorType)
		{
			GD.Print($"[TowerTestScene] 楼层完成: 第{floorLevel}层 ({floorType})");
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 塔进度变化事件处理
		/// </summary>
		private void OnTowerProgressChanged(int currentFloor, int totalFloors)
		{
			GD.Print($"[TowerTestScene] 塔进度变化: {currentFloor}/{totalFloors}");
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 节点选择事件处理
		/// </summary>
		private void OnNodeSelected(string nodeId)
		{
			GD.Print($"[TowerTestScene] 选择节点: {nodeId}");
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 楼层变更请求事件处理
		/// </summary>
		private void OnFloorChangeRequested(int targetFloor)
		{
			GD.Print($"[TowerTestScene] 请求变更楼层: {targetFloor}");
		}
		
		/// <summary>
		/// 地图重新生成请求事件处理
		/// </summary>
		private void OnMapRegenerateRequested()
		{
			GD.Print("[TowerTestScene] 请求重新生成地图");
		}
		
		/// <summary>
		/// 下一层按钮事件
		/// </summary>
		private void OnNextFloorPressed()
		{
			if (_towerManager == null) return;
			
			var nextFloor = _towerManager.CurrentFloor + 1;
			if (nextFloor <= _towerManager.TotalTowerFloors)
			{
				_towerManager.EnterFloor(nextFloor);
				
				if (FloorSpinBox != null)
				{
					FloorSpinBox.Value = nextFloor;
				}
			}
		}
		
		/// <summary>
		/// 上一层按钮事件
		/// </summary>
		private void OnPrevFloorPressed()
		{
			if (_towerManager == null) return;
			
			var prevFloor = _towerManager.CurrentFloor - 1;
			if (prevFloor >= 0)
			{
				_towerManager.EnterFloor(prevFloor);
				
				if (FloorSpinBox != null)
				{
					FloorSpinBox.Value = prevFloor;
				}
			}
		}
		
		/// <summary>
		/// 重置进度按钮事件
		/// </summary>
		private void OnResetProgressPressed()
		{
			if (_towerManager == null) return;
			
			_towerManager.ResetTowerProgress();
			
			if (FloorSpinBox != null)
			{
				FloorSpinBox.Value = 1;
			}
			
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 楼层数值框变化事件
		/// </summary>
		private void OnFloorSpinBoxChanged(double value)
		{
			// 更新测试配置
			if (_testConfig != null)
			{
				_testConfig.FloorLevel = (int)value;
			}
		}
		

		/// <summary>
		/// 难度选择事件
		/// </summary>
		private void OnDifficultySelected(long index)
		{
			if (DifficultySelector == null || _testConfig == null) return;
			
			var difficulty = (MapDifficulty)DifficultySelector.GetItemId((int)index);
			_testConfig.Difficulty = difficulty;
		}
		
		/// <summary>
		/// 调试模式切换事件
		/// </summary>
		private void OnDebugModeToggled(bool enabled)
		{
			if (TowerUI != null)
			{
				TowerUI.SetDebugMode(enabled);
			}
			
			if (DebugPanel != null)
			{
				DebugPanel.Visible = enabled;
			}
		}
		
		/// <summary>
		/// 模拟节点完成
		/// </summary>
		public void SimulateNodeCompletion(string nodeId, bool success = true)
		{
			if (_towerManager == null) return;
			
			var currentMap = _towerManager.CurrentMap;
			if (currentMap == null) return;
			
			var node = currentMap.GetNode(nodeId);
			if (node == null) return;
			
			// 模拟节点完成
			node.IsCompleted = success;
			node.IsVisited = true;
			
			// 触发事件
			_towerManager.EmitSignal(TowerManager.SignalName.NodeCompleted, node, success);
			
			UpdateDebugInfo();
		}
		
		/// <summary>
		/// 模拟楼层完成
		/// </summary>
		public void SimulateFloorCompletion()
		{
			if (_towerManager == null) return;
			
			var currentFloor = _towerManager.CurrentFloor;
			var floorType = _towerManager.CurrentFloorType;
			
			// 标记楼层为已完成
			var progress = _towerManager.TowerProgress;
			if (progress != null)
			{
				progress.CompleteFloor(currentFloor);
				progress.UnlockFloor(currentFloor + 1);
			}
			
			// 触发事件
			_towerManager.EmitSignal(TowerManager.SignalName.FloorCompleted, currentFloor, (int)floorType);
			
			UpdateDebugInfo();
		}
		
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				switch (keyEvent.Keycode)
				{
					case Key.F1:
						GenerateTestMap();
						break;
					case Key.F5:
						OnResetProgressPressed();
						break;
					case Key.Pageup:
						OnNextFloorPressed();
						break;
					case Key.Pagedown:
						OnPrevFloorPressed();
						break;
				}
			}
		}
	}
}
