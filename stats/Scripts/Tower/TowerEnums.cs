using Godot;

/// <summary>
/// 爬塔系统枚举定义
/// </summary>

/// <summary>
/// 塔层类型 - 基于51层修仙高塔的设计
/// </summary>
public enum TowerFloorType
{
    // 第一关卡：凡俗层（1-17层）
    UrbanEnvironment,      // 1-5层：现代城市环境
    TraditionalLab,        // 6-11层：传统修炼场所与科技实验室
    VirtualCultivation,    // 12-17层：虚拟修炼空间
    
    // 第二关卡：灵能层（18-34层）
    AncientSect,           // 18-22层：古代宗门虚拟重现
    TechFusion,            // 23-28层：灵能与科技融合实验空间
    AdvancedVirtual,       // 29-34层：高级虚拟修炼环境
    
    // 第三关卡：天机层（35-51层）
    MythicalRealm,         // 35-42层：神话级修炼环境
    UltimateTrial,         // 43-50层：终极试炼
    CoreSystem,            // 第51层：天梯系统核心
    
    // 隐藏关卡：塔底废墟（第0层）
    EndlessRuins,          // 无尽验证模式
    
    // 特殊区域
    SpiritualRealm,        // 灵境
    HiddenRuins,           // 隐藏废墟
    
    // 数据流相关
    DataStream,            // 数据流层
    QuantumSpace,          // 量子空间
    ConsciousnessNet       // 意识网络
}

/// <summary>
/// 地图节点类型
/// </summary>
public enum MapNodeType
{
    // 战斗节点
    NormalBattle,          // 普通战斗
    EliteBattle,           // 精英战斗
    BossBattle,            // Boss战斗
    
    // 功能节点
    CardConstruct,         // 卡牌构筑室
    SpiritualCharge,       // 灵能充电站
    MemoryFragment,        // 记忆碎片库
    TeleportGate,          // 试炼传送门
    
    // 事件节点
    RandomEvent,           // 随机事件
    SectTrial,             // 宗门试炼
    TechFusion,            // 科技融合
    NetworkConsciousness,  // 网络神识
    
    // 商店节点
    TechMarket,            // 技术交易市场
    CultivatorTavern,      // 修炼者酒馆
    OpenLibrary,           // 开源图书馆
    
    // 特殊节点
    RestSite,              // 休息点
    Treasure,              // 宝藏
    Mystery,               // 神秘事件
    Choice,                // 选择事件
    
    // 起始和结束
    Start,                 // 起始点
    Exit                   // 出口
}

/// <summary>
/// 地图拓扑结构类型
/// </summary>
public enum MapTopologyType
{
    Linear,                // 线性结构
    Branching,             // 分支结构
    Grid,                  // 网格结构
    Spiral,                // 螺旋结构
    Maze,                  // 迷宫结构
    Hub,                   // 中心辐射结构
    Random,                // 随机结构
    Branch,                // 分支结构
    Circular,              // 环形结构
    Tree,                  // 树形结构
    Network,               // 网络结构
    Ring                   // 环形结构
}

/// <summary>
/// 地图难度等级
/// </summary>
public enum MapDifficulty
{
    Easy,                  // 简单
    Normal,                // 普通
    Hard,                  // 困难
    Expert,                // 专家
    Master,                // 大师
    Legendary              // 传奇
}

/// <summary>
/// 地图生成规则
/// </summary>
public enum MapGenerationRule
{
    Balanced,              // 平衡分布
    CombatFocused,         // 战斗导向
    EventRich,             // 事件丰富
    EventFocused,          // 事件导向
    ResourceAbundant,      // 资源丰富
    ResourceFocused,       // 资源导向
    ChallengeIntensive,    // 挑战密集
    Challenging,           // 挑战性
    StoryDriven,           // 剧情驱动
    ExplorationFocused,    // 探索导向
    Ring,                  // 环形布局
    Relaxed                // 轻松模式
}

/// <summary>
/// 节点连接类型
/// </summary>
public enum NodeConnectionType
{
    Normal,                // 普通连接
    Hidden,                // 隐藏连接
    Conditional,           // 条件连接
    OneWay,                // 单向连接
    Locked,                // 锁定连接
    Teleport,              // 传送连接
    Lateral,               // 横向连接
    Skip                   // 跳跃连接
}

/// <summary>
/// 地图层级
/// </summary>
public enum MapLayer
{
    Surface,               // 表层
    Underground,           // 地下层
    Sky,                   // 天空层
    Virtual,               // 虚拟层
    Dimensional           // 维度层
}

/// <summary>
/// 地图状态
/// </summary>
public enum MapState
{
    NotGenerated,          // 未生成
    Locked,                // 锁定
    Available,             // 可用
    InProgress,            // 进行中
    Completed,             // 已完成
    Failed                 // 失败
}