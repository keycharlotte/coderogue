using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeRogue.Core;
using CodeRogue.Monsters;

/// <summary>
/// 打字战斗系统
/// 处理召唤师打字伤害和怪物协同伤害的计算
/// </summary>
[GlobalClass]
public partial class TypingCombatSystem : Node
{
    // 单例实例
    public static TypingCombatSystem Instance { get; private set; }
    
    [Signal] public delegate void DamageCalculatedEventHandler(float totalDamage, float typingDamage, float monsterDamage);
    [Signal] public delegate void SynergyTriggeredEventHandler(MonsterCard monster, float synergyBonus);
    [Signal] public delegate void TypingStatsUpdatedEventHandler(float wpm, float accuracy);
    
    // 打字统计
    [Export] public float CurrentWPM { get; private set; } = 0f;
    [Export] public float CurrentAccuracy { get; private set; } = 100f;
    [Export] public int TotalCharactersTyped { get; private set; } = 0;
    [Export] public int CorrectCharacters { get; private set; } = 0;
    
    // 充能系统
    [Export] public float CurrentCharge { get; private set; } = 0f;
    [Export] public float MaxCharge { get; set; } = 100f;
    
    // 伤害衰减参数
    [Export] public float BaseDecayRate { get; set; } = 0.02f; // 每个字符2%衰减
    [Export] public float MinDamageMultiplier { get; set; } = 0.3f; // 最小伤害倍数
    
    // 协同效果关键词映射
    private Godot.Collections.Dictionary<MagicColor, Godot.Collections.Array<string>> _colorKeywords;
    private Godot.Collections.Dictionary<MonsterRace, Godot.Collections.Array<string>> _raceKeywords;
    
    // 当前战斗状态
    private HeroInstance _currentSummoner;
    private SummonSystem _summonSystem;
    private float _combatStartTime;
    private List<TypingInput> _typingHistory = new List<TypingInput>();
    
    public override void _Ready()
    {
        // 设置单例实例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PrintErr("Multiple TypingCombatSystem instances detected!");
            QueueFree();
            return;
        }
        
        InitializeKeywords();
        _combatStartTime = (float)Time.GetUnixTimeFromSystem();
        
        // 获取召唤系统引用
        _summonSystem = GetNode<SummonSystem>("/root/SummonSystem");
        if (_summonSystem == null)
        {
            GD.PrintErr("SummonSystem not found! TypingCombatSystem requires SummonSystem.");
        }
    }
    
    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    /// <summary>
    /// 初始化颜色和种族关键词
    /// </summary>
    private void InitializeKeywords()
    {
        _colorKeywords = new Godot.Collections.Dictionary<MagicColor, Godot.Collections.Array<string>>
        {
            { MagicColor.White, new Godot.Collections.Array<string> { "light", "heal", "protect", "shield", "holy", "pure", "divine", "blessing" } },
            { MagicColor.Blue, new Godot.Collections.Array<string> { "water", "ice", "frost", "freeze", "flow", "wave", "ocean", "magic", "spell" } },
            { MagicColor.Black, new Godot.Collections.Array<string> { "dark", "shadow", "death", "curse", "poison", "decay", "drain", "fear" } },
            { MagicColor.Red, new Godot.Collections.Array<string> { "fire", "flame", "burn", "rage", "fury", "blood", "war", "strike", "attack" } },
            { MagicColor.Green, new Godot.Collections.Array<string> { "nature", "forest", "growth", "life", "earth", "plant", "wild", "beast" } }
        };
        
        _raceKeywords = new Godot.Collections.Dictionary<MonsterRace, Godot.Collections.Array<string>>
        {
            { MonsterRace.Human, new Godot.Collections.Array<string> { "human", "warrior", "knight", "soldier", "guard", "hero" } },
            { MonsterRace.Beast, new Godot.Collections.Array<string> { "beast", "wolf", "bear", "tiger", "lion", "wild", "feral", "hunt" } },
            { MonsterRace.Dragon, new Godot.Collections.Array<string> { "dragon", "drake", "wyrm", "scale", "wing", "roar", "ancient", "mighty" } },
            { MonsterRace.Elemental, new Godot.Collections.Array<string> { "elemental", "spirit", "essence", "energy", "force", "power" } },
            { MonsterRace.Undead, new Godot.Collections.Array<string> { "undead", "skeleton", "zombie", "ghost", "bone", "soul", "grave" } },
            { MonsterRace.Demon, new Godot.Collections.Array<string> { "demon", "devil", "fiend", "evil", "corrupt", "infernal", "hell" } },
            { MonsterRace.Angel, new Godot.Collections.Array<string> { "angel", "seraph", "celestial", "heaven", "divine", "sacred" } },
            { MonsterRace.Goblin, new Godot.Collections.Array<string> { "goblin", "orc", "troll", "small", "quick", "sneaky", "tribe" } }
        };
    }
    
    /// <summary>
    /// 设置当前召唤师
    /// </summary>
    public void SetCurrentSummoner(HeroInstance summoner)
    {
        _currentSummoner = summoner;
    }
    
    /// <summary>
    /// 处理打字输入并计算伤害
    /// </summary>
    public float ProcessTypingInput(string inputText, float inputTime)
    {
        if (string.IsNullOrEmpty(inputText))
            return 0f;
            
        // 记录打字输入
        var typingInput = new TypingInput
        {
            Text = inputText,
            Timestamp = inputTime,
            CharacterCount = inputText.Length
        };
        _typingHistory.Add(typingInput);
        
        // 更新打字统计
        UpdateTypingStats();
        
        // 计算召唤师打字伤害
        float typingDamage = CalculateTypingDamage(inputText);
        
        // 计算怪物协同伤害
        float monsterDamage = CalculateMonsterSynergyDamage(inputText);
        
        // 计算总伤害
        float totalDamage = typingDamage + monsterDamage;
        
        // 发送伤害计算信号
        EmitSignal(SignalName.DamageCalculated, totalDamage, typingDamage, monsterDamage);
        
        GD.Print($"Typing damage: {typingDamage:F1}, Monster damage: {monsterDamage:F1}, Total: {totalDamage:F1}");
        
        return totalDamage;
    }
    
    /// <summary>
    /// 计算召唤师打字伤害
    /// </summary>
    private float CalculateTypingDamage(string inputText)
    {
        if (_currentSummoner == null)
            return 0f;
            
        // 基础伤害
        float baseDamage = _currentSummoner.Config.TypingDamageBase;
        
        // 应用速度加成
        float speedBonus = 1f + (_currentSummoner.Config.TypingSpeedBonus * (CurrentWPM / 100f));
        
        // 应用准确率加成
        float accuracyBonus = 1f + (_currentSummoner.Config.TypingAccuracyBonus * (CurrentAccuracy / 100f));
        
        // 字符长度加成
        float lengthMultiplier = inputText.Length;
        
        // 伤害衰减（基于总打字字符数）
        float decayMultiplier = CalculateDamageDecay();
        
        // 召唤师技能加成
        float skillBonus = GetSummonerSkillBonus();
        
        float finalDamage = baseDamage * speedBonus * accuracyBonus * lengthMultiplier * decayMultiplier * (1f + skillBonus);
        
        return Mathf.Max(finalDamage, baseDamage * MinDamageMultiplier);
    }
    
    /// <summary>
    /// 计算怪物协同伤害
    /// </summary>
    private float CalculateMonsterSynergyDamage(string inputText)
    {
        if (_summonSystem == null)
            return 0f;
            
        var activeMonsters = _summonSystem.GetActiveMonsters();
        if (activeMonsters.Count == 0)
            return 0f;
            
        float totalSynergyDamage = 0f;
        string lowerInput = inputText.ToLower();
        
        foreach (var monster in activeMonsters)
        {
            float synergyBonus = CalculateMonsterSynergy(monster, lowerInput);
            if (synergyBonus > 0f)
            {
                float monsterBaseDamage = monster.Attack * 0.5f; // 怪物基础协同伤害为攻击力的50%
                float synergyDamage = monsterBaseDamage * synergyBonus;
                
                totalSynergyDamage += synergyDamage;
                
                // 发送协同触发信号
                EmitSignal(SignalName.SynergyTriggered, monster, synergyBonus);
            }
        }
        
        // 应用羁绊加成
        float bondBonus = _summonSystem.GetTotalBondBonus();
        totalSynergyDamage *= (1f + bondBonus);
        
        return totalSynergyDamage;
    }
    
    /// <summary>
    /// 计算单个怪物的协同效果
    /// </summary>
    private float CalculateMonsterSynergy(MonsterCard monster, string inputText)
    {
        float synergyBonus = 0f;
        
        // 颜色协同
        foreach (var color in monster.ColorRequirements)
        {
            if (_colorKeywords.ContainsKey(color))
            {
                foreach (var keyword in _colorKeywords[color])
                {
                    if (inputText.Contains(keyword))
                    {
                        synergyBonus += 0.2f; // 每个颜色需求20%加成
                    }
                }
            }
        }
        
        // 种族协同
        if (_raceKeywords.ContainsKey(monster.Race))
        {
            foreach (var keyword in _raceKeywords[monster.Race])
            {
                if (inputText.Contains(keyword))
                {
                    synergyBonus += 0.3f; // 种族匹配30%加成
                }
            }
        }
        
        // 怪物名称匹配
        if (inputText.Contains(monster.MonsterName.ToLower()))
        {
            synergyBonus += 0.5f; // 名称匹配50%加成
        }
        
        // 技能关键词匹配
        foreach (var skill in monster.Skills)
        {
            string skillName = skill.ToString().ToLower();
            if (inputText.Contains(skillName))
            {
                synergyBonus += 0.15f; // 技能匹配15%加成
            }
        }
        
        return Mathf.Min(synergyBonus, 2.0f); // 最大200%协同加成
    }
    
    /// <summary>
    /// 计算伤害衰减
    /// </summary>
    private float CalculateDamageDecay()
    {
        if (_currentSummoner == null)
            return 1f;
            
        float decayRate = BaseDecayRate * (1f - _currentSummoner.Config.TypingDecayResistance);
        float decay = 1f - (TotalCharactersTyped * decayRate);
        
        return Mathf.Max(decay, MinDamageMultiplier);
    }
    
    /// <summary>
    /// 获取召唤师技能加成
    /// </summary>
    private float GetSummonerSkillBonus()
    {
        if (_currentSummoner == null)
            return 0f;
            
        float bonus = 0f;
        
        // 打字强化技能
        if (_currentSummoner.HasSummonerSkill(SummonerSkillType.TypingEnhancement))
        {
            bonus += _currentSummoner.GetSummonerSkillValue(SummonerSkillType.TypingEnhancement);
        }
        
        // 打字精通技能
        if (_currentSummoner.HasSummonerSkill(SummonerSkillType.TypingMastery))
        {
            bonus += _currentSummoner.GetSummonerSkillValue(SummonerSkillType.TypingMastery);
        }
        
        return bonus;
    }
    
    /// <summary>
    /// 更新打字统计
    /// </summary>
    private void UpdateTypingStats()
    {
        if (_typingHistory.Count == 0)
            return;
            
        // 计算总字符数
        TotalCharactersTyped = _typingHistory.Sum(t => t.CharacterCount);
        
        // 计算WPM（基于最近60秒的输入）
        float currentTime = (float)Time.GetUnixTimeFromSystem();
        var recentInputs = _typingHistory.Where(t => currentTime - t.Timestamp <= 60f).ToList();
        
        if (recentInputs.Count > 0)
        {
            int recentCharacters = recentInputs.Sum(t => t.CharacterCount);
            float timeSpan = currentTime - recentInputs.First().Timestamp;
            
            if (timeSpan > 0)
            {
                CurrentWPM = (recentCharacters / 5f) / (timeSpan / 60f); // 假设平均每个单词5个字符
            }
        }
        
        // 简化的准确度计算（实际应该基于错误输入）
        CurrentAccuracy = Mathf.Max(90f, 100f - (TotalCharactersTyped * 0.01f));
        
        // 发送统计更新信号
        EmitSignal(SignalName.TypingStatsUpdated, CurrentWPM, CurrentAccuracy);
    }
    
    /// <summary>
    /// 重置打字统计
    /// </summary>
    public void ResetTypingStats()
    {
        _typingHistory.Clear();
        TotalCharactersTyped = 0;
        CorrectCharacters = 0;
        CurrentWPM = 0f;
        CurrentAccuracy = 100f;
        _combatStartTime = (float)Time.GetUnixTimeFromSystem();
        
        GD.Print("Typing stats reset");
    }
    
    /// <summary>
    /// 获取当前伤害占比
    /// </summary>
    public Godot.Collections.Dictionary<string, float> GetDamageRatio()
    {
        // 基于游戏进程计算伤害占比
        float gameProgress = Mathf.Min(TotalCharactersTyped / 1000f, 1f); // 1000字符为满进度
        
        float typingRatio = Mathf.Lerp(0.8f, 0.3f, gameProgress); // 从80%降到30%
        float monsterRatio = 1f - typingRatio;
        
        return new Godot.Collections.Dictionary<string, float>
        {
            { "typing", typingRatio },
            { "monster", monsterRatio }
        };
    }
    
    /// <summary>
    /// 获取打字历史
    /// </summary>
    public List<TypingInput> GetTypingHistory()
    {
        return new List<TypingInput>(_typingHistory);
    }
    
    /// <summary>
    /// 应用充能倍率效果
    /// </summary>
    public void ApplyChargeMultiplier(float multiplier, float duration)
    {
        // 这里可以实现充能倍率的逻辑
        // 例如：临时增加技能充能获取速度
        GD.Print($"Applied charge multiplier: {multiplier}x for {duration} seconds");
        
        // 可以使用Tween或Timer来实现持续时间效果
        // 这里先简单记录，具体实现可以根据需要扩展
    }
}