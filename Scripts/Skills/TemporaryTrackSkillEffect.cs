using Godot;
using CodeRogue.Buffs;

namespace CodeRogue.Skills
{
    /// <summary>
    /// 创建临时轨道的技能效果
    /// </summary>
    [GlobalClass]
    public partial class TemporaryTrackSkillEffect : SkillEffect
    {
        [Export] public TemporaryTrackDestroyCondition DestroyCondition { get; set; } = TemporaryTrackDestroyCondition.Timer;
        [Export] public float Duration { get; set; } = 10f;
        [Export] public string TriggerSkillId { get; set; } = "";
        [Export] public SkillCard TrackSkill { get; set; } // 要装载到临时轨道的技能
        [Export] public int BuffConfigId { get; set; } = 1001; // 临时轨道Buff的配置ID
        
        public void Execute(Node target = null)
        {
            // 获取BuffManager（通过场景树根节点访问）
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree?.Root == null)
            {
                GD.PrintErr("TemporaryTrackSkillEffect: 无法获取场景树");
                return;
            }
            
            var buffManager = sceneTree.Root.GetNode<BuffManager>("/root/BuffManager");
            if (buffManager == null)
            {
                GD.PrintErr("TemporaryTrackSkillEffect: 无法找到BuffManager");
                return;
            }
            
            // 应用临时轨道Buff到目标（通常是玩家）
            var targetNode = target ?? sceneTree.Root.GetNode("/root/Player"); // 默认应用到玩家
            if (targetNode != null)
            {
                // 通过BuffConfigId应用Buff
                var buffInstance = buffManager.ApplyBuff(BuffConfigId, targetNode);
                if (buffInstance != null)
                {
                    // 设置自定义数据
                    buffInstance.CustomData["DestroyCondition"] = (int)DestroyCondition;
                    buffInstance.CustomData["TriggerSkillId"] = TriggerSkillId;
                    buffInstance.CustomData["TrackSkill"] = TrackSkill;
                    buffInstance.RemainingTime = Duration;
                    buffInstance.TotalDuration = Duration;
                    
                    GD.Print($"TemporaryTrackSkillEffect: 已应用临时轨道Buff，持续时间: {Duration}秒");
                }
                else
                {
                    GD.PrintErr($"TemporaryTrackSkillEffect: 无法创建Buff实例，配置ID: {BuffConfigId}");
                }
            }
            else
            {
                GD.PrintErr("TemporaryTrackSkillEffect: 无法找到目标节点");
            }
        }
    }
}