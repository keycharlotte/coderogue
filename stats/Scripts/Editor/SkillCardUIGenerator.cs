using Godot;

[Tool]
public partial class SkillCardUIGenerator : EditorScript
{
	public override void _Run()
	{
		CreateSkillCardUIScene();
		GD.Print("SkillCardUI.tscn 场景文件已生成完成！");
	}
	
	private void CreateSkillCardUIScene()
	{
		// 创建根节点 - SkillCardUI (Control)
		var skillCardUI = new Control();
		skillCardUI.Name = "SkillCardUI";
		skillCardUI.SetScript(GD.Load("res://Scripts/UI/SkillCardUI.cs"));
		skillCardUI.CustomMinimumSize = new Vector2(200, 150);
		skillCardUI.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		
		// 创建卡片面板
		var cardPanel = new Panel();
		cardPanel.Name = "CardPanel";
		// cardPanel.SetAnchorsAndOffsetsPreset(Control.PresetMode.FullRect);
		
		// 设置面板样式
		var panelStyleBox = new StyleBoxFlat();
		panelStyleBox.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
		panelStyleBox.SetCornerRadiusAll(8);
		panelStyleBox.BorderWidthTop = 2;
		panelStyleBox.BorderWidthBottom = 2;
		panelStyleBox.BorderWidthLeft = 2;
		panelStyleBox.BorderWidthRight = 2;
		panelStyleBox.BorderColor = new Color(0.4f, 0.4f, 0.4f);
		cardPanel.AddThemeStyleboxOverride("panel", panelStyleBox);
		
		skillCardUI.AddChild(cardPanel);
		
		// 创建主容器
		var mainContainer = new VBoxContainer();
		mainContainer.Name = "MainContainer";
		// mainContainer.SetAnchorsAndOffsetsPreset(Control.PresetMode.FullRect);
		mainContainer.AddThemeConstantOverride("separation", 5);
		
		// 设置边距
		// mainContainer.SetOffsetsPreset(Control.PresetMode.FullRect);
		mainContainer.OffsetLeft = 8;
		mainContainer.OffsetTop = 8;
		mainContainer.OffsetRight = -8;
		mainContainer.OffsetBottom = -8;
		
		cardPanel.AddChild(mainContainer);
		
		// 卡片头部容器（图标和名称）
		var headerContainer = new HBoxContainer();
		headerContainer.Name = "HeaderContainer";
		headerContainer.AddThemeConstantOverride("separation", 8);
		mainContainer.AddChild(headerContainer);
		
		// 图标
		var iconRect = new TextureRect();
		iconRect.Name = "IconRect";
		iconRect.CustomMinimumSize = new Vector2(32, 32);
		iconRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
		iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		headerContainer.AddChild(iconRect);
		
		// 名称标签
		var nameLabel = new Label();
		nameLabel.Name = "NameLabel";
		nameLabel.Text = "技能名称";
		nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		nameLabel.AddThemeFontSizeOverride("font_size", 14);
		nameLabel.AddThemeColorOverride("font_color", Colors.White);
		headerContainer.AddChild(nameLabel);
		
		// 信息容器（消耗和类型）
		var infoContainer = new HBoxContainer();
		infoContainer.Name = "InfoContainer";
		mainContainer.AddChild(infoContainer);
		
		// 消耗标签
		var costLabel = new Label();
		costLabel.Name = "CostLabel";
		costLabel.Text = "消耗: 0";
		costLabel.AddThemeFontSizeOverride("font_size", 12);
		costLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 1f));
		infoContainer.AddChild(costLabel);
		
		// 类型标签
		var typeLabel = new Label();
		typeLabel.Name = "TypeLabel";
		typeLabel.Text = "攻击";
		typeLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		typeLabel.HorizontalAlignment = HorizontalAlignment.Right;
		typeLabel.AddThemeFontSizeOverride("font_size", 12);
		typeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.8f));
		infoContainer.AddChild(typeLabel);
		
		// 描述标签
		var descriptionLabel = new Label();
		descriptionLabel.Name = "DescriptionLabel";
		descriptionLabel.Text = "技能描述文本";
		descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		descriptionLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		descriptionLabel.VerticalAlignment = VerticalAlignment.Top;
		descriptionLabel.AddThemeFontSizeOverride("font_size", 11);
		descriptionLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
		mainContainer.AddChild(descriptionLabel);
		
		// 标签容器
		var tagsContainer = new VBoxContainer();
		tagsContainer.Name = "TagsContainer";
		mainContainer.AddChild(tagsContainer);
		
		// 标签显示（示例）
		var tagsLabel = new Label();
		tagsLabel.Name = "TagsLabel";
		tagsLabel.Text = "标签: 示例标签";
		tagsLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		tagsLabel.AddThemeFontSizeOverride("font_size", 10);
		tagsLabel.AddThemeColorOverride("font_color", Colors.LightGray);
		tagsContainer.AddChild(tagsLabel);
		
		// 移除按钮
		var removeButton = new Button();
		removeButton.Name = "RemoveButton";
		removeButton.Text = "移除";
		removeButton.CustomMinimumSize = new Vector2(60, 25);
		
		// 设置按钮样式
		var buttonStyleBox = new StyleBoxFlat();
		buttonStyleBox.BgColor = new Color(0.8f, 0.3f, 0.3f, 0.8f);
		buttonStyleBox.SetCornerRadiusAll(4);
		removeButton.AddThemeStyleboxOverride("normal", buttonStyleBox);
		
		var buttonHoverStyleBox = new StyleBoxFlat();
		buttonHoverStyleBox.BgColor = new Color(1f, 0.4f, 0.4f, 0.9f);
		buttonHoverStyleBox.SetCornerRadiusAll(4);
		removeButton.AddThemeStyleboxOverride("hover", buttonHoverStyleBox);
		
		removeButton.AddThemeFontSizeOverride("font_size", 12);
		removeButton.AddThemeColorOverride("font_color", Colors.White);
		
		mainContainer.AddChild(removeButton);
		
		// 创建PackedScene并保存
		var packedScene = new PackedScene();
		packedScene.Pack(skillCardUI);
		
		var savePath = "res://Scenes/UI/SkillCardUI.tscn";
		var error = ResourceSaver.Save(packedScene, savePath);
		
		if (error == Error.Ok)
		{
			GD.Print($"SkillCardUI场景已保存到: {savePath}");
		}
		else
		{
			GD.PrintErr($"保存SkillCardUI场景失败: {error}");
		}
		
		// 清理临时节点
		skillCardUI.QueueFree();
	}
}
