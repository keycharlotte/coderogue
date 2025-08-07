using CodeRogue.Core;
using Godot;
using CodeRogue.Utils;

public partial class EnemyView : Node2D
{
	[Export] public Sprite2D _sprite;
	// public Label _nameLabel;
	[Export] public RichTextLabel _wordLabel;  // 改为RichTextLabel支持富文本
	[Export] public ProgressBar _healthBar;
	private EnemyModel _model;
	private string _currentWord = "";
	
	public string CurrentWord 
	{ 
		get => _currentWord;
		set
		{
			_currentWord = value;
			UpdateWordDisplay(""); // 初始化时没有高亮
		}
	}
	
	public override void _Ready()
	{
		// SetupVisualComponents();
		AssignRandomWord();
	}
	
	private void SetupVisualComponents()
	{
		// 创建精灵
		// _sprite = new Sprite2D();
		_sprite.Name = "Sprite";
		AddChild(_sprite);
		
		// 创建单词标签（改为RichTextLabel）
		// _wordLabel = new RichTextLabel();
		_wordLabel.Name = "WordLabel";
		_wordLabel.Position = new Vector2(-30, -60);
		_wordLabel.Size = new Vector2(60, 20);
		_wordLabel.FitContent = true;
		_wordLabel.ScrollActive = false;
		_wordLabel.BbcodeEnabled = true;
		AddChild(_wordLabel);
		
		// // 创建名称标签
		// _nameLabel = new Label();
		// _nameLabel.Name = "NameLabel";
		// _nameLabel.Position = new Vector2(-20, -40);
		// _nameLabel.Size = new Vector2(40, 20);
		// _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		// _nameLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
		// AddChild(_nameLabel);
		
		// 创建血条
		// _healthBar = new ProgressBar();
		_healthBar.Name = "HealthBar";
		_healthBar.Position = new Vector2(-20, -25);
		_healthBar.Size = new Vector2(40, 8);
		_healthBar.ShowPercentage = false;
		_healthBar.Value = 100;
		
		// 设置血条样式
		var healthBarStyle = new StyleBoxFlat();
		healthBarStyle.BgColor = Colors.Red;
		healthBarStyle.CornerRadiusTopLeft = 2;
		healthBarStyle.CornerRadiusTopRight = 2;
		healthBarStyle.CornerRadiusBottomLeft = 2;
		healthBarStyle.CornerRadiusBottomRight = 2;
		_healthBar.AddThemeStyleboxOverride("fill", healthBarStyle);
		
		var healthBarBg = new StyleBoxFlat();
		healthBarBg.BgColor = Colors.DarkRed;
		healthBarBg.CornerRadiusTopLeft = 2;
		healthBarBg.CornerRadiusTopRight = 2;
		healthBarBg.CornerRadiusBottomLeft = 2;
		healthBarBg.CornerRadiusBottomRight = 2;
		_healthBar.AddThemeStyleboxOverride("background", healthBarBg);
		
		AddChild(_healthBar);
	}
	
	private void AssignRandomWord()
	{
		var wordManager = NodeUtils.GetWordManager(this);
			if (wordManager != null)
			{
				CurrentWord = wordManager.GetRandomWord();
		}
	}
	
	public bool UpdateWordHighlight(string currentInput)
	{
		return UpdateWordDisplay(currentInput);
	}
	
	private bool UpdateWordDisplay(string currentInput)
	{
		if (_wordLabel == null || string.IsNullOrEmpty(_currentWord)) return false;
		
		if (string.IsNullOrEmpty(currentInput))
		{
			// 没有输入时显示普通文本
			_wordLabel.Text = "[center][color=yellow]" + _currentWord + "[/color][/center]";
			return false;
		}
		else
		{
			// 检查是否匹配开头
			if (_currentWord.ToLower().StartsWith(currentInput.ToLower()))
			{
				// 匹配时高亮已输入部分
				string highlightedPart = _currentWord.Substring(0, currentInput.Length);
				string remainingPart = _currentWord.Substring(currentInput.Length);
				
				_wordLabel.Text = "[center][color=lime][b]" + highlightedPart + "[/b][/color][color=yellow]" + remainingPart + "[/color][/center]";
				return true;
			}
			else
			{
				// 不匹配时显示普通文本
				_wordLabel.Text = "[center][color=yellow]" + _currentWord + "[/color][/center]";
				return false;
			}
		}
	}
	
	public void OnWordMatched()
	{
		// 单词匹配成功时的效果
		// AssignRandomWord(); // 重新分配新单词
	}
	
	public void UpdateHealthBar(int currentHealth, int maxHealth)
	{
		if (_healthBar != null)
		{
			_healthBar.Value = (float)currentHealth / maxHealth * 100;
		}
	}
	
	public void UpdateView(EnemyModel model)
	{
		_model = model;
		
		if (_sprite != null && !string.IsNullOrEmpty(model.SpritePath))
		{
			var texture = GD.Load<Texture2D>(model.SpritePath);
			if (texture != null)
			{
				_sprite.Texture = texture;
			}
		}
		
		if (_healthBar != null)
		{
			_healthBar.Value = model.GetHealthPercentage() * 100;
			_healthBar.Visible = model.CurrentHealth < model.MaxHealth;
		}
		
		// EnemyView作为EnemyController的子节点，不需要设置绝对位置
	}
	
	public void PlayDamageEffect()
	{
		// 受伤效果
		var tween = CreateTween();
		tween.TweenProperty(_sprite, "modulate", Colors.Red, 0.1f);
		tween.TweenProperty(_sprite, "modulate", Colors.White, 0.1f);
	}
	
	public void PlayDeathEffect()
	{
		// 死亡效果
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}
	
	public void SetVisible(bool visible)
	{
		Visible = visible;
	}
}
