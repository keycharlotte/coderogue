# Project Statistics Formatter
# Format project statistics into different output formats

param(
    [string]$OutputFormat = "markdown",
    [string]$PreviousStatsFile = "",
    [switch]$ShowChanges = $false,
    [switch]$Verbose = $false
)

# Get current statistics
$currentStats = & "$PSScriptRoot\project_stats.ps1" -OutputFormat json | ConvertFrom-Json

# Read previous statistics if provided
$previousStats = $null
if ($PreviousStatsFile -and (Test-Path $PreviousStatsFile)) {
    try {
        $previousStats = Get-Content $PreviousStatsFile -Raw | ConvertFrom-Json
        if ($Verbose) {
            Write-Host "Loaded previous stats from: $PreviousStatsFile" -ForegroundColor Cyan
        }
    } catch {
        Write-Warning "Cannot read previous stats file: $PreviousStatsFile"
    }
}

# Function to calculate changes
function Get-Change {
    param($current, $previous, $name)
    
    if ($previous -eq $null -or $previous -eq 0) {
        return "${name}: $current (new)"
    }
    
    $diff = $current - $previous
    $percent = if ($previous -gt 0) { [math]::Round(($diff / $previous) * 100, 1) } else { 0 }
    
    if ($diff -gt 0) {
        return "${name}: $current (+$diff, +${percent}%)"
    } elseif ($diff -lt 0) {
        return "${name}: $current ($diff, ${percent}%)"
    } else {
        return "${name}: $current (no change)"
    }
}

# Calculate changes for each metric
if ($ShowChanges -and $previousStats) {
    $csharpChange = Get-Change $currentStats.CSharpFiles $previousStats.CSharpFiles "C# Files"
    $classChange = Get-Change $currentStats.Classes $previousStats.Classes "Classes"
    $locChange = Get-Change $currentStats.LinesOfCode $previousStats.LinesOfCode "Lines of Code"
    $tscnChange = Get-Change $currentStats.TscnFiles $previousStats.TscnFiles "TSCN Files"
    $tresChange = Get-Change $currentStats.TresFiles $previousStats.TresFiles "TRES Files"
}

# Generate output based on format
switch ($OutputFormat.ToLower()) {
    "markdown" {
        $output = @"
## Project Statistics

| Metric | Count | Description |
|--------|-------|-------------|
| C# Files | **$($currentStats.CSharpFiles)** | Number of C# script files |
| Classes | **$($currentStats.Classes)** | Total number of defined classes |
| Lines of Code | **$($currentStats.LinesOfCode)** | Total lines of code (excluding empty lines and comments) |
| TSCN Files | **$($currentStats.TscnFiles)** | Number of Godot scene files |
| TRES Files | **$($currentStats.TresFiles)** | Number of Godot resource files |

> Last updated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
"@
        
        if ($ShowChanges -and $previousStats) {
            $output += "`n`n### Changes`n`n"
            $output += "- $csharpChange`n"
            $output += "- $classChange`n"
            $output += "- $locChange`n"
            $output += "- $tscnChange`n"
            $output += "- $tresChange`n"
        }
    }
    
    "commit" {
        $output = "Stats: $($currentStats.Classes) classes | $($currentStats.LinesOfCode) lines | $($currentStats.CSharpFiles) C# files"
        
        if ($ShowChanges -and $previousStats) {
            $classDiff = $currentStats.Classes - $previousStats.Classes
            $locDiff = $currentStats.LinesOfCode - $previousStats.LinesOfCode
            $fileDiff = $currentStats.CSharpFiles - $previousStats.CSharpFiles
            
            if ($classDiff -ne 0 -or $locDiff -ne 0 -or $fileDiff -ne 0) {
                $changes = @()
                if ($classDiff -ne 0) { $changes += "${classDiff} classes" }
                if ($locDiff -ne 0) { $changes += "${locDiff} lines" }
                if ($fileDiff -ne 0) { $changes += "${fileDiff} files" }
                $output += " (" + ($changes -join ", ") + ")"
            }
        }
    }
    
    "json" {
        $result = @{
            current = $currentStats
            timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
        }
        
        if ($ShowChanges -and $previousStats) {
            $result.previous = $previousStats
            $result.changes = @{
                CSharpFiles = $currentStats.CSharpFiles - $previousStats.CSharpFiles
                Classes = $currentStats.Classes - $previousStats.Classes
                LinesOfCode = $currentStats.LinesOfCode - $previousStats.LinesOfCode
                TscnFiles = $currentStats.TscnFiles - $previousStats.TscnFiles
                TresFiles = $currentStats.TresFiles - $previousStats.TresFiles
            }
        }
        
        $output = $result | ConvertTo-Json -Depth 3
    }
    
    "badge" {
        $output = @"
![Classes](https://img.shields.io/badge/Classes-$($currentStats.Classes)-blue?style=flat-square)
![Lines of Code](https://img.shields.io/badge/Lines_of_Code-$($currentStats.LinesOfCode)-green?style=flat-square)
![C# Files](https://img.shields.io/badge/C%23_Files-$($currentStats.CSharpFiles)-orange?style=flat-square)
![TSCN Files](https://img.shields.io/badge/TSCN_Files-$($currentStats.TscnFiles)-purple?style=flat-square)
![TRES Files](https://img.shields.io/badge/TRES_Files-$($currentStats.TresFiles)-red?style=flat-square)
"@
    }
    
    "summary" {
        $output = "$($currentStats.Classes) classes | $($currentStats.LinesOfCode) lines | $($currentStats.CSharpFiles) C# files | $($currentStats.TscnFiles) scenes | $($currentStats.TresFiles) resources"
    }
    
    default {
        Write-Error "Unsupported output format: $OutputFormat"
        Write-Host "Supported formats: markdown, commit, json, badge, summary"
        exit 1
    }
}

# Output the result
Write-Output $output

# Show additional info in verbose mode
if ($Verbose) {
    Write-Host "`n=== Formatting Complete ===" -ForegroundColor Magenta
    Write-Host "Output Format: $OutputFormat" -ForegroundColor White
    Write-Host "Current Stats: $($currentStats.Classes) classes, $($currentStats.LinesOfCode) lines" -ForegroundColor White
    if ($previousStats) {
        Write-Host "Historical Comparison: Enabled" -ForegroundColor White
    } else {
        Write-Host "Historical Comparison: Disabled" -ForegroundColor Gray
    }
    Write-Host "Processing Time: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor White
    Write-Host "==========================" -ForegroundColor Magenta
}