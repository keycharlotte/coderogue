# Update project statistics and generate visualization data
# This script combines statistics generation with visualization data updates

param(
    [switch]$UpdateReadme = $true,              # Update README with latest stats
    [switch]$GenerateVisualization = $true,     # Generate/update visualization data
    [switch]$OpenVisualization = $false,        # Open visualization in browser
    [int]$HistoryDays = 90,                     # Days of history to maintain
    [string]$VisualizationFile = "historical_stats.json"
)

Write-Host "=== Project Statistics & Visualization Updater ===" -ForegroundColor Green

# Step 1: Generate current project statistics
Write-Host "`n📊 Generating current project statistics..." -ForegroundColor Cyan
try {
    $currentStats = & "$PSScriptRoot\project_stats.ps1" -OutputJson | ConvertFrom-Json
    Write-Host "✅ Current statistics generated successfully" -ForegroundColor Green
} catch {
    Write-Error "Failed to generate current statistics: $_"
    exit 1
}

# Step 2: Update README if requested
if ($UpdateReadme) {
    Write-Host "`n📝 Updating README with latest statistics..." -ForegroundColor Cyan
    try {
        & "$PSScriptRoot\update_readme_stats.ps1"
        Write-Host "✅ README updated successfully" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to update README: $_"
    }
}

# Step 3: Update visualization data
if ($GenerateVisualization) {
    Write-Host "`n📈 Updating visualization data..." -ForegroundColor Cyan
    
    $currentDate = Get-Date -Format "yyyy-MM-dd"
    $currentTime = Get-Date -Format "HH:mm:ss"
    
    # Load existing visualization data or create new
    $visualizationData = @()
    if (Test-Path $VisualizationFile) {
        try {
            $visualizationData = Get-Content $VisualizationFile -Raw | ConvertFrom-Json
            Write-Host "📂 Loaded existing visualization data ($($visualizationData.Count) entries)" -ForegroundColor Gray
        } catch {
            Write-Warning "Failed to load existing visualization data: $_"
            $visualizationData = @()
        }
    }
    
    # Remove old entries beyond history limit
    $cutoffDate = (Get-Date).AddDays(-$HistoryDays)
    $visualizationData = $visualizationData | Where-Object { 
        try {
            [DateTime]::Parse($_.date) -ge $cutoffDate
        } catch {
            $false
        }
    }
    
    # Check if we already have an entry for today
    $todayEntry = $visualizationData | Where-Object { $_.date -eq $currentDate }
    
    if ($todayEntry) {
        # Update existing entry for today
        $todayEntry.CsFiles = $currentStats.CsFiles
        $todayEntry.Classes = $currentStats.Classes
        $todayEntry.CodeLines = $currentStats.CodeLines
        $todayEntry.TscnFiles = $currentStats.TscnFiles
        $todayEntry.TresFiles = $currentStats.TresFiles
        $todayEntry.lastUpdated = "$currentDate $currentTime"
        Write-Host "🔄 Updated today's entry" -ForegroundColor Yellow
    } else {
        # Add new entry for today
        $newEntry = [PSCustomObject]@{
            date = $currentDate
            CsFiles = $currentStats.CsFiles
            Classes = $currentStats.Classes
            CodeLines = $currentStats.CodeLines
            TscnFiles = $currentStats.TscnFiles
            TresFiles = $currentStats.TresFiles
            lastUpdated = "$currentDate $currentTime"
            commit = "manual-update"
            message = "Statistics update - $currentTime"
        }
        
        $visualizationData += $newEntry
        Write-Host "➕ Added new entry for today" -ForegroundColor Green
    }
    
    # Sort data by date
    $visualizationData = $visualizationData | Sort-Object date
    
    # Save updated visualization data
    try {
        $visualizationData | ConvertTo-Json -Depth 10 | Out-File -FilePath $VisualizationFile -Encoding utf8
        Write-Host "✅ Visualization data updated ($($visualizationData.Count) total entries)" -ForegroundColor Green
        
        # Display data summary
        if ($visualizationData.Count -gt 1) {
            $oldest = $visualizationData[0]
            $latest = $visualizationData[-1]
            
            Write-Host "`n📊 Data Range Summary:" -ForegroundColor Magenta
            Write-Host "   From: $($oldest.date) to $($latest.date)"
            Write-Host "   Total entries: $($visualizationData.Count)"
            
            # Calculate growth
            $csGrowth = $latest.CsFiles - $oldest.CsFiles
            $classGrowth = $latest.Classes - $oldest.Classes
            $lineGrowth = $latest.CodeLines - $oldest.CodeLines
            
            Write-Host "`n📈 Growth Since $($oldest.date):" -ForegroundColor Cyan
            Write-Host "   C# Files: $csGrowth ($($oldest.CsFiles) → $($latest.CsFiles))"
            Write-Host "   Classes: $classGrowth ($($oldest.Classes) → $($latest.Classes))"
            Write-Host "   Lines of Code: $lineGrowth ($($oldest.CodeLines) → $($latest.CodeLines))"
        }
        
    } catch {
        Write-Error "Failed to save visualization data: $_"
    }
}

# Step 4: Open visualization if requested
if ($OpenVisualization) {
    Write-Host "`n🌐 Opening visualization in browser..." -ForegroundColor Cyan
    try {
        if (Test-Path "stats_visualization.html") {
            Start-Process "stats_visualization.html"
            Write-Host "✅ Visualization opened in default browser" -ForegroundColor Green
        } else {
            Write-Warning "Visualization file not found: stats_visualization.html"
        }
    } catch {
        Write-Warning "Failed to open visualization: $_"
    }
}

# Step 5: Display current statistics summary
Write-Host "`n=== Current Project Statistics ===" -ForegroundColor Yellow
Write-Host "📁 C# Files: $($currentStats.CsFiles)" -ForegroundColor White
Write-Host "🏛️ Classes: $($currentStats.Classes)" -ForegroundColor White
Write-Host "📝 Lines of Code: $($currentStats.CodeLines)" -ForegroundColor White
Write-Host "🎬 Scene Files: $($currentStats.TscnFiles)" -ForegroundColor White
Write-Host "📦 Resource Files: $($currentStats.TresFiles)" -ForegroundColor White
Write-Host "📊 Total Files: $($currentStats.CsFiles + $currentStats.TscnFiles + $currentStats.TresFiles)" -ForegroundColor White
Write-Host "================================" -ForegroundColor Yellow

# Step 6: Provide next steps
Write-Host "🎯 Next Steps:" -ForegroundColor Magenta
if (-not $OpenVisualization) {
    Write-Host "   • Open stats_visualization.html to view charts" -ForegroundColor Gray
}
Write-Host "   • Load $VisualizationFile in the visualization page" -ForegroundColor Gray
Write-Host "   • Run this script regularly to maintain historical data" -ForegroundColor Gray

Write-Host "`n🎉 Statistics and visualization update completed!" -ForegroundColor Green