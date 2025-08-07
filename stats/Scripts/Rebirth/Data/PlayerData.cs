using Godot;
using System;
using CodeRogue.Rebirth;

namespace CodeRogue.Rebirth.Data
{
    /// <summary>
    /// 玩家数据
    /// </summary>
    [System.Serializable]
    public partial class PlayerData : Resource
{
    [Export] public string PlayerId { get; set; } = System.Guid.NewGuid().ToString();
    [Export] public string PlayerName { get; set; }
    [Export] public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public string LastLogin { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Export] public int TotalSessions { get; set; }
    [Export] public int TotalVictories { get; set; }
    [Export] public int PlayerLevel { get; set; } = 1;
    [Export] public int Experience { get; set; }
    [Export] public int BestFloorReached { get; set; }
    [Export] public float TotalPlayTime { get; set; } // 总游戏时间（小时）
    [Export] public int TotalCurrencyEarned { get; set; }
    [Export] public int TotalCurrencySpent { get; set; }
    [Export] public Godot.Collections.Array<string> UnlockedCards { get; set; } = new();
    [Export] public Godot.Collections.Array<string> UnlockedHeroes { get; set; } = new();
    [Export] public Godot.Collections.Array<string> UnlockedRelics { get; set; } = new();
    [Export] public Godot.Collections.Dictionary<string, int> Statistics { get; set; } = new();
    
    /// <summary>
    /// 更新登录时间
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// 添加游戏会话
    /// </summary>
    public void AddGameSession(bool isVictory, int floorsReached, float playTime, int currencyEarned)
    {
        TotalSessions++;
        
        if (isVictory)
        {
            TotalVictories++;
        }
        
        if (floorsReached > BestFloorReached)
        {
            BestFloorReached = floorsReached;
        }
        
        TotalPlayTime += playTime;
        TotalCurrencyEarned += currencyEarned;
        
        // 增加经验值
        AddExperience(CalculateExperienceGain(isVictory, floorsReached));
    }
    
    /// <summary>
    /// 计算经验值获得
    /// </summary>
    private int CalculateExperienceGain(bool isVictory, int floorsReached)
    {
        int baseExp = floorsReached * 10;
        int victoryBonus = isVictory ? 100 : 0;
        return baseExp + victoryBonus;
    }
    
    /// <summary>
    /// 添加经验值
    /// </summary>
    /// <returns>是否升级了</returns>
    public bool AddExperience(int exp)
    {
        Experience += exp;
        return CheckLevelUp();
    }
    
    /// <summary>
    /// 检查升级
    /// </summary>
    /// <returns>是否升级了</returns>
    private bool CheckLevelUp()
    {
        int requiredExp = GetRequiredExperience(PlayerLevel);
        bool leveledUp = false;
        
        while (Experience >= requiredExp)
        {
            Experience -= requiredExp;
            PlayerLevel++;
            leveledUp = true;
            requiredExp = GetRequiredExperience(PlayerLevel);
        }
        
        return leveledUp;
    }
    
    /// <summary>
    /// 获取升级所需经验
    /// </summary>
    public int GetRequiredExperience(int level)
    {
        return level * 1000 + (level - 1) * 500; // 递增的经验需求
    }
    
    /// <summary>
    /// 获取胜率
    /// </summary>
    public float GetWinRate()
    {
        return TotalSessions > 0 ? (float)TotalVictories / TotalSessions : 0f;
    }
    
    /// <summary>
    /// 获取平均游戏时长
    /// </summary>
    public float GetAveragePlayTime()
    {
        return TotalSessions > 0 ? TotalPlayTime / TotalSessions : 0f;
    }
    
    /// <summary>
    /// 解锁卡牌
    /// </summary>
    public bool UnlockCard(string cardId)
    {
        if (UnlockedCards.Contains(cardId))
            return false;
            
        UnlockedCards.Add(cardId);
        return true;
    }
    
    /// <summary>
    /// 解锁英雄
    /// </summary>
    public bool UnlockHero(string heroId)
    {
        if (UnlockedHeroes.Contains(heroId))
            return false;
            
        UnlockedHeroes.Add(heroId);
        return true;
    }
    
    /// <summary>
    /// 解锁遗物
    /// </summary>
    public bool UnlockRelic(string relicId)
    {
        if (UnlockedRelics.Contains(relicId))
            return false;
            
        UnlockedRelics.Add(relicId);
        return true;
    }
    
    /// <summary>
    /// 更新统计数据
    /// </summary>
    public void UpdateStatistic(string key, int value)
    {
        if (Statistics.ContainsKey(key))
        {
            Statistics[key] = value;
        }
        else
        {
            Statistics.Add(key, value);
        }
    }
    
    /// <summary>
    /// 获取统计数据
    /// </summary>
    public int GetStatistic(string key)
    {
        return Statistics.ContainsKey(key) ? Statistics[key] : 0;
    }
    
    /// <summary>
    /// 增加统计数据
    /// </summary>
    public void IncrementStatistic(string key, int increment = 1)
    {
        int currentValue = GetStatistic(key);
        UpdateStatistic(key, currentValue + increment);
    }
    
    /// <summary>
    /// 获取玩家等级进度
    /// </summary>
    public float GetLevelProgress()
    {
        int requiredExp = GetRequiredExperience(PlayerLevel);
        return requiredExp > 0 ? (float)Experience / requiredExp : 0f;
    }
}
}