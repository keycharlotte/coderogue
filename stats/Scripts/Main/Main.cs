using Godot;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        // 自动打开MainMenu界面
        var sceneTree = GetTree();
        if (sceneTree != null)
        {
            // 加载MainMenu场景
            var mainMenuScene = GD.Load<PackedScene>("res://Scenes/UI/MainMenu.tscn");
            if (mainMenuScene != null)
            {
                var mainMenuInstance = mainMenuScene.Instantiate();
                GetTree().Root.AddChild(mainMenuInstance);
                
                // 隐藏当前Main场景或切换到MainMenu
                // 这里选择切换场景的方式
                GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
            }
            else
            {
                GD.PrintErr("无法加载MainMenu场景文件");
            }
        }
    }
}