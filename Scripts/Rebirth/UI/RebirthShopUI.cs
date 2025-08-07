using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Rebirth;
using CodeRogue.Rebirth.Data;

/// <summary>
/// 死亡重来商店UI控制器
/// </summary>
public partial class RebirthShopUI : Control
{
    private RebirthManager _rebirthManager;
    private ShopItem _selectedItem;
    
    // UI节点引用
    private Label _currencyLabel;
    private Label _playerLevelLabel;
    private TabContainer _tabContainer;
    private GridContainer _allItemsGrid;
    private GridContainer _upgradesGrid;
    private GridContainer _unlocksGrid;
    private GridContainer _boostsGrid;
    private GridContainer _specialGrid;
    private AcceptDialog _purchaseDialog;
    private AcceptDialog _statisticsDialog;
    private RichTextLabel _statisticsLabel;
    private Button _confirmButton;
    private Label _itemNameLabel;
    private Label _itemDescriptionLabel;
    private Label _priceLabel;
    private Label _currencyAfterLabel;
    private TextureRect _itemIcon;
    
    [Signal] public delegate void ShopClosedEventHandler();
    [Signal] public delegate void ItemPurchasedEventHandler(ShopItem item);
    
    public override void _Ready()
    {
        InitializeNodes();
        ConnectSignals();
        
        // 获取死亡重来管理器
        _rebirthManager = GetNode<RebirthManager>("/root/RebirthManager");
        if (_rebirthManager != null)
        {
            ConnectManagerSignals();
            RefreshUI();
        }
        else
        {
            GD.PrintErr("无法找到RebirthManager");
        }
    }
    
    /// <summary>
    /// 初始化UI节点
    /// </summary>
    private void InitializeNodes()
    {
        _currencyLabel = GetNode<Label>("MainContainer/PlayerInfo/CurrencyContainer/CurrencyLabel");
        _playerLevelLabel = GetNode<Label>("MainContainer/PlayerInfo/PlayerLevel");
        _tabContainer = GetNode<TabContainer>("MainContainer/TabContainer");
        _allItemsGrid = GetNode<GridContainer>("MainContainer/TabContainer/全部/AllItemsGrid");
        _upgradesGrid = GetNode<GridContainer>("MainContainer/TabContainer/永久升级/UpgradesGrid");
        _unlocksGrid = GetNode<GridContainer>("MainContainer/TabContainer/解锁内容/UnlocksGrid");
        _boostsGrid = GetNode<GridContainer>("MainContainer/TabContainer/临时增益/BoostsGrid");
        _specialGrid = GetNode<GridContainer>("MainContainer/TabContainer/特殊商品/SpecialGrid");
        _purchaseDialog = GetNode<AcceptDialog>("PurchaseDialog");
        _statisticsDialog = GetNode<AcceptDialog>("StatisticsDialog");
        _statisticsLabel = GetNode<RichTextLabel>("StatisticsDialog/StatisticsContent/StatisticsLabel");
        _confirmButton = GetNode<Button>("PurchaseDialog/PurchaseContent/ConfirmButton");
        _itemNameLabel = GetNode<Label>("PurchaseDialog/PurchaseContent/ItemName");
        _itemDescriptionLabel = GetNode<Label>("PurchaseDialog/PurchaseContent/ItemDescription");
        _priceLabel = GetNode<Label>("PurchaseDialog/PurchaseContent/PriceInfo/PriceLabel");
        _currencyAfterLabel = GetNode<Label>("PurchaseDialog/PurchaseContent/PriceInfo/CurrencyAfter");
        _itemIcon = GetNode<TextureRect>("PurchaseDialog/PurchaseContent/ItemIcon");
    }
    
    /// <summary>
    /// 连接UI信号
    /// </summary>
    private void ConnectSignals()
    {
        _tabContainer.TabChanged += OnTabChanged;
    }
    
    /// <summary>
    /// 连接管理器信号
    /// </summary>
    private void ConnectManagerSignals()
    {
        _rebirthManager.ShopManager.ShopItemPurchased += OnShopItemPurchased;
        _rebirthManager.ShopManager.ShopRefreshed += OnShopRefreshed;
        _rebirthManager.ShopManager.CurrencyUpdated += OnCurrencyUpdated;
    }
    
    /// <summary>
    /// 刷新UI显示
    /// </summary>
    private void RefreshUI()
    {
        UpdatePlayerInfo();
        RefreshShopItems();
    }
    
    /// <summary>
    /// 更新玩家信息显示
    /// </summary>
    private void UpdatePlayerInfo()
    {
        if (_rebirthManager?.PlayerCurrency != null)
        {
            _currencyLabel.Text = $"投胎币: {_rebirthManager.PlayerCurrency.TotalCurrency}";
        }
        
        if (_rebirthManager?.PlayerData != null)
        {
            _playerLevelLabel.Text = $"等级: {_rebirthManager.PlayerData.PlayerLevel}";
        }
    }
    
    /// <summary>
    /// 刷新商店商品
    /// </summary>
    private void RefreshShopItems()
    {
        if (_rebirthManager?.ShopManager == null) return;
        
        // 清空所有网格
        ClearGrid(_allItemsGrid);
        ClearGrid(_upgradesGrid);
        ClearGrid(_unlocksGrid);
        ClearGrid(_boostsGrid);
        ClearGrid(_specialGrid);
        
        // 获取商品并分类显示
        var allItems = _rebirthManager.ShopManager.GetShopItems(ShopPageType.All);
        var upgrades = _rebirthManager.ShopManager.GetShopItems(ShopPageType.Upgrades);
        var unlocks = _rebirthManager.ShopManager.GetShopItems(ShopPageType.Unlocks);
        var boosts = _rebirthManager.ShopManager.GetShopItems(ShopPageType.Boosts);
        var special = _rebirthManager.ShopManager.GetShopItems(ShopPageType.Special);
        
        PopulateGrid(_allItemsGrid, allItems);
        PopulateGrid(_upgradesGrid, upgrades);
        PopulateGrid(_unlocksGrid, unlocks);
        PopulateGrid(_boostsGrid, boosts);
        PopulateGrid(_specialGrid, special);
    }
    
    /// <summary>
    /// 清空网格容器
    /// </summary>
    private void ClearGrid(GridContainer grid)
    {
        foreach (Node child in grid.GetChildren())
        {
            child.QueueFree();
        }
    }
    
    /// <summary>
    /// 填充网格容器
    /// </summary>
    private void PopulateGrid(GridContainer grid, List<ShopItem> items)
    {
        foreach (var item in items)
        {
            var itemCard = CreateShopItemCard(item);
            grid.AddChild(itemCard);
        }
    }
    
    /// <summary>
    /// 创建商店商品卡片
    /// </summary>
    private Control CreateShopItemCard(ShopItem item)
    {
        var card = new Panel();
        card.CustomMinimumSize = new Vector2(200, 250);
        card.AddThemeStyleboxOverride("panel", CreateCardStyleBox(item));
        
        var vbox = new VBoxContainer();
        card.AddChild(vbox);
        
        // 商品图标
        var icon = new TextureRect();
        icon.CustomMinimumSize = new Vector2(64, 64);
        icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
        icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        // 这里应该加载实际的图标纹理
        vbox.AddChild(icon);
        
        // 商品名称
        var nameLabel = new Label();
        nameLabel.Text = item.Name;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
        vbox.AddChild(nameLabel);
        
        // 商品描述
        var descLabel = new Label();
        descLabel.Text = item.Description;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.CustomMinimumSize = new Vector2(180, 60);
        descLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
        vbox.AddChild(descLabel);
        
        // 价格和稀有度
        var priceContainer = new HBoxContainer();
        vbox.AddChild(priceContainer);
        
        var priceLabel = new Label();
        priceLabel.Text = $"{item.GetActualPrice()}";
        priceLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        priceContainer.AddChild(priceLabel);
        
        var rarityLabel = new Label();
        rarityLabel.Text = GetRarityText(item.Rarity);
        rarityLabel.Modulate = item.GetRarityColor();
        priceContainer.AddChild(rarityLabel);
        
        // 购买按钮
        var buyButton = new Button();
        buyButton.Text = item.IsAvailable ? "购买" : "不可用";
        buyButton.Disabled = !item.IsAvailable;
        buyButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        
        // 连接购买按钮信号
        buyButton.Pressed += () => OnItemClicked(item);
        
        vbox.AddChild(buyButton);
        
        return card;
    }
    
    /// <summary>
    /// 创建卡片样式框
    /// </summary>
    private StyleBox CreateCardStyleBox(ShopItem item)
    {
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
        styleBox.BorderColor = item.GetRarityColor();
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.CornerRadiusTopLeft = 8;
        styleBox.CornerRadiusTopRight = 8;
        styleBox.CornerRadiusBottomLeft = 8;
        styleBox.CornerRadiusBottomRight = 8;
        
        return styleBox;
    }
    
    /// <summary>
    /// 获取稀有度文本
    /// </summary>
    private string GetRarityText(ShopItemRarity rarity)
    {
        return rarity switch
        {
            ShopItemRarity.Common => "普通",
            ShopItemRarity.Uncommon => "稀有",
            ShopItemRarity.Rare => "史诗",
            ShopItemRarity.Epic => "传说",
            ShopItemRarity.Legendary => "神话",
            _ => "未知"
        };
    }
    
    /// <summary>
    /// 商品点击事件
    /// </summary>
    private void OnItemClicked(ShopItem item)
    {
        if (!item.IsAvailable) return;
        
        _selectedItem = item;
        ShowPurchaseDialog(item);
    }
    
    /// <summary>
    /// 显示购买确认对话框
    /// </summary>
    private void ShowPurchaseDialog(ShopItem item)
    {
        _itemNameLabel.Text = item.Name;
        _itemDescriptionLabel.Text = item.Description;
        
        var actualPrice = item.GetActualPrice();
        _priceLabel.Text = $"价格: {actualPrice}";
        
        var currentCurrency = _rebirthManager.PlayerCurrency.TotalCurrency;
        var afterPurchase = currentCurrency - actualPrice;
        _currencyAfterLabel.Text = $"剩余: {afterPurchase}";
        _currencyAfterLabel.Modulate = afterPurchase >= 0 ? Colors.White : Colors.Red;
        
        _confirmButton.Disabled = !_rebirthManager.PlayerCurrency.HasEnoughCurrency(actualPrice);
        
        _purchaseDialog.PopupCentered();
    }
    
    /// <summary>
    /// 标签页切换事件
    /// </summary>
    private void OnTabChanged(long tab)
    {
        // 可以在这里添加标签页切换的特殊逻辑
        GD.Print($"切换到标签页: {tab}");
    }
    
    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void _on_close_button_pressed()
    {
        EmitSignal(SignalName.ShopClosed);
        Hide();
    }
    
    /// <summary>
    /// 刷新按钮点击事件
    /// </summary>
    private void _on_refresh_button_pressed()
    {
        if (_rebirthManager?.ShopManager != null)
        {
            _rebirthManager.ShopManager.RefreshShopAvailability();
        }
    }
    
    /// <summary>
    /// 统计信息按钮点击事件
    /// </summary>
    private void _on_statistics_button_pressed()
    {
        ShowStatisticsDialog();
    }
    
    /// <summary>
    /// 确认购买按钮点击事件
    /// </summary>
    private void _on_confirm_purchase_pressed()
    {
        if (_selectedItem != null && _rebirthManager?.ShopManager != null)
        {
            var success = _rebirthManager.ShopManager.PurchaseItem(_selectedItem.Id);
            if (success)
            {
                EmitSignal(SignalName.ItemPurchased, _selectedItem);
                _purchaseDialog.Hide();
                
                // 显示购买成功提示
                ShowPurchaseSuccessNotification(_selectedItem);
            }
            else
            {
                // 显示购买失败提示
                ShowPurchaseFailedNotification();
            }
        }
    }
    
    /// <summary>
    /// 显示统计信息对话框
    /// </summary>
    private void ShowStatisticsDialog()
    {
        if (_rebirthManager == null) return;
        
        var stats = _rebirthManager.GetStatisticsSummary();
        var statsText = FormatStatisticsText(stats);
        _statisticsLabel.Text = statsText;
        
        _statisticsDialog.PopupCentered();
    }
    
    /// <summary>
    /// 格式化统计信息文本
    /// </summary>
    private string FormatStatisticsText(Dictionary<string, object> stats)
    {
        var text = "[center][b]玩家统计[/b][/center]\n\n";
        
        text += "[b]基础信息[/b]\n";
        text += $"等级: {stats.GetValueOrDefault("player_level", 0)}\n";
        text += $"总游戏次数: {stats.GetValueOrDefault("total_games", 0)}\n";
        text += $"胜率: {stats.GetValueOrDefault("win_rate", 0):F1}%\n\n";
        
        text += "[b]游戏记录[/b]\n";
        text += $"最高层数: {stats.GetValueOrDefault("highest_floor", 0)}\n";
        text += $"平均游戏时长: {stats.GetValueOrDefault("average_game_duration", 0):F1}分钟\n";
        text += $"总游戏时间: {stats.GetValueOrDefault("total_play_time", 0):F1}分钟\n\n";
        
        text += "[b]货币统计[/b]\n";
        text += $"当前拥有: {stats.GetValueOrDefault("total_currency", 0)}\n\n";
        
        text += "[b]解锁进度[/b]\n";
        text += $"已解锁英雄: {stats.GetValueOrDefault("unlocked_heroes", 0)}\n";
        text += $"已解锁卡牌: {stats.GetValueOrDefault("unlocked_cards", 0)}\n";
        text += $"已解锁遗物: {stats.GetValueOrDefault("unlocked_relics", 0)}";
        
        return text;
    }
    
    /// <summary>
    /// 显示购买成功通知
    /// </summary>
    private void ShowPurchaseSuccessNotification(ShopItem item)
    {
        // 这里可以显示一个临时的成功提示
        GD.Print($"成功购买: {item.Name}");
        
        // 可以添加一个临时的通知UI
        var notification = new Label();
        notification.Text = $"成功购买 {item.Name}!";
        notification.Modulate = Colors.Green;
        notification.Position = new Vector2(GetViewportRect().Size.X / 2 - 100, 100);
        GetParent().AddChild(notification);
        
        // 2秒后移除通知
        var timer = GetTree().CreateTimer(2.0);
        timer.Timeout += () => notification.QueueFree();
    }
    
    /// <summary>
    /// 显示购买失败通知
    /// </summary>
    private void ShowPurchaseFailedNotification()
    {
        GD.Print("购买失败");
        
        var notification = new Label();
        notification.Text = "购买失败!";
        notification.Modulate = Colors.Red;
        notification.Position = new Vector2(GetViewportRect().Size.X / 2 - 50, 100);
        GetParent().AddChild(notification);
        
        var timer = GetTree().CreateTimer(2.0);
        timer.Timeout += () => notification.QueueFree();
    }
    
    /// <summary>
    /// 商店商品购买事件处理
    /// </summary>
    private void OnShopItemPurchased(ShopItem item, PurchaseRecord record)
    {
        RefreshUI();
    }
    
    /// <summary>
    /// 商店刷新事件处理
    /// </summary>
    private void OnShopRefreshed(Godot.Collections.Array items)
    {
        RefreshShopItems();
    }
    
    /// <summary>
    /// 货币更新事件处理
    /// </summary>
    private void OnCurrencyUpdated(RebirthCurrency currency)
    {
        UpdatePlayerInfo();
    }
    
    /// <summary>
    /// 显示商店
    /// </summary>
    public void ShowShop()
    {
        Show();
        RefreshUI();
    }
    
    /// <summary>
    /// 隐藏商店
    /// </summary>
    public void HideShop()
    {
        Hide();
    }
}