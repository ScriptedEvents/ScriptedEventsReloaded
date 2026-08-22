# Variables and properties

This page documents variable value families and property access. Variable
visibility is not conventional lexical scope; see
[functions, variable lifetimes, and errors](functions-scopes-and-errors.md) for
the exact local, global, function-argument, and `ephm` behavior.

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

## Player variable readiness

Every read of a player variable checks its entries against LabAPI's
`Player.ReadyList`. Entries which are not in that list are silently and
permanently removed before the value reaches a method, property, loop, or
argument. Ready dummy players remain; unauthenticated players, the host,
non-ready NPCs, and disconnected wrappers do not. An empty player value is
valid.

The check compares the current player wrapper itself, not only an account or
round ID. If somebody disconnects and reconnects, the new player is not restored
to an older stored variable. Pruning also does not add later joiners or update a
stored selection after role, team, or zone changes. Reassign a predefined player
group when a fresh snapshot is required. Predefined groups are generated from
the current ready-player list each time they are read.

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
