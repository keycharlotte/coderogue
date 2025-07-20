# 临时轨道系统使用指南

## 概述

临时轨道系统允许通过Buff机制动态创建和管理临时的技能轨道。这些轨道可以根据不同的条件自动销毁，为游戏提供了更灵活的技能系统。

## 核心组件

### 1. TemporaryTrackDestroyCondition 枚举
位置：`Scripts/Skills/SkillEnums.cs`

定义了临时轨道的销毁条件：
- `Timer`: 计时器到期
- `TrackEmpty`: 轨道为空时
- `SkillActivated`: 特定技能激活时

### 2. TemporaryTrackBuffConfig 类
位置：`Scripts/Buffs/TemporaryTrackBuffConfig.cs`

继承自 `BuffConfig`，用于配置临时轨道Buff的属性：
- `DestroyCondition`: 销毁条件
- `TriggerSkillId`: 触发技能ID（用于SkillActivated条件）
- `TrackSkill`: 要装载到临时轨道的技能

### 3. TemporaryTrackBuffEffect 类
位置：`Scripts/Buffs/TemporaryTrackBuffEffect.cs`

实现 `IBuffEffect` 接口，处理临时轨道的创建和销毁：
- `OnApply()`: 创建临时轨道
- `OnRemove()`: 移除临时轨道
- `OnUpdate()`: 更新处理（由SkillTrackManager负责）
- `OnStack()`: 叠加处理（临时轨道不支持叠加）

### 4. TemporaryTrackSkillEffect 类
位置：`Scripts/Skills/TemporaryTrackSkillEffect.cs`

继承自 `SkillEffect`，用于技能卡中创建临时轨道效果：
- `Execute()`: 执行临时轨道创建逻辑

## 使用方法

### 1. 创建临时轨道技能

```csharp
public partial class MyTemporaryTrackSkill : SkillCard
{
    public override void _Ready()
    {
        base._Ready();
        
        // 设置技能基本信息
        Name = "召唤临时轨道";
        Description = "创建一个临时轨道";
        
        // 创建临时轨道效果
        var tempTrackEffect = new TemporaryTrackSkillEffect
        {
            Type = SkillEffectType.TemporaryTrack,
            DestroyCondition = TemporaryTrackDestroyCondition.Timer,
            Duration = 10f,
            TrackSkill = mySkillToLoad,
            BuffConfigId = 1001
        };
        
        Effects = new Array<SkillEffect> { tempTrackEffect };
    }
}
```

### 2. 配置BuffDatabase

需要在BuffDatabase中添加对应的TemporaryTrackBuffConfig：

```csharp
// 在BuffDatabase中添加ID为1001的Buff配置
var tempTrackBuffConfig = new TemporaryTrackBuffConfig
{
    Id = 1001,
    Name = "临时轨道",
    BaseDuration = 10f,
    // 其他配置...
};
```

### 3. 销毁条件说明

#### Timer (计时器)
- 轨道在指定时间后自动销毁
- 最常用的销毁条件

#### TrackEmpty (轨道为空)
- 当轨道中的技能被使用完毕后销毁
- 适用于一次性技能轨道

#### SkillActivated (技能激活)
- 当特定技能被激活时销毁
- 需要设置`TriggerSkillId`
- 适用于条件性临时轨道

## 系统架构

```
SkillCard (技能卡)
    └── TemporaryTrackSkillEffect (临时轨道技能效果)
        └── BuffManager.ApplyBuff() (应用Buff)
            └── TemporaryTrackBuffEffect (Buff效果处理器)
                └── SkillTrackManager.AddTemporaryTrack() (添加临时轨道)
```

## 注意事项

1. **Buff配置**: 确保在BuffDatabase中正确配置对应的TemporaryTrackBuffConfig
2. **节点路径**: 系统依赖于`/root/BuffManager`和`/root/SkillTrackManager`的自动加载节点
3. **生命周期**: 临时轨道的生命周期完全由Buff系统管理
4. **UI更新**: SkillTrackUI会自动响应临时轨道的添加和移除事件
5. **性能考虑**: 避免同时创建过多临时轨道
6. **Resource类限制**: TemporaryTrackBuffConfig和TemporaryTrackSkillEffect继承自Resource，不能直接使用GetNode方法，需要通过Engine.GetMainLoop()获取场景树根节点来访问AutoLoad节点

## 技术实现细节

### Resource类中访问AutoLoad节点的方法

由于Resource类不继承自Node，无法直接使用GetNode方法，需要使用以下方式访问AutoLoad节点：

```csharp
// 获取场景树根节点
var sceneTree = Engine.GetMainLoop() as SceneTree;
if (sceneTree?.Root != null)
{
    var manager = sceneTree.Root.GetNode<ManagerType>("/root/ManagerName");
    // 使用manager...
}
```

这种方式确保了Resource类也能正确访问AutoLoad管理器实例。

## 示例场景

### 场景1: 时限增强轨道
```csharp
// 创建一个10秒的临时轨道，装载强化版火球术
DestroyCondition = TemporaryTrackDestroyCondition.Timer
Duration = 10f
```

### 场景2: 一次性技能轨道
```csharp
// 创建一个临时轨道，技能使用后立即销毁
DestroyCondition = TemporaryTrackDestroyCondition.TrackEmpty
```

### 场景3: 条件性临时轨道
```csharp
// 创建一个临时轨道，当使用"治疗术"时销毁
DestroyCondition = TemporaryTrackDestroyCondition.SkillActivated
TriggerSkillId = "heal_spell"
```

这个系统提供了灵活的临时轨道管理机制，可以根据游戏需求创建各种类型的临时技能增强效果。