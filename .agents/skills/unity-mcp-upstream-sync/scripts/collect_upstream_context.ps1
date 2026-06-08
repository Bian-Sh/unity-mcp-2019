param(
    [string]$UpstreamRef = "upstream/beta",
    [string]$BaseCommit = "",
    [string]$OutputPath = "Temp/upstream-sync-context.md"
)

$ErrorActionPreference = "Stop"

function Write-Section {
    param([string]$Title)
    Add-Content -Path $OutputPath -Value ""
    Add-Content -Path $OutputPath -Value "## $Title"
    Add-Content -Path $OutputPath -Value ""
}

function Add-CommandOutput {
    param(
        [string]$Title,
        [string]$Command
    )

    Write-Section $Title
    Add-Content -Path $OutputPath -Value '```text'
    try {
        $output = Invoke-Expression $Command 2>&1 | Out-String
        Add-Content -Path $OutputPath -Value $output.TrimEnd()
    }
    catch {
        Add-Content -Path $OutputPath -Value $_.Exception.Message
    }
    Add-Content -Path $OutputPath -Value '```'
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Force $outputDirectory | Out-Null
}

if (-not $BaseCommit) {
    $BaseCommit = (git merge-base HEAD $UpstreamRef).Trim()
}

Set-Content -Path $OutputPath -Encoding UTF8 -Value "# Upstream Sync Context"
Add-Content -Path $OutputPath -Value ""
Add-Content -Path $OutputPath -Value "- Generated: $(Get-Date -Format o)"
Add-Content -Path $OutputPath -Value "- UpstreamRef: $UpstreamRef"
Add-Content -Path $OutputPath -Value "- BaseCommit: $BaseCommit"
Add-Content -Path $OutputPath -Value "- TargetCommit: $(git rev-parse $UpstreamRef)"

Add-CommandOutput -Title "Working Tree Status" -Command "git status --short"
Add-CommandOutput -Title "Ahead Behind" -Command "git rev-list --left-right --count HEAD...$UpstreamRef"
Add-CommandOutput -Title "Changed Files" -Command "git diff --name-status $BaseCommit..$UpstreamRef"
Add-CommandOutput -Title "Recent Upstream Commits" -Command "git log --oneline --decorate -n 30 $BaseCommit..$UpstreamRef"
Add-CommandOutput -Title "Recent Local Commits" -Command "git log --oneline --decorate -n 30 $UpstreamRef..HEAD"

Write-Output "Wrote $OutputPath"
