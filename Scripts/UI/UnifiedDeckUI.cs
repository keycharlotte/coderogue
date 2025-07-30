using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CodeRogue.UI
{
	/// <summary>
	/// 统一卡组UI - 显示和管理包含怪物卡和技能卡的统一卡组
	/// </summary>
	public partial class UnifiedDeckUI : Control
	{
		[Export] private TabContainer _tabContainer;
		[Export] private GridContainer _monsterCardsGrid;
		[Export] private GridContainer _skillCardsGrid;
		[Export] private Label _deckInfoLabel;
		[Export] private Button _closeButton;
		[Export] private Button _shuffleButton;
		[Export] private ScrollContainer _monsterScrollContainer;
		[Export] private ScrollContainer _skillScrollContainer;
		[Export] private VBoxContainer _statsContainer;
		[Export] private OptionButton _deckSelector;
		[Export] private Button _newDeckButton;
		[Export] private Button _deleteDeckButton;
		[Export] private Button _duplicateDeckButton;
		
		private UnifiedDeck _currentDeck;
		private List<SkillCardUI> _skillCardUIs;
		private List<MonsterCardUI> _monsterCardUIs;
		private DeckManager _deckManager;
		
		public override void _Ready()
		{
			_skillCardUIs = new List<SkillCardUI>();
			_monsterCardUIs = new List<MonsterCardUI>();
			ConnectSignals();
			LoadDeckManager();
			UpdateDeckSelector();
			LoadCurrentDeck();
		}
		
		private void ConnectSignals()
		{
			_closeButton.Pressed += OnCloseButtonPressed;
			_shuffleButton.Pressed += OnShuffleButtonPressed;
			_newDeckButton.Pressed += OnNewDeckButtonPressed;
			_deleteDeckButton.Pressed += OnDeleteDeckButtonPressed;
			_duplicateDeckButton.Pressed += OnDuplicateDeckButtonPressed;
			_deckSelector.ItemSelected += OnDeckSelected;
		}
		
		private void LoadDeckManager()
		{
			_deckManager = GetNode<DeckManager>("/root/DeckManager");
			if (_deckManager != null)
			{
				_deckManager.DeckChanged += OnDeckChanged;
				_deckManager.CardAdded += OnCardAdded;
				_deckManager.CardRemoved += OnCardRemoved;
			}
			else
			{
				GD.PrintErr("UnifiedDeckUI: 无法找到DeckManager");
			}
		}
		
		private void UpdateDeckSelector()
		{
			if (_deckManager == null) return;
			
			_deckSelector.Clear();
			foreach (var deck in _deckManager.SavedDecks)
			{
				_deckSelector.AddItem(deck.DeckName);
			}
			
			// 选择当前卡组
			if (_deckManager.CurrentDeck != null)
			{
				for (int i = 0; i < _deckManager.SavedDecks.Count; i++)
				{
					if (_deckManager.SavedDecks[i] == _deckManager.CurrentDeck)
					{
						_deckSelector.Selected = i;
						break;
					}
				}
			}
		}
		
		private void LoadCurrentDeck()
		{
			if (_deckManager != null)
			{
				_currentDeck = _deckManager.GetCurrentDeck();
				UpdateDeckDisplay();
			}
		}
		
		private void UpdateDeckDisplay()
		{
			if (_currentDeck == null) return;
			
			// 清除现有卡片UI
			ClearCardUIs();
			
			// 更新卡组信息
			_deckInfoLabel.Text = $"卡组: {_currentDeck.DeckName} ({_currentDeck.Cards.Count}/{_currentDeck.MaxDeckSize})";
			
			// 创建怪物卡片UI
			foreach (var card in _currentDeck.MonsterCards)
			{
				CreateMonsterCardUI(card);
			}
			
			// 创建技能卡片UI
			foreach (var card in _currentDeck.SkillCards)
			{
				CreateSkillCardUI(card);
			}
			
			// 更新统计信息
			UpdateStatsDisplay();
		}
		
		private void CreateMonsterCardUI(MonsterCard card)
		{
			// 这里需要MonsterCardUI的实现，暂时跳过
			// var cardUI = GD.Load<PackedScene>("res://Scenes/UI/MonsterCardUI.tscn").Instantiate<MonsterCardUI>();
			// cardUI.SetMonsterCard(card);
			// cardUI.CardClicked += OnMonsterCardClicked;
			// cardUI.CardRemoveRequested += OnMonsterCardRemoveRequested;
			// _monsterCardsGrid.AddChild(cardUI);
			// _monsterCardUIs.Add(cardUI);
		}
		
		private void CreateSkillCardUI(SkillCard card)
		{
			var cardUIScene = GD.Load<PackedScene>("res://Scenes/UI/SkillCardUI.tscn");
			if (cardUIScene != null)
			{
				var cardUI = cardUIScene.Instantiate<SkillCardUI>();
				cardUI.SetSkillCard(card);
				cardUI.CardClicked += OnSkillCardClicked;
				cardUI.CardRemoveRequested += OnSkillCardRemoveRequested;
				_skillCardsGrid.AddChild(cardUI);
				_skillCardUIs.Add(cardUI);
			}
		}
		
		private void ClearCardUIs()
		{
			// 清除技能卡UI
			if (_skillCardUIs != null)
			{
				foreach (var cardUI in _skillCardUIs)
				{
					cardUI.QueueFree();
				}
				_skillCardUIs.Clear();
			}
			
			// 清除怪物卡UI
			if (_monsterCardUIs != null)
			{
				foreach (var cardUI in _monsterCardUIs)
				{
					cardUI.QueueFree();
				}
				_monsterCardUIs.Clear();
			}
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
			
			// 基础统计
			AddStatLabel($"总卡片数: {_currentDeck.Cards.Count}");
			AddStatLabel($"怪物卡: {_currentDeck.MonsterCards.Count}");
			AddStatLabel($"技能卡: {_currentDeck.SkillCards.Count}");
			
			// 技能统计
			if (_currentDeck.SkillCards.Count > 0)
			{
				AddStatLabel($"\n技能类型分布:");
				AddStatLabel($"攻击技能: {_currentDeck.SkillCards.Count(c => c.SkillType == SkillType.Attack)}");
				AddStatLabel($"防御技能: {_currentDeck.SkillCards.Count(c => c.SkillType == SkillType.Defense)}");
				AddStatLabel($"辅助技能: {_currentDeck.SkillCards.Count(c => c.SkillType == SkillType.Utility)}");
				AddStatLabel($"平均技能消耗: {_currentDeck.SkillCards.Average(c => c.Cost):F1}");
			}
			
			// 怪物统计
			if (_currentDeck.MonsterCards.Count > 0)
			{
				AddStatLabel($"\n怪物种族分布:");
				var raceStats = _currentDeck.GetMonsterRaceDistribution();
				foreach (var race in System.Enum.GetValues<MonsterRace>())
				{
					if (raceStats.ContainsKey(race) && raceStats[race] > 0)
					{
						AddStatLabel($"{race}: {raceStats[race]}");
					}
				}
				AddStatLabel($"平均怪物消耗: {_currentDeck.MonsterCards.Average(c => c.Cost):F1}");
			}
			
			// 稀有度统计
			var rarityStats = new Godot.Collections.Dictionary<CardRarity, int>();
			foreach (var card in _currentDeck.Cards)
			{
				if (rarityStats.ContainsKey(card.Rarity))
					rarityStats[card.Rarity]++;
				else
					rarityStats[card.Rarity] = 1;
			}
			
			AddStatLabel($"\n稀有度分布:");
			foreach (var rarity in System.Enum.GetValues<CardRarity>())
			{
				if (rarityStats.ContainsKey(rarity))
				{
					AddStatLabel($"{rarity}: {rarityStats[rarity]}");
				}
			}
			
			// 卡组评分
			AddStatLabel($"\n卡组评分: {_currentDeck.CalculateDeckScore():F1}");
		}
		
		private void AddStatLabel(string text)
		{
			// UI组件应在.tscn文件中预定义，这里通过遍历子节点来更新预定义Label的文本
			foreach (Node child in _statsContainer.GetChildren())
			{
				if (child is Label label && string.IsNullOrEmpty(label.Text))
				{
					label.Text = text;
					break;
				}
			}
		}
		
		// 事件处理
		private void OnSkillCardClicked(SkillCard card)
		{
			GD.Print($"技能卡点击: {card.Name}");
		}
		
		private void OnSkillCardRemoveRequested(SkillCard card)
		{
			_deckManager?.RemoveCardFromDeck(card);
		}
		
		private void OnMonsterCardClicked(MonsterCard card)
		{
			GD.Print($"怪物卡点击: {card.MonsterName}");
		}
		
		private void OnMonsterCardRemoveRequested(MonsterCard card)
		{
			_deckManager?.RemoveCardFromDeck(card);
		}
		
		private void OnDeckChanged(UnifiedDeck newDeck)
		{
			_currentDeck = newDeck;
			UpdateDeckDisplay();
			UpdateDeckSelector();
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
		
		private void OnNewDeckButtonPressed()
		{
			// 创建新卡组对话框
			var dialog = new AcceptDialog();
			dialog.Title = "创建新卡组";
			dialog.DialogText = "请输入新卡组名称:";
			
			var lineEdit = new LineEdit();
			lineEdit.PlaceholderText = "新卡组名称";
			dialog.AddChild(lineEdit);
			
			dialog.Confirmed += () => {
				var deckName = lineEdit.Text.Trim();
				if (!string.IsNullOrEmpty(deckName))
				{
					var newDeck = _deckManager?.CreateNewDeck(deckName);
					if (newDeck != null)
					{
						_deckManager.SetCurrentDeck(newDeck);
						GD.Print($"创建新卡组: {deckName}");
					}
					else
					{
						GD.PrintErr($"无法创建卡组: {deckName} (可能名称重复)");
					}
				}
				dialog.QueueFree();
			};
			
			GetTree().CurrentScene.AddChild(dialog);
			dialog.PopupCentered();
		}
		
		private void OnDeleteDeckButtonPressed()
		{
			if (_currentDeck == null) return;
			
			var dialog = new ConfirmationDialog();
			dialog.DialogText = $"确定要删除卡组 '{_currentDeck.DeckName}' 吗？";
			dialog.Confirmed += () => {
				_deckManager?.RemoveDeck(_currentDeck.DeckName);
				GD.Print($"删除卡组: {_currentDeck.DeckName}");
				dialog.QueueFree();
			};
			
			GetTree().CurrentScene.AddChild(dialog);
			dialog.PopupCentered();
		}
		
		private void OnDuplicateDeckButtonPressed()
		{
			if (_currentDeck == null) return;
			
			var dialog = new AcceptDialog();
			dialog.Title = "复制卡组";
			dialog.DialogText = "请输入复制卡组的名称:";
			
			var lineEdit = new LineEdit();
			lineEdit.PlaceholderText = $"{_currentDeck.DeckName} (副本)";
			lineEdit.Text = $"{_currentDeck.DeckName} (副本)";
			dialog.AddChild(lineEdit);
			
			dialog.Confirmed += () => {
				var newDeckName = lineEdit.Text.Trim();
				if (!string.IsNullOrEmpty(newDeckName))
				{
					var duplicatedDeck = _deckManager?.DuplicateDeck(_currentDeck.DeckName, newDeckName);
					if (duplicatedDeck != null)
					{
						_deckManager.SetCurrentDeck(duplicatedDeck);
						GD.Print($"复制卡组: {newDeckName}");
					}
					else
					{
						GD.PrintErr($"无法复制卡组: {newDeckName} (可能名称重复)");
					}
				}
				dialog.QueueFree();
			};
			
			GetTree().CurrentScene.AddChild(dialog);
			dialog.PopupCentered();
		}
		
		private void OnDeckSelected(long index)
		{
			if (_deckManager != null && index >= 0 && index < _deckManager.SavedDecks.Count)
			{
				var selectedDeck = _deckManager.SavedDecks[(int)index];
				_deckManager.SetCurrentDeck(selectedDeck);
			}
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
		
		public override void _ExitTree()
		{
			// 清理信号连接
			if (_deckManager != null)
			{
				_deckManager.DeckChanged -= OnDeckChanged;
				_deckManager.CardAdded -= OnCardAdded;
				_deckManager.CardRemoved -= OnCardRemoved;
			}
		}
	}
	
	// 临时的MonsterCardUI类定义，直到实际实现
	public partial class MonsterCardUI : Control
	{
		public void SetMonsterCard(MonsterCard card) { }
		public void QueueFree() { base.QueueFree(); }
	}
}