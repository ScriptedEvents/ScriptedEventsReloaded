# SCRIPTED EVENTS RELOADED (SER) - Language Specification v1.0.0

## Core Concepts

SER is a scripting language for SCP: Secret Laboratory that allows you to automate game events and create custom commands. Every script is fundamentally a sequence of **methods** (commands) that tell the game what to do.

### The Four Data Types

Every value in SER belongs to one of four categories. When using a **variable**, you must use its specific prefix so the engine knows the data type.

**Literal (`$`)** - Numbers, text, time, booleans, colors, enums
- Examples: `10`, `5s`, `"Scientist"`, `true`, `ff00ffA5`
> `EnumValue` is a subclass of `TextValue` - enums are just text values. User cannot create an `EnumValue`, it only contains metadata about the enum type.

**Player (`@`)** - Array of players
- Examples: `@sender`, `@all`, `@evAttacker`
> Player value acts like an array of players, but if it only has one player, its behavior changes as if it were a singular player object, allowing for property retrieval and more.

**Reference (`*`)** - Game objects like rooms or items
- Examples: `*spawnRoom`, `*evRoom`

**Collection (`&`)** - A list of multiple things
- Examples: `&inventory`, `&rooms`

---

## Methods

Methods are commands that perform actions. Write the method name in `PascalCase`, followed by its arguments.

```
MethodName Arg1 Arg2
Broadcast @all 5s "Hello, world!"
Kill @classDPlayers
GiveItem @sender KeycardO5
```

### Assigning Variables from Methods

You can store method return values in variables:

```
$name = ServerInfo name
@newPlrs = LimitPlayers @all @classDPlayers
```

### The Wildcard (`*`)

Use `*` to specify ALL of something:

```
# kills all players
Kill *

# closes all doors
CloseDoor *
```

### Omitting Optional Arguments

Use `_` to skip optional arguments:

```
*embed = DiscordEmbed "Title" _ _ "Author"
```

---

## Properties and the Arrow Operator (`->`)

Properties allow you to access internal data of values using the `->` operator. This works with:
- Player variables: `@plr -> name`, `@plr -> role`, `@plr -> health`
- Reference variables: `*item -> type`, `*room -> name`
- Collection variables: `&coll -> length`, `&coll -> first`
- Literal values: `$text -> length`, `$num -> abs`

### Player Value Property Access

To access properties of a player value, there MUST ONLY be ONE PLAYER!
There is no "length" property of player value, use `AmountOf` method.

### Chaining Properties

You can chain property accesses together:

```
$nameLengthOdd = @sender -> name -> length -> isOdd
```

### Enum Values vs String Representations

When using enum values in methods vs properties, note the type difference:

**In methods** - Use bare enum tokens (unquoted):
```
SetRole @plr ClassD
```

This is still allowed:
```
SetRole @plr "ClassD"

$role = "ClassD"
SetRole @plr $role
```

**In properties/conditions** - Enums are converted to strings (quoted):
```
if {@plr -> role} is "ClassD"
    ...
end
```

This difference exists because methods can parse bare tokens as enum values, while expressions require quoted strings for safety and clarity. 
The enum is automatically converted to its string representation when accessed as a property.

### Reference Validity

References are pointers to game objects. A reference can become **invalid** (null) if:
- The object no longer exists (e.g., a room was destroyed)
- The player is in a state where the reference doesn't apply (e.g., `@plr -> roomRef` is null when the player is a spectator)
- The object was deleted during script execution

Always validate references before using them:

```
*room = @plr -> roomRef

if {ValidRef *room} is false
    Print "Player is not in a room"
    stop
end

# Safe to use *room now
CloseDoor *room
```

The `ValidRef` method checks if a reference is null. If it returns false, the reference cannot be used.


### Using Properties in Different Contexts

**In variable definitions** - No brackets needed:
```
$name = @plr -> name
$health = @plr -> health
```

**In conditions** - Use brackets:
```
if {@plr -> role} is "ClassD"
    ...
end
```

**In text interpolation** - Use brackets:
```
Print "Player: {@plr -> name}"
```

---

## Variables

### Variable Naming

A variable's name must always include its type prefix:
- `$name` (Literal)
- `@name` (Player)
- `*name` (Reference)
- `&name` (Collection)

### Variable Assignment

Create or update variables using `=`:

```
# static value
$kills = 0

# variable
@target = @sender

# math 
$kills = $kills + 1

# method return value
$name = ServerInfo name

# function return value
&items = run &GetItems @plr true
```

### Default Values

```
$name = ""  # empty text
$num = 0  # zero
$bool = false  # false
@plrs = @empty  # empty player array
&coll = EmptyCollection  # empty collection
```

### Memory Scopes

Variables live in different "buckets" that determine their lifespan:

**Local** - Deleted when the script finishes. Accessible anywhere in the script.
```
$var = 10
```

**Global** - Stays in memory for the entire round. Accessible by other scripts.
```
global $score = 100
```

To read a global variable later, just use its name (no `global` keyword):
```
Broadcast @all 5s "Score: {$score}"
```

To change the value of a global variable, you MUST use `global`:
```
global $number = 1
global $number = $number * 2
```
Not using `global` when changing value makes LOCAL variable with the same name, causing NAME COLLISION error.

**Ephemeral** - Only exists inside a specific loop or function (defined with `with`).
Preferred way:
```
over @all with @plr
    # @plr only exists here
end
```

Other valid way: (carried over from previous versions)
```
over @all 
    with @plr

    # @plr only exists here
end
```

### Important: Always Verify Global Variables Exist

Never assume a global variable exists. Always check first:
```
if {VarExists $myGlobal} is false
    stop
end
```

---

## Math Expressions

```
$five = 2 + 3
$result = $health + 20
$damage = $baseDamage * 1.5
```

You can use standard operators: `+`, `-`, `*`, `/`, `%`
Math expressions are handled via old NCalc (1.3.8) - refer to NCalc knowledge about type coercion.

### Negative Values

Negative values don't require parentheses in methods:
```
$num = -21
TPPosition @fighter -37 313 -140
```
This is because the whitespace seperates these values into different arguments.

### Percent Sign

The `%` suffix is a valid number modifier that divides by 100:
```
$val = 100%  # becomes 1
$chance = 50%  # becomes 0.5
```

---

## Text and String Interpolation

### Basic Text

Text is enclosed in double quotes:
```
Print "Hello, world!"
Broadcast @all 5s "Welcome!"
```

### Text Interpolation with `{}`

Insert values into text using curly braces:

```
$name = "Player"

Print "Hello {$name}"  # prints: Hello Player
Print "Players online: {AmountOf @all}"  # prints: Players online: 5
Print "Role: {@plr -> role}"  # prints: Role: ClassD
```

### Escaping Interpolation

Use `~` before `{}` to prevent interpolation:

```
$var = "hi"

Print "var: {$var}"  # prints: var: hi
Print "var: ~{$var}"  # prints: var: {$var}
```

### Newlines

Use `<br>` for newlines:

```
Print "Line 1<br>Line 2<br>Line 3"
```

---

## Comments

Comments start with `#`:

```
# Comments must have a space after the pound sign
Print "Hello"

Print "Hello"  # Maintain 2 spaces between code and comment if inline
```

---

## Conditional Branching (`if`, `else`, `elif`)

Use `if` to run code only when a condition is met:

```
if $hp < 20 and $healedRecently is false
    Broadcast @sender 5s "You are critically injured!"
else
    Broadcast @sender 5s "You are healing."
end
```

### Condition Syntax

You can use natural language or symbols:
- `is` / `==` - equals
- `isnt` / `!=` - not equals
- `>` - greater than
- `<` - less than
- `and` / `&&` - logical AND
- `or` / `||` - logical OR
- `not` / `!` - ILLEGAL, DO NOT USE
> `!` is illegal because SER relies heavily on spaces, providing `!$var` would break the compiler
> `not` is illegal because it could confuse `x isnt y` with `x is not y`

### Using Properties in Conditions

Always use brackets when accessing properties in conditions:

```
if {@plr -> role} is "ClassD"
    ...
end

if {RoundInfo duration} > 10m
    ...
end
```

### Early Returns

Avoid unnecessary nesting by using early returns:

```
if $invalid is true
    stop
end

# rest of script here
```

---

## Loops

### `repeat` - Fixed Iterations

Runs a specific number of times:

```
repeat 5
    Print "Hello"
end
```

Allows an iteration variable:
```
repeat 5 with $iter
    Print "Current iteration: {$iter}"
end
```

### `while` - Conditional Loop

Runs as long as a condition is true:

```
while $count < 10
    $count = $count + 1
end
```

Allows an iteration variable:
```
while $count < 10 with $iter
    $count = $count + $iter
end
```

### `over` - Iterate Over Collections

Loops through every item in a collection or player array:

```
over @all with @plr
    if {@plr -> role} is "Scientist"
        GiveItem @plr KeycardScientist
    end
end
```

Allows an iteration variable:
```
over @all with @plr $iter
    if {@plr -> name -> length} > $iter
        ...
    end
end
```

### `forever` - Infinite Loop

Runs indefinitely. **Must include `wait` to prevent server freeze:**

```
forever
    wait 1s
    Print "Still running..."
end
```

Allows an iteration variable:
```
forever with $iter
    wait 1s
    Print "Waiting: {$iter}"
end
```

### Loop Control

**`break`** - Exit the loop immediately:
```
repeat 10
    if $condition is true
        break
    end
end
```

**`continue`** - Skip to the next iteration:
```
over @all with @plr
    if {@plr -> role} is "Spectator"
        continue
    end
    
    # only runs for non-spectators
end
```

### The `with` Keyword

Name the current item or iteration:

```
over @all with @plr
    # @plr is now available
end

repeat 5 with $i
    # $i is the current iteration (1-5)
end
```

---

## Waiting and Yielding

### `wait` - Pause Execution

Pause the script for a duration:

```
wait 5s
wait 100ms
wait 1m
```

### `wait_until` - Pause Until Condition

Pause until a condition becomes true:

```
wait_until {AmountOf @all} > 0
wait_until {@plr -> health} < 50
```

---

## Functions

Functions allow you to reuse code. They must be defined before use (hoisted).

### Defining Functions

```
func $GetStatus with $val
    if $val > 50
        return "Healthy"
    else
        return "Injured"
    end
end
```

### Function Naming Convention

The function name prefix indicates what it returns:
- `func $Name` - returns a literal
- `func @Name` - returns players
- `func *Name` - returns a reference
- `func &Name` - returns a collection
- `func Name` - returns nothing

### Calling Functions

Use `run` to execute a function:

```
$result = run $GetStatus 75
@players = run @GetAllScientists
run PrintMessage
```

### Function Parameters

Parameters are defined with `with` on the line after the function header:

```
func $Add with $a $b
    return $a + $b
end

$sum = run $Add 5 3
```

---

## Error Handling

Use `attempt` and `on_error` for try-catch logic:

```
attempt
    PlayAudio "invalid speaker" "invalid clip name"
on_error with $msg
    Print "Error occurred: {$msg}"
end
```

---

## Script Entry Points - Flags

A script can optionally start with a flag. This must be the **very first line**.

### No Flag (Utility Script)

If there's no `!--` line, the script is a utility. Run it manually via `serrun` or `RunScript`:

### Custom Command Flag

Creates a custom command and binds it to the script. When the command is ran, the script runs as well.

```
!-- CustomCommand heal
-- availableFor Server RemoteAdmin
-- description "Heals the sender"

Heal @sender
Broadcast @sender 5s "You have been healed!"
```

### Event Flag

```
!-- OnEvent PlayerDying

if {@evPlayer -> role} is "ClassD"
    IsAllowed false
    stop
end
```

**Important:** Flags are only registered when the server starts or when `serreload` command is used. If you modify a script with a flag on a running server, you **must use `serreload`**.

---

## Event Cancellation

For `OnEvent` scripts, you can prevent the game from executing the event:

```
!-- OnEvent Dying

if {@evPlayer -> role} is "ClassD"
    IsAllowed false
    stop
end
```

### Checking if Event Variables Exist

`OnEvent` flags add variables associated with a given event. (done by reflecting on C#)
But event variables might not always be provided by the C# event:

```
!-- OnEvent Dying

if {VarExists @evAttacker} is false
    stop
end
```

---

## Stopping Execution

The `stop` keyword immediately ends the script. No further lines are executed:

```
if $invalid is true
    Print "Invalid!"
    stop
end

Print "This won't run if invalid"
```

---

## Common Patterns

### Check if Player is SCP

```
if {@plr -> team} is "SCPs"
    # player is an SCP
end
```

### Select Random Player

```
@plr = LimitPlayers @all 1
```

### Percentage Chance

```
if {Chance 20%}
    # 20% chance to execute
end
```

### Check if Variable Exists

```
if {VarExists $myVar} is false
    stop
end
```

---

## Advanced Features

SER includes several advanced systems you should explore:

- **DB System** - JSON-based long-term storage
- **HTTP/JSON** - Web requests and JSON manipulation
- **Collections** - Methods for manipulating lists
- **Discord Webhooks** - Discord integration
- **Player Data** - Dictionary/HashMap attached to players

## TOP 20 ESSENTIAL METHODS

**1. Broadcast** - `Broadcast @players duration "message"` - Sends broadcast to players
**2. Hint** - `Hint @players duration "message"` - Sends hint (different display)
**3. GiveItem** - `GiveItem @players ItemType amount` - Gives items to players
**4. SetRole** - `SetRole @players RoleTypeId flags reason` - Changes player roles
**5. Kill** - `Kill @players reason` - Kills players (optional reason)
**6. Damage** - `Damage @players amount` - Damages players by amount
**7. Heal** - `Heal @players amount` - Heals players (won't exceed max)
**8. TPPlayer** - `TPPlayer @targets @destination` - Teleports to another player
**9. TPPosition** - `TPPosition @players x y z` - Teleports to coordinates
**10. SetHealth** - `SetHealth @players amount` - Sets exact health value
**11. SetMaxHealth** - `SetMaxHealth @players amount` - Sets maximum health
**12. Cassie** - `Cassie jingle/noJingle "announcement" "translation"` - CASSIE announcements
**13. CloseDoor** - `CloseDoor *` - Closes all doors (use * for all)
**14. OpenDoor** - `OpenDoor *` - Opens all doors
**15. LockDoor** - `LockDoor * LockReason` - Locks doors with reason
**16. UnlockDoor** - `UnlockDoor *` - Unlocks doors
**17. GiveEffect** - `GiveEffect @players EffectType duration intensity` - Applies status effects
**18. ClearInventory** - `ClearInventory @players` - Clears player inventory
**19. SetSize** - `SetSize @players x y z` - Changes player size (0.1-10 scale)
**20. Explode** - `Explode @players` - Creates explosion at player

**Utility Methods:**
- **AmountOf** - `AmountOf @players` - Returns count of players
- **RoundLock** - `RoundLock true/false` - Locks/unlocks round
- **LobbyLock** - `LobbyLock true/false` - Locks/unlocks lobby
- **SetPlayerData** - `SetPlayerData @player "key" value` - Stores custom player data
- **GetPlayerData** - `GetPlayerData @player "key"` - Retrieves custom player data
- **HasPlayerData** - `HasPlayerData @player "key"` - Checks if data exists
- **LimitPlayers** - `LimitPlayers @players count` - Returns random subset
- **RandomNum** - `RandomNum min max type` - Random number (int/real)
- **Chance** - `Chance percentage` - Returns true with given chance
- **VarExists** - `VarExists $variable` - Checks if variable exists
- **ValidRef** - `ValidRef *reference` - Checks if reference is valid

## TOP 5 ESSENTIAL EVENTS

**1. RoundStarted** - `!-- OnEvent RoundStarted` - Triggers when round starts
- **Use:** Setup scripts, initial item distribution, round-specific events
- **Variables:** None

**2. Death** - `!-- OnEvent Death` - Triggers when player dies
- **Use:** Kill streaks, death rewards, statistics tracking
- **Variables:** `@evPlayer` (victim), `@evAttacker` (killer), `$evOldRole`, `*evOldPosition`

**3. Hurt** - `!-- OnEvent Hurt` - Triggers when player takes damage
- **Use:** Damage tracking, special damage effects, hit reactions
- **Variables:** `@evPlayer` (victim), `@evAttacker` (damager), `$evDamage`

**4. Joined** - `!-- OnEvent Joined` - Triggers when player joins server
- **Use:** Welcome messages, initial setup, tutorial hints
- **Variables:** `@evPlayer` (joining player)

**5. ChangedRole** - `!-- OnEvent ChangedRole` - Triggers when role changes
- **Use:** Role-specific setups, class transitions, spawn effects
- **Variables:** `@evPlayer`, `$evOldRole`, `$evNewRole`

## COMMON PATTERNS

**Random Player Selection:**
```ser
@randomPlayer = LimitPlayers @all 1
```

**Percentage Chance:**
```ser
if {Chance 25%}
    # 25% chance to execute
end
```

**Check if Player is SCP:**
```ser
if {@player -> team} is "SCPs"
    # Player is SCP
end
```

**Early Return Pattern:**
```ser
if $invalid is true
    stop
end
```

**Player Data Storage:**
```ser
SetPlayerData @player "kills" 5
$kills = GetPlayerData @player "kills"
```

**Door Control:**
```ser
CloseDoor *
LockDoor * NoPower
wait 10s
UnlockDoor *
```

**Teleport to Room:**
```ser
*room = GetRoomByName "RoomName"
TPRoom @player *room
```

**Item Management:**
```ser
GiveItem @player KeycardO5
ClearInventory @player
```

**Health Management:**
```ser
SetHealth @player 100
SetMaxHealth @player 150
Heal @player 50
```

**Broadcast with Variables:**
```ser
Broadcast @all 5s "Player: {@player -> name}, Health: {@player -> health}"
```

