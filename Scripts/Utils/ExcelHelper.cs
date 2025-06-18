using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class ExcelHelper
{
	// 简单的CSV格式处理（Excel可以导出为CSV）
	public static List<Dictionary<string, string>> ReadCsvFile(string filePath)
	{
		var result = new List<Dictionary<string, string>>();
		
		if (!File.Exists(filePath))
		{
			GD.PrintErr($"File not found: {filePath}");
			return result;
		}
		
		try
		{
			var lines = File.ReadAllLines(filePath, Encoding.UTF8);
			if (lines.Length < 2) return result;
			
			// 第一行作为表头
			var headers = ParseCsvLine(lines[0]);
			
			// 从第二行开始解析数据
			for (int i = 1; i < lines.Length; i++)
			{
				var values = ParseCsvLine(lines[i]);
				if (values.Count == 0) continue;
				
				var row = new Dictionary<string, string>();
				for (int j = 0; j < Math.Min(headers.Count, values.Count); j++)
				{
					row[headers[j]] = values[j];
				}
				result.Add(row);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error reading CSV file: {ex.Message}");
		}
		
		return result;
	}
	
	public static void WriteCsvFile(string filePath, List<Dictionary<string, string>> data, List<string> headers = null)
	{
		if (data.Count == 0) return;
		
		try
		{
			// 如果没有指定表头，使用第一行数据的键
			if (headers == null)
			{
				headers = new List<string>(data[0].Keys);
			}
			
			var lines = new List<string>();
			
			// 写入表头
			lines.Add(string.Join(",", headers.ConvertAll(h => EscapeCsvValue(h))));
			
			// 写入数据行
			foreach (var row in data)
			{
				var values = new List<string>();
				foreach (var header in headers)
				{
					var value = row.ContainsKey(header) ? row[header] : "";
					values.Add(EscapeCsvValue(value));
				}
				lines.Add(string.Join(",", values));
			}
			
			File.WriteAllLines(filePath, lines, Encoding.UTF8);
			GD.Print($"CSV file saved: {filePath}");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error writing CSV file: {ex.Message}");
		}
	}
	
	private static List<string> ParseCsvLine(string line)
	{
		var result = new List<string>();
		var current = new StringBuilder();
		bool inQuotes = false;
		
		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			
			if (c == '"')
			{
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					// 转义的引号
					current.Append('"');
					i++;
				}
				else
				{
					inQuotes = !inQuotes;
				}
			}
			else if (c == ',' && !inQuotes)
			{
				result.Add(current.ToString());
				current.Clear();
			}
			else
			{
				current.Append(c);
			}
		}
		
		result.Add(current.ToString());
		return result;
	}
	
	private static string EscapeCsvValue(string value)
	{
		if (string.IsNullOrEmpty(value))
			return "";
			
		if (value.Contains(",") || value.Contains('"') || value.Contains('\n'))
		{
			return $"\"{value.Replace("\"", "\"\"")}\";";
		}
		
		return value;
	}
	
	// 将JSON转换为Excel友好的格式
	public static List<Dictionary<string, string>> JsonToExcelData(string jsonString, string configType)
	{
		var result = new List<Dictionary<string, string>>();
		
		try
		{
			var json = Json.ParseString(jsonString);
			var data = json.AsGodotDictionary();
			
			switch (configType.ToLower())
			{
				case "enemy":
					result = ConvertEnemyConfigToExcel(data);
					break;
				case "level":
					result = ConvertLevelConfigToExcel(data);
					break;
				default:
					GD.PrintErr($"Unknown config type: {configType}");
					break;
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error converting JSON to Excel data: {ex.Message}");
		}
		
		return result;
	}
	
	private static List<Dictionary<string, string>> ConvertEnemyConfigToExcel(Godot.Collections.Dictionary data)
	{
		var result = new List<Dictionary<string, string>>();
		
		foreach (var kvp in data)
		{
			var enemyId = kvp.Key.ToString();
			var enemyData = kvp.Value.AsGodotDictionary();
			
			var row = new Dictionary<string, string>
			{
				["ID"] = enemyId,
				["Name"] = enemyData.GetValueOrDefault("name", "").ToString(),
				["MaxHealth"] = enemyData.GetValueOrDefault("maxHealth", "100").ToString(),
				["AttackPower"] = enemyData.GetValueOrDefault("attackPower", "10").ToString(),
				["MoveSpeed"] = enemyData.GetValueOrDefault("moveSpeed", "50").ToString(),
				["AttackRange"] = enemyData.GetValueOrDefault("attackRange", "32").ToString(),
				["DetectionRange"] = enemyData.GetValueOrDefault("detectionRange", "64").ToString(),
				["SpritePath"] = enemyData.GetValueOrDefault("spritePath", "").ToString(),
				["ExperienceReward"] = enemyData.GetValueOrDefault("experienceReward", "10").ToString()
			};
			
			result.Add(row);
		}
		
		return result;
	}
	
	private static List<Dictionary<string, string>> ConvertLevelConfigToExcel(Godot.Collections.Dictionary data)
	{
		var result = new List<Dictionary<string, string>>();
		
		foreach (var levelKvp in data)
		{
			var levelId = levelKvp.Key.ToString();
			var levelData = levelKvp.Value.AsGodotDictionary();
			var waves = levelData["waves"].AsGodotArray();
			
			for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
			{
				var waveData = waves[waveIndex].AsGodotDictionary();
				var enemyTypes = waveData["enemyTypes"].AsGodotDictionary();
				
				var row = new Dictionary<string, string>
				{
					["LevelID"] = levelId,
					["WaveIndex"] = (waveIndex + 1).ToString(),
					["MaxEnemies"] = waveData.GetValueOrDefault("maxEnemies", "5").ToString(),
					["SpawnInterval"] = waveData.GetValueOrDefault("spawnInterval", "2.0").ToString(),
					["Duration"] = waveData.GetValueOrDefault("duration", "30.0").ToString(),
					["EnemyTypes"] = string.Join(";", enemyTypes.Select(et => $"{et.Key}:{et.Value}"))
				};
				
				result.Add(row);
			}
		}
		
		return result;
	}
	
	// 将Excel数据转换为JSON
	public static string ExcelDataToJson(List<Dictionary<string, string>> data, string configType)
	{
		try
		{
			// 使用通用方法处理所有配置类型
			return ConvertExcelToConfig(data, configType);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error converting Excel data to JSON: {ex.Message}");
			return "{}";
		}
	}

	// 通用的配置转换方法，使用反射或动态处理
	public static List<Dictionary<string, string>> ConvertConfigToExcel(Godot.Collections.Dictionary data, string configType)
	{
		var result = new List<Dictionary<string, string>>();
		
		foreach (var kvp in data)
		{
			var id = kvp.Key.ToString();
			var configData = kvp.Value.AsGodotDictionary();
			
			var row = new Dictionary<string, string>
			{
				["ID"] = id
			};
			
			// 动态处理所有属性
			foreach (var propertyKvp in configData)
			{
				var propertyName = propertyKvp.Key.ToString();
				var propertyValue = propertyKvp.Value.ToString() ?? "";
				
				// 将属性名转换为Excel列名（首字母大写）
				var columnName = ConvertToColumnName(propertyName);
				row[columnName] = propertyValue;
			}
			
			result.Add(row);
		}
		
		return result;
	}

	// 属性名转换为Excel列名的辅助方法
	private static string ConvertToColumnName(string propertyName)
	{
		if (string.IsNullOrEmpty(propertyName))
			return propertyName;
		
		// 将驼峰命名转换为Pascal命名（首字母大写）
		return char.ToUpper(propertyName[0]) + propertyName.Substring(1);
	}

	// 通用的Excel到配置的转换方法
	private static string ConvertExcelToConfig(List<Dictionary<string, string>> data, string configType)
	{
		var config = new Godot.Collections.Dictionary();
		
		foreach (var row in data)
		{
			if (!row.ContainsKey("ID") || string.IsNullOrEmpty(row["ID"]))
				continue;
				
			var id = row["ID"];
			var configData = new Godot.Collections.Dictionary();
			
			// 动态处理所有列（除了ID列）
			foreach (var columnKvp in row)
			{
				if (columnKvp.Key == "ID")
					continue;
					
				var columnName = columnKvp.Key;
				var columnValue = columnKvp.Value;
				
				// 将Excel列名转换回属性名（首字母小写）
				var propertyName = ConvertToPropertyName(columnName);
				
				// 尝试智能转换数据类型
				var convertedValue = ConvertValue(columnValue, propertyName);
				configData[propertyName] = convertedValue;
			}
			
			config[id] = configData;
		}
		
		return Json.Stringify(config);
	}

	// Excel列名转换为属性名的辅助方法
	private static string ConvertToPropertyName(string columnName)
	{
		if (string.IsNullOrEmpty(columnName))
			return columnName;
		
		// 将Pascal命名转换为驼峰命名（首字母小写）
		return char.ToLower(columnName[0]) + columnName.Substring(1);
	}

	// 智能类型转换方法
	private static object ConvertValue(string value, string propertyName)
	{
		if (string.IsNullOrEmpty(value))
			return "";
		
		// 根据属性名或值的特征进行类型推断
		var lowerPropertyName = propertyName.ToLower();
		
		// 整数类型的属性
		if (lowerPropertyName.Contains("health") || 
			lowerPropertyName.Contains("power") || 
			lowerPropertyName.Contains("reward") ||
			lowerPropertyName.Contains("max") ||
			lowerPropertyName.Contains("count") ||
			lowerPropertyName.Contains("level") ||
			lowerPropertyName.Contains("index"))
		{
			if (int.TryParse(value, out int intValue))
				return intValue;
		}
		
		// 浮点数类型的属性
		if (lowerPropertyName.Contains("speed") || 
			lowerPropertyName.Contains("range") ||
			lowerPropertyName.Contains("interval") ||
			lowerPropertyName.Contains("duration") ||
			lowerPropertyName.Contains("time"))
		{
			if (float.TryParse(value, out float floatValue))
				return floatValue;
		}
		
		// 布尔类型的属性
		if (lowerPropertyName.Contains("enable") || 
			lowerPropertyName.Contains("active") ||
			lowerPropertyName.Contains("visible"))
		{
			if (bool.TryParse(value, out bool boolValue))
				return boolValue;
		}
		
		// 默认返回字符串
		return value;
	}
}
