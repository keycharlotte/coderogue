using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class RelicInstance : RefCounted
{
    [Export] public string InstanceId { get; set; }         // 实例ID
    [Export] public int ConfigId { get; set; }              // 配置ID
    [Export] public RelicConfig Config { get; set; }        // 配置引用
    
    // 动态数据
    [Export] public double ObtainTime { get; set; }         // 获得时间
    [Export] public int TriggerCount { get; set; }          // 触发次数
    [Export] public double LastTriggerTime { get; set; }    // 最后触发时间
    [Export] public bool IsActive { get; set; } = true;     // 是否激活
    
    // 状态数据
    [Export] public Dictionary CustomData { get; set; }     // 自定义数据
    [Export] public Array<BuffInstance> GeneratedBuffs { get; set; } // 生成的Buff
    
    // 运行时属性
    public float Progress => Config != null && Config.Cooldown > 0 
        ? Math.Min(1.0f, (float)((Time.GetUnixTimeFromSystem() - LastTriggerTime) / Config.Cooldown))
        : 1.0f;
    
    public bool IsOnCooldown => Config != null && Config.Cooldown > 0 
        && (Time.GetUnixTimeFromSystem() - LastTriggerTime) < Config.Cooldown;
    
    public RelicInstance()
    {
        InstanceId = Guid.NewGuid().ToString();
        CustomData = new Dictionary();
        GeneratedBuffs = new Array<BuffInstance>();
        ObtainTime = Time.GetUnixTimeFromSystem();
    }
    
    public void ResetCooldown()
    {
        LastTriggerTime = 0;
    }
    
    public void SetCustomData(string key, Variant value)
    {
        CustomData[key] = value;
    }
    
    public T GetCustomData<T>(string key, T defaultValue = default(T))
    {
        if (CustomData.TryGetValue(key, out var value))
        {
            return value.As<T>();
        }
        return defaultValue;
    }
}