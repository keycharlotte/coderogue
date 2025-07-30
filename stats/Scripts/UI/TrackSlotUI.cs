using Godot;
using CodeRogue.Skills;
using CodeRogue.Data;

namespace CodeRogue.UI
{
    /// <summary>
    /// 单个技能轨道槽UI
    /// </summary>
    public partial class TrackSlotUI : Control
    {
        [Signal] public delegate void TrackClickedEventHandler(int trackIndex);
        
        public int TrackIndex { get; set; }
        
        // 使用Export标记引用场景中的节点
        [Export] private Panel _background;
        [Export] private TextureRect _skillIcon;
        [Export] private ProgressBar _chargeProgress;
        [Export] private Label _skillNameLabel;
        [Export] private Label _stateLabel;
        [Export] private Button _clickArea;
        
        public override void _Ready()
        {
            SetupUI();
        }
        
        private void SetupUI()
        {
            // 连接点击事件
            if (_clickArea != null)
            {
                _clickArea.Pressed += OnClicked;
            }
            
            // 设置默认样式
            // SetDefaultStyle();
            
            // 字体颜色应在.tscn文件中设置，这里只处理数据绑定逻辑
            // 默认样式已在场景文件中定义
        }
        
        private void SetDefaultStyle()
        {
            // UI样式应在.tscn文件中定义，这里只处理数据绑定逻辑
            // 样式设置已移至TrackSlotUI.tscn场景文件中
        }
        
        public void UpdateTrack(SkillTrack track)
        {
            if (_skillNameLabel == null || _skillIcon == null || _stateLabel == null) return;
            
            // 检测状态变化
            var previousState = GetCurrentState();
            var newState = track.State;
            
            if (track.EquippedSkill != null)
            {
                _skillNameLabel.Text = track.EquippedSkill.Name;
                
                // 设置技能图标
                if (track.EquippedSkill.IconPath != null)
                {
                    _skillIcon.Texture = ResourceLoader.Load<Texture2D>(track.EquippedSkill.IconPath);
                }
                else
                {
                    // 使用默认图标或根据技能类型设置图标
                    _skillIcon.Texture = GetDefaultIconForSkillType(track.EquippedSkill.SkillType);
                }
                
                // 显示技能图标
                _skillIcon.Visible = true;
            }
            else
            {
                _skillNameLabel.Text = "空";
                _skillIcon.Texture = null;
                _skillIcon.Visible = false;
            }
            
            // 更新状态
            _stateLabel.Text = GetStateText(track.State);
            
            // 字体颜色应通过.tscn文件中的主题设置，而非代码动态修改
            // 状态变化的视觉反馈通过UpdateStateStyle方法中的Modulate属性实现
            
            UpdateStateStyle(track.State);
            
            // 播放状态变化动画
            if (previousState != newState)
            {
                PlayStateChangeEffect(previousState, newState);
            }
        }
        
        public void UpdateChargeProgress(float currentCharge, float maxCharge)
        {
            if (_chargeProgress == null) return;
            
            if (maxCharge > 0)
            {
                _chargeProgress.Value = (currentCharge / maxCharge) * 100;
            }
            else
            {
                _chargeProgress.Value = 0;
            }
        }
        
        public void SetReadyState(bool ready)
        {
            if (_background == null) return;
            
            // 通过修改现有组件的属性而非创建新样式来表示状态
            // 具体的视觉样式应在.tscn文件中预定义
            _background.Modulate = ready ? new Color(0.4f, 0.6f, 0.8f, 1.0f) : Colors.White;
        }
        
        public void PlayActivationEffect()
        {
            // 播放激活效果动画
            var tween = CreateTween();
            tween.Parallel().TweenProperty(this, "modulate", new Color(0.4f, 0.6f, 0.8f, 1.0f), 0.1f);
            tween.Parallel().TweenProperty(this, "scale", Vector2.One * 1.2f, 0.1f);
            tween.TweenProperty(this, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.2f);
            tween.Parallel().TweenProperty(this, "scale", Vector2.One, 0.2f);
            tween.TweenCallback(Callable.From(SetDefaultStyle));
        }
        
        private void UpdateStateStyle(TrackState state)
        {
            if (_background == null) return;
            
            // 通过修改现有组件的属性而非创建新样式来表示状态
            // 具体的视觉样式应在.tscn文件中预定义
            Color stateColor = state switch
            {
                TrackState.Empty => new Color(0.85f, 0.85f, 0.85f, 1.0f),
                TrackState.Charging => new Color(0.9f, 0.8f, 0.3f, 1.0f),
                TrackState.Ready => new Color(0.4f, 0.6f, 0.8f, 1.0f),
                TrackState.Cooldown => new Color(0.7f, 0.7f, 0.7f, 1.0f),
                _ => Colors.White
            };
            
            _background.Modulate = stateColor;
        }
        
        private string GetStateText(TrackState state)
        {
            return state switch
            {
                TrackState.Empty => "空闲",
                TrackState.Charging => "充能中",
                TrackState.Ready => "就绪",
                TrackState.Cooldown => "冷却",
                _ => "未知"
            };
        }
        
        private Texture2D GetDefaultIconForSkillType(SkillType skillType)
        {
            // 根据技能类型返回默认图标
            // 这里可以加载预设的图标资源
            return GD.Load<Texture2D>("res://Art/AssetsTextures/default.png");
        }
        
        private void OnClicked()
        {
            // 播放点击反馈
            PlayClickEffect();
            EmitSignal(SignalName.TrackClicked, TrackIndex);
        }
        
        private TrackState GetCurrentState()
        {
            if (_stateLabel == null) return TrackState.Empty;
            
            return _stateLabel.Text switch
            {
                "空闲" => TrackState.Empty,
                "充能中" => TrackState.Charging,
                "就绪" => TrackState.Ready,
                "冷却" => TrackState.Cooldown,
                _ => TrackState.Empty
            };
        }
        
        private void PlayStateChangeEffect(TrackState from, TrackState to)
        {
            // 根据状态变化播放不同的动画效果
            switch (to)
            {
                case TrackState.Charging:
                    PlayChargingStartEffect();
                    break;
                case TrackState.Ready:
                    PlayReadyEffect();
                    break;
                case TrackState.Empty:
                    PlayEmptyEffect();
                    break;
            }
        }
        
        private void PlayChargingStartEffect()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(1.0f, 0.9f, 0.6f, 1.0f), 0.3f);
            tween.TweenProperty(this, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.3f);
        }
        
        private void PlayReadyEffect()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(0.4f, 0.6f, 0.8f, 1.0f), 0.2f);
            tween.TweenProperty(this, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.3f);
        }
        
        private void PlayEmptyEffect()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(0.85f, 0.85f, 0.85f, 1.0f), 0.2f);
            tween.TweenProperty(this, "modulate", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.3f);
        }
        
        private void PlayClickEffect()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "scale", Vector2.One * 0.95f, 0.1f);
            tween.TweenProperty(this, "scale", Vector2.One, 0.1f);
        }
    }
}