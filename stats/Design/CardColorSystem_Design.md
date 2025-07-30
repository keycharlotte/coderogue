# 怪物召唤卡牌颜色系统设计文档

## 1. 系统概述

本文档描述了一个基于万智牌颜色体系的怪物召唤卡牌系统。玩家通过收集不同颜色的怪物卡牌，构建最多7只怪物的战斗阵容，利用怪物间的羁绊效果来获得战斗优势。

### 1.1 核心机制
- **怪物召唤**: 主要卡牌类型为怪物卡，可召唤到战场参与战斗
- **颜色体系**: 参考万智牌的五色体系，每种颜色有独特的怪物特性
- **召唤上限**: 同时最多召唤7只怪物到战场
- **羁绊系统**: 特定怪物组合产生强力的协同效果

### 1.2 设计目标
- 创造丰富的怪物阵容构建策略
- 通过羁绊系统鼓励特定怪物组合
- 提供深度的战术选择和反制机制
- 建立清晰的颜色身份和游戏风格

### 1.3 与万智牌的对比
我们的设计灵感来源于万智牌，但针对塔防游戏进行了适配：
- **白色**: 秩序与保护 → 防御型怪物和治疗支援
- **蓝色**: 知识与控制 → 法术型怪物和战场控制
- **黑色**: 力量与牺牲 → 高攻击力怪物，以生命换取力量
- **红色**: 混沌与激情 → 爆发型怪物和范围伤害
- **绿色**: 自然与成长 → 成长型怪物和资源加速

## 2. 万智牌风格颜色系统

### 2.1 五色怪物体系

#### ⚪ 白色 (White) - 秩序与保护
- **核心理念**: 团结、秩序、保护、正义
- **怪物特性**: 防御型怪物、治疗者、护盾兵
- **战斗风格**: 稳固防线、团队增益、生命恢复
- **代表怪物**: 圣骑士、治疗师、守护天使、盾卫兵
- **特殊机制**: 护盾、治疗、减伤、复活

#### 🔵 蓝色 (Blue) - 知识与控制  
- **核心理念**: 知识、逻辑、控制、完美
- **怪物特性**: 法师型怪物、控制者、召唤师
- **战斗风格**: 战场控制、法术输出、战术操控
- **代表怪物**: 元素法师、时间法师、幻象师、奥术师
- **特殊机制**: 冰冻、传送、反制、抽卡

#### ⚫ 黑色 (Black) - 力量与牺牲
- **核心理念**: 野心、力量、牺牲、不择手段
- **怪物特性**: 高攻击怪物、吸血鬼、恶魔
- **战斗风格**: 以命换命、高风险高回报、资源转换
- **代表怪物**: 吸血鬼、恶魔、死灵法师、暗影刺客
- **特殊机制**: 吸血、献祭、诅咒、复活死者

#### 🔴 红色 (Red) - 混沌与激情
- **核心理念**: 自由、激情、混沌、直接行动
- **怪物特性**: 爆发型怪物、狂战士、火元素
- **战斗风格**: 快速攻击、范围伤害、不稳定效果
- **代表怪物**: 火龙、狂战士、爆破专家、火元素
- **特殊机制**: 暴击、范围伤害、随机效果、自爆

#### 🟢 绿色 (Green) - 自然与成长
- **核心理念**: 自然、成长、和谐、原始力量
- **怪物特性**: 成长型怪物、野兽、植物
- **战斗风格**: 持续成长、自然治愈、群体召唤
- **代表怪物**: 树人、巨熊、精灵射手、自然守护者
- **特殊机制**: 成长、再生、召唤小怪、毒素

### 2.2 颜色组合与羁绊系统

#### 双色羁绊组合
- **白+蓝**: 【秩序法师】- 防御型法师，提供护盾和控制
- **白+黑**: 【堕落骑士】- 以生命为代价的强力防御
- **白+红**: 【圣战士】- 攻防兼备的战士，爆发治疗
- **白+绿**: 【自然守护】- 持续治疗和防御增强
- **蓝+黑**: 【暗黑法师】- 控制+吸血的法术组合
- **蓝+红**: 【元素法师】- 不稳定但强力的法术爆发
- **蓝+绿**: 【德鲁伊】- 自然魔法，成长型控制
- **黑+红**: 【混沌恶魔】- 高风险高回报的狂暴攻击
- **黑+绿**: 【腐化自然】- 毒素+吸血的持续伤害
- **红+绿**: 【野性狂战】- 成长型的爆发攻击

#### 三色羁绊组合
- **白+蓝+绿**: 【自然圣殿】- 完美的防御+控制+治疗体系
- **黑+红+绿**: 【原始混沌】- 野性+破坏+成长的强力组合
- **白+黑+红**: 【审判军团】- 正义与力量的极端结合

#### 羁绊效果机制
- **2怪物羁绊**: 基础属性提升10-20%
- **3怪物羁绊**: 解锁特殊技能或被动效果
- **4+怪物羁绊**: 场地效果，影响整个战场
- **同色羁绊**: 相同颜色怪物数量越多，该颜色特性越强

#### 怪物稀有度与颜色需求
- **普通怪物**: 单色，基础属性
- **稀有怪物**: 单色，特殊技能
- **史诗怪物**: 双色，强力羁绊核心
- **传奇怪物**: 双色或三色，改变战局的存在

## 3. 怪物召唤卡牌系统

### 3.1 卡牌类型分类

#### 主要卡牌类型（80%）
- **怪物卡**: 可召唤到战场的战斗单位
- **召唤费用**: 每只怪物需要消耗一定的召唤点数
- **战场限制**: 同时最多存在7只怪物

#### 辅助卡牌类型（20%）
- **法术卡**: 即时效果，不占用怪物位置
- **装备卡**: 为怪物提供永久增强
- **环境卡**: 改变战场规则的特殊卡牌

### 3.2 怪物卡牌属性系统

#### 基础属性
- **生命值**: 怪物的耐久度
- **攻击力**: 怪物的伤害输出
- **召唤费用**: 召唤该怪物需要的资源
- **颜色需求**: 召唤需要的颜色槽位

#### 特殊属性
- **种族标签**: 决定羁绊效果（如：恶魔、天使、元素等）
- **技能**: 怪物的主动或被动技能
- **羁绊标识**: 与其他怪物的协同关系

### 3.3 召唤机制

#### 召唤限制
- **数量上限**: 战场最多7只怪物
- **费用限制**: 每回合有限的召唤点数
- **颜色匹配**: 必须满足怪物的颜色需求

#### 召唤策略
- **替换召唤**: 可以用新怪物替换旧怪物
- **合成进化**: 特定怪物组合可以合成更强怪物
- **羁绊激活**: 达到羁绊条件时自动触发效果

```
怪物召唤流程:
1. 选择怪物卡牌
2. 检查颜色需求
3. 消耗召唤费用
4. 放置到战场
5. 检查羁绊效果
6. 激活怪物技能
```

## 4. 召唤师英雄系统

### 4.1 英雄作为召唤师

#### 英雄定位
- **召唤师角色**: 英雄可以通过打字直接造成伤害，同时召唤和指挥怪物
- **伤害占比变化**: 游戏初期召唤师打字伤害占主导，后期怪物召唤成为主要伤害来源
- **颜色亲和**: 每个英雄对特定颜色有亲和性
- **召唤加成**: 为特定类型的怪物提供召唤和战斗加成
- **打字战斗**: 召唤师通过打字系统直接对敌人造成伤害，无需技能轨道充能

#### 英雄颜色槽位系统
- **初始槽位**: 每个英雄有3个颜色槽位
- **槽位升级**: 通过游戏进程可以解锁更多槽位（最多5个）
- **颜色专精**: 英雄对主要颜色有额外加成

### 4.2 英雄类型与颜色倾向

#### 单色专精英雄
```
圣光召唤师: [白][白][白] - 专精防御和治疗怪物
奥术大师: [蓝][蓝][蓝] - 专精法师和控制怪物
暗黑领主: [黑][黑][黑] - 专精恶魔和吸血怪物
火焰君主: [红][红][红] - 专精爆发和火元素怪物
自然守护: [绿][绿][绿] - 专精野兽和植物怪物
```

#### 双色平衡英雄
```
秩序法师: [白][白][蓝] - 防御+控制的平衡
堕落骑士: [白][黑][黑] - 正义与黑暗的冲突
元素法师: [蓝][红][红] - 理性与激情的结合
德鲁伊: [蓝][绿][绿] - 智慧与自然的和谐
混沌领主: [黑][红][绿] - 破坏与野性的融合
```

### 4.3 英雄技能与战斗机制

#### 打字伤害系统
- **直接伤害**: 召唤师通过打字对敌人造成即时伤害
- **伤害衰减**: 随着游戏进程，召唤师打字伤害占比从90%逐渐降至30%
- **怪物崛起**: 后期怪物召唤伤害占比从10%逐渐提升至70%
- **协同作战**: 召唤师打字和怪物攻击可以产生协同效果

#### 召唤师技能
- **召唤加速**: 减少特定颜色怪物的召唤费用
- **属性强化**: 提升特定类型怪物的属性
- **羁绊增强**: 强化特定羁绊效果
- **特殊召唤**: 解锁独特的怪物或召唤方式
- **打字强化**: 提升召唤师打字伤害和效果

#### 英雄被动效果
- **颜色共鸣**: 场上同色怪物越多，效果越强
- **召唤精通**: 特定怪物种族获得额外能力
- **战场掌控**: 影响整个战场的环境效果
- **打字精通**: 提升打字速度和准确度的伤害加成

## 5. 怪物卡牌获取系统

### 5.1 获取渠道

#### 战斗奖励
- **胜利奖励**: 每次战斗胜利后获得1-3张怪物卡牌
- **完美胜利**: 无损胜利额外获得稀有怪物卡牌
- **连胜奖励**: 连续胜利提升怪物卡牌品质
- **击败特定敌人**: 有概率获得对应类型的怪物卡牌

#### 召唤仪式
- **基础召唤**: 消耗基础资源召唤普通怪物
- **高级召唤**: 消耗稀有资源召唤强力怪物
- **颜色召唤**: 针对特定颜色的专门召唤
- **羁绊召唤**: 有更高概率获得特定羁绊的怪物组合

#### 特殊事件
- **精英战**: 击败精英敌人获得强力怪物卡牌
- **神秘商人**: 用特殊货币购买稀有怪物
- **古老遗迹**: 探索获得传说怪物
- **契约仪式**: 通过特殊条件获得独特怪物

### 5.2 获取限制规则

#### 颜色匹配限制
- **英雄亲和**: 更容易获得英雄颜色倾向的怪物
- **颜色发现**: 新颜色的怪物需要特殊条件才能获得
- **颜色平衡**: 系统会平衡不同颜色怪物的获得概率

#### 稀有度渐进
- **初期**: 主要获得普通怪物（单色需求）
- **中期**: 开始出现稀有怪物（双色需求）
- **后期**: 有机会获得史诗和传奇怪物（多色需求）
- **进阶条件**: 高稀有度怪物需要满足特定的召唤条件

#### 羁绊获取机制
- **套装获取**: 某些怪物以套装形式出现，便于组成羁绊
- **羁绊提示**: 系统会提示玩家缺少的羁绊怪物
- **羁绊强化**: 已有羁绊的玩家更容易获得相关怪物

#### 5.2.1 攻击槽位未满时
**英雄只有1个红色攻击槽位：**
- 可获取：除了"三种颜色都不是红色"的所有攻击卡牌
- 禁止获取：[蓝,绿,黄]、[蓝,绿,黑]等不含红色的攻击卡牌

**英雄有2个红色攻击槽位：**
- 可获取：至少包含一种红色或只有一种非红色的攻击卡牌
- 禁止获取：包含两种或以上非红色的攻击卡牌

#### 5.2.2 攻击槽位填满后
**英雄攻击槽位为 [红,红,蓝]：**
- 只能获取包含红色和/或蓝色的攻击卡牌
- 禁止获取包含绿、黄、黑、白的攻击卡牌

## 6. 技术实现要点

### 6.1 怪物卡牌数据结构

```csharp
public class MonsterCard : Resource
{
    public string MonsterName { get; set; }
    public CardRarity Rarity { get; set; }
    public ColorSlot[] ColorRequirements { get; set; }
    public int Health { get; set; }
    public int Attack { get; set; }
    public int SummonCost { get; set; }
    public MonsterRace Race { get; set; }
    public string[] Skills { get; set; }
    public BondType[] BondTypes { get; set; }
    public string Description { get; set; }
}

public enum MonsterRace
{
    Human,      // 人类
    Beast,      // 野兽
    Demon,      // 恶魔
    Elemental,  // 元素
    Undead,     // 亡灵
    Dragon,     // 龙族
    Angel,      // 天使
    Plant       // 植物
}

public enum BondType
{
    OrderMage,      // 秩序法师
    DarkMage,       // 暗黑法师
    NatureTemple,   // 自然圣殿
    ChaosLord,      // 混沌领主
    ElementalStorm, // 元素风暴
    SameColor       // 同色羁绊
}
```

### 6.2 召唤师英雄系统

```csharp
public class SummonerHero : Resource
{
    public string HeroName { get; set; }
    public ColorSlot[] ColorSlots { get; set; }
    public int MaxColorSlots { get; set; } = 5;
    public CardColor PrimaryColor { get; set; }
    public Dictionary<MonsterRace, float> RaceBonus { get; set; }
    public Dictionary<BondType, float> BondBonus { get; set; }
    
    // 打字伤害相关属性
    public float TypingDamageBase { get; set; } = 100f;
    public float TypingDamageDecayRate { get; set; } = 0.1f; // 每关卡衰减率
    public float TypingSpeedBonus { get; set; } = 1.0f;
    public float TypingAccuracyBonus { get; set; } = 1.0f;
    
    public bool CanSummonMonster(MonsterCard monster)
    {
        return ColorSlot.CanSatisfyRequirements(ColorSlots, monster.ColorRequirements);
    }
    
    public float GetSummonBonus(MonsterCard monster)
    {
        float bonus = 0f;
        
        // 种族加成
        if (RaceBonus.ContainsKey(monster.Race))
            bonus += RaceBonus[monster.Race];
            
        // 颜色亲和加成
        if (monster.ColorRequirements.Any(req => req.HasColor(PrimaryColor)))
            bonus += 0.2f;
            
        return bonus;
    }
    
    public float CalculateTypingDamage(int currentLevel, float typingSpeed, float accuracy)
    {
        // 基础伤害随关卡衰减
        float levelDecay = Mathf.Pow(1f - TypingDamageDecayRate, currentLevel - 1);
        float baseDamage = TypingDamageBase * levelDecay;
        
        // 打字速度和准确度加成
        float speedMultiplier = 1f + (typingSpeed - 1f) * TypingSpeedBonus;
        float accuracyMultiplier = 1f + (accuracy - 1f) * TypingAccuracyBonus;
        
        return baseDamage * speedMultiplier * accuracyMultiplier;
    }
}
```

### 6.3 召唤与羁绊系统

```csharp
public class SummonSystem : Node
{
    public const int MAX_SUMMONED_MONSTERS = 7;
    public List<MonsterCard> SummonedMonsters { get; private set; } = new List<MonsterCard>();
    public Dictionary<BondType, int> ActiveBonds { get; private set; } = new Dictionary<BondType, int>();
    
    public bool CanSummon(MonsterCard monster, SummonerHero hero)
    {
        return SummonedMonsters.Count < MAX_SUMMONED_MONSTERS && 
               hero.CanSummonMonster(monster);
    }
    
    public void SummonMonster(MonsterCard monster)
    {
        if (SummonedMonsters.Count >= MAX_SUMMONED_MONSTERS)
        {
            // 替换最老的怪物或让玩家选择
            ReplaceSummonedMonster(monster);
        }
        else
        {
            SummonedMonsters.Add(monster);
        }
        
        UpdateBonds();
    }
    
    private void UpdateBonds()
    {
        ActiveBonds.Clear();
        
        // 计算羁绊
        foreach (var bondType in System.Enum.GetValues<BondType>())
        {
            int count = CountMonstersWithBond(bondType);
            if (count >= 2)
            {
                ActiveBonds[bondType] = count;
            }
        }
    }
    
    private int CountMonstersWithBond(BondType bondType)
    {
        return SummonedMonsters.Count(monster => monster.BondTypes.Contains(bondType));
    }
}
```

### 6.4 羁绊效果系统

```csharp
public class BondEffectSystem : Node
{
    public Dictionary<BondType, BondEffect> BondEffects { get; private set; }
    
    public void ApplyBondEffects(List<MonsterCard> monsters, Dictionary<BondType, int> activeBonds)
    {
        foreach (var bond in activeBonds)
        {
            var effect = BondEffects[bond.Key];
            var affectedMonsters = monsters.Where(m => m.BondTypes.Contains(bond.Key)).ToList();
            
            effect.Apply(affectedMonsters, bond.Value);
        }
    }
}

public abstract class BondEffect
{
    public abstract void Apply(List<MonsterCard> monsters, int bondCount);
}
```

### 6.5 打字战斗系统

```csharp
public class TypingCombatSystem : Node
{
    public SummonerHero CurrentHero { get; set; }
    public SummonSystem SummonSystem { get; set; }
    
    public float CalculateTotalDamage(string typedWord, float typingSpeed, float accuracy, int currentLevel)
    {
        // 召唤师打字伤害
        float heroTypingDamage = CurrentHero.CalculateTypingDamage(currentLevel, typingSpeed, accuracy);
        
        // 怪物协同伤害
        float monsterSynergyDamage = CalculateMonsterSynergy(typedWord);
        
        // 总伤害
        return heroTypingDamage + monsterSynergyDamage;
    }
    
    private float CalculateMonsterSynergy(string typedWord)
    {
        float synergyDamage = 0f;
        
        foreach (var monster in SummonSystem.SummonedMonsters)
        {
            // 根据怪物类型和打字内容计算协同伤害
            if (IsWordMatchMonsterType(typedWord, monster))
            {
                synergyDamage += monster.Attack * 0.1f; // 10%协同伤害
            }
        }
        
        return synergyDamage;
    }
    
    private bool IsWordMatchMonsterType(string word, MonsterCard monster)
    {
        // 根据怪物种族和颜色判断是否与打字内容匹配
        // 例如：火元素怪物与包含"fire"的单词匹配
        return word.ToLower().Contains(monster.Race.ToString().ToLower());
    }
}
```

### 6.6 UI显示系统

#### 怪物卡牌显示
- **颜色需求**: 显示怪物的颜色需求
- **属性面板**: 显示生命值、攻击力、技能
- **羁绊标识**: 显示怪物所属的羁绊类型
- **召唤状态**: 显示是否可以召唤

#### 召唤场地显示
- **7个召唤位**: 显示当前召唤的怪物
- **羁绊效果**: 显示当前激活的羁绊和效果
- **颜色槽位**: 显示英雄的颜色槽位状态

#### 伤害显示系统
- **伤害分解**: 分别显示召唤师打字伤害和怪物伤害
- **伤害占比**: 实时显示两种伤害来源的占比变化
- **协同提示**: 显示打字与怪物的协同效果

## 7. 平衡性考虑

### 7.1 怪物召唤平衡
- **召唤上限**: 7只怪物的上限确保战场不会过于拥挤
- **颜色需求**: 高稀有度怪物需要更多颜色，增加获取难度
- **召唤费用**: 强力怪物需要更高的召唤费用

### 7.2 羁绊效果平衡
- **羁绊门槛**: 2只怪物激活基础羁绊，避免过于容易
- **效果递增**: 羁绊效果随怪物数量递增，但不应过于强力
- **羁绊冲突**: 某些羁绊之间可能存在冲突，增加策略选择

### 7.3 颜色体系平衡
- **颜色特色**: 每种颜色都有独特的战斗风格和优势
- **颜色互克**: 参考万智牌的颜色哲学，设计颜色间的制衡关系
- **颜色获取**: 平衡不同颜色怪物的获取难度和强度

### 7.4 打字与召唤平衡
- **伤害转移**: 确保从打字主导到怪物主导的平滑过渡
- **协同奖励**: 打字与怪物的协同效果不应过强，避免破坏平衡
- **技能差异**: 不同打字技能水平的玩家都能享受游戏乐趣
- **策略选择**: 玩家可以选择专精打字或专精召唤的不同路线

### 7.5 进程平衡
- **早期怪物**: 确保玩家在早期就能获得基础怪物
- **成长曲线**: 怪物强度随游戏进程稳步提升
- **策略深度**: 避免最优解过于明显，鼓励多样化构筑
- **伤害曲线**: 打字伤害衰减和怪物伤害增长保持合理的节奏

## 8. 扩展性设计

### 8.1 新颜色扩展
- **颜色添加**: 系统支持添加新的颜色（如紫色-时空、橙色-混沌）
- **颜色机制**: 新颜色可以带来全新的战斗机制
- **向后兼容**: 新颜色不影响现有颜色体系的平衡

### 8.2 怪物种族扩展
- **新种族**: 可以添加新的怪物种族（如机械、虫族）
- **种族特性**: 每个种族都有独特的特性和技能
- **跨种族羁绊**: 支持跨种族的特殊羁绊效果

### 8.3 羁绊系统扩展
- **新羁绊类型**: 可以添加更多羁绊组合
- **动态羁绊**: 某些羁绊效果可能随战斗情况动态变化
- **羁绊进化**: 高级羁绊可能需要特定条件才能激活

### 8.4 召唤系统扩展
- **召唤位扩展**: 通过特殊道具或成就增加召唤位
- **特殊召唤**: 某些怪物可能需要特殊的召唤条件
- **怪物融合**: 支持怪物之间的融合进化

## 总结

本设计文档定义了一个基于万智牌颜色体系的怪物召唤卡牌系统，融合了打字战斗机制，核心特点包括：

1. **双重战斗系统**: 召唤师打字伤害 + 怪物召唤伤害的混合战斗模式
2. **动态伤害占比**: 从早期打字主导(90%)到后期怪物主导(70%)的自然过渡
3. **怪物召唤**: 80%怪物卡牌，20%辅助卡牌的构成
4. **颜色体系**: 参考万智牌的五色哲学，每色有独特特性
5. **羁绊机制**: 怪物间的协同效果增加策略深度
6. **召唤限制**: 7只怪物上限和颜色需求创造资源管理挑战
7. **协同战斗**: 打字内容与怪物类型的协同效果
8. **英雄成长**: 召唤师从直接战斗者逐渐转变为战场指挥者

这个系统为塔防游戏提供了丰富的策略选择和深度的构筑玩法，同时保持了打字游戏的核心乐趣，创造了独特的游戏体验。玩家可以根据自己的打字技能和策略偏好选择不同的发展路线，确保了游戏的可玩性和重复性。

---

**请审阅以上设计文档，确认理解一致后我将开始代码实现。**