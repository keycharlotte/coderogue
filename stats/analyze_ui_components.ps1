# UI组件分析脚本 - 简化版
# 分析所有.tscn文件中的UI组件类型

$uiComponents = @{}
$tscnFiles = Get-ChildItem -Path . -Recurse -Filter "*.tscn"

Write-Host "开始分析 $($tscnFiles.Count) 个.tscn文件..."

# 定义UI相关的组件类型
$uiTypes = @(
    'Control', 'Container', 'Panel', 'VBoxContainer', 'HBoxContainer',
    'GridContainer', 'ScrollContainer', 'TabContainer', 'SplitContainer',
    'HSplitContainer', 'VSplitContainer', 'MarginContainer', 'CenterContainer',
    'Button', 'CheckBox', 'OptionButton', 'MenuButton', 'LinkButton',
    'Label', 'RichTextLabel', 'LineEdit', 'TextEdit', 'SpinBox',
    'Slider', 'HSlider', 'VSlider', 'ProgressBar', 'Range',
    'ItemList', 'Tree', 'PopupMenu', 'FileDialog',
    'AcceptDialog', 'ConfirmationDialog', 'WindowDialog', 'Popup',
    'ColorPicker', 'ColorRect', 'NinePatchRect', 'TextureRect',
    'VideoPlayer', 'GraphEdit', 'GraphNode', 'HSeparator', 'VSeparator',
    'TabBar', 'MenuBar', 'ToolBar', 'StatusBar', 'CanvasLayer'
)

foreach ($file in $tscnFiles) {
    Write-Host "分析文件: $($file.Name)"
    $content = Get-Content $file.FullName -Encoding UTF8
    
    foreach ($line in $content) {
        # 查找包含type=的行
        if ($line -like '*type=*') {
            # 提取type值
            $matches = [regex]::Matches($line, 'type="([^"]+)"')
            foreach ($match in $matches) {
                $nodeType = $match.Groups[1].Value
                
                if ($nodeType -in $uiTypes) {
                    if ($uiComponents.ContainsKey($nodeType)) {
                        $uiComponents[$nodeType]++
                    } else {
                        $uiComponents[$nodeType] = 1
                    }
                }
            }
        }
    }
}

Write-Host "`n=== UI组件统计结果 ==="
Write-Host "总共找到 $($uiComponents.Keys.Count) 种UI组件类型:`n"

# 按使用频率排序显示
$sortedComponents = $uiComponents.GetEnumerator() | Sort-Object Value -Descending

foreach ($component in $sortedComponents) {
    Write-Host "$($component.Key): $($component.Value) 个"
}

# 将结果保存到文件
$outputFile = "ui_components_analysis.txt"
$output = "UI组件分析结果`n" + "=" * 20 + "`n`n"
$output += "分析文件数量: $($tscnFiles.Count)`n"
$output += "UI组件类型数量: $($uiComponents.Keys.Count)`n`n"
$output += "详细统计:`n"

foreach ($component in $sortedComponents) {
    $output += "$($component.Key): $($component.Value)`n"
}

$output | Out-File -FilePath $outputFile -Encoding UTF8
Write-Host "`n结果已保存到: $outputFile"