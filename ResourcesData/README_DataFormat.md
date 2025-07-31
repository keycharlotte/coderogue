# 数据配置文件格式说明

## JSON格式文件（推荐用于版本控制）

### CardTag
- **位置**: `ResourcesData/CardTag/*.json`
- **原因**: 结构简单，只包含基本属性（Name, Description, Color）
- **优势**: 易于版本控制、人类可读、便于编辑
- **加载器**: `Scripts/Skills/data/CardTagLoader.cs`

### 其他简单配置
- `CardConfigs.json` - 卡片基础配置
- `EnemyConfig.json` - 敌人配置
- `LevelConfig.json` - 关卡配置

## .tres格式文件（保持原格式）

### SkillCard
- **位置**: `ResourcesData/SkillCard/*.tres`
- **原因**: 包含复杂的嵌套资源和引用关系
- **包含**: 
  - 对其他资源的引用（CardTag, Texture2D）
  - 嵌套的子资源（SkillEffect, SkillLevelData）
  - 复杂的数组结构
- **建议**: 保持.tres格式，因为Godot编辑器能更好地处理资源引用

## 转换完成的文件

✅ **已转换为JSON**:
- `jichu.tres` → `jichu.json`
- `xihuan.tres` → `xihuan.json`

⚠️ **保持.tres格式**:
- 所有SkillCard文件（复杂度过高）

## 使用建议

1. **简单数据**: 使用JSON格式，便于版本控制和手动编辑
2. **复杂资源**: 使用.tres格式，利用Godot的资源系统
3. **混合使用**: 根据数据复杂度选择合适的格式