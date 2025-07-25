using Godot;
using System;
using System.Linq;

namespace CodeRogue.UI
{
	/// <summary>
	/// 技能卡片UI组件 - 显示单个技能卡片
	/// </summary>
	public partial class SkillCardUI : Control
	{
		private SkillCard _skillCard;
		[Export] private Label _nameLabel;
		[Export] private Label _descriptionLabel;
		[Export] private Label _costLabel;
		[Export] private Label _typeLabel;
		[Export] private Panel _cardPanel;
		[Export] public TextureRect _iconRect;
		[Export] private VBoxContainer _tagsContainer;
		
		[Signal] public delegate void CardClickedEventHandler(SkillCard card);
		[Signal] public delegate void CardRemoveRequestedEventHandler(SkillCard card);
		
		public override void _Ready()
		{
			InitializeUI();
			ConnectSignals();
		}
		
		private void InitializeUI()
		{
			// SetSkillCard();
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
			
			// 加载图标 - 优先使用直接引用的Icon
			if (_skillCard.IconPath != null)
			{
				// 也可以使用ResourceLoader
				GD.Print(_skillCard.IconPath);
				_iconRect.Texture = ResourceLoader.Load<Texture2D>(_skillCard.IconPath);
			}
			// 如果两者都为空，使用默认图标
			else
			{
				_iconRect.Texture = ResourceLoader.Load<Texture2D>("res://Art/AssetsTextures/default.png");
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
		
	}
}
