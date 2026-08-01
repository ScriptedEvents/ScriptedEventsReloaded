# Installation

Use the plugin artifact and documentation from the same SER release. When
testing a pre-release commit, use an artifact built from that exact commit.
Documentation for older releases may describe different syntax.

This guide assumes the SCP:SL dedicated server and its plugin loader already
work.

## Choose a plugin build

SER produces two assemblies:

| Host | Assembly | Typical destination |
|---|---|---|
| LabAPI | `SER.dll` | `LabAPI/plugins/<port>/` or `LabAPI/plugins/global/` |
| EXILED | `SER-Exiled.dll` | `EXILED/Plugins/` |

Use only one assembly for a server instance. The LabAPI build is the default.
SER 1.0 is built and runtime-tested against LabAPI 1.1.7.

The EXILED artifact is a first-class supported build, not a compatibility shim.
Install the official EXILED loader before copying `SER-Exiled.dll`. SER 1.0 is
built and runtime-tested against EXILED 9.14.2; use the framework version named
by the matching SER release notes when game updates require that baseline to
move. Do not place `SER.dll` and `SER-Exiled.dll` on the same server instance.

Official release downloads provide separate `LabAPI` and `EXILED` zip files.
Each contains only its plugin DLL, this installation guide, and the required
license notices. This keeps host references and build-only files out of server
plugin directories.

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

## Remote Admin permissions

The server console can use SER's administrative commands directly. Remote
Admin groups need the matching permission before those commands succeed:

| Permission | Commands |
|---|---|
| `ser.run` | `serrun`, `serstatus`/`serlist`, `serrunning`, `sermethod` |
| `ser.reload` | `serreload` |
| `ser.stop` | `serstop`, `serstopall` |
| `ser.docs` | `serdocs` |

`serhelp` does not require an SER permission. `serexamples` is intentionally a
server-console command because it writes example files.

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
