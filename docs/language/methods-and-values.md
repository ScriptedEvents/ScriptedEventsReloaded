# Methods and values

Methods perform actions or calculate values. Their names use `PascalCase` or a
namespace-like dot:

```ser
Print "Server started"
Broadcast @all 5s "Welcome!"
$count = AmountOf @all
*date = Date.Current
```

Inspect the current server rather than guessing:

```text
serhelp methods
serhelp methods essential
serhelp methods Player
serhelp methods all
serhelp Broadcast
```

The specific method page describes argument order, accepted types, optional
defaults, return types, and defined errors.

## Common values

| Kind | Examples |
|---|---|
| Text | `"hello"`, `"Player: {@sender -> name}"` |
| Number | `10`, `3.5`, `25%` |
| Duration | `500ms`, `5s`, `2m`, `1h` |
| Boolean | `true`, `false` |
| Enum | `ClassD`, `Coin`, `LczCheckpointA` |
| Color | `#ff8800` |

Text containing spaces must be quoted. Use `<br>` for a line break displayed
in SCP:SL text.

## Numbers and durations

Expressions support `+`, `-`, `*`, `/`, and `%`. Parentheses make evaluation
order explicit:

```ser
$damage = 20
$remaining = 100 - ($damage * 2)
$healAmount = Round ((100 + 50) * 25%)
Print "Remaining: {$remaining}; heal: {$healAmount}"
```

Duration suffixes such as `ms`, `s`, `m`, and `h` are used when the number is
known in the script. Convert a calculated number with `ToDuration`:

```ser
$seconds = Random 10 30
$delay = ToDuration $seconds seconds
wait $delay
```

Use `# ` at the beginning of a line for a comment. Comments should explain the
reason for an instruction or another non-obvious decision.

## Return values

A returning method can be stored:

```ser
$aliveCount = AmountOf @alivePlayers
@target = Take @alivePlayers 1
*room = GetRoomByName Lcz173
```

The variable prefix must be able to hold the returned type. `serhelp MethodName`
states which type is returned.

## Inline expressions

Use braces when a method call or property chain is passed inside another
instruction:

```ser
Print "Alive: {AmountOf @alivePlayers}"
Broadcast @all 5s "Room: {@sender -> roomRef -> name}"
if {AmountOf @alivePlayers} > 0
    Print "The round has players"
end
```

A standalone prefixed variable is already one complete value:

```ser
Reply $message
if $enabled is true
    Print $message
end
```

Use `_` to retain an optional argument's default when a later argument is
needed. Use `*` as a wildcard only where the selected method documents it.

Next: [variables and properties](variables-and-properties.md).
