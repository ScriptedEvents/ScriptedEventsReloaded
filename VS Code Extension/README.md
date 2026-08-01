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

This extension is maintained together with the main SER repository. SER's build
regenerates the reflection-backed language manifest. The shared tooling build
then produces the extension, visual-editor webview and standalone editor from
that same manifest and shared language core.

The maintainable extension source is in `src`. Files in `out` are synchronized
build artifacts used by VS Code.

From the repository's `Tooling` directory:

```powershell
npm install
npm run verify
```

If the tooling dependencies are installed, the normal SER build runs this
synchronization automatically before copying the extension to
`%USERPROFILE%\\.vscode\\extensions\\ser`.

To use a different installed-extension path, pass
`/p:SerExtensionInstallDirectory="C:\\path\\to\\.vscode\\extensions\\ser"`
to MSBuild.
