using Godot;
using System;

[GlobalClass]
public partial class TypingCombatSystem : Node
{
    // 全局单例实例
    private static TypingCombatSystem _instance;
    public static TypingCombatSystem Instance => _instance;

    public int CurrentCharge { get; internal set; }

    [Signal]
    public delegate void ChargeGainedEventHandler(int amount, string source);
    
    [Signal]
    public delegate void ComboChangedEventHandler(int comboCount);
    
    [Signal]
    public delegate void SkillTriggeredEventHandler(string skillId);
    
    // 充能系统
    private int _currentCharge = 0;
    private int _maxCharge = 100;
    
    // 连击系统
    private int _comboCount = 0;
    private float _comboTimer = 0f;
    private float _comboTimeWindow = 3f;
    
    // 打字效率计算
    public struct TypingResult
    {
        public int BaseDamage;
        public int ChargeGain;
        public float EfficiencyMultiplier;
        public bool IsPerfectInput;
    }
    
    public override void _Ready()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            QueueFree();
        }
    }
    
    public TypingResult ProcessWordInput(string word, float inputTime)
    {
        var result = new TypingResult();
        
        // 基础伤害计算（受英雄属性影响）
        result.BaseDamage = CalculateBaseDamage(word.Length);
        
        // 充能获取（受卡组效率影响）
        result.ChargeGain = CalculateChargeGain(word.Length, inputTime);
        
        // 效率倍率（受遗物影响）
        result.EfficiencyMultiplier = CalculateEfficiencyMultiplier();
        
        // 完美输入判定
        result.IsPerfectInput = IsPerfectTyping(inputTime, word.Length);
        
        // 更新连击
        UpdateCombo(result.IsPerfectInput);
        
        return result;
    }

    private void UpdateCombo(bool isPerfectInput)
    {
        throw new NotImplementedException();
    }

    private bool IsPerfectTyping(float inputTime, int length)
    {
        throw new NotImplementedException();
    }

    private float CalculateEfficiencyMultiplier()
    {
        throw new NotImplementedException();
    }

    private int CalculateBaseDamage(int wordLength)
    {
        // 基础伤害 = 单词长度 × 英雄攻击力 × 连击倍率
        HeroInstance hero = HeroManager.Instance.GetActiveHero();
        float baseDamage = wordLength * hero.GetFinalStats().Attack;
        float comboMultiplier = 1f + (_comboCount * 0.1f); // 每连击+10%
        return Mathf.RoundToInt(baseDamage * comboMultiplier);
    }
    
    private int CalculateChargeGain(int wordLength, float inputTime)
    {
        // 基础充能 = 单词长度 + 速度奖励
        int baseCharge = wordLength;
        
        // 速度奖励（越快打字充能越多）
        float speedBonus = Mathf.Max(0, 2f - inputTime) * 2f;
        
        // 卡组充能效率加成
        float deckEfficiency = SkillDeckManager.Instance.GetChargeEfficiency();
        
        return Mathf.RoundToInt((baseCharge + speedBonus) * deckEfficiency);
    }

    internal void ApplyChargeMultiplier(float value, float duration)
    {
        throw new NotImplementedException();
    }
}