<!-- STATS_BADGES_START -->
![Classes](https://img.shields.io/badge/Classes-127-blue?style=flat-square)
![Lines of Code](https://img.shields.io/badge/Lines_of_Code-18256-green?style=flat-square)
![C# Files](https://img.shields.io/badge/C%23_Files-132-orange?style=flat-square)
![TSCN Files](https://img.shields.io/badge/TSCN_Files-32-purple?style=flat-square)
![TRES Files](https://img.shields.io/badge/TRES_Files-15-red?style=flat-square)
![Last Updated](https://img.shields.io/badge/Updated-2025--07--31-lightgrey?style=flat-square)
<!-- STATS_BADGES_END -->

<!-- STATS_TABLE_START -->
| Statistics | Count | Description |
|------------|-------|--------------|
| C# Files | **132** | Number of C# script files in the project |
| Classes | **127** | Total number of defined classes |
| Lines of Code | **18256** | Total lines of code (excluding empty lines and comments) |
| TSCN Files | **32** | Number of Godot scene files |
| TRES Files | **15** | Number of Godot resource files |

> Last updated: 2025-07-31 15:45:15
<!-- STATS_TABLE_END -->

## 📊 项目统计可视化

查看项目开发历程的交互式统计图表：

🔗 **[在线查看统计图表](https://keycharlotte.github.io/coderogue/stats_visualization.html)**

### 可视化功能
- 📈 **趋势图表**: 查看项目各项指标的历史变化趋势
- 📊 **分布图表**: 了解不同类型文件的分布情况
- 🎛️ **交互控制**: 支持时间范围筛选和数据导出
- 📱 **响应式设计**: 支持桌面和移动设备访问

### 本地使用
```powershell
# 生成历史统计数据
.\generate_historical_stats.ps1 -Days 90

# 更新可视化数据并打开图表
.\update_stats_with_visualization.ps1 -OpenVisualization
```

详细使用说明请参考 [可视化指南](VISUALIZATION_GUIDE.md)。

# CodeRogue: 赛博修仙

一个基于Godot 4.4和C#开发的赛博修仙Roguelike游戏项目。融合传统修仙文化与赛博朋克科技，打造独特的阴阳五行修炼卡牌体验。

## 项目结构
CodeRogue/
├── Scenes/                    # 场景文件
│   ├── Main/                  # 主场景
│   ├── Player/                # 玩家相关场景
│   ├── Enemy/                 # 敌人场景
│   ├── UI/                    # UI场景
│   └── Level/                 # 关卡场景
├── Scripts/                   # C#脚本
│   ├── Core/                  # 核心系统
│   ├── Player/                # 玩家系统
│   ├── Enemy/                 # 敌人系统
│   ├── Combat/                # 战斗系统
│   ├── UI/                    # UI系统
│   ├── Level/                 # 关卡系统
│   ├── Data/                  # 数据类
│   ├── Utils/                 # 工具类
│   ├── Components/            # 组件系统
│   └── Editor/                # 编辑器工具
├── Assets/                    # 资源文件
├── Resources/                 # Godot资源
├── Design/                    # 设计文档
└── Tools/                     # 开发工具
## 开发环境

- Godot 4.4+
- .NET 6.0+
- C# 10+

## 核心特性

- **阴阳五行修炼体系**：基于金木水火土五行相生相克的修炼卡牌系统
- **赛博修仙世界观**：传统修仙文化与未来科技的完美融合
- **职业修炼系统**：剑修、法修、体修、丹修、阵修五大修炼职业
- **51层修仙高塔**：从凡俗层到天机层的终极挑战
- **灵力与科技融合**：传统灵力修炼与赛博科技改造并存
- **模块化架构设计**：清晰的代码结构，便于扩展和维护
- **AI友好的代码结构**：完整的注释文档和标准设计模式

## 开始开发

1. 在Godot中打开项目
2. 确保启用了C#支持
3. 构建项目以生成C#程序集
4. 开始编码！

## AI代码生成指南

本项目专门为AI代码生成优化，具有：
- 清晰的命名规范
- 完整的注释文档
- 模块化的代码结构
- 标准的设计模式

## 许可证

MIT License
