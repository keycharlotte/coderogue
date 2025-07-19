using Godot;
using CodeRogue.Skills;

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
            
            // 设置默认字体颜色，参考GameUITheme.tres
            if (_skillNameLabel != null)
                _skillNameLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.2f, 0.2f, 1.0f));
            if (_stateLabel != null)
                _stateLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.2f, 0.2f, 1.0f));
        }
        
        private void SetDefaultStyle()
        {
            if (_background == null) return;

            // 参考GameUITheme.tres中Panel的样式设计 (StyleBoxFlat_76abw)
            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0.85f, 0.85f, 0.85f, 1.0f), // 与主题Panel背景色一致
                CornerRadiusTopLeft = 8, // 与主题Panel圆角一致
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8,

                // Panel样式没有边框，保持简洁
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                BorderWidthLeft = 0,
                BorderWidthRight = 0
            };

            _background.AddThemeStyleboxOverride("panel", styleBox);
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
                    _skillIcon.Texture = GetDefaultIconForSkillType(track.EquippedSkill.Type);
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
            
            // 根据状态设置字体颜色
            switch (track.State)
            {
                case TrackState.Ready:
                    // 就绪状态使用白色字体，与Button pressed样式一致
                    _skillNameLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));
                    _stateLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));
                    break;
                default:
                    // 其他状态使用主题的默认Label字体颜色
                    _skillNameLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.2f, 0.2f, 1.0f));
                    _stateLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.2f, 0.2f, 1.0f));
                    break;
            }
            
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
            
            if (ready)
            {
                // 就绪状态使用主题的Button pressed样式 (StyleBoxFlat_ouyse)
                var styleBox = new StyleBoxFlat
                {
                    BgColor = new Color(0.4f, 0.6f, 0.8f, 1.0f),
                    BorderColor = new Color(0.4f, 0.6f, 0.8f, 1.0f),
                    BorderWidthTop = 1,
                    BorderWidthBottom = 1,
                    BorderWidthLeft = 1,
                    BorderWidthRight = 1,

                    // 使用Button样式的圆角
                    CornerRadiusTopLeft = 4,
                    CornerRadiusTopRight = 4,
                    CornerRadiusBottomLeft = 4,
                    CornerRadiusBottomRight = 4,

                    // 添加内容边距，与Button样式一致
                    ContentMarginTop = 8,
                    ContentMarginBottom = 8,
                    ContentMarginLeft = 12,
                    ContentMarginRight = 12
                };

                _background.AddThemeStyleboxOverride("panel", styleBox);
            }
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
            
            // 根据状态设置不同的样式，严格参考GameUITheme.tres的设计
            var styleBox = new StyleBoxFlat();
            
            // 设置基础圆角，与主题一致
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;
            
            // 根据状态设置颜色和边框
            switch (state)
            {
                case TrackState.Empty:
                    // 空闲状态 - 使用主题的默认Panel样式 (StyleBoxFlat_76abw)
                    styleBox.BgColor = new Color(0.85f, 0.85f, 0.85f, 1.0f);
                    // Panel样式无边框
                    styleBox.BorderWidthTop = 0;
                    styleBox.BorderWidthBottom = 0;
                    styleBox.BorderWidthLeft = 0;
                    styleBox.BorderWidthRight = 0;
                    break;
                    
                case TrackState.Charging:
                    // 充能状态 - 使用Button normal样式的变体 (StyleBoxFlat_5bphu)
                    styleBox.BgColor = new Color(0.85f, 0.85f, 0.85f, 1.0f);
                    styleBox.BorderColor = new Color(0.9f, 0.8f, 0.3f, 1.0f); // 黄色边框表示充能
                    styleBox.BorderWidthTop = 2;
                    styleBox.BorderWidthBottom = 2;
                    styleBox.BorderWidthLeft = 2;
                    styleBox.BorderWidthRight = 2;
                    // 添加内容边距，参考Button样式
                    styleBox.ContentMarginTop = 8;
                    styleBox.ContentMarginBottom = 8;
                    styleBox.ContentMarginLeft = 12;
                    styleBox.ContentMarginRight = 12;
                    break;
                    
                case TrackState.Ready:
                    // 就绪状态 - 使用Button pressed样式 (StyleBoxFlat_ouyse)
                    styleBox.BgColor = new Color(0.4f, 0.6f, 0.8f, 1.0f);
                    styleBox.BorderColor = new Color(0.4f, 0.6f, 0.8f, 1.0f);
                    styleBox.BorderWidthTop = 1;
                    styleBox.BorderWidthBottom = 1;
                    styleBox.BorderWidthLeft = 1;
                    styleBox.BorderWidthRight = 1;
                    // 圆角调整为Button样式
                    styleBox.CornerRadiusTopLeft = 4;
                    styleBox.CornerRadiusTopRight = 4;
                    styleBox.CornerRadiusBottomLeft = 4;
                    styleBox.CornerRadiusBottomRight = 4;
                    // 添加内容边距
                    styleBox.ContentMarginTop = 8;
                    styleBox.ContentMarginBottom = 8;
                    styleBox.ContentMarginLeft = 12;
                    styleBox.ContentMarginRight = 12;
                    break;
                    
                case TrackState.Cooldown:
                    // 冷却状态 - 使用HSlider slider样式 (StyleBoxFlat_10hko)
                    styleBox.BgColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
                    styleBox.CornerRadiusTopLeft = 3;
                    styleBox.CornerRadiusTopRight = 3;
                    styleBox.CornerRadiusBottomLeft = 3;
                    styleBox.CornerRadiusBottomRight = 3;
                    // 无边框
                    styleBox.BorderWidthTop = 0;
                    styleBox.BorderWidthBottom = 0;
                    styleBox.BorderWidthLeft = 0;
                    styleBox.BorderWidthRight = 0;
                    break;
                    
                default:
                    // 默认状态 - 使用Panel样式
                    styleBox.BgColor = new Color(0.85f, 0.85f, 0.85f, 1.0f);
                    styleBox.BorderWidthTop = 0;
                    styleBox.BorderWidthBottom = 0;
                    styleBox.BorderWidthLeft = 0;
                    styleBox.BorderWidthRight = 0;
                    break;
            }
            
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