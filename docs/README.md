# SER 1.0 documentation

This directory is the canonical user documentation for the in-development
Scripted Events Reloaded 1.0 release. It lives beside the implementation so a
change to syntax, commands, examples, or tooling can update the matching
tutorial in the same pull request.

Start here:

1. [Install SER](getting-started/installation.md)
2. [Create and run your first script](getting-started/first-script.md)
3. [Understand files, names, and reloads](getting-started/files-and-reloads.md)
4. [Learn methods and values](language/methods-and-values.md)
5. [Learn variables and properties](language/variables-and-properties.md)
6. [Add conditions and loops](language/conditions-and-loops.md)
7. [Work with collections](language/collections.md)
8. [Pause and resume execution safely](language/timing-and-yielding.md)
9. [Connect scripts to commands and events](guides/flags-events-and-commands.md)
10. [Organize larger scripts](language/functions-scopes-and-errors.md)
11. [Debug a script](guides/debugging.md)

For complete examples that are compiled during every build, see the
[example index](guides/examples.md). Experienced authors can use the compact
[language specification](../language_specification.md).

## Documentation boundaries

- The implementation and build-generated `ser_method_info.js` are the source of
  truth for available methods, flags, events, variables, and properties.
- `serhelp` exposes that information on the running server. It should be used
  instead of copied snapshots of hundreds of symbols.
- SER Blocks teaches a deliberately small beginner vocabulary. The VS Code
  extension and text format expose the complete language.
- The old German and Italian GitBook pages have not been migrated because they
  describe older syntax. See [translation policy](translations.md).

## Contributing

Keep tutorials task-oriented. A beginner should know what a command changes,
what they should see after running it, and which diagnostic command to use when
the result differs.

When changing the language or user workflow:

1. update the relevant page here;
2. update a build-validated script in `Example Scripts` when appropriate;
3. update `language_specification.md` for grammar changes;
4. run `npm run verify` from `Tooling`;
5. build SER so all bundled examples compile.
