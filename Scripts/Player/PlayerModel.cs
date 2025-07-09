using Godot;

namespace CodeRogue.Player
{
    [System.Serializable]
    public partial class PlayerModel : RefCounted
    {
        [Export] public int MaxHealth { get; set; } = 100;
        [Export] public int CurrentHealth { get; set; } = 100;
        [Export] public float Speed { get; set; } = 300.0f;
        [Export] public int Level { get; set; } = 1;
        [Export] public int Experience { get; set; } = 0;
        [Export] public int ExperienceToNextLevel { get; set; } = 100;
        [Export] public Vector2 Position { get; set; } = Vector2.Zero;
        [Export] public bool IsAlive { get; set; } = true;
        [Export] public string SpritePath { get; set; } = "res://Art/AssetsTextures/player.webp";
        
        public PlayerModel()
        {
            CurrentHealth = MaxHealth;
        }
        
        public void TakeDamage(int damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            if (CurrentHealth <= 0)
            {
                IsAlive = false;
            }
        }
        
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }
        
        public float GetHealthPercentage()
        {
            return MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0.0f;
        }
        
        public void GainExperience(int exp)
        {
            Experience += exp;
            CheckLevelUp();
        }
        
        private void CheckLevelUp()
        {
            while (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.2f); // 每级增加20%经验需求
                
                // 升级时增加最大生命值
                MaxHealth += 10;
                CurrentHealth = MaxHealth; // 升级时回满血
            }
        }
        
        public void SetMaxHealth(int newMaxHealth)
        {
            MaxHealth = newMaxHealth;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }
    }
}