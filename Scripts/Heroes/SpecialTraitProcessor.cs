using Godot;
using Godot.Collections;

public partial class SpecialTraitProcessor : Node
{
    [Signal] public delegate void TraitTriggeredEventHandler(HeroInstance hero, SpecialTraitConfig trait);
    
    private Dictionary<int, ISpecialTraitHandler> _traitHandlers;
    
    public override void _Ready()
    {
        InitializeTraitHandlers();
    }
    
    private void InitializeTraitHandlers()
    {
        _traitHandlers = new Dictionary<int, ISpecialTraitHandler>();
        // TODO: 注册特性处理器
    }
    
    // 处理特性触发
    public void ProcessTrait(HeroInstance hero, SpecialTraitTrigger trigger, Variant context = default)
    {
        var trait = hero.Config.SpecialTrait;
        if (trait == null || trait.Trigger != trigger) return;
        
        if (_traitHandlers.TryGetValue(trait.Id, out var handler))
        {
            handler.Execute(hero, trait, context);
            EmitSignal(SignalName.TraitTriggered, hero, trait);
        }
    }
    
    // 获取特性描述
    public string GetTraitDescription(SpecialTraitConfig trait, int heroLevel)
    {
        var description = trait.Description;
        
        // 替换参数占位符
        foreach (var param in trait.Parameters)
        {
            var value = param.Value;
            if (trait.ScalesWithLevel)
            {
                value = CalculateScaledValue(value, heroLevel, trait.LevelScaling);
            }
            description = description.Replace($"{{{param.Key}}}", value.ToString());
        }
        
        return description;
    }
    
    private Variant CalculateScaledValue(Variant baseValue, int level, float scaling)
    {
        if (baseValue.VariantType == Variant.Type.Float)
        {
            return baseValue.AsSingle() * (1.0f + (level - 1) * scaling);
        }
        if (baseValue.VariantType == Variant.Type.Int)
        {
            return Mathf.RoundToInt(baseValue.AsInt32() * (1.0f + (level - 1) * scaling));
        }
        return baseValue;
    }
}