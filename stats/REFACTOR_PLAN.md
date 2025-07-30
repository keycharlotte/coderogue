# CodeRogue 项目重构计划

## 重构目标
按照 project_rules.md 中的新规则重构整个项目，确保符合MVVM架构和文件组织规范。

## 主要问题分析

### 1. MVVM架构违规
- **UI代码设置属性问题**：
  - `SaveSystemUI.cs` 中使用 `SetAnchorsAndOffsetsPreset`
  - `TrackSlotUI.cs` 中使用 `AddThemeColorOverride`
  - 这些应该在 .tscn 文件中设置

### 2. AutoLoad配置问题
- **Manager类直接引用.cs文件**：
  - Database类等直接引用.cs而非.tscn
  - 违反规则2.2：Manager类应通过.tscn文件加载

### 3. 文件组织问题
- **数据类位置错误**：
  - `Scripts/Data/` 中的类应分散到各模块的data文件夹
  - 如：SaveMetadata.cs 应在 Core/data/ 中
  - GameData.cs 应在 Core/data/ 中

- **测试文件位置错误**：
  - `MonsterSystemTest.cs` 应在 Monsters/test/ 中
  - `SkillSystemTest.cs` 应在 Skills/test/ 中
  - `SkillTrackTest.cs` 应在 Skills/test/ 中

## 重构步骤

### 阶段1：文件组织重构
1. 创建模块化的data和test文件夹结构
2. 移动数据类到对应模块的data文件夹
3. 移动测试文件到对应模块的test文件夹
4. 更新所有引用路径

### 阶段2：MVVM架构重构
1. 移除UI代码中的属性设置
2. 在.tscn文件中设置所有UI属性
3. 确保xxxUI.cs只包含ViewModel逻辑

### 阶段3：Manager类重构
1. 为所有Manager类创建对应的.tscn文件
2. 更新project.godot中的AutoLoad配置
3. 确保Manager类通过.tscn加载

### 阶段4：验证和测试
1. 验证所有文件路径正确
2. 测试游戏功能正常
3. 确保符合所有项目规则

## 详细执行计划

### 1.1 创建新的文件夹结构
```
Scripts/
├── Core/
│   ├── data/
│   │   ├── GameData.cs
│   │   ├── SaveMetadata.cs
│   │   ├── SaveSlotInfo.cs
│   │   └── SaveOperationResult.cs
│   └── test/
│       └── CoreSystemTest.cs
├── Monsters/
│   ├── data/
│   │   └── MonsterData.cs
│   └── test/
│       └── MonsterSystemTest.cs
├── Skills/
│   ├── data/
│   │   └── SkillData.cs
│   └── test/
│       ├── SkillSystemTest.cs
│       └── SkillTrackTest.cs
```

### 1.2 UI重构优先级
1. **高优先级**：SaveSystemUI.cs - 移除所有UI属性设置
2. **高优先级**：TrackSlotUI.cs - 移除主题和样式设置
3. **中优先级**：其他UI类的类似问题

### 1.3 Manager重构优先级
1. **高优先级**：Database相关Manager
2. **中优先级**：其他核心Manager
3. **低优先级**：辅助Manager

## 风险评估
- **高风险**：移动文件可能破坏现有引用
- **中风险**：UI重构可能影响显示效果
- **低风险**：Manager重构相对安全

## 回滚计划
- 每个阶段完成后创建备份
- 保留原始文件直到验证完成
- 记录所有修改以便回滚