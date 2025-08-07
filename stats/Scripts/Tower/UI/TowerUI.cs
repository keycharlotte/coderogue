using Godot;
using Godot.Collections;
using System.Linq;
using CodeRogue.Data;
using CodeRogue.Tower;
using CodeRogue.Utils;

namespace CodeRogue.Tower.UI
{
    /// <summary>
    /// 爬塔系统UI界面 - 显示地图、节点信息和玩家交互
    /// </summary>
    public partial class TowerUI : Control
    {
        [Signal] public delegate void NodeSelectedEventHandler(string nodeId);
        [Signal] public delegate void FloorChangeRequestedEventHandler(int targetFloor);
        [Signal] public delegate void MapRegenerateRequestedEventHandler();
        
        // Export节点引用 - 在.tscn文件中配置
        [Export] public Control MapContainer { get; set; }
        [Export] public Control NodeInfoPanel { get; set; }
        [Export] public Control FloorInfoPanel { get; set; }
        [Export] public Control ProgressPanel { get; set; }
        [Export] public ScrollContainer MapScrollContainer { get; set; }
        [Export] public Button RegenerateMapButton { get; set; }
        [Export] public OptionButton FloorSelector { get; set; }
        [Export] public Label CurrentFloorLabel { get; set; }
        [Export] public Label FloorTypeLabel { get; set; }
        [Export] public ProgressBar FloorProgressBar { get; set; }
        [Export] public RichTextLabel NodeDescriptionLabel { get; set; }
        [Export] public Label NodeTypeLabel { get; set; }
        [Export] public Label NodeStatusLabel { get; set; }
        [Export] public VBoxContainer NodeConnectionsList { get; set; }
        [Export] public Button EnterNodeButton { get; set; }
        [Export] public Control MapViewport { get; set; }
        
        // 地图显示相关
        private TowerManager _towerManager;
        private MapTopology _currentMap;
        private Dictionary<string, Control> _nodeVisuals;
        private Dictionary<string, Line2D> _connectionLines;
        private MapNode _selectedNode;
        private Vector2 _mapOffset = Vector2.Zero;
        private float _mapScale = 1.0f;
        private bool _isDragging = false;
        private Vector2 _lastMousePosition;
        
        // UI配置
        private readonly Vector2 NodeSize = new Vector2(60, 60);
        private readonly Color NodeColorNormal = Colors.LightBlue;
        private readonly Color NodeColorCompleted = Colors.Green;
        private readonly Color NodeColorLocked = Colors.Gray;
        private readonly Color NodeColorCurrent = Colors.Yellow;
        private readonly Color NodeColorSelected = Colors.Orange;
        private readonly Color ConnectionColorNormal = Colors.White;
        private readonly Color ConnectionColorActive = Colors.Cyan;
        
        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
            SetupMapInteraction();
        }
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            _nodeVisuals = new Dictionary<string, Control>();
            _connectionLines = new Dictionary<string, Line2D>();
            
            // 获取TowerManager引用
            _towerManager = NodeUtils.GetTowerManager(this);
            if (_towerManager == null)
            {
                GD.PrintErr("[TowerUI] 找不到TowerManager");
                return;
            }
            
            // 初始化楼层选择器
            InitializeFloorSelector();
            
            // 隐藏节点信息面板
            if (NodeInfoPanel != null)
            {
                NodeInfoPanel.Visible = false;
            }
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
                _towerManager.MapStateChanged += OnMapStateChanged;
            }
            
            if (RegenerateMapButton != null)
            {
                RegenerateMapButton.Pressed += OnRegenerateMapPressed;
            }
            
            if (FloorSelector != null)
            {
                FloorSelector.ItemSelected += OnFloorSelected;
            }
            
            if (EnterNodeButton != null)
            {
                EnterNodeButton.Pressed += OnEnterNodePressed;
            }
        }
        
        /// <summary>
        /// 设置地图交互
        /// </summary>
        private void SetupMapInteraction()
        {
            if (MapScrollContainer != null)
            {
                MapScrollContainer.GuiInput += OnMapInput;
            }
        }
        
        /// <summary>
        /// 初始化楼层选择器
        /// </summary>
        private void InitializeFloorSelector()
        {
            if (FloorSelector == null || _towerManager == null) return;
            
            FloorSelector.Clear();
            
            var progress = _towerManager.TowerProgress;
            if (progress == null) return;
            
            // 添加解锁的楼层
            var unlockedFloors = progress.UnlockedFloors.OrderBy(f => f).ToList();
            foreach (var floor in unlockedFloors)
            {
                var floorName = floor == 0 ? "塔底废墟" : $"第{floor}层";
                FloorSelector.AddItem(floorName, floor);
            }
            
            // 选择当前楼层
            var currentFloor = _towerManager.CurrentFloor;
            for (int i = 0; i < FloorSelector.ItemCount; i++)
            {
                if (FloorSelector.GetItemId(i) == currentFloor)
                {
                    FloorSelector.Selected = i;
                    break;
                }
            }
        }
        
        /// <summary>
        /// 处理地图输入
        /// </summary>
        private void OnMapInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                HandleMouseButton(mouseButton);
            }
            else if (@event is InputEventMouseMotion mouseMotion)
            {
                HandleMouseMotion(mouseMotion);
            }
            else if (@event is InputEventKey keyEvent)
            {
                HandleKeyInput(keyEvent);
            }
        }
        
        /// <summary>
        /// 处理鼠标按钮事件
        /// </summary>
        private void HandleMouseButton(InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    _isDragging = true;
                    _lastMousePosition = mouseButton.Position;
                    
                    // 检查是否点击了节点
                    var clickedNode = GetNodeAtPosition(mouseButton.Position);
                    if (clickedNode != null)
                    {
                        SelectNode(clickedNode);
                    }
                    else
                    {
                        DeselectNode();
                    }
                }
                else
                {
                    _isDragging = false;
                }
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                ZoomMap(1.1f, mouseButton.Position);
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                ZoomMap(0.9f, mouseButton.Position);
            }
        }
        
        /// <summary>
        /// 处理鼠标移动事件
        /// </summary>
        private void HandleMouseMotion(InputEventMouseMotion mouseMotion)
        {
            if (_isDragging)
            {
                var delta = mouseMotion.Position - _lastMousePosition;
                _mapOffset += delta;
                _lastMousePosition = mouseMotion.Position;
                UpdateMapTransform();
            }
        }
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyInput(InputEventKey keyEvent)
        {
            if (!keyEvent.Pressed) return;
            
            switch (keyEvent.Keycode)
            {
                case Key.R:
                    if (keyEvent.CtrlPressed)
                    {
                        OnRegenerateMapPressed();
                    }
                    break;
                    
                case Key.Space:
                    if (_selectedNode != null)
                    {
                        OnEnterNodePressed();
                    }
                    break;
                    
                case Key.Escape:
                    DeselectNode();
                    break;
            }
        }
        
        /// <summary>
        /// 获取指定位置的节点
        /// </summary>
        private MapNode GetNodeAtPosition(Vector2 position)
        {
            if (_currentMap == null) return null;
            
            // 转换屏幕坐标到地图坐标
            var mapPosition = (position - _mapOffset) / _mapScale;
            
            foreach (var kvp in _nodeVisuals)
            {
                var nodeVisual = kvp.Value;
                var nodeRect = new Rect2(nodeVisual.Position, NodeSize);
                
                if (nodeRect.HasPoint(mapPosition))
                {
                    return _currentMap.GetNode(kvp.Key);
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 缩放地图
        /// </summary>
        private void ZoomMap(float factor, Vector2 zoomCenter)
        {
            var oldScale = _mapScale;
            _mapScale = Mathf.Clamp(_mapScale * factor, 0.5f, 3.0f);
            
            // 调整偏移以保持缩放中心
            var scaleDelta = _mapScale - oldScale;
            _mapOffset -= (zoomCenter - _mapOffset) * (scaleDelta / oldScale);
            
            UpdateMapTransform();
        }
        
        /// <summary>
        /// 更新地图变换
        /// </summary>
        private void UpdateMapTransform()
        {
            if (MapContainer == null) return;
            
            MapContainer.Position = _mapOffset;
            MapContainer.Scale = Vector2.One * _mapScale;
        }
        
        /// <summary>
        /// 选择节点
        /// </summary>
        private void SelectNode(MapNode node)
        {
            if (node == null) return;
            
            _selectedNode = node;
            UpdateNodeVisuals();
            ShowNodeInfo(node);
            
            EmitSignal(SignalName.NodeSelected, node.NodeId);
        }
        
        /// <summary>
        /// 取消选择节点
        /// </summary>
        private void DeselectNode()
        {
            _selectedNode = null;
            UpdateNodeVisuals();
            HideNodeInfo();
        }
        
        /// <summary>
        /// 显示节点信息
        /// </summary>
        private void ShowNodeInfo(MapNode node)
        {
            if (NodeInfoPanel == null) return;
            
            NodeInfoPanel.Visible = true;
            
            if (NodeTypeLabel != null)
            {
                NodeTypeLabel.Text = GetNodeTypeDisplayName(node.NodeType);
            }
            
            if (NodeDescriptionLabel != null)
            {
                NodeDescriptionLabel.Text = node.Description;
            }
            
            if (NodeStatusLabel != null)
            {
                NodeStatusLabel.Text = GetNodeStatusText(node);
            }
            
            if (EnterNodeButton != null)
            {
                var playerState = GetPlayerState();
                EnterNodeButton.Disabled = !node.CanAccess(playerState);
                EnterNodeButton.Text = node.IsVisited ? "重新进入" : "进入";
            }
            
            UpdateNodeConnectionsList(node);
        }
        
        /// <summary>
        /// 隐藏节点信息
        /// </summary>
        private void HideNodeInfo()
        {
            if (NodeInfoPanel != null)
            {
                NodeInfoPanel.Visible = false;
            }
        }
        
        /// <summary>
        /// 更新节点连接列表
        /// </summary>
        private void UpdateNodeConnectionsList(MapNode node)
        {
            if (NodeConnectionsList == null || _currentMap == null) return;
            
            // 清除现有连接
            foreach (Node child in NodeConnectionsList.GetChildren())
            {
                child.QueueFree();
            }
            
            // 添加连接节点
            var connectedIds = node.ConnectedNodeIds;
            foreach (var nodeId in connectedIds)
            {
                var connectedNode = _currentMap.GetNode(nodeId);
                if (connectedNode == null) continue;
                
                var connectionLabel = new Label();
                connectionLabel.Text = $"→ {connectedNode.NodeName} ({GetNodeTypeDisplayName(connectedNode.NodeType)})";
                
                var playerState = GetPlayerState();
                if (connectedNode.CanAccess(playerState))
                {
                    connectionLabel.Modulate = Colors.White;
                }
                else
                {
                    connectionLabel.Modulate = Colors.Gray;
                }
                
                NodeConnectionsList.AddChild(connectionLabel);
            }
        }
        
        /// <summary>
        /// 获取节点类型显示名称
        /// </summary>
        private string GetNodeTypeDisplayName(MapNodeType nodeType)
        {
            return nodeType switch
            {
                MapNodeType.Start => "起始点",
                MapNodeType.Exit => "出口",
                MapNodeType.NormalBattle => "普通战斗",
                MapNodeType.EliteBattle => "精英战斗",
                MapNodeType.BossBattle => "Boss战斗",
                MapNodeType.RestSite => "休息点",
                MapNodeType.Treasure => "宝藏",
                MapNodeType.CardConstruct => "卡牌构筑",
                MapNodeType.TechMarket => "科技市场",
                MapNodeType.CultivatorTavern => "修炼者酒馆",
                MapNodeType.OpenLibrary => "开放图书馆",
                MapNodeType.SpiritualCharge => "灵能充能",
                MapNodeType.MemoryFragment => "记忆碎片",
                MapNodeType.TeleportGate => "传送门",
                MapNodeType.NetworkConsciousness => "网络意识",
                MapNodeType.SectTrial => "宗门试炼",
                MapNodeType.RandomEvent => "随机事件",
                MapNodeType.Choice => "选择事件",
                MapNodeType.Mystery => "神秘事件",
                _ => nodeType.ToString()
            };
        }
        
        /// <summary>
        /// 获取节点状态文本
        /// </summary>
        private string GetNodeStatusText(MapNode node)
        {
            if (node.IsCompleted)
                return "已完成";
            if (node.IsVisited)
                return "已访问";
            var playerState = GetPlayerState();
            if (node.CanAccess(playerState))
                return "可访问";
            return "锁定";
        }
        
        /// <summary>
        /// 地图生成事件处理
        /// </summary>
        private void OnMapGenerated(MapTopology mapTopology)
        {
            _currentMap = mapTopology;
            DisplayMap(mapTopology);
            UpdateFloorInfo();
        }
        
        /// <summary>
        /// 显示地图
        /// </summary>
        private void DisplayMap(MapTopology mapTopology)
        {
            if (MapContainer == null || mapTopology == null) return;
            
            ClearMapDisplay();
            
            var nodes = mapTopology.Nodes;
            if (nodes.Count == 0) return;
            
            // 创建节点视觉效果
            foreach (var node in nodes)
            {
                CreateNodeVisual(node);
            }
            
            // 创建连接线
            foreach (var node in nodes)
            {
                CreateNodeConnections(node);
            }
            
            // 居中显示地图
            CenterMap();
            
            UpdateNodeVisuals();
        }
        
        /// <summary>
        /// 清除地图显示
        /// </summary>
        private void ClearMapDisplay()
        {
            if (MapContainer == null) return;
            
            foreach (Node child in MapContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            _nodeVisuals.Clear();
            _connectionLines.Clear();
        }
        
        /// <summary>
        /// 创建节点视觉效果
        /// </summary>
        private void CreateNodeVisual(MapNode node)
        {
            var nodeControl = new Control();
            nodeControl.Size = NodeSize;
            nodeControl.Position = node.Position - NodeSize / 2;
            
            // 创建节点背景
            var background = new ColorRect();
            background.Size = NodeSize;
            background.Color = GetNodeColor(node);
            nodeControl.AddChild(background);
            
            // 创建节点图标/文字
            var label = new Label();
            label.Text = GetNodeIcon(node.NodeType);
            label.Size = NodeSize;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            nodeControl.AddChild(label);
            
            // 创建边框
            var border = new NinePatchRect();
            border.Size = NodeSize;
            // 这里可以设置边框纹理
            nodeControl.AddChild(border);
            
            MapContainer.AddChild(nodeControl);
            _nodeVisuals[node.NodeId] = nodeControl;
        }
        
        /// <summary>
        /// 创建节点连接
        /// </summary>
        private void CreateNodeConnections(MapNode node)
        {
            if (_currentMap == null) return;
            
            var connectedIds = node.ConnectedNodeIds;
            foreach (var targetId in connectedIds)
            {
                var targetNode = _currentMap.GetNode(targetId);
                if (targetNode == null) continue;
                
                var connectionKey = $"{node.NodeId}->{targetId}";
                if (_connectionLines.ContainsKey(connectionKey)) continue;
                
                var line = new Line2D();
                line.AddPoint(node.Position);
                line.AddPoint(targetNode.Position);
                line.Width = 3.0f;
                line.DefaultColor = ConnectionColorNormal;
                
                MapContainer.AddChild(line);
                _connectionLines[connectionKey] = line;
            }
        }
        
        /// <summary>
        /// 获取节点颜色
        /// </summary>
        private Color GetNodeColor(MapNode node)
        {
            if (_selectedNode == node)
                return NodeColorSelected;
            if (_towerManager?.CurrentNode == node)
                return NodeColorCurrent;
            if (node.IsCompleted)
                return NodeColorCompleted;
            var playerState = GetPlayerState();
            if (!node.CanAccess(playerState))
                return NodeColorLocked;
            return NodeColorNormal;
        }
        
        /// <summary>
        /// 获取节点图标
        /// </summary>
        private string GetNodeIcon(MapNodeType nodeType)
        {
            return nodeType switch
            {
                MapNodeType.Start => "🏠",
                MapNodeType.Exit => "🚪",
                MapNodeType.NormalBattle => "⚔️",
                MapNodeType.EliteBattle => "🛡️",
                MapNodeType.BossBattle => "👑",
                MapNodeType.RestSite => "🛏️",
                MapNodeType.Treasure => "💎",
                MapNodeType.CardConstruct => "🔧",
                MapNodeType.TechMarket => "🏪",
                MapNodeType.CultivatorTavern => "🍺",
                MapNodeType.OpenLibrary => "📚",
                MapNodeType.SpiritualCharge => "⚡",
                MapNodeType.MemoryFragment => "🧩",
                MapNodeType.TeleportGate => "🌀",
                MapNodeType.NetworkConsciousness => "🌐",
                MapNodeType.SectTrial => "🏛️",
                MapNodeType.RandomEvent => "❓",
                MapNodeType.Choice => "🔀",
                MapNodeType.Mystery => "🔮",
                _ => "❔"
            };
        }
        
        /// <summary>
        /// 居中显示地图
        /// </summary>
        private void CenterMap()
        {
            if (MapContainer == null || _nodeVisuals.Count == 0) return;
            
            // 计算地图边界
            var minPos = Vector2.Inf;
            var maxPos = -Vector2.Inf;
            
            foreach (var visual in _nodeVisuals.Values)
            {
                var pos = visual.Position;
                minPos = new Vector2(Mathf.Min(minPos.X, pos.X), Mathf.Min(minPos.Y, pos.Y));
                maxPos = new Vector2(Mathf.Max(maxPos.X, pos.X), Mathf.Max(maxPos.Y, pos.Y));
            }
            
            var mapSize = maxPos - minPos + NodeSize;
            var containerSize = MapScrollContainer?.Size ?? Size;
            
            // 计算居中偏移
            _mapOffset = (containerSize - mapSize) / 2 - minPos;
            _mapScale = 1.0f;
            
            UpdateMapTransform();
        }
        
        /// <summary>
        /// 更新节点视觉效果
        /// </summary>
        private void UpdateNodeVisuals()
        {
            if (_currentMap == null) return;
            
            foreach (var kvp in _nodeVisuals)
            {
                var node = _currentMap.GetNode(kvp.Key);
                if (node == null) continue;
                
                var visual = kvp.Value;
                var background = visual.GetChild<ColorRect>(0);
                if (background != null)
                {
                    background.Color = GetNodeColor(node);
                }
            }
        }
        
        /// <summary>
        /// 更新楼层信息
        /// </summary>
        private void UpdateFloorInfo()
        {
            if (_towerManager == null) return;
            
            if (CurrentFloorLabel != null)
            {
                var floorText = _towerManager.CurrentFloor == 0 ? "塔底废墟" : $"第{_towerManager.CurrentFloor}层";
                CurrentFloorLabel.Text = floorText;
            }
            
            if (FloorTypeLabel != null)
            {
                FloorTypeLabel.Text = GetFloorTypeDisplayName(_towerManager.CurrentFloorType);
            }
            
            if (FloorProgressBar != null)
            {
                var progress = _towerManager.TowerProgress;
                if (progress != null)
                {
                    FloorProgressBar.Value = progress.GetCompletionPercentage(_towerManager.TotalTowerFloors);
                }
            }
        }
        
        /// <summary>
        /// 获取楼层类型显示名称
        /// </summary>
        private string GetFloorTypeDisplayName(TowerFloorType floorType)
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
        /// 节点进入事件处理
        /// </summary>
        private void OnNodeEntered(MapNode node)
        {
            UpdateNodeVisuals();
            
            if (_selectedNode == node)
            {
                ShowNodeInfo(node);
            }
        }
        
        /// <summary>
        /// 节点完成事件处理
        /// </summary>
        private void OnNodeCompleted(MapNode node, bool success)
        {
            UpdateNodeVisuals();
        }
        
        /// <summary>
        /// 楼层完成事件处理
        /// </summary>
        private void OnFloorCompleted(int floorLevel, TowerFloorType floorType)
        {
            UpdateFloorInfo();
            InitializeFloorSelector(); // 更新楼层选择器
        }
        
        /// <summary>
        /// 塔进度变化事件处理
        /// </summary>
        private void OnTowerProgressChanged(int currentFloor, int totalFloors)
        {
            UpdateFloorInfo();
            InitializeFloorSelector();
        }
        
        /// <summary>
        /// 地图状态变化事件处理
        /// </summary>
        private void OnMapStateChanged(MapState oldState, MapState newState)
        {
            // 可以在这里添加状态变化的视觉反馈
        }
        
        /// <summary>
        /// 重新生成地图按钮事件
        /// </summary>
        private void OnRegenerateMapPressed()
        {
            EmitSignal(SignalName.MapRegenerateRequested);
            
            if (_towerManager != null)
            {
                _towerManager.RegenerateCurrentFloor();
            }
        }
        
        /// <summary>
        /// 楼层选择事件
        /// </summary>
        private void OnFloorSelected(long index)
        {
            if (FloorSelector == null) return;
            
            var selectedFloor = FloorSelector.GetItemId((int)index);
            EmitSignal(SignalName.FloorChangeRequested, selectedFloor);
            
            if (_towerManager != null)
            {
                _towerManager.EnterFloor(selectedFloor);
            }
        }
        
        /// <summary>
        /// 进入节点按钮事件
        /// </summary>
        private void OnEnterNodePressed()
        {
            if (_selectedNode == null || _towerManager == null) return;
            
            _towerManager.EnterNode(_selectedNode.NodeId);
        }
        
        /// <summary>
        /// 刷新UI显示
        /// </summary>
        public void RefreshUI()
        {
            if (_towerManager == null) return;
            
            var currentMap = _towerManager.CurrentMap;
            if (currentMap != null)
            {
                DisplayMap(currentMap);
            }
            
            UpdateFloorInfo();
            InitializeFloorSelector();
            
            if (_selectedNode != null)
            {
                ShowNodeInfo(_selectedNode);
            }
        }
        
        /// <summary>
        /// 设置调试模式
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            if (RegenerateMapButton != null)
            {
                RegenerateMapButton.Visible = enabled;
            }
        }
    }
}