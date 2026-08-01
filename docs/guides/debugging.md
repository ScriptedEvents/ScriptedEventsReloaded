# Debugging scripts

## Start with file status

```text
serstatus
```

It shows the active script directory and separates:

- accepted snapshots;
- failed candidates and their full paths;
- files disabled by a leading `#`;
- folders and their contents excluded by a leading `#`;
- globally conflicting base names.

Use `serstatus all` when a category was shortened.

## Understand reload behavior

For a new, unregistered utility file:

```text
serrun myScript
```

`serrun` performs a targeted search and reports where it looked if no matching
`.ser` or `.txt` file exists.

For edits, renames, deletions, enabled examples, or flag changes:

```text
serreload
```

The reload response includes exact failed files. A message that the previous
accepted version remains active means the disk candidate is invalid but live
bindings were rolled back safely.

## Inspect, do not guess

```text
serhelp methods
serhelp methods essential
serhelp Broadcast
serhelp events
serhelp Death
serhelp flags
serhelp CustomCommand
serhelp properties player
```

Mistyped specific topics suggest a close match.

## Print intermediate state

```ser
$count = AmountOf @alivePlayers
Print "Alive players: {$count}"
Print {LogVar @alivePlayers}
```

Place temporary prints before and after the suspected instruction.

## Reduce the reproduction

Create a uniquely named utility file containing only the failing operation:

```ser
@target = Take @alivePlayers 1
Print {LogVar @target}
```

If it works alone, inspect earlier state, event data, and overwritten variables
in the original script.

## Checklist

1. Is the method, flag, or event present in `serhelp`?
2. Does capitalization match?
3. Are text values containing spaces quoted?
4. Does every variable use the correct prefix?
5. Are arguments in the documented order?
6. Does a player property receive exactly one player?
7. Was a reference checked for invalidity?
8. Can an event variable be absent?
9. Does every statement have its required `end`?
10. Does every `forever` loop yield?
11. Is another file using the same base name?

When reporting an internal error, include its short identifier and the matching
server-console log.
