using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRogue.Core
{
/// <summary>
	/// 输入管理器 - 处理玩家的文字输入
	/// </summary>
	public partial class InputManager : Node
	{
		[Signal]
		public delegate void InputChangedEventHandler(string currentInput);
		
		[Signal]
		public delegate void WordMatchedEventHandler(string word, EnemyController enemy);
		
		public static InputManager Instance { get; private set; }
		
		private string _currentInput = "";
		private bool _isInputActive = true;
		private List<EnemyController> _enemies = new List<EnemyController>();
		
		public string CurrentInput => _currentInput;
		public bool IsInputActive 
		{ 
			get => _isInputActive; 
			set => _isInputActive = value; 
		}
		
		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				QueueFree();
			}
		}
		
		public override void _Input(InputEvent @event)
		{
			if (!_isInputActive) return;
			
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				HandleKeyInput(keyEvent);
			}
		}
		
		private void HandleKeyInput(InputEventKey keyEvent)
		{
			switch (keyEvent.Keycode)
			{		
				default:
					// 处理字母输入
					if (IsValidInputKey(keyEvent.Keycode))
					{
						char inputChar = GetCharFromKeycode(keyEvent.Keycode);
						_currentInput += inputChar;
						OnInputChanged();
					}
					break;
			}
		}
		
		private bool IsValidInputKey(Key keycode)
		{
			return keycode >= Key.A && keycode <= Key.Z;
		}
		
		private char GetCharFromKeycode(Key keycode)
		{
			return (char)('a' + (keycode - Key.A));
		}
		
		private void OnInputChanged()
		{
			EmitSignal(SignalName.InputChanged, _currentInput);
			
			// 检查是否有完全匹配的敌人
			CheckForCompleteMatch();
			
			// 更新所有敌人的视觉反馈
			bool Res = UpdateEnemyVisualFeedback();
			if (Res)
			{
				ClearInput("NoOneMatched");
			}
		}
		
		private void CheckForCompleteMatch()
		{
			if (string.IsNullOrEmpty(_currentInput)) return;
			
			foreach (var enemy in _enemies)
			{
				if (enemy != null && enemy.IsAlive)
				{
					var enemyView = enemy.GetEnemyView();
					if (enemyView != null)
					{
						string targetWord = enemyView.CurrentWord;
						if (string.Equals(_currentInput.ToLower(), targetWord.ToLower(), StringComparison.OrdinalIgnoreCase))
						{
							// 找到完全匹配，发射攻击信号
							EmitSignal(SignalName.WordMatched, targetWord, enemy);
							ClearInput("SomeOneMatched");
							return; // 只攻击第一个匹配的敌人
						}
					}
				}
			}
		}
		
		private bool UpdateEnemyVisualFeedback()
		{
			bool Res = false;
			foreach (var enemy in _enemies)
			{
				if (enemy != null && enemy.IsAlive)
				{
					var enemyView = enemy.GetEnemyView();
					Res = enemyView.UpdateWordHighlight(_currentInput);
				}
			}
			return Res;
		}
		
		public void RegisterEnemy(EnemyController enemy)
		{
			if (!_enemies.Contains(enemy))
			{
				_enemies.Add(enemy);
			}
		}
		
		public void UnregisterEnemy(EnemyController enemy)
		{
			_enemies.Remove(enemy);
		}
		
		public void ClearInput(String Reason)
		{
			_currentInput = "";
			EmitSignal(SignalName.InputChanged, _currentInput);
			UpdateEnemyVisualFeedback();
			GD.Print($"ClearInput Reason{Reason}");
		}
	}
}
