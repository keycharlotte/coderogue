using Godot;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Skills;

namespace CodeRogue.UI
{
	/// <summary>
	/// 技能轨道UI - 可视化技能轨道系统
	/// </summary>
	public partial class SkillTrackUI : Control
	{
		[Export] private VBoxContainer _tracksContainer;
		[Export] private Label _statusLabel;
		[Export] private Button _activateButton;
		
		private List<TrackSlotUI> _trackSlots;
		private SkillTrackManager _trackManager;
		private int _lastTrackCount = 0;
		private SkillDeck _lastDeck = null;
		
		public override void _Ready()
		{
			InitializeUI();
			ConnectSignals();
			SetupTrackSlots();
		}
		
		private void InitializeUI()
		{
			_trackSlots = new List<TrackSlotUI>();
			_trackManager = GetNode<SkillTrackManager>("/root/SkillTrackManager");
			
			if (_trackManager == null)
			{
				GD.PrintErr("SkillTrackManager autoload is null");
				return;
			}
			
			// 尝试连接到SkillDeckManager以监听卡组变化
			var deckManager = GetNode<SkillDeckManager>("/root/SkillDeckManager");
			if (deckManager != null)
			{
				// 如果SkillDeckManager有卡组变化信号，可以在这里连接
				GD.Print("SkillTrackUI: 已连接到SkillDeckManager");
			}
			else
			{
				GD.Print("SkillTrackUI: 无法找到SkillDeckManager");
			}
		}
		
		private void ConnectSignals()
		{
			if (_trackManager != null)
			{
				// 连接所有状态变化信号
				_trackManager.TrackCharged += OnTrackCharged;
				_trackManager.SkillActivated += OnSkillActivated;
				_trackManager.ChargeUpdated += OnChargeUpdated;
				_trackManager.SkillEquipped += OnSkillEquipped;
				
				GD.Print("SkillTrackUI: 已连接到SkillTrackManager的状态变化信号");
			}
			else
			{
				GD.PrintErr("SkillTrackUI: 无法连接到SkillTrackManager，状态监听失败");
			}
			
			if (_activateButton != null)
			{
				_activateButton.Pressed += OnActivateButtonPressed;
			}
		}
		
		private void SetupTrackSlots()
		{
			if (_trackManager == null) return;
			
			var tracks = _trackManager.GetTracks();
			
			// 清除现有的轨道槽
			foreach (Node child in _tracksContainer.GetChildren())
			{
				child.QueueFree();
			}
			GD.Print("SkillTrackUI: 已清除现有的轨道槽");
			_trackSlots.Clear();
			
			// 创建轨道槽UI
			for (int i = 0; i < tracks.Count; i++)
			{
				var trackSlot = CreateTrackSlot(i);
				_tracksContainer.AddChild(trackSlot);
				_trackSlots.Add(trackSlot);
				
				// 初始化轨道状态
				UpdateTrackSlot(i, tracks[i]);
			}
		}
		
		private TrackSlotUI CreateTrackSlot(int index)
		{
			// 加载TrackSlotUI场景文件，符合UI组件设计最佳实践
			var trackSlotScene = GD.Load<PackedScene>("res://Scenes/UI/TrackSlotUI.tscn");
			var trackSlot = trackSlotScene.Instantiate<TrackSlotUI>();
			trackSlot.TrackIndex = index;
			trackSlot.CustomMinimumSize = new Vector2(400, 50);
			trackSlot.TrackClicked += OnTrackClicked;
			return trackSlot;
		}
		
		private void UpdateTrackSlot(int trackIndex, SkillTrack track)
		{
			if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
			{
				_trackSlots[trackIndex].UpdateTrack(track);
			}
		}
		
		private void OnTrackCharged(int trackIndex, SkillCard skill)
		{
			GD.Print($"轨道 {trackIndex} 充能完成: {skill.Name}");
			UpdateStatusLabel($"轨道 {trackIndex} 已就绪: {skill.Name}!");
			
			if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
			{
				_trackSlots[trackIndex].SetReadyState(true);
				
				// 播放就绪提示效果
				PlayTrackReadyEffect(trackIndex);
			}
			else
			{
				GD.PrintErr($"SkillTrackUI: 轨道索引 {trackIndex} 超出范围");
			}
		}
		
		private void OnSkillActivated(SkillCard skill, int trackIndex)
		{
			GD.Print($"技能激活: {skill.Name} (轨道 {trackIndex})");
			UpdateStatusLabel($"✨ 激活技能: {skill.Name}");
			
			if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
			{
				_trackSlots[trackIndex].PlayActivationEffect();
				
				// 播放全局激活效果
				PlayGlobalActivationEffect();
			}
			else
			{
				GD.PrintErr($"SkillTrackUI: 轨道索引 {trackIndex} 超出范围");
			}
		}
		
		private void OnChargeUpdated(int trackIndex, float currentCharge, float maxCharge)
		{
			if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
			{
				_trackSlots[trackIndex].UpdateChargeProgress(currentCharge, maxCharge);
				
				// 更新充能进度提示
				float progress = maxCharge > 0 ? (currentCharge / maxCharge) * 100 : 0;
				if (progress >= 100)
				{
					UpdateStatusLabel($"轨道 {trackIndex} 充能完成!");
				}
			}
			else
			{
				GD.PrintErr($"SkillTrackUI: 轨道索引 {trackIndex} 超出范围");
			}
		}
		
		private void OnSkillEquipped(int trackIndex, SkillCard skill)
		{
			GD.Print($"技能装备: {skill.Name} 装备到轨道 {trackIndex}");
			UpdateStatusLabel($"⚡ 装备技能: {skill.Name} → 轨道 {trackIndex}");
			
			if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
			{
				// 立即更新轨道UI显示
				var track = _trackManager.GetTrack(trackIndex);
				if (track != null)
				{
					_trackSlots[trackIndex].UpdateTrack(track);
					
					// 播放装备效果
					// _trackSlots[trackIndex].PlayChargingStartEffect();
				}
			}
			else
			{
				GD.PrintErr($"SkillTrackUI: 轨道索引 {trackIndex} 超出范围");
			}
		}
		
		private void OnTrackClicked(int trackIndex)
		{
			// 手动激活技能
			_trackManager?.ActivateSkill(trackIndex);
		}
		
		private void OnActivateButtonPressed()
		{
			// 激活所有就绪的技能
			var tracks = _trackManager?.GetTracks();
			if (tracks != null)
			{
				for (int i = 0; i < tracks.Count; i++)
				{
					if (tracks[i].State == TrackState.Ready)
					{
						_trackManager.ActivateSkill(i);
					}
				}
			}
		}
		
		private void UpdateStatusLabel(string message)
		{
			if (_statusLabel != null)
			{
				_statusLabel.Text = message;
				
				// 创建淡入淡出效果
				var tween = CreateTween();
				tween.TweenProperty(_statusLabel, "modulate:a", 1.0f, 0.1f);
				tween.TweenInterval(2.0f);
				tween.TweenProperty(_statusLabel, "modulate:a", 0.5f, 0.5f);
			}
		}
		
		private void PlayTrackReadyEffect(int trackIndex)
		{
			if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
			{
				var trackSlot = _trackSlots[trackIndex];
				var tween = CreateTween();
				tween.TweenProperty(trackSlot, "scale", Vector2.One * 1.1f, 0.2f);
				tween.TweenProperty(trackSlot, "scale", Vector2.One, 0.2f);
				
				// 添加颜色闪烁效果，使用主题的Button pressed颜色
				tween.Parallel().TweenProperty(trackSlot, "modulate", new Color(0.4f, 0.6f, 0.8f, 1.0f), 0.1f);
				tween.TweenProperty(trackSlot, "modulate", Colors.White, 0.3f);
			}
		}
		
		private void PlayGlobalActivationEffect()
		{
			// 播放全局激活效果，使用主题的蓝色调
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate", new Color(0.4f, 0.6f, 0.8f, 1.0f), 0.15f);
			tween.TweenProperty(this, "modulate", Colors.White, 0.35f);
		}
		
		private void UpdateActivateButtonState(List<SkillTrack> tracks)
		{
			if (_activateButton == null) return;
			
			bool hasReadySkills = tracks.Any(track => track.State == TrackState.Ready);
			_activateButton.Disabled = !hasReadySkills;
			
			// 更新按钮文本和样式
			int readyCount = tracks.Count(track => track.State == TrackState.Ready);
			_activateButton.Text = readyCount > 0 ? $"激活技能 ({readyCount})" : "激活技能";
			
			// 为就绪状态添加视觉提示，使用主题颜色
			if (hasReadySkills)
			{
				// 使用主题的Button hover颜色调制
				_activateButton.Modulate = new Color(0.4f, 0.6f, 0.8f, 1.0f);
			}
			else
			{
				_activateButton.Modulate = Colors.White;
			}
		}
		
		private void OnTrackCountChanged(int newCount)
		{
			GD.Print($"SkillTrackUI: 轨道数量变化 {_lastTrackCount} -> {newCount}");
			UpdateStatusLabel($"轨道数量: {newCount}");
			
			// 重新设置轨道槽
			SetupTrackSlots();
		}
		
		private void CheckOverallStatus(List<SkillTrack> tracks)
		{
			int emptyCount = tracks.Count(t => t.State == TrackState.Empty);
			int chargingCount = tracks.Count(t => t.State == TrackState.Charging);
			int readyCount = tracks.Count(t => t.State == TrackState.Ready);
			int cooldownCount = tracks.Count(t => t.State == TrackState.Cooldown);
			
			// 更新整体状态显示
			string statusText = $"空闲:{emptyCount} 充能:{chargingCount} 就绪:{readyCount} 冷却:{cooldownCount}";
			
			// 只在状态发生显著变化时更新
			if (readyCount > 0)
			{
				UpdateStatusLabel($"⚡ {readyCount} 个技能就绪!");
			}
			else if (chargingCount == tracks.Count)
			{
				UpdateStatusLabel("🔄 所有轨道充能中...");
			}
		}
		
		/// <summary>
		/// 手动刷新UI状态 - 用于外部调用
		/// </summary>
		public void RefreshUI()
		{
			if (_trackManager != null)
			{
				var tracks = _trackManager.GetTracks();
				if (tracks != null)
				{
					for (int i = 0; i < tracks.Count && i < _trackSlots.Count; i++)
					{
						UpdateTrackSlot(i, tracks[i]);
					}
					UpdateActivateButtonState(tracks);
				}
			}
		}
		
		/// <summary>
		/// 设置卡组变化监听 - 外部调用接口
		/// </summary>
		public void OnDeckChanged(SkillDeck newDeck)
		{
			if (_lastDeck != newDeck)
			{
				GD.Print($"SkillTrackUI: 卡组切换 - {newDeck?.Name ?? "无"}");
				UpdateStatusLabel($"📦 切换卡组: {newDeck?.Name ?? "无"}");
				_lastDeck = newDeck;
				
				// 播放卡组切换效果
				PlayDeckChangeEffect();
				
				// 刷新轨道显示
				RefreshUI();
			}
		}
		
		/// <summary>
		/// 监听技能系统错误事件
		/// </summary>
		public void OnSkillSystemError(string errorMessage)
		{
			GD.PrintErr($"SkillTrackUI: 技能系统错误 - {errorMessage}");
			UpdateStatusLabel($"❌ 错误: {errorMessage}");
			
			// 播放错误提示效果
			PlayErrorEffect();
		}
		
		/// <summary>
		/// 监听技能系统重置事件
		/// </summary>
		public void OnSkillSystemReset()
		{
			GD.Print("SkillTrackUI: 技能系统重置");
			UpdateStatusLabel("🔄 系统重置");
			
			// 重置所有状态
			_lastTrackCount = 0;
			_lastDeck = null;
			
			// 重新初始化UI
			SetupTrackSlots();
		}
		
		private void PlayDeckChangeEffect()
		{
			// 播放卡组切换的视觉效果，使用主题的绿色调
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate", new Color(0.6f, 0.9f, 0.6f, 1.0f), 0.3f);
			tween.TweenProperty(this, "modulate", Colors.White, 0.5f);
		}
		
		private void PlayErrorEffect()
		{
			// 播放错误提示的视觉效果，使用更柔和的红色调
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate", new Color(1.0f, 0.6f, 0.6f, 1.0f), 0.2f);
			tween.TweenProperty(this, "modulate", Colors.White, 0.3f);
			tween.TweenProperty(this, "modulate", new Color(1.0f, 0.6f, 0.6f, 1.0f), 0.2f);
			tween.TweenProperty(this, "modulate", Colors.White, 0.3f);
			
			// 添加轻微的震动效果
			var originalPosition = Position;
			for (int i = 0; i < 2; i++)
			{
				tween.TweenProperty(this, "position", originalPosition + Vector2.Right * 3, 0.05f);
				tween.TweenProperty(this, "position", originalPosition + Vector2.Left * 3, 0.05f);
			}
			tween.TweenProperty(this, "position", originalPosition, 0.05f);
		}
		
		public override void _Process(double delta)
		{
			// 实时更新轨道状态
			if (_trackManager != null)
			{
				var tracks = _trackManager.GetTracks();
				if (tracks != null)
				{
					// 检查轨道数量是否发生变化
					if (tracks.Count != _lastTrackCount)
					{
						OnTrackCountChanged(tracks.Count);
						_lastTrackCount = tracks.Count;
					}
					
					for (int i = 0; i < tracks.Count && i < _trackSlots.Count; i++)
					{
						UpdateTrackSlot(i, tracks[i]);
					}
					
					// 更新激活按钮状态
					UpdateActivateButtonState(tracks);
					
					// 检查整体状态变化
					CheckOverallStatus(tracks);
				}
			}
		}
		
		public override void _ExitTree()
		{
			// 断开信号连接以避免内存泄漏
			if (_trackManager != null)
			{
				_trackManager.TrackCharged -= OnTrackCharged;
				_trackManager.SkillActivated -= OnSkillActivated;
				_trackManager.ChargeUpdated -= OnChargeUpdated;
				_trackManager.SkillEquipped -= OnSkillEquipped;
				
				GD.Print("SkillTrackUI: 已断开SkillTrackManager信号连接");
			}
			
			base._ExitTree();
		}
	}
}
