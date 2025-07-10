using Godot;
using System.IO;
using System.Text;

#if TOOLS
[Tool]
public partial class EnumReferenceGenerator : EditorPlugin
{
    public override void _EnterTree()
    {
        AddToolMenuItem("生成枚举值参考文档", new Callable(this, nameof(GenerateEnumReference)));
    }
    
    public override void _ExitTree()
    {
        RemoveToolMenuItem("生成枚举值参考文档");
    }
    
    private void GenerateEnumReference()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# 枚举值参考文档");
        sb.AppendLine();
        
        // SkillType枚举
        sb.AppendLine("## SkillType (技能类型)");
        var skillTypes = System.Enum.GetValues<SkillType>();
        for (int i = 0; i < skillTypes.Length; i++)
        {
            sb.AppendLine($"{i} = {skillTypes[i]}");
        }
        sb.AppendLine();
        
        // SkillRarity枚举
        sb.AppendLine("## SkillRarity (技能稀有度)");
        var skillRarities = System.Enum.GetValues<SkillRarity>();
        for (int i = 0; i < skillRarities.Length; i++)
        {
            sb.AppendLine($"{i} = {skillRarities[i]}");
        }
        sb.AppendLine();
        
        // RelicRarity枚举
        sb.AppendLine("## RelicRarity (遗物稀有度)");
        var relicRarities = System.Enum.GetValues<RelicRarity>();
        for (int i = 0; i < relicRarities.Length; i++)
        {
            sb.AppendLine($"{i} = {relicRarities[i]}");
        }
        sb.AppendLine();
        
        // RelicCategory枚举
        sb.AppendLine("## RelicCategory (遗物分类)");
        var relicCategories = System.Enum.GetValues<RelicCategory>();
        for (int i = 0; i < relicCategories.Length; i++)
        {
            sb.AppendLine($"{i} = {relicCategories[i]}");
        }
        sb.AppendLine();
        
        // RelicTriggerType枚举
        sb.AppendLine("## RelicTriggerType (遗物触发类型)");
        var triggerTypes = System.Enum.GetValues<RelicTriggerType>();
        for (int i = 0; i < triggerTypes.Length; i++)
        {
            sb.AppendLine($"{i} = {triggerTypes[i]}");
        }
        
        string filePath = ProjectSettings.GlobalizePath("res://ExportedData/EnumReference.md");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        
        GD.Print("枚举值参考文档已生成: " + filePath);
    }
}
#endif