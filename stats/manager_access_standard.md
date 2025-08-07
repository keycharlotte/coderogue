# Manager获取统一标准设计文档

## 1. 设计原则

### 1.1 核心原则
- **统一性**: 所有Manager获取必须使用统一的方式
- **安全性**: 提供完善的错误处理和空值检查
- **可维护性**: 集中管理，便于修改和扩展
- **性能优化**: 减少重复获取，提供缓存机制
- **类型安全**: 编译时类型检查，避免运行时错误

### 1.2 设计目标
- 消除硬编码的AutoLoad路径
- 提供统一的错误处理机制
- 支持所有类型的调用场景（Node类、Resource类）
- 建立清晰的使用规范

## 2. 标准规范

### 2.1 强制使用NodeUtils
- **规则**: 禁止直接使用`GetNode<T>("/root/ManagerName")`
- **替代**: 必须使用`NodeUtils.GetManagerName(node)`
- **例外**: 无例外情况

### 2.2 方法命名规范
- **格式**: `Get{ManagerName}(Node node)`
- **示例**: `GetGameManager(Node node)`, `GetDeckManager(Node node)`
- **返回值**: 返回Manager实例或null

### 2.3 错误处理标准
- **空值检查**: 所有方法必须包含空值检查
- **错误日志**: 统一的错误信息格式
- **异常处理**: 捕获所有可能的异常
- **降级策略**: 获取失败时的处理方案

### 2.4 Resource类特殊支持
- **专用方法**: 为Resource类提供专门的获取方法
- **格式**: `Get{ManagerName}FromResource()`
- **实现**: 内部处理SceneTree获取逻辑

## 3. 技术实现方案

### 3.1 NodeUtils类扩展

#### 3.1.1 基础Manager获取方法
```csharp
/// <summary>
/// 获取指定Manager的通用方法
/// </summary>
/// <typeparam name="T">Manager类型</typeparam>
/// <param name="node">调用节点</param>
/// <param name="managerName">Manager名称</param>
/// <returns>Manager实例或null</returns>
public static T GetManager<T>(Node node, string managerName) where T : Node
{
    try
    {
        if (node == null)
        {
            GD.PrintErr($"调用节点为空，无法获取{managerName}");
            return null;
        }
        
        var manager = node.GetNode<T>($"/root/{managerName}");
        if (manager == null)
        {
            GD.PrintErr($"未找到{managerName}节点");
        }
        return manager;
    }
    catch (Exception ex)
    {
        GD.PrintErr($"获取{managerName}时发生错误: {ex.Message}");
        return null;
    }
}
```

#### 3.1.2 Resource类专用方法
```csharp
/// <summary>
/// 从Resource类中获取Manager的通用方法
/// </summary>
/// <typeparam name="T">Manager类型</typeparam>
/// <param name="managerName">Manager名称</param>
/// <returns>Manager实例或null</returns>
public static T GetManagerFromResource<T>(string managerName) where T : Node
{
    try
    {
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        if (sceneTree?.Root == null)
        {
            GD.PrintErr($"无法获取场景树，无法获取{managerName}");
            return null;
        }
        
        var manager = sceneTree.Root.GetNode<T>($"/root/{managerName}");
        if (manager == null)
        {
            GD.PrintErr($"未找到{managerName}节点");
        }
        return manager;
    }
    catch (Exception ex)
    {
        GD.PrintErr($"从Resource获取{managerName}时发生错误: {ex.Message}");
        return null;
    }
}
```

#### 3.1.3 具体Manager获取方法
```csharp
// 为每个Manager提供专用方法
public static GameManager GetGameManager(Node node) => GetManager<GameManager>(node, "GameManager");
public static AudioManager GetAudioManager(Node node) => GetManager<AudioManager>(node, "AudioManager");
public static UIManager GetUIManager(Node node) => GetManager<UIManager>(node, "UIManager");
public static DeckManager GetDeckManager(Node node) => GetManager<DeckManager>(node, "DeckManager");
public static BuffManager GetBuffManager(Node node) => GetManager<BuffManager>(node, "BuffManager");
public static SkillTrackManager GetSkillTrackManager(Node node) => GetManager<SkillTrackManager>(node, "SkillTrackManager");
public static InputManager GetInputManager(Node node) => GetManager<InputManager>(node, "InputManager");
public static WordManager GetWordManager(Node node) => GetManager<WordManager>(node, "WordManager");
public static RelicManager GetRelicManager(Node node) => GetManager<RelicManager>(node, "RelicManager");
public static LevelManager GetLevelManager(Node node) => GetManager<LevelManager>(node, "LevelManager");
public static HeroManager GetHeroManager(Node node) => GetManager<HeroManager>(node, "HeroManager");
public static TowerManager GetTowerManager(Node node) => GetManager<TowerManager>(node, "TowerManager");
public static ConfigManager GetConfigManager(Node node) => GetManager<ConfigManager>(node, "ConfigManager");

// Resource类专用方法
public static GameManager GetGameManagerFromResource() => GetManagerFromResource<GameManager>("GameManager");
public static BuffManager GetBuffManagerFromResource() => GetManagerFromResource<BuffManager>("BuffManager");
public static SkillTrackManager GetSkillTrackManagerFromResource() => GetManagerFromResource<SkillTrackManager>("SkillTrackManager");
// ... 其他Manager的Resource版本
```

### 3.2 缓存机制（可选优化）
```csharp
private static readonly Dictionary<string, WeakReference> _managerCache = new();

/// <summary>
/// 带缓存的Manager获取方法
/// </summary>
public static T GetManagerCached<T>(Node node, string managerName) where T : Node
{
    // 检查缓存
    if (_managerCache.TryGetValue(managerName, out var weakRef) && 
        weakRef.IsAlive && weakRef.Target is T cachedManager)
    {
        return cachedManager;
    }
    
    // 获取新实例
    var manager = GetManager<T>(node, managerName);
    if (manager != null)
    {
        _managerCache[managerName] = new WeakReference(manager);
    }
    
    return manager;
}
```

## 4. 使用规范

### 4.1 Node类中的使用
```csharp
// 正确方式
var gameManager = NodeUtils.GetGameManager(this);
var deckManager = NodeUtils.GetDeckManager(this);

// 错误方式（禁止）
var gameManager = GetNode<GameManager>("/root/GameManager");
```

### 4.2 Resource类中的使用
```csharp
// 正确方式
var buffManager = NodeUtils.GetBuffManagerFromResource();
var trackManager = NodeUtils.GetSkillTrackManagerFromResource();

// 错误方式（禁止）
var sceneTree = Engine.GetMainLoop() as SceneTree;
var buffManager = sceneTree.Root.GetNode<BuffManager>("/root/BuffManager");
```

### 4.3 空值检查规范
```csharp
// 必须进行空值检查
var gameManager = NodeUtils.GetGameManager(this);
if (gameManager == null)
{
    GD.PrintErr("无法获取GameManager，操作中止");
    return;
}

// 使用Manager
gameManager.SomeMethod();
```

## 5. 迁移计划

### 5.1 第一阶段：扩展NodeUtils
- 为所有Manager添加获取方法
- 实现Resource类专用方法
- 添加完善的错误处理

### 5.2 第二阶段：代码迁移
- 替换所有直接GetNode调用
- 更新Resource类中的获取方式
- 清理废弃的单例模式引用

### 5.3 第三阶段：验证和优化
- 编译测试确保无错误
- 性能测试和优化
- 建立代码审查规范

## 6. 质量保证

### 6.1 编译时检查
- 使用泛型确保类型安全
- 编译器警告和错误检查

### 6.2 运行时检查
- 完善的空值检查
- 详细的错误日志
- 异常处理机制

### 6.3 代码审查
- 禁止直接GetNode调用
- 强制使用NodeUtils方法
- 检查错误处理完整性

## 7. 性能考虑

### 7.1 获取频率
- 高频获取的Manager考虑缓存
- 低频获取的Manager直接获取

### 7.2 内存管理
- 使用WeakReference避免内存泄漏
- 定期清理无效缓存

### 7.3 错误处理开销
- 平衡错误检查和性能
- 在Debug模式下提供详细日志

## 8. 扩展性

### 8.1 新Manager添加
- 自动生成获取方法的模板
- 统一的命名和实现规范

### 8.2 特殊需求支持
- 条件获取（如仅在特定场景下获取）
- 延迟获取（如需要时才获取）
- 批量获取（如一次获取多个Manager）

这个标准将确保项目中所有Manager获取方式的一致性、安全性和可维护性。