using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 召唤系统核心类
/// 管理怪物召唤、羁绊计算和战场状态
/// </summary>
[GlobalClass]
public partial class SummonSystem : Node
{
    [Signal] public delegate void MonsterSummonedEventHandler(MonsterCard monster, int position);
    [Signal] public delegate void MonsterReplacedEventHandler(MonsterCard oldMonster, MonsterCard newMonster, int position);
    [Signal] public delegate void BondsUpdatedEventHandler(Godot.Collections.Dictionary<BondType, int> activeBonds);
    [Signal] public delegate void SummonFailedEventHandler(string reason);
    
    // 召唤限制
    public const int MAX_SUMMONED_MONSTERS = 7;
    
    // 当前召唤状态
    [Export] public Array<MonsterCard> SummonedMonsters { get; private set; } = new Array<MonsterCard>();
    [Export] public Godot.Collections.Dictionary<BondType, int> ActiveBonds { get; private set; } = new Godot.Collections.Dictionary<BondType, int>();
    [Export] public int CurrentSummonPoints { get; set; } = 10; // 当前可用召唤点数
    [Export] public int MaxSummonPoints { get; set; } = 10; // 最大召唤点数
    
    // 当前召唤师
    private SummonerHero _currentSummoner;
    
    public override void _Ready()
    {
        SummonedMonsters = new Array<MonsterCard>();
        ActiveBonds = new Godot.Collections.Dictionary<BondType, int>();
        
        // 初始化空的召唤位
        for (int i = 0; i < MAX_SUMMONED_MONSTERS; i++)
        {
            SummonedMonsters.Add(null);
        }
    }
    
    /// <summary>
    /// 设置当前召唤师
    /// </summary>
    public void SetCurrentSummoner(SummonerHero summoner)
    {
        _currentSummoner = summoner;
        GD.Print($"Current summoner set to: {summoner?.HeroName}");
    }
    
    /// <summary>
    /// 检查是否可以召唤指定怪物
    /// </summary>
    public bool CanSummon(MonsterCard monster)
    {
        if (monster == null)
        {
            return false;
        }
        
        // 检查召唤点数
        if (CurrentSummonPoints < monster.SummonCost)
        {
            return false;
        }
        
        // 检查召唤师颜色需求
        if (_currentSummoner != null && !_currentSummoner.CanSummonMonster(monster))
        {
            return false;
        }
        
        // 检查是否有空位或可替换的位置
        return HasEmptySlot() || CanReplace();
    }
    
    /// <summary>
    /// 召唤怪物到指定位置
    /// </summary>
    public bool SummonMonster(MonsterCard monster, int position = -1)
    {
        if (!CanSummon(monster))
        {
            EmitSignal(SignalName.SummonFailed, "Cannot summon monster: requirements not met");
            return false;
        }
        
        // 如果没有指定位置，寻找第一个空位
        if (position == -1)
        {
            position = FindEmptySlot();
        }
        
        // 如果没有空位，替换最老的怪物
        if (position == -1)
        {
            position = 0; // 替换第一个位置
        }
        
        // 检查位置有效性
        if (position < 0 || position >= MAX_SUMMONED_MONSTERS)
        {
            EmitSignal(SignalName.SummonFailed, "Invalid summon position");
            return false;
        }
        
        // 应用召唤师加成
        var enhancedMonster = ApplySummonerBonus(monster);
        
        // 执行召唤
        var oldMonster = SummonedMonsters[position];
        SummonedMonsters[position] = enhancedMonster;
        
        // 消耗召唤点数
        CurrentSummonPoints -= monster.SummonCost;
        
        // 更新羁绊
        UpdateBonds();
        
        // 发送信号
        if (oldMonster == null)
        {
            EmitSignal(SignalName.MonsterSummoned, enhancedMonster, position);
        }
        else
        {
            EmitSignal(SignalName.MonsterReplaced, oldMonster, enhancedMonster, position);
        }
        
        GD.Print($"Summoned {enhancedMonster.MonsterName} at position {position}");
        return true;
    }
    
    /// <summary>
    /// 移除指定位置的怪物
    /// </summary>
    public bool RemoveMonster(int position)
    {
        if (position < 0 || position >= MAX_SUMMONED_MONSTERS || SummonedMonsters[position] == null)
        {
            return false;
        }
        
        var removedMonster = SummonedMonsters[position];
        SummonedMonsters[position] = null;
        
        // 更新羁绊
        UpdateBonds();
        
        GD.Print($"Removed {removedMonster.MonsterName} from position {position}");
        return true;
    }
    
    /// <summary>
    /// 应用召唤师加成到怪物
    /// </summary>
    private MonsterCard ApplySummonerBonus(MonsterCard monster)
    {
        if (_currentSummoner == null)
            return monster;
            
        var enhancedMonster = monster.CreateCopy() as MonsterCard;
        float bonus = _currentSummoner.GetSummonBonus(monster);
        
        // 应用属性加成
        enhancedMonster.Health = Mathf.RoundToInt(enhancedMonster.Health * (1f + bonus));
        enhancedMonster.Attack = Mathf.RoundToInt(enhancedMonster.Attack * (1f + bonus));
        
        return enhancedMonster;
    }
    
    /// <summary>
    /// 更新羁绊效果
    /// </summary>
    private void UpdateBonds()
    {
        ActiveBonds.Clear();
        
        var activeMonsters = SummonedMonsters.Where(m => m != null).ToList();
        
        // 计算各种羁绊
        CalculateBondEffects(activeMonsters);
        CalculateSameColorBonds(activeMonsters);
        
        EmitSignal(SignalName.BondsUpdated, ActiveBonds);
        
        // 输出调试信息
        if (ActiveBonds.Count > 0)
        {
            var bondInfo = string.Join(", ", ActiveBonds.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            GD.Print($"Active bonds: {bondInfo}");
        }
    }
    
    /// <summary>
    /// 计算特定羁绊效果
    /// </summary>
    private void CalculateBondEffects(List<MonsterCard> activeMonsters)
    {
        var allBondTypes = System.Enum.GetValues<BondType>().Where(b => b != BondType.SameColor);
        
        foreach (var bondType in allBondTypes)
        {
            int count = activeMonsters.Count(monster => monster.HasBondType(bondType));
            if (count >= 2) // 至少需要2个怪物才能激活羁绊
            {
                ActiveBonds[bondType] = count;
            }
        }
    }
    
    /// <summary>
    /// 计算同色羁绊
    /// </summary>
    private void CalculateSameColorBonds(List<MonsterCard> activeMonsters)
    {
        var colorGroups = activeMonsters
            .GroupBy(monster => monster.GetPrimaryColor())
            .Where(group => group.Count() >= 2);
            
        int maxSameColorCount = 0;
        foreach (var group in colorGroups)
        {
            maxSameColorCount = Mathf.Max(maxSameColorCount, group.Count());
        }
        
        if (maxSameColorCount >= 2)
        {
            ActiveBonds[BondType.SameColor] = maxSameColorCount;
        }
    }
    
    /// <summary>
    /// 寻找空的召唤位
    /// </summary>
    private int FindEmptySlot()
    {
        for (int i = 0; i < MAX_SUMMONED_MONSTERS; i++)
        {
            if (SummonedMonsters[i] == null)
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 检查是否有空位
    /// </summary>
    private bool HasEmptySlot()
    {
        return FindEmptySlot() != -1;
    }
    
    /// <summary>
    /// 检查是否可以替换（所有位置都被占用）
    /// </summary>
    private bool CanReplace()
    {
        return !HasEmptySlot();
    }
    
    /// <summary>
    /// 获取当前召唤的怪物数量
    /// </summary>
    public int GetSummonedCount()
    {
        return SummonedMonsters.Count(m => m != null);
    }
    
    /// <summary>
    /// 获取指定位置的怪物
    /// </summary>
    public MonsterCard GetMonsterAt(int position)
    {
        if (position >= 0 && position < MAX_SUMMONED_MONSTERS)
        {
            return SummonedMonsters[position];
        }
        return null;
    }
    
    /// <summary>
    /// 获取所有活跃怪物
    /// </summary>
    public List<MonsterCard> GetActiveMonsters()
    {
        return SummonedMonsters.Where(m => m != null).ToList();
    }
    
    /// <summary>
    /// 恢复召唤点数
    /// </summary>
    public void RestoreSummonPoints(int amount)
    {
        CurrentSummonPoints = Mathf.Min(CurrentSummonPoints + amount, MaxSummonPoints);
    }
    
    /// <summary>
    /// 设置最大召唤点数
    /// </summary>
    public void SetMaxSummonPoints(int maxPoints)
    {
        MaxSummonPoints = maxPoints;
        CurrentSummonPoints = Mathf.Min(CurrentSummonPoints, MaxSummonPoints);
    }
    
    /// <summary>
    /// 清空所有召唤的怪物
    /// </summary>
    public void ClearAllMonsters()
    {
        for (int i = 0; i < MAX_SUMMONED_MONSTERS; i++)
        {
            SummonedMonsters[i] = null;
        }
        
        ActiveBonds.Clear();
        EmitSignal(SignalName.BondsUpdated, ActiveBonds);
        
        GD.Print("All monsters cleared");
    }
    
    /// <summary>
    /// 获取羁绊效果的总加成
    /// </summary>
    public float GetTotalBondBonus()
    {
        float totalBonus = 0f;
        
        foreach (var bond in ActiveBonds)
        {
            // 基础羁绊加成：2怪物10%，3怪物20%，4+怪物30%
            float bondBonus = bond.Value switch
            {
                2 => 0.1f,
                3 => 0.2f,
                >= 4 => 0.3f,
                _ => 0f
            };
            
            totalBonus += bondBonus;
        }
        
        return totalBonus;
    }
}