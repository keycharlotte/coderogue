using Godot;
using CodeRogue.Skills;
using Godot.Collections;

namespace CodeRogue.Buffs
{
    /// <summary>
    /// 临时轨道Buff效果处理器
    /// </summary>
    public partial class TemporaryTrackBuffEffect : Node, IBuffEffect
    {
        private Dictionary<string, int> _temporaryTrackIndices = new();
        
        public void OnApply(BuffInstance buff)
        {
            if (buff.Config is not TemporaryTrackBuffConfig config)
            {
                GD.PrintErr("TemporaryTrackBuffEffect: Buff配置类型不匹配");
                return;
            }
            
            // 获取SkillTrackManager（通过场景树根节点访问）
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree?.Root == null)
            {
                GD.PrintErr("TemporaryTrackBuffEffect: 无法获取场景树");
                return;
            }
            
            var trackManager = sceneTree.Root.GetNode<SkillTrackManager>("/root/SkillTrackManager");
            if (trackManager == null)
            {
                GD.PrintErr("TemporaryTrackBuffEffect: 无法找到SkillTrackManager");
                return;
            }
            
            // 创建临时轨道
            int trackIndex = trackManager.AddTemporaryTrack(config.TrackSkill, config.DestroyCondition, config.BaseDuration, buff.InstanceId);
            
            if (trackIndex >= 0)
            {
                _temporaryTrackIndices[buff.InstanceId] = trackIndex;
                GD.Print($"TemporaryTrackBuffEffect: 已创建临时轨道，索引: {trackIndex}");
            }
            else
            {
                GD.PrintErr("TemporaryTrackBuffEffect: 创建临时轨道失败");
            }
        }
        
        public void OnUpdate(BuffInstance buff, float deltaTime)
        {
            // 临时轨道的更新由SkillTrackManager处理
        }
        
        public void OnRemove(BuffInstance buff)
        {
            // 移除临时轨道
            if (_temporaryTrackIndices.TryGetValue(buff.InstanceId, out int trackIndex))
            {
                // 获取SkillTrackManager（通过场景树根节点访问）
                var sceneTree = Engine.GetMainLoop() as SceneTree;
                if (sceneTree?.Root != null)
                {
                    var trackManager = sceneTree.Root.GetNode<SkillTrackManager>("/root/SkillTrackManager");
                    if (trackManager != null)
                    {
                        trackManager.RemoveTemporaryTrack(trackIndex);
                        GD.Print($"TemporaryTrackBuffEffect: 已移除临时轨道，索引: {trackIndex}");
                    }
                }
                _temporaryTrackIndices.Remove(buff.InstanceId);
            }
        }
        
        public void OnStack(BuffInstance buff, int newStack)
        {
            // 临时轨道不支持叠加
        }
    }
}