# SER Developer Guide

This guide describes the current architecture and development workflow of Scripted Events Reloaded (SER). It is intended for contributors and release maintainers. The implementation remains the source of truth; user-facing script syntax is documented separately in `language_specification.md` and through the in-game `serhelp` command.

Last verified against the repository: 2026-07-13.

## 1. Project at a glance

SER is a .NET Framework 4.8 plugin that compiles and executes `.ser`/`.txt` scripts inside an SCP:SL server. The same project produces two distributions:

| Configuration | Plugin host | Assembly | Output directory |
| --- | --- | --- | --- |
| `Release` | LabAPI | `SER.dll` | `bin/LABAPI/net48/` |
| `EXILED` | EXILED | `SER-Exiled.dll` | `bin/EXILED/net48/` |
| `Full Debug` | LabAPI, `DEBUG` and `SIGNAL` enabled | `SER.dll` | `bin/LABAPI/net48/` |
| `Partial Debug` | LabAPI, `SIGNAL` enabled | `SER.dll` | `bin/LABAPI/net48/` |

The repository currently has no standalone unit-test project. The build does contain an important integration check: after compiling the DLL, MSBuild loads it, indexes its methods, compiles every embedded example script, and initializes several registries. This catches many parser and registration regressions, but it does not simulate a running game server.

## 2. Repository map

| Path | Responsibility |
| --- | --- |
| `Code/Plugin` | Plugin entry point, configuration, commands, framework lifecycle |
| `Code/FileSystem` | Script discovery, example generation, databases |
| `Code/ScriptSystem` | Script construction, compilation, execution, executors |
| `Code/TokenSystem` | Line splitting, slices, tokens, lexical precedence |
| `Code/ContextSystem` | Context tree construction and executable statements |
| `Code/ArgumentSystem` | Method argument definitions, conversion, typed access |
| `Code/MethodSystem` | Method registry, base classes, built-in script methods |
| `Code/FlagSystem` | Script metadata flags and their bindings |
| `Code/EventSystem` | Dynamic LabAPI event discovery and script event handlers |
| `Code/VariableSystem` | Local/global variables and predefined player variables |
| `Code/ValueSystem` | Literal, player, collection and reference values; properties |
| `Code/Builders` | Generated help metadata and the Blockly visual editor |
| `Example Scripts` | Scripts embedded into the plugin and compiled during every build |
| `language_specification.md` | LLM-oriented/user-facing language reference |

Important root files:

- `SER.csproj` defines all configurations, dependencies, embedded resources, post-build copying and build-time validation.
- `App.config` contains .NET Framework binding redirects.
- `global.json` selects the .NET SDK used by command-line builds.
- `SER Visual Editor.html` and `ser_method_info.js` are generated and intentionally ignored by Git.
- `packages.config` is legacy metadata; `PackageReference` entries in `SER.csproj` control current NuGet restore.

## 3. Local development and build

### Prerequisites

1. A Windows development environment with an SDK capable of building `net48` and the .NET Framework 4.8 reference assemblies.
2. `SL_DEV_REFERENCES` set to a directory containing the SCP:SL/LabAPI/Unity DLLs referenced with explicit `HintPath` entries in `SER.csproj`.
3. Optionally, `LABAPI_PLUGINS` set to a LabAPI plugin directory for automatic local deployment.

Check the environment before building:

```powershell
dotnet --info
$env:SL_DEV_REFERENCES
$env:LABAPI_PLUGINS
```

`dotnet --info` must report `global.json` as valid. Use an SDK feature-band version such as `10.0.100`, not a runtime-style version such as `10.0.0`.

### Build commands

```powershell
dotnet restore SER.sln
dotnet build SER.sln -c Release --no-restore
dotnet build SER.sln -c EXILED --no-restore
```

For a complete pre-release check, also compile `Full Debug` and `Partial Debug`.

Build side effects matter:

- A non-EXILED build copies the resulting assembly to `LABAPI_PLUGINS` when that variable is set.
- Every build copies the assembly to `SL_DEV_REFERENCES` when that variable is set.
- The `RunValidation` target loads the newly built DLL and compiles all embedded example scripts.
- Validation calls `Builder.CreateFiles()`, which regenerates `SER Visual Editor.html` and `ser_method_info.js` in the working directory.

Do not interpret “build succeeded” as full server compatibility. Always smoke-test both release assemblies against the exact SCP:SL, LabAPI and EXILED versions intended for release.

## 4. Plugin and round lifecycle

`Code/Plugin/MainPlugin.cs` is the entry point for both hosts. Conditional compilation changes only the plugin base class and lifecycle method names.

On plugin enable, SER:

1. Stores `MainPlugin.Instance`.
2. Starts `FrameworkBridge` discovery for optional integrations.
3. Subscribes to map, server and player events.
4. Registers custom Tesla and damage event handlers.

On `MapGenerating`, SER resets round-scoped state, then initializes systems in this order:

1. Stop running scripts and clear registered script flags.
2. Clear player data, Tesla rules, damage rules and custom roles.
3. Initialize the reference-property registry.
4. Register flags.
5. Discover LabAPI events.
6. Index built-in methods.
7. Create predefined global variables.
8. Initialize command-context capture.
9. Refresh the script catalog, compile changed files and bind their flags.

This order is significant. Script flag parsing requires methods, variables and events to be available. Any new registry that scripts depend on should be initialized before `FileSystem.Initialize()`.

Lifecycle rule: every event subscription, command registration, custom handler and coroutine created during enable/initialization must have a matching cleanup path. Test repeated map generation as well as plugin disable/re-enable; a clean first boot alone does not expose duplicate subscriptions.

## 5. Script discovery and flag registration

`FileSystem.UpdateScriptPathCollection()` recursively scans the SER config directory for `.ser` and `.txt` files.

- A file whose base filename starts with `#` is ignored.
- Physical-file identity is the filename without its extension, not its relative path. Runtime sections add a numbered selector when needed.
- If two files anywhere in the tree share the same base filename, all scripts with that duplicate name are excluded and an error is logged.
- A watcher queues filesystem changes onto SER's main-thread coroutine; script lookups also check the catalog so manual execution cannot miss an edit.

`ScriptCatalog` owns the accepted source snapshot and registered flags for each physical file. On an edit, manual execution, forced reload or round restart, SER splits the file at every `!--` declaration and compiles every section before changing live bindings. Each declaration is inclusive in its section, and the section ends immediately before the next declaration. Multi-section files receive selectors such as `file:1` and `file:2`; flagless and single-section files retain their bare filename. Only blank lines and comments may precede the first declaration in a flagged file.

Flag parsing is side-effect-free. After every section compiles and every flag parses, the catalog unbinds the accepted snapshot and binds the candidate. A registration failure rolls the candidate back and restores the last known-good snapshot. Invalid edits therefore do not partially replace handlers. Watcher refreshes use the configurable `AutomaticScriptReloadDelay` quiet period and cache failed file stamps so an unchanged draft does not flood the console. Every successful file reload emits a server info log. `serreload` forces the same pipeline instead of maintaining a separate registration path. `ScriptFlagHandler.Clear()` calls `Unbind()` on every registered flag before clearing the registry.

Bindings store the section selector rather than only the physical filename, so callbacks reload and execute the correct slice. Section compilation retains original source-file line numbers. A bare multi-section filename is deliberately ambiguous for manual execution; use its numbered selector. File-level stop and running checks match all of that file's sections.

## 6. Compilation pipeline

`Script.Compile()` runs three stages after defining numbered lines:

```text
raw content
  -> Line[]
  -> Slice[] per line
  -> BaseToken[] per line
  -> RunnableContext[] tree
```

### 6.1 Slicing

`Tokenizer.SliceLine` groups raw characters into:

- `SingleSlice` for ordinary ungrouped text;
- `CollectionSlice` for quoted text and grouped expressions such as `(...)` and `{...}`.

The slice validates its own closing delimiter before tokenization.

### 6.2 Tokenization

`Tokenizer` tries token classes in explicit precedence order. There are separate lists for single and collection slices:

- `OrderedImportanceTokensFromSingleSlices`
- `OrderedImportanceTokensFromCollectionSlices`

The first token whose `TryInit` succeeds wins. Unknown input falls back to a plain `BaseToken`. Precedence is therefore part of the language grammar; adding or moving a token can change how existing scripts parse.

### 6.3 Contexting

`Contexter` turns tokenized lines into executable contexts. A stack tracks nested `StatementContext` instances. Block starters are pushed, children are attached to the current statement, and `end` pops the current block. Extenders such as `else`, `elif` and `onerror` replace/extend the active statement according to their interfaces and signals.

Compilation returns `Result`/`TryGet<T>` errors with script line context. It should not depend on a live player or round unless a specific token/argument is explicitly dynamic.

## 7. Runtime model

`Script.RunForEvent` checks flag approval, injects any pre-execution state, adds the script to the running set and executes its root contexts through MEC coroutines.

Contexts fall into two main runtime categories:

- `StandardContext` executes synchronously.
- `YieldingContext` returns `IEnumerator<float>` and may pause execution.

Methods follow the same split through `SynchronousMethod` and `YieldingMethod`, with returning variants for each. `BetterCoros` owns common coroutine error handling and observes a script's killed state. `SafeScripts` introduces frame yields around method execution, but it is not a substitute for explicit waits in intentionally long or infinite script loops.

`Script.MarkAsStopped()` removes a script from the running registry and sets `Killed`. New runtime work must remain attached to the script-aware coroutine path when cancellation and script-level error reporting are expected; detached coroutines can outlive their originating script.

## 8. Values and variables

The value hierarchy bridges script syntax with CLR and game objects:

| Prefix | Variable/value family | Examples |
| --- | --- | --- |
| `$` | literal | text, number, boolean, duration-like data |
| `@` | player | one or more LabAPI players |
| `&` | collection | heterogeneous SER values |
| `*` | reference | wrapped game/API objects |

`VariableIndex` stores global variables and recreates the built-in player collections on every map. Each `Script` stores local variables keyed by `(prefix, name)`. Variables with the same name but different prefixes are distinct.

Properties are accessed with `->`. Property metadata is represented by `PropInfo`; built-in value properties live with value types, while external/game object properties are registered by `ReferencePropertyRegistry`. `ValueExpressionContext` resolves chained property access and returning expressions.

When exposing a new CLR object to scripts, decide whether it should become a literal, player, collection or reference value. Do not serialize live game wrappers directly into databases.

## 9. Methods and arguments

### Method discovery and naming

`MethodIndex` reflects over non-abstract `Method` subclasses. A class name must end in `Method`; underscores become dots in the script name. For example, `HTTP_GetMethod` is exposed as `HTTP.Get`. The namespace segment containing the class supplies its help/editor subgroup.

Framework-dependent methods implement `IDependOnFramework`. They are held separately until `FrameworkBridge` detects the required integration.

### Adding a method

1. Put the class in the appropriate `Code/MethodSystem/Methods/<Group>Methods` namespace.
2. Derive from the correct synchronous/yielding and returning/non-returning base.
3. Name the class with the required `Method` suffix.
4. Provide `Description` and `ExpectedArguments`.
5. Implement `Execute()`.
6. Add `IAdditionalDescription`, `ICanError`, `IHasAliases` or `IDependOnFramework` when applicable.
7. Add or update an example script that exercises the new behavior.
8. Build both release configurations and inspect generated help/editor metadata.

`[UsedImplicitly]` is recommended on reflection-created classes to keep IDE/static analysis accurate. It is not what performs runtime registration; inheritance and `MethodIndex` reflection do that.

### Adding an argument

1. Derive from `Argument` (or the closest specialized base).
2. Implement a public instance `GetConvertSolution(BaseToken)` method returning `DynamicTryGet<T>`.
3. Set `MustBeProvided`, `DefaultValue`, `ConsumesRemainingValues` and descriptions deliberately.
4. Add a typed accessor in `ProvidedArguments` if method implementations need a new convenience getter.
5. Test static values, dynamic values, `_` defaults, missing arguments, extra arguments and consuming-the-rest behavior.

`MethodArgumentDispatcher` discovers and compiles the converter delegate by reflection, then caches it by argument type.

## 10. Adding tokens, contexts, values and flags

### Token or keyword

- Implement the correct token interface/base.
- Add ordinary token types to the appropriate ordered list in `Tokenizer`.
- For keywords, ensure `ContextableKeywordToken` can discover the keyword context.
- Test precedence against booleans, methods, numbers, colors, variables, durations and wildcard tokens.
- Test invalid delimiters and an empty/whitespace-only line.

### Context

- Choose `StandardContext`, `YieldingContext` or `StatementContext` based on runtime behavior.
- Implement `TryAddToken` and `VerifyCurrentState` so incomplete syntax fails during compilation.
- Use statement interfaces/signals for block extensions and parent-control messages for `break`, `continue`, `return` and `stop` semantics.
- Test nesting, a missing `end`, an extra `end`, and every supported extender.

### Value/property

- Implement parsing/string representation without assuming a live game object for static compilation.
- Register reference properties in `ReferencePropertyRegistry`.
- Declare accurate `TypeOfValue` information so argument and return-type validation works.
- Ensure null/invalid references produce a script error rather than a raw CLR exception.

### Flag

- Derive from `Flag`; implement inline/secondary arguments, `Bind()` and `Unbind()`. The default `Bind()` invokes the legacy `OnParsingComplete()` hook.
- Implement `IMajorBehaviorFlag` when the flag owns primary script execution behavior.
- Keep argument parsing free of registration side effects. Make `Bind()` and `Unbind()` symmetric, repeatable and safe during rollback.
- External plugins can use `Flag.RegisterFlagsAsExternalPlugin()`; external methods can use `MethodIndex.AddAllDefinedMethodsInAssembly()`.

## 11. Error handling

Expected validation failures use `Result`, `TryGet<T>` or `DynamicTryGet`, not ordinary exceptions.

- `true` converts to a successful `Result`.
- A non-empty string converts to an error `Result`.
- `Result + Result` adds context to an existing error.
- `Result.Merge` combines independent compile errors.

Script runtime failures use `ScriptRuntimeError`/`CustomScriptRuntimeError` and should run through the script-aware coroutine wrapper so they can be logged against the script and line. Exceptions whose names indicate a developer error represent violated engine invariants; they are not appropriate for invalid user input.

Avoid broad `catch` blocks that discard the original exception. If recovery is possible, preserve enough context to identify the script, method, line and external operation.

## 12. Events and optional frameworks

`EventHandler.Initialize()` discovers static LabAPI handler events by reflection. A script using `!-- OnEvent EventName` causes SER to bind that event lazily. Event properties are converted into local variables named `ev<PropertyName>` with a prefix inferred from the value type. Cancellable events can propagate a script's allow/deny result.

`DisableEvent` and `EnableEvent` only accept events whose argument type implements LabAPI's `ICancellableEvent`. Both are returning methods: they return `true` when the disabled state changed and `false` when it was already in the requested state. A missing or non-cancellable event is a script runtime error.

`sermethod` detects a single synchronous `ReturningMethod` and includes its formatted value in the command response. Ordinary and yielding methods keep the normal coroutine execution path so a command cannot block the server thread.

`EventHandler.Clear()` executes stored unsubscribe actions and clears handler state. Any additional event binding mechanism should follow this pattern.

`FrameworkBridge` probes for EXILED, Callvote and UncomplicatedCustomRoles. Methods implementing `IDependOnFramework` are unavailable until their framework is detected and loaded. Release testing should cover both absence and presence of every optional framework represented in the shipped method set.

## 13. File I/O and security boundaries

Scripts can reach databases, custom YAML configs, HTTP endpoints, Discord webhooks and audio/archive code. Treat all script-provided names, paths, URLs and serialized content as untrusted input even when scripts are normally written by server administrators.

- Resolve file paths and verify that the final full path remains inside the intended SER subdirectory.
- Reject rooted paths and `..` traversal.
- Database JSON uses a restricted serialization binder; keep its explicit type allowlist narrow when adding new persisted values.
- Persist game structs through stable SER representations rather than serializing wrapper objects directly. Colors are stored as canonical hex strings and parsed back into `ColorValue` instances.
- Put timeouts and response-size limits on network operations.
- Keep asynchronous operations attached to script cancellation/error handling.
- Audit embedded/transitive dependencies, because selected DLLs are packed into the plugin assembly.

## 14. Generated artifacts and embedded resources

`Builder.CreateFiles()` produces:

- `ser_method_info.js`, a JSON-like truth table of methods and arguments;
- `SER Visual Editor.html`, a Blockly-based visual script editor.

The build embeds example scripts and selected dependencies (`NCalc`, `Newtonsoft.Json`, `AudioPlayerApi`, `NVorbis`, `SharpCompress`) into the plugin assembly. It also attempts to embed SER and API XML documentation used by help generation.

Before publishing, inspect the final DLL's manifest resources and confirm that required examples, dependency DLLs and XML files are present. Test from a clean checkout/build directory so stale XML or generated output cannot mask packaging problems.

## 15. Pre-release checklist

### Automated/local

- [ ] `git status --short` contains only intended source/documentation changes.
- [ ] `dotnet --info` accepts `global.json` and selects the intended SDK.
- [ ] Restore succeeds from a clean NuGet cache or CI agent.
- [ ] `Release`, `EXILED`, `Full Debug` and `Partial Debug` all build with zero warnings.
- [ ] Build-time validation compiles every embedded example.
- [ ] `dotnet list SER.csproj package --vulnerable --include-transitive` reports no accepted-unreviewed vulnerability.
- [ ] Final LabAPI and EXILED assemblies contain the intended embedded resources.
- [ ] Version, changelog/release notes, README, language specification, examples and in-game help agree.

### Clean-server smoke test

- [ ] Enable and disable the plugin without retained event handlers or coroutines.
- [ ] Generate at least two maps and verify callbacks/commands execute exactly once.
- [ ] Load valid scripts, reject malformed scripts and reject duplicate filenames cleanly.
- [ ] Edit single- and multi-section files and verify one info log per successful reload.
- [ ] Leave a syntax error in an edited file and verify its last known-good event/command bindings stay active.
- [ ] Run event, command, function, loop, callback and returning-method examples.
- [ ] Stop/reload scripts while they are waiting or performing network work.
- [ ] Exercise cancellable event disable/enable behavior and verify returned values.
- [ ] Create/read/update databases and configs, including malicious path inputs.
- [ ] Test HTTP/Discord failures, timeouts and invalid JSON.
- [ ] Test with no optional frameworks, then with each supported framework combination.
- [ ] Repeat the matrix for both LabAPI and EXILED artifacts.

## 16. Current validation limits

The build-time example validator proves that the assembly can be loaded, core method registration succeeds, and shipped examples compile. It does not currently prove:

- plugin enable/disable symmetry;
- behavior across repeated maps or hot reloads;
- real LabAPI/EXILED event compatibility;
- correctness of return values and game mutations;
- coroutine cancellation and asynchronous exception handling;
- filesystem containment, network limits or serialization safety;
- generated editor behavior in a browser.

Changes in these areas require targeted tests or a clean-server smoke test even when every build is green.
