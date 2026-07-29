# Installation

SER 1.0 is still in development. Use an artifact built from the same commit as
these docs or one supplied by the project maintainers. Public documentation for
older releases may describe different syntax.

This guide assumes the SCP:SL dedicated server and its plugin loader already
work.

## Choose a plugin build

SER produces two assemblies:

| Host | Assembly | Typical destination |
|---|---|---|
| LabAPI | `SER.dll` | `LabAPI/plugins/<port>/` or `LabAPI/plugins/global/` |
| EXILED | `SER-Exiled.dll` | `EXILED/Plugins/` |

Use only one assembly for a server instance. The LabAPI build is the default.

For local development:

```text
dotnet build --configuration Release
dotnet build --configuration EXILED
```

The resulting assemblies are placed under `bin/LABAPI/net48/` and
`bin/EXILED/net48/`.

## Start and verify

Restart the server, then run:

```text
serhelp start
serstatus
```

`serhelp start` prints the exact script directory used by that installation.
`serstatus` confirms that SER can scan it and reports any script errors.

The large startup logo is disabled by default, so the absence of ASCII art is
not an installation failure.

## Generate examples

In the server console, run:

```text
serexamples
```

SER creates an `Example Scripts` directory. Generated filenames begin with `#`,
which keeps them disabled. Copy an example or remove its leading `#`, then run
`serreload`.

The generated scripts come from this repository's
[`Example Scripts`](../../Example%20Scripts) directory and are compiled during
the build.

## Editor options

- Use any text editor for `.ser` or `.txt` files.
- Use the VS Code extension for completion, hovers, signature help, and shared
  diagnostics.
- Use SER Blocks to learn common tasks visually, then switch to text when you
  need the full language.

Next: [create your first script](first-script.md).
