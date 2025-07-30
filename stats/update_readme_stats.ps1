# README Statistics Update Script
# Automatically update project statistics badges and information in README.md

param(
    [string]$ReadmePath = "README.md",
    [switch]$UpdateBadges = $true,
    [switch]$UpdateTable = $true,
    [switch]$CreateBackup = $true
)

# Check if README file exists
if (-not (Test-Path $ReadmePath)) {
    Write-Warning "README file does not exist: $ReadmePath"
    Write-Host "Create new README file? (y/N): " -NoNewline
    $create = Read-Host
    if ($create -eq 'y' -or $create -eq 'Y') {
        # Create basic README template
        $template = @"
# CodeRogue

A Roguelike game project developed with Godot 4.x.

## Project Statistics

<!-- STATS_BADGES_START -->
<!-- STATS_BADGES_END -->

<!-- STATS_TABLE_START -->
<!-- STATS_TABLE_END -->

## Quick Start

### Requirements
- Godot 4.4+
- .NET 8.0+

### Running the Project
1. Clone the repository
2. Open project with Godot
3. Run the main scene

## Project Structure

- `Scripts/` - C# script files
- `Scenes/` - Godot scene files
- `ResourcesData/` - Game configuration data
- `Design/` - Design documents

## Development

This project follows MVVM architecture pattern.

---

*Statistics automatically updated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')*
"@
        $template | Out-File -FilePath $ReadmePath -Encoding utf8
        Write-Host "README file created successfully" -ForegroundColor Green
    } else {
        exit 0
    }
}

# Create backup
if ($CreateBackup) {
    $backupPath = "$ReadmePath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Copy-Item $ReadmePath $backupPath
    Write-Host "Backup created: $backupPath" -ForegroundColor Cyan
}

# Get current statistics
Write-Host "Getting project statistics..." -ForegroundColor Cyan
$statsJson = & "$PSScriptRoot\project_stats.ps1" -OutputJson
Write-Host "Raw JSON output: $statsJson" -ForegroundColor Yellow
$stats = $statsJson | ConvertFrom-Json
Write-Host "Parsed stats - C# Files: $($stats.CsFiles), Classes: $($stats.Classes)" -ForegroundColor Yellow

# Read current README content
$readmeContent = Get-Content $ReadmePath -Raw -Encoding utf8

# Update badges
if ($UpdateBadges) {
    Write-Host "Updating statistics badges..." -ForegroundColor Cyan
    
    $badges = @"
![Classes](https://img.shields.io/badge/Classes-$($stats.Classes)-blue?style=flat-square)
![Lines of Code](https://img.shields.io/badge/Lines_of_Code-$($stats.CodeLines)-green?style=flat-square)
![C# Files](https://img.shields.io/badge/C%23_Files-$($stats.CsFiles)-orange?style=flat-square)
![TSCN Files](https://img.shields.io/badge/TSCN_Files-$($stats.TscnFiles)-purple?style=flat-square)
![TRES Files](https://img.shields.io/badge/TRES_Files-$($stats.TresFiles)-red?style=flat-square)
![Last Updated](https://img.shields.io/badge/Updated-$(Get-Date -Format 'yyyy--MM--dd')-lightgrey?style=flat-square)
"@
    
    # Replace badges section
    $badgePattern = '(?s)<!-- STATS_BADGES_START -->.*?<!-- STATS_BADGES_END -->'
    $badgeReplacement = "<!-- STATS_BADGES_START -->`n$badges`n<!-- STATS_BADGES_END -->"
    
    if ($readmeContent -match $badgePattern) {
        $readmeContent = $readmeContent -replace $badgePattern, $badgeReplacement
    } else {
        # If no markers found, add at the beginning
        $readmeContent = $badgeReplacement + "`n`n" + $readmeContent
    }
}

# Update statistics table
if ($UpdateTable) {
    Write-Host "Updating statistics table..." -ForegroundColor Cyan
    
    $table = @"
| Statistics | Count | Description |
|------------|-------|--------------|
| C# Files | **$($stats.CsFiles)** | Number of C# script files in the project |
| Classes | **$($stats.Classes)** | Total number of defined classes |
| Lines of Code | **$($stats.CodeLines)** | Total lines of code (excluding empty lines and comments) |
| TSCN Files | **$($stats.TscnFiles)** | Number of Godot scene files |
| TRES Files | **$($stats.TresFiles)** | Number of Godot resource files |

> Last updated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
"@
    
    # Replace table section
    $tablePattern = '(?s)<!-- STATS_TABLE_START -->.*?<!-- STATS_TABLE_END -->'
    $tableReplacement = "<!-- STATS_TABLE_START -->`n$table`n<!-- STATS_TABLE_END -->"
    
    if ($readmeContent -match $tablePattern) {
        $readmeContent = $readmeContent -replace $tablePattern, $tableReplacement
    } else {
        # If no markers found, add after badges
        $insertPoint = '<!-- STATS_BADGES_END -->'
        if ($readmeContent -match $insertPoint) {
            $readmeContent = $readmeContent -replace $insertPoint, "$insertPoint`n`n$tableReplacement"
        } else {
            $readmeContent = $tableReplacement + "`n`n" + $readmeContent
        }
    }
}

# Write updated content
try {
    $readmeContent | Out-File -FilePath $ReadmePath -Encoding utf8 -NoNewline
    Write-Host "README file updated successfully" -ForegroundColor Green
    
    # Show update summary
     Write-Host "`n=== Update Summary ===" -ForegroundColor Magenta
     Write-Host "File: $ReadmePath" -ForegroundColor White
     Write-Host "C# Files: $($stats.CsFiles)" -ForegroundColor White
     Write-Host "Classes: $($stats.Classes)" -ForegroundColor White
     Write-Host "Lines of Code: $($stats.CodeLines)" -ForegroundColor White
     Write-Host "TSCN Files: $($stats.TscnFiles)" -ForegroundColor White
     Write-Host "TRES Files: $($stats.TresFiles)" -ForegroundColor White
     Write-Host "Update Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor White
     Write-Host "=====================" -ForegroundColor Magenta
    
} catch {
    Write-Error "Failed to update README file: $_"
    
    # If backup exists, ask to restore
    if ($CreateBackup -and (Test-Path $backupPath)) {
        Write-Host "Restore backup file? (y/N): " -NoNewline
        $restore = Read-Host
        if ($restore -eq 'y' -or $restore -eq 'Y') {
            Copy-Item $backupPath $ReadmePath
            Write-Host "Backup file restored successfully" -ForegroundColor Green
        }
    }
    exit 1
}

# Show git status if in git repository
if (Test-Path ".git") {
    Write-Host "`nGit Status:" -ForegroundColor Cyan
    git status --short $ReadmePath
}

Write-Host "`nREADME statistics update completed!" -ForegroundColor Green