using Godot;

[Tool]
public partial class SettingsSceneGenerator : EditorScript
{
	public override void _Run()
	{
		// 创建场景
		var scene = new PackedScene();
		
		// 创建根节点
		var root = new Control();
		root.Name = "SettingsMenu";
		
		// root.SetAnchorsAndOffsetsPreset(Control.PresetMode.FullRect);

		
		// 添加脚本
		var script = GD.Load<Script>("res://Scripts/UI/SettingsMenu.cs");
		root.SetScript(script);
		
		// 创建背景
		var background = new ColorRect();
		background.Name = "Background";
		background.Color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
		root.AddChild(background);
		// background.Owner = root; // 移除这行
		
		// 创建滚动容器
		var scrollContainer = new ScrollContainer();
		scrollContainer.Name = "ScrollContainer";
		scrollContainer.OffsetLeft = 50;
		scrollContainer.OffsetTop = 50;
		scrollContainer.OffsetRight = -50;
		scrollContainer.OffsetBottom = -50;
		root.AddChild(scrollContainer);
		// scrollContainer.Owner = root; // 移除这行
		
		// 创建主容器
		var vboxContainer = new VBoxContainer();
		vboxContainer.Name = "VBoxContainer";
		vboxContainer.AddThemeConstantOverride("separation", 20);
		scrollContainer.AddChild(vboxContainer);
		vboxContainer.Owner = root;
		
		// 创建标题
		var titleLabel = new Label();
		titleLabel.Name = "TitleLabel";
		titleLabel.Text = "设置";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeColorOverride("font_color", Colors.White);
		vboxContainer.AddChild(titleLabel);
		titleLabel.Owner = root;
		
		// 音频设置部分
		var audioSection = CreateSection("音频设置");
		vboxContainer.AddChild(audioSection);
		audioSection.Owner = root;
		
		// 主音量
		var masterVolumeContainer = CreateSliderContainer("主音量", "MasterVolumeSlider", 0.0f, 1.0f, 0.01f, 1.0f);
		audioSection.AddChild(masterVolumeContainer);
		masterVolumeContainer.Owner = root;
		
		// 音乐音量
		var musicVolumeContainer = CreateSliderContainer("音乐音量", "MusicVolumeSlider", 0.0f, 1.0f, 0.01f, 0.8f);
		audioSection.AddChild(musicVolumeContainer);
		musicVolumeContainer.Owner = root;
		
		// 音效音量
		var sfxVolumeContainer = CreateSliderContainer("音效音量", "SfxVolumeSlider", 0.0f, 1.0f, 0.01f, 1.0f);
		audioSection.AddChild(sfxVolumeContainer);
		sfxVolumeContainer.Owner = root;
		
		// 全屏设置
		var fullscreenContainer = CreateCheckBoxContainer("全屏", "FullscreenCheckBox", false);
		// 创建显示设置部分
		var displaySection = CreateSection("显示设置");
		vboxContainer.AddChild(displaySection);
		displaySection.Owner = root;
		displaySection.AddChild(fullscreenContainer);
		fullscreenContainer.Owner = root;
		
		// 垂直同步
		var vsyncContainer = CreateCheckBoxContainer("垂直同步", "VsyncCheckBox", true);
		displaySection.AddChild(vsyncContainer);
		vsyncContainer.Owner = root;

		// 分辨率设置
		var resolutionContainer = CreateOptionButtonContainer("分辨率", "ResolutionOption", 
		new string[] { "1920x1080", "1600x900", "1366x768", "1280x720" });
		displaySection.AddChild(resolutionContainer);
		resolutionContainer.Owner = root;
		
		// 按钮容器
		var buttonContainer = new HBoxContainer();
		buttonContainer.Name = "ButtonContainer";
		buttonContainer.AddThemeConstantOverride("separation", 20);
		vboxContainer.AddChild(buttonContainer);
		buttonContainer.Owner = root;
		
		// 创建按钮
		var resetButton = CreateMenuButton("重置默认", "ResetButton");
		var backButton = CreateMenuButton("返回", "BackButton");
		
		buttonContainer.AddChild(resetButton);
		resetButton.Owner = root;
		
		buttonContainer.AddChild(backButton);
		backButton.Owner = root;
		
		// 在保存场景之前，统一设置所有节点的Owner
		SetOwnerRecursively(root, root);
		
		// 保存场景
		scene.Pack(root);
		ResourceSaver.Save(scene, "res://Scenes/UI/SettingsMenu.tscn");
		
		GD.Print("SettingsMenu.tscn 场景已生成完成！");
	}
	
	// 添加递归设置Owner的辅助方法
	private void SetOwnerRecursively(Node node, Node owner)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child != owner) // 避免将根节点设置为自己的Owner
			{
				child.Owner = owner;
				SetOwnerRecursively(child, owner);
			}
		}
	}
	
	private VBoxContainer CreateSection(string title)
	{
		var section = new VBoxContainer();
		section.Name = title.Replace(" ", "") + "Section";
		section.AddThemeConstantOverride("separation", 10);
		
		var titleLabel = new Label();
		titleLabel.Text = title;
		titleLabel.AddThemeColorOverride("font_color", Colors.Yellow);
		section.AddChild(titleLabel);
		
		return section;
	}
	
	private VBoxContainer CreateSliderContainer(string labelText, string sliderName, float minValue, float maxValue, float step, float defaultValue)
	{
		var container = new VBoxContainer();
		
		var label = new Label();
		label.Text = labelText;
		label.CustomMinimumSize = new Vector2(120, 0);
		container.AddChild(label);
		
		var slider = new HSlider();
		slider.Name = sliderName;
		slider.MinValue = minValue;
		slider.MaxValue = maxValue;
		slider.Step = step;
		slider.Value = defaultValue;
		slider.CustomMinimumSize = new Vector2(200, 30);
		container.AddChild(slider);
		
		return container;
	}
	
	private HBoxContainer CreateCheckBoxContainer(string labelText, string checkBoxName, bool pressed)
	{
		var container = new HBoxContainer();
		container.Name = checkBoxName + "Container";
		
		var label = new Label();
		label.Text = labelText;
		label.CustomMinimumSize = new Vector2(120, 0);
		container.AddChild(label);
		
		var checkBox = new CheckBox();
		checkBox.Name = checkBoxName;
		checkBox.ButtonPressed = pressed;
		container.AddChild(checkBox);
		
		return container;
	}
	
	private HBoxContainer CreateOptionButtonContainer(string labelText, string optionName, string[] options)
	{
		var container = new HBoxContainer();
		container.Name = optionName + "Container";
		
		var label = new Label();
		label.Text = labelText;
		label.CustomMinimumSize = new Vector2(120, 0);
		container.AddChild(label);
		
		var optionButton = new OptionButton();
		optionButton.Name = optionName;
		optionButton.CustomMinimumSize = new Vector2(200, 0);
		
		foreach (var option in options)
		{
			optionButton.AddItem(option);
		}
		
		container.AddChild(optionButton);
		
		return container;
	}
	
	private Button CreateMenuButton(string text, string name)
	{
		var button = new Button();
		button.Name = name;
		button.Text = text;
		button.CustomMinimumSize = new Vector2(120, 40);
		
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
