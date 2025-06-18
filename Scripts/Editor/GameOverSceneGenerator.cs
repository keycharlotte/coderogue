using Godot;

[Tool]
public partial class GameOverSceneGenerator : EditorScript
{
	public override void _Run()
	{
		// 创建场景
		var scene = new PackedScene();
		
		// 创建根节点
		var root = new Control();
		root.Name = "GameOverScreen";
		//root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.PRESET_FULL_RECT);
		
		// 添加脚本
		var script = GD.Load<Script>("res://Scripts/UI/GameOverScreen.cs");
		root.SetScript(script);
		
		// 创建背景
		var background = new ColorRect();
		background.Name = "Background";
		//background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.PRESET_FULL_RECT);
		background.Color = new Color(0, 0, 0, 0.8f);
		root.AddChild(background);
		background.Owner = root;
		
		// 创建中心容器
		var centerContainer = new CenterContainer();
		centerContainer.Name = "CenterContainer";
		//centerContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.PRESET_FULL_RECT);
		root.AddChild(centerContainer);
		centerContainer.Owner = root;
		
		// 创建垂直布局容器
		var vboxContainer = new VBoxContainer();
		vboxContainer.Name = "VBoxContainer";
		vboxContainer.AddThemeConstantOverride("separation", 30);
		centerContainer.AddChild(vboxContainer);
		vboxContainer.Owner = root;
		
		// 创建游戏结束标题
		var titleLabel = new Label();
		titleLabel.Name = "TitleLabel";
		titleLabel.Text = "游戏结束";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeColorOverride("font_color", Colors.Red);
		vboxContainer.AddChild(titleLabel);
		titleLabel.Owner = root;
		
		// 创建分数标签
		var scoreLabel = new Label();
		scoreLabel.Name = "ScoreLabel";
		scoreLabel.Text = "得分: 0";
		scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
		vboxContainer.AddChild(scoreLabel);
		scoreLabel.Owner = root;
		
		// 创建最高分标签
		var highScoreLabel = new Label();
		highScoreLabel.Name = "HighScoreLabel";
		highScoreLabel.Text = "最高分: 0";
		highScoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
		vboxContainer.AddChild(highScoreLabel);
		highScoreLabel.Owner = root;
		
		// 创建按钮容器
		var buttonContainer = new HBoxContainer();
		buttonContainer.Name = "ButtonContainer";
		buttonContainer.AddThemeConstantOverride("separation", 20);
		vboxContainer.AddChild(buttonContainer);
		buttonContainer.Owner = root;
		
		// 创建按钮
		var restartButton = CreateMenuButton("重新开始", "RestartButton");
		var mainMenuButton = CreateMenuButton("主菜单", "MainMenuButton");
		var quitButton = CreateMenuButton("退出游戏", "QuitButton");
		
		buttonContainer.AddChild(restartButton);
		restartButton.Owner = root;
		
		buttonContainer.AddChild(mainMenuButton);
		mainMenuButton.Owner = root;
		
		buttonContainer.AddChild(quitButton);
		quitButton.Owner = root;
		
		// 保存场景
		scene.Pack(root);
		ResourceSaver.Save(scene, "res://Scenes/UI/GameOverScreen.tscn");
		
		GD.Print("GameOverScreen.tscn 场景已生成完成！");
	}
	
	private Button CreateMenuButton(string text, string name)
	{
		var button = new Button();
		button.Name = name;
		button.Text = text;
		button.CustomMinimumSize = new Vector2(150, 50);
		
		// 设置按钮样式
		var normalStyle = new StyleBoxFlat();
		normalStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
		normalStyle.CornerRadiusTopLeft = 8;
		normalStyle.CornerRadiusTopRight = 8;
		normalStyle.CornerRadiusBottomLeft = 8;
		normalStyle.CornerRadiusBottomRight = 8;
		
		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
		hoverStyle.CornerRadiusTopLeft = 8;
		hoverStyle.CornerRadiusTopRight = 8;
		hoverStyle.CornerRadiusBottomLeft = 8;
		hoverStyle.CornerRadiusBottomRight = 8;
		
		var pressedStyle = new StyleBoxFlat();
		pressedStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
		pressedStyle.CornerRadiusTopLeft = 8;
		pressedStyle.CornerRadiusTopRight = 8;
		pressedStyle.CornerRadiusBottomLeft = 8;
		pressedStyle.CornerRadiusBottomRight = 8;
		
		button.AddThemeStyleboxOverride("normal", normalStyle);
		button.AddThemeStyleboxOverride("hover", hoverStyle);
		button.AddThemeStyleboxOverride("pressed", pressedStyle);
		
		return button;
	}
}
