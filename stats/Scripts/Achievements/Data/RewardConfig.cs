using Godot;
using Godot.Collections;
using CodeRogue.Achievements;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 奖励配置数据模型
    /// 继承自Resource，用于存储奖励的配置信息
    /// </summary>
    [GlobalClass]
    public partial class RewardConfig : Resource
    {
        /// <summary>奖励类型</summary>
        [Export] public RewardType Type { get; set; } = RewardType.Experience;
        
        /// <summary>物品ID（用于特定物品奖励）</summary>
        [Export] public string ItemId { get; set; } = string.Empty;
        
        /// <summary>奖励数量</summary>
        [Export] public int Quantity { get; set; } = 1;
        
        /// <summary>奖励数量（Amount属性，用于兼容性）</summary>
        public int Amount => Quantity;
        
        /// <summary>奖励描述</summary>
        [Export] public string Description { get; set; } = string.Empty;
        
        /// <summary>奖励图标路径</summary>
        [Export] public string IconPath { get; set; } = string.Empty;
        
        /// <summary>是否为稀有奖励</summary>
        [Export] public bool IsRare { get; set; } = false;
        
        /// <summary>奖励权重（用于随机奖励）</summary>
        [Export] public float Weight { get; set; } = 1.0f;
        
        /// <summary>奖励参数字典</summary>
        [Export] public Dictionary<string, Variant> Parameters { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>
        /// 获取奖励的显示名称
        /// </summary>
        /// <returns>显示名称</returns>
        public string GetDisplayName()
        {
            return Type switch
            {
                RewardType.Experience => $"{Quantity} 经验值",
                RewardType.Gold => $"{Quantity} 金币",
                RewardType.SkillCard => GetItemDisplayName("技能卡"),
                RewardType.Hero => GetItemDisplayName("英雄"),
                RewardType.Relic => GetItemDisplayName("遗物"),
                RewardType.Title => GetItemDisplayName("称号"),
                RewardType.Avatar => GetItemDisplayName("头像"),
                RewardType.Badge => GetItemDisplayName("徽章"),
                RewardType.Unlock => GetItemDisplayName("解锁内容"),
                _ => Description
            };
        }
        
        /// <summary>
        /// 获取物品类型的显示名称
        /// </summary>
        /// <param name="itemTypeName">物品类型名称</param>
        /// <returns>显示名称</returns>
        private string GetItemDisplayName(string itemTypeName)
        {
            if (!string.IsNullOrEmpty(ItemId))
            {
                // 这里可以通过ItemId查询具体的物品名称
                // 暂时使用ItemId作为显示名称
                return Quantity > 1 ? $"{ItemId} x{Quantity}" : ItemId;
            }
            
            return Quantity > 1 ? $"{itemTypeName} x{Quantity}" : itemTypeName;
        }
        
        /// <summary>
        /// 获取奖励类型对应的颜色
        /// </summary>
        /// <returns>奖励类型颜色</returns>
        public Color GetTypeColor()
        {
            if (IsRare)
                return Colors.Gold;
                
            return Type switch
            {
                RewardType.Experience => Colors.LightBlue,
                RewardType.Gold => Colors.Yellow,
                RewardType.SkillCard => Colors.LightGreen,
                RewardType.Hero => Colors.Orange,
                RewardType.Relic => Colors.Purple,
                RewardType.Title => Colors.Pink,
                RewardType.Avatar => Colors.Cyan,
                RewardType.Badge => Colors.Red,
                RewardType.Unlock => Colors.White,
                _ => Colors.Gray
            };
        }
        
        /// <summary>
        /// 获取奖励的默认图标路径
        /// </summary>
        /// <returns>默认图标路径</returns>
        public string GetDefaultIconPath()
        {
            if (!string.IsNullOrEmpty(IconPath))
                return IconPath;
                
            return Type switch
            {
                RewardType.Experience => "res://Icons/Rewards/experience.png",
                RewardType.Gold => "res://Icons/Rewards/gold.png",
                RewardType.SkillCard => "res://Icons/Rewards/skill_card.png",
                RewardType.Hero => "res://Icons/Rewards/hero.png",
                RewardType.Relic => "res://Icons/Rewards/relic.png",
                RewardType.Title => "res://Icons/Rewards/title.png",
                RewardType.Avatar => "res://Icons/Rewards/avatar.png",
                RewardType.Badge => "res://Icons/Rewards/badge.png",
                RewardType.Unlock => "res://Icons/Rewards/unlock.png",
                _ => "res://Icons/Rewards/default.png"
            };
        }
        
        /// <summary>
        /// 检查奖励是否需要物品ID
        /// </summary>
        /// <returns>是否需要物品ID</returns>
        public bool RequiresItemId()
        {
            return Type switch
            {
                RewardType.SkillCard => true,
                RewardType.Hero => true,
                RewardType.Relic => true,
                RewardType.Title => true,
                RewardType.Avatar => true,
                RewardType.Badge => true,
                RewardType.Unlock => true,
                _ => false
            };
        }
        
        /// <summary>
        /// 获取奖励的优先级（用于排序）
        /// </summary>
        /// <returns>优先级值（越小优先级越高）</returns>
        public int GetPriority()
        {
            if (IsRare)
                return 0;
                
            return Type switch
            {
                RewardType.Hero => 1,
                RewardType.Relic => 2,
                RewardType.SkillCard => 3,
                RewardType.Title => 4,
                RewardType.Badge => 5,
                RewardType.Avatar => 6,
                RewardType.Unlock => 7,
                RewardType.Gold => 8,
                RewardType.Experience => 9,
                _ => 10
            };
        }
        
        /// <summary>
        /// 创建奖励的副本
        /// </summary>
        /// <returns>奖励配置的副本</returns>
        public RewardConfig Clone()
        {
            var clone = new RewardConfig
            {
                Type = Type,
                ItemId = ItemId,
                Quantity = Quantity,
                Description = Description,
                IconPath = IconPath,
                IsRare = IsRare,
                Weight = Weight,
                Parameters = new Dictionary<string, Variant>(Parameters)
            };
            
            return clone;
        }
        
        /// <summary>
        /// 检查奖励配置是否有效（简化版本）
        /// </summary>
        /// <returns>是否有效</returns>
        public bool IsValid()
        {
            var (isValid, _) = Validate();
            return isValid;
        }
        
        /// <summary>
        /// 验证奖励配置的有效性
        /// </summary>
        /// <returns>验证结果和错误信息</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (Quantity <= 0)
                return (false, "奖励数量必须大于0");
                
            if (Weight < 0)
                return (false, "奖励权重不能为负数");
                
            if (RequiresItemId() && string.IsNullOrEmpty(ItemId))
                return (false, $"奖励类型 {Type} 需要指定物品ID");
                
            return (true, string.Empty);
        }
        
        /// <summary>
        /// 转换为字典格式（用于序列化）
        /// </summary>
        /// <returns>字典格式的奖励数据</returns>
        public Dictionary<string, Variant> ToDictionary()
        {
            var dict = new Dictionary<string, Variant>
            {
                ["type"] = (int)Type,
                ["itemId"] = ItemId,
                ["quantity"] = Quantity,
                ["description"] = Description,
                ["iconPath"] = IconPath,
                ["isRare"] = IsRare,
                ["weight"] = Weight
            };
            
            if (Parameters.Count > 0)
            {
                dict["parameters"] = Parameters;
            }
            
            return dict;
        }
        
        /// <summary>
        /// 从字典格式创建奖励配置（用于反序列化）
        /// </summary>
        /// <param name="dict">字典格式的奖励数据</param>
        /// <returns>奖励配置对象</returns>
        public static RewardConfig FromDictionary(Dictionary<string, Variant> dict)
        {
            var reward = new RewardConfig();
            
            if (dict.TryGetValue("type", out var type))
                reward.Type = (RewardType)type.AsInt32();
                
            if (dict.TryGetValue("itemId", out var itemId))
                reward.ItemId = itemId.AsString();
                
            if (dict.TryGetValue("quantity", out var quantity))
                reward.Quantity = quantity.AsInt32();
                
            if (dict.TryGetValue("description", out var description))
                reward.Description = description.AsString();
                
            if (dict.TryGetValue("iconPath", out var iconPath))
                reward.IconPath = iconPath.AsString();
                
            if (dict.TryGetValue("isRare", out var isRare))
                reward.IsRare = isRare.AsBool();
                
            if (dict.TryGetValue("weight", out var weight))
                reward.Weight = weight.AsSingle();
                
            if (dict.TryGetValue("parameters", out var parameters))
                reward.Parameters = parameters.AsGodotDictionary<string, Variant>();
                
            return reward;
        }
    }
}