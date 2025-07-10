using Godot;

// 特性处理器接口
public interface ISpecialTraitHandler
{
    void Execute(HeroInstance hero, SpecialTraitConfig trait, Variant context);
}