using Godot;
using CodeRogue.Core;

namespace CodeRogue.UI
{
	/// <summary>
	/// 输入显示UI - 显示玩家当前输入的文字
	/// </summary>
	public partial class InputDisplay : Control
	{
		[Export] private Label _inputLabel;
		[Export] private Panel _inputPanel;
		
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
			// UI组件应在.tscn文件中预定义，这里只处理初始化逻辑
			if (_inputLabel != null)
			{
				_inputLabel.Text = "输入: ";
			}
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
