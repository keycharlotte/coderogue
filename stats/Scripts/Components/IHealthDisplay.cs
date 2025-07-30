using Godot;

namespace CodeRogue.Components
{
    public interface IHealthDisplay
    {
        void UpdateHealthDisplay(int currentHealth, int maxHealth);
        void PlayDamageEffect();
        void PlayHealEffect();
    }
}