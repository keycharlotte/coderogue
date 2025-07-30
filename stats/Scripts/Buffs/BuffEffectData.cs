using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BuffEffectData : Resource
{
    [Export] public BuffEffectType EffectType { get; set; }
    [Export] public string TargetProperty { get; set; }
    [Export] public BuffValueType ValueType { get; set; }
    [Export] public float BaseValue { get; set; }
    [Export] public float PerStackValue { get; set; }
    [Export] public BuffCalculationType CalculationType { get; set; }
    [Export] public BuffTriggerTiming TriggerTiming { get; set; }
    [Export] public float TriggerInterval { get; set; }
    [Export] public string Formula { get; set; }
    [Export] public string EventName { get; set; }
    [Export] public string Condition { get; set; }
    [Export] public Dictionary CustomData { get; set; } = new();
}