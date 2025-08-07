using Godot;
using CodeRogue.Core;
using CodeRogue.Player;
using CodeRogue.Utils;
using System;

namespace CodeRogue.UI
{
	/// <summary>
	/// 游戏内UI - 显示玩家状态和游戏信息
	/// </summary>
	public partial class GameUI : Control
	{
		private ProgressBar _healthBar;
		private Label _healthLabel;
		private Label _levelLabel;
		private Label _scoreLabel;
		private Button _pauseButton;
		private Button _deckButton;
		private PlayerController _player;
		
		public override void _Ready()
		{
			InitializeUI();
			ConnectSignals();
			FindPlayer();
		}
		
		private void InitializeUI()
		{
			// 获取UI节点
			_healthBar = GetNode<ProgressBar>("BG/HBoxContainer/HealthContainer/HealthBar");
			_healthLabel = GetNode<Label>("BG/HBoxContainer/HealthContainer/HealthLabel");
			_levelLabel = GetNode<Label>("BG/HBoxContainer/InfoContainer/LevelLabel");
			_scoreLabel = GetNode<Label>("BG/HBoxContainer/InfoContainer/ScoreLabel");
			_pauseButton = GetNode<Button>("BG/PauseButton");
			_deckButton = GetNode<Button>("BG/HBoxContainer/DeckButton");
		}
		
		private void ConnectSignals()
		{
			if (_pauseButton != null)
			{
				_pauseButton.Pressed += OnPauseButtonPressed;
			}
			if (_deckButton != null)
			{
				_deckButton.Pressed += OnDeckButtonPressed;
			}
		}

		private void OnDeckButtonPressed()
	{
		var deckManager = NodeUtils.GetDeckManager(this);
		deckManager?.OpenDeckUI();
	}

		private void FindPlayer()
		{
			// 查找玩家节点
			_player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
			
			if (_player != null)
			{
				_player.HealthChanged += OnPlayerHealthChanged;
			}
		}
		
		private void OnPlayerHealthChanged(int newHealth, int maxHealth)
		{
			UpdateHealthDisplay(newHealth, maxHealth);
		}
		
		private void UpdateHealthDisplay(int currentHealth, int maxHealth)
		{
			if (_healthBar != null)
			{
				_healthBar.MaxValue = maxHealth;
				_healthBar.Value = currentHealth;
			}
			
			if (_healthLabel != null)
			{
				_healthLabel.Text = $"{currentHealth}/{maxHealth}";
			}
		}
		
		public void UpdateLevel(int level)
		{
			if (_levelLabel != null)
			{
				_levelLabel.Text = $"Level: {level}";
			}
		}
		
		public void UpdateScore(int score)
		{
			if (_scoreLabel != null)
			{
				_scoreLabel.Text = $"Score: {score}";
			}
		}
		
		private void OnPauseButtonPressed()
	{
		var gameManager = NodeUtils.GetGameManager(this);
		gameManager?.PauseGame();
	}
		
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				if (keyEvent.Keycode == Key.Escape)
				{
					OnPauseButtonPressed();
				}
			}
		}
	}
}
