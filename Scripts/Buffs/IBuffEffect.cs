using Godot;

namespace CodeRogue.Buffs
{
    public interface IBuffEffect
    {
        void OnApply(BuffInstance buff);
        void OnUpdate(BuffInstance buff, float deltaTime);
        void OnRemove(BuffInstance buff);
        void OnStack(BuffInstance buff, int newStack);
    }
}