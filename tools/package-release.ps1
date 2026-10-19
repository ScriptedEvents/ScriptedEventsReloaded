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

# Online fallbacks for links whose target is not shipped in a bundle.
$onlineDocsBaseUrl = 'https://scriptedevents.github.io/ScriptedEventsReloaded/'
$repositoryWebUrl = 'https://github.com/ScriptedEvents/ScriptedEventsReloaded'
$repositoryRawUrl = 'https://raw.githubusercontent.com/ScriptedEvents/ScriptedEventsReloaded/main'
$markdownLinkPattern = '(?<prefix>\]\(|\bsrc=")(?<target>[^)"\s]+)(?<suffix>[)"])'

function ConvertTo-UrlPath {
    param([Parameter(Mandatory)] [AllowEmptyString()] [string]$RelativePath)

    (($RelativePath -split '[\\/]') | ForEach-Object { [System.Uri]::EscapeDataString($_) }) -join '/'
}

function Test-IsRelativeLinkTarget {
    param([Parameter(Mandatory)] [string]$Target)

    # Skip web/mail links, in-page anchors and site-root paths.
    return -not ($Target -match '^(?:[a-zA-Z][a-zA-Z0-9+.-]*:|#|/)')
}

function Split-LinkTarget {
    param([Parameter(Mandatory)] [string]$Target)

    $hashIndex = $Target.IndexOf('#')
    if ($hashIndex -lt 0) {
        return @([System.Uri]::UnescapeDataString($Target), '')
    }
    return @([System.Uri]::UnescapeDataString($Target.Substring(0, $hashIndex)), $Target.Substring($hashIndex))
}

function Get-OnlineLinkTarget {
    param(
        [Parameter(Mandatory)] [string]$RepositoryRelativePath,
        [switch]$RawFile
    )

    $path = $RepositoryRelativePath -replace '\\', '/'
    if ($path -match '^docs/(?<page>.+)\.md$') {
        $page = $Matches['page']
        if ($page -eq 'README') {
            return $onlineDocsBaseUrl
        }
        if ($page.EndsWith('/README')) {
            $page = $page.Substring(0, $page.Length - 'README'.Length)
        }
        return $onlineDocsBaseUrl + (ConvertTo-UrlPath $page)
    }
    if ($RawFile) {
        return "$repositoryRawUrl/$(ConvertTo-UrlPath $path)"
    }
    $kind = if (Test-Path -LiteralPath (Join-Path $repositoryDirectory $path) -PathType Container) { 'tree' } else { 'blob' }
    return "$repositoryWebUrl/$kind/main/$(ConvertTo-UrlPath $path)"
}

# Copies a repository Markdown file to the root of a bundle. Relative links are
# pointed at the matching file inside that bundle when it is shipped there, and
# at the online documentation or GitHub repository when it is not.
function Copy-MarkdownToBundleRoot {
    param(
        [Parameter(Mandatory)] [string]$RepositoryRelativePath,
        [Parameter(Mandatory)] [string]$BundleRoot
    )

    $sourceFile = Join-Path $repositoryDirectory $RepositoryRelativePath
    $sourceDirectory = Split-Path -Parent $sourceFile
    $repositoryPrefix = $repositoryDirectory.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $content = [System.IO.File]::ReadAllText($sourceFile)
    $output = New-Object System.Text.StringBuilder
    $position = 0

    foreach ($match in [regex]::Matches($content, $markdownLinkPattern)) {
        $target = $match.Groups['target'].Value
        $newTarget = $target
        if (Test-IsRelativeLinkTarget $target) {
            $pathPart, $anchor = Split-LinkTarget $target
            $fullPath = [System.IO.Path]::GetFullPath((Join-Path $sourceDirectory $pathPart))
            if (-not $fullPath.StartsWith($repositoryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "$RepositoryRelativePath links outside the repository: $target"
            }
            $relativeTarget = $fullPath.Substring($repositoryPrefix.Length).TrimEnd('\', '/')
            if (Test-Path -LiteralPath (Join-Path $BundleRoot $relativeTarget)) {
                $newTarget = (ConvertTo-UrlPath $relativeTarget) + $anchor
            }
            else {
                $isImage = $match.Groups['prefix'].Value -ne ']('
                $newTarget = (Get-OnlineLinkTarget -RepositoryRelativePath $relativeTarget -RawFile:$isImage) + $anchor
            }
        }

        $targetGroup = $match.Groups['target']
        [void]$output.Append($content, $position, $targetGroup.Index - $position)
        [void]$output.Append($newTarget)
        $position = $targetGroup.Index + $targetGroup.Length
    }
    [void]$output.Append($content, $position, $content.Length - $position)

    $destination = Join-Path $BundleRoot (Split-Path -Leaf $sourceFile)
    [System.IO.File]::WriteAllText($destination, $output.ToString(), (New-Object System.Text.UTF8Encoding($false)))
}

# Fails the release if any Markdown file in a bundle links to a local file that
# is missing from that bundle.
function Assert-BundleMarkdownLinks {
    param([Parameter(Mandatory)] [string]$BundleRoot)

    $bundlePrefix = [System.IO.Path]::GetFullPath($BundleRoot).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $brokenLinks = New-Object System.Collections.Generic.List[string]
    foreach ($file in Get-ChildItem -LiteralPath $BundleRoot -Filter '*.md' -File -Recurse) {
        $content = [System.IO.File]::ReadAllText($file.FullName)
        foreach ($match in [regex]::Matches($content, $markdownLinkPattern)) {
            $target = $match.Groups['target'].Value
            if (-not (Test-IsRelativeLinkTarget $target)) {
                continue
            }
            $pathPart = (Split-LinkTarget $target)[0]
            if ($pathPart.Length -eq 0) {
                continue
            }
            $fullPath = [System.IO.Path]::GetFullPath((Join-Path $file.DirectoryName $pathPart))
            if (-not $fullPath.StartsWith($bundlePrefix, [System.StringComparison]::OrdinalIgnoreCase) -or
                -not (Test-Path -LiteralPath $fullPath)) {
                $brokenLinks.Add("$($file.FullName.Substring($bundlePrefix.Length)): $target")
            }
        }
    }

    if ($brokenLinks.Count -gt 0) {
        throw "Bundle $BundleRoot has links to files it does not contain:`n$($brokenLinks -join "`n")"
    }
}

function Copy-ReleaseNotices {
    param([Parameter(Mandatory)] [string]$Destination)

    Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'LICENSE') -Destination $Destination
    Copy-Item -LiteralPath (Join-Path $repositoryDirectory 'THIRD_PARTY_LICENSES.txt') -Destination $Destination
    # Copy these last, after the bundle's other content, so their links can be
    # pointed at files that are actually in the bundle.
    Copy-MarkdownToBundleRoot -RepositoryRelativePath 'README.md' -BundleRoot $Destination
    Copy-MarkdownToBundleRoot -RepositoryRelativePath 'docs\getting-started\installation.md' -BundleRoot $Destination
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

foreach ($stage in @($learningStage, $editorStage, $completeStage)) {
    Assert-BundleMarkdownLinks -BundleRoot $stage
}

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
