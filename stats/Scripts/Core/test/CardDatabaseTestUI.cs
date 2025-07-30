using Godot;
using Godot.Collections;
using System.Linq;
using CodeRogue.UI;

/// <summary>
/// CardDatabase测试UI - 显示数据库中的所有卡牌并提供过滤、筛选、排序功能
/// </summary>
public partial class CardDatabaseTestUI : Control
{
    [Export] private TabContainer _tabContainer;
    [Export] private GridContainer _monsterCardsContainer;
    [Export] private GridContainer _skillCardsContainer;
    [Export] private ScrollContainer _monsterScrollContainer;
    [Export] private ScrollContainer _skillScrollContainer;
    
    // 过滤控件
    [Export] private OptionButton _monsterRarityFilter;
    [Export] private OptionButton _monsterRaceFilter;
    [Export] private OptionButton _skillRarityFilter;
    [Export] private OptionButton _skillTypeFilter;
    [Export] private OptionButton _colorFilter;
    [Export] private OptionButton _sortOption;
    [Export] private LineEdit _searchInput;
    [Export] private Button _resetFiltersButton;
    [Export] private Label _statsLabel;
    
    public override void _Ready()
    {
        // 初始化组件
        InitializeComponents();
    }
    
    private CardDatabase _cardDatabase;
    private PackedScene _skillCardUIScene;
    
    // 当前过滤条件
    private CardRarity? _currentMonsterRarityFilter = null;
    private MonsterRace? _currentMonsterRaceFilter = null;
    private CardRarity? _currentSkillRarityFilter = null;
    private SkillType? _currentSkillTypeFilter = null;
    private MagicColor? _currentColorFilter = null;
    private string _currentSearchText = "";
    private SortType _currentSortType = SortType.Name;
    
    public enum SortType
    {
        Name,
        Rarity,
        Cost,
        Attack, // 仅怪物卡
        Health  // 仅怪物卡
    }
    
    private void InitializeComponents()
    {
        InitializeDatabase();
        InitializeUI();
        LoadCardUIScene();
        PopulateFilterOptions();
        ConnectSignals();
        RefreshDisplay();
    }
    
    private void InitializeDatabase()
    {
        _cardDatabase = GetNode<CardDatabase>("/root/CardDatabase");
        if (_cardDatabase == null)
        {
            GD.PrintErr("CardDatabase not found in AutoLoad!");
            return;
        }
    }
    
    private void InitializeUI()
    {
        if (_statsLabel != null)
        {
            UpdateStatsDisplay();
        }
    }
    
    private void LoadCardUIScene()
    {
        _skillCardUIScene = GD.Load<PackedScene>("res://Scenes/UI/SkillCardUI.tscn");
        if (_skillCardUIScene == null)
        {
            GD.PrintErr("Failed to load SkillCardUI scene!");
        }
    }
    
    private void PopulateFilterOptions()
    {
        // 怪物稀有度过滤器
        if (_monsterRarityFilter != null)
        {
            _monsterRarityFilter.AddItem("全部稀有度");
            foreach (CardRarity rarity in System.Enum.GetValues<CardRarity>())
            {
                _monsterRarityFilter.AddItem(rarity.ToString());
            }
        }
        
        // 怪物种族过滤器
        if (_monsterRaceFilter != null)
        {
            _monsterRaceFilter.AddItem("全部种族");
            foreach (MonsterRace race in System.Enum.GetValues<MonsterRace>())
            {
                _monsterRaceFilter.AddItem(race.ToString());
            }
        }
        
        // 技能稀有度过滤器
        if (_skillRarityFilter != null)
        {
            _skillRarityFilter.AddItem("全部稀有度");
            foreach (CardRarity rarity in System.Enum.GetValues<CardRarity>())
            {
                _skillRarityFilter.AddItem(rarity.ToString());
            }
        }
        
        // 技能类型过滤器
        if (_skillTypeFilter != null)
        {
            _skillTypeFilter.AddItem("全部类型");
            foreach (SkillType type in System.Enum.GetValues<SkillType>())
            {
                _skillTypeFilter.AddItem(type.ToString());
            }
        }
        
        // 颜色过滤器
        if (_colorFilter != null)
        {
            _colorFilter.AddItem("全部颜色");
            foreach (MagicColor color in System.Enum.GetValues<MagicColor>())
            {
                _colorFilter.AddItem(color.ToString());
            }
        }
        
        // 排序选项
        if (_sortOption != null)
        {
            _sortOption.AddItem("按名称排序");
            _sortOption.AddItem("按稀有度排序");
            _sortOption.AddItem("按消耗排序");
            _sortOption.AddItem("按攻击力排序");
            _sortOption.AddItem("按生命值排序");
        }
    }
    
    private void ConnectSignals()
    {
        if (_monsterRarityFilter != null)
            _monsterRarityFilter.ItemSelected += OnMonsterRarityFilterChanged;
        if (_monsterRaceFilter != null)
            _monsterRaceFilter.ItemSelected += OnMonsterRaceFilterChanged;
        if (_skillRarityFilter != null)
            _skillRarityFilter.ItemSelected += OnSkillRarityFilterChanged;
        if (_skillTypeFilter != null)
            _skillTypeFilter.ItemSelected += OnSkillTypeFilterChanged;
        if (_colorFilter != null)
            _colorFilter.ItemSelected += OnColorFilterChanged;
        if (_sortOption != null)
            _sortOption.ItemSelected += OnSortOptionChanged;
        if (_searchInput != null)
            _searchInput.TextChanged += OnSearchTextChanged;
        if (_resetFiltersButton != null)
            _resetFiltersButton.Pressed += OnResetFiltersPressed;
    }
    
    private void OnMonsterRarityFilterChanged(long index)
    {
        _currentMonsterRarityFilter = index == 0 ? null : (CardRarity)(index - 1);
        RefreshDisplay();
    }
    
    private void OnMonsterRaceFilterChanged(long index)
    {
        _currentMonsterRaceFilter = index == 0 ? null : (MonsterRace)(index - 1);
        RefreshDisplay();
    }
    
    private void OnSkillRarityFilterChanged(long index)
    {
        _currentSkillRarityFilter = index == 0 ? null : (CardRarity)(index - 1);
        RefreshDisplay();
    }
    
    private void OnSkillTypeFilterChanged(long index)
    {
        _currentSkillTypeFilter = index == 0 ? null : (SkillType)(index - 1);
        RefreshDisplay();
    }
    
    private void OnColorFilterChanged(long index)
    {
        _currentColorFilter = index == 0 ? null : (MagicColor)(index - 1);
        RefreshDisplay();
    }
    
    private void OnSortOptionChanged(long index)
    {
        _currentSortType = (SortType)index;
        RefreshDisplay();
    }
    
    private void OnSearchTextChanged(string newText)
    {
        _currentSearchText = newText.ToLower();
        RefreshDisplay();
    }
    
    private void OnResetFiltersPressed()
    {
        ResetAllFilters();
        RefreshDisplay();
    }
    
    private void ResetAllFilters()
    {
        _currentMonsterRarityFilter = null;
        _currentMonsterRaceFilter = null;
        _currentSkillRarityFilter = null;
        _currentSkillTypeFilter = null;
        _currentColorFilter = null;
        _currentSearchText = "";
        _currentSortType = SortType.Name;
        
        // 重置UI控件
        if (_monsterRarityFilter != null) _monsterRarityFilter.Selected = 0;
        if (_monsterRaceFilter != null) _monsterRaceFilter.Selected = 0;
        if (_skillRarityFilter != null) _skillRarityFilter.Selected = 0;
        if (_skillTypeFilter != null) _skillTypeFilter.Selected = 0;
        if (_colorFilter != null) _colorFilter.Selected = 0;
        if (_sortOption != null) _sortOption.Selected = 0;
        if (_searchInput != null) _searchInput.Text = "";
    }
    
    private void RefreshDisplay()
    {
        if (_cardDatabase == null) return;
        
        DisplayMonsterCards();
        DisplaySkillCards();
        UpdateStatsDisplay();
    }
    
    private void DisplayMonsterCards()
    {
        if (_monsterCardsContainer == null) return;
        
        // 清除现有显示
        foreach (Node child in _monsterCardsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var filteredCards = FilterMonsterCards(_cardDatabase.GetAllMonsterCards());
        var sortedCards = SortMonsterCards(filteredCards);
        
        foreach (var card in sortedCards)
        {
            var cardLabel = CreateMonsterCardDisplay(card);
            _monsterCardsContainer.AddChild(cardLabel);
        }
    }
    
    private void DisplaySkillCards()
    {
        if (_skillCardsContainer == null) return;
        
        // 清除现有显示
        foreach (Node child in _skillCardsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var filteredCards = FilterSkillCards(_cardDatabase.GetAllSkillCards());
        var sortedCards = SortSkillCards(filteredCards);
        
        foreach (var card in sortedCards)
        {
            if (_skillCardUIScene != null)
            {
                var cardUI = _skillCardUIScene.Instantiate<SkillCardUI>();
                cardUI.SetSkillCard(card);
                _skillCardsContainer.AddChild(cardUI);
            }
            else
            {
                var cardLabel = CreateSkillCardDisplay(card);
                _skillCardsContainer.AddChild(cardLabel);
            }
        }
    }
    
    private Array<MonsterCard> FilterMonsterCards(Array<MonsterCard> cards)
    {
        var filtered = new Array<MonsterCard>();
        
        foreach (var card in cards)
        {
            // 稀有度过滤
            if (_currentMonsterRarityFilter.HasValue && card.Rarity != _currentMonsterRarityFilter.Value)
                continue;
            
            // 种族过滤
            if (_currentMonsterRaceFilter.HasValue && card.Race != _currentMonsterRaceFilter.Value)
                continue;
            
            // 颜色过滤
            if (_currentColorFilter.HasValue && !card.ColorRequirements.Contains(_currentColorFilter.Value))
                continue;
            
            // 搜索文本过滤
            if (!string.IsNullOrEmpty(_currentSearchText))
            {
                var searchText = _currentSearchText.ToLower();
                if (!card.CardName.ToLower().Contains(searchText) && 
                    !card.Description.ToLower().Contains(searchText))
                    continue;
            }
            
            filtered.Add(card);
        }
        
        return filtered;
    }
    
    private Array<SkillCard> FilterSkillCards(Array<SkillCard> cards)
    {
        var filtered = new Array<SkillCard>();
        
        foreach (var card in cards)
        {
            // 稀有度过滤
            if (_currentSkillRarityFilter.HasValue && card.Rarity != _currentSkillRarityFilter.Value)
                continue;
            
            // 技能类型过滤
            if (_currentSkillTypeFilter.HasValue && card.SkillType != _currentSkillTypeFilter.Value)
                continue;
            
            // 颜色过滤
            if (_currentColorFilter.HasValue && !card.ColorRequirements.Contains(_currentColorFilter.Value))
                continue;
            
            // 搜索文本过滤
            if (!string.IsNullOrEmpty(_currentSearchText))
            {
                var searchText = _currentSearchText.ToLower();
                if (!card.CardName.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase) && 
                    !card.Description.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase))
                    continue;
            }
            
            filtered.Add(card);
        }
        
        return filtered;
    }
    
    private Array<MonsterCard> SortMonsterCards(Array<MonsterCard> cards)
    {
        return _currentSortType switch
        {
            SortType.Name => new Array<MonsterCard>(cards.OrderBy(c => c.CardName)),
            SortType.Rarity => new Array<MonsterCard>(cards.OrderBy(c => c.Rarity)),
            SortType.Cost => new Array<MonsterCard>(cards.OrderBy(c => c.Cost)),
            SortType.Attack => new Array<MonsterCard>(cards.OrderByDescending(c => c.Attack)),
            SortType.Health => new Array<MonsterCard>(cards.OrderByDescending(c => c.Health)),
            _ => cards
        };
    }
    
    private Array<SkillCard> SortSkillCards(Array<SkillCard> cards)
    {
        return _currentSortType switch
        {
            SortType.Name => new Array<SkillCard>(cards.OrderBy(c => c.CardName)),
            SortType.Rarity => new Array<SkillCard>(cards.OrderBy(c => c.Rarity)),
			SortType.Cost => new Array<SkillCard>(cards.OrderBy(c => c.Cost)),
            _ => cards
        };
    }
    
    private Control CreateMonsterCardDisplay(MonsterCard card)
    {
        var container = new VBoxContainer();
        var panel = new Panel();
        var vbox = new VBoxContainer();
        
        // 设置稀有度颜色
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = GetRarityColor(card.Rarity);
        styleBox.SetCornerRadiusAll(8);
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderColor = GetRarityColor(card.Rarity).Lightened(0.3f);
        panel.AddThemeStyleboxOverride("panel", styleBox);
        
        var nameLabel = new Label();
        nameLabel.Text = $"{card.CardName} ({card.Rarity})";
        nameLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
        
        var statsLabel = new Label();
        statsLabel.Text = $"攻击: {card.Attack} | 生命: {card.Health} | 消耗: {card.Cost}";
        
        var raceLabel = new Label();
        raceLabel.Text = $"种族: {card.Race}";
        
        var descLabel = new Label();
        descLabel.Text = card.Description;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        
        vbox.AddChild(nameLabel);
        vbox.AddChild(statsLabel);
        vbox.AddChild(raceLabel);
        vbox.AddChild(descLabel);
        
        panel.AddChild(vbox);
        container.AddChild(panel);
        
        return container;
    }
    
    private Control CreateSkillCardDisplay(SkillCard card)
    {
        var container = new VBoxContainer();
        var panel = new Panel();
        var vbox = new VBoxContainer();
        
        // 设置稀有度颜色
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = GetRarityColor(card.Rarity);
		styleBox.SetCornerRadiusAll(8);
		styleBox.BorderWidthTop = 2;
		styleBox.BorderWidthBottom = 2;
		styleBox.BorderWidthLeft = 2;
		styleBox.BorderWidthRight = 2;
		styleBox.BorderColor = GetRarityColor(card.Rarity).Lightened(0.3f);
        panel.AddThemeStyleboxOverride("panel", styleBox);
        
        var nameLabel = new Label();
        nameLabel.Text = $"{card.CardName} ({card.Rarity})";
		
		var typeLabel = new Label();
		typeLabel.Text = $"类型: {card.SkillType} | 消耗: {card.Cost}";
        
        var descLabel = new Label();
        descLabel.Text = card.Description;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        
        vbox.AddChild(nameLabel);
        vbox.AddChild(typeLabel);
        vbox.AddChild(descLabel);
        
        panel.AddChild(vbox);
        container.AddChild(panel);
        
        return container;
    }
    
    private Color GetRarityColor(CardRarity rarity)
    {
        return rarity switch
        {
            CardRarity.Common => new Color(0.8f, 0.8f, 0.8f, 0.3f),
            CardRarity.Uncommon => new Color(0.4f, 0.8f, 0.4f, 0.3f),
            CardRarity.Rare => new Color(0.3f, 0.6f, 1f, 0.3f),
            CardRarity.Epic => new Color(0.8f, 0.3f, 1f, 0.3f),
            CardRarity.Legendary => new Color(1f, 0.6f, 0.1f, 0.3f),
            _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
        };
    }
    
    private void UpdateStatsDisplay()
    {
        if (_statsLabel == null || _cardDatabase == null) return;
        
        var totalMonsters = _cardDatabase.GetAllMonsterCards().Count;
        var totalSkills = _cardDatabase.GetAllSkillCards().Count;
        var filteredMonsters = FilterMonsterCards(_cardDatabase.GetAllMonsterCards()).Count;
        var filteredSkills = FilterSkillCards(_cardDatabase.GetAllSkillCards()).Count;
        
        _statsLabel.Text = $"怪物卡: {filteredMonsters}/{totalMonsters} | 技能卡: {filteredSkills}/{totalSkills}";
    }
}