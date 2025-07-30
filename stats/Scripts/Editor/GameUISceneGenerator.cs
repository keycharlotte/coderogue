#if TOOLS
using Godot;

namespace CodeRogue.Editor
{
	/// <summary>
	/// 游戏UI场景生成器 - 自动创建游戏内UI场景
	/// </summary>
	[Tool]
	public partial class GameUISceneGenerator : EditorScript
	{
		public override void _Run()
		{
			CreateGameUIScene();
			GD.Print("GameUISceneGenerator 运行完成");
		}
		
		private void CreateGameUIScene()
		{
			var scene = new PackedScene();
			var gameUI = new Control();
			gameUI.Name = "GameUI";
			
			// 设置为全屏
			gameUI.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			
			// 添加GameUI脚本
			var gameUIScript = GD.Load<CSharpScript>("res://Scripts/UI/GameUI.cs");
			gameUI.SetScript(gameUIScript);
			
			// 创建主要容器
			var hboxContainer = new HBoxContainer();
			hboxContainer.Name = "HBoxContainer";
			hboxContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);
			hboxContainer.OffsetBottom = 80;
			hboxContainer.AddThemeConstantOverride("separation", 20);
			
			gameUI.AddChild(hboxContainer);
			hboxContainer.Owner = gameUI;
			
			// 创建健康容器
			var healthContainer = CreateHealthContainer();
			hboxContainer.AddChild(healthContainer);
			healthContainer.Owner = gameUI;
			
			// 创建信息容器
			var infoContainer = CreateInfoContainer();
			hboxContainer.AddChild(infoContainer);
			infoContainer.Owner = gameUI;
			
			// 创建暂停按钮
			var pauseButton = CreatePauseButton();
			gameUI.AddChild(pauseButton);
			pauseButton.Owner = gameUI;
			
			// 最后统一设置所有子节点的 Owner
			SetOwnerRecursively(gameUI, gameUI);
			
			// 确保目录存在
			if (!DirAccess.DirExistsAbsolute("res://Scenes/UI"))
			{
				DirAccess.MakeDirRecursiveAbsolute("res://Scenes/UI");
			}
			
			// 保存场景
			scene.Pack(gameUI);
			var saveResult = ResourceSaver.Save(scene, "res://Scenes/UI/GameUI.tscn");
			
			if (saveResult == Error.Ok)
			{
				GD.Print("GameUI.tscn 创建完成");
			}
			else
			{
				GD.PrintErr($"保存 GameUI.tscn 失败: {saveResult}");
			}
		}
		
		private VBoxContainer CreateHealthContainer()
		{
			var healthContainer = new VBoxContainer();
			healthContainer.Name = "HealthContainer";
			healthContainer.CustomMinimumSize = new Vector2(200, 60);
			
			// 健康标签
			var healthLabel = new Label();
			healthLabel.Name = "HealthLabel";
			healthLabel.Text = "100/100";
			healthLabel.HorizontalAlignment = HorizontalAlignment.Center;
			
			// 设置标签样式
			var labelTheme = new Theme();
			var labelFont = new SystemFont();
			labelFont.FontNames = new[] { "Arial" };
			labelTheme.SetFont("font", "Label", labelFont);
			labelTheme.SetFontSize("font_size", "Label", 16);
			labelTheme.SetColor("font_color", "Label", Colors.White);
			healthLabel.Theme = labelTheme;
			
			healthContainer.AddChild(healthLabel);
			// healthLabel.Owner = healthContainer;  // 删除这行
			
			// 健康条
			var healthBar = new ProgressBar();
			healthBar.Name = "HealthBar";
			healthBar.MinValue = 0;
			healthBar.MaxValue = 100;
			healthBar.Value = 100;
			healthBar.CustomMinimumSize = new Vector2(180, 20);
			
			// 设置健康条样式
			var progressTheme = new Theme();
			
			// 背景样式
			var bgStyle = new StyleBoxFlat();
			bgStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
			bgStyle.CornerRadiusTopLeft = 5;
			bgStyle.CornerRadiusTopRight = 5;
			bgStyle.CornerRadiusBottomLeft = 5;
			bgStyle.CornerRadiusBottomRight = 5;
			progressTheme.SetStylebox("background", "ProgressBar", bgStyle);
			
			// 填充样式
			var fillStyle = new StyleBoxFlat();
			fillStyle.BgColor = new Color(0.8f, 0.2f, 0.2f, 1.0f); // 红色
			fillStyle.CornerRadiusTopLeft = 5;
			fillStyle.CornerRadiusTopRight = 5;
			fillStyle.CornerRadiusBottomLeft = 5;
			fillStyle.CornerRadiusBottomRight = 5;
			progressTheme.SetStylebox("fill", "ProgressBar", fillStyle);
			
			healthBar.Theme = progressTheme;
			
			healthContainer.AddChild(healthBar);
			// healthBar.Owner = healthContainer;  // 删除这行
			
			return healthContainer;
		}
		
		private VBoxContainer CreateInfoContainer()
		{
			var infoContainer = new VBoxContainer();
			infoContainer.Name = "InfoContainer";
			infoContainer.CustomMinimumSize = new Vector2(200, 60);
			
			// 等级标签
			var levelLabel = new Label();
			levelLabel.Name = "LevelLabel";
			levelLabel.Text = "Level: 1";
			levelLabel.HorizontalAlignment = HorizontalAlignment.Left;
			
			// 分数标签
			var scoreLabel = new Label();
			scoreLabel.Name = "ScoreLabel";
			scoreLabel.Text = "Score: 0";
			scoreLabel.HorizontalAlignment = HorizontalAlignment.Left;
			
			// 设置标签样式
			var labelTheme = new Theme();
			var labelFont = new SystemFont();
			labelFont.FontNames = new[] { "Arial" };
			labelTheme.SetFont("font", "Label", labelFont);
			labelTheme.SetFontSize("font_size", "Label", 18);
			labelTheme.SetColor("font_color", "Label", Colors.White);
			
			levelLabel.Theme = labelTheme;
			scoreLabel.Theme = labelTheme;
			
			infoContainer.AddChild(levelLabel);
			levelLabel.Owner = infoContainer;
			
			infoContainer.AddChild(scoreLabel);
			scoreLabel.Owner = infoContainer;
			
			return infoContainer;
		}
		
		private Button CreatePauseButton()
		{
			var pauseButton = new Button();
			pauseButton.Name = "PauseButton";
			pauseButton.Text = "暂停";
			pauseButton.CustomMinimumSize = new Vector2(80, 40);
			
			// 设置按钮位置（右上角）
			pauseButton.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
			pauseButton.Position = new Vector2(-100, 20);
			
			// 设置按钮样式
			var buttonTheme = new Theme();
			var buttonFont = new SystemFont();
			buttonFont.FontNames = new[] { "Arial" };
			buttonTheme.SetFont("font", "Button", buttonFont);
			buttonTheme.SetFontSize("font_size", "Button", 14);
			
			// 按钮背景样式
			var normalStyle = new StyleBoxFlat();
			normalStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
			normalStyle.CornerRadiusTopLeft = 5;
			normalStyle.CornerRadiusTopRight = 5;
			normalStyle.CornerRadiusBottomLeft = 5;
			normalStyle.CornerRadiusBottomRight = 5;
			buttonTheme.SetStylebox("normal", "Button", normalStyle);
			
			var hoverStyle = new StyleBoxFlat();
			hoverStyle.BgColor = new Color(0.4f, 0.4f, 0.4f, 0.9f);
			hoverStyle.CornerRadiusTopLeft = 5;
			hoverStyle.CornerRadiusTopRight = 5;
			hoverStyle.CornerRadiusBottomLeft = 5;
			hoverStyle.CornerRadiusBottomRight = 5;
			buttonTheme.SetStylebox("hover", "Button", hoverStyle);
			
			var pressedStyle = new StyleBoxFlat();
			pressedStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			pressedStyle.CornerRadiusTopLeft = 5;
			pressedStyle.CornerRadiusTopRight = 5;
			pressedStyle.CornerRadiusBottomLeft = 5;
			pressedStyle.CornerRadiusBottomRight = 5;
			buttonTheme.SetStylebox("pressed", "Button", pressedStyle);
			
			pauseButton.Theme = buttonTheme;
			
			return pauseButton;
		}
		
		// 添加这个辅助方法
		private void SetOwnerRecursively(Node node, Node owner)
		{
			foreach (Node child in node.GetChildren())
			{
				child.Owner = owner;
				SetOwnerRecursively(child, owner);
			}
		}
	}
}
#endif
