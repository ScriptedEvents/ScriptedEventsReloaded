### Added

- Versioned `docs/` tutorial for SER 1.0, replacing the old GitBook as the
  canonical beginner documentation.
- Multi-section `.ser` and `.txt` files. Several event handlers and custom commands can now live in one file; sections are addressable as `filename:1`, `filename:2`, and so on.
  ```ser
  !-- CustomCommand test
  Reply "test successful!"

  !-- OnEvent RoundStarted
  Broadcast @all 5s "round has started!"
  ```
- Transactional script reloads through the permission-protected `serreload` command. SER compiles and validates a complete changed file before replacing its handlers, keeps the last known-good version after a failed reload, and restores accepted scripts across map changes without rereading edited files.
- SER Blocks, an offline beginner editor, plus a synchronized VS Code extension with:
  - method, keyword, flag, event, variable, enum, and option completions;
  - hover documentation and signature help;
  - shared diagnostics for incomplete values, malformed function calls, unclosed statements, and unsafe `forever` loops;
  - the `SER: Open Blocks Editor` command.
- ProjectMER scripting support for loading, unloading, reading, saving, and merging maps; creating, finding, moving, renaming, refreshing, and deleting map objects; and spawning, inspecting, animating, hiding, and destroying schematics.
- ProjectMER schematic events are available when the ProjectMER plugin is loaded.
- The `requireSender` custom-command option, which validates that player commands receive the `@sender` variable before the script runs.
- Richer `serhelp` output, including essential-method filtering, event and enum documentation, event-variable descriptions, framework availability, and property inspection for returned values and running-script variables.
- `sermethod` now reports the result of a single synchronous returning method.
- Safer database and file handling: path traversal is rejected, database deserialization uses an allowlist, failed saves roll back in-memory changes, and colors round-trip through a stable SER representation.
- New and refreshed example scripts covering multi-section handlers, player commands, databases, map/object status, custom roles, rewards, infections, and common server utilities.
- Automatic discovery of a newly added `.ser` or `.txt` file when `serrun name`
  cannot find a registered script.
- `serstatus`/`serlist`, which reports accepted snapshots, failed candidates,
  disabled files, and every path involved in a global-name conflict.

### Changed and fixed

- Optional framework discovery and reference wrapping were reorganized so unavailable framework methods stay out of the active method set while still being documented.
- Event and command bindings now have explicit bind/unbind paths, improving reload rollback and plugin lifecycle cleanup.
- Framework load order and disabled initialization were corrected, including bridges that previously failed when the startup message was disabled.
- AudioPlayerApi packaging was updated to version 1.1.3, and float/integer method argument descriptions were corrected.
- Build validation no longer locks the generated plugin assembly, and the build can regenerate and export the VS Code extension and standalone editor from one language manifest.
- First-run guidance is now available through `serhelp start`; method help opens
  with a compact category index and suggests close matches for mistyped topics.
- Startup output, configuration descriptions, reload errors, generated-example
  guidance, and internal-error reporting were rewritten for server operators.
- `.ser` remains the preferred format and `.txt` the compatibility format; both
  use the same globally unique script basename.
