using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CodeRogue.UI
{
	/// <summary>
	/// 技能卡组UI - 显示和管理技能卡组
	/// </summary>
	public partial class SkillDeckUI : Control
	{
		private GridContainer _cardsGrid;
		private Label _deckInfoLabel;
		private Button _closeButton;
		private Button _shuffleButton;
		private ScrollContainer _scrollContainer;
		private VBoxContainer _statsContainer;
		
		private SkillDeck _currentDeck;
		private List<SkillCardUI> _cardUIs;
		
		public override void _Ready()
		{
			InitializeUI();
			ConnectSignals();
			LoadCurrentDeck();
		}
		
		private void InitializeUI()
		{
			// 设置主面板
			// SetAnchorsAndOffsetsPreset(PresetMode.FullRect);
			
			// 创建背景面板
			var backgroundPanel = new Panel();
			// backgroundPanel.SetAnchorsAndOffsetsPreset(PresetMode.FullRect);
			var bgStyle = new StyleBoxFlat();
			bgStyle.BgColor = new Color(0, 0, 0, 0.8f);
			backgroundPanel.AddThemeStyleboxOverride("panel", bgStyle);
			AddChild(backgroundPanel);
			
			// 创建主容器
			var mainContainer = new VBoxContainer();
			// mainContainer.SetAnchorsAndOffsetsPreset(PresetMode.CenterWide);
			mainContainer.CustomMinimumSize = new Vector2(800, 600);
			mainContainer.AddThemeConstantOverride("separation", 10);
			AddChild(mainContainer);
			var theme = GD.Load<Theme>("res://ResourcesThemes/GameUITheme.tres");
			theme.SetFontSize("font_size", "Label", 18);
			// 标题栏
			var titleContainer = new HBoxContainer();
			mainContainer.AddChild(titleContainer);
			
			var titleLabel = new Label();
			titleLabel.Text = "技能卡组";
			titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			titleContainer.AddChild(titleLabel);
			
			_closeButton = new Button();
			_closeButton.Text = "关闭";
			_closeButton.CustomMinimumSize = new Vector2(80, 40);
			titleContainer.AddChild(_closeButton);
			
			// 卡组信息
			_deckInfoLabel = new Label();
			_deckInfoLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
			mainContainer.AddChild(_deckInfoLabel);
			
			// 操作按钮
			var buttonContainer = new HBoxContainer();
			mainContainer.AddChild(buttonContainer);
			
			_shuffleButton = new Button();
			_shuffleButton.Text = "洗牌";
			_shuffleButton.CustomMinimumSize = new Vector2(80, 35);
			buttonContainer.AddChild(_shuffleButton);
			
			// 主内容区域
			var contentContainer = new HBoxContainer();
			contentContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			mainContainer.AddChild(contentContainer);
			
			// 卡片滚动区域
			_scrollContainer = new ScrollContainer();
			_scrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			_scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			contentContainer.AddChild(_scrollContainer);
			
			_cardsGrid = new GridContainer();
			_cardsGrid.Columns = 3;
			_cardsGrid.AddThemeConstantOverride("h_separation", 10);
			_cardsGrid.AddThemeConstantOverride("v_separation", 10);
			_scrollContainer.AddChild(_cardsGrid);
			
			// 统计信息面板
			_statsContainer = new VBoxContainer();
			_statsContainer.CustomMinimumSize = new Vector2(200, 0);
			contentContainer.AddChild(_statsContainer);
			
			var statsTitle = new Label();
			statsTitle.Text = "卡组统计";
			_statsContainer.AddChild(statsTitle);
			
			_cardUIs = new List<SkillCardUI>();
		}
		
		private void ConnectSignals()
		{
			_closeButton.Pressed += OnCloseButtonPressed;
			_shuffleButton.Pressed += OnShuffleButtonPressed;
			
			// 连接卡组管理器信号
			if (SkillDeckManager.Instance != null)
			{
				SkillDeckManager.Instance.DeckChanged += OnDeckChanged;
				SkillDeckManager.Instance.CardAdded += OnCardAdded;
				SkillDeckManager.Instance.CardRemoved += OnCardRemoved;
			}
		}
		
		private void LoadCurrentDeck()
		{
			if (SkillDeckManager.Instance != null)
			{
				_currentDeck = SkillDeckManager.Instance.GetCurrentDeck();
				UpdateDeckDisplay();
			}
		}
		
		private void UpdateDeckDisplay()
		{
			if (_currentDeck == null) return;
			
			// 清除现有卡片UI
			ClearCardUIs();
			
			// 更新卡组信息
			_deckInfoLabel.Text = $"卡组大小: {_currentDeck.Cards.Count}/{_currentDeck.MaxDeckSize}";
			
			// 创建卡片UI
			foreach (var card in _currentDeck.Cards)
			{
				CreateCardUI(card);
			}
			
			// 更新统计信息
			UpdateStatsDisplay();
		}
		
		private void CreateCardUI(SkillCard card)
		{
			var cardUI = new SkillCardUI();
			cardUI.CustomMinimumSize = new Vector2(200, 280);
			cardUI.SetSkillCard(card);
			cardUI.CardClicked += OnCardClicked;
			cardUI.CardRemoved += OnCardRemoveRequested;
			
			_cardsGrid.AddChild(cardUI);
			_cardUIs.Add(cardUI);
		}
		
		private void ClearCardUIs()
		{
			foreach (var cardUI in _cardUIs)
			{
				cardUI.QueueFree();
			}
			_cardUIs.Clear();
		}
		
		private void UpdateStatsDisplay()
		{
			// 清除现有统计信息
			foreach (Node child in _statsContainer.GetChildren())
			{
				if (child.Name != "统计标题")
				{
					child.QueueFree();
				}
			}
			
			if (_currentDeck == null) return;
			
			// 添加统计信息
			AddStatLabel($"总卡片数: {_currentDeck.Cards.Count}");
			AddStatLabel($"攻击技能: {_currentDeck.Cards.Count(c => c.Type == SkillType.Attack)}");
			AddStatLabel($"防御技能: {_currentDeck.Cards.Count(c => c.Type == SkillType.Defense)}");
			AddStatLabel($"辅助技能: {_currentDeck.Cards.Count(c => c.Type == SkillType.Utility)}");
			AddStatLabel($"平均消耗: {(_currentDeck.Cards.Count > 0 ? _currentDeck.Cards.Average(c => c.ChargeCost):0):F1}");
			
			// 稀有度统计
			var rarityStats = _currentDeck.Cards.GroupBy(c => c.Rarity)
				.ToDictionary(g => g.Key, g => g.Count());
			
			AddStatLabel("\n稀有度分布:");
			foreach (var rarity in System.Enum.GetValues<SkillRarity>())
			{
				var count = rarityStats.GetValueOrDefault(rarity, 0);
				if (count > 0)
				{
					AddStatLabel($"{rarity}: {count}");
				}
			}
		}
		
		private void AddStatLabel(string text)
		{
			var label = new Label();
			label.Text = text;
			label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
			_statsContainer.AddChild(label);
		}
		
		private void OnCardClicked(SkillCard card)
		{
			// 显示卡片详细信息或执行其他操作
			GD.Print($"点击了卡片: {card.Name}");
		}
		
		private void OnCardRemoveRequested(SkillCard card)
		{
			SkillDeckManager.Instance?.RemoveCardFromDeck(card);
		}
		
		private void OnDeckChanged(SkillDeck newDeck)
		{
			_currentDeck = newDeck;
			UpdateDeckDisplay();
		}
		
		private void OnCardAdded(SkillCard card)
		{
			UpdateDeckDisplay();
		}
		
		private void OnCardRemoved(SkillCard card)
		{
			UpdateDeckDisplay();
		}
		
		private void OnCloseButtonPressed()
		{
			QueueFree();
		}
		
		private void OnShuffleButtonPressed()
		{
			_currentDeck?.Initialize(); // 重新洗牌
			GD.Print("卡组已洗牌");
		}
		
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				if (keyEvent.Keycode == Key.Escape)
				{
					OnCloseButtonPressed();
				}
			}
		}
	}
}
