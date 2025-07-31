using Godot;
using CodeRogue.Skills;

namespace CodeRogue.Test
{
[GlobalClass]
public partial class SkillTestRunner : Control
{
    private SkillSystemTest _testSystem;
    private VBoxContainer _container;
    private Button _runTestButton;
    private RichTextLabel _outputLabel;
    
    public override void _Ready()
    {
        SetupUI();
        SetupTestSystem();
    }
    
    private void SetupUI()
    {
        // 创建UI容器
        _container = new VBoxContainer();
        AddChild(_container);
        
        // 创建标题
        var title = new Label();
        title.Text = "技能系统测试";
        title.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        _container.AddChild(title);
        
        // 创建运行测试按钮
        _runTestButton = new Button();
        _runTestButton.Text = "运行测试";
        _runTestButton.Pressed += OnRunTestPressed;
        _container.AddChild(_runTestButton);
        
        // 创建输出显示
        _outputLabel = new RichTextLabel();
        _outputLabel.CustomMinimumSize = new Vector2(600, 400);
        _outputLabel.BbcodeEnabled = true;
        _container.AddChild(_outputLabel);
        
        // 设置布局
        // _container.SetAnchorsAndOffsetsPreset(Control.PresetEnum.FullRect);
        _container.AddThemeConstantOverride("separation", 10);
    }
    
    private void SetupTestSystem()
    {
        _testSystem = new SkillSystemTest();
        _testSystem.RunTestsOnReady = false; // 手动控制测试运行
        _testSystem.VerboseOutput = true;
        AddChild(_testSystem);
    }
    
    private void OnRunTestPressed()
    {
        _outputLabel.Clear();
        _outputLabel.AppendText("[color=yellow]开始运行技能系统测试...[/color]\n\n");
        
        // 重定向GD.Print输出到我们的标签
        // var originalPrint = GD.Print;
        
        _testSystem.RunAllTests();
        
        _outputLabel.AppendText("\n[color=green]测试完成！[/color]");
    }
}
}