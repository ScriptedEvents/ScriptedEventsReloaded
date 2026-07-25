# Scripted Events Reloaded
Allow your SER scripts to get colorful!

Adds custom theme support for SER scripts. Works regardless of selected theme.

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
