# Variables and properties

SER prefixes variable names with their value family:

| Prefix | Family | Typical contents |
|---|---|---|
| `$` | Literal | text, number, boolean, duration, enum, color |
| `@` | Player | zero, one, or many players |
| `*` | Reference | item, room, door, JSON object, game wrapper |
| `&` | Collection | a list of SER values |

Create or replace a local variable with `=`:

```ser
$message = "Welcome"
@target = Take @alivePlayers 1
&names = Coll.Create
```

Predefined variables such as `@all`, `@alivePlayers`, and `@scpPlayers` are
provided by SER. Event and command flags can inject additional local variables.
Run `serhelp variables` for the current predefined list.

## Text interpolation

Inside quoted text, wrap a variable or expression in braces:

```ser
$name = @target -> name
Print "Selected player: {$name}"
Print "Their role: {@target -> role}"
```

`~` escapes SER interpolation characters when literal braces are required.

## Properties

`->` reads a property:

```ser
$name = @target -> name
$role = @target -> role
*room = @target -> roomRef
$roomName = *room -> name
```

Player properties generally require exactly one player. Select one first with a
method such as `Take`.

Inspect properties with:

```text
serhelp properties
serhelp properties player
serhelp properties Door
```

## Reference safety

Game objects may become invalid between instructions. Check a reference before
using it:

```ser
if {*room -> isInvalid}
    Print "The room reference is no longer valid"
    stop
end
```

## Enums in different contexts

Methods accept bare enum values:

```ser
SetRole @target ClassD
```

Properties are compared as text:

```ser
if {@target -> role} is "ClassD"
    Print "The target is Class-D"
end
```

Next: [conditions and loops](conditions-and-loops.md).
