using Godot;
using CodeRogue.Core;

namespace CodeRogue.UI
{
	/// <summary>
	/// 输入显示UI - 显示玩家当前输入的文字
	/// </summary>
	public partial class InputDisplay : Control
	{
		private Label _inputLabel;
		private Panel _inputPanel;
		
		public override void _Ready()
		{
			SetupUI();
			
			// 连接输入管理器信号
			var inputManager = GetNode<InputManager>("/root/InputManager");
		if (inputManager != null)
		{
			inputManager.InputChanged += OnInputChanged;
			}
		}
		
		private void SetupUI()
		{
			_inputPanel = new Panel();
			_inputPanel.Position = new Vector2(10, 10);
			_inputPanel.Size = new Vector2(200, 40);
			AddChild(_inputPanel);
			
			_inputLabel = new Label();
			_inputLabel.Position = new Vector2(10, 10);
			_inputLabel.Size = new Vector2(180, 20);
			_inputLabel.Text = "输入: ";
			_inputPanel.AddChild(_inputLabel);
		}
		
		private void OnInputChanged(string currentInput)
		{
			if (_inputLabel != null)
			{
				_inputLabel.Text = $"输入: {currentInput}";
			}
		}
		
		public override void _ExitTree()
		{
			var inputManager = GetNode<InputManager>("/root/InputManager");
		if (inputManager != null)
		{
			inputManager.InputChanged -= OnInputChanged;
			}
		}
	}
}
