using Godot;

namespace CodeRogue.Heroes
{
    [GlobalClass]
    public abstract partial class SpecialTraitHandler : RefCounted
    {
        public abstract void Execute(HeroInstance hero, SpecialTraitConfig trait, Variant context);
    }
}