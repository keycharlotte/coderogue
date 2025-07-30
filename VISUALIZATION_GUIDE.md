# 📊 Project Statistics Visualization Guide

This guide explains how to use the project statistics visualization system for the CodeRogue project.

## 🎯 Overview

The visualization system provides interactive charts and graphs to track project development trends over time, including:

- **C# Files Count** - Number of C# source files
- **Classes Count** - Total number of classes defined
- **Lines of Code** - Total lines of C# code
- **Scene Files** - Number of Godot scene files (.tscn)
- **Resource Files** - Number of Godot resource files (.tres)

## 🚀 Quick Start

### 1. Generate Current Statistics
```powershell
# Basic statistics generation
.\project_stats.ps1

# Generate with JSON output
.\project_stats.ps1 -OutputJson
```

### 2. Update Everything (Recommended)
```powershell
# Update README, generate visualization data, and open charts
.\update_stats_with_visualization.ps1 -OpenVisualization
```

### 3. View Visualization
Open `stats_visualization.html` in your web browser to see interactive charts.

## 📁 Files Overview

### Core Scripts
- **`project_stats.ps1`** - Main statistics generation script
- **`update_readme_stats.ps1`** - Updates README with current stats
- **`format_stats.ps1`** - Formats statistics for different outputs
- **`auto_commit_with_stats.ps1`** - Auto-commit with statistics

### Visualization Files
- **`stats_visualization.html`** - Interactive dashboard (open in browser)
- **`generate_historical_stats.ps1`** - Generate historical data
- **`update_stats_with_visualization.ps1`** - All-in-one updater
- **`historical_stats.json`** - Historical data file

### GitHub Integration
- **`.github/workflows/project-stats.yml`** - GitHub Actions workflow

## 🎨 Visualization Features

### Interactive Dashboard
The `stats_visualization.html` provides:

1. **📈 Trends Chart** - Line chart showing development progress over time
2. **📊 Distribution Chart** - Pie chart showing current file type distribution
3. **📋 Statistics Cards** - Current project metrics at a glance
4. **📅 Time Range Filters** - View data for different time periods
5. **📊 Data Table** - Detailed historical data view

### Chart Controls
- **Time Range Selection**: All Time, Last 30/90/365 Days
- **Data Import**: Load custom JSON data files
- **Export Function**: Download data as JSON
- **Sample Data**: Built-in demo data for testing

## 🔧 Usage Examples

### Daily Development Tracking
```powershell
# Run this daily to track progress
.\update_stats_with_visualization.ps1
```

### Commit with Statistics
```powershell
# Auto-commit with statistical summary
.\auto_commit_with_stats.ps1 -CommitMessage "Added new feature" -CommitType "feat" -AutoConfirm
```

### Generate Historical Data
```powershell
# Generate 90 days of sample data
.\generate_historical_stats.ps1 -DaysBack 90 -GenerateSample

# Use actual Git history (experimental)
.\generate_historical_stats.ps1 -UseGitHistory -DaysBack 30
```

### Custom Visualization Data
```powershell
# Generate data for specific time range
.\generate_historical_stats.ps1 -DaysBack 180 -OutputFile "custom_stats.json"
```

## 📊 Data Format

The visualization system uses JSON format for historical data:

```json
[
  {
    "date": "2025-07-30",
    "CsFiles": 125,
    "Classes": 127,
    "CodeLines": 18217,
    "TscnFiles": 32,
    "TresFiles": 15,
    "commit": "abc1234",
    "message": "Feature implementation"
  }
]
```

## 🔄 Automation

### GitHub Actions
The project includes automated statistics generation on every push:
- Runs `project_stats.ps1` automatically
- Updates PR comments with statistics
- Generates workflow summaries

### Local Automation
Set up a scheduled task to run daily:
```powershell
# Windows Task Scheduler example
schtasks /create /tn "ProjectStats" /tr "powershell.exe -File 'C:\path\to\update_stats_with_visualization.ps1'" /sc daily /st 09:00
```

## 🎯 Best Practices

### 1. Regular Updates
- Run `update_stats_with_visualization.ps1` daily
- Commit changes with `auto_commit_with_stats.ps1`
- Review trends weekly using the visualization

### 2. Data Management
- Keep historical data files under version control
- Export data regularly for backup
- Clean old data beyond your retention period

### 3. Team Collaboration
- Share visualization HTML file with team members
- Include statistics in code reviews
- Use trends to identify development patterns

## 🛠️ Customization

### Adding New Metrics
To track additional file types or metrics:

1. Modify `project_stats.ps1` to collect new data
2. Update `stats_visualization.html` to display new metrics
3. Adjust chart configurations as needed

### Styling Changes
Customize the visualization appearance by editing:
- CSS styles in `stats_visualization.html`
- Chart colors and themes
- Layout and responsive design

### Integration with Other Tools
The JSON data format makes it easy to integrate with:
- External dashboards (Grafana, etc.)
- CI/CD pipelines
- Project management tools
- Custom reporting systems

## 🐛 Troubleshooting

### Common Issues

**Script Execution Policy**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Missing Dependencies**
- Ensure PowerShell 5.0+ is installed
- Verify Git is available in PATH
- Check file permissions

**Chart Not Loading**
- Verify internet connection (for Chart.js CDN)
- Check browser console for JavaScript errors
- Ensure JSON data file is valid

**Statistics Inaccurate**
- Verify file paths and extensions
- Check for symbolic links or junction points
- Review exclusion patterns

## 📈 Advanced Features

### Performance Tracking
Monitor development velocity:
- Lines of code per day
- File creation rate
- Class complexity trends

### Quality Metrics
Combine with code analysis tools:
- Code coverage statistics
- Complexity measurements
- Technical debt tracking

### Team Analytics
Extend for team insights:
- Individual contributor statistics
- Feature development timelines
- Code review metrics

## 🎉 Conclusion

The visualization system provides powerful insights into your project's development progress. Use it regularly to:

- Track development velocity
- Identify growth patterns
- Make data-driven decisions
- Share progress with stakeholders
- Maintain project documentation

For questions or improvements, refer to the script comments or create an issue in the project repository.

---

**Happy Coding! 🚀**