using Godot;

[Tool]
public partial class SkillDeckUIGenerator : EditorScript
{
	public override void _Run()
	{
		GenerateSkillDeckUIScene();
	}
	
	private void GenerateSkillDeckUIScene()
	{
		var scene = new PackedScene();
		var root = new Control();
		root.Name = "SkillDeckUI";
		root.SetScript(GD.Load("res://Scripts/UI/SkillDeckUI.cs"));
		
		scene.Pack(root);
		
		var savePath = "res://Scenes/UI/SkillDeckUI.tscn";
		ResourceSaver.Save(scene, savePath);
		
		GD.Print($"技能卡组UI场景已生成: {savePath}");
	}
}
