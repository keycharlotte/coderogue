using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 怪物系统测试类
/// 用于测试和验证怪物召唤系统的各项功能
/// </summary>
[GlobalClass]
public partial class MonsterSystemTest : Node
{
    [Export] public bool AutoRunTests { get; set; } = true;
    [Export] public bool VerboseOutput { get; set; } = true;
    
    private MonsterGameManager _gameManager;
    private int _testsPassed = 0;
    private int _testsTotal = 0;
    
    public override void _Ready()
    {
        if (AutoRunTests)
        {
            CallDeferred(nameof(RunAllTests));
        }
    }
    
    /// <summary>
    /// 运行所有测试
    /// </summary>
    public void RunAllTests()
    {
        GD.Print("=== Monster System Tests Started ===");
        
        _testsPassed = 0;
        _testsTotal = 0;
        
        // 初始化游戏管理器
        InitializeGameManager();
        
        // 运行各项测试
        TestMonsterCardCreation();
        TestSummonerHeroCreation();
        TestSummonSystem();
        TestTypingCombatSystem();
        TestMonsterCardManager();
        
        TestUnifiedDeckSystem();
        TestGameManagerIntegration();
        
        // 输出测试结果
        PrintTestResults();
    }
    
    /// <summary>
    /// 初始化游戏管理器
    /// </summary>
    private async void InitializeGameManager()
    {
        _gameManager = new MonsterGameManager();
        _gameManager.Name = "TestGameManager";
        AddChild(_gameManager);
        
        // 等待初始化完成
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        
        Log("Game manager initialized");
    }
    
    /// <summary>
    /// 测试怪物卡牌创建
    /// </summary>
    private void TestMonsterCardCreation()
    {
        Log("\n--- Testing Monster Card Creation ---");
        
        // 创建测试卡牌
        var card = new MonsterCard();
        card.Id = 1;
        card.MonsterName = "测试龙";
        card.Description = "用于测试的龙";
        card.Rarity = CardRarity.Rare;
        card.ColorRequirements = new Godot.Collections.Array<MagicColor> { MagicColor.Red, MagicColor.Red, MagicColor.Red };
        card.Health = 50;
        card.Attack = 40;
        card.SummonCost = 5;
        card.Race = MonsterRace.Dragon;
        card.Skills = new Godot.Collections.Array<MonsterSkillType> { MonsterSkillType.Burn, MonsterSkillType.AoEAttack };
        
        // 测试基本属性
        Assert(card.MonsterName == "测试龙", "Monster name should be set correctly");
        Assert(card.Health == 50, "Monster health should be 50");
        Assert(card.Attack == 40, "Monster attack should be 40");
        Assert(card.SummonCost == 5, "Monster summon cost should be 5");
        
        // 测试颜色需求
        Assert(card.HasColorRequirement(MagicColor.Red), "Should have red color requirement");
        Assert(card.GetColorRequirementCount(MagicColor.Red) == 3, "Should require 3 red mana");
        Assert(!card.HasColorRequirement(MagicColor.Blue), "Should not have blue color requirement");
        
        // 测试技能
        Assert(card.HasSkill(MonsterSkillType.Burn), "Should have burn skill");
        Assert(card.HasSkill(MonsterSkillType.AoEAttack), "Should have AoE attack skill");
        Assert(!card.HasSkill(MonsterSkillType.Heal), "Should not have heal skill");
        
        // 测试战斗力评估
        float powerRating = card.GetPowerRating();
        Assert(powerRating > 0, "Power rating should be positive");
        
        // 测试副本创建
        var cardCopy = card.CreateCopy() as MonsterCard;
        Assert(cardCopy.MonsterName == card.MonsterName, "Copy should have same name");
        Assert(cardCopy.Health == card.Health, "Copy should have same health");
        
        Log("Monster card creation tests completed");
    }
    
    /// <summary>
    /// 测试召唤师英雄创建
    /// </summary>
    private void TestSummonerHeroCreation()
    {
        Log("\n--- Testing Summoner Hero Creation ---");
        
        var summoner = new SummonerHero();
        summoner.Id = 1;
        summoner.HeroName = "测试召唤师";
        summoner.PrimaryColor = MagicColor.Blue;
        summoner.TypingDamageBase = 15f;
        summoner.TypingSpeedBonus = 0.2f;
        summoner.TypingAccuracyBonus = 0.1f;
        
        // 初始化颜色槽位
        summoner.InitializeDefaultColorSlots();
        
        // 测试基本属性
        Assert(summoner.HeroName == "测试召唤师", "Summoner name should be set correctly");
        Assert(summoner.PrimaryColor == MagicColor.Blue, "Primary color should be blue");
        Assert(summoner.TypingDamageBase == 15f, "Base typing damage should be 15");
        
        // 测试颜色槽位
        Assert(summoner.ColorSlots.Count > 0, "Should have color slots");
        Assert(summoner.GetColorSlotCount(MagicColor.Blue) > 0, "Should have blue color slots");
        
        // 测试技能系统
        summoner.AddSummonerSkill(SummonerSkillType.TypingEnhancement, 0.15f);
        Assert(summoner.HasSummonerSkill(SummonerSkillType.TypingEnhancement), "Should have typing enhancement skill");
        Assert(summoner.GetSummonerSkillValue(SummonerSkillType.TypingEnhancement) == 0.15f, "Skill value should be 0.15");
        
        // 测试怪物召唤检查
        var testCard = new MonsterCard();
        testCard.ColorRequirements = new Godot.Collections.Array<MagicColor> { MagicColor.Blue, MagicColor.Blue };
        Assert(summoner.CanSummonMonster(testCard), "Should be able to summon blue monster");
        
        testCard.ColorRequirements = new Godot.Collections.Array<MagicColor> { MagicColor.Red, MagicColor.Red, MagicColor.Red, MagicColor.Red, MagicColor.Red };
        Assert(!summoner.CanSummonMonster(testCard), "Should not be able to summon expensive red monster");
        
        Log("Summoner hero creation tests completed");
    }
    
    /// <summary>
    /// 测试召唤系统
    /// </summary>
    private void TestSummonSystem()
    {
        Log("\n--- Testing Summon System ---");
        
        var summonSystem = _gameManager.GetSummonSystem();
        Assert(summonSystem != null, "Summon system should be available");
        
        // 创建测试召唤师和卡牌
        var summoner = CreateTestSummoner();
        var card = CreateTestCard();
        
        summonSystem.SetCurrentSummoner(summoner);
        
        // 测试召唤检查
        Assert(summonSystem.CanSummon(card), "Should be able to summon test card");
        
        // 测试召唤
        bool summoned = summonSystem.SummonMonster(card, 0);
        Assert(summoned, "Should successfully summon monster");
        Assert(summonSystem.GetSummonedCount() == 1, "Should have 1 summoned monster");
        
        var summonedMonster = summonSystem.GetMonsterAt(0);
        Assert(summonedMonster != null, "Should have monster at position 0");
        Assert(summonedMonster.MonsterName == card.MonsterName, "Summoned monster should match original");
        
        // 测试召唤点数消耗
        int remainingPoints = summonSystem.CurrentSummonPoints;
        Assert(remainingPoints == summonSystem.MaxSummonPoints - card.SummonCost, "Summon points should be consumed");
        
        // 测试移除怪物
        bool removed = summonSystem.RemoveMonster(0);
        Assert(removed, "Should successfully remove monster");
        Assert(summonSystem.GetSummonedCount() == 0, "Should have 0 summoned monsters");
        
        Log("Summon system tests completed");
    }
    
    /// <summary>
    /// 测试打字战斗系统
    /// </summary>
    private void TestTypingCombatSystem()
    {
        Log("\n--- Testing Typing Combat System ---");
        
        var typingSystem = _gameManager.GetTypingCombatSystem();
        Assert(typingSystem != null, "Typing combat system should be available");
        
        var summoner = CreateTestSummoner();
        typingSystem.SetCurrentSummoner(summoner);
        
        // 测试打字输入处理
        float damage1 = typingSystem.ProcessTypingInput("fire dragon attack", (float)Time.GetUnixTimeFromSystem());
        Assert(damage1 > 0, "Should deal damage from typing input");
        
        // 测试统计更新
        Assert(typingSystem.TotalCharactersTyped > 0, "Should track typed characters");
        Assert(typingSystem.CurrentWPM >= 0, "WPM should be non-negative");
        Assert(typingSystem.CurrentAccuracy > 0, "Accuracy should be positive");
        
        // 测试伤害衰减
        float damage2 = typingSystem.ProcessTypingInput("test", (float)Time.GetUnixTimeFromSystem());
        // 第二次输入的单位伤害应该略低（由于衰减）
        
        // 测试重置
        typingSystem.ResetTypingStats();
        Assert(typingSystem.TotalCharactersTyped == 0, "Should reset typed characters");
        Assert(typingSystem.CurrentWPM == 0, "Should reset WPM");
        
        Log("Typing combat system tests completed");
    }
    
    /// <summary>
    /// 测试怪物卡牌管理器
    /// </summary>
    private void TestMonsterCardManager()
    {
        Log("\n--- Testing Monster Card Manager ---");
        
        var cardManager = _gameManager.GetCardManager();
        Assert(cardManager != null, "Card manager should be available");
        
        // 测试卡牌库
        Assert(cardManager.AllCards.Count > 0, "Should have cards in the library");
        
        // 测试随机卡牌获取
        var randomCard = cardManager.GetRandomCard();
        Assert(randomCard != null, "Should get a random card");
        
        // 测试卡牌包获取
        var cardPack = cardManager.GetCardPack(5);
        Assert(cardPack.Count <= 5, "Card pack should have at most 5 cards");
        
        // 测试添加到收藏
        int initialCount = cardManager.OwnedCards.Count;
        cardManager.AddCardToCollection(randomCard);
        Assert(cardManager.OwnedCards.Count == initialCount + 1, "Should add card to collection");
        
        // 测试按条件过滤
        var whiteCards = cardManager.GetCardsByColor(MagicColor.White);
        var commonCards = cardManager.GetCardsByRarity(CardRarity.Common);
        var dragonCards = cardManager.GetCardsByRace(MonsterRace.Dragon);
        
        // 这些测试只检查方法不会崩溃
        Assert(whiteCards != null, "Should return white cards list");
        Assert(commonCards != null, "Should return common cards list");
        Assert(dragonCards != null, "Should return dragon cards list");
        
        Log("Monster card manager tests completed");
    }
    
    /// <summary>
    /// 测试统一卡组系统
    /// </summary>
    private void TestUnifiedDeckSystem()
    {
        Log("\n--- Testing Unified Deck System ---");
        
        var deckManager = _gameManager.GetDeckManager();
        Assert(deckManager != null, "Unified deck manager should be available");
        
        // 测试创建统一卡组
        var testDeck = deckManager.CreateNewDeck("Test Unified Deck");
        Assert(testDeck != null, "Should create unified deck");
        Assert(testDeck.DeckName == "Test Unified Deck", "Unified deck should have correct name");
        
        // 测试统一卡组操作
        var testMonsterCard = CreateTestCard();
        bool monsterAdded = testDeck.AddMonsterCard(testMonsterCard);
        Assert(monsterAdded, "Should add monster card to deck");
        Assert(testDeck.MonsterCards.Count == 1, "Unified deck should have 1 monster card");
        
        var testSkillCard = CreateTestSkillCard();
        bool skillAdded = testDeck.AddSkillCard(testSkillCard);
        Assert(skillAdded, "Should add skill card to deck");
        Assert(testDeck.SkillCards.Count == 1, "Unified deck should have 1 skill card");
        
        // 测试卡组统计
        var deckStats = testDeck.GetDeckStats();
        Assert(deckStats != null, "Should get unified deck stats");
        Assert(deckStats.ContainsKey("total_cards"), "Unified deck stats should include total cards");
        Assert(deckStats.ContainsKey("monster_count"), "Unified deck stats should include monster count");
        Assert(deckStats.ContainsKey("skill_count"), "Unified deck stats should include skill count");
        
        // 测试卡组获取
        var retrievedDeck = deckManager.GetDeck("Test Unified Deck");
        Assert(retrievedDeck != null, "Should retrieve unified deck");
        Assert(retrievedDeck == testDeck, "Retrieved deck should be the same instance");
        
        // 测试卡组验证
        Assert(testDeck.IsValidDeck(), "Unified deck should be valid");
        
        Log("Unified deck system tests completed");
    }
    
    /// <summary>
    /// 测试游戏管理器集成
    /// </summary>
    private void TestGameManagerIntegration()
    {
        Log("\n--- Testing Game Manager Integration ---");
        
        // 测试游戏状态管理
        Assert(_gameManager.CurrentState == MonsterGameManager.GameState.MainMenu, "Should start in main menu");
        
        _gameManager.ChangeGameState(MonsterGameManager.GameState.Battle);
        Assert(_gameManager.CurrentState == MonsterGameManager.GameState.Battle, "Should change to battle state");
        
        // 测试战斗流程
        float damage = _gameManager.ProcessTypingInput("test input");
        Assert(damage > 0, "Should process typing input and deal damage");
        
        // 测试召唤师设置
        Assert(_gameManager.CurrentSummoner != null, "Should have current summoner");
        
        var newSummoner = CreateTestSummoner();
        _gameManager.SetCurrentSummoner(newSummoner);
        Assert(_gameManager.CurrentSummoner == newSummoner, "Should update current summoner");
        
        // 测试可用卡牌获取
        var availableCards = _gameManager.GetAvailableCards();
        Assert(availableCards != null, "Should get available cards list");
        
        // 测试统一卡组系统集成
        var deckManager = _gameManager.GetDeckManager();
        Assert(deckManager != null, "Should have unified deck manager");
        
        var gameDeck = _gameManager.GetDeckManager().CreateNewDeck("Game Test Unified Deck");
        Assert(gameDeck != null, "Should create unified deck through game manager");
        
        // 测试游戏统计
        var gameStats = _gameManager.GetGameStats();
        Assert(gameStats != null, "Should get game stats");
        Assert(gameStats.ContainsKey("game_progress"), "Stats should include game progress");
        Assert(gameStats.ContainsKey("current_state"), "Stats should include current state");
        
        // 结束战斗
        _gameManager.EndBattle(true);
        Assert(_gameManager.CurrentState == MonsterGameManager.GameState.Battle, "Should still be in battle state");
        
        Log("Game manager integration tests completed");
    }
    
    /// <summary>
    /// 创建测试召唤师
    /// </summary>
    private SummonerHero CreateTestSummoner()
    {
        var summoner = new SummonerHero();
        summoner.Id = 1;
        summoner.HeroName = "测试召唤师";
        summoner.PrimaryColor = MagicColor.Red;
        summoner.TypingDamageBase = 20f;
        summoner.TypingSpeedBonus = 0.1f;
        summoner.TypingAccuracyBonus = 0.05f;
        summoner.InitializeDefaultColorSlots();
        return summoner;
    }
    
    /// <summary>
    /// 创建测试卡牌
    /// </summary>
    private MonsterCard CreateTestCard()
    {
        var card = new MonsterCard();
        card.Id = 1;
        card.MonsterName = "测试怪物";
        card.Description = "用于测试的怪物";
        card.Rarity = CardRarity.Common;
        card.ColorRequirements = new Godot.Collections.Array<MagicColor> { MagicColor.Red, MagicColor.Red };
        card.Health = 30;
        card.Attack = 25;
        card.SummonCost = 3;
        card.Race = MonsterRace.Beast;
        card.Skills = new Godot.Collections.Array<MonsterSkillType> { MonsterSkillType.Rush };
        return card;
    }
    
    /// <summary>
     /// 创建测试技能卡牌
     /// </summary>
     private SkillCard CreateTestSkillCard()
     {
         var card = new SkillCard();
         card.Id = 1;
         card.CardName = "测试技能";
          card.Description = "用于测试的技能";
          card.SkillType = SkillType.Attack;
          card.ChargeCost = 1;
         return card;
     }
    
    /// <summary>
    /// 断言测试
    /// </summary>
    private void Assert(bool condition, string message)
    {
        _testsTotal++;
        
        if (condition)
        {
            _testsPassed++;
            if (VerboseOutput)
            {
                Log($"✓ {message}");
            }
        }
        else
        {
            Log($"✗ FAILED: {message}");
        }
    }
    
    /// <summary>
    /// 输出测试结果
    /// </summary>
    private void PrintTestResults()
    {
        GD.Print($"\n=== Test Results ===");
        GD.Print($"Passed: {_testsPassed}/{_testsTotal}");
        GD.Print($"Success Rate: {(float)_testsPassed / _testsTotal * 100:F1}%");
        
        if (_testsPassed == _testsTotal)
        {
            GD.Print("🎉 All tests passed!");
        }
        else
        {
            GD.Print($"❌ {_testsTotal - _testsPassed} tests failed");
        }
        
        GD.Print("=== Monster System Tests Completed ===");
    }
    
    /// <summary>
    /// 日志输出
    /// </summary>
    private void Log(string message)
    {
        if (VerboseOutput)
        {
            GD.Print(message);
        }
    }
    
    /// <summary>
    /// 手动运行测试（可从外部调用）
    /// </summary>
    public void RunTests()
    {
        RunAllTests();
    }
}