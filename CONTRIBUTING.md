# Contributing to Scripted Events Reloaded

Thanks for helping improve SER. This guide gets a Windows contributor from a
fresh clone to a verified change without relying on files from another
developer's machine.

## Prerequisites

- Windows with the .NET Framework 4.8 Developer Pack.
- The .NET SDK selected by [`global.json`](./global.json). Confirm it with
  `dotnet --version`.
- SCP:SL/LabAPI/Unity reference DLLs in a local directory. Set
  `SL_DEV_REFERENCES` to that directory before restoring or building.
- Node.js 22 or newer when changing the editor, VS Code extension, or website.

`LABAPI_PLUGINS` is optional. When it is set, a non-EXILED build copies the
plugin DLL there, so leave it unset if you only want to compile. SER treats
`SL_DEV_REFERENCES` as read-only unless you explicitly pass
`-p:CopySerToReferenceDirectory=true`.

```powershell
$env:SL_DEV_REFERENCES = 'C:\path\to\scp-sl-references'
dotnet restore SER.sln
```

## Everyday workflow

Build the LabAPI plugin and run the repository's example-script validation:

```powershell
dotnet build SER.csproj -c Release --no-restore
```

The normal command compiles every example and refreshes the generated editor
data. For a quick compile while editing C#, use
`-p:RunSerPostBuildValidation=false`, then run the normal Release command before
you hand the change off. To update your locally installed VS Code extension as
part of a build, pass `-p:InstallSerExtensionAfterBuild=true`.

The build writes ignored generated files such as `SER Visual Editor.html` and
`ser_method_info.js`. Do not add them to a commit. Versioned files under
`VS Code Extension/out/` are intentional release inputs; when tooling changes
them, include the synchronized output in the same commit.

For editor or extension changes, install the locked dependencies and run the
full tooling check:

```powershell
npm ci --prefix Tooling
npm run verify --prefix Tooling
```

For documentation-site changes, use its separate dependency tree:

```powershell
npm ci --prefix website
npm run build --prefix website
```

Use `tools/package-release.ps1` only when preparing distributable release
bundles. See [`PROJECT_GUIDE.md`](./PROJECT_GUIDE.md) for architecture,
configuration-specific builds, and release smoke-testing expectations.

## Publish the VS Code extension

Publish the current `main` branch from the repository root:

```powershell
.\tools\publish-vscode-extension.ps1 -Version 1.6.1
```

The command starts a GitHub workflow that builds, tests, packages and publishes
the extension. It accepts stable `major.minor.patch` versions, releases only
from `main`, and keeps the finished `.vsix` as a downloadable workflow artifact.

Marketplace access needs to be connected once. Open the
[ElektrykAndrzej publisher page](https://marketplace.visualstudio.com/manage/publishers/ElektrykAndrzej),
add a trusted GitHub Actions publisher, and use these values:

- GitHub owner: `ScriptedEvents`
- Repository: `ScriptedEventsReloaded`
- Workflow: `publish-vscode-extension.yml`

Later releases use a short-lived GitHub identity, so there is no Marketplace
token to save or renew. If GitHub CLI is signed out, run
`gh auth login --hostname github.com` once and retry the publish command.

## Before opening a pull request

1. Keep a change focused and update documentation/examples when behavior or
   script syntax changes.
2. Run the applicable commands above. Every implementation change must include
   a successful `Release` build.
3. Check `git status --ignored --short` if an unexpected generated file appears.
4. Describe the user-visible effect, verification performed, and any required
   SCP:SL/LabAPI version in the pull request.
