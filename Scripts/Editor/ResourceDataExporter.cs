using Godot;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

#if TOOLS
[Tool]
public partial class ResourceDataExporter : Node
{
    private const string EXPORT_PATH = "res://ExportedData/";
    
    public override void _Ready()
    {
        // 添加导出菜单项
        var editorInterface = Engine.GetSingleton("EditorInterface") as GodotObject;
        if (editorInterface != null)
        {
            var editorPlugin = GetParent() as EditorPlugin;
            if (editorPlugin != null)
            {
                editorPlugin.AddToolMenuItem("导出Resource数据到CSV", new Callable(this, nameof(ExportAllResourceData)));
                editorPlugin.AddToolMenuItem("从CSV导入Resource数据", new Callable(this, nameof(ImportAllResourceData)));
            }
        }
    }
    
    public override void _ExitTree()
    {
        var editorPlugin = GetParent() as EditorPlugin;
        if (editorPlugin != null)
        {
            editorPlugin.RemoveToolMenuItem("导出Resource数据到CSV");
            editorPlugin.RemoveToolMenuItem("从CSV导入Resource数据");
        }
        
    }
    
    private void ExportAllResourceData()
    {
        // 确保导出目录存在
        if (!DirAccess.DirExistsAbsolute(EXPORT_PATH))
        {
            DirAccess.MakeDirRecursiveAbsolute(EXPORT_PATH);
        }
        
        // 导出技能卡牌数据
        ExportSkillCards();
        
        // 导出技能标签数据
        ExportSkillTags();
        
        // 导出遗物配置数据
        ExportRelicConfigs();
        
        GD.Print("所有Resource数据已导出到: " + EXPORT_PATH);
    }
    
    private void ExportSkillCards()
    {
        var skillCards = LoadAllResourcesOfType<SkillCard>("res://ResourcesData/SkillCard/");
        var csv = new StringBuilder();
        
        // CSV头部
        csv.AppendLine("Id,Name,Description,Type,Rarity,ChargeCost,Level,IconPath,RarityColor_R,RarityColor_G,RarityColor_B,RarityColor_A");
        
        // 数据行
        foreach (var skill in skillCards)
        {
            csv.AppendLine($"{skill.Id},\"{EscapeCsv(skill.Name)}\",\"{EscapeCsv(skill.Description)}\",{(int)skill.Type},{(int)skill.Rarity},{skill.ChargeCost},{skill.Level},\"{skill.IconPath}\",{skill.RarityColor.R},{skill.RarityColor.G},{skill.RarityColor.B},{skill.RarityColor.A}");
        }
        
        File.WriteAllText(ProjectSettings.GlobalizePath(EXPORT_PATH + "SkillCards.csv"), csv.ToString(), Encoding.UTF8);
    }
    
    private void ExportSkillTags()
    {
        var skillTags = LoadAllResourcesOfType<SkillTag>("res://ResourcesData/SkillTag/");
        var csv = new StringBuilder();
        
        // CSV头部
        csv.AppendLine("Name,Description,Color_R,Color_G,Color_B,Color_A");
        
        // 数据行
        foreach (var tag in skillTags)
        {
            csv.AppendLine($"\"{EscapeCsv(tag.Name)}\",\"{EscapeCsv(tag.Description)}\",{tag.Color.R},{tag.Color.G},{tag.Color.B},{tag.Color.A}");
        }
        
        File.WriteAllText(ProjectSettings.GlobalizePath(EXPORT_PATH + "SkillTags.csv"), csv.ToString(), Encoding.UTF8);
    }
    
    private void ExportRelicConfigs()
    {
        var relicConfigs = LoadAllResourcesOfType<RelicConfig>("res://ResourcesData/Relics/");
        var csv = new StringBuilder();
        
        // CSV头部
        csv.AppendLine("Id,Name,Description,FlavorText,Rarity,Category,IconPath,TriggerType,TriggerChance,Cooldown,DropWeight,MinLevel,IsUnique,RarityColor_R,RarityColor_G,RarityColor_B,RarityColor_A");
        
        // 数据行
        foreach (var relic in relicConfigs)
        {
            csv.AppendLine($"{relic.Id},\"{EscapeCsv(relic.Name)}\",\"{EscapeCsv(relic.Description)}\",\"{EscapeCsv(relic.FlavorText)}\",{(int)relic.Rarity},{(int)relic.Category},\"{relic.IconPath}\",{(int)relic.TriggerType},{relic.TriggerChance},{relic.Cooldown},{relic.DropWeight},{relic.MinLevel},{relic.IsUnique},{relic.RarityColor.R},{relic.RarityColor.G},{relic.RarityColor.B},{relic.RarityColor.A}");
        }
        
        File.WriteAllText(ProjectSettings.GlobalizePath(EXPORT_PATH + "RelicConfigs.csv"), csv.ToString(), Encoding.UTF8);
    }
    
    private void ImportAllResourceData()
    {
        // 导入技能卡牌数据
        ImportSkillCards();
        
        // 导入技能标签数据
        ImportSkillTags();
        
        // 导入遗物配置数据
        ImportRelicConfigs();
        
        GD.Print("所有CSV数据已导入并更新Resource文件");
    }
    
    private void ImportSkillCards()
    {
        string csvPath = ProjectSettings.GlobalizePath(EXPORT_PATH + "SkillCards.csv");
        if (!File.Exists(csvPath)) return;
        
        var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
        if (lines.Length <= 1) return; // 只有头部或空文件
        
        for (int i = 1; i < lines.Length; i++) // 跳过头部
        {
            var values = ParseCsvLine(lines[i]);
            if (values.Length < 12) continue;
            
            var skillCard = new SkillCard()
            {
                Id = int.Parse(values[0]),
                Name = values[1],
                Description = values[2],
                Type = (SkillType)int.Parse(values[3]),
                Rarity = (SkillRarity)int.Parse(values[4]),
                ChargeCost = int.Parse(values[5]),
                Level = int.Parse(values[6]),
                IconPath = values[7],
                RarityColor = new Color(float.Parse(values[8]), float.Parse(values[9]), float.Parse(values[10]), float.Parse(values[11]))
            };
            
            // 保存为.tres文件
            string resourcePath = $"res://ResourcesData/SkillCard/{skillCard.Id:D2}_{skillCard.Name}.tres";
            ResourceSaver.Save(skillCard, resourcePath);
        }
    }
    
    private void ImportSkillTags()
    {
        string csvPath = ProjectSettings.GlobalizePath(EXPORT_PATH + "SkillTags.csv");
        if (!File.Exists(csvPath)) return;
        
        var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
        if (lines.Length <= 1) return;
        
        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            if (values.Length < 6) continue;
            
            var skillTag = new SkillTag()
            {
                Name = values[0],
                Description = values[1],
                Color = new Color(float.Parse(values[2]), float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]))
            };
            
            // 保存为.tres文件
            string resourcePath = $"res://ResourcesData/SkillTag/{skillTag.Name}.tres";
            ResourceSaver.Save(skillTag, resourcePath);
        }
    }
    
    private void ImportRelicConfigs()
    {
        string csvPath = ProjectSettings.GlobalizePath(EXPORT_PATH + "RelicConfigs.csv");
        if (!File.Exists(csvPath)) return;
        
        var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
        if (lines.Length <= 1) return;
        
        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            if (values.Length < 17) continue;
            
            var relicConfig = new RelicConfig()
            {
                Id = int.Parse(values[0]),
                Name = values[1],
                Description = values[2],
                FlavorText = values[3],
                Rarity = (RelicRarity)int.Parse(values[4]),
                Category = (RelicCategory)int.Parse(values[5]),
                IconPath = values[6],
                TriggerType = (RelicTriggerType)int.Parse(values[7]),
                TriggerChance = float.Parse(values[8]),
                Cooldown = float.Parse(values[9]),
                DropWeight = float.Parse(values[10]),
                MinLevel = int.Parse(values[11]),
                IsUnique = bool.Parse(values[12]),
                RarityColor = new Color(float.Parse(values[13]), float.Parse(values[14]), float.Parse(values[15]), float.Parse(values[16]))
            };
            
            // 保存为.tres文件
            string resourcePath = $"res://ResourcesData/Relics/{relicConfig.Id:D2}_{relicConfig.Name}.tres";
            ResourceSaver.Save(relicConfig, resourcePath);
        }
    }
    
    private List<T> LoadAllResourcesOfType<T>(string directory) where T : Resource
    {
        var resources = new List<T>();
        var dir = DirAccess.Open(directory);
        if (dir == null) return resources;
        
        dir.ListDirBegin();
        string fileName = dir.GetNext();
        
        while (fileName != "")
        {
            if (fileName.EndsWith(".tres"))
            {
                var resource = GD.Load<T>(directory + fileName);
                if (resource != null)
                {
                    resources.Add(resource);
                }
            }
            fileName = dir.GetNext();
        }
        
        return resources;
    }
    
    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
    
    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var currentField = new StringBuilder();
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    currentField.Append('\"');
                    i++; // 跳过下一个引号
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }
        
        result.Add(currentField.ToString());
        return result.ToArray();
    }
}
#endif