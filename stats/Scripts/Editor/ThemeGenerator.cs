using Godot;

[Tool]
public partial class ThemeGenerator : EditorScript
{
	public override void _Run()
	{
		// 创建新主题
		var theme = new Theme();
		
		// 定义颜色调色板（基于游戏界面分析）
		var darkMenuBg = new Color(0.15f, 0.15f, 0.15f, 1.0f); // 深色菜单背景
		var lightPanelBg = new Color(0.85f, 0.85f, 0.85f, 1.0f); // 浅色面板背景
		var buttonDark = new Color(0.25f, 0.25f, 0.25f, 1.0f); // 深色按钮
		var buttonHover = new Color(0.35f, 0.35f, 0.35f, 1.0f); // 悬停状态
		var buttonPressed = new Color(0.45f, 0.45f, 0.45f, 1.0f); // 按下状态
		var textWhite = Colors.White; // 白色文字
		var textDark = new Color(0.2f, 0.2f, 0.2f, 1.0f); // 深色文字
		var accentColor = new Color(0.4f, 0.6f, 0.8f, 1.0f); // 蓝色强调色
		
		// 创建菜单按钮样式（左侧深色菜单）
		CreateMenuButtonStyles(theme, buttonDark, buttonHover, buttonPressed, textWhite);
		
		// 创建设置按钮样式（右侧设置区）
		CreateSettingsButtonStyles(theme, lightPanelBg, accentColor, textDark);
		
		// 创建面板样式
		CreatePanelStyles(theme, darkMenuBg, lightPanelBg);
		
		// 创建标签样式
		CreateLabelStyles(theme, textWhite, textDark);
		
		// 创建滑块样式
		CreateSliderStyles(theme, accentColor, lightPanelBg);
		
		// 创建复选框样式
		CreateCheckBoxStyles(theme, accentColor, textDark);
		
		// 保存主题
		ResourceSaver.Save(theme, "res://ResourcesThemes/GameUITheme.tres");
		
		GD.Print("游戏UI主题已创建完成！");
	}
	
	// 左侧菜单按钮样式
	private void CreateMenuButtonStyles(Theme theme, Color buttonDark, Color buttonHover, Color buttonPressed, Color textWhite)
	{
		// 普通状态
		var normalStyle = new StyleBoxFlat();
		normalStyle.BgColor = buttonDark;
		normalStyle.CornerRadiusTopLeft = 0;
		normalStyle.CornerRadiusTopRight = 0;
		normalStyle.CornerRadiusBottomLeft = 0;
		normalStyle.CornerRadiusBottomRight = 0;
		normalStyle.ContentMarginLeft = 20;
		normalStyle.ContentMarginRight = 20;
		normalStyle.ContentMarginTop = 12;
		normalStyle.ContentMarginBottom = 12;
		
		// 悬停状态
		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = buttonHover;
		hoverStyle.CornerRadiusTopLeft = 0;
		hoverStyle.CornerRadiusTopRight = 0;
		hoverStyle.CornerRadiusBottomLeft = 0;
		hoverStyle.CornerRadiusBottomRight = 0;
		hoverStyle.ContentMarginLeft = 20;
		hoverStyle.ContentMarginRight = 20;
		hoverStyle.ContentMarginTop = 12;
		hoverStyle.ContentMarginBottom = 12;
		
		// 按下状态
		var pressedStyle = new StyleBoxFlat();
		pressedStyle.BgColor = buttonPressed;
		pressedStyle.CornerRadiusTopLeft = 0;
		pressedStyle.CornerRadiusTopRight = 0;
		pressedStyle.CornerRadiusBottomLeft = 0;
		pressedStyle.CornerRadiusBottomRight = 0;
		pressedStyle.ContentMarginLeft = 20;
		pressedStyle.ContentMarginRight = 20;
		pressedStyle.ContentMarginTop = 12;
		pressedStyle.ContentMarginBottom = 12;
		
		// 应用菜单按钮样式
		theme.SetStylebox("normal", "MenuButton", normalStyle);
		theme.SetStylebox("hover", "MenuButton", hoverStyle);
		theme.SetStylebox("pressed", "MenuButton", pressedStyle);
		theme.SetColor("font_color", "MenuButton", textWhite);
		theme.SetColor("font_hover_color", "MenuButton", textWhite);
		theme.SetColor("font_pressed_color", "MenuButton", textWhite);
	}
	
	// 设置区按钮样式
	private void CreateSettingsButtonStyles(Theme theme, Color lightBg, Color accentColor, Color textDark)
	{
		// 设置按钮普通状态
		var normalStyle = new StyleBoxFlat();
		normalStyle.BgColor = lightBg;
		normalStyle.BorderColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
		normalStyle.BorderWidthLeft = 1;
		normalStyle.BorderWidthRight = 1;
		normalStyle.BorderWidthTop = 1;
		normalStyle.BorderWidthBottom = 1;
		normalStyle.CornerRadiusTopLeft = 4;
		normalStyle.CornerRadiusTopRight = 4;
		normalStyle.CornerRadiusBottomLeft = 4;
		normalStyle.CornerRadiusBottomRight = 4;
		normalStyle.ContentMarginLeft = 12;
		normalStyle.ContentMarginRight = 12;
		normalStyle.ContentMarginTop = 8;
		normalStyle.ContentMarginBottom = 8;
		
		// 设置按钮悬停状态
		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = accentColor;
		hoverStyle.BorderColor = accentColor;
		hoverStyle.BorderWidthLeft = 1;
		hoverStyle.BorderWidthRight = 1;
		hoverStyle.BorderWidthTop = 1;
		hoverStyle.BorderWidthBottom = 1;
		hoverStyle.CornerRadiusTopLeft = 4;
		hoverStyle.CornerRadiusTopRight = 4;
		hoverStyle.CornerRadiusBottomLeft = 4;
		hoverStyle.CornerRadiusBottomRight = 4;
		hoverStyle.ContentMarginLeft = 12;
		hoverStyle.ContentMarginRight = 12;
		hoverStyle.ContentMarginTop = 8;
		hoverStyle.ContentMarginBottom = 8;
		
		// 应用普通按钮样式
		theme.SetStylebox("normal", "Button", normalStyle);
		theme.SetStylebox("hover", "Button", hoverStyle);
		theme.SetStylebox("pressed", "Button", hoverStyle);
		theme.SetColor("font_color", "Button", textDark);
		theme.SetColor("font_hover_color", "Button", Colors.White);
		theme.SetColor("font_pressed_color", "Button", Colors.White);
	}
	
	// 面板样式
	private void CreatePanelStyles(Theme theme, Color darkBg, Color lightBg)
	{
		// 深色面板（左侧菜单）
		var darkPanel = new StyleBoxFlat();
		darkPanel.BgColor = darkBg;
		darkPanel.CornerRadiusTopLeft = 0;
		darkPanel.CornerRadiusTopRight = 0;
		darkPanel.CornerRadiusBottomLeft = 0;
		darkPanel.CornerRadiusBottomRight = 0;
		
		// 浅色面板（右侧设置区）
		var lightPanel = new StyleBoxFlat();
		lightPanel.BgColor = lightBg;
		lightPanel.CornerRadiusTopLeft = 8;
		lightPanel.CornerRadiusTopRight = 8;
		lightPanel.CornerRadiusBottomLeft = 8;
		lightPanel.CornerRadiusBottomRight = 8;
		
		theme.SetStylebox("panel", "DarkPanel", darkPanel);
		theme.SetStylebox("panel", "Panel", lightPanel);
	}
	
	// 标签样式
	private void CreateLabelStyles(Theme theme, Color textWhite, Color textDark)
	{
		// 菜单标签（白色文字）
		theme.SetColor("font_color", "MenuLabel", textWhite);
		
		// 普通标签（深色文字）
		theme.SetColor("font_color", "Label", textDark);
		
		// 标题标签
		theme.SetColor("font_color", "TitleLabel", textDark);
	}
	
	// 滑块样式
	private void CreateSliderStyles(Theme theme, Color accentColor, Color lightBg)
	{
		// 滑块轨道
		var sliderBg = new StyleBoxFlat();
		sliderBg.BgColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
		sliderBg.CornerRadiusTopLeft = 3;
		sliderBg.CornerRadiusTopRight = 3;
		sliderBg.CornerRadiusBottomLeft = 3;
		sliderBg.CornerRadiusBottomRight = 3;
		
		// 滑块抓手
		var sliderGrabber = new StyleBoxFlat();
		sliderGrabber.BgColor = accentColor;
		sliderGrabber.CornerRadiusTopLeft = 6;
		sliderGrabber.CornerRadiusTopRight = 6;
		sliderGrabber.CornerRadiusBottomLeft = 6;
		sliderGrabber.CornerRadiusBottomRight = 6;
		
		theme.SetStylebox("slider", "HSlider", sliderBg);
		theme.SetStylebox("grabber_area", "HSlider", sliderGrabber);
		theme.SetStylebox("grabber_area_highlight", "HSlider", sliderGrabber);
	}
	
	// 复选框样式
	private void CreateCheckBoxStyles(Theme theme, Color accentColor, Color textDark)
	{
		// 未选中状态
		var Unchecked = new StyleBoxFlat();
		Unchecked.BgColor = Colors.White;
		Unchecked.BorderColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
		Unchecked.BorderWidthLeft = 2;
		Unchecked.BorderWidthRight = 2;
		Unchecked.BorderWidthTop = 2;
		Unchecked.BorderWidthBottom = 2;
		Unchecked.CornerRadiusTopLeft = 3;
		Unchecked.CornerRadiusTopRight = 3;
		Unchecked.CornerRadiusBottomLeft = 3;
		Unchecked.CornerRadiusBottomRight = 3;
		
		// 选中状态
		var Checked = new StyleBoxFlat();
		Checked.BgColor = accentColor;
		Checked.BorderColor = accentColor;
		Checked.BorderWidthLeft = 2;
		Checked.BorderWidthRight = 2;
		Checked.BorderWidthTop = 2;
		Checked.BorderWidthBottom = 2;
		Checked.CornerRadiusTopLeft = 3;
		Checked.CornerRadiusTopRight = 3;
		Checked.CornerRadiusBottomLeft = 3;
		Checked.CornerRadiusBottomRight = 3;
		
		theme.SetStylebox("normal", "CheckBox", Unchecked);
		theme.SetStylebox("pressed", "CheckBox", Checked);
		theme.SetColor("font_color", "CheckBox", textDark);
	}
}
