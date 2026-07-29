# Timing and yielding

Yielding pauses only the current script. It does not freeze the server or other
SER scripts.

## Wait for a duration

```ser
Print "Starting"
wait 500ms
Print "Half a second passed"
wait 2s
Print "Two more seconds passed"
```

Known durations may use `ms`, `s`, `m`, or `h`. For a duration calculated at
runtime, use `ToDuration`.

## Wait for a condition

`wait_until` reevaluates a condition until it becomes true:

```ser
Print "Waiting for every SCP to leave play"
wait_until {AmountOf @scpPlayers} is 0
Print "No living SCPs remain"
```

Only use a condition that can eventually change. Event data and local
references may become invalid while a script is paused, so validate them again
after a long wait when necessary.

## Yielding methods

Some integration methods yield until an asynchronous operation finishes. Their
help describes that behavior. Do not assume that a method yields from its name;
inspect it with:

```text
serhelp MethodName
```

Every `forever` loop must reach a yielding instruction on every repeating path.
A branch that repeatedly uses `continue` before the only `wait` can still
monopolize the server thread.

Next: [commands and events](../guides/flags-events-and-commands.md).
