using Godot;
using Godot.Collections;
using System;
using System.Linq;
using CodeRogue.Achievements.Data;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 成就界面控制器 (ViewModel层)
    /// 负责成就界面的数据绑定和用户交互逻辑
    /// </summary>
    public partial class AchievementUI : Control
    {
        [Signal]
        public delegate void AchievementSelectedEventHandler(string achievementId);
        
        [Signal]
        public delegate void RewardClaimedEventHandler(string achievementId);
        
        [Signal]
        public delegate void CategoryChangedEventHandler(AchievementCategory category);
        
        [Signal]
        public delegate void FilterChangedEventHandler(AchievementFilter filter);
        
        [Signal]
        public delegate void SortChangedEventHandler(AchievementSortType sortType);

        // UI组件引用 - 通过Export在场景文件中绑定
        [Export] public Control AchievementListContainer { get; set; }
        [Export] public Control AchievementDetailPanel { get; set; }
        [Export] public Control CategoryTabContainer { get; set; }
        [Export] public Control FilterPanel { get; set; }
        [Export] public Control SortPanel { get; set; }
        [Export] public Control SearchPanel { get; set; }
        [Export] public Control StatisticsPanel { get; set; }
        [Export] public ScrollContainer AchievementScrollContainer { get; set; }
        [Export] public LineEdit SearchLineEdit { get; set; }
        [Export] public OptionButton CategoryOptionButton { get; set; }
        [Export] public OptionButton SortOptionButton { get; set; }
        [Export] public CheckBox CompletedFilterCheckBox { get; set; }
        [Export] public CheckBox UnlockedFilterCheckBox { get; set; }
        [Export] public CheckBox LockedFilterCheckBox { get; set; }
        [Export] public ProgressBar OverallProgressBar { get; set; }
        [Export] public Label OverallProgressLabel { get; set; }
        [Export] public Label TotalAchievementsLabel { get; set; }
        [Export] public Label CompletedAchievementsLabel { get; set; }
        [Export] public PackedScene AchievementItemPrefab { get; set; }
        
        // 数据和状态
        private AchievementManager _achievementManager;
        private AchievementDatabase _achievementDatabase;
        private Array<AchievementConfig> _allAchievements;
        private Array<AchievementConfig> _filteredAchievements;
        private Array<AchievementItemUI> _achievementItems;
        private AchievementConfig _selectedAchievement;
        private AchievementFilter _currentFilter;
        private AchievementCategory _currentCategory;
        private AchievementSortType _currentSortType;
        private string _searchText;
        
        // 配置参数
        [Export] public bool AutoRefresh { get; set; } = true;
        [Export] public float RefreshInterval { get; set; } = 1.0f;
        [Export] public bool EnableAnimations { get; set; } = true;
        [Export] public bool ShowProgressBars { get; set; } = true;
        [Export] public bool ShowStatistics { get; set; } = true;
        
        // 定时器
        private Timer _refreshTimer;
        
        public override void _Ready()
        {
            InitializeAchievementUI();
        }
        
        /// <summary>
        /// 初始化成就界面
        /// </summary>
        private void InitializeAchievementUI()
        {
            // 获取管理器引用
            _achievementManager = GetNode<AchievementManager>("/root/AchievementManager");
            _achievementDatabase = GetNode<AchievementDatabase>("/root/AchievementDatabase");
            
            if (_achievementManager == null || _achievementDatabase == null)
            {
                GD.PrintErr("[AchievementUI] 无法获取成就管理器或数据库引用");
                return;
            }
            
            // 初始化数据
            _allAchievements = new Array<AchievementConfig>();
            _filteredAchievements = new Array<AchievementConfig>();
            _achievementItems = new Array<AchievementItemUI>();
            _currentFilter = new AchievementFilter();
            _currentCategory = AchievementCategory.All;
            _currentSortType = AchievementSortType.Name;
            _searchText = string.Empty;
            
            // 验证UI组件
            ValidateUIComponents();
            
            // 设置UI事件
            SetupUIEvents();
            
            // 初始化UI状态
            InitializeUIState();
            
            // 连接管理器信号
            ConnectManagerSignals();
            
            // 设置自动刷新
            SetupAutoRefresh();
            
            // 加载成就数据
            LoadAchievements();
            
            GD.Print("[AchievementUI] 成就界面初始化完成");
        }
        
        /// <summary>
        /// 验证UI组件
        /// </summary>
        private void ValidateUIComponents()
        {
            var requiredComponents = new (string name, object component)[]
            {
                ("AchievementListContainer", AchievementListContainer),
                ("AchievementScrollContainer", AchievementScrollContainer),
                ("AchievementItemPrefab", AchievementItemPrefab)
            };
            
            foreach (var (name, component) in requiredComponents)
            {
                if (component == null)
                {
                    GD.PrintErr($"[AchievementUI] 必需的UI组件未设置: {name}");
                }
            }
        }
        
        /// <summary>
        /// 设置UI事件
        /// </summary>
        private void SetupUIEvents()
        {
            // 搜索事件
            if (SearchLineEdit != null)
            {
                SearchLineEdit.TextChanged += OnSearchTextChanged;
            }
            
            // 分类选择事件
            if (CategoryOptionButton != null)
            {
                CategoryOptionButton.ItemSelected += OnCategorySelected;
                PopulateCategoryOptions();
            }
            
            // 排序选择事件
            if (SortOptionButton != null)
            {
                SortOptionButton.ItemSelected += OnSortSelected;
                PopulateSortOptions();
            }
            
            // 过滤器事件
            if (CompletedFilterCheckBox != null)
                CompletedFilterCheckBox.Toggled += (pressed) => OnFilterChanged();
            
            if (UnlockedFilterCheckBox != null)
                UnlockedFilterCheckBox.Toggled += (pressed) => OnFilterChanged();
            
            if (LockedFilterCheckBox != null)
                LockedFilterCheckBox.Toggled += (pressed) => OnFilterChanged();
        }
        
        /// <summary>
        /// 初始化UI状态
        /// </summary>
        private void InitializeUIState()
        {
            // 设置默认过滤器状态
            if (CompletedFilterCheckBox != null)
                CompletedFilterCheckBox.ButtonPressed = true;
            
            if (UnlockedFilterCheckBox != null)
                UnlockedFilterCheckBox.ButtonPressed = true;
            
            if (LockedFilterCheckBox != null)
                LockedFilterCheckBox.ButtonPressed = true;
            
            // 隐藏详情面板
            if (AchievementDetailPanel != null)
                AchievementDetailPanel.Visible = false;
        }
        
        /// <summary>
        /// 连接管理器信号
        /// </summary>
        private void ConnectManagerSignals()
        {
            if (_achievementManager != null)
            {
                _achievementManager.AchievementCompleted += OnAchievementCompleted;
                _achievementManager.AchievementUnlocked += OnAchievementUnlocked;
                _achievementManager.AchievementProgressUpdated += OnAchievementProgressUpdated;
                _achievementManager.RewardGranted += OnRewardGranted;
            }
        }
        
        /// <summary>
        /// 设置自动刷新
        /// </summary>
        private void SetupAutoRefresh()
        {
            if (AutoRefresh)
            {
                _refreshTimer = new Timer();
                _refreshTimer.WaitTime = RefreshInterval;
                _refreshTimer.Autostart = true;
                _refreshTimer.Timeout += RefreshAchievements;
                AddChild(_refreshTimer);
            }
        }
        
        /// <summary>
        /// 填充分类选项
        /// </summary>
        private void PopulateCategoryOptions()
        {
            if (CategoryOptionButton == null)
                return;
            
            CategoryOptionButton.Clear();
            
            var categories = Enum.GetValues<AchievementCategory>();
            foreach (var category in categories)
            {
                CategoryOptionButton.AddItem(GetCategoryDisplayName(category));
            }
        }
        
        /// <summary>
        /// 填充排序选项
        /// </summary>
        private void PopulateSortOptions()
        {
            if (SortOptionButton == null)
                return;
            
            SortOptionButton.Clear();
            
            var sortTypes = Enum.GetValues<AchievementSortType>();
            foreach (var sortType in sortTypes)
            {
                SortOptionButton.AddItem(GetSortTypeDisplayName(sortType));
            }
        }
        
        /// <summary>
        /// 加载成就数据
        /// </summary>
        private void LoadAchievements()
        {
            if (_achievementDatabase == null)
                return;
            
            _allAchievements = _achievementDatabase.GetAllAchievements();
            ApplyFiltersAndSort();
            UpdateAchievementList();
            UpdateStatistics();
        }
        
        /// <summary>
        /// 应用过滤器和排序
        /// </summary>
        private void ApplyFiltersAndSort()
        {
            _filteredAchievements.Clear();
            
            foreach (var achievement in _allAchievements)
            {
                if (ShouldShowAchievement(achievement))
                {
                    _filteredAchievements.Add(achievement);
                }
            }
            
            // 排序
            SortAchievements();
        }
        
        /// <summary>
        /// 检查是否应该显示成就
        /// </summary>
        private bool ShouldShowAchievement(AchievementConfig achievement)
        {
            // 分类过滤
            if (_currentCategory != AchievementCategory.All && achievement.Category != _currentCategory)
                return false;
            
            // 搜索过滤
            if (!string.IsNullOrEmpty(_searchText))
            {
                var searchLower = _searchText.ToLower();
                if (!achievement.Name.ToLower().Contains(searchLower) && 
                    !achievement.Description.ToLower().Contains(searchLower))
                    return false;
            }
            
            // 状态过滤
            var progress = _achievementManager?.GetAchievementProgress(achievement.Id);
            if (progress != null)
            {
                var isCompleted = progress.IsCompleted;
                var isUnlocked = progress.Status == AchievementStatus.Unlocked;
                var isLocked = progress.Status == AchievementStatus.Locked;
                
                if (isCompleted && !(CompletedFilterCheckBox?.ButtonPressed ?? true))
                    return false;
                
                if (isUnlocked && !isCompleted && !(UnlockedFilterCheckBox?.ButtonPressed ?? true))
                    return false;
                
                if (isLocked && !(LockedFilterCheckBox?.ButtonPressed ?? true))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 排序成就
        /// </summary>
        private void SortAchievements()
        {
            switch (_currentSortType)
            {
                case AchievementSortType.Name:
                    _filteredAchievements = new Array<AchievementConfig>(_filteredAchievements.OrderBy(a => a.Name));
                    break;
                
                case AchievementSortType.Category:
                    _filteredAchievements = new Array<AchievementConfig>(_filteredAchievements.OrderBy(a => a.Category).ThenBy(a => a.Name));
                    break;
                
                case AchievementSortType.Rarity:
                    _filteredAchievements = new Array<AchievementConfig>(_filteredAchievements.OrderBy(a => a.Rarity).ThenBy(a => a.Name));
                    break;
                
                case AchievementSortType.Progress:
                    _filteredAchievements = new Array<AchievementConfig>(_filteredAchievements.OrderByDescending(a => GetAchievementProgressPercentage(a)).ThenBy(a => a.Name));
                    break;
                
                case AchievementSortType.CompletionDate:
                    _filteredAchievements = new Array<AchievementConfig>(_filteredAchievements.OrderByDescending(a => GetAchievementCompletionDate(a)).ThenBy(a => a.Name));
                    break;
            }
        }
        
        /// <summary>
        /// 更新成就列表
        /// </summary>
        private void UpdateAchievementList()
        {
            if (AchievementListContainer == null || AchievementItemPrefab == null)
                return;
            
            // 清空现有项目
            ClearAchievementItems();
            
            // 创建新的成就项目
            foreach (var achievement in _filteredAchievements)
            {
                CreateAchievementItem(achievement);
            }
        }
        
        /// <summary>
        /// 清空成就项目
        /// </summary>
        private void ClearAchievementItems()
        {
            foreach (var item in _achievementItems)
            {
                if (IsInstanceValid(item))
                {
                    item.QueueFree();
                }
            }
            _achievementItems.Clear();
        }
        
        /// <summary>
        /// 创建成就项目
        /// </summary>
        private void CreateAchievementItem(AchievementConfig achievement)
        {
            var itemInstance = AchievementItemPrefab.Instantiate() as AchievementItemUI;
            if (itemInstance == null)
            {
                GD.PrintErr("[AchievementUI] 无法实例化成就项目预制体");
                return;
            }
            
            // 配置成就项目
            var progress = _achievementManager?.GetAchievementProgress(achievement.Id);
            itemInstance.SetupAchievementItem(achievement, progress);
            
            // 连接事件
            itemInstance.AchievementClicked += OnAchievementItemClicked;
            itemInstance.RewardClaimClicked += OnRewardClaimClicked;
            
            // 添加到容器
            AchievementListContainer.AddChild(itemInstance);
            _achievementItems.Add(itemInstance);
        }
        
        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStatistics()
        {
            if (!ShowStatistics)
                return;
            
            var totalCount = _allAchievements.Count;
            var completedCount = 0;
            var totalProgress = 0.0f;
            
            foreach (var achievement in _allAchievements)
            {
                var progress = _achievementManager?.GetAchievementProgress(achievement.Id);
                if (progress != null)
                {
                    if (progress.IsCompleted)
                        completedCount++;
                    
                    totalProgress += progress.GetProgressPercentage();
                }
            }
            
            var overallProgress = totalCount > 0 ? totalProgress / totalCount : 0.0f;
            
            // 更新UI
            if (TotalAchievementsLabel != null)
                TotalAchievementsLabel.Text = $"总成就: {totalCount}";
            
            if (CompletedAchievementsLabel != null)
                CompletedAchievementsLabel.Text = $"已完成: {completedCount}";
            
            if (OverallProgressBar != null)
                OverallProgressBar.Value = overallProgress;
            
            if (OverallProgressLabel != null)
                OverallProgressLabel.Text = $"总进度: {overallProgress:F1}%";
        }
        
        /// <summary>
        /// 刷新成就数据
        /// </summary>
        private void RefreshAchievements()
        {
            ApplyFiltersAndSort();
            UpdateAchievementList();
            UpdateStatistics();
        }
        
        /// <summary>
        /// 获取成就进度百分比
        /// </summary>
        private float GetAchievementProgressPercentage(AchievementConfig achievement)
        {
            var progress = _achievementManager?.GetAchievementProgress(achievement.Id);
            return progress?.GetProgressPercentage() ?? 0.0f;
        }
        
        /// <summary>
        /// 获取成就完成日期
        /// </summary>
        private DateTime GetAchievementCompletionDate(AchievementConfig achievement)
        {
            var progress = _achievementManager?.GetAchievementProgress(achievement.Id);
            return progress?.CompletionTime ?? DateTime.MinValue;
        }
        
        /// <summary>
        /// 获取分类显示名称
        /// </summary>
        private string GetCategoryDisplayName(AchievementCategory category)
        {
            return category switch
            {
                AchievementCategory.All => "全部",
                AchievementCategory.Combat => "战斗",
                AchievementCategory.Exploration => "探索",
                AchievementCategory.Collection => "收集",
                AchievementCategory.Social => "社交",
                AchievementCategory.Progression => "进度",
                AchievementCategory.Special => "特殊",
                AchievementCategory.Hidden => "隐藏",
                _ => category.ToString()
            };
        }
        
        /// <summary>
        /// 获取排序类型显示名称
        /// </summary>
        private string GetSortTypeDisplayName(AchievementSortType sortType)
        {
            return sortType switch
            {
                AchievementSortType.Name => "名称",
                AchievementSortType.Category => "分类",
                AchievementSortType.Rarity => "稀有度",
                AchievementSortType.Progress => "进度",
                AchievementSortType.CompletionDate => "完成日期",
                _ => sortType.ToString()
            };
        }
        
        // UI事件处理
        private void OnSearchTextChanged(string newText)
        {
            _searchText = newText;
            ApplyFiltersAndSort();
            UpdateAchievementList();
        }
        
        private void OnCategorySelected(long index)
        {
            var categories = Enum.GetValues<AchievementCategory>();
            if (index >= 0 && index < categories.Length)
            {
                _currentCategory = categories[index];
                EmitSignal(SignalName.CategoryChanged, (int)_currentCategory);
                ApplyFiltersAndSort();
                UpdateAchievementList();
            }
        }
        
        private void OnSortSelected(long index)
        {
            var sortTypes = Enum.GetValues<AchievementSortType>();
            if (index >= 0 && index < sortTypes.Length)
            {
                _currentSortType = sortTypes[index];
                EmitSignal(SignalName.SortChanged, (int)_currentSortType);
                ApplyFiltersAndSort();
                UpdateAchievementList();
            }
        }
        
        private void OnFilterChanged()
        {
            _currentFilter.ShowCompleted = CompletedFilterCheckBox?.ButtonPressed ?? true;
            _currentFilter.ShowUnlocked = UnlockedFilterCheckBox?.ButtonPressed ?? true;
            _currentFilter.ShowLocked = LockedFilterCheckBox?.ButtonPressed ?? true;
            
            EmitSignal(SignalName.FilterChanged, _currentFilter);
            ApplyFiltersAndSort();
            UpdateAchievementList();
        }
        
        private void OnAchievementItemClicked(string achievementId)
        {
            _selectedAchievement = _achievementDatabase?.GetAchievementById(achievementId);
            EmitSignal(SignalName.AchievementSelected, achievementId);
            ShowAchievementDetails(achievementId);
        }
        
        private void OnRewardClaimClicked(string achievementId)
        {
            EmitSignal(SignalName.RewardClaimed, achievementId);
            // TODO: 处理奖励领取逻辑
        }
        
        /// <summary>
        /// 显示成就详情
        /// </summary>
        private void ShowAchievementDetails(string achievementId)
        {
            if (AchievementDetailPanel == null)
                return;
            
            var achievement = _achievementDatabase?.GetAchievementById(achievementId);
            var progress = _achievementManager?.GetAchievementProgress(achievementId);
            
            if (achievement != null)
            {
                // TODO: 配置详情面板UI
                AchievementDetailPanel.Visible = true;
            }
        }
        
        // 管理器信号处理
        private void OnAchievementCompleted(string achievementId)
        {
            RefreshAchievements();
        }
        
        private void OnAchievementUnlocked(string achievementId)
        {
            RefreshAchievements();
        }
        
        private void OnAchievementProgressUpdated(string achievementId, float currentValue, float targetValue)
        {
            // 更新特定成就项目的进度
            var achievementItem = _achievementItems.FirstOrDefault(item => item.AchievementId == achievementId);
            achievementItem?.UpdateProgress(currentValue, targetValue);
        }
        
        private void OnRewardGranted(string achievementId, Array<RewardConfig> rewards)
        {
            RefreshAchievements();
        }
        
        /// <summary>
        /// 公共方法：手动刷新
        /// </summary>
        public void ManualRefresh()
        {
            LoadAchievements();
        }
        
        /// <summary>
        /// 公共方法：设置分类过滤
        /// </summary>
        public void SetCategoryFilter(AchievementCategory category)
        {
            _currentCategory = category;
            if (CategoryOptionButton != null)
            {
                CategoryOptionButton.Selected = (int)category;
            }
            ApplyFiltersAndSort();
            UpdateAchievementList();
        }
        
        /// <summary>
        /// 公共方法：设置搜索文本
        /// </summary>
        public void SetSearchText(string searchText)
        {
            _searchText = searchText;
            if (SearchLineEdit != null)
            {
                SearchLineEdit.Text = searchText;
            }
            ApplyFiltersAndSort();
            UpdateAchievementList();
        }
    }
    
    /// <summary>
    /// 成就过滤器
    /// </summary>
    public partial class AchievementFilter : RefCounted
    {
        public bool ShowCompleted { get; set; } = true;
        public bool ShowUnlocked { get; set; } = true;
        public bool ShowLocked { get; set; } = true;
        public AchievementCategory Category { get; set; } = AchievementCategory.All;
        public AchievementRarity MinRarity { get; set; } = AchievementRarity.Common;
        public AchievementRarity MaxRarity { get; set; } = AchievementRarity.Legendary;
    }
    
    /// <summary>
    /// 成就排序类型
    /// </summary>
    public enum AchievementSortType
    {
        Name,
        Category,
        Rarity,
        Progress,
        CompletionDate
    }
    
    /// <summary>
    /// 成就项目UI（需要单独实现）
    /// </summary>
    public partial class AchievementItemUI : Control
    {
        [Signal]
        public delegate void AchievementClickedEventHandler(string achievementId);
        
        [Signal]
        public delegate void RewardClaimClickedEventHandler(string achievementId);
        
        public string AchievementId { get; private set; }
        
        public void SetupAchievementItem(AchievementConfig achievement, AchievementProgress progress)
        {
            AchievementId = achievement.Id;
            // TODO: 实现成就项目UI设置逻辑
        }
        
        public void UpdateProgress(float currentValue, float targetValue)
        {
            // TODO: 实现进度更新逻辑
        }
    }
}