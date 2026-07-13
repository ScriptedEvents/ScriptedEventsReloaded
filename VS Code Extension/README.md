# Scripted Events Reloaded
Allow your SER scripts to get colorful!

Adds custom theme support for SER scripts. Works regardless of selected theme.

This extension is maintained together with the main SER repository. The plugin
build regenerates `ser_method_info.js` and copies it to this extension's `out`
folder. If the default VS Code installation is present, it is also copied to
`%USERPROFILE%\\.vscode\\extensions\\ser\\out`.

To use a different installed-extension path, pass
`/p:SerExtensionInstallDirectory="C:\\path\\to\\.vscode\\extensions\\ser"`
to MSBuild.
