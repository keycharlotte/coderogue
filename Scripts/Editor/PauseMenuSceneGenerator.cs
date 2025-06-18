using System.Runtime;
using Godot;

[Tool]
public partial class PauseMenuSceneGenerator : EditorScript
{
	public override void _Run()
	{
		// 创建场景
		var scene = new PackedScene();
		
		// 创建根节点
		var root = new Control();
		root.Name = "PauseMenu";
		//root.SetAnchorsAndOffsetsPreset(Control.Preset.Rect); // 15 对应 FullRect
		
		// 添加脚本
		var script = GD.Load<Script>("res://Scripts/UI/PauseMenu.cs");
		root.SetScript(script);
		
		// 创建半透明背景面板
		var backgroundPanel = new Panel();
		backgroundPanel.Name = "BackgroundPanel";
		//backgroundPanel.SetAnchorsAndOffsetsPreset((Control.Preset)15);
		backgroundPanel.Modulate = new Color(0, 0, 0, 0.7f);
		root.AddChild(backgroundPanel);
		backgroundPanel.Owner = root;
		
		// 创建中心容器
		var centerContainer = new CenterContainer();
		centerContainer.Name = "CenterContainer";
		//centerContainer.SetAnchorsAndOffsetsPreset((Control.Preset)15);
		root.AddChild(centerContainer);
		centerContainer.Owner = root;
		
		// 创建垂直布局容器
		var vboxContainer = new VBoxContainer();
		vboxContainer.Name = "VBoxContainer";
		vboxContainer.AddThemeConstantOverride("separation", 20);
		centerContainer.AddChild(vboxContainer);
		vboxContainer.Owner = root;
		
		// 创建标题
		var titleLabel = new Label();
		titleLabel.Name = "TitleLabel";
		titleLabel.Text = "游戏暂停";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
		vboxContainer.AddChild(titleLabel);
		titleLabel.Owner = root;
		
		// 创建按钮
		var resumeButton = CreateMenuButton("继续游戏", "ResumeButton");
		var settingsButton = CreateMenuButton("设置", "SettingsButton");
		var mainMenuButton = CreateMenuButton("主菜单", "MainMenuButton");
		var quitButton = CreateMenuButton("退出游戏", "QuitButton");
		
		vboxContainer.AddChild(resumeButton);
		resumeButton.Owner = root;
		
		vboxContainer.AddChild(settingsButton);
		settingsButton.Owner = root;
		
		vboxContainer.AddChild(mainMenuButton);
		mainMenuButton.Owner = root;
		
		vboxContainer.AddChild(quitButton);
		quitButton.Owner = root;
		
		// 保存场景
		scene.Pack(root);
		ResourceSaver.Save(scene, "res://Scenes/UI/PauseMenu.tscn");
		
		GD.Print("PauseMenu.tscn 场景已生成完成！");
	}
	
	private Button CreateMenuButton(string text, string name)
	{
		var button = new Button();
		button.Name = name;
		button.Text = text;
		button.CustomMinimumSize = new Vector2(200, 50);
		
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
		
		//// 手动设置锚点为全屏
		//root.AnchorLeft = 0.0f;
		//root.AnchorTop = 0.0f;
		//root.AnchorRight = 1.0f;
		//root.AnchorBottom = 1.0f;
		//root.OffsetLeft = 0.0f;
		//root.OffsetTop = 0.0f;
		//root.OffsetRight = 0.0f;
		//root.OffsetBottom = 0.0f;
		return button;
	}
}
