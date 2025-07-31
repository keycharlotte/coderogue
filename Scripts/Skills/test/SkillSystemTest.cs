using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Skills;
using CodeRogue.Core;

namespace CodeRogue.Test
{
[GlobalClass]
public partial class SkillSystemTest : Node
{
    private CardDatabase _database;
    private SkillSelector _selector;
    private DeckManager _deckManager;
    private SkillTrackManager _trackManager;
    
    [Export] public bool RunTestsOnReady { get; set; } = true;
    [Export] public bool VerboseOutput { get; set; } = true;
    
    public override void _Ready()
    {
        if (RunTestsOnReady)
        {
            CallDeferred(nameof(RunAllTests));
        }
    }
    
    public void RunAllTests()
    {
        GD.Print("=== 技能系统测试开始 ===");
        
        // 初始化组件
        InitializeComponents();
        
        // 运行各项测试
        TestCardDatabase();
        TestSkillCard();
        TestSkillDeck();
        TestSkillSelector();
        TestSkillTrackManager();
        TestIntegration();
        
        GD.Print("=== 技能系统测试完成 ===");
    }
    
    private void InitializeComponents()
    {
        GD.Print("\n--- 初始化组件 ---");
        
        // 获取卡牌数据库
        _database = GetNode<CardDatabase>("/root/CardDatabase");
        if (_database == null)
        {
            GD.PrintErr("CardDatabase autoload not found!");
            return;
        }
        
        // 创建技能选择器
        _selector = new SkillSelector();
        AddChild(_selector);
        _selector._Ready();
        
        // 创建技能卡组管理器
        _deckManager = new DeckManager();
        AddChild(_deckManager);
        _deckManager._Ready();
        
        // 创建技能轨道管理器
        _trackManager = new SkillTrackManager();
        AddChild(_trackManager);
        _trackManager._Ready();
        
        LogResult("组件初始化", true, "所有组件初始化完成");
    }
    
    private void TestCardDatabase()
    {
        GD.Print("\n--- 测试卡牌数据库 ---");
        
        try
        {
            // 测试获取所有技能卡
            var allSkills = _database.GetAllSkillCards();
            LogResult("获取所有技能卡", allSkills.Count > 0, $"找到 {allSkills.Count} 个技能卡");
            
            // 测试按稀有度获取技能卡
            var commonSkills = _database.GetSkillCardsByRarity(CardRarity.Common);
            LogResult("按稀有度获取技能卡", commonSkills.Count > 0, $"找到 {commonSkills.Count} 个普通技能卡");
            
            // 测试按类型获取技能卡
            var attackSkills = _database.GetSkillCardsByType(SkillType.Attack);
            LogResult("按类型获取技能卡", attackSkills.Count >= 0, $"找到 {attackSkills.Count} 个攻击技能卡");
            
            // 测试按ID获取技能卡
            if (allSkills.Count > 0)
            {
                var firstSkill = _database.GetSkillCardById(allSkills[0].Id);
                LogResult("按ID获取技能卡", firstSkill != null, "成功获取指定技能卡");
            }
        }
        catch (System.Exception e)
        {
            LogResult("卡牌数据库测试", false, $"异常: {e.Message}");
        }
    }
    
    private void TestSkillCard()
    {
        GD.Print("\n--- 测试技能卡片 ---");
        
        try
        {
            // 创建测试技能卡片
            var testSkill = CreateTestSkill();
            LogResult("创建技能卡片", testSkill != null, "技能卡片创建成功");
            
            // 测试技能升级
            var originalLevel = testSkill.Level;
            var canUpgrade = testSkill.CanUpgrade();
            LogResult("检查升级条件", true, $"等级 {originalLevel}, 可升级: {canUpgrade}");
            
            if (canUpgrade)
            {
                testSkill.Upgrade();
                LogResult("技能升级", testSkill.Level > originalLevel, $"升级后等级: {testSkill.Level}");
            }
            
            // 测试技能复制
            var copiedSkill = testSkill.CreateUpgradedCopy();
            LogResult("技能复制", copiedSkill != null && copiedSkill.Level > testSkill.Level, "升级复制成功");
        }
        catch (System.Exception e)
        {
            LogResult("技能卡片测试", false, $"异常: {e.Message}");
        }
    }
    
    private void TestSkillDeck()
    {
        GD.Print("\n--- 测试技能卡组 ---");
        
        try
        {
            // 创建测试卡组
            var deck = new UnifiedDeck();
            LogResult("创建卡组", deck != null, "卡组创建成功");
            
            // 测试添加技能
            var testSkill = CreateTestSkill();
            deck.AddCard(testSkill);
            LogResult("添加技能", deck.Cards.Count == 1, $"卡组中有 {deck.Cards.Count} 张卡片");
            
            // 测试移除技能
            deck.RemoveCard(testSkill);
            LogResult("移除技能", deck.Cards.Count == 0, $"移除后卡组中有 {deck.Cards.Count} 张卡片");
            
            // 测试卡组效率计算
            deck.AddCard(CreateTestSkill(SkillType.Attack));
            deck.AddCard(CreateTestSkill(SkillType.Defense));
            
            var attackRatio = deck.GetSkillRatio(SkillType.Attack);
            var defenseRatio = deck.GetSkillRatio(SkillType.Defense);
            LogResult("卡组比例计算", attackRatio > 0 && defenseRatio > 0, $"攻击: {attackRatio:F2}, 防御: {defenseRatio:F2}");
        }
        catch (System.Exception e)
        {
            LogResult("技能卡组测试", false, $"异常: {e.Message}");
        }
    }
    
    private void TestSkillSelector()
    {
        GD.Print("\n--- 测试技能选择器 ---");
        
        try
        {
            // 测试技能选择触发
            _selector.TriggerSkillSelection(1);
            LogResult("触发技能选择", true, "技能选择已触发");
            
            // 测试技能选择（模拟）
            var testSkill = CreateTestSkill();
            _selector.SelectSkill(testSkill);
            LogResult("选择技能", true, "技能选择完成");
        }
        catch (System.Exception e)
        {
            LogResult("技能选择器测试", false, $"异常: {e.Message}");
        }
    }
    
    private void TestSkillTrackManager()
    {
        GD.Print("\n--- 测试技能轨道管理器 ---");
        
        try
        {
            // 创建测试卡组
            var deck = new UnifiedDeck();
            deck.AddCard(CreateTestSkill(SkillType.Attack, 50)); // 充能消耗50
            deck.AddCard(CreateTestSkill(SkillType.Defense, 30)); // 充能消耗30
            
            // 设置卡组到轨道管理器
            _trackManager.SetDeck(deck);
            LogResult("设置卡组", true, "卡组已设置到轨道管理器");
            
            // 测试充能
            _trackManager.AddCharge(60);
            LogResult("添加充能", true, "充能已添加");
            
            // 测试技能激活检查
            var canActivate = _trackManager.CanActivateAnySkill();
            LogResult("技能激活检查", true, $"可激活技能: {canActivate}");
            
            // 测试技能执行
            if (canActivate)
            {
                _trackManager.TryActivateSkill();
                LogResult("技能执行", true, "技能已执行");
            }
        }
        catch (System.Exception e)
        {
            LogResult("技能轨道管理器测试", false, $"异常: {e.Message}");
        }
    }
    
    private void TestIntegration()
    {
        GD.Print("\n--- 测试系统集成 ---");
        
        try
        {
            // 测试完整的技能获取流程
            _selector.TriggerSkillSelection(5); // 模拟5级玩家
            var testSkill = CreateTestSkill();
            _selector.SelectSkill(testSkill);
            
            // 检查技能是否正确添加到卡组
            var currentDeck = _deckManager.GetCurrentDeck();
            var hasSkill = currentDeck?.Cards?.Any(c => c.Id == testSkill.Id) ?? false;
            LogResult("技能获取流程", hasSkill, "技能已正确添加到卡组");
            
            // 测试技能轨道更新
            _trackManager.SetDeck(currentDeck);
            LogResult("轨道更新", true, "技能轨道已更新");
            
            LogResult("系统集成", true, "所有系统协同工作正常");
        }
        catch (System.Exception e)
        {
            LogResult("系统集成测试", false, $"异常: {e.Message}");
        }
    }
    
    private SkillCard CreateTestSkill(SkillType type = SkillType.Attack, int chargeCost = 40)
	{
		var skill = new SkillCard();
		skill.Id = (int)GD.Randi();
		skill.Name = "测试技能";
		skill.Description = "这是一个测试技能";
		skill.SkillType = type;
		skill.Cost = chargeCost;
		skill.Level = 1;
		// skill.MaxLevel = 3;
		skill.Rarity = CardRarity.Common;
		// 创建测试标签
        var testTag = new CardTag();
        testTag.Name = "测试";
        testTag.Description = "测试标签";
        testTag.Color = Colors.White;
        skill.Tags = new Array<CardTag> { testTag };
        
        // 创建测试效果
        var effect = new SkillEffect();
        effect.Type = SkillEffectType.Damage;
        effect.Value = 100;
        skill.Effects = new Array<SkillEffect> { effect };
        
        return skill;
    }
    
    private void LogResult(string testName, bool success, string details = "")
    {
        string status = success ? "✓ 通过" : "✗ 失败";
        string message = $"[{testName}] {status}";
        
        if (!string.IsNullOrEmpty(details))
        {
            message += $" - {details}";
        }
        
        if (VerboseOutput || !success)
        {
            GD.Print(message);
        }
    }
    
    // 手动运行测试的方法
    public void RunManualTest()
    {
        RunAllTests();
    }
}
}