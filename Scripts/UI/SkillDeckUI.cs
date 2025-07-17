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
		[Export] private GridContainer _cardsGrid;
		[Export] private Label _deckInfoLabel;
		[Export] private Button _closeButton;
		// [Export] private Button _shuffleButton;
		[Export] private ScrollContainer _scrollContainer;
		[Export] private VBoxContainer _statsContainer;
		
		private SkillDeck _currentDeck;
		private List<SkillCardUI> _cardUIs;
		
		public override void _Ready()
		{
			ConnectSignals();
			LoadCurrentDeck();
			_cardUIs = new List<SkillCardUI>();
		}
		
		private void ConnectSignals()
		{
			_closeButton.Pressed += OnCloseButtonPressed;
			// _shuffleButton.Pressed += OnShuffleButtonPressed;
			
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
			if (SkillDeckManager.Instance == null)
			{
				GD.PrintErr("SkillDeckManager.Instance is null, retrying later...");
				CallDeferred("LoadCurrentDeck");
				return;
			}
			
			// 原有的加载逻辑
			_currentDeck = SkillDeckManager.Instance.GetCurrentDeck();
			UpdateDeckDisplay();
		}
		
		private void UpdateDeckDisplay()
		{
			if (_currentDeck == null) return;
			GD.Print($"UpdateDeckDisplay: {_currentDeck._drawPile}");
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
			// 加载SkillCardUI场景
			var cardScene = GD.Load<PackedScene>("res://Scenes/UI/SkillCardUI.tscn");
			var cardUI = cardScene.Instantiate<SkillCardUI>();
			cardUI.CustomMinimumSize = new Vector2(200, 280);
			cardUI.CardClicked += OnCardClicked;
			cardUI.CardRemoveRequested += OnCardRemoveRequested;
			
			_cardsGrid.AddChild(cardUI); // 先添加到场景树
			_cardUIs.Add(cardUI);
			
			// 延迟设置技能卡数据，确保节点已完全初始化
			cardUI.CallDeferred("SetSkillCard", card);
		}
		
		private void ClearCardUIs()
		{
			if (_cardUIs == null) return;
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
