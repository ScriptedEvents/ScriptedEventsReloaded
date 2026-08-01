# SER Tooling

This directory contains the shared, runtime-independent tooling used by both:

- the standalone SER Blocks beginner editor;
- the VS Code extension and its visual-editor webview.

The SER plugin remains the source of truth. Its build writes
`ser_method_info.js`; `npm run build` normalizes that manifest and produces both
client artifacts.

## Commands

Run these from `Tooling`:

```powershell
npm run build
npm run verify
```

The build:

1. reads the generated SER manifest;
2. builds the standalone `SER Visual Editor.html`;
3. copies the same editor into the VS Code extension;
4. copies the shared language core and extension source into `out`;
5. copies the repository license and third-party notices into the extension.

`npm run verify` also checks generated-file synchronization, all 16 local
documentation files, JavaScript behavior, third-party notice coverage, and
whether C# method implementations read every argument they declare (without
reading undeclared names).

The tooling has no runtime dependency on the game server or SER internals.

## Visual-editor scope

SER Blocks is a teaching surface, not a complete visual API browser. Its
maintained block vocabulary covers:

- event, command and manual starting points;
- common player targets;
- messages, healing, damage, items, roles, teleporting, and inventory;
- one-level decisions, random chance, short repeats, and waits.

Recipes demonstrate useful complete scripts, the side panel explains the
selected block in plain language, and the generated SER remains visible. New
SER methods stay available to the VS Code text editor without appearing in SER
Blocks unless they support a clear beginner task.
