using Godot;

[GlobalClass]
public abstract partial class SpecialTraitHandler : RefCounted
{
    public abstract void Execute(HeroInstance hero, SpecialTraitConfig trait, Variant context);
}

// 特性处理器接口
public interface ISpecialTraitHandler
{
    void Execute(HeroInstance hero, SpecialTraitConfig trait, Variant context);
}