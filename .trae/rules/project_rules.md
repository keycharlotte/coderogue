1. 能用代码解决的事情优先用代码解决
2. godot版本：4.4+
3. **Godot场景生成器Owner设置规则**：
   - 必须在所有节点添加到场景树后再设置Owner属性
   - Owner必须是节点在场景树中的祖先节点
   - 使用递归方法统一设置所有子节点的Owner
   - 避免在节点创建函数内部设置Owner，应在主场景构建完成后统一设置
   - 示例模式：先AddChild()，后SetOwner()或SetOwnerRecursively()
4. 一个类一个文件，不要多个类写在一个文件
5. **UI组件设计最佳实践**：
   - 优先在场景文件(.tscn)中预先创建UI元素，而不是在代码中动态创建
   - 使用`[Export]`标记来引用场景中的节点，避免在代码中使用`new`和`AddChild()`
   - **Export变量绑定优先在.tscn文件中完成**：在场景文件的根节点脚本属性中直接设置NodePath绑定（如`_skillIcon = NodePath("SkillIcon")`），而不是在代码中使用`GetNode()`方法
   - 在所有使用UI元素的方法中添加空值检查，确保代码健壮性
   - 代码只负责逻辑处理，UI布局和基本属性在场景文件中定义
   - 这样可以提高性能、可维护性，并符合Godot的最佳实践
6. **项目文件组织规则**：
   - `.tscn`场景文件必须放在`Scenes/`文件夹中，不要放在`Scripts/`文件夹
   - `.cs`代码文件放在`Scripts/`文件夹中
   - 保持项目结构清晰：`Scripts/`存放代码，`Scenes/`存放场景
   - 这样有助于项目的可维护性和团队协作

7. **Manager和Database架构设计规则**：
   - **AutoLoad配置**：所有Manager和Database类都必须配置为AutoLoad单例
   - **严格加载顺序**：必须按以下顺序配置AutoLoad
     1. **GameData** - 游戏核心数据，必须最先加载
     2. **所有Database类** - 配置数据库，在Manager之前加载
     3. **所有Manager类** - 业务逻辑管理器，最后加载
   - **加载顺序示例**：GameData → BuffDatabase/SkillDatabase/HeroDatabase/RelicDatabase → GameManager/AudioManager等
   - **场景文件优先**：所有Manager类都应该使用.tscn场景文件而不是直接引用.cs文件
   - **场景文件位置**：Manager场景文件统一存放在`Scenes/Manager/`文件夹中
   - **节点预配置**：在Manager场景文件中预先创建必要的子节点（如Timer、AudioStreamPlayer等）
   - **避免动态创建**：Manager类应通过`GetNode<T>()`获取预配置的节点，避免在代码中使用`new`和`AddChild()`
   - **Export属性配置**：Manager的Export属性应在Godot编辑器中配置，而不是在代码中硬编码
   - **依赖注入**：非Node类需要Database实例时，应通过方法参数传入，而不是内部创建
   - **单例访问**：Manager类通过AutoLoad路径访问Database，如`GetNode<SkillDatabase>("/root/SkillDatabase")`
   - **初始化检查**：所有Manager类都应添加空值检查和错误处理，确保依赖的Database已正确加载

8. **Database类设计规则**：
   - **继承Node**：所有Database类都应继承自Node，而不是RefCounted
   - **移除单例模式**：不使用传统的单例模式（Instance属性），改用AutoLoad
   - **自动初始化**：Database类在`_Ready()`方法中自动加载配置，无需手动调用
   - **配置加载**：使用`LoadConfigs()`方法在`_Ready()`中自动加载数据
   - **避免手动实例化**：其他类不应通过`new Database()`创建实例
   - **统一命名**：Database类文件名应以"Database"结尾，如`SkillDatabase.cs`

9. **GameData类设计规则**：
   - **必须继承Node**：GameData类必须继承自Node，不能继承Resource，否则AutoLoad会失败
   - **自动加载数据**：在`_Ready()`方法中自动调用`LoadGameData()`加载存档
   - **JSON存储格式**：使用JSON格式存储游戏数据，而不是Godot的.tres格式
   - **文件存储位置**：存档文件保存在`user://savegame.save`
   - **错误处理**：加载数据时要有容错机制，文件不存在时使用默认值
   - **Export属性**：所有游戏数据属性都应使用`[Export]`标记，便于在编辑器中观察和调试

10. **Godot场景文件(.tscn)节点parent属性规则**：
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

11. **Godot C#颜色使用规则**：
   - **使用Colors类而非Color类**：在Godot 4.x中，预定义颜色应使用`Colors.White`、`Colors.Black`等，而不是`Color.White`
   - **常见错误修复**：将所有`Color.White`替换为`Colors.White`，`Color.Black`替换为`Colors.Black`等
   - **自定义颜色**：自定义颜色仍使用`new Color(r, g, b, a)`构造函数
   - **编译错误**：使用`Color.White`会导致"Color未包含White的定义"编译错误

12. **Resource类GetNode使用限制**：
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

## 13. 文件组织和拆分规则

### 13.1 一个类一个文件原则
- **强制要求**：每个C#文件只能包含一个主要的类定义
- **例外情况**：紧密相关的小型辅助类（如内部类、数据结构）可以放在同一文件中
- **文件命名**：文件名必须与类名完全一致

### 13.2 枚举类型组织
- **集中管理**：相关的枚举类型应该集中在专门的Enums文件中
- **命名规范**：枚举文件命名格式为`{模块名}Enums.cs`
- **示例**：`BuffEnums.cs`、`SkillEnums.cs`、`RelicEnums.cs`

### 13.3 文件拆分标准
- **触发条件**：当单个文件包含多个类、枚举或超过200行代码时，应考虑拆分
- **拆分原则**：按功能职责拆分，确保每个文件有明确的单一职责
- **依赖管理**：拆分后确保所有引用关系正确，添加必要的using语句

## 14. 字典类型使用规则

- **强制使用Godot字典**：在任何情况下都只使用`Godot.Collections.Dictionary<TKey, TValue>`，不使用`System.Collections.Generic.Dictionary<TKey, TValue>`
- **统一性原则**：确保项目中所有字典类型的一致性，避免类型混用导致的序列化和兼容性问题
- **引用命名空间**：使用`using Godot.Collections;`引用命名空间，直接使用`Dictionary<TKey, TValue>`
- **适用范围**：此规则适用于所有场景，包括但不限于：
  - 数据存储和缓存
  - 配置信息管理
  - 统计信息收集
  - 游戏状态管理
- **迁移要求**：现有代码中的`System.Collections.Generic.Dictionary`应逐步迁移为`Godot.Collections.Dictionary`

## 16. 配置数据管理规则

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
  public float SkillDamage => SkillDatabase.GetSkillConfig(skillId).Damage;
  ```

游戏设计向规则
1.所有技能卡都有一个耗能属性，没有冷却时间。

## 15. 代码生成流程规则

### 15.1 枚举优先原则
- **先生成枚举类**：在生成具体功能代码之前，必须先生成相关的枚举类定义
- **复核确认**：枚举类生成后，需要等待用户复核确认后再继续生成具体的实现代码
- **依赖明确**：确保所有枚举定义完整且正确，避免后续代码生成时出现类型引用错误

### 15.2 自动编译验证规则
- **生成后编译**：每次生成代码后，必须自动执行编译检查，排查和修复编译错误
- **错误修复限制**：如果同一个编译错误连续修复3次后仍未解决，必须停止自动修复
- **错误报告**：达到修复次数限制时，应向用户报告具体的错误信息和已尝试的修复方案
- **手动介入**：复杂错误需要用户手动介入或重新设计解决方案