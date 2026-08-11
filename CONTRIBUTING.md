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
plugin DLL there, so leave it unset if you only want to compile.

```powershell
$env:SL_DEV_REFERENCES = 'C:\path\to\scp-sl-references'
dotnet restore SER.sln
```

## Everyday workflow

Build the LabAPI plugin and run the repository's example-script validation:

```powershell
dotnet build SER.csproj -c Release --no-restore
```

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

## Before opening a pull request

1. Keep a change focused and update documentation/examples when behavior or
   script syntax changes.
2. Run the applicable commands above. Every implementation change must include
   a successful `Release` build.
3. Check `git status --ignored --short` if an unexpected generated file appears.
4. Describe the user-visible effect, verification performed, and any required
   SCP:SL/LabAPI version in the pull request.
