# GitHub Pages 可视化部署设置指南

本文档说明如何为 CodeRogue 项目设置 GitHub Pages，以便自动部署项目统计可视化图表。

## 🚀 快速设置

### 1. 启用 GitHub Pages

1. 进入你的 GitHub 仓库页面
2. 点击 **Settings** 标签
3. 在左侧菜单中找到 **Pages**
4. 在 **Source** 部分选择 **GitHub Actions**

### 2. 配置仓库权限

确保 GitHub Actions 有足够的权限：

1. 在仓库的 **Settings** → **Actions** → **General** 中
2. 找到 **Workflow permissions** 部分
3. 选择 **Read and write permissions**
4. 勾选 **Allow GitHub Actions to create and approve pull requests**

### 3. 更新 README 中的链接

在 `README.md` 文件中，将以下链接中的 `your-username` 替换为你的 GitHub 用户名：

```markdown
🔗 **[在线查看统计图表](https://your-username.github.io/coderogue/stats_visualization.html)**
```

替换为：

```markdown
🔗 **[在线查看统计图表](https://你的用户名.github.io/coderogue/stats_visualization.html)**
```

## 📋 工作流说明

项目包含两个 GitHub Actions 工作流：

### 1. `project-stats.yml`
- **触发条件**: 每次推送到主分支或创建 PR
- **功能**: 
  - 生成项目统计数据
  - 更新可视化图表
  - 部署到 GitHub Pages
  - 在 PR 中添加统计评论

### 2. `deploy-visualization.yml`
- **触发条件**: 
  - 推送影响可视化文件的更改
  - 手动触发 (workflow_dispatch)
- **功能**:
  - 专门用于部署可视化图表
  - 生成完整的历史统计数据
  - 创建导航页面

## 🔧 自定义配置

### 修改历史数据天数

在工作流文件中，你可以调整历史数据的天数：

```yaml
# 生成 180 天的历史数据
.\generate_historical_stats.ps1 -Days 180 -UseGitHistory
```

### 自定义部署路径

如果你想将可视化部署到不同的路径，可以修改 `deploy-visualization.yml` 中的 `destination_dir`：

```yaml
destination_dir: stats  # 部署到 /stats 路径
```

## 🌐 访问链接

部署成功后，你可以通过以下链接访问：

- **主导航页**: `https://你的用户名.github.io/仓库名/navigation.html`
- **统计图表**: `https://你的用户名.github.io/仓库名/stats_visualization.html`
- **使用指南**: `https://你的用户名.github.io/仓库名/VISUALIZATION_GUIDE.md`
- **原始数据**: `https://你的用户名.github.io/仓库名/historical_stats.json`

## 🐛 故障排除

### 部署失败

1. **检查权限**: 确保 GitHub Actions 有 Pages 写入权限
2. **检查分支**: 确保推送到了正确的主分支 (main 或 master)
3. **查看日志**: 在 Actions 标签页查看详细的错误日志

### 图表不显示数据

1. **检查数据文件**: 确保 `historical_stats.json` 文件存在且格式正确
2. **检查脚本**: 确保统计脚本能正常运行
3. **本地测试**: 先在本地运行脚本确保功能正常

### 链接无法访问

1. **等待部署**: GitHub Pages 部署可能需要几分钟时间
2. **检查设置**: 确保在仓库设置中启用了 GitHub Pages
3. **检查分支**: 确保部署的是正确的分支

## 📞 获取帮助

如果遇到问题，可以：

1. 查看 GitHub Actions 的运行日志
2. 检查 [GitHub Pages 文档](https://docs.github.com/en/pages)
3. 参考 [VISUALIZATION_GUIDE.md](VISUALIZATION_GUIDE.md) 了解更多使用方法

---

*最后更新: 2025-07-30*