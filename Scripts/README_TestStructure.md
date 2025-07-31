# 测试文件组织结构

本文档说明了CodeRogue项目中测试文件的组织结构和规范。

## 文件夹结构

每个模块都有对应的`test`子文件夹，用于存放该模块的测试文件和示例代码：

```
Scripts/
├── Core/
│   └── test/
│       └── CardDatabaseTestUI.cs
├── Monsters/
│   └── test/
│       └── MonsterSystemTest.cs
├── Skills/
│   └── test/
│       ├── SkillSystemTest.cs
│       ├── SkillTrackTest.cs
│       ├── SkillTestRunner.cs
│       └── ExampleTemporaryTrackSkill.cs
└── UI/
    └── test/
        └── SkillTrackUIExample.cs
```

## 测试文件类型

### 1. 单元测试文件
- **命名规范**: `{ModuleName}SystemTest.cs`
- **功能**: 测试模块的核心功能
- **示例**: `MonsterSystemTest.cs`, `SkillSystemTest.cs`

### 2. 组件测试文件
- **命名规范**: `{ComponentName}Test.cs`
- **功能**: 测试特定组件的功能
- **示例**: `SkillTrackTest.cs`

### 3. UI测试文件
- **命名规范**: `{UIName}TestUI.cs`
- **功能**: 测试UI组件的交互和显示
- **示例**: `CardDatabaseTestUI.cs`

### 4. 示例文件
- **命名规范**: `{ComponentName}Example.cs` 或 `Example{ComponentName}.cs`
- **功能**: 提供使用示例和演示代码
- **示例**: `SkillTrackUIExample.cs`, `ExampleTemporaryTrackSkill.cs`

### 5. 测试运行器
- **命名规范**: `{ModuleName}TestRunner.cs`
- **功能**: 提供测试执行的控制界面
- **示例**: `SkillTestRunner.cs`

## 测试文件规范

### 命名空间
- 测试文件使用 `CodeRogue.Test` 命名空间
- 示例文件使用 `CodeRogue.Examples` 命名空间

### 基本结构
```csharp
using Godot;
using CodeRogue.Core;

namespace CodeRogue.Test
{
    public partial class ModuleSystemTest : Node
    {
        [Export] public bool AutoRunTests { get; set; } = true;
        
        public override void _Ready()
        {
            if (AutoRunTests)
            {
                CallDeferred(nameof(RunAllTests));
            }
        }
        
        public void RunAllTests()
        {
            // 测试实现
        }
    }
}
```

## 场景文件对应关系

测试相关的场景文件位于 `Scenes/Test/` 和 `Scenes/UI/` 文件夹中：

- `Scenes/Test/SkillTrackTest.tscn` → `Scripts/Skills/test/SkillTrackTest.cs`
- `Scenes/Test/CardDatabaseTest.tscn` → `Scripts/Core/test/CardDatabaseTestUI.cs`
- `Scenes/UI/SkillTrackUIExample.tscn` → `Scripts/UI/test/SkillTrackUIExample.cs`

## 移动的文件

在本次重构中，以下文件被移动到了对应的test文件夹：

1. `Scripts/UI/SkillTrackUIExample.cs` → `Scripts/UI/test/SkillTrackUIExample.cs`
2. `Scripts/Skills/SkillTestRunner.cs` → `Scripts/Skills/test/SkillTestRunner.cs`
3. `Scripts/Skills/ExampleTemporaryTrackSkill.cs` → `Scripts/Skills/test/ExampleTemporaryTrackSkill.cs`

相关的场景文件路径也已更新。

## 注意事项

1. 所有测试文件都应该放在对应模块的`test`子文件夹中
2. 测试文件应该有清晰的命名规范
3. 场景文件中的脚本路径需要与移动后的文件路径保持一致
4. 测试文件应该包含适当的文档说明其测试目的和使用方法