# Make your server do something new

SER is a programming language, but you do not need to study a programming
language before you can enjoy it. Start with one method, turn it into a player
command or game event, and watch the server change. Learn variables and control
flow later, when a script you actually want to build needs them.

These docs therefore have two different reading paths.

## Tutorial: build first, learn as you need it

Follow this path in order if SER is new to you:

1. [Install SER and generate its examples](getting-started/installation.md)
2. [Make the server say something](getting-started/first-script.md)
3. [Discover methods that change the game](tutorial/methods.md)
4. [Make a command or react to an event](tutorial/flags.md)
5. [Choose which players are affected](tutorial/player-targets.md)
6. [Remember values and inspect players](tutorial/variables-and-properties.md)
7. [Add decisions, chance, and timing](tutorial/decisions-and-time.md)
8. [Build a Hot Potato event](tutorial/hot-potato.md)

You will make useful scripts before the tutorial introduces loops, collections,
functions, or memory lifetimes. That is intentional.

## Reference: find the exact rule

Use the reference when you need a complete or technical answer:

- [script files, identity, and reload behavior](getting-started/files-and-reloads.md);
- [methods, values, expressions, and return types](language/methods-and-values.md);
- [variable families and properties](language/variables-and-properties.md);
- [conditions and loops](language/conditions-and-loops.md);
- [collections](language/collections.md);
- [timing and yielding](language/timing-and-yielding.md);
- [functions, variable visibility and lifetime, and errors](language/functions-scopes-and-errors.md);
- [flags, events, and commands](guides/flags-events-and-commands.md);
- [optional integrations](guides/integrations.md);
- [debugging](guides/debugging.md).

The compact [language specification](../language_specification.md) is for
experienced authors and tool builders. The [validated example index](guides/examples.md)
points to complete scripts that compile during every SER build.

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

Keep the tutorial outcome-oriented and the reference precise. A tutorial page
should begin with something worth adding to a server, introduce only the
language features needed to build it, show the expected result, and then offer
one safe experiment. Put exhaustive behavior and implementation-shaped edge
cases in the reference instead of interrupting the first learning path.

When changing the language or user workflow:

1. update the relevant page here;
2. update a build-validated script in `Example Scripts` when appropriate;
3. update `language_specification.md` for grammar changes;
4. run `npm run verify` from `Tooling`;
5. build SER so all bundled examples compile.
