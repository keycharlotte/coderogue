# CodeRogue 项目开发规则

## 0. Godot优先开发理念

### 0.1 核心原则
- **Godot引擎规则优先**：在Godot中开发C#代码时，必须始终以Godot引擎的规则为准，而不是按照传统的C#/.NET开发方式来思考
- **引擎适配原则**：所有代码设计和实现都应该优先考虑与Godot引擎的兼容性和最佳实践
- **避免传统思维**：摒弃纯C#/.NET的开发习惯，拥抱Godot的开发哲学和技术栈

### 0.2 类型系统优先级
- **Variant系统兼容性**：优先考虑Godot的Variant系统兼容性，避免使用无法序列化的C#特性
- **Godot集合类型**：强制使用`Godot.Collections`命名空间下的集合类型，而非`System.Collections`
- **接口限制认知**：认识到Godot Variant系统对C#接口的限制，优先使用抽象基类或委托模式
- **类型转换谨慎**：在进行类型转换时，始终考虑Godot的序列化和反序列化需求

### 0.3 架构设计理念
- **Node树优先**：架构设计遵循Godot的Node树和场景系统，而不是传统的OOP层次结构
- **场景驱动设计**：优先使用.tscn场景文件定义结构，代码仅负责逻辑实现
- **AutoLoad单例**：使用Godot的AutoLoad机制管理全局状态，而不是传统的单例模式
- **信号事件系统**：优先使用Godot的信号系统进行组件间通信

### 0.4 数据管理原则
- **Godot序列化优先**：数据序列化使用Godot.Collections而非System.Collections
- **Resource系统利用**：充分利用Godot的Resource系统进行配置和数据管理
- **Variant兼容设计**：所有数据结构设计都应考虑Variant系统的兼容性要求
- **JSON与Resource结合**：根据需求选择JSON或.tres格式，但都要符合Godot的加载机制

### 0.5 资源管理理念
- **AutoLoad机制**：资源管理遵循Godot的Resource和AutoLoad机制
- **场景预配置**：优先在场景文件中预配置资源引用，减少代码中的动态创建
- **节点生命周期**：遵循Godot的节点生命周期管理，正确使用_Ready()、_Process()等方法
- **内存管理意识**：理解Godot的内存管理机制，避免不必要的对象创建和引用持有

### 0.6 开发思维转换
- **从C#到Godot C#**：将开发思维从"如何用C#实现"转换为"如何在Godot中用C#实现"
- **引擎特性优先**：优先使用Godot提供的特性和工具，而不是重新发明轮子
- **社区最佳实践**：关注Godot社区的最佳实践，而不是通用的C#开发模式
- **性能考量**：始终考虑Godot引擎的性能特点，优化代码以适应游戏开发需求

## 1. 开发环境配置

### 1.1 基础环境
- **开发原则**：能用代码解决的事情优先用代码解决
- **Godot版本**：4.4+

### 1.2 Godot安装路径配置
- **实际安装路径**：`D:\godot\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64.exe`
- **命令行使用**：在PowerShell中使用完整路径执行Godot命令
- **示例命令**：`& "D:\godot\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64.exe" --headless --path . ScenePath`
- **别名设置**：可使用`Set-Alias -Name godot -Value "D:\godot\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64.exe"`设置别名

## 2. 架构设计规则

### 2.1 MVVM架构设计规则
- **架构模式要求**：整个程序设计架构必须遵循MVVM（Model-View-ViewModel）模式
- **View层实现**：
  - **tscn文件专用**：所有View层组件必须在Godot的.tscn文件中实现
  - **禁止代码设置UI**：严禁在代码中设置UI相关的属性，如PresetMode、锚点、边距等
  - **声明式UI**：UI布局和样式完全通过tscn文件的可视化编辑器配置
- **ViewModel层定义**：
  - **xxxUI.cs归属**：所有以UI结尾的C#类（如SaveSystemUI.cs）属于ViewModel层
  - **数据绑定职责**：ViewModel负责处理UI逻辑、数据绑定和用户交互
  - **业务逻辑分离**：ViewModel不应包含核心业务逻辑，应通过调用Manager或Service层实现
- **Model层职责**：
  - **数据管理**：Model层负责数据的存储、获取和业务逻辑处理
  - **Manager类归属**：各种Manager类（如SaveManager、BackupManager等）属于Model层
- **分层交互规则**：
  - **单向依赖**：View依赖ViewModel，ViewModel依赖Model，严禁反向依赖
  - **事件通信**：层间通信优先使用事件系统，避免直接引用
- **代码审查要点**：
  - **检查UI代码**：如发现在C#代码中设置UI属性，必须重构到tscn文件中
  - **职责明确**：确保每个类的职责符合其所属层的定义

### 2.2 Manager和Database架构设计规则
- **AutoLoad配置**：所有Manager和Database类都必须配置为AutoLoad单例
- **严格加载顺序**：必须按以下顺序配置AutoLoad
  1. **GameData** - 游戏核心数据，必须最先加载
  2. **所有Database类** - 配置数据库，在Manager之前加载
  3. **所有Manager类** - 业务逻辑管理器，最后加载
- **加载顺序示例**：GameData → BuffDatabase/CardDatabase/HeroDatabase/RelicDatabase → GameManager/AudioManager等
- **场景文件优先**：所有Manager类都应该使用.tscn场景文件而不是直接引用.cs文件
- **场景文件位置**：Manager场景文件统一存放在`Scenes/Manager/`文件夹中
- **节点预配置**：在Manager场景文件中预先创建必要的子节点（如Timer、AudioStreamPlayer等）
- **避免动态创建**：Manager类应通过`GetNode<T>()`获取预配置的节点，避免在代码中使用`new`和`AddChild()`
- **Export属性配置**：Manager的Export属性应在Godot编辑器中配置，而不是在代码中硬编码
- **依赖注入**：非Node类需要Database实例时，应通过方法参数传入，而不是内部创建
- **单例访问**：Manager类通过AutoLoad路径访问Database，如`GetNode<CardDatabase>("/root/CardDatabase")`
- **初始化检查**：所有Manager类都应添加空值检查和错误处理，确保依赖的Database已正确加载

### 2.3 Database类设计规则
- **继承Node**：所有Database类都应继承自Node，而不是RefCounted
- **移除单例模式**：不使用传统的单例模式（Instance属性），改用AutoLoad
- **自动初始化**：Database类在`_Ready()`方法中自动加载配置，无需手动调用
- **配置加载**：使用`LoadConfigs()`方法在`_Ready()`中自动加载数据
- **避免手动实例化**：其他类不应通过`new Database()`创建实例
- **统一命名**：Database类文件名应以"Database"结尾，如`CardDatabase.cs`

### 2.4 GameData类设计规则
- **必须继承Node**：GameData类必须继承自Node，不能继承Resource，否则AutoLoad会失败
- **自动加载数据**：在`_Ready()`方法中自动调用`LoadGameData()`加载存档
- **JSON存储格式**：使用JSON格式存储游戏数据，而不是Godot的.tres格式
- **文件存储位置**：存档文件保存在`user://savegame.save`
- **错误处理**：加载数据时要有容错机制，文件不存在时使用默认值
- **Export属性**：所有游戏数据属性都应使用`[Export]`标记，便于在编辑器中观察和调试

### 2.5 配置数据管理规则
- **禁止硬编码配置**：所有配置类数据（如技能参数、怪物属性、关卡设置等）严禁直接写死在代码中
- **Database类加载**：配置数据必须通过相应的Database类从外部文件加载
- **支持的文件格式**：
  - **JSON文件**：推荐用于复杂配置数据，便于版本控制和外部编辑
  - **Resource文件(.tres)**：适用于Godot特有的资源配置
- **文件存储位置**：配置文件统一存放在`ResourcesData/`文件夹中
- **动态加载**：支持运行时重新加载配置，便于调试和热更新
- **默认值机制**：当配置文件缺失或损坏时，应提供合理的默认值
- **配置验证**：Database类应对加载的配置数据进行有效性验证
- **示例实现**：
  ```csharp
  // 错误示例：硬编码配置
  public float SkillDamage = 100f;
  
  // 正确示例：通过Database加载
  public float SkillDamage => CardDatabase.GetSkillCardById(skillId).Damage;
  ```

## 3. 代码规范

### 3.1 类文件组织规则
- **一个类一个文件原则**：每个C#文件只能包含一个主要的类定义
- **例外情况**：紧密相关的小型辅助类（如内部类、数据结构）可以放在同一文件中
- **文件命名**：文件名必须与类名完全一致

### 3.2 枚举类型组织
- **集中管理**：相关的枚举类型应该集中在专门的Enums文件中
- **命名规范**：枚举文件命名格式为`{模块名}Enums.cs`
- **示例**：`BuffEnums.cs`、`SkillEnums.cs`、`RelicEnums.cs`

### 3.3 字典类型使用规则
- **强制使用Godot字典**：在任何情况下都只使用`Godot.Collections.Dictionary<TKey, TValue>`，不使用`System.Collections.Generic.Dictionary<TKey, TValue>`
- **统一性原则**：确保项目中所有字典类型的一致性，避免类型混用导致的序列化和兼容性问题
- **引用命名空间**：使用`using Godot.Collections;`引用命名空间，直接使用`Dictionary<TKey, TValue>`
- **适用范围**：此规则适用于所有场景，包括但不限于：
  - 数据存储和缓存
  - 配置信息管理
  - 统计信息收集
  - 游戏状态管理
- **迁移要求**：现有代码中的`System.Collections.Generic.Dictionary`应逐步迁移为`Godot.Collections.Dictionary`

### 3.4 Resource类GetNode使用限制
- **严禁直接调用GetNode**：继承自`Resource`的类（如`SkillEffect`、`BuffConfig`等）不能直接调用`GetNode()`方法
- **正确的访问方式**：必须通过`Engine.GetMainLoop()`获取场景树根节点，然后调用`GetNode()`
- **标准模式**：
  ```csharp
  var sceneTree = Engine.GetMainLoop() as SceneTree;
  if (sceneTree?.Root != null)
  {
      var manager = sceneTree.Root.GetNode<ManagerType>("/root/ManagerName");
  }
  ```
- **常见错误**：在`SkillEffect`、`BuffConfig`等Resource类中直接使用`GetNode<T>()`会导致编译错误
- **空值检查**：必须添加空值检查，确保场景树和目标节点存在
- **适用范围**：此规则适用于所有继承自`Resource`的类，包括但不限于技能效果、Buff配置、卡牌配置等

### 3.5 using语句使用规则
- **按需添加原则**：using命令不要想当然地加上去，确实需要了再添加
- **编译驱动**：只有在编译报错的情况下才添加相应的using语句
- **避免冗余引用**：不要预先添加可能用到的命名空间引用
- **最小化原则**：保持using语句列表的简洁，只包含当前文件实际使用的命名空间
- **错误修复时机**：当出现类型未找到或命名空间缺失的编译错误时，再添加对应的using语句
- **代码审查要点**：检查是否存在未使用的using语句，及时清理冗余引用

## 4. UI设计规范

### 4.1 UI组件设计最佳实践
- **优先在场景文件(.tscn)中预先创建UI元素**，而不是在代码中动态创建
- **使用`[Export]`标记来引用场景中的节点**，避免在代码中使用`new`和`AddChild()`
- **Export变量绑定优先在.tscn文件中完成**：在场景文件的根节点脚本属性中直接设置NodePath绑定（如`_skillIcon = NodePath("SkillIcon")`），而不是在代码中使用`GetNode()`方法
- **在所有使用UI元素的方法中添加空值检查**，确保代码健壮性
- **代码只负责逻辑处理**，UI布局和基本属性在场景文件中定义
- **这样可以提高性能、可维护性**，并符合Godot的最佳实践

## 5. 文件组织规则

### 5.1 项目文件组织规则
- **`.tscn`场景文件必须放在`Scenes/`文件夹中**，不要放在`Scripts/`文件夹
- **`.cs`代码文件放在`Scripts/`文件夹中**
- **保持项目结构清晰**：`Scripts/`存放代码，`Scenes/`存放场景
- **这样有助于项目的可维护性和团队协作**

### 5.2 数据类文件夹组织
- **模块化存储**：所有数据类（特别是继承自Resource类的）应该存放在对应模块的单独文件夹中
- **文件夹结构**：按功能模块创建专门的数据文件夹，如`Scripts/Skills/Data/`、`Scripts/Buffs/Data/`、`Scripts/Heroes/Data/`等
- **Resource类归属**：继承自Resource的配置类、数据类应统一放在各自模块的Data子文件夹中
- **示例结构**：
  - `Scripts/Skills/Data/SkillConfig.cs`
  - `Scripts/Buffs/Data/BuffConfig.cs`
  - `Scripts/Heroes/Data/HeroConfig.cs`
  - `Scripts/Relics/Data/RelicConfig.cs`
- **便于维护**：这种组织方式便于数据类的查找、维护和版本控制

### 5.3 测试文件组织
- **模块化测试**：所有example和test文件应该存放在对应模块的test文件夹下面
- **文件夹结构**：按功能模块创建专门的测试文件夹，如`Scripts/Skills/Test/`、`Scripts/Buffs/Test/`、`Scripts/Heroes/Test/`等
- **测试文件归属**：包括但不限于以下类型的文件：
  - 单元测试文件（如`SkillSystemTest.cs`）
  - 示例代码文件（如`SkillTrackUIExample.cs`）
  - 测试运行器文件（如`SkillTestRunner.cs`）
- **示例结构**：
  - `Scripts/Skills/Test/SkillSystemTest.cs`
  - `Scripts/Skills/Test/SkillTestRunner.cs`
  - `Scripts/UI/Test/SkillTrackUIExample.cs`
  - `Scripts/Buffs/Test/BuffSystemTest.cs`
- **便于管理**：这种组织方式便于测试代码的查找、维护和执行
- **避免混淆**：将测试代码与生产代码分离，保持项目结构清晰

### 5.4 文件拆分标准
- **触发条件**：当单个文件包含多个类、枚举或超过200行代码时，应考虑拆分
- **拆分原则**：按功能职责拆分，确保每个文件有明确的单一职责
- **依赖管理**：拆分后确保所有引用关系正确，添加必要的using语句

## 6. Godot特定规则

### 6.1 场景生成器Owner设置规则
- **必须在所有节点添加到场景树后再设置Owner属性**
- **Owner必须是节点在场景树中的祖先节点**
- **使用递归方法统一设置所有子节点的Owner**
- **避免在节点创建函数内部设置Owner**，应在主场景构建完成后统一设置
- **示例模式**：先AddChild()，后SetOwner()或SetOwnerRecursively()

### 6.2 场景文件(.tscn)节点parent属性规则
- **必须指定parent**：所有子节点都必须明确指定parent属性
- **正确的parent值**：子节点的parent应该是其直接父节点的名称
- **根节点的子节点**：直接属于根节点的子节点，parent应设置为"."（表示根节点）
- **禁止包含根节点名称**：parent属性中不应包含根节点的名称，例如不应写成"RootNodeName/ChildNode"，而应写成"ChildNode"
- **相对路径原则**：所有parent路径都应该是相对于根节点的相对路径，不包含根节点名称前缀
- **示例格式**：
  - 根节点的直接子节点：`[node name="MainContainer" type="VBoxContainer" parent="."]`
  - 嵌套子节点：`[node name="Button" type="Button" parent="MainContainer/ButtonContainer"]`
- **常见错误**：
  - 缺少parent属性或parent值不正确会导致"无效或损坏"的引擎错误
  - 在parent路径中包含根节点名称（如"RootNode/MainContainer"）会导致节点无法正确加载

### 6.3 C#颜色使用规则
- **使用Colors类而非Color类**：在Godot 4.x中，预定义颜色应使用`Colors.White`、`Colors.Black`等，而不是`Color.White`
- **常见错误修复**：将所有`Color.White`替换为`Colors.White`，`Color.Black`替换为`Colors.Black`等
- **自定义颜色**：自定义颜色仍使用`new Color(r, g, b, a)`构造函数
- **编译错误**：使用`Color.White`会导致"Color未包含White的定义"编译错误

### 6.4 Godot Variant兼容性规则

#### 6.4.1 问题根本原因
- **Variant系统限制**：Godot的Variant系统无法直接序列化和反序列化C#接口类型
- **泛型Dictionary序列化问题**：`Dictionary<TKey, IInterface>`在Godot中无法正确处理
- **Export属性冲突**：带有`[Export]`标记的Dictionary如果包含接口类型，会在编辑器中报错
- **类型转换错误**：非泛型`Godot.Collections.Dictionary`与泛型`Dictionary<string, Variant>`之间的隐式转换问题

#### 6.4.2 禁止使用的模式
- **严禁在Dictionary中使用接口类型**：
  ```csharp
  // 错误示例：会导致Variant兼容性错误
  private Dictionary<NotificationType, INotificationHandler> _handlers;
  private Dictionary<RewardType, IRewardHandler> _rewardHandlers;
  ```
- **避免非泛型Dictionary赋值**：
  ```csharp
  // 错误示例：类型转换错误
  Dictionary<string, Variant> data = someVariant.AsGodotDictionary();
  ```

#### 6.4.3 推荐解决方案

##### 方案1：抽象基类替代接口（推荐）
```csharp
// 正确示例：使用抽象基类
public abstract partial class NotificationHandlerBase : RefCounted
{
    public abstract void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification);
}

// 具体实现
public partial class AchievementCompletedHandler : NotificationHandlerBase
{
    public override void ConfigureNotificationUI(Control notificationUI, AchievementNotification notification)
    {
        // 实现逻辑
    }
}

// 使用抽象基类的Dictionary
private Dictionary<NotificationType, NotificationHandlerBase> _notificationHandlers;
```

##### 方案2：工厂模式 + 枚举映射
```csharp
public static class NotificationHandlerFactory
{
    public static NotificationHandlerBase CreateHandler(NotificationType type)
    {
        return type switch
        {
            NotificationType.AchievementCompleted => new AchievementCompletedHandler(),
            NotificationType.AchievementUnlocked => new AchievementUnlockedHandler(),
            _ => throw new ArgumentException($"未支持的通知类型: {type}")
        };
    }
}
```

##### 方案3：委托模式（轻量级解决方案）
```csharp
// 使用委托替代接口
public delegate void NotificationConfigureDelegate(Control notificationUI, AchievementNotification notification);
public delegate RewardResult RewardGrantDelegate(string achievementId, RewardConfig reward);

// Dictionary使用委托
private Dictionary<NotificationType, NotificationConfigureDelegate> _notificationConfigurers;
private Dictionary<RewardType, RewardGrantDelegate> _rewardGranters;
```

#### 6.4.4 类型转换最佳实践
- **显式类型转换**：当需要将非泛型Dictionary转换为泛型Dictionary时，使用显式转换
  ```csharp
  // 正确示例：显式转换
  var rawDict = data["parameters"].AsGodotDictionary();
  var typedDict = new Dictionary<string, Variant>();
  foreach (var kvp in rawDict)
  {
      typedDict[kvp.Key.AsString()] = kvp.Value;
  }
  ```
- **空值检查**：始终对Variant转换结果进行空值检查
  ```csharp
  // 正确示例：空值检查
  var statusVariant = progressData["status"];
  if (statusVariant.VariantType != Variant.Type.Nil)
  {
      var statusString = statusVariant.AsString();
      if (!string.IsNullOrEmpty(statusString))
      {
          Enum.TryParse<AchievementStatus>(statusString, out var status);
      }
  }
  ```

#### 6.4.5 开发规避规则
- **设计阶段规避**：在系统设计阶段就避免使用接口类型作为Dictionary的值类型
- **代码审查要点**：重点检查Dictionary类型声明，确保不包含接口类型
- **编译验证**：每次代码修改后必须进行编译验证，及时发现Variant兼容性问题
- **迁移策略**：对于现有的接口Dictionary，采用渐进式迁移，优先迁移核心模块

#### 6.4.6 长期维护建议
- **架构一致性**：在整个项目中保持一致的类型设计模式
- **文档更新**：及时更新相关文档，确保团队成员了解最新的设计规范
- **定期检查**：定期检查代码库，确保没有新的Variant兼容性问题引入
- **工具支持**：考虑开发自动化工具来检测和预防Variant兼容性问题

## 7. 开发流程规则

### 7.1 代码生成流程规则

#### 7.1.1 枚举优先原则
- **先生成枚举类**：在生成具体功能代码之前，必须先生成相关的枚举类定义
- **复核确认**：枚举类生成后，需要等待用户复核确认后再继续生成具体的实现代码
- **依赖明确**：确保所有枚举定义完整且正确，避免后续代码生成时出现类型引用错误

#### 7.1.2 系统级别代码生成三步流程规则
- **适用范围**：涉及到系统级别的更改（如新增系统、重构核心架构、添加新的管理器等）
- **第一步：生成开发文档**：首先生成详细的开发文档，包括系统设计、架构说明、接口定义等
- **第二步：生成枚举和数据结构**：生成相关的枚举类和核心数据结构类，确保类型定义完整
- **第三步：生成具体代码逻辑**：在前两步确认无误后，再生成具体的业务逻辑代码
- **步骤间确认**：每个步骤完成后需要用户确认无误才能进行下一步
- **质量保证**：这种分步骤的方式确保系统级更改的质量和可维护性

#### 7.1.3 自动编译验证规则
- **生成后编译**：每次生成代码后，必须自动执行编译检查，排查和修复编译错误
- **错误修复限制**：如果同一个编译错误连续修复3次后仍未解决，必须停止自动修复
- **错误报告**：达到修复次数限制时，应向用户报告具体的错误信息和已尝试的修复方案
- **手动介入**：复杂错误需要用户手动介入或重新设计解决方案

## 8. 游戏设计规则

### 8.1 技能系统设计
- **技能卡耗能属性**：所有技能卡都有一个耗能属性，没有冷却时间

## 9. GitHub版本控制规则

### 9.1 自动化提交规则
- **重大改动自动上传**：每次进行重大改动时（如系统重构、新功能完成、架构调整等），必须先自动merge并上传到GitHub
- **定时自动上传**：每天18:00自动执行一次代码提交和上传，确保当日工作进度得到保存
- **提交信息规范**：
  - 重大改动提交格式：`feat: [功能描述] - 重大改动自动提交`
  - 定时提交格式：`chore: 每日自动提交 - YYYY-MM-DD 18:00`
- **自动生成改动描述**：
  - 每次提交前必须自动分析代码变更内容
  - 自动生成详细的改动描述，包括：
    - 新增的文件和功能
    - 修改的文件和具体变更
    - 删除的文件和原因
    - 重构的代码模块
    - 修复的bug和问题
  - 改动描述应包含在提交信息的body部分
  - 格式示例：
    ```
    feat: 实现技能系统重构 - 重大改动自动提交
    
    ## 改动内容
    ### 新增文件
    - Scripts/Skills/SkillManager.cs - 技能管理器
    - Scripts/Skills/Data/SkillConfig.cs - 技能配置数据
    
    ### 修改文件
    - Scripts/UI/SkillCardUI.cs - 优化UI显示逻辑
    - project.godot - 添加新的AutoLoad配置
    
    ### 重构内容
    - 将技能系统从单一文件拆分为多个模块
    - 优化MVVM架构实现
    ```
- **分支管理**：
  - 主要开发在`main`分支进行
  - 重大改动前可创建临时分支进行测试
  - 确认无误后merge到main分支
- **冲突处理**：
  - 自动上传前先执行pull操作
  - 如遇冲突，优先保留本地更改
  - 复杂冲突需要手动介入解决
- **备份保障**：通过定时自动上传确保代码安全，避免本地数据丢失

## 10. 数据格式选择规则

### 10.1 Resource vs JSON格式选择指南
- **Resource格式优势**：
  - Godot原生支持，序列化性能优化
  - 类型安全，支持复杂对象引用
  - 编辑器集成，可视化编辑
  - 引用完整性保证
- **Resource格式劣势**：
  - 二进制格式，人类不可读
  - 版本控制不友好，难以进行代码审查
  - 外部工具支持有限
- **JSON格式优势**：
  - 人类可读，便于调试和维护
  - 版本控制友好，支持diff和merge
  - 跨平台通用，外部工具丰富
  - 便于外部编辑和批量处理
- **JSON格式劣势**：
  - 类型限制，需要手动类型转换
  - 性能开销相对较大
  - 无原生引用支持
  - 需要额外的解析和验证逻辑

### 10.2 格式选择建议
- **优先使用JSON的场景**：
  - 简单配置数据（如技能参数、关卡设置）
  - 需要外部编辑的数据
  - 版本控制重要的配置
  - 跨工具使用的数据
- **使用Resource的场景**：
  - 复杂游戏资源（如场景、材质）
  - Godot特有类型（如Texture、AudioStream）
  - 性能敏感的数据加载
  - 需要引用完整性的复杂对象
- **项目统一原则**：
  - 优先使用JSON格式，Resource作为补充
  - 统一存储位置：JSON文件放在`ResourcesData/`，Resource文件按类型分类
  - 通过Database类统一管理数据加载和访问
  - 保持数据格式选择的一致性和可预测性