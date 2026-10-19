# Scripted Events Reloaded

Language support and a guided blocks editor for Scripted Events Reloaded 1.0.
The extension is generated from the same method, flag, event, variable, and
property manifest used by the plugin's built-in `serhelp` reference.

Language features include:

- hover documentation for methods, method arguments, keywords, flags and flag arguments;
- method and keyword completions at the beginning of a SER line;
- signature help that follows the currently typed method argument;
- value documentation and immediate completions for `EnumArgument` and `OptionsArgument` parameters;
- `run` completions for functions declared earlier in the script, including argument placeholders;
- variable completions filtered by the `@`, `$`, `&` and `*` type prefixes;
- global-variable completions collected from every SER script in the workspace;
- flag, flag-argument and `OnEvent` event-name completions, with flag arguments resolved against the nearest section above them.
- shared diagnostics for incomplete values, malformed function calls, unclosed
  statements and unsafe forever loops;
- the same **SER Blocks** beginner editor shipped as the standalone editor,
  available through **SER: Open Blocks Editor**. It uses guided recipes and a
  deliberately small block set; the text editor remains the path to the full
  SER language.

SER associates `.ser` files automatically. The plugin also accepts `.txt`
scripts for hosting-panel compatibility; in VS Code, choose the SER language
mode manually when editing one of those files.

Start with the
[SER 1.0 documentation](https://github.com/ScriptedEvents/ScriptedEventsReloaded/tree/main/docs),
or run `serhelp start` on a server to locate the active script directory and
see the first-run workflow.

## Install

If you downloaded a SER release, install the extension from its `.vsix` file:

1. Find `ser-<version>.vsix` in the release files. The Complete zip also has it
   in its `VS Code Extension` folder.
2. In VS Code, open **Extensions**, select the **...** menu, choose
   **Install from VSIX...**, and pick the file. You can also run
   `code --install-extension ser-<version>.vsix` in a terminal.
3. Reload VS Code if it asks.

## Build from source

This extension is maintained together with the main SER repository. The
extension, its Blocks editor and the standalone Blocks editor are all built
from the same SER language data that the plugin's build creates.

The editable source is in `src`. Files in `out` are generated from it and are
what VS Code loads, so do not edit them by hand.

From the repository's `Tooling` directory:

```powershell
npm install
npm run verify
```

Once these dependencies are installed, each SER build also refreshes the
extension files in this folder.

### Building does not install the extension

A normal SER build only refreshes the extension files in this folder. It does
not change the extension your VS Code uses.

To also copy the extension into VS Code during the build, add
`/p:InstallSerExtensionAfterBuild=true`:

```powershell
dotnet build SER.csproj -c Release --no-restore /p:InstallSerExtensionAfterBuild=true
```

This copies the extension to `%USERPROFILE%\.vscode\extensions\ser`. Reload
VS Code afterwards to use the new version.

To copy it somewhere else, such as a VS Code Insiders profile, also pass
`/p:SerExtensionInstallDirectory="C:\path\to\.vscode\extensions\ser"`. This
setting only changes the destination. On its own, without
`/p:InstallSerExtensionAfterBuild=true`, it copies nothing.
