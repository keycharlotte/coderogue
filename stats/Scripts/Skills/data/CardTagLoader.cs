using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace CodeRogue.Skills.Data
{
public static class CardTagLoader
{
    private static Dictionary<string, CardTag> _cardTags = new Dictionary<string, CardTag>();
    
    public static void LoadCardTags()
    {
        _cardTags.Clear();
        
        // 加载所有CardTag JSON文件
        var dir = DirAccess.Open("res://ResourcesData/CardTag/");
        if (dir != null)
        {
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            
            while (fileName != "")
            {
                if (fileName.EndsWith(".json"))
                {
                    LoadCardTagFromJson($"res://ResourcesData/CardTag/{fileName}");
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }
    
    private static void LoadCardTagFromJson(string filePath)
    {
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file != null)
        {
            var jsonString = file.GetAsText();
            file.Close();
            
            try
            {
                var jsonData = JsonSerializer.Deserialize<CardTagData>(jsonString);
                var cardTag = new CardTag
                {
                    Name = jsonData.name,
                    Description = jsonData.description,
                    Color = new Color((float)jsonData.color.r, (float)jsonData.color.g, 
                                    (float)jsonData.color.b, (float)jsonData.color.a)
                };
                
                _cardTags[cardTag.Name] = cardTag;
                GD.Print($"Loaded CardTag: {cardTag.Name}");
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Failed to load CardTag from {filePath}: {e.Message}");
            }
        }
    }
    
    public static CardTag GetCardTag(string name)
    {
        return _cardTags.TryGetValue(name, out var cardTag) ? cardTag : null;
    }
    
    public static Dictionary<string, CardTag> GetAllCardTags()
    {
        return new Dictionary<string, CardTag>(_cardTags);
    }
}

// JSON数据结构
public class CardTagData
{
    public string name { get; set; }
    public string description { get; set; }
    public ColorData color { get; set; }
}

public class ColorData
{
    public double r { get; set; }
    public double g { get; set; }
    public double b { get; set; }
    public double a { get; set; }
}
}