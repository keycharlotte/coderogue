using Godot;
using System.Collections.Generic;
using System.IO;

public partial class ConfigManager : Node
{
	private static ConfigManager _instance;
	public static ConfigManager Instance => _instance;
	
	[Export] public string ConfigDataPath { get; set; } = "res://ResourcesData/";
	[Export] public string ExcelExportPath { get; set; } = "user://ExcelExports/";
	
	public override void _Ready()
	{
		_instance = this;
		
		// 确保导出目录存在
		var exportDir = ProjectSettings.GlobalizePath(ExcelExportPath);
		if (!Directory.Exists(exportDir))
		{
			Directory.CreateDirectory(exportDir);
		}
	}
	
	// 导出配置到Excel（CSV格式）
	public void ExportConfigToExcel(string configType)
	{
		string jsonPath = "";
		string excelPath = "";
		
		switch (configType.ToLower())
		{
			case "enemy":
				jsonPath = ConfigDataPath + "EnemyConfig.json";
				excelPath = ProjectSettings.GlobalizePath(ExcelExportPath + "EnemyConfig.csv");
				break;
			case "level":
				jsonPath = ConfigDataPath + "LevelConfig.json";
				excelPath = ProjectSettings.GlobalizePath(ExcelExportPath + "LevelConfig.csv");
				break;
			default:
				GD.PrintErr($"Unknown config type: {configType}");
				return;
		}
		
		if (!Godot.FileAccess.FileExists(jsonPath))
		{
			GD.PrintErr($"Config file not found: {jsonPath}");
			return;
		}
		
		try
		{
			var file = Godot.FileAccess.Open(jsonPath, Godot.FileAccess.ModeFlags.Read);
			var jsonString = file.GetAsText();
			file.Close();
			
			var excelData = ExcelHelper.JsonToExcelData(jsonString, configType);
			ExcelHelper.WriteCsvFile(excelPath, excelData);
			
			GD.Print($"Config exported to: {excelPath}");
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"Error exporting config: {ex.Message}");
		}
	}
	
	// 从Excel导入配置
	public void ImportConfigFromExcel(string configType, string excelFilePath = "")
	{
		if (string.IsNullOrEmpty(excelFilePath))
		{
			switch (configType.ToLower())
			{
				case "enemy":
					excelFilePath = ProjectSettings.GlobalizePath(ExcelExportPath + "EnemyConfig.csv");
					break;
				case "level":
					excelFilePath = ProjectSettings.GlobalizePath(ExcelExportPath + "LevelConfig.csv");
					break;
				default:
					GD.PrintErr($"Unknown config type: {configType}");
					return;
			}
		}
		
		if (!File.Exists(excelFilePath))
		{
			GD.PrintErr($"Excel file not found: {excelFilePath}");
			return;
		}
		
		try
		{
			var excelData = ExcelHelper.ReadCsvFile(excelFilePath);
			var jsonString = ExcelHelper.ExcelDataToJson(excelData, configType);
			
			string jsonPath = "";
			switch (configType.ToLower())
			{
				case "enemy":
					jsonPath = ProjectSettings.GlobalizePath(ConfigDataPath + "EnemyConfig.json");
					break;
				case "level":
					jsonPath = ProjectSettings.GlobalizePath(ConfigDataPath + "LevelConfig.json");
					break;
			}
			
			File.WriteAllText(jsonPath, jsonString);
			GD.Print($"Config imported from: {excelFilePath}");
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"Error importing config: {ex.Message}");
		}
	}
	
	// 批量导出所有配置
	public void ExportAllConfigs()
	{
		ExportConfigToExcel("enemy");
		ExportConfigToExcel("level");
		GD.Print("All configs exported to Excel format");
	}
	
	// 批量导入所有配置
	public void ImportAllConfigs()
	{
		ImportConfigFromExcel("enemy");
		ImportConfigFromExcel("level");
		GD.Print("All configs imported from Excel format");
	}
	
	// 获取导出路径
	public string GetExportPath()
	{
		return ProjectSettings.GlobalizePath(ExcelExportPath);
	}
}
