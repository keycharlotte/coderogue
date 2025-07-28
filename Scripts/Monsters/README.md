# 怪物召唤卡牌颜色系统

基于策划文档实现的完整怪物召唤系统，采用万智牌风格的五色体系，结合打字战斗机制。

## 系统架构

### 核心组件

1. **BaseCard.cs** - 所有卡牌的抽象基类
   - 基础属性：ID、名称、描述、卡牌类型、稀有度
   - 费用系统：统一的费用属性
   - 颜色需求：万智牌风格的颜色系统
   - 标签系统：灵活的标签管理
   - 视觉属性：图标路径、稀有度颜色
   - 抽象方法：战斗力评估、升级、复制等

2. **MonsterCard.cs** - 继承自BaseCard的怪物卡牌
   - 战斗属性：攻击力、生命值、护甲值
   - 特殊属性：种族、技能、羁绊类型
   - 召唤相关：召唤费用映射到基类Cost属性

3. **SkillCard.cs** - 继承自BaseCard的技能卡牌
   - 技能属性：技能类型、伤害倍率、防御值
   - 使用限制：冷却回合、充能费用
   - 效果系统：技能效果和持续时间

4. **SummonerHero.cs** - 召唤师英雄系统
   - 召唤师的颜色槽位管理
   - 打字伤害相关属性
   - 召唤师技能系统
   - 怪物召唤条件检查

5. **SummonSystem.cs** - 召唤系统核心
   - 管理战场上的怪物
   - 处理召唤逻辑和限制
   - 计算羁绊效果
   - 召唤点数管理

6. **TypingCombatSystem.cs** - 打字战斗系统
   - 处理打字输入和伤害计算
   - 召唤师打字伤害 + 怪物协同伤害
   - 伤害衰减和统计追踪
   - 关键词匹配和协同效果

7. **MonsterManager.cs** - 怪物管理器
   - 卡牌库管理和随机获取
   - 收藏系统
   - 按条件过滤卡牌
   - 稀有度和颜色权重系统

8. **BaseDeck.cs** - 所有卡组的抽象基类
   - 卡组基础：名称、卡牌列表、大小限制
   - 抽牌系统：抽牌堆、弃牌堆管理
   - 统计功能：费用分布、颜色需求、稀有度分布
   - 验证功能：卡组合法性检查
   - 操作功能：添加、移除、洗牌、复制
   - 抽象方法：特定统计计算、卡组复制

9. **UnifiedDeck.cs** - 继承自BaseDeck的统一卡组
   - 支持怪物卡和技能卡的混合管理
   - 召唤师关联：与特定召唤师的兼容性
   - 统计功能：种族分布、技能分布、类型分布
   - 类型安全访问：MonsterCards和SkillCards属性

11. **DeckManager.cs** - 卡组管理器
    - 所有类型卡组的统一管理
    - 类型安全的卡组获取和操作
    - 卡组的保存和加载
    - 卡组切换和操作
    - 自动构建和验证
    - 支持怪物卡组和技能卡组的创建

12. **MonsterGameManager.cs** - 游戏管理器
   - 整合所有系统组件
   - 游戏状态管理
   - 战斗流程控制
   - 统一的API接口

13. **CardEnums.cs** - 统一的卡牌系统枚举
    - CardType：卡牌类型（怪物、技能、法术等）
    - CardRarity：通用稀有度系统
    - MagicColor：万智牌风格的魔法颜色
    - SkillType：技能类型
    - MonsterRace：怪物种族
    - BondType：羁绊类型
    - 各种技能类型枚举

### 支持组件

- **MonsterSystemTest.cs** - 完整的测试套件，验证统一卡牌基类功能、怪物卡牌和技能卡牌、召唤师功能、召唤系统逻辑、打字战斗系统、卡牌管理器、统一卡组系统、游戏管理器集成
- **README.md** - 系统文档

## 核心特性

### 1. 统一卡牌设计
- **BaseCard基类**: 所有卡牌的统一基础
- **类型安全**: 强类型的卡牌系统
- **灵活扩展**: 易于添加新的卡牌类型
- **统一属性**: 费用、稀有度、颜色需求等

### 2. 万智牌风格五色系统
- **白色（White）**: 治疗、保护、秩序
- **蓝色（Blue）**: 魔法、控制、智慧
- **黑色（Black）**: 黑暗、诅咒、死亡
- **红色（Red）**: 火焰、愤怒、破坏
- **绿色（Green）**: 自然、生命、成长

### 3. 双重战斗系统
- **召唤师打字伤害**: 基于WPM、准确度和技能
- **怪物协同伤害**: 基于打字内容与怪物的匹配度
- **技能系统**: 独立的技能卡牌系统
- **动态伤害占比**: 从早期打字主导到后期怪物主导

### 4. 羁绊系统
- **双色羁绊**: 如白蓝、蓝黑等10种组合
- **三色羁绊**: 多色怪物激活
- **同色羁绊**: 相同颜色怪物的协同
- **种族羁绊**: 相同种族的额外加成

### 5. 召唤师系统
- **颜色槽位**: 限制可召唤的怪物类型
- **打字技能**: 影响打字伤害和衰减
- **召唤加成**: 对特定怪物的属性提升

### 6. 统一卡组系统
- **多类型支持**: 怪物卡组和技能卡组统一管理
- **类型安全访问**: 强类型的卡组获取
- **智能验证**: 检查颜色需求和召唤师兼容性
- **自动构建**: 基于召唤师特点的智能卡组生成
- **统计分析**: 费用曲线、颜色分布、羁绊潜力

## 使用方法

### 基础设置

```csharp
// 创建游戏管理器
var gameManager = new MonsterGameManager();
AddChild(gameManager);

// 获取系统组件
var summonSystem = gameManager.GetSummonSystem();
var typingSystem = gameManager.GetTypingCombatSystem();
var cardManager = gameManager.GetCardManager();
var deckManager = gameManager.GetDeckManager(); // 统一卡组管理器
```

### 召唤师设置

```csharp
// 创建召唤师
var summoner = new SummonerHero();
summoner.HeroName = "火焰法师";
summoner.PrimaryColor = MagicColor.Red;
summoner.BaseTypingDamage = 15f;
summoner.InitializeDefaultColorSlots();

// 设置为当前召唤师
gameManager.SetCurrentSummoner(summoner);
```

### 战斗流程

```csharp
// 开始战斗
gameManager.ChangeGameState(MonsterGameManager.GameState.Battle);

// 处理打字输入
float damage = gameManager.ProcessTypingInput("fire dragon attack");

// 召唤怪物
var availableCards = gameManager.GetAvailableCards();
if (availableCards.Count > 0)
{
    bool success = gameManager.SummonMonster(availableCards[0]);
}

// 结束战斗
gameManager.EndBattle(true); // true表示胜利
```

### 卡组管理

```csharp
// 创建统一卡组（支持怪物卡和技能卡）
var unifiedDeck = deckManager.CreateNewDeck("我的统一卡组");
var monsterCard = cardManager.GetRandomCard() as MonsterCard;
if (monsterCard != null)
    unifiedDeck.AddMonsterCard(monsterCard);

var skillCard = new SkillCard("fireball", "火球术", "造成火焰伤害");
unifiedDeck.AddSkillCard(skillCard);

// 类型安全的卡组获取
var retrievedDeck = deckManager.GetDeck("我的统一卡组");

// 自动构建卡组
var autoDeck = deckManager.AutoBuildDeck("自动卡组", 15, 10);

// 切换当前卡组
deckManager.SetCurrentDeck(autoDeck);
```

### 卡牌获取

```csharp
// 获取随机卡牌
var randomCard = cardManager.GetRandomCard();

// 获取特定类型的卡牌
var randomMonster = cardManager.GetRandomCard() as MonsterCard;
var randomSkill = cardManager.GetRandomCard() as SkillCard;

// 获取卡牌包
var cardPack = cardManager.GetCardPack(5);

// 按条件过滤
var redCards = cardManager.GetCardsByColor(MagicColor.Red);
var rareCards = cardManager.GetCardsByRarity(CardRarity.Rare);
var dragonCards = cardManager.GetCardsByRace(MonsterRace.Dragon);
var attackSkills = cardManager.GetCardsBySkillType(SkillType.Attack);
```

## 扩展性设计

### 1. 新增怪物种族
```csharp
// 在MonsterEnums.cs中添加新种族
public enum MonsterRace
{
    // 现有种族...
    Fairy,      // 新增精灵种族
    Construct   // 新增构造体种族
}

// 在TypingCombatSystem.cs中添加对应关键词
_raceKeywords[MonsterRace.Fairy] = new List<string> { "fairy", "magic", "sparkle" };
```

### 2. 新增召唤师技能
```csharp
// 在MonsterEnums.cs中添加新技能
public enum SummonerSkillType
{
    // 现有技能...
    ElementalMastery,  // 新增元素精通
    BondAmplifier      // 新增羁绊放大器
}

// 在相关系统中实现技能效果
```

### 3. 新增羁绊类型
```csharp
// 在MonsterEnums.cs中添加新羁绊
public enum BondType
{
    // 现有羁绊...
    TribalUnity,    // 种族团结
    ElementalFusion // 元素融合
}
```

### 4. 自定义伤害计算
```csharp
// 继承TypingCombatSystem并重写计算方法
public partial class CustomTypingCombatSystem : TypingCombatSystem
{
    protected override float CalculateTypingDamage(string inputText)
    {
        // 自定义伤害计算逻辑
        return base.CalculateTypingDamage(inputText) * customMultiplier;
    }
}
```

## 测试

系统包含完整的测试套件，可以验证所有核心功能：

```csharp
// 创建测试节点
var testNode = new MonsterSystemTest();
AddChild(testNode);

// 手动运行测试
testNode.RunTests();
```

测试覆盖：
- 怪物卡牌创建和属性
- 召唤师系统
- 召唤逻辑和限制
- 打字战斗机制
- 卡牌管理和获取
- 卡组构建和验证
- 系统集成

## 配置选项

游戏管理器提供多种配置选项：

```csharp
gameManager.GameConfig["auto_save_interval"] = 60f;           // 自动保存间隔
gameManager.GameConfig["max_summoned_monsters"] = 10;         // 最大召唤数量
gameManager.GameConfig["starting_summon_points"] = 15;        // 初始召唤点数
gameManager.GameConfig["typing_damage_decay_rate"] = 0.015f;  // 打字伤害衰减率
gameManager.GameConfig["monster_synergy_multiplier"] = 1.2f;  // 怪物协同倍数
```

## 性能考虑

- 使用对象池管理频繁创建的对象
- 缓存计算结果避免重复计算
- 异步加载大量卡牌数据
- 限制同时处理的协同效果数量

## 未来扩展方向

1. **UI系统**: 卡牌展示、卡组编辑界面
2. **动画系统**: 召唤、攻击、技能效果动画
3. **音效系统**: 打字音效、技能音效
4. **AI对手**: 自动战斗的AI系统
5. **网络对战**: 多人对战功能
6. **成就系统**: 游戏进度和成就追踪
7. **商店系统**: 卡牌购买和交易
8. **竞技场模式**: 限制构筑的竞技模式

## 注意事项

1. 所有类都继承自Godot的Resource或Node
2. 使用Godot的信号系统进行组件通信
3. 支持序列化保存和加载
4. 遵循Godot的命名约定
5. 提供完整的错误处理和日志输出

这个系统为1.0版本的实现，具有良好的扩展性，可以根据后续需求轻松添加新功能和调整平衡性。