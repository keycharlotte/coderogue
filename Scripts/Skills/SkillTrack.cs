using Godot;

namespace CodeRogue.Skills
{
    public class SkillTrack
    {
        public int Index { get; set; }
        public SkillCard EquippedSkill { get; set; }
        public float CurrentCharge { get; set; }
        public float MaxCharge { get; set; }
        public TrackState State { get; set; }
    }

    public enum TrackState
    {
        Empty,
        Charging,
        Ready,
        Cooldown
    }
}