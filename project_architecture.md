# CodeRogue 项目架构图

## 整体架构概览

```mermaid
graph TB
    subgraph "游戏入口层"
        Main[Main.tscn]
        MainMenu[MainMenu]
        GameUI[GameUI]
    end

    subgraph "核心管理层"
        GameManager[GameManager]
        MonsterGameManager[MonsterGameManager]
        AudioManager[AudioManager]
        ConfigManager[ConfigManager]
        InputManager[InputManager]
        UIManager[UIManager]
    end

    subgraph "卡牌系统"
        BaseCard[BaseCard - 抽象基类]
        MonsterCard[MonsterCard]
        SkillCard[SkillCard]
        
        BaseDeck[BaseDeck - 抽象基类]
        UnifiedDeck[UnifiedDeck]
        
        
        DeckManager[DeckManager]
    end

    subgraph "战斗系统"
        SummonSystem[SummonSystem]
        TypingCombatSystem[TypingCombatSystem]
        SummonerHero[SummonerHero]
        SkillTrackSystem[SkillTrackSystem]
        SkillTrackManager[SkillTrackManager]
        MonsterManager[MonsterManager]
    end

    subgraph "数据库层"
        SkillDatabase[SkillDatabase]
        HeroDatabase[HeroDatabase]
        RelicDatabase[RelicDatabase]
        BuffDatabase[BuffDatabase]
    end

    subgraph "增强系统"
        BuffManager[BuffManager]
        RelicManager[RelicManager]
        HeroManager[HeroManager]
    end

    subgraph "UI系统"
        SkillDeckUI[SkillDeckUI]
        SkillCardUI[SkillCardUI]
        SkillTrackUI[SkillTrackUI]
        GameOverScreen[GameOverScreen]
        PauseMenu[PauseMenu]
        SettingsMenu[SettingsMenu]
    end

    subgraph "工具系统"
        WordManager[WordManager]
        LevelManager[LevelManager]
    end

    subgraph "枚举定义"
        CardEnums[CardEnums]
        SkillEnums[SkillEnums]
        MonsterEnums[MonsterEnums]
        HeroEnums[HeroEnums]
        BuffEnums[BuffEnums]
    end

    %% 连接关系
    Main --> GameManager
    Main --> MonsterGameManager
    
    MonsterGameManager --> SummonSystem
    MonsterGameManager --> TypingCombatSystem
    MonsterGameManager --> MonsterManager
    MonsterGameManager --> DeckManager
    
    BaseCard --> MonsterCard
    BaseCard --> SkillCard
    
    BaseDeck --> UnifiedDeck
    
    DeckManager --> UnifiedDeck
    
    MonsterManager --> MonsterCard
    SkillDatabase --> SkillCard
    
    SummonSystem --> MonsterCard
    SummonSystem --> SummonerHero
    TypingCombatSystem --> SkillCard
    
    SkillTrackManager --> SkillTrackSystem
    SkillTrackManager --> UnifiedDeck
    
    BuffManager --> BuffDatabase
    RelicManager --> RelicDatabase
    HeroManager --> HeroDatabase
    
    SkillDeckUI --> DeckManager
    SkillCardUI --> SkillCard
    SkillTrackUI --> SkillTrackManager
    
    GameUI --> UIManager
    MainMenu --> UIManager
```

## 核心模块详细说明

### 1. 卡牌系统架构

```mermaid
classDiagram
    class BaseCard {
        <<abstract>>
        +string CardId
        +string CardName
        +string Description
        +CardType Type
        +CardRarity Rarity
        +int Cost
        +Dictionary ColorRequirements
        +CalculatePower()*
        +CreateCopy()*
    }
    
    class MonsterCard {
        +int Attack
        +int Health
        +int Armor
        +MonsterRace Race
        +List~MonsterSkillType~ Skills
        +BondType BondType
    }
    
    class SkillCard {
        +SkillType SkillType
        +float DamageMultiplier
        +int DefenseValue
        +int CooldownTurns
        +int ChargeCost
    }
    
    class BaseDeck {
        <<abstract>>
        +string DeckName
        +List~BaseCard~ Cards
        +int MaxDeckSize
        +List~BaseCard~ DrawPile
        +List~BaseCard~ DiscardPile
        +AddCard()
        +RemoveCard()
        +Shuffle()
        +DrawCard()
        +CalculateSpecificStats()*
    }
    
    class UnifiedDeck {
        +List~MonsterCard~ MonsterCards
        +List~SkillCard~ SkillCards
        +DrawMonsterCard()
        +DrawSkillCard()
        +GetCardTypeDistribution()
        +CalculateDeckScore()
    }
    
    BaseCard <|-- MonsterCard
    BaseCard <|-- SkillCard
    BaseDeck <|-- UnifiedDeck
    BaseDeck o-- BaseCard
```

### 2. 管理器系统架构

```mermaid
classDiagram
    class MonsterGameManager {
        +GameState CurrentState
        +SummonerHero CurrentSummoner
        +SummonSystem _summonSystem
        +TypingCombatSystem _typingCombatSystem
        +MonsterManager _cardManager
    +DeckManager _deckManager
        +ChangeGameState()
        +ProcessTypingInput()
        +SummonMonster()
    }
    
    class DeckManager {
        +UnifiedDeck CurrentDeck
        +Array~UnifiedDeck~ SavedDecks
        +CreateNewDeck()
        +SetCurrentDeck()
        +AddCardToDeck()
        +RemoveCardFromDeck()
        +SaveAllDecks()
        +LoadAllDecks()
    }
    
    class MonsterManager {
        +Array~MonsterCard~ AllCards
        +Dictionary~string, MonsterCard~ CardDatabase
        +GetRandomCard()
        +GetCardsByRarity()
        +GetCardsByColor()
        +AddToCollection()
    }
    
    class SkillDatabase {
        +Array~SkillCard~ AllSkills
        // 移除SkillDeck数组，现在使用UnifiedDeck
        +GetSkillByName()
        +GetDeckByName()
        +GetSkillsByTag()
    }
    
    MonsterGameManager --> DeckManager
    MonsterGameManager --> MonsterManager
    DeckManager --> UnifiedDeck
    MonsterManager --> MonsterCard
    SkillDatabase --> SkillCard
```

### 3. 战斗系统架构

```mermaid
classDiagram
    class SummonSystem {
        +List~MonsterCard~ SummonedMonsters
        +int MaxSummonPoints
        +int CurrentSummonPoints
        +SummonerHero CurrentSummoner
        +SummonMonster()
        +CalculateBondEffects()
        +CheckSummonConditions()
    }
    
    class TypingCombatSystem {
        +float BaseTypingDamage
        +float BaseDecayRate
        +Dictionary~string, float~ KeywordBonuses
        +ProcessTypingInput()
        +CalculateDamage()
        +ApplyDecay()
    }
    
    class SummonerHero {
        +string HeroName
        +MagicColor PrimaryColor
        +float BaseTypingDamage
        +List~ColorSlot~ ColorSlots
        +CheckColorRequirements()
        +GetAvailableColors()
    }
    
    class SkillTrackManager {
        +List~SkillTrack~ ActiveTracks
        +UnifiedDeck CurrentDeck
        +ActivateTrack()
        +ProcessSkillEffect()
        +UpdateTracks()
    }
    
    SummonSystem --> SummonerHero
    SummonSystem --> MonsterCard
    TypingCombatSystem --> SkillCard
    SkillTrackManager --> UnifiedDeck
```

### 4. UI系统架构

```mermaid
classDiagram
    class SkillDeckUI {
        +UnifiedDeck _currentDeck
        +List~SkillCardUI~ _cardUIs
        +GridContainer _cardsGrid
        +UpdateDeckDisplay()
        +CreateCardUI()
        +OnCardClicked()
    }
    
    class SkillCardUI {
        +SkillCard _skillCard
        +Label _nameLabel
        +Label _descriptionLabel
        +SetSkillCard()
        +UpdateDisplay()
    }
    
    class SkillTrackUI {
        +List~TrackSlotUI~ _trackSlots
        +SkillTrackManager _trackManager
        +SetupTrackSlots()
        +UpdateTrackDisplay()
        +OnTrackActivated()
    }
    
    SkillDeckUI --> SkillCardUI
    SkillDeckUI --> SkillDeckManager
    SkillTrackUI --> SkillTrackManager
    SkillCardUI --> SkillCard
```

## 数据流向图

```mermaid
flowchart TD
    A[用户输入] --> B[InputManager]
    B --> C[MonsterGameManager]
    
    C --> D{游戏状态}
    D -->|战斗| E[TypingCombatSystem]
    D -->|召唤| F[SummonSystem]
    D -->|卡组构建| G[DeckManager]
    
    E --> H[技能效果计算]
    F --> I[怪物召唤验证]
    G --> J[卡组操作]
    
    H --> K[UI更新]
    I --> K
    J --> K
    
    K --> L[用户反馈]
    
    subgraph "数据持久化"
        M[ConfigManager]
        N[保存文件]
        O[资源数据]
    end
    
    G --> M
    M --> N
    O --> C
```

## 模块依赖关系

### 核心依赖
- **MonsterGameManager**: 整合所有系统的核心管理器
- **DeckManager**: 统一卡组管理，负责牌组的管理
- **BaseCard/BaseDeck**: 提供卡牌和卡组的抽象基础

### 系统间通信
- 使用Godot信号系统进行组件间通信
- 通过Manager类提供统一的API接口
- 数据库类提供静态数据访问

### 扩展性设计
- 抽象基类支持新卡牌类型扩展
- 插件式的增强系统（Buff、Relic、Hero）
- 模块化的UI组件设计

## 技术特点

1. **统一卡牌设计**: 所有卡牌继承自BaseCard，支持多种卡牌类型
2. **万智牌风格五色系统**: 完整的颜色需求和召唤师系统
3. **双重战斗系统**: 打字战斗 + 怪物召唤的创新结合
4. **统一卡组管理**: UnifiedDeck支持混合卡牌类型
5. **模块化架构**: 高内聚低耦合的系统设计
6. **信号驱动**: 基于Godot信号的事件系统
7. **数据持久化**: 完整的保存/加载系统
8. **类型安全**: 强类型的卡组和卡牌访问

## 开发状态

- ✅ 核心卡牌系统
- ✅ 统一卡组管理
- ✅ 战斗系统基础
- ✅ UI框架
- 🔄 增强系统集成
- 🔄 完整战斗流程
- ⏳ 动画和音效
- ⏳ AI和网络功能

---

*此架构图基于当前代码分析生成，展示了CodeRogue项目的完整技术架构和模块关系。*