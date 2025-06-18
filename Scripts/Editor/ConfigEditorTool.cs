using Godot;

[Tool]
public partial class ConfigEditorTool : EditorScript
{
	public override void _Run()
	{
		ShowConfigMenu();
	}
	
	private void ShowConfigMenu()
	{
		var dialog = new AcceptDialog();
		dialog.Title = "配置表管理工具";
		dialog.Size = new Vector2I(400, 300);
		
		var vbox = new VBoxContainer();
		dialog.AddChild(vbox);
		
		// 标题
		var titleLabel = new Label();
		titleLabel.Text = "Excel配置表导入/导出工具";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		vbox.AddChild(titleLabel);
		
		vbox.AddChild(new HSeparator());
		
		// 导出按钮
		var exportLabel = new Label();
		exportLabel.Text = "导出配置到Excel:";
		vbox.AddChild(exportLabel);
		
		var exportHBox = new HBoxContainer();
		vbox.AddChild(exportHBox);
		
		var exportEnemyBtn = new Button();
		exportEnemyBtn.Text = "导出敌人配置";
		exportEnemyBtn.Pressed += () => ExportConfig("enemy");
		exportHBox.AddChild(exportEnemyBtn);
		
		var exportLevelBtn = new Button();
		exportLevelBtn.Text = "导出关卡配置";
		exportLevelBtn.Pressed += () => ExportConfig("level");
		exportHBox.AddChild(exportLevelBtn);
		
		var exportAllBtn = new Button();
		exportAllBtn.Text = "导出全部";
		exportAllBtn.Pressed += ExportAllConfigs;
		exportHBox.AddChild(exportAllBtn);
		
		vbox.AddChild(new HSeparator());
		
		// 导入按钮
		var importLabel = new Label();
		importLabel.Text = "从Excel导入配置:";
		vbox.AddChild(importLabel);
		
		var importHBox = new HBoxContainer();
		vbox.AddChild(importHBox);
		
		var importEnemyBtn = new Button();
		importEnemyBtn.Text = "导入敌人配置";
		importEnemyBtn.Pressed += () => ImportConfig("enemy");
		importHBox.AddChild(importEnemyBtn);
		
		var importLevelBtn = new Button();
		importLevelBtn.Text = "导入关卡配置";
		importLevelBtn.Pressed += () => ImportConfig("level");
		importHBox.AddChild(importLevelBtn);
		
		var importAllBtn = new Button();
		importAllBtn.Text = "导入全部";
		importAllBtn.Pressed += ImportAllConfigs;
		importHBox.AddChild(importAllBtn);
		
		vbox.AddChild(new HSeparator());
		
		// 路径信息
		var pathLabel = new Label();
		pathLabel.Text = $"Excel文件路径: {ProjectSettings.GlobalizePath("user://ExcelExports/")}";
		pathLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		vbox.AddChild(pathLabel);
		
		var openFolderBtn = new Button();
		openFolderBtn.Text = "打开导出文件夹";
		openFolderBtn.Pressed += OpenExportFolder;
		vbox.AddChild(openFolderBtn);
		
		EditorInterface.Singleton.GetBaseControl().AddChild(dialog);
		dialog.PopupCentered();
	}
	
	private void ExportConfig(string configType)
	{
		var configManager = new ConfigManager();
		configManager.ConfigDataPath = "res://ResourcesData/";
		configManager.ExcelExportPath = "user://ExcelExports/";
		configManager.ExportConfigToExcel(configType);
		
		GD.Print($"{configType} 配置已导出到Excel格式");
	}
	
	private void ImportConfig(string configType)
	{
		var configManager = new ConfigManager();
		configManager.ConfigDataPath = "res://ResourcesData/";
		configManager.ExcelExportPath = "user://ExcelExports/";
		configManager.ImportConfigFromExcel(configType);
		
		GD.Print($"{configType} 配置已从Excel导入");
	}
	
	private void ExportAllConfigs()
	{
		var configManager = new ConfigManager();
		configManager.ConfigDataPath = "res://ResourcesData/";
		configManager.ExcelExportPath = "user://ExcelExports/";
		configManager.ExportAllConfigs();
		
		GD.Print("所有配置已导出到Excel格式");
	}
	
	private void ImportAllConfigs()
	{
		var configManager = new ConfigManager();
		configManager.ConfigDataPath = "res://ResourcesData/";
		configManager.ExcelExportPath = "user://ExcelExports/";
		configManager.ImportAllConfigs();
		
		GD.Print("所有配置已从Excel导入");
	}
	
	private void OpenExportFolder()
	{
		var path = ProjectSettings.GlobalizePath("user://ExcelExports/");
		OS.ShellOpen(path);
	}
}
