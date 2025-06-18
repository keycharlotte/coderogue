#if TOOLS
using Godot;

namespace CodeRogue.Editor
{
	/// <summary>
	/// 主菜单场景生成器 - 自动创建主菜单UI场景
	/// </summary>
	[Tool]
	public partial class MainMenuGenerator : EditorScript
	{
		public override void _Run()
		{
			CreateMainMenuScene();
			GD.Print("MainMenuGenerator 运行完成");
		}
		
		private void CreateMainMenuScene()
		{
			var scene = new PackedScene();
			var mainMenu = new Control();
			mainMenu.Name = "MainMenu";
			
			// 设置主菜单为全屏
			mainMenu.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			
			// 添加主菜单脚本
			var mainMenuScript = GD.Load<CSharpScript>("res://Scripts/UI/MainMenu.cs");
			mainMenu.SetScript(mainMenuScript);
			
			// 创建背景
			var background = new ColorRect();
			background.Name = "Background";
			background.Color = new Color(0.1f, 0.1f, 0.2f, 1.0f); // 深蓝色背景
			background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			mainMenu.AddChild(background);
			background.Owner = mainMenu;
			
			// 创建标题标签
			var titleLabel = new Label();
			titleLabel.Name = "TitleLabel";
			titleLabel.Text = "Code Rogue";
			titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			titleLabel.VerticalAlignment = VerticalAlignment.Center;
			
			// 设置标题位置和大小
			titleLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterTop);
			titleLabel.Position = new Vector2(-100, 100);
			titleLabel.Size = new Vector2(200, 80);
			
			// 设置标题字体大小（通过主题）
			var titleTheme = new Theme();
			var titleFont = new SystemFont();
			titleFont.FontNames = new[] { "Arial" };
			titleTheme.SetFont("font", "Label", titleFont);
			titleTheme.SetFontSize("font_size", "Label", 48);
			titleLabel.Theme = titleTheme;
			
			mainMenu.AddChild(titleLabel);
			titleLabel.Owner = mainMenu;
			
			// 创建按钮容器
			var vboxContainer = new VBoxContainer();
			vboxContainer.Name = "VBoxContainer";
			vboxContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
			vboxContainer.Position = new Vector2(-100, -100);
			vboxContainer.Size = new Vector2(200, 300);
			vboxContainer.AddThemeConstantOverride("separation", 20);
			
			mainMenu.AddChild(vboxContainer);
			vboxContainer.Owner = mainMenu;
			
			// 创建开始游戏按钮
			var startButton = CreateMenuButton("StartButton", "开始游戏");
			vboxContainer.AddChild(startButton);
			startButton.Owner = mainMenu;
			
			// 创建继续游戏按钮
			var continueButton = CreateMenuButton("ContinueButton", "继续游戏");
			vboxContainer.AddChild(continueButton);
			continueButton.Owner = mainMenu;
			
			// 创建设置按钮
			var settingsButton = CreateMenuButton("SettingsButton", "设置");
			vboxContainer.AddChild(settingsButton);
			settingsButton.Owner = mainMenu;
			
			// 创建退出按钮
			var quitButton = CreateMenuButton("QuitButton", "退出游戏");
			vboxContainer.AddChild(quitButton);
			quitButton.Owner = mainMenu;
			
			// 创建音频播放器
			var audioPlayer = new AudioStreamPlayer();
			audioPlayer.Name = "AudioStreamPlayer";
			mainMenu.AddChild(audioPlayer);
			audioPlayer.Owner = mainMenu;
			
			// 确保目录存在
			if (!DirAccess.DirExistsAbsolute("res://Scenes/UI"))
			{
				DirAccess.MakeDirRecursiveAbsolute("res://Scenes/UI");
			}
			
			// 保存场景
			scene.Pack(mainMenu);
			var saveResult = ResourceSaver.Save(scene, "res://Scenes/UI/MainMenu.tscn");
			
			if (saveResult == Error.Ok)
			{
				GD.Print("MainMenu.tscn 创建完成");
			}
			else
			{
				GD.PrintErr($"保存 MainMenu.tscn 失败: {saveResult}");
			}
		}
		
		private Button CreateMenuButton(string name, string text)
		{
			var button = new Button();
			button.Name = name;
			button.Text = text;
			button.CustomMinimumSize = new Vector2(180, 50);
			
			// 设置按钮样式
			var buttonTheme = new Theme();
			var buttonFont = new SystemFont();
			buttonFont.FontNames = new[] { "Arial" };
			buttonTheme.SetFont("font", "Button", buttonFont);
			buttonTheme.SetFontSize("font_size", "Label", 16);
			
			// 设置按钮颜色
			var normalStyle = new StyleBoxFlat();
			normalStyle.BgColor = new Color(0.3f, 0.3f, 0.5f, 1.0f);
			normalStyle.CornerRadiusTopLeft = 5;
			normalStyle.CornerRadiusTopRight = 5;
			normalStyle.CornerRadiusBottomLeft = 5;
			normalStyle.CornerRadiusBottomRight = 5;
			buttonTheme.SetStylebox("normal", "Button", normalStyle);
			
			var hoverStyle = new StyleBoxFlat();
			hoverStyle.BgColor = new Color(0.4f, 0.4f, 0.6f, 1.0f);
			hoverStyle.CornerRadiusTopLeft = 5;
			hoverStyle.CornerRadiusTopRight = 5;
			hoverStyle.CornerRadiusBottomLeft = 5;
			hoverStyle.CornerRadiusBottomRight = 5;
			buttonTheme.SetStylebox("hover", "Button", hoverStyle);
			
			var pressedStyle = new StyleBoxFlat();
			pressedStyle.BgColor = new Color(0.2f, 0.2f, 0.4f, 1.0f);
			pressedStyle.CornerRadiusTopLeft = 5;
			pressedStyle.CornerRadiusTopRight = 5;
			pressedStyle.CornerRadiusBottomLeft = 5;
			pressedStyle.CornerRadiusBottomRight = 5;
			buttonTheme.SetStylebox("pressed", "Button", pressedStyle);
			
			button.Theme = buttonTheme;
			
			return button;
		}
	}
}
#endif
