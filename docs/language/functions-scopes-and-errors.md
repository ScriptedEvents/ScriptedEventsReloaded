# Functions, scopes, and errors

## Inline functions

Define a function before it is used:

```ser
func $Add with $left $right
    return $left + $right
end

$sum = run $Add 2 3
Print "Sum: {$sum}"
```

The function name's prefix declares its return family. A function without a
prefix returns no value:

```ser
func Greet with $name
    Print "Hello {$name}"
end

run Greet "Scientist"
```

Inline `func`/`run` is different from the legacy `!-- Function` flag and
`RunFunc` method used for cross-file compatibility.

## Memory scopes

Assignments create local variables by default. They disappear when the script
finishes.

Use `global` for round-wide state shared by scripts:

```ser
global $roundScore = 0
global $roundScore = $roundScore + 1
```

Global variables last for the current round. Use distinctive names to avoid
collisions between unrelated scripts.

Use `ephm` for a variable that should disappear when its current block or
function ends:

```ser
if {AmountOf @alivePlayers} > 0
    ephm @target = Take @alivePlayers 1
    Print {@target -> name}
end
```

Use `delete` when a variable must be removed explicitly.

## Prevent errors first

Before handling an exception:

- require event data with `-- require`;
- validate optional variables with `VarExists`;
- check a reference's `isInvalid` property;
- inspect collection length before fetching an index.

## Handle a recoverable error

```ser
attempt
    &items = Coll.Create
    Print {Coll.Fetch &items 2}
on_error with $message $type
    Print "Could not read the item: {$message}"
    Print "Error type: {$type}"
end
```

Keep `attempt` blocks small so it is obvious which instruction failed.
Unexpected internal SER failures are presented with a short identifier; full
technical details remain in the server console.

Next: [debugging](../guides/debugging.md).
