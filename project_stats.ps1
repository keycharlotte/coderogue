# CodeRogue项目统计脚本
# 统计项目中的类数量、代码行数、.tscn文件数量和.tres文件数量

param(
    [string]$ProjectPath = ".",
    [switch]$OutputJson,
    [switch]$Verbose
)

# 设置项目根目录
$ProjectRoot = Resolve-Path $ProjectPath
Write-Host "正在统计项目: $ProjectRoot" -ForegroundColor Green

# 初始化统计变量
$ClassCount = 0
$TotalLines = 0
$TscnCount = 0
$TresCount = 0
$CsFileCount = 0

# 统计.cs文件和类数量
Write-Host "正在统计C#文件和类..." -ForegroundColor Yellow
$CsFiles = Get-ChildItem -Path $ProjectRoot -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\.godot\\" }
$CsFileCount = $CsFiles.Count

foreach ($file in $CsFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ($content) {
        # 统计行数（排除空行）
        $lines = ($content -split "\r?\n" | Where-Object { $_.Trim() -ne "" }).Count
        $TotalLines += $lines
        
        # 统计类数量（匹配 public class, internal class, private class 等）
        $classMatches = [regex]::Matches($content, "(?:public|internal|private|protected)?\s*(?:static|abstract|sealed)?\s*class\s+\w+")
        $ClassCount += $classMatches.Count
        
        if ($Verbose) {
            Write-Host "  $($file.Name): $lines 行, $($classMatches.Count) 个类" -ForegroundColor Gray
        }
    }
}

# 统计.tscn文件
Write-Host "正在统计.tscn文件..." -ForegroundColor Yellow
$TscnFiles = Get-ChildItem -Path $ProjectRoot -Recurse -Filter "*.tscn" | Where-Object { $_.FullName -notmatch "\\.godot\\" }
$TscnCount = $TscnFiles.Count

if ($Verbose) {
    foreach ($file in $TscnFiles) {
        Write-Host "  找到场景文件: $($file.Name)" -ForegroundColor Gray
    }
}

# 统计.tres文件
Write-Host "正在统计.tres文件..." -ForegroundColor Yellow
$TresFiles = Get-ChildItem -Path $ProjectRoot -Recurse -Filter "*.tres" | Where-Object { $_.FullName -notmatch "\\.godot\\" }
$TresCount = $TresFiles.Count

if ($Verbose) {
    foreach ($file in $TresFiles) {
        Write-Host "  找到资源文件: $($file.Name)" -ForegroundColor Gray
    }
}

# 创建统计结果对象
$Stats = @{
    ProjectPath = $ProjectRoot.Path
    Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    CsFiles = $CsFileCount
    Classes = $ClassCount
    CodeLines = $TotalLines
    TscnFiles = $TscnCount
    TresFiles = $TresCount
    TotalFiles = $CsFileCount + $TscnCount + $TresCount
}

# 输出结果
Write-Host "`n=== CodeRogue 项目统计结果 ===" -ForegroundColor Cyan
Write-Host "统计时间: $($Stats.Timestamp)" -ForegroundColor White
Write-Host "项目路径: $($Stats.ProjectPath)" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "📁 C# 文件数量: $($Stats.CsFiles)" -ForegroundColor Green
Write-Host "🏗️  类数量: $($Stats.Classes)" -ForegroundColor Green
Write-Host "📝 代码行数: $($Stats.CodeLines)" -ForegroundColor Green
Write-Host "🎬 场景文件(.tscn): $($Stats.TscnFiles)" -ForegroundColor Blue
Write-Host "📦 资源文件(.tres): $($Stats.TresFiles)" -ForegroundColor Blue
Write-Host "📊 总文件数: $($Stats.TotalFiles)" -ForegroundColor Magenta
Write-Host "===============================" -ForegroundColor Cyan

# 如果指定了JSON输出
if ($OutputJson) {
    $JsonOutput = $Stats | ConvertTo-Json -Depth 2
    $JsonFile = Join-Path $ProjectRoot "project_stats.json"
    $JsonOutput | Out-File -FilePath $JsonFile -Encoding UTF8
    Write-Host "统计结果已保存到: $JsonFile" -ForegroundColor Green
    return $JsonOutput
}

# 返回统计对象供其他脚本使用
return $Stats