using Godot;

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
            SetDefaultStyle();
        }
        
        private void SetDefaultStyle()
        {
            if (_background == null) return;
            
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            styleBox.SetCornerRadiusAll(8);
            styleBox.BorderColor = new Color(0.5f, 0.5f, 0.5f);
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            
            _background.AddThemeStyleboxOverride("panel", styleBox);
        }
        
        public void UpdateTrack(SkillTrack track)
        {
            if (_skillNameLabel == null || _skillIcon == null || _stateLabel == null) return;
            
            if (track.EquippedSkill != null)
            {
                _skillNameLabel.Text = track.EquippedSkill.Name;
                
                // 设置技能图标
                if (track.EquippedSkill.Icon != null)
                {
                    _skillIcon.Texture = track.EquippedSkill.Icon;
                }
                else
                {
                    // 使用默认图标或根据技能类型设置图标
                    _skillIcon.Texture = GetDefaultIconForSkillType(track.EquippedSkill.Type);
                }
            }
            else
            {
                _skillNameLabel.Text = "空";
                _skillIcon.Texture = null;
            }
            
            // 更新状态
            _stateLabel.Text = GetStateText(track.State);
            UpdateStateStyle(track.State);
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
            
            if (ready)
            {
                var styleBox = new StyleBoxFlat();
                styleBox.BgColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                styleBox.SetCornerRadiusAll(8);
                styleBox.BorderColor = new Color(0.0f, 1.0f, 0.0f);
                styleBox.BorderWidthTop = 3;
                styleBox.BorderWidthBottom = 3;
                styleBox.BorderWidthLeft = 3;
                styleBox.BorderWidthRight = 3;
                
                _background.AddThemeStyleboxOverride("panel", styleBox);
            }
        }
        
        public void PlayActivationEffect()
        {
            // 播放激活效果动画
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0.5f), 0.1f);
            tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.1f);
            tween.TweenCallback(Callable.From(SetDefaultStyle));
        }
        
        private void UpdateStateStyle(TrackState state)
        {
            if (_background == null) return;
            
            Color borderColor = state switch
            {
                TrackState.Empty => new Color(0.5f, 0.5f, 0.5f),
                TrackState.Charging => new Color(1.0f, 1.0f, 0.0f),
                TrackState.Ready => new Color(0.0f, 1.0f, 0.0f),
                TrackState.Cooldown => new Color(1.0f, 0.0f, 0.0f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
            
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            styleBox.SetCornerRadiusAll(8);
            styleBox.BorderColor = borderColor;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            
            _background.AddThemeStyleboxOverride("panel", styleBox);
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
            EmitSignal(SignalName.TrackClicked, TrackIndex);
        }
    }
}