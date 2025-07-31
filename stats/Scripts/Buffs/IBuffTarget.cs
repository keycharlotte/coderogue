using Godot;

namespace CodeRogue.Buffs
{
    public interface IBuffTarget
    {
        void OnBuffApplied(BuffInstance buff);
        void OnBuffRemoved(BuffInstance buff);
        float GetPropertyValue(string property);
        void SetPropertyValue(string property, float value);
    }
}