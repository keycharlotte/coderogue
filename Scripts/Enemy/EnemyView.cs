using Godot;

public partial class EnemyView : Node2D
{
    private Sprite2D _sprite;
    private Label _nameLabel;
    private ProgressBar _healthBar;
    private EnemyModel _model;
    
    public override void _Ready()
    {
        SetupVisualComponents();
    }
    
    private void SetupVisualComponents()
    {
        // 创建精灵
        _sprite = new Sprite2D();
        _sprite.Name = "Sprite";
        AddChild(_sprite);
        
        // 创建名称标签
        _nameLabel = new Label();
        _nameLabel.Name = "NameLabel";
        _nameLabel.Position = new Vector2(-20, -40);
        _nameLabel.Size = new Vector2(40, 20);
        _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _nameLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        AddChild(_nameLabel);
        
        // 创建血条
        _healthBar = new ProgressBar();
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
        
        if (_nameLabel != null)
        {
            _nameLabel.Text = model.Name;
        }
        
        if (_healthBar != null)
        {
            _healthBar.Value = model.GetHealthPercentage() * 100;
            _healthBar.Visible = model.CurrentHealth < model.MaxHealth;
        }
        
        Position = model.Position;
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