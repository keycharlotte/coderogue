# 赛博修仙卡牌属性系统设计文档

## 1. 系统概述

本文档描述了一个基于阴阳五行理论的赛博修仙卡牌系统。玩家通过选择修炼职业，收集不同属性的卡牌，构建符合职业特色的卡组，利用阴阳调和与五行相生相克来获得战斗优势。

### 1.1 核心机制
- **职业系统**: 七大修炼职业，每个职业有独特的属性倾向和战斗风格
- **属性体系**: 基于阴阳五行的七属性体系（阴、阳、金、木、水、火、土）
- **游客机制**: 混合属性卡牌需要特殊条件才能使用，类似炉石的游客机制
- **相生相克**: 五行相生相克关系影响战斗效果

### 1.2 设计目标
- 创造深度的职业构筑策略
- 通过属性相生相克建立战术深度
- 提供灵活的混合属性游客机制
- 建立清晰的职业身份和修炼风格

### 1.3 阴阳五行理论基础
我们的设计基于传统阴阳五行理论，结合赛博修仙世界观：
- **阴**: 柔性、防御、治愈、内敛的修炼之道
- **阳**: 刚性、攻击、爆发、外放的修炼之道
- **金**: 锐利、破甲、坚固、金属科技的融合
- **木**: 生长、治疗、持续、生物科技的融合
- **水**: 流动、控制、适应、液态科技的融合
- **火**: 燃烧、爆发、范围、能源科技的融合
- **土**: 厚重、防御、稳定、大地科技的融合

## 2. 阴阳五行职业系统

### 2.1 七大修炼职业详解

#### ☯️ 太极道士 (阴阳平衡)
- **核心理念**: 阴阳调和，平衡至上，万物归一
- **属性倾向**: 阴+阳属性卡牌，可以使用所有阴阳属性技能
- **战斗风格**: 攻防平衡，适应性强，阴阳转换
- **职业优势**: 平衡型发展，无明显短板，阴阳转换能力
- **职业劣势**: 缺乏极端爆发，专精度不如单属性职业
- **独有机制**: 阴阳转换 - 可以将阴性卡牌转为阳性，反之亦然
- **代表技能**: 太极图、阴阳鱼、无极生太极、两仪剑法

#### ⚔️ 金刚武僧 (金行专精)
- **核心理念**: 刚猛无敌，破甲制胜，金石不坏
- **属性倾向**: 金行+阳属性卡牌，专精物理攻击
- **战斗风格**: 高攻击力，破防专家，近战强者
- **职业优势**: 极高物理伤害，破甲能力强，防御坚固
- **职业劣势**: 缺乏远程手段，对法术防御较弱
- **独有机制**: 金刚体 - 免疫控制效果，提升物理抗性
- **代表技能**: 金刚拳、破甲术、不坏金身、降龙十八掌

#### 🌿 青木法师 (木行专精)
- **核心理念**: 生生不息，治愈成长，万物复苏
- **属性倾向**: 木行+阴属性卡牌，专精治疗和持续效果
- **战斗风格**: 治疗支援，持续效果，生命力强
- **职业优势**: 强大治疗能力，持续战斗力，生存能力强
- **职业劣势**: 爆发伤害不足，前期较为弱势
- **独有机制**: 生长 - 技能效果随时间增强，治疗效果翻倍
- **代表技能**: 回春术、藤蔓缠绕、森林守护、木遁·树界降诞

#### 🌊 玄水道人 (水行专精)
- **核心理念**: 上善若水，以柔克刚，变化无穷
- **属性倾向**: 水行+阴属性卡牌，专精控制和流转
- **战斗风格**: 控制流转，适应性强，战术灵活
- **职业优势**: 强大控制能力，适应性强，可改变战场环境
- **职业劣势**: 直接伤害较低，依赖技巧和时机
- **独有机制**: 流转 - 可以改变卡牌属性，重置技能冷却
- **代表技能**: 寒冰术、水镜术、波涛汹涌、水遁·大瀑布之术

#### 🔥 烈火剑仙 (火行专精)
- **核心理念**: 烈火燎原，一往无前，焚尽万物
- **属性倾向**: 火行+阳属性卡牌，专精爆发和范围伤害
- **战斗风格**: 爆发输出，范围伤害，速战速决
- **职业优势**: 极高爆发伤害，范围清场能力，攻击速度快
- **职业劣势**: 防御较弱，持续战斗力不足，容易被控制
- **独有机制**: 燃烧 - 造成持续伤害，可以点燃环境
- **代表技能**: 烈火剑气、凤凰涅槃、火海滔天、火遁·豪火球之术

#### 🏔️ 厚土仙人 (土行专精)
- **核心理念**: 厚德载物，稳如泰山，大地之力
- **属性倾向**: 土行+阴属性卡牌，专精防御和控制
- **战斗风格**: 超强防御，控制压制，持久战斗
- **职业优势**: 极高防御力，控制能力强，持久战斗力
- **职业劣势**: 攻击力较低，机动性差，缺乏爆发
- **独有机制**: 镇压 - 可以封印敌人技能，创造地形障碍
- **代表技能**: 大地之盾、山岳镇压、土遁术、土遁·土流壁

#### 🌌 混元散仙 (游客职业)
- **核心理念**: 无拘无束，万法皆通，混沌归元
- **属性倾向**: 可使用所有属性卡牌，但无专精加成
- **战斗风格**: 灵活多变，战术丰富，不可预测
- **职业优势**: 卡牌选择最多，战术变化丰富，适应性极强
- **职业劣势**: 无专精加成，卡牌获取难度高，需要更多资源
- **独有机制**: 混元 - 可以混合不同属性产生特殊效果
- **代表技能**: 万法归一、混沌初开、无极而太极、五行大循环

### 2.2 五行相生相克系统

#### 五行相生关系
- **金生水**: 金属科技净化水源，金行技能增强水行效果
- **水生木**: 水分滋养植物，水行技能增强木行效果
- **木生火**: 木材燃烧生火，木行技能增强火行效果
- **火生土**: 火焰化为灰烬，火行技能增强土行效果
- **土生金**: 大地孕育金属，土行技能增强金行效果

#### 五行相克关系
- **金克木**: 金属切割植物，金行技能对木行造成额外伤害
- **木克土**: 植物破土而出，木行技能对土行造成额外伤害
- **土克水**: 土壤吸收水分，土行技能对水行造成额外伤害
- **水克火**: 水能灭火，水行技能对火行造成额外伤害
- **火克金**: 烈火熔化金属，火行技能对金行造成额外伤害

#### 阴阳调和机制
- **阴阳平衡**: 阴阳属性卡牌同时使用时，效果增强50%
- **阴阳转换**: 太极道士可以将阴性卡牌转为阳性，反之亦然
- **阴阳相济**: 阴性技能提供防御和治疗，阳性技能提供攻击和爆发

### 2.3 混合属性卡牌系统

#### 双属性混合卡牌
- **金金木**: "钢木合璧" - 需要2个金行+1个木行
  - 效果：坚韧攻击，破甲同时治疗
  - 职业限制：金刚武僧和青木法师可用

- **火火水**: "水火既济" - 需要2个火行+1个水行
  - 效果：蒸汽爆发，范围伤害后产生治疗雾气
  - 职业限制：烈火剑仙和玄水道人可用

- **阴阴阳**: "三才归一" - 需要2个阴性+1个阳性
  - 效果：阴阳调和，大幅提升下一个技能效果
  - 职业限制：太极道士专用

#### 三属性混合卡牌
- **金木水**: "三行轮转" - 需要金木水各1个
  - 效果：连续三段攻击，每段不同属性效果
  - 职业限制：只有混元散仙可直接使用

- **火土阳**: "烈阳焚土" - 需要火土阳各1个
  - 效果：大范围持续燃烧地带
  - 职业限制：需要游客机制支持

#### 卡牌稀有度与属性需求
- **普通卡牌**: 单属性，基础效果
- **稀有卡牌**: 单属性，特殊效果或双属性简单组合
- **史诗卡牌**: 双属性混合，强力效果
- **传奇卡牌**: 三属性混合，改变战局的存在
- **神话卡牌**: 阴阳五行全属性，终极技能

## 3. 修炼卡牌系统

### 3.1 卡牌类型分类

#### 主要卡牌类型（80%）
- **技能卡**: 修炼者的核心战斗技能，消耗灵力值释放
- **功法卡**: 被动增强效果，提升修炼者基础属性
- **法宝卡**: 装备类卡牌，提供持续的战斗加成

#### 辅助卡牌类型（20%）
- **丹药卡**: 即时恢复效果，恢复生命值或灵力值
- **阵法卡**: 改变战场环境的特殊卡牌
- **符咒卡**: 一次性强力效果，关键时刻扭转战局

### 3.2 修炼卡牌属性系统

#### 基础属性
- **灵力消耗**: 释放技能需要的灵力值
- **伤害值**: 技能造成的伤害数值
- **冷却时间**: 技能再次使用的间隔时间
- **属性需求**: 使用卡牌需要的属性槽位

#### 特殊属性
- **修炼境界**: 决定卡牌威力的境界要求
- **五行属性**: 卡牌的五行归属（金木水火土）
- **阴阳性质**: 卡牌的阴阳属性（阴性/阳性/中性）
- **相生相克**: 与其他属性卡牌的相互作用

### 3.3 卡牌使用机制

#### 使用限制
- **灵力限制**: 每回合有限的灵力值
- **属性匹配**: 必须满足卡牌的属性需求
- **职业限制**: 某些卡牌只有特定职业可以使用

#### 使用策略
- **连招组合**: 特定卡牌组合可以产生连击效果
- **属性共鸣**: 同属性卡牌连续使用时效果增强
- **相生增益**: 按五行相生顺序使用卡牌获得额外效果

```
卡牌使用流程:
1. 选择技能卡牌
2. 检查属性需求
3. 消耗灵力值
4. 释放技能效果
5. 检查相生相克
6. 触发连招效果
```

## 4. 修炼者职业系统

### 4.1 修炼者作为战斗核心

#### 修炼者定位
- **修炼角色**: 修炼者可以通过打字直接造成伤害，同时使用修炼卡牌进行战斗
- **伤害占比变化**: 游戏初期修炼者打字伤害占主导，后期卡牌技能成为主要伤害来源
- **属性亲和**: 每个职业对特定阴阳五行属性有亲和性
- **修炼加成**: 为特定属性的卡牌提供使用和威力加成
- **打字修炼**: 修炼者通过打字系统直接对敌人造成伤害，体现修炼功力

#### 修炼者属性槽位系统
- **初始槽位**: 每个修炼者有3个属性槽位
- **槽位升级**: 通过境界提升可以解锁更多槽位（最多7个）
- **属性专精**: 修炼者对主要属性有额外加成

### 4.2 修炼者类型与属性倾向

#### 单属性专精修炼者
```
太极道士: [阴][阳][中] - 专精阴阳调和和平衡技能
金刚武僧: [金][金][阳] - 专精金属性和物理攻击
青木法师: [木][木][阴] - 专精木属性和治疗恢复
玄水道人: [水][水][阴] - 专精水属性和控制流转
烈火剑仙: [火][火][阳] - 专精火属性和爆发攻击
厚土仙人: [土][土][阴] - 专精土属性和防御稳固
```

#### 混合属性修炼者
```
混元散仙: [万][万][万] - 可使用任意属性卡牌
金木双修: [金][木][阴] - 坚韧与生长的结合
水火既济: [水][火][中] - 流动与爆发的平衡
土金相生: [土][金][阳] - 稳固与坚韧的融合
木火通明: [木][火][阳] - 生长与光明的和谐
```

### 4.3 修炼者技能与战斗机制

#### 打字修炼系统
- **直接伤害**: 修炼者通过打字对敌人造成即时伤害
- **伤害衰减**: 随着境界提升，修炼者打字伤害占比从90%逐渐降至30%
- **卡牌崛起**: 后期卡牌技能伤害占比从10%逐渐提升至70%
- **协同修炼**: 修炼者打字和卡牌技能可以产生协同效果

#### 修炼者技能
- **属性增幅**: 减少特定属性卡牌的灵力消耗
- **技能强化**: 提升特定类型卡牌的威力
- **相生增强**: 强化五行相生效果
- **特殊功法**: 解锁独特的卡牌或使用方式
- **打字强化**: 提升修炼者打字伤害和效果

#### 修炼者被动效果
- **属性共鸣**: 场上同属性卡牌越多，效果越强
- **修炼精通**: 特定属性卡牌获得额外能力
- **境界掌控**: 影响整个战场的环境效果
- **打字精通**: 提升打字速度和准确度的伤害加成

## 5. 修炼卡牌获取系统

### 5.1 获取渠道

#### 战斗感悟（40%）
- **胜利感悟**: 每次战斗胜利后获得1-3张修炼卡牌
- **完美胜利**: 无损胜利额外获得稀有功法卡牌
- **连胜奖励**: 连续胜利提升修炼卡牌品质
- **击败特定敌人**: 有概率领悟对应属性的修炼技能

#### 宗门传承（30%）
- **基础传承**: 消耗基础资源学习普通功法
- **高级传承**: 消耗稀有资源学习强力功法
- **属性传承**: 针对特定阴阳五行属性的专门传承
- **职业传承**: 有更高概率获得特定职业的功法组合

#### 特殊机缘（20%）
- **精英对决**: 击败精英敌人获得强力修炼卡牌
- **神秘商人**: 用特殊货币购买稀有功法
- **古老洞府**: 探索获得传说级功法
- **天地感应**: 通过特殊条件获得独特修炼法门

#### 炼制合成（10%）
- **功法融合**: 使用多张低级功法合成高级功法
- **材料炼制**: 收集天材地宝炼制特定卡牌
- **丹药转化**: 将丹药转化为对应属性的修炼卡牌

### 5.2 获取限制规则

#### 属性匹配限制
- **职业亲和**: 更容易获得职业属性倾向的修炼卡牌
- **属性发现**: 新属性的功法需要特殊条件才能获得
- **属性平衡**: 系统会平衡不同属性功法的获得概率

#### 稀有度渐进
- **初期**: 主要获得基础功法（单属性需求）
- **中期**: 开始出现进阶功法（双属性需求）
- **后期**: 有机会获得高级和神话功法（多属性需求）
- **进阶条件**: 高稀有度功法需要满足特定的修炼条件

#### 相生相克获取机制
- **套装获取**: 某些功法以套装形式出现，便于组成相生相克
- **属性提示**: 系统会提示玩家缺少的属性功法
- **属性强化**: 已有属性亲和的玩家更容易获得相关功法

#### 5.2.1 属性槽位未满时
**修炼者只有1个金属性槽位：**
- 可获取：除了"三种属性都不是金属性"的所有修炼卡牌
- 禁止获取：[木,水,火]、[木,水,土]等不含金属性的修炼卡牌

**修炼者有2个金属性槽位：**
- 可获取：至少包含一种金属性或只有一种非金属性的修炼卡牌
- 禁止获取：包含两种或以上非金属性的修炼卡牌

#### 5.2.2 属性槽位填满后
**修炼者属性槽位为 [金,金,木]：**
- 只能获取包含金属性和/或木属性的修炼卡牌
- 禁止获取包含水、火、土、阴、阳属性的修炼卡牌

## 6. 技术实现要点

### 6.1 修炼卡牌数据结构

```csharp
public class CultivationCard : Resource
{
    public string CardName { get; set; }
    public CardRarity Rarity { get; set; }
    public WuxingAttribute[] AttributeRequirements { get; set; }
    public int SpiritCost { get; set; }
    public int Damage { get; set; }
    public int CooldownTime { get; set; }
    public CultivationCardType CardType { get; set; }
    public string[] Skills { get; set; }
    public ElementalAffinity[] Affinities { get; set; }
    public string Description { get; set; }
}

public enum WuxingAttribute
{
    Yin,        // 阴
    Yang,       // 阳
    Metal,      // 金
    Wood,       // 木
    Water,      // 水
    Fire,       // 火
    Earth,      // 土
    Neutral     // 中性
}

public enum CultivationCardType
{
    SkillCard,      // 技能卡
    TechniqueCard,  // 功法卡
    TreasureCard,   // 法宝卡
    PillCard,       // 丹药卡
    FormationCard,  // 阵法卡
    TalismanCard    // 符咒卡
}

public enum ElementalAffinity
{
    MetalGeneratesWater,    // 金生水
    WaterGeneratesWood,     // 水生木
    WoodGeneratesFire,      // 木生火
    FireGeneratesEarth,     // 火生土
    EarthGeneratesMetal,    // 土生金
    MetalOvercomesWood,     // 金克木
    WoodOvercomesEarth,     // 木克土
    EarthOvercomesWater,    // 土克水
    WaterOvercomesFire,     // 水克火
    FireOvercomesMetal,     // 火克金
    YinYangBalance,         // 阴阳平衡
    YinYangConversion       // 阴阳转换
}
```

### 6.2 修炼者职业系统

```csharp
public class Cultivator : Resource
{
    public string CultivatorName { get; set; }
    public WuxingAttribute[] AttributeSlots { get; set; }
    public int MaxAttributeSlots { get; set; } = 7;
    public CultivatorProfession Profession { get; set; }
    public Dictionary<WuxingAttribute, float> AttributeBonus { get; set; }
    public Dictionary<CultivationCardType, float> CardTypeBonus { get; set; }
    
    // 打字修炼相关属性
    public float TypingDamageBase { get; set; } = 100f;
    public float TypingDamageDecayRate { get; set; } = 0.1f; // 每境界衰减率
    public float TypingSpeedBonus { get; set; } = 1.0f;
    public float TypingAccuracyBonus { get; set; } = 1.0f;
    public CultivationRealm CurrentRealm { get; set; }
    
    public bool CanUseCultivationCard(CultivationCard card)
    {
        return AttributeSlot.CanSatisfyRequirements(AttributeSlots, card.AttributeRequirements);
    }
    
    public float GetCultivationBonus(CultivationCard card)
    {
        float bonus = 0f;
        
        // 属性加成
        foreach (var attr in card.AttributeRequirements)
        {
            if (AttributeBonus.ContainsKey(attr))
                bonus += AttributeBonus[attr];
        }
        
        // 卡牌类型加成
        if (CardTypeBonus.ContainsKey(card.CardType))
            bonus += CardTypeBonus[card.CardType];
            
        return bonus;
    }
    
    public float CalculateTypingDamage(int currentRealm, float typingSpeed, float accuracy)
    {
        // 基础伤害随境界衰减
        float realmDecay = Mathf.Pow(1f - TypingDamageDecayRate, currentRealm - 1);
        float baseDamage = TypingDamageBase * realmDecay;
        
        // 打字速度和准确度加成
        float speedMultiplier = 1f + (typingSpeed - 1f) * TypingSpeedBonus;
        float accuracyMultiplier = 1f + (accuracy - 1f) * TypingAccuracyBonus;
        
        return baseDamage * speedMultiplier * accuracyMultiplier;
    }
}

public enum CultivatorProfession
{
    TaijiDaoist,        // 太极道士
    VajraMonk,          // 金刚武僧
    GreenWoodMage,      // 青木法师
    XuanWaterDaoist,    // 玄水道人
    FireSwordImmortal,  // 烈火剑仙
    EarthImmortal,      // 厚土仙人
    HunyuanScatteredImmortal  // 混元散仙
}

public enum CultivationRealm
{
    QiRefining,         // 炼气期
    Foundation,         // 筑基期
    GoldenCore,         // 金丹期
    NascentSoul,        // 元婴期
    SoulTransform,      // 化神期
    VoidReturn,         // 返虚期
    BodyIntegration,    // 合体期
    Mahayana,           // 大乘期
    Tribulation,        // 渡劫期
    Immortal            // 仙人境
}
```

### 6.3 修炼与五行相生相克系统

```csharp
public class CultivationSystem : Node
{
    public const int MAX_ACTIVE_CARDS = 7;
    public List<CultivationCard> ActiveCards { get; private set; } = new List<CultivationCard>();
    public Dictionary<ElementalAffinity, int> ActiveAffinities { get; private set; } = new Dictionary<ElementalAffinity, int>();
    
    public bool CanUseCultivationCard(CultivationCard card, Cultivator cultivator)
    {
        return ActiveCards.Count < MAX_ACTIVE_CARDS && 
               cultivator.CanUseCultivationCard(card) &&
               cultivator.CurrentSpiritPower >= card.SpiritCost;
    }
    
    public void UseCultivationCard(CultivationCard card)
    {
        if (ActiveCards.Count >= MAX_ACTIVE_CARDS)
        {
            // 替换最老的卡牌或让玩家选择
            ReplaceActiveCard(card);
        }
        else
        {
            ActiveCards.Add(card);
        }
        
        UpdateElementalAffinities();
        CalculateWuxingEffects();
    }
    
    private void UpdateElementalAffinities()
    {
        ActiveAffinities.Clear();
        
        // 计算五行相生相克
        foreach (var affinity in System.Enum.GetValues<ElementalAffinity>())
        {
            int count = CountCardsWithAffinity(affinity);
            if (count >= 2)
            {
                ActiveAffinities[affinity] = count;
            }
        }
    }
    
    private int CountCardsWithAffinity(ElementalAffinity affinity)
    {
        return ActiveCards.Count(card => card.Affinities.Contains(affinity));
    }
    
    private void CalculateWuxingEffects()
    {
        // 计算五行相生相克效果
        foreach (var card in ActiveCards)
        {
            float damageMultiplier = 1.0f;
            
            // 相生加成
            damageMultiplier *= CalculateGenerationBonus(card);
            
            // 相克制衡
            damageMultiplier *= CalculateOvercomingEffect(card);
            
            // 阴阳调和
            damageMultiplier *= CalculateYinYangBalance(card);
            
            card.ApplyDamageMultiplier(damageMultiplier);
        }
    }
}
```

### 6.4 五行相生相克效果系统

```csharp
public class WuxingEffectSystem : Node
{
    public Dictionary<ElementalAffinity, WuxingEffect> WuxingEffects { get; private set; }
    
    public void ApplyWuxingEffects(List<CultivationCard> cards, Dictionary<ElementalAffinity, int> activeAffinities)
    {
        foreach (var affinity in activeAffinities)
        {
            var effect = WuxingEffects[affinity.Key];
            var affectedCards = cards.Where(c => c.Affinities.Contains(affinity.Key)).ToList();
            
            effect.Apply(affectedCards, affinity.Value);
        }
    }
    
    public float CalculateGenerationBonus(CultivationCard card)
    {
        float bonus = 1.0f;
        
        // 检查五行相生
        foreach (var attr in card.AttributeRequirements)
        {
            switch (attr)
            {
                case WuxingAttribute.Metal:
                    if (HasActiveAttribute(WuxingAttribute.Earth)) bonus *= 1.2f;
                    break;
                case WuxingAttribute.Water:
                    if (HasActiveAttribute(WuxingAttribute.Metal)) bonus *= 1.2f;
                    break;
                case WuxingAttribute.Wood:
                    if (HasActiveAttribute(WuxingAttribute.Water)) bonus *= 1.2f;
                    break;
                case WuxingAttribute.Fire:
                    if (HasActiveAttribute(WuxingAttribute.Wood)) bonus *= 1.2f;
                    break;
                case WuxingAttribute.Earth:
                    if (HasActiveAttribute(WuxingAttribute.Fire)) bonus *= 1.2f;
                    break;
            }
        }
        
        return bonus;
    }
    
    public float CalculateOvercomingEffect(CultivationCard card)
    {
        float effect = 1.0f;
        
        // 检查五行相克
        foreach (var attr in card.AttributeRequirements)
        {
            switch (attr)
            {
                case WuxingAttribute.Metal:
                    if (HasActiveAttribute(WuxingAttribute.Fire)) effect *= 0.8f;
                    if (HasTargetAttribute(WuxingAttribute.Wood)) effect *= 1.3f;
                    break;
                case WuxingAttribute.Wood:
                    if (HasActiveAttribute(WuxingAttribute.Metal)) effect *= 0.8f;
                    if (HasTargetAttribute(WuxingAttribute.Earth)) effect *= 1.3f;
                    break;
                case WuxingAttribute.Water:
                    if (HasActiveAttribute(WuxingAttribute.Earth)) effect *= 0.8f;
                    if (HasTargetAttribute(WuxingAttribute.Fire)) effect *= 1.3f;
                    break;
                case WuxingAttribute.Fire:
                    if (HasActiveAttribute(WuxingAttribute.Water)) effect *= 0.8f;
                    if (HasTargetAttribute(WuxingAttribute.Metal)) effect *= 1.3f;
                    break;
                case WuxingAttribute.Earth:
                    if (HasActiveAttribute(WuxingAttribute.Wood)) effect *= 0.8f;
                    if (HasTargetAttribute(WuxingAttribute.Water)) effect *= 1.3f;
                    break;
            }
        }
        
        return effect;
    }
}

public abstract class WuxingEffect
{
    public abstract void Apply(List<CultivationCard> cards, int affinityCount);
}
```

### 6.5 打字修炼战斗系统

```csharp
public class TypingCultivationSystem : Node
{
    public Cultivator CurrentCultivator { get; set; }
    public CultivationSystem CultivationSystem { get; set; }
    
    public float CalculateTotalDamage(string typedWord, float typingSpeed, float accuracy, int currentRealm)
    {
        // 修炼者打字伤害
        float cultivatorTypingDamage = CurrentCultivator.CalculateTypingDamage(currentRealm, typingSpeed, accuracy);
        
        // 修炼卡牌协同伤害
        float cardSynergyDamage = CalculateCardSynergy(typedWord);
        
        // 总伤害
        return cultivatorTypingDamage + cardSynergyDamage;
    }
    
    private float CalculateCardSynergy(string typedWord)
    {
        float synergyDamage = 0f;
        
        foreach (var card in CultivationSystem.ActiveCards)
        {
            // 根据卡牌属性和打字内容计算协同伤害
            if (IsWordMatchCardAttribute(typedWord, card))
            {
                synergyDamage += card.Damage * 0.1f; // 10%协同伤害
            }
        }
        
        return synergyDamage;
    }
    
    private bool IsWordMatchCardAttribute(string word, CultivationCard card)
    {
        // 根据卡牌属性判断是否与打字内容匹配
        // 例如：火属性卡牌与包含"fire"的单词匹配
        foreach (var attr in card.AttributeRequirements)
        {
            if (word.ToLower().Contains(GetAttributeKeyword(attr)))
                return true;
        }
        return false;
    }
    
    private string GetAttributeKeyword(WuxingAttribute attribute)
    {
        return attribute switch
        {
            WuxingAttribute.Fire => "fire",
            WuxingAttribute.Water => "water",
            WuxingAttribute.Wood => "wood",
            WuxingAttribute.Metal => "metal",
            WuxingAttribute.Earth => "earth",
            WuxingAttribute.Yin => "yin",
            WuxingAttribute.Yang => "yang",
            _ => ""
        };
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