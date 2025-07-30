# Auto commit script with project statistics
# Used to automatically generate commit messages with statistical data

param(
    [string]$CommitMessage = "",           # Base commit message
    [string]$CommitType = "feat",          # Commit type: feat, fix, docs, style, refactor, test, chore
    [switch]$IncludeStats = $true,         # Whether to include statistics
    [switch]$AutoPush = $false,            # Whether to auto push to remote
    [string]$Branch = "main",              # Target branch
    [switch]$AutoConfirm = $false          # Skip confirmation prompt
)

# Check if in Git repository
if (-not (Test-Path ".git")) {
    Write-Error "Current directory is not a Git repository"
    exit 1
}

# Check for uncommitted changes
$gitStatus = git status --porcelain
if (-not $gitStatus) {
    Write-Host "No changes to commit" -ForegroundColor Yellow
    exit 0
}

# Generate default commit message if not provided
if (-not $CommitMessage) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
    $CommitMessage = "Code update - $timestamp"
}

# Get current statistics
if ($IncludeStats) {
    Write-Host "Generating project statistics..." -ForegroundColor Cyan
    
    # Try to get last commit statistics for comparison
    $lastStatsFile = "last-commit-stats.json"
    $compareWith = if (Test-Path $lastStatsFile) { $lastStatsFile } else { "" }
    
    # Generate commit format statistics
    $statsInfo = & "$PSScriptRoot\format_stats.ps1" -OutputFormat commit -CompareWith $compareWith
    
    # Save current statistics for next comparison
    & "$PSScriptRoot\project_stats.ps1" -OutputJson | Out-File -FilePath $lastStatsFile -Encoding utf8
    
    # Build complete commit message
    $fullCommitMessage = @"
$CommitType`: $CommitMessage

$statsInfo

Auto-generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
"@
} else {
    $fullCommitMessage = "$CommitType`: $CommitMessage"
}

# Display commit preview
Write-Host "=== Commit Preview ===" -ForegroundColor Green
Write-Host $fullCommitMessage
Write-Host "=====================" -ForegroundColor Green

# Display files to be committed
Write-Host "`n=== Files to Commit ===" -ForegroundColor Yellow
git status --short
Write-Host "======================" -ForegroundColor Yellow

# Confirm commit
if (-not $AutoConfirm) {
    $confirmation = Read-Host "`nContinue with commit? (y/N)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Host "Commit cancelled" -ForegroundColor Red
        exit 0
    }
}

# Add all changes
Write-Host "`nAdding files..." -ForegroundColor Cyan
git add .

# Commit changes
Write-Host "Committing changes..." -ForegroundColor Cyan
try {
    git commit -m $fullCommitMessage
    Write-Host "✅ Commit successful!" -ForegroundColor Green
} catch {
    Write-Error "Commit failed: $_"
    exit 1
}

# Auto push if enabled
if ($AutoPush) {
    Write-Host "`nPushing to remote repository..." -ForegroundColor Cyan
    try {
        git push origin $Branch
        Write-Host "✅ Push successful!" -ForegroundColor Green
        
        # Display post-push statistics summary
        if ($IncludeStats) {
            Write-Host "`n=== Project Statistics Summary ===" -ForegroundColor Magenta
            & "$PSScriptRoot\format_stats.ps1" -OutputFormat summary
            Write-Host "=================================" -ForegroundColor Magenta
        }
    } catch {
        Write-Warning "Push failed: $_"
        Write-Host "Please manually execute: git push origin $Branch" -ForegroundColor Yellow
    }
}

# Generate GitHub Actions output
if ($env:GITHUB_ACTIONS) {
    Write-Host "`nGenerating GitHub Actions output..." -ForegroundColor Cyan
    
    # Set output variables
    $currentStats = & "$PSScriptRoot\project_stats.ps1" -OutputJson | ConvertFrom-Json
    
    echo "commit_message=$($CommitMessage)" >> $env:GITHUB_OUTPUT
    echo "classes_count=$($currentStats.Classes)" >> $env:GITHUB_OUTPUT
    echo "lines_count=$($currentStats.CodeLines)" >> $env:GITHUB_OUTPUT
    echo "files_count=$($currentStats.CsFiles)" >> $env:GITHUB_OUTPUT
    
    # Generate summary
    $summary = & "$PSScriptRoot\format_stats.ps1" -OutputFormat markdown
    $summary | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}

Write-Host "`n🎉 Operation completed!" -ForegroundColor Green