using Godot;

public interface IBuffTarget
{
    void OnBuffApplied(BuffInstance buff);
    void OnBuffRemoved(BuffInstance buff);
    float GetPropertyValue(string property);
    void SetPropertyValue(string property, float value);
}

public interface IBuffEffect
{
    void OnApply(BuffInstance buff);
    void OnUpdate(BuffInstance buff, float deltaTime);
    void OnRemove(BuffInstance buff);
    void OnStack(BuffInstance buff, int newStack);
}