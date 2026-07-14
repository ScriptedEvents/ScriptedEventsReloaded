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

This extension is maintained together with the main SER repository. The plugin
build regenerates `ser_method_info.js` and copies it to this extension's `out`
folder. If the default VS Code installation is present, it is also copied to
`%USERPROFILE%\\.vscode\\extensions\\ser\\out`.

To use a different installed-extension path, pass
`/p:SerExtensionInstallDirectory="C:\\path\\to\\.vscode\\extensions\\ser"`
to MSBuild.
