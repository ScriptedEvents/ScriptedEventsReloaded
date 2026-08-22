# Remember values and inspect players

You do not need a variable for every script. Use one when SER gives you a value
which you want to name, inspect, change, or reuse.

## A command that knows its player

Create `whoami.ser`:

```ser
!-- CustomCommand whoami
-- availableFor player
-- requireSender
-- description "Shows your current player information"

$name = @sender -> name
$role = @sender -> role
$health = @sender -> health

Reply "Name: {$name}"
Reply "Role: {$role}"
Reply "Health: {$health}"
```

The command already gave you `@sender`. The `->` operator asks that player for
a **property**. The result is stored so the reply lines can reuse it.

## Four prefixes, four kinds of value

The first character is part of every variable name:

| Prefix | Holds | Example |
|---|---|---|
| `$` | text, numbers, booleans, durations, enums, colors | `$health` |
| `@` | zero, one, or many players | `@target` |
| `*` | a game or plugin object such as an item, room, or door | `*room` |
| `&` | an ordered collection of values | `&messages` |

Create or replace a value with `=`:

```ser
$message = "Welcome"
$duration = 8s
@targets = @alivePlayers
```

## Let a method return something

Some methods calculate or find a value instead of only changing the game:

```ser
@target = Take @surfacePlayers 1
$targetCount = AmountOf @target
*room = GetRoomByName Lcz173
```

Ask a method what it returns:

```text
serhelp Take
serhelp AmountOf
serhelp GetRoomByName
```

The help tells you which prefix can hold the result.

## Put live values inside text

Braces evaluate a value or expression inside quoted text:

```ser
Reply "There are {AmountOf @all} connected players."
Broadcast @sender 5s "Hello {@sender -> name}!"
```

A plain variable already forms one complete method argument:

```ser
$message = "The round is starting."
Broadcast @all 5s $message
```

## Properties usually want one player

`@all -> name` is ambiguous because `@all` can contain many players. Select one
first or use a value which is guaranteed to identify one player:

```ser
@target = Take @alivePlayers 1
$name = @target -> name
```

There is still one problem: `@alivePlayers` might be empty, and a previously
selected player might disconnect before the property is read. Automatic cleanup
prevents a stale player from being used, but the next lesson still checks that
exactly one player remains before reading the property.

The reference has the full rules for [variable families and properties](../language/variables-and-properties.md).
It saves the unusual details about visibility and lifetime for when you need
them.

Next: [add decisions, chance, and timing](decisions-and-time.md).
