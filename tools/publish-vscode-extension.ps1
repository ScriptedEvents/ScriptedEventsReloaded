[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'Medium')]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$')]
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repository = 'ScriptedEvents/ScriptedEventsReloaded'
$workflow = 'publish-vscode-extension.yml'
$releaseTarget = "$repository VS Code extension $Version from main"

if (-not (Get-Command 'gh' -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI is required. Install it from https://cli.github.com/ and run 'gh auth login'."
}

if (-not $PSCmdlet.ShouldProcess($releaseTarget, 'Start the Marketplace publish workflow')) {
    return
}

& gh auth status --hostname github.com
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not signed in. Run 'gh auth login --hostname github.com', then try again."
}

& gh workflow run $workflow --repo $repository --ref main --raw-field "version=$Version"
if ($LASTEXITCODE -ne 0) {
    throw "GitHub could not start the VS Code extension publish workflow."
}

Write-Host "Publishing extension $Version from the current main branch."
Write-Host "Follow the run at https://github.com/$repository/actions/workflows/$workflow"
