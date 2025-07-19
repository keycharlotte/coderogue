using Godot;

namespace CodeRogue.Skills
{
    public class SkillTrack
    {
        private TrackState _state;
        
        public int Index { get; set; }
        public SkillCard EquippedSkill { get; set; }
        public float CurrentCharge { get; set; }
        public float MaxCharge { get; set; }
        
        public TrackState State 
        { 
            get => _state;
            set 
            {
                var previousState = _state;
                _state = value;
                
                // 记录状态变化
                if (previousState != value)
                {
                    GD.Print($"轨道 {Index} 状态变化: {previousState} → {value}");
                }
            }
        }
    }

    public enum TrackState
    {
        Empty,
        Charging,
        Ready,
        Cooldown
    }
}