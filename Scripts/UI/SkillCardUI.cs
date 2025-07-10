using Godot;
using System.Linq;

namespace CodeRogue.UI
{
    /// <summary>
    /// 技能卡片UI组件 - 显示单个技能卡片
    /// </summary>
    public partial class SkillCardUI : Control
    {
        private SkillCard _skillCard;
        private Label _nameLabel;
        private Label _descriptionLabel;
        private Label _costLabel;
        private Label _typeLabel;
        private Panel _cardPanel;
        private TextureRect _iconRect;
        private VBoxContainer _tagsContainer;
        
        [Signal] public delegate void CardClickedEventHandler(SkillCard card);
        [Signal] public delegate void CardRemovedEventHandler(SkillCard card);
        
        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
        }
        
        private void InitializeUI()
        {
            // 创建卡片面板
            _cardPanel = new Panel();
            // _cardPanel.SetAnchorsAndOffsetsPreset(Control.PresetMode.FullRect);
            AddChild(_cardPanel);
            
            // 创建主容器
            var mainContainer = new VBoxContainer();
            // mainContainer.SetAnchorsAndOffsetsPreset(Control.PresetMode.FullRect);
            mainContainer.AddThemeConstantOverride("separation", 5);
            _cardPanel.AddChild(mainContainer);
            
            // 卡片头部（图标和名称）
            var headerContainer = new HBoxContainer();
            mainContainer.AddChild(headerContainer);
            
            _iconRect = new TextureRect();
            _iconRect.CustomMinimumSize = new Vector2(32, 32);
            _iconRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            headerContainer.AddChild(_iconRect);
            
            _nameLabel = new Label();
            _nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            headerContainer.AddChild(_nameLabel);
            
            // 消耗和类型信息
            var infoContainer = new HBoxContainer();
            mainContainer.AddChild(infoContainer);
            
            _costLabel = new Label();
            _costLabel.Text = "消耗: 0";
            infoContainer.AddChild(_costLabel);
            
            _typeLabel = new Label();
            _typeLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _typeLabel.HorizontalAlignment = HorizontalAlignment.Right;
            infoContainer.AddChild(_typeLabel);
            
            // 描述
            _descriptionLabel = new Label();
            _descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _descriptionLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainContainer.AddChild(_descriptionLabel);
            
            // 标签容器
            _tagsContainer = new VBoxContainer();
            mainContainer.AddChild(_tagsContainer);
            
            // 移除按钮
            var removeButton = new Button();
            removeButton.Text = "移除";
            removeButton.CustomMinimumSize = new Vector2(60, 25);
            removeButton.Pressed += OnRemoveButtonPressed;
            mainContainer.AddChild(removeButton);
        }
        
        private void ConnectSignals()
        {
            _cardPanel.GuiInput += OnCardPanelInput;
        }
        
        public void SetSkillCard(SkillCard skillCard)
        {
            _skillCard = skillCard;
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (_skillCard == null) return;
            
            _nameLabel.Text = _skillCard.Name;
            _descriptionLabel.Text = _skillCard.Description;
            _costLabel.Text = $"消耗: {_skillCard.ChargeCost}";
            _typeLabel.Text = _skillCard.Type.ToString();
            
            // 设置稀有度颜色
            var rarityColor = GetRarityColor(_skillCard.Rarity);
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = rarityColor;
            styleBox.SetCornerRadiusAll(8);
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderColor = rarityColor.Lightened(0.3f);
            _cardPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            // 更新标签显示
            UpdateTagsDisplay();
            
            // 加载图标
            if (!string.IsNullOrEmpty(_skillCard.IconPath))
            {
                var texture = GD.Load<Texture2D>(_skillCard.IconPath);
                if (texture != null)
                {
                    _iconRect.Texture = texture;
                }
            }
        }
        
        private void UpdateTagsDisplay()
        {
            // 清除现有标签
            foreach (Node child in _tagsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (_skillCard.Tags == null) return;
            
            var tagsLabel = new Label();
            tagsLabel.Text = "标签: " + string.Join(", ", _skillCard.Tags.Select(tag => tag.Name));
            tagsLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            tagsLabel.AddThemeColorOverride("font_color", Colors.LightGray);
            _tagsContainer.AddChild(tagsLabel);
        }
        
        private Color GetRarityColor(SkillRarity rarity)
        {
            return rarity switch
            {
                SkillRarity.Common => new Color(0.8f, 0.8f, 0.8f, 0.3f),
                SkillRarity.Rare => new Color(0.3f, 0.6f, 1f, 0.3f),
                SkillRarity.Epic => new Color(0.8f, 0.3f, 1f, 0.3f),
                SkillRarity.Legendary => new Color(1f, 0.6f, 0.1f, 0.3f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
            };
        }
        
        private void OnCardPanelInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    EmitSignal(SignalName.CardClicked, _skillCard);
                }
            }
        }
        
        private void OnRemoveButtonPressed()
        {
            EmitSignal(SignalName.CardRemoved, _skillCard);
        }
    }
}