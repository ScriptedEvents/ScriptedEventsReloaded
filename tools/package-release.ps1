[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [string]$UcrReferencePath,
    [switch]$SkipDependencyInstall
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryDirectory = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$toolingDirectory = Join-Path $repositoryDirectory 'Tooling'
$extensionDirectory = Join-Path $repositoryDirectory 'VS Code Extension'
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryDirectory 'artifacts'
}
$outputRoot = [System.IO.Path]::GetFullPath($OutputDirectory)

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory)] [string]$FilePath,
        [Parameter(Mandatory)] [string[]]$Arguments,
        [Parameter(Mandatory)] [string]$WorkingDirectory
    )

    Push-Location -LiteralPath $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}

function Copy-ReleaseNotices {
    param([Parameter(Mandatory)] [string]$Destination)

    Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'LICENSE') -Destination $Destination
    Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'THIRD_PARTY_LICENSES.txt') -Destination $Destination
    Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'README.md') -Destination $Destination
    Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'docs\getting-started\installation.md') -Destination $Destination
}

function Compress-ReleaseDirectory {
    param(
        [Parameter(Mandatory)] [string]$Source,
        [Parameter(Mandatory)] [string]$Destination
    )

    Compress-Archive -Path (Join-Path $Source '*') -DestinationPath $Destination -CompressionLevel Optimal
}

if (-not $SkipDependencyInstall) {
    Invoke-CheckedCommand -FilePath 'npm' -Arguments @('ci') -WorkingDirectory $toolingDirectory
    Invoke-CheckedCommand -FilePath 'npm' -Arguments @('ci') -WorkingDirectory $extensionDirectory
}

$exiledBuildArguments = @('build', 'SER.csproj', '--configuration', 'EXILED', '--no-restore')
$releaseBuildArguments = @('build', 'SER.csproj', '--configuration', 'Release', '--no-restore')
if (-not [string]::IsNullOrWhiteSpace($UcrReferencePath)) {
    $resolvedUcrReferencePath = (Resolve-Path -LiteralPath $UcrReferencePath -ErrorAction Stop).Path
    $releaseBuildArguments += "-p:UcrReferencePath=$resolvedUcrReferencePath"
    $exiledBuildArguments += "-p:UcrReferencePath=$resolvedUcrReferencePath"
}
# LabAPI is SER's backbone and the canonical source for shared editor/help
# metadata. Build EXILED first, then leave the LabAPI manifest in place for the
# tooling build below.
Invoke-CheckedCommand -FilePath 'dotnet' -Arguments $exiledBuildArguments -WorkingDirectory $repositoryDirectory
Invoke-CheckedCommand -FilePath 'dotnet' -Arguments $releaseBuildArguments -WorkingDirectory $repositoryDirectory
Invoke-CheckedCommand -FilePath 'npm' -Arguments @('run', 'verify') -WorkingDirectory $toolingDirectory
Invoke-CheckedCommand -FilePath 'npm' -Arguments @('run', 'verify') -WorkingDirectory $extensionDirectory

$labApiAssembly = Join-Path $repositoryDirectory 'bin\LABAPI\net48\SER.dll'
$exiledAssembly = Join-Path $repositoryDirectory 'bin\EXILED\net48\SER-Exiled.dll'
$labApiVersion = [System.Reflection.AssemblyName]::GetAssemblyName($labApiAssembly).Version
$exiledVersion = [System.Reflection.AssemblyName]::GetAssemblyName($exiledAssembly).Version
if ($labApiVersion -ne $exiledVersion) {
    throw "Plugin assembly versions differ: LabAPI $labApiVersion, EXILED $exiledVersion."
}

$coreVersion = "$($labApiVersion.Major).$($labApiVersion.Minor).$($labApiVersion.Build)"
$extensionManifest = Get-Content -LiteralPath (Join-Path $extensionDirectory 'package.json') -Raw | ConvertFrom-Json
$extensionVersion = [string]$extensionManifest.version
$releaseDirectory = Join-Path $outputRoot "SER-$coreVersion-release"
if (Test-Path -LiteralPath $releaseDirectory) {
    throw "Release output already exists: $releaseDirectory. Choose a different -OutputDirectory or archive the existing release first."
}

$stagingDirectory = Join-Path $releaseDirectory 'staging'
$learningStage = Join-Path $stagingDirectory 'Examples-and-Documentation'
$editorStage = Join-Path $stagingDirectory 'Visual-Editor'
$completeStage = Join-Path $stagingDirectory 'Complete'
New-Item -ItemType Directory -Path $learningStage, $editorStage, $completeStage -Force | Out-Null

# Keep the normal server install as a one-file download. Notices remain in the
# optional bundles and in the repository without hiding the plugin DLLs.
Copy-Item -LiteralPath $labApiAssembly -Destination (Join-Path $releaseDirectory 'SER.dll')
Copy-Item -LiteralPath $exiledAssembly -Destination (Join-Path $releaseDirectory 'SER-Exiled.dll')

Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'Example Scripts') -Destination $learningStage -Recurse
Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'docs') -Destination $learningStage -Recurse
Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'language_specification.md') -Destination $learningStage
Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'PROJECT_GUIDE.md') -Destination $learningStage
Copy-ReleaseNotices -Destination $learningStage

Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'SER Visual Editor.html') -Destination $editorStage
Copy-ReleaseNotices -Destination $editorStage

New-Item -ItemType Directory -Path (Join-Path $completeStage 'Plugins\LabAPI'), (Join-Path $completeStage 'Plugins\EXILED'), (Join-Path $completeStage 'Editor'), (Join-Path $completeStage 'VS Code Extension') -Force | Out-Null
Copy-Item -LiteralPath $labApiAssembly -Destination (Join-Path $completeStage 'Plugins\LabAPI')
Copy-Item -LiteralPath $exiledAssembly -Destination (Join-Path $completeStage 'Plugins\EXILED')
Copy-Item -LiteralPath (Join-Path $learningStage 'Example Scripts') -Destination $completeStage -Recurse
Copy-Item -LiteralPath (Join-Path $learningStage 'docs') -Destination $completeStage -Recurse
Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'language_specification.md') -Destination $completeStage
Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'PROJECT_GUIDE.md') -Destination $completeStage
Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'SER Visual Editor.html') -Destination (Join-Path $completeStage 'Editor')
Copy-ReleaseNotices -Destination $completeStage

$vsixPath = Join-Path $releaseDirectory "ser-$extensionVersion.vsix"
Invoke-CheckedCommand -FilePath 'npm' -Arguments @('run', 'package', '--', '--out', $vsixPath) -WorkingDirectory $extensionDirectory
Copy-Item -LiteralPath $vsixPath -Destination (Join-Path $completeStage 'VS Code Extension')

Compress-ReleaseDirectory -Source $learningStage -Destination (Join-Path $releaseDirectory "SER-$coreVersion-Examples-and-Documentation.zip")
Compress-ReleaseDirectory -Source $editorStage -Destination (Join-Path $releaseDirectory "SER-Visual-Editor-$coreVersion.zip")
Compress-ReleaseDirectory -Source $completeStage -Destination (Join-Path $releaseDirectory "SER-$coreVersion-Complete.zip")

$resolvedStagingDirectory = (Resolve-Path -LiteralPath $stagingDirectory).Path
$resolvedReleaseDirectory = (Resolve-Path -LiteralPath $releaseDirectory).Path.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
if (-not $resolvedStagingDirectory.StartsWith($resolvedReleaseDirectory, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to remove staging directory outside the release directory: $resolvedStagingDirectory"
}
Remove-Item -LiteralPath $resolvedStagingDirectory -Recurse -Force

$artifactFiles = Get-ChildItem -LiteralPath $releaseDirectory -File | Sort-Object Name
$checksumLines = foreach ($artifact in $artifactFiles) {
    $hash = (Get-FileHash -LiteralPath $artifact.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $($artifact.Name)"
}
[System.IO.File]::WriteAllLines((Join-Path $releaseDirectory 'SHA256SUMS.txt'), $checksumLines)

Write-Host "SER $coreVersion release artifacts created in $releaseDirectory"
Get-ChildItem -LiteralPath $releaseDirectory -File | Sort-Object Name | Select-Object Name, Length
