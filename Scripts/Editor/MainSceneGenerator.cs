using Godot;

#if TOOLS
namespace CodeRogue.Editor
{
	[Tool]
	public partial class MainSceneGenerator : EditorScript
	{
		public override void _Run()
		{
			CreateMainScene();
			GD.Print("MainSceneGenerator 运行完成");
		}
		
		// 添加菜单项方法
		public static void AddToMenu()
		{
			var editorInterface = Engine.GetSingleton("EditorInterface");
			var mainScreen = editorInterface.Call("get_editor_main_screen");
			
			// 这里可以添加自定义菜单项
		}
		
		private void CreateMainScene()
		{
			var scene = new PackedScene();
			var main = new Node2D();
			main.Name = "Main";
			
			// 游戏管理器
			var gameManager = new Node();
			gameManager.Name = "GameManager";
			var gmScript = GD.Load<CSharpScript>("res://Scripts/Core/GameManager.cs");
			gameManager.SetScript(gmScript);
			main.AddChild(gameManager);
			gameManager.Owner = main;
			
			// UI管理器
			var uiManager = new CanvasLayer();
			uiManager.Name = "UIManager";
			var uiScript = GD.Load<CSharpScript>("res://Scripts/UI/UIManager.cs");
			uiManager.SetScript(uiScript);
			main.AddChild(uiManager);
			uiManager.Owner = main;
			
			// 音频管理器
			var audioManager = new Node();
			audioManager.Name = "AudioManager";
			var audioScript = GD.Load<CSharpScript>("res://Scripts/Core/AudioManager.cs");
			audioManager.SetScript(audioScript);
			
			// 先将 AudioManager 添加到主节点
			main.AddChild(audioManager);
			audioManager.Owner = main;
			
			// 音乐播放器
			var musicPlayer = new AudioStreamPlayer();
			musicPlayer.Name = "MusicPlayer";
			audioManager.AddChild(musicPlayer);
			musicPlayer.Owner = main;  // 现在 main 是 musicPlayer 的祖先
			
			// 音效播放器
			var sfxPlayer = new AudioStreamPlayer();
			sfxPlayer.Name = "SFXPlayer";
			audioManager.AddChild(sfxPlayer);
			sfxPlayer.Owner = main;  // 现在 main 是 sfxPlayer 的祖先
			
			main.AddChild(audioManager);
			audioManager.Owner = main;
			
			// 保存场景
			scene.Pack(main);
			ResourceSaver.Save(scene, "res://Scenes/Main/Main.tscn");
			
			GD.Print("Main.tscn 创建完成");
		}
	}
}
#endif
