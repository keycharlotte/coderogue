using Godot;
using Godot.Collections;

public partial class BuffInstance : RefCounted
{
    public string InstanceId { get; set; }
    public int ConfigId { get; set; }
    public BuffConfig Config { get; set; }
    public Node Target { get; set; }
    public Node Caster { get; set; }
    public int CurrentStack { get; set; } = 1;
    public float TotalDuration { get; set; }
    public float RemainingTime { get; set; }
    public double CreationTime { get; set; }
    public double LastUpdateTime { get; set; }
    public Array<BuffEffectInstance> EffectInstances { get; set; } = new();
    public Dictionary CustomData { get; set; } = new();
    
    public BuffInstance()
    {
        InstanceId = System.Guid.NewGuid().ToString();
        CreationTime = Time.GetUnixTimeFromSystem();
        LastUpdateTime = CreationTime;
    }
    
    public void Reset()
    {
        InstanceId = System.Guid.NewGuid().ToString();
        ConfigId = 0;
        Config = null;
        Target = null;
        Caster = null;
        CurrentStack = 1;
        TotalDuration = 0;
        RemainingTime = 0;
        CreationTime = Time.GetUnixTimeFromSystem();
        LastUpdateTime = CreationTime;
        EffectInstances.Clear();
        CustomData.Clear();
    }
    
    public bool IsExpired => RemainingTime <= 0;
    public float Progress => TotalDuration > 0 ? (TotalDuration - RemainingTime) / TotalDuration : 0;
}

public partial class BuffEffectInstance : RefCounted
{
    public BuffEffectData EffectData { get; set; }
    public float LastTriggerTime { get; set; }
    public float NextTriggerTime { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary EffectState { get; set; } = new();
}