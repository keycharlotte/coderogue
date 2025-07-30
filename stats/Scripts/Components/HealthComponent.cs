using Godot;

namespace CodeRogue.Components
{
    /// <summary>
    /// 健康组件 - 处理生命值逻辑
    /// </summary>
    public partial class HealthComponent : Node2D
    {
        [Signal]
        public delegate void HealthChangedEventHandler(int newHealth, int maxHealth);
        
        [Signal]
        public delegate void DiedEventHandler();
        
        [Export]
        public int MaxHealth { get; set; } = 100;
        
        private int _currentHealth;
        
        public int CurrentHealth 
        {
            get => _currentHealth;
            private set
            {
                int oldHealth = _currentHealth;
                _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                
                if (oldHealth != _currentHealth)
                {
                    EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
                    
                    if (_currentHealth <= 0 && oldHealth > 0)
                    {
                        EmitSignal(SignalName.Died);
                    }
                }
            }
        }
        
        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
        }
        
        public void TakeDamage(int damage)
        {
            if (damage > 0)
            {
                CurrentHealth -= damage;
                GD.Print($"Took {damage} damage. Health: {CurrentHealth}/{MaxHealth}");
            }
        }
        
        public void Heal(int amount)
        {
            if (amount > 0)
            {
                CurrentHealth += amount;
                GD.Print($"Healed {amount}. Health: {CurrentHealth}/{MaxHealth}");
            }
        }
        
        public void SetMaxHealth(int newMaxHealth)
        {
            MaxHealth = newMaxHealth;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }
        
        public bool IsAlive()
        {
            return CurrentHealth > 0;
        }
        
        public float GetHealthPercentage()
        {
            return MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        }
    }
}