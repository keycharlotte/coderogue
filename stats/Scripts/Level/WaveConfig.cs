using Godot;

namespace CodeRogue.Level
{
    public class WaveConfig
    {
        public int MaxEnemies { get; set; } = 5;
        public float SpawnInterval { get; set; } = 2.0f;
        public float Duration { get; set; } = 30.0f;
        public Godot.Collections.Dictionary<int, float> EnemyTypes { get; set; } = new Godot.Collections.Dictionary<int, float>();
    }
}