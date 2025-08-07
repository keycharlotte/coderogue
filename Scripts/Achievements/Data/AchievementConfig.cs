using Godot;
using Godot.Collections;
using CodeRogue.Achievements;

namespace CodeRogue.Achievements.Data
{
    /// <summary>
    /// 成就配置数据模型
    /// 继承自Resource，用于存储成就的基本配置信息
    /// </summary>
    [GlobalClass]
    public partial class AchievementConfig : Resource
    {
        /// <summary>成就唯一标识符</summary>
        [Export] public string Id { get; set; } = string.Empty;
        
        /// <summary>成就名称</summary>
        [Export] public string Name { get; set; } = string.Empty;
        
        /// <summary>成就标题（别名）</summary>
        public string Title => Name;
        
        /// <summary>成就描述</summary>
        [Export] public string Description { get; set; } = string.Empty;
        
        /// <summary>成就类型</summary>
        [Export] public AchievementType Type { get; set; } = AchievementType.Counter;
        
        /// <summary>成就分类</summary>
        [Export] public AchievementCategory Category { get; set; } = AchievementCategory.Progress;
        
        /// <summary>成就稀有度</summary>
        [Export] public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;
        
        /// <summary>目标值（完成条件的数值）</summary>
        [Export] public int TargetValue { get; set; } = 1;
        
        /// <summary>成就图标路径</summary>
        [Export] public string IconPath { get; set; } = string.Empty;
        
        /// <summary>奖励配置数组</summary>
        [Export] public RewardConfig[] Rewards { get; set; } = System.Array.Empty<RewardConfig>();
        
        /// <summary>前置成就ID数组</summary>
        [Export] public string[] Prerequisites { get; set; } = System.Array.Empty<string>();
        
        /// <summary>是否为隐藏成就</summary>
        [Export] public bool IsHidden { get; set; } = false;
        
        /// <summary>成就条件数组</summary>
        [Export] public AchievementCondition[] Conditions { get; set; } = System.Array.Empty<AchievementCondition>();
        
        /// <summary>条件逻辑类型（AND/OR/Sequence）</summary>
        [Export] public ConditionLogicType ConditionLogic { get; set; } = ConditionLogicType.And;
        
        /// <summary>成就点数</summary>
        [Export] public int Points { get; set; } = 10;
        
        /// <summary>成就点数（别名）</summary>
        public int AchievementPoints => Points;
        
        /// <summary>是否可重复完成</summary>
        [Export] public bool IsRepeatable { get; set; } = false;
        
        /// <summary>重置周期（天数，0表示不重置）</summary>
        [Export] public int ResetPeriodDays { get; set; } = 0;
        
        /// <summary>额外数据字典</summary>
        [Export] public Dictionary<string, Variant> ExtraData { get; set; } = new Dictionary<string, Variant>();
        
        /// <summary>
        /// 检查前置条件是否满足
        /// </summary>
        /// <param name="completedAchievements">已完成的成就ID列表</param>
        /// <returns>是否满足前置条件</returns>
        public bool CheckPrerequisites(string[] completedAchievements)
        {
            if (Prerequisites == null || Prerequisites.Length == 0)
                return true;
                
            foreach (var prerequisite in Prerequisites)
            {
                if (!System.Array.Exists(completedAchievements, id => id == prerequisite))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取成就的显示名称（考虑隐藏状态）
        /// </summary>
        /// <param name="isUnlocked">是否已解锁</param>
        /// <returns>显示名称</returns>
        public string GetDisplayName(bool isUnlocked)
        {
            if (IsHidden && !isUnlocked)
                return "???";
            return Name;
        }
        
        /// <summary>
        /// 获取成就的显示描述（考虑隐藏状态）
        /// </summary>
        /// <param name="isUnlocked">是否已解锁</param>
        /// <returns>显示描述</returns>
        public string GetDisplayDescription(bool isUnlocked)
        {
            if (IsHidden && !isUnlocked)
                return "隐藏成就，完成特定条件后解锁";
            return Description;
        }
        
        /// <summary>
        /// 获取成就稀有度对应的颜色
        /// </summary>
        /// <returns>稀有度颜色</returns>
        public Color GetRarityColor()
        {
            return Rarity switch
            {
                AchievementRarity.Common => Colors.White,
                AchievementRarity.Rare => Colors.LightBlue,
                AchievementRarity.Epic => Colors.Purple,
                AchievementRarity.Legendary => Colors.Orange,
                _ => Colors.White
            };
        }
        
        /// <summary>
        /// 验证成就配置的有效性
        /// </summary>
        /// <returns>验证结果和错误信息</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrEmpty(Id))
                return (false, "成就ID不能为空");
                
            if (string.IsNullOrEmpty(Name))
                return (false, "成就名称不能为空");
                
            if (TargetValue <= 0)
                return (false, "目标值必须大于0");
                
            if (Points < 0)
                return (false, "成就点数不能为负数");
                
            if (ResetPeriodDays < 0)
                return (false, "重置周期不能为负数");
                
            return (true, string.Empty);
        }
    }
}