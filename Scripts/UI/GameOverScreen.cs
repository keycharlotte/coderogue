using Godot;
using CodeRogue.Core;

namespace CodeRogue.UI
{
    /// <summary>
    /// 游戏结束界面
    /// </summary>
    public partial class GameOverScreen : Control
    {
        [Export]
        public Button RestartButton { get; set; }
        
        [Export]
        public Button MainMenuButton { get; set; }
        
        [Export]
        public Button QuitButton { get; set; }
        
        [Export]
        public Label ScoreLabel { get; set; }
        
        [Export]
        public Label HighScoreLabel { get; set; }
        
        public override void _Ready()
        {
            // 连接按钮信号
            if (RestartButton != null)
                RestartButton.Pressed += OnRestartPressed;
                
            if (MainMenuButton != null)
                MainMenuButton.Pressed += OnMainMenuPressed;
                
            if (QuitButton != null)
                QuitButton.Pressed += OnQuitPressed;
        }
        
        /// <summary>
        /// 显示游戏结束界面
        /// </summary>
        /// <param name="score">玩家得分</param>
        /// <param name="highScore">最高分</param>
        public void ShowGameOver(int score, int highScore)
        {
            Visible = true;
            
            if (ScoreLabel != null)
                ScoreLabel.Text = $"得分: {score}";
                
            if (HighScoreLabel != null)
                HighScoreLabel.Text = $"最高分: {highScore}";
        }
        
        /// <summary>
        /// 隐藏游戏结束界面
        /// </summary>
        public void HideGameOver()
        {
            Visible = false;
        }
        
        private void OnRestartPressed()
        {
            // 重新开始游戏
            var gameManager = GetNode<GameManager>("/root/GameManager");
            gameManager?.RestartGame();
            HideGameOver();
        }
        
        private void OnMainMenuPressed()
        {
            // 返回主菜单
            var gameManager = GetNode<GameManager>("/root/GameManager");
            gameManager?.ReturnToMainMenu();
            HideGameOver();
        }
        
        private void OnQuitPressed()
        {
            // 退出游戏
            GetTree().Quit();
        }

        
    }
}