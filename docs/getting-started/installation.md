# Install SER and prove it works

This guide assumes you already have an SCP:SL dedicated server and know how to
install a plugin. It does not cover server hosting, ports, or Remote Admin setup.

Your goal is simple: load the correct SER build, ask it where scripts belong,
and generate working examples you can take apart.

## 1. Choose one plugin build

SER has two fully supported hosts:

| Your server uses | Install | Put it in |
|---|---|---|
| LabAPI | `SER.dll` | `LabAPI/plugins/<port>/` or `LabAPI/plugins/global/` |
| EXILED | `SER-Exiled.dll` | `EXILED/Plugins/` |

Most servers should use the LabAPI build. Install **one** of these assemblies,
never both on the same server instance.

SER 1.0 is built and runtime-tested against LabAPI 1.1.7 and EXILED 9.14.2.
The release downloads keep the two plugin DLLs in separate archives so it is
harder to install the wrong host accidentally.

> Use the plugin and documentation from the same SER release. Old tutorials may
> describe names or syntax which no longer exist.

## 2. Restart and ask SER for directions

After restarting the server, enter:

```text
serhelp start
```

SER prints the exact script directory for that installation and the commands
which matter on your first day. You do not need to guess whether a particular
port or loader changes the path.

Then run:

```text
serstatus
```

If SER reports its script directory and no failed files, the plugin is ready.
The large startup logo is disabled by default; missing ASCII art is not an
installation failure.

## 3. Generate scripts worth exploring

In the **server console**, run:

```text
serexamples
```

SER creates an `Example Scripts` folder containing small commands, welcome
messages, utilities, custom roles, and complete events. Every generated
filename starts with `#`, so examples are safe and disabled until you choose
one.

To enable an example, copy it or remove the leading `#`, then run:

```text
serreload
```

The source versions live in [`Example Scripts`](../../Example%20Scripts) and
are compiled during every SER build.

## 4. Pick an editor

- Any text editor works for `.ser` and `.txt` scripts.
- The VS Code extension adds completions, hover help, signatures, diagnostics,
  and **SER: Open Blocks Editor**.
- SER Blocks is a visual way to assemble common beginner scripts. The text
  language remains the route to every SER feature.

SER is ready. Continue with [make the server say something](first-script.md)
and return to the details below only when you need them.

## Installation reference

### Remote Admin permissions

The server console can use administrative SER commands directly. Remote Admin
groups need the matching permission:

| Permission | Commands |
|---|---|
| `ser.run` | `serrun`, `serstatus`/`serlist`, `serrunning`, `sermethod` |
| `ser.reload` | `serreload` |
| `ser.stop` | `serstop`, `serstopall` |
| `ser.docs` | `serdocs` |

`serhelp` needs no SER permission. `serexamples` is server-console-only because
it writes files.

### Building from source

Release maintainers can build both supported hosts with:

```text
dotnet build --configuration Release
dotnet build --configuration EXILED
```

The results are written to `bin/LABAPI/net48/` and `bin/EXILED/net48/`.
