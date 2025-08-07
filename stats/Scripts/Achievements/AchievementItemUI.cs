using Godot;
using Godot.Collections;
using System;
using CodeRogue.Achievements.Data;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就项目UI控制器
    /// 负责单个成就项的显示和交互
    /// </summary>
    public partial class AchievementItemUI : Control
    {
        #region UI组件引用
        [Export] public Panel Background { get; set; }
        [Export] public TextureRect AchievementIcon { get; set; }
        [Export] public ColorRect RarityIndicator { get; set; }
        [Export] public Label TitleLabel { get; set; }
        [Export] public Label StatusLabel { get; set; }
        [Export] public Label CompletionTimeLabel { get; set; }
        [Export] public Label DescriptionLabel { get; set; }
        [Export] public Label ProgressLabel { get; set; }
        [Export] public Label ProgressPercentLabel { get; set; }
        [Export] public ProgressBar ProgressBar { get; set; }
        [Export] public Label RewardLabel { get; set; }
        [Export] public HBoxContainer RewardIconsContainer { get; set; }
        [Export] public Button ClaimButton { get; set; }
        [Export] public Button ViewDetailButton { get; set; }
        [Export] public AnimationPlayer AnimationPlayer { get; set; }
        #endregion

        #region 数据和状态
        private AchievementConfig _achievementConfig;
        private AchievementProgress _achievementProgress;
        private bool _isSelected;
        private bool _isAnimating;
        #endregion

        #region 配置参数
        [Export] public bool ShowProgressBar { get; set; } = true;
        [Export] public bool ShowRewards { get; set; } = true;
        [Export] public bool EnableAnimations { get; set; } = true;
        [Export] public bool ShowCompletionTime { get; set; } = true;
        [Export] public float AnimationDuration { get; set; } = 0.3f;
        #endregion

        #region 信号定义
        [Signal] public delegate void AchievementSelectedEventHandler(string achievementId);
        [Signal] public delegate void ClaimRewardRequestedEventHandler(string achievementId);
        [Signal] public delegate void ViewDetailRequestedEventHandler(string achievementId);
        #endregion

        #region 初始化
        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
        }

        private void InitializeUI()
        {
            // 获取UI组件引用
            Background ??= GetNode<Panel>("Background");
            AchievementIcon ??= GetNode<TextureRect>("MainContainer/IconContainer/AchievementIcon");
            RarityIndicator ??= GetNode<ColorRect>("MainContainer/IconContainer/RarityIndicator");
            TitleLabel ??= GetNode<Label>("MainContainer/ContentContainer/HeaderContainer/TitleLabel");
            StatusLabel ??= GetNode<Label>("MainContainer/ContentContainer/HeaderContainer/StatusContainer/StatusLabel");
            CompletionTimeLabel ??= GetNode<Label>("MainContainer/ContentContainer/HeaderContainer/StatusContainer/CompletionTimeLabel");
            DescriptionLabel ??= GetNode<Label>("MainContainer/ContentContainer/DescriptionLabel");
            ProgressLabel ??= GetNode<Label>("MainContainer/ContentContainer/ProgressContainer/ProgressInfoContainer/ProgressLabel");
            ProgressPercentLabel ??= GetNode<Label>("MainContainer/ContentContainer/ProgressContainer/ProgressInfoContainer/ProgressPercentLabel");
            ProgressBar ??= GetNode<ProgressBar>("MainContainer/ContentContainer/ProgressContainer/ProgressBar");
            RewardLabel ??= GetNode<Label>("MainContainer/ContentContainer/RewardContainer/RewardLabel");
            RewardIconsContainer ??= GetNode<HBoxContainer>("MainContainer/ContentContainer/RewardContainer/RewardIconsContainer");
            ClaimButton ??= GetNode<Button>("MainContainer/ActionContainer/ClaimButton");
            ViewDetailButton ??= GetNode<Button>("MainContainer/ActionContainer/ViewDetailButton");
            AnimationPlayer ??= GetNode<AnimationPlayer>("AnimationPlayer");

            // 设置初始状态
            SetSelected(false);
            UpdateVisibility();
        }

        private void ConnectSignals()
        {
            if (ClaimButton != null)
                ClaimButton.Pressed += OnClaimButtonPressed;
            if (ViewDetailButton != null)
                ViewDetailButton.Pressed += OnViewDetailButtonPressed;
            if (Background != null)
                Background.GuiInput += OnBackgroundInput;
        }
        #endregion

        #region 数据设置
        /// <summary>
        /// 设置成就数据
        /// </summary>
        public void SetAchievementData(AchievementConfig config, AchievementProgress progress)
        {
            _achievementConfig = config;
            _achievementProgress = progress;
            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示内容
        /// </summary>
        private void UpdateDisplay()
        {
            if (_achievementConfig == null) return;

            UpdateBasicInfo();
            UpdateProgress();
            UpdateStatus();
            UpdateRewards();
            UpdateActions();
            UpdateVisualStyle();
        }

        private void UpdateBasicInfo()
        {
            if (TitleLabel != null)
                TitleLabel.Text = _achievementConfig.Title;
            
            if (DescriptionLabel != null)
                DescriptionLabel.Text = _achievementConfig.Description;
            
            if (AchievementIcon != null && !string.IsNullOrEmpty(_achievementConfig.IconPath))
            {
                var texture = GD.Load<Texture2D>(_achievementConfig.IconPath);
                if (texture != null)
                    AchievementIcon.Texture = texture;
            }
        }

        private void UpdateProgress()
        {
            if (_achievementProgress == null) return;

            var currentValue = _achievementProgress.CurrentValue;
            var targetValue = _achievementConfig.TargetValue;
            var percentage = targetValue > 0 ? (currentValue / targetValue) * 100f : 0f;

            if (ProgressLabel != null)
                ProgressLabel.Text = $"进度: {currentValue:F0}/{targetValue:F0}";
            
            if (ProgressPercentLabel != null)
                ProgressPercentLabel.Text = $"{percentage:F1}%";
            
            if (ProgressBar != null)
            {
                ProgressBar.MaxValue = targetValue;
                ProgressBar.Value = currentValue;
            }
        }

        private void UpdateStatus()
        {
            if (_achievementProgress == null) return;

            string statusText = "";
            Color statusColor = Colors.White;

            switch (_achievementProgress.Status)
            {
                case AchievementStatus.Locked:
                    statusText = "已锁定";
                    statusColor = Colors.Gray;
                    break;
                case AchievementStatus.Unlocked:
                    statusText = "已解锁";
                    statusColor = Colors.Yellow;
                    break;
                case AchievementStatus.Completed:
                    statusText = "已完成";
                    statusColor = Colors.Green;
                    break;
            }

            if (StatusLabel != null)
            {
                StatusLabel.Text = statusText;
                StatusLabel.Modulate = statusColor;
            }

            // 更新完成时间
            if (CompletionTimeLabel != null && ShowCompletionTime)
            {
                if (_achievementProgress.Status == AchievementStatus.Completed && _achievementProgress.CompletionTime.HasValue)
                {
                    CompletionTimeLabel.Text = _achievementProgress.CompletionTime.Value.ToString("yyyy-MM-dd HH:mm");
                    CompletionTimeLabel.Visible = true;
                }
                else
                {
                    CompletionTimeLabel.Visible = false;
                }
            }
        }

        private void UpdateRewards()
        {
            if (!ShowRewards || _achievementConfig?.Rewards == null) return;

            // 清空现有奖励图标
            if (RewardIconsContainer != null)
            {
                foreach (Node child in RewardIconsContainer.GetChildren())
                {
                    child.QueueFree();
                }
            }

            // 添加奖励图标
            foreach (var reward in _achievementConfig.Rewards)
            {
                CreateRewardIcon(reward);
            }
        }

        private void CreateRewardIcon(RewardConfig reward)
        {
            if (RewardIconsContainer == null) return;

            var iconContainer = new VBoxContainer();
            var icon = new TextureRect()
            {
                CustomMinimumSize = new Vector2(24, 24),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
            };
            var label = new Label()
            {
                Text = reward.Amount.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // 根据奖励类型设置图标
            string iconPath = GetRewardIconPath(reward.Type);
            if (!string.IsNullOrEmpty(iconPath))
            {
                var texture = GD.Load<Texture2D>(iconPath);
                if (texture != null)
                    icon.Texture = texture;
            }

            iconContainer.AddChild(icon);
            iconContainer.AddChild(label);
            RewardIconsContainer.AddChild(iconContainer);
        }

        private string GetRewardIconPath(RewardType rewardType)
        {
            return rewardType switch
            {
                RewardType.Experience => "res://Icons/Rewards/experience.png",
                RewardType.Gold => "res://Icons/Rewards/gold.png",
                RewardType.Item => "res://Icons/Rewards/item.png",
                RewardType.Card => "res://Icons/Rewards/card.png",
                RewardType.Relic => "res://Icons/Rewards/relic.png",
                RewardType.Title => "res://Icons/Rewards/title.png",
                _ => "res://Icons/Rewards/default.png"
            };
        }

        private void UpdateActions()
        {
            if (_achievementProgress == null) return;

            bool canClaim = _achievementProgress.Status == AchievementStatus.Completed && !_achievementProgress.IsRewardClaimed;
            
            if (ClaimButton != null)
            {
                ClaimButton.Visible = canClaim;
                ClaimButton.Disabled = !canClaim;
            }

            if (ViewDetailButton != null)
            {
                ViewDetailButton.Visible = true;
            }
        }

        private void UpdateVisualStyle()
        {
            if (_achievementConfig == null || RarityIndicator == null) return;

            // 根据稀有度设置颜色
            Color rarityColor = _achievementConfig.Rarity switch
            {
                AchievementRarity.Common => Colors.Gray,
                AchievementRarity.Uncommon => Colors.Green,
                AchievementRarity.Rare => Colors.Blue,
                AchievementRarity.Epic => Colors.Purple,
                AchievementRarity.Legendary => Colors.Orange,
                _ => Colors.Gray
            };

            RarityIndicator.Color = rarityColor;

            // 根据状态调整整体透明度
            float alpha = _achievementProgress?.Status == AchievementStatus.Locked ? 0.6f : 1.0f;
            Modulate = new Color(1, 1, 1, alpha);
        }

        private void UpdateVisibility()
        {
            if (ProgressBar != null)
                ProgressBar.Visible = ShowProgressBar;
            
            if (RewardIconsContainer != null)
                RewardIconsContainer.GetParent<Control>().Visible = ShowRewards;
            
            if (CompletionTimeLabel != null)
                CompletionTimeLabel.Visible = ShowCompletionTime;
        }
        #endregion

        #region 选择状态
        /// <summary>
        /// 设置选择状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_isSelected == selected) return;

            _isSelected = selected;
            UpdateSelectionVisual();

            if (selected)
                EmitSignal(SignalName.AchievementSelected, _achievementConfig?.Id ?? "");
        }

        private void UpdateSelectionVisual()
        {
            if (Background == null) return;

            Color backgroundColor = _isSelected ? new Color(0.3f, 0.3f, 0.5f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            if (EnableAnimations && AnimationPlayer != null)
            {
                // 使用动画过渡
                AnimateBackgroundColor(backgroundColor);
            }
            else
            {
                // 直接设置颜色
                if (Background is Panel panel)
                {
                    var styleBox = new StyleBoxFlat();
                    styleBox.BgColor = backgroundColor;
                    panel.AddThemeStyleboxOverride("panel", styleBox);
                }
            }
        }

        private void AnimateBackgroundColor(Color targetColor)
        {
            if (_isAnimating) return;

            _isAnimating = true;
            var tween = CreateTween();
            
            if (Background is Panel panel)
            {
                var styleBox = new StyleBoxFlat();
                styleBox.BgColor = targetColor;
                panel.AddThemeStyleboxOverride("panel", styleBox);
            }
            
            tween.TweenCallback(Callable.From(() => _isAnimating = false));
        }
        #endregion

        #region 事件处理
        private void OnBackgroundInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                SetSelected(true);
            }
        }

        private void OnClaimButtonPressed()
        {
            if (_achievementConfig != null)
                EmitSignal(SignalName.ClaimRewardRequested, _achievementConfig.Id);
        }

        private void OnViewDetailButtonPressed()
        {
            if (_achievementConfig != null)
                EmitSignal(SignalName.ViewDetailRequested, _achievementConfig.Id);
        }
        #endregion

        #region 动画效果
        /// <summary>
        /// 播放完成动画
        /// </summary>
        public void PlayCompletionAnimation()
        {
            if (!EnableAnimations || AnimationPlayer == null) return;

            // 播放完成动画效果
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 缩放效果
            tween.TweenProperty(this, "scale", Vector2.One * 1.1f, 0.2f);
            tween.TweenProperty(this, "scale", Vector2.One, 0.2f).SetDelay(0.2f);
            
            // 发光效果
            tween.TweenProperty(this, "modulate", new Color(1.2f, 1.2f, 1.0f, 1.0f), 0.2f);
            tween.TweenProperty(this, "modulate", Colors.White, 0.2f).SetDelay(0.2f);
        }

        /// <summary>
        /// 播放进度更新动画
        /// </summary>
        public void PlayProgressUpdateAnimation()
        {
            if (!EnableAnimations || ProgressBar == null) return;

            var tween = CreateTween();
            tween.TweenProperty(ProgressBar, "modulate", new Color(1.0f, 1.5f, 1.0f, 1.0f), 0.3f);
            tween.TweenProperty(ProgressBar, "modulate", Colors.White, 0.3f);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取成就ID
        /// </summary>
        public string GetAchievementId()
        {
            return _achievementConfig?.Id ?? "";
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 检查是否匹配过滤条件
        /// </summary>
        public bool MatchesFilter(string searchText, AchievementCategory? category, AchievementStatus? status)
        {
            if (_achievementConfig == null || _achievementProgress == null)
                return false;

            // 搜索文本过滤
            if (!string.IsNullOrEmpty(searchText))
            {
                string lowerSearchText = searchText.ToLower();
                if (!_achievementConfig.Title.ToLower().Contains(lowerSearchText) &&
                    !_achievementConfig.Description.ToLower().Contains(lowerSearchText))
                {
                    return false;
                }
            }

            // 分类过滤
            if (category.HasValue && _achievementConfig.Category != category.Value)
                return false;

            // 状态过滤
            if (status.HasValue && _achievementProgress.Status != status.Value)
                return false;

            return true;
        }
        #endregion
    }
}