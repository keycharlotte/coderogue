using Godot;
using System;
using System.Linq;
using CodeRogue.Rebirth;
using CodeRogue.Rebirth.Data;
using System.Collections.Generic;

/// <summary>
/// 新颖度计算器
/// </summary>
public partial class NoveltyCalculator : Node
{
    private const float BASE_NOVELTY_BONUS = 0.1f;
    private const float MAX_NOVELTY_MULTIPLIER = 3.0f;
    private const int NOVELTY_DECAY_THRESHOLD = 5;
    
    [Signal] public delegate void NoveltyCalculatedEventHandler(float multiplier, NoveltyLevel level);
    
    /// <summary>
    /// 计算新颖度倍率
    /// </summary>
    public float CalculateNoveltyMultiplier(GameSession session, NoveltyDatabase noveltyDb)
    {
        string combinationHash = GenerateCombinationHash(session);
        var noveltyRecord = noveltyDb.GetNoveltyRecord(combinationHash);
        
        float multiplier;
        NoveltyLevel level;
        
        if (noveltyRecord == null)
        {
            // 全新组合，最高奖励
            multiplier = MAX_NOVELTY_MULTIPLIER;
            level = NoveltyLevel.Legendary;
        }
        else
        {
            // 根据使用次数计算衰减
            float decayFactor = CalculateDecayFactor(noveltyRecord.UsageCount);
            float timeBonus = CalculateTimeBonus(noveltyRecord.LastUsed);
            
            multiplier = 1.0f + (BASE_NOVELTY_BONUS * decayFactor * timeBonus);
            level = DetermineNoveltyLevel(noveltyRecord.UsageCount, timeBonus);
        }
        
        // 确保倍率在合理范围内
        multiplier = Mathf.Clamp(multiplier, 1.0f, MAX_NOVELTY_MULTIPLIER);
        
        EmitSignal(SignalName.NoveltyCalculated, multiplier, (int)level);
        
        return multiplier;
    }
    
    /// <summary>
    /// 生成组合哈希
    /// </summary>
    public string GenerateCombinationHash(GameSession session)
    {
        var cardIds = session.UsedCards.Select(c => c.CardId).OrderBy(id => id);
        var relicIds = session.UsedRelics.Select(r => r.RelicId).OrderBy(id => id);
        
        string combination = $"{session.HeroId}|{string.Join(",", cardIds)}|{string.Join(",", relicIds)}";
        return combination.GetHashCode().ToString();
    }
    
    /// <summary>
    /// 计算时间奖励
    /// </summary>
    private float CalculateTimeBonus(string lastUsedStr)
    {
        if (DateTime.TryParse(lastUsedStr, out var lastUsed))
        {
            var daysSinceLastUse = (DateTime.Now - lastUsed).TotalDays;
            return Mathf.Min(2.0f, 1.0f + (float)(daysSinceLastUse / 30.0)); // 每30天增加100%奖励，最高200%
        }
        return 1.0f;
    }
    
    /// <summary>
    /// 计算衰减系数
    /// </summary>
    private float CalculateDecayFactor(int usageCount)
    {
        return Mathf.Max(0.1f, 1.0f - (usageCount * 0.2f));
    }
    
    /// <summary>
    /// 确定新颖度等级
    /// </summary>
    private NoveltyLevel DetermineNoveltyLevel(int usageCount, float timeBonus)
    {
        if (usageCount == 0)
            return NoveltyLevel.Legendary;
            
        if (usageCount == 1 && timeBonus > 1.5f)
            return NoveltyLevel.High;
            
        if (usageCount <= 3)
            return NoveltyLevel.Medium;
            
        if (usageCount <= 7)
            return NoveltyLevel.Low;
            
        return NoveltyLevel.None;
    }
    
    /// <summary>
    /// 分析组合复杂度
    /// </summary>
    public float AnalyzeCombinationComplexity(GameSession session)
    {
        float complexity = 0f;
        
        // 卡牌数量复杂度
        complexity += session.UsedCards.Count * 0.1f;
        
        // 遗物数量复杂度
        complexity += session.UsedRelics.Count * 0.15f;
        
        // 卡牌使用效率复杂度
        float avgEffectiveness = session.UsedCards.Count > 0 
            ? session.UsedCards.Average(c => c.EffectivenessScore) 
            : 0f;
        complexity += avgEffectiveness * 0.2f;
        
        // 遗物影响力复杂度
        float avgImpact = session.UsedRelics.Count > 0 
            ? session.UsedRelics.Average(r => r.ImpactScore) 
            : 0f;
        complexity += avgImpact * 0.25f;
        
        return Mathf.Clamp(complexity, 0f, 2f);
    }
    
    /// <summary>
    /// 计算协同效应奖励
    /// </summary>
    public float CalculateSynergyBonus(GameSession session)
    {
        // 这里可以根据具体的卡牌和遗物组合来计算协同效应
        // 暂时使用简单的计算方式
        
        float synergyBonus = 1.0f;
        
        // 卡牌与遗物的协同
        if (session.UsedCards.Count > 0 && session.UsedRelics.Count > 0)
        {
            synergyBonus += 0.1f;
        }
        
        // 多样性奖励
        if (session.UsedCards.Count >= 5)
        {
            synergyBonus += 0.05f;
        }
        
        if (session.UsedRelics.Count >= 3)
        {
            synergyBonus += 0.05f;
        }
        
        return synergyBonus;
    }
    
    /// <summary>
    /// 获取新颖度描述
    /// </summary>
    public string GetNoveltyDescription(NoveltyLevel level, float multiplier)
    {
        string levelDesc = level switch
        {
            NoveltyLevel.Legendary => "传奇",
            NoveltyLevel.High => "高度新颖",
            NoveltyLevel.Medium => "中等新颖",
            NoveltyLevel.Low => "略有新意",
            NoveltyLevel.None => "常见",
            _ => "未知"
        };
        
        return $"{levelDesc}组合 (x{multiplier:F2})";
    }
    
    /// <summary>
    /// 预测组合潜力
    /// </summary>
    public CombinationRating PredictCombinationPotential(GameSession session)
    {
        float score = 0f;
        
        // 基于胜利结果
        if (session.IsVictory)
            score += 30f;
            
        // 基于到达层数
        score += session.FloorsReached * 2f;
        
        // 基于击败敌人数
        score += session.EnemiesDefeated * 0.5f;
        
        // 基于组合复杂度
        score += AnalyzeCombinationComplexity(session) * 10f;
        
        return score switch
        {
            >= 80f => CombinationRating.Perfect,
            >= 60f => CombinationRating.Excellent,
            >= 40f => CombinationRating.Good,
            >= 20f => CombinationRating.Fair,
            _ => CombinationRating.Poor
        };
    }
    
    /// <summary>
    /// 获取新颖度历史
    /// </summary>
    public List<NoveltyRecord> GetNoveltyHistory()
    {
        var noveltyDb = GetNode<NoveltyDatabase>("/root/NoveltyDatabase");
        return noveltyDb?.GetNoveltyHistory() ?? new List<NoveltyRecord>();
    }
    
    /// <summary>
    /// 预测组合潜力
    /// </summary>
    public float PredictCombinationPotential(string heroId, List<string> cardIds, List<string> relicIds)
    {
        // 创建临时会话来计算潜力
        var tempSession = new GameSession
        {
            HeroId = heroId,
            UsedCards = new Godot.Collections.Array<UsedCard>(),
            UsedRelics = new Godot.Collections.Array<UsedRelic>()
        };
        
        // 添加卡牌
        foreach (var cardId in cardIds)
        {
            tempSession.UsedCards.Add(new UsedCard { CardId = cardId });
        }
        
        // 添加遗物
        foreach (var relicId in relicIds)
        {
            tempSession.UsedRelics.Add(new UsedRelic { RelicId = relicId });
        }
        
        // 计算复杂度作为潜力指标
        return AnalyzeCombinationComplexity(tempSession);
    }
}