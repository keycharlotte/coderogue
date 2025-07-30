# Generate historical statistics data for visualization
# This script creates sample historical data or processes existing Git history

param(
    [string]$OutputFile = "historical_stats.json",     # Output JSON file
    [int]$DaysBack = 90,                              # How many days back to generate data
    [switch]$UseGitHistory = $false,                  # Use actual Git commit history
    [switch]$GenerateSample = $true                   # Generate sample data
)

# Function to get project statistics for a specific date/commit
function Get-ProjectStatsAtCommit {
    param(
        [string]$CommitHash = "HEAD"
    )
    
    try {
        # Checkout the specific commit (in a safe way)
        $currentBranch = git rev-parse --abbrev-ref HEAD
        git checkout $CommitHash --quiet
        
        # Get statistics
        $stats = & "$PSScriptRoot\project_stats.ps1" -OutputJson | ConvertFrom-Json
        
        # Return to original branch
        git checkout $currentBranch --quiet
        
        return $stats
    } catch {
        Write-Warning "Failed to get stats for commit $CommitHash`: $_"
        return $null
    }
}

# Function to generate sample historical data
function Generate-SampleData {
    param(
        [int]$Days
    )
    
    Write-Host "Generating sample historical data for $Days days..." -ForegroundColor Cyan
    
    $data = @()
    $startDate = (Get-Date).AddDays(-$Days)
    
    # Base values (simulating project growth)
    $baseCsFiles = 30
    $baseClasses = 80
    $baseCodeLines = 2500
    $baseTscnFiles = 15
    $baseTresFiles = 10
    
    for ($i = 0; $i -lt $Days; $i += 3) {
        $date = $startDate.AddDays($i)
        
        # Simulate realistic growth patterns
        $growthFactor = [Math]::Min($i / 30.0, 3.0)  # Slower growth over time
        $randomVariation = (Get-Random -Minimum -2 -Maximum 3)
        
        $csFiles = [Math]::Max(1, $baseCsFiles + [Math]::Floor($growthFactor * 5) + $randomVariation)
        $classes = [Math]::Max(1, $baseClasses + [Math]::Floor($growthFactor * 15) + $randomVariation * 2)
        $codeLines = [Math]::Max(100, $baseCodeLines + [Math]::Floor($growthFactor * 400) + $randomVariation * 50)
        $tscnFiles = [Math]::Max(1, $baseTscnFiles + [Math]::Floor($growthFactor * 3) + [Math]::Floor($randomVariation / 2))
        $tresFiles = [Math]::Max(1, $baseTresFiles + [Math]::Floor($growthFactor * 2) + [Math]::Floor($randomVariation / 2))
        
        # Add some realistic spikes (major development days)
        if ((Get-Random -Minimum 1 -Maximum 10) -eq 1) {
            $spike = Get-Random -Minimum 5 -Maximum 15
            $classes += $spike
            $codeLines += $spike * 20
        }
        
        $data += [PSCustomObject]@{
            date = $date.ToString("yyyy-MM-dd")
            CsFiles = $csFiles
            Classes = $classes
            CodeLines = $codeLines
            TscnFiles = $tscnFiles
            TresFiles = $tresFiles
            commit = "sample-" + $date.ToString("yyyyMMdd")
            message = "Development progress - " + $date.ToString("MMM dd")
        }
    }
    
    return $data
}

# Function to generate data from Git history
function Generate-FromGitHistory {
    param(
        [int]$Days
    )
    
    Write-Host "Generating data from Git history for last $Days days..." -ForegroundColor Cyan
    
    # Get commits from the last N days
    $sinceDate = (Get-Date).AddDays(-$Days).ToString("yyyy-MM-dd")
    $commits = git log --since="$sinceDate" --pretty=format:"%H|%ad|%s" --date=short | ForEach-Object {
        $parts = $_ -split '\|', 3
        [PSCustomObject]@{
            Hash = $parts[0]
            Date = $parts[1]
            Message = $parts[2]
        }
    }
    
    if (-not $commits) {
        Write-Warning "No commits found in the last $Days days. Using sample data instead."
        return Generate-SampleData -Days $Days
    }
    
    Write-Host "Found $($commits.Count) commits. Processing..." -ForegroundColor Yellow
    
    $data = @()
    $processedDates = @{}
    
    foreach ($commit in $commits) {
        # Skip if we already processed this date
        if ($processedDates.ContainsKey($commit.Date)) {
            continue
        }
        
        Write-Host "Processing commit $($commit.Hash.Substring(0,7)) from $($commit.Date)..." -ForegroundColor Gray
        
        $stats = Get-ProjectStatsAtCommit -CommitHash $commit.Hash
        if ($stats) {
            $data += [PSCustomObject]@{
                date = $commit.Date
                CsFiles = $stats.CsFiles
                Classes = $stats.Classes
                CodeLines = $stats.CodeLines
                TscnFiles = $stats.TscnFiles
                TresFiles = $stats.TresFiles
                commit = $commit.Hash.Substring(0,7)
                message = $commit.Message
            }
            
            $processedDates[$commit.Date] = $true
        }
        
        # Add a small delay to avoid overwhelming Git
        Start-Sleep -Milliseconds 100
    }
    
    if ($data.Count -eq 0) {
        Write-Warning "No valid statistics found in Git history. Using sample data instead."
        return Generate-SampleData -Days $Days
    }
    
    return $data | Sort-Object date
}

# Main execution
Write-Host "=== Historical Statistics Generator ===" -ForegroundColor Green
Write-Host "Output file: $OutputFile" -ForegroundColor Cyan
Write-Host "Days back: $DaysBack" -ForegroundColor Cyan

# Check if we're in a Git repository
if (-not (Test-Path ".git")) {
    Write-Warning "Not in a Git repository. Forcing sample data generation."
    $UseGitHistory = $false
    $GenerateSample = $true
}

# Generate data based on parameters
if ($UseGitHistory -and -not $GenerateSample) {
    $historicalData = Generate-FromGitHistory -Days $DaysBack
} else {
    $historicalData = Generate-SampleData -Days $DaysBack
}

# Add current statistics as the latest entry
Write-Host "Adding current project statistics..." -ForegroundColor Cyan
try {
    $currentStats = & "$PSScriptRoot\project_stats.ps1" -OutputJson | ConvertFrom-Json
    $currentDate = Get-Date -Format "yyyy-MM-dd"
    
    # Remove any existing entry for today
    $historicalData = $historicalData | Where-Object { $_.date -ne $currentDate }
    
    # Add current stats
    $historicalData += [PSCustomObject]@{
        date = $currentDate
        CsFiles = $currentStats.CsFiles
        Classes = $currentStats.Classes
        CodeLines = $currentStats.CodeLines
        TscnFiles = $currentStats.TscnFiles
        TresFiles = $currentStats.TresFiles
        commit = "current"
        message = "Current state"
    }
} catch {
    Write-Warning "Failed to get current statistics: $_"
}

# Sort data by date
$historicalData = $historicalData | Sort-Object date

# Save to JSON file
Write-Host "Saving data to $OutputFile..." -ForegroundColor Cyan
try {
    $historicalData | ConvertTo-Json -Depth 10 | Out-File -FilePath $OutputFile -Encoding utf8
    Write-Host "✅ Successfully generated $($historicalData.Count) data points" -ForegroundColor Green
    
    # Display summary
    Write-Host "`n=== Data Summary ===" -ForegroundColor Yellow
    Write-Host "Date range: $($historicalData[0].date) to $($historicalData[-1].date)"
    Write-Host "Total entries: $($historicalData.Count)"
    
    if ($historicalData.Count -gt 0) {
        $latest = $historicalData[-1]
        Write-Host "Latest stats:"
        Write-Host "  - C# Files: $($latest.CsFiles)"
        Write-Host "  - Classes: $($latest.Classes)"
        Write-Host "  - Lines of Code: $($latest.CodeLines)"
        Write-Host "  - Scene Files: $($latest.TscnFiles)"
        Write-Host "  - Resource Files: $($latest.TresFiles)"
    }
    
    Write-Host "`n📊 Open stats_visualization.html to view the charts!" -ForegroundColor Magenta
    
} catch {
    Write-Error "Failed to save data: $_"
    exit 1
}

Write-Host "`n🎉 Historical data generation completed!" -ForegroundColor Green