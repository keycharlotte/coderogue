using Godot;
using CodeRogue.Core;
using CodeRogue.Data;
using CodeRogue.Utils;
using Godot.Collections;
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
		
		private UnifiedDeck _currentDeck;
		private List<SkillCardUI> _cardUIs;
		
		public override void _Ready()
		{
			_cardUIs = new List<SkillCardUI>();
			ConnectSignals();
			LoadCurrentDeck();
		}
		
		private void ConnectSignals()
		{
			_closeButton.Pressed += OnCloseButtonPressed;
			// _shuffleButton.Pressed += OnShuffleButtonPressed;
			
			// 连接卡组管理器信号
	var deckManager = NodeUtils.GetDeckManager(this);
		if (deckManager != null)
		{
			deckManager.DeckChanged += OnDeckChanged;
			deckManager.CardAdded += OnCardAdded;
			deckManager.CardRemoved += OnCardRemoved;
		}
		}
		
		private void LoadCurrentDeck()
	{
		var deckManager = NodeUtils.GetDeckManager(this);
		if (deckManager == null)
		{
			GD.PrintErr("DeckManager autoload is null, retrying later...");
			CallDeferred("LoadCurrentDeck");
			return;
		}
		
		// 原有的加载逻辑
		_currentDeck = deckManager.GetCurrentDeck();
		UpdateDeckDisplay();
	}
		
		private void UpdateDeckDisplay()
		{
			if (_currentDeck == null) return;
			// 清除现有卡片UI
			ClearCardUIs();
			
			// 更新卡组信息
			_deckInfoLabel.Text = $"卡组大小: {_currentDeck.SkillCards.Count}/{_currentDeck.MaxDeckSize}";
			
			// 创建卡片UI
			foreach (var card in _currentDeck.SkillCards)
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
			// CustomMinimumSize应在SkillCardUI.tscn场景文件中设置
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
			AddStatLabel($"总卡片数: {_currentDeck.SkillCards.Count}");
			AddStatLabel($"攻击技能: {_currentDeck.SkillCards.Count(c => c.SkillType == SkillType.Attack)}");
			AddStatLabel($"防御技能: {_currentDeck.SkillCards.Count(c => c.SkillType == SkillType.Defense)}");
			AddStatLabel($"辅助技能: {_currentDeck.SkillCards.Count(c => c.SkillType == SkillType.Utility)}");
			AddStatLabel($"平均消耗: {(_currentDeck.SkillCards.Count > 0 ? _currentDeck.SkillCards.Average(c => c.Cost):0):F1}");
			
			// 稀有度统计
			var rarityStats = new Godot.Collections.Dictionary<CardRarity, int>();
			foreach (var card in _currentDeck.SkillCards)
			{
				if (rarityStats.ContainsKey(card.Rarity))
				rarityStats[card.Rarity]++;
			else
				rarityStats[card.Rarity] = 1;
			}
			
			AddStatLabel("\n稀有度分布:");
			foreach (var rarity in System.Enum.GetValues<CardRarity>())
			{
				if (rarityStats.ContainsKey(rarity))
				{
					AddStatLabel($"{rarity}: {rarityStats[rarity]}");
				}
			}
		}
		
		private void AddStatLabel(string text)
		{
			// UI组件应在.tscn文件中预定义，这里只处理数据绑定
			// 如果需要动态显示统计信息，应在场景文件中预先创建足够的Label组件
			if (_statsContainer != null && _statsContainer.GetChildCount() > 0)
			{
				// 查找可用的Label组件并设置文本
				foreach (Node child in _statsContainer.GetChildren())
				{
					if (child is Label label && string.IsNullOrEmpty(label.Text))
					{
						label.Text = text;
						break;
					}
				}
			}
		}
		
		private void OnCardClicked(SkillCard card)
		{
			// 显示卡片详细信息或执行其他操作
			GD.Print($"点击了卡片: {card.Name}");
		}
		
		private void OnCardRemoveRequested(SkillCard card)
	{
		var deckManager = NodeUtils.GetDeckManager(this);
		deckManager?.RemoveCardFromDeck(card);
	}
		
		private void OnDeckChanged(UnifiedDeck newDeck)
		{
			_currentDeck = newDeck;
			UpdateDeckDisplay();
		}
		
		private void OnCardAdded(BaseCard card)
		{
			UpdateDeckDisplay();
		}
		
		private void OnCardRemoved(BaseCard card)
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
