using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CodeRogue.UI
{
    /// <summary>
    /// 技能轨道UI - 可视化技能轨道系统
    /// </summary>
    public partial class SkillTrackUI : Control
    {
        [Export] private HBoxContainer _tracksContainer;
        [Export] private Label _statusLabel;
        [Export] private Button _activateButton;
        
        private List<TrackSlotUI> _trackSlots;
        private SkillTrackManager _trackManager;
        
        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
            SetupTrackSlots();
        }
        
        private void InitializeUI()
        {
            _trackSlots = new List<TrackSlotUI>();
            _trackManager = SkillTrackManager.Instance;
            
            if (_trackManager == null)
            {
                GD.PrintErr("SkillTrackManager.Instance is null");
                return;
            }
        }
        
        private void ConnectSignals()
        {
            if (_trackManager != null)
            {
                _trackManager.TrackCharged += OnTrackCharged;
                _trackManager.SkillActivated += OnSkillActivated;
                _trackManager.ChargeUpdated += OnChargeUpdated;
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
            var trackSlot = new TrackSlotUI();
            trackSlot.TrackIndex = index;
            trackSlot.CustomMinimumSize = new Vector2(120, 160);
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
            UpdateStatusLabel($"轨道 {trackIndex} 已就绪!");
            
            if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
            {
                _trackSlots[trackIndex].SetReadyState(true);
            }
        }
        
        private void OnSkillActivated(SkillCard skill, int trackIndex)
        {
            GD.Print($"技能激活: {skill.Name} (轨道 {trackIndex})");
            UpdateStatusLabel($"激活技能: {skill.Name}");
            
            if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
            {
                _trackSlots[trackIndex].PlayActivationEffect();
            }
        }
        
        private void OnChargeUpdated(int trackIndex, float currentCharge, float maxCharge)
        {
            if (trackIndex >= 0 && trackIndex < _trackSlots.Count)
            {
                _trackSlots[trackIndex].UpdateChargeProgress(currentCharge, maxCharge);
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
                
                // 创建淡出效果
                var tween = CreateTween();
                tween.TweenProperty(_statusLabel, "modulate:a", 1.0f, 0.1f);
                tween.TweenInterval(2.0f);
                tween.TweenProperty(_statusLabel, "modulate:a", 0.5f, 0.5f);
            }
        }
        
        public override void _Process(double delta)
        {
            // 实时更新轨道状态
            if (_trackManager != null)
            {
                var tracks = _trackManager.GetTracks();
                for (int i = 0; i < tracks.Count && i < _trackSlots.Count; i++)
                {
                    UpdateTrackSlot(i, tracks[i]);
                }
            }
        }
    }
}