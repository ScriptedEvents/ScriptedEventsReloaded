# SCRIPTED EVENTS RELOADED (SER) - Language Spec v1.0.0

## 1. Data Types & Variables

Variables must always include their specific prefix so the engine knows the data type. Create or update variables using `=`.

### The Four Data Types
From most to least used:

| Type           | Prefix | Description                                  | Examples                                         |
|----------------|--------|:---------------------------------------------|--------------------------------------------------|
| **Player**     | `@`    | Array of players                             | `@sender`, `@all`, `@evAttacker`                 |
| **Literal**    | `$`    | Numbers, text, time, booleans, colors, enums | `$age = 10`, `$time = 5s`, `$role = "Scientist"` |
| **Reference**  | `*`    | C# objects (e.g., rooms, items)              | `*spawnRoom`, `*evRoom`                          |
| **Collection** | `&`    | A list of multiple items                     | `&inventory`, `&rooms`                           |

### Memory Scopes

* **Local (Default):** Deleted when the script finishes. (`$var = 10`)


* **Global:** Persists for the entire round. Accessible by other scripts. You **must** use the `global` keyword when assigning/changing it to avoid local name collisions. Read without the keyword.
* *Set:* `global $score = 100`
* *Read:* `Print {$score}`
* *Verify:* `if {VarExists $myGlobal} is false`


* **Ephemeral:** Exists only inside a specific loop or function. (`ephm $x = 1`)

---

## 2. Text, Math, & Syntax

### Text Interpolation & Comments

* **Basic Text:** Enclosed in double quotes (`"Hello!"`).
* **Interpolation (`{}`):** Insert variables/properties into text (`"Hello {$name}"`).
* **Escaping (`~`):** Prevent interpolation (`"var: ~{$var}"` prints `var: {$var}`).
* **Newlines:** Use `<br>`.
* **Comments (`#`):** Must have a space after the pound sign (`# Comment`).

### Math Expressions

* **Operators:** `+`, `-`, `*`, `/`, `%` (Handled via NCalc 1.3.8).
* **Negative Values:** Script parsing relies on whitespace, so negatives don't need parentheses (e.g., `TPPosition @plr -37 313 -140`).
* **Percent Sign (`%`):** Divides a number by 100 (`50%` becomes `0.5`).

---

## 3. Methods & Properties

### Methods (Commands)

Methods perform actions and are written in `PascalCase`.

* **Syntax:** `MethodName Arg1 Arg2` (e.g., `Broadcast @all 5s "Hello"`)
* **Return Values:** Can be stored directly (`$name = ServerInfo name`)
* **The Wildcard (`*`):** Targets ALL of something, excluding players (`CloseDoor *`)
* **Omit Arguments (`_`):** Skip optional arguments (`*embed = Embed.Create "Title" _ _ "Author"`)

### Properties (`->`)

Access internal data of values. Can be chained (e.g., `@plr -> name -> length -> isOdd`).

* **Player Properties:** MUST strictly be one player. Use `{AmountOf @all}` if you need a length count.
* **Reference Validity:** C# objects can become null. Always validate before use: `if {*room -> isInvalid}`.
* **Context Rules:**
* *Variable definitions:* No brackets (`$name = @plr -> name`)
* *Conditions & Interpolation:* Use brackets (`if {@plr -> role} is "ClassD"`)


* **Enum Conversion:** In methods, bare enum tokens work (`SetRole @plr ClassD`). In properties/conditions, enums are converted to strings (`if {@plr -> role} is "ClassD"`).

---

## 4. Control Flow & Execution

### Conditionals (`if`, `elif`, `else`)

Compare values using standard operators (`is`/`==`, `isnt`/`!=`, `>`, `<`, `and`/`&&`, `or`/`||`).

* *Note:* `!` and `not` are illegal.
* *Early Return:* Use `stop` to immediately end the script.

### Loops

| Loop Type     | Description                             | Example Syntax                 |
|---------------|-----------------------------------------|--------------------------------|
| **`repeat`**  | Fixed iterations.                       | `repeat 5 with $iter`          |
| **`while`**   | Runs while condition is true.           | `while $count < 10 with $iter` |
| **`over`**    | Iterates through collections/arrays.    | `over @all with @plr`          |
| **`forever`** | Infinite loop. **Must include `wait`.** | `forever with $iter`           |

* **Loop Control:** `break` (exit loop) and `continue` (skip iteration).
* **`with` Keyword:** Assigns a name to the current item or iteration number.

### Waiting & Yielding

* **`wait`:** Pause for duration (`wait 5s`, `wait 100ms`).
* **`wait_until`:** Pause until a condition is met (`wait_until {AmountOf @all} > 0`).

---

## 5. Functions & Errors

### Functions

Must be hoisted (defined before use). 
The prefix in the name defines the return type(`$Name` = literal, `@Name` = players, `*Name` = reference, `&Name` = collection, `Name` = nothing). 
Call using `run`.

```ser
func $Add with $a $b
    return $a + $b
end
$sum = run $Add 5 3
```

### Error Handling

Use `attempt` and `on_error` to catch exceptions without breaking the script.

```ser
attempt
    PlayAudio "invalid speaker" "invalid clip name"
on_error with $msg
    Print "Error: {$msg}"
end
```

---

## 6. Script Entry Points (Flags)

A script can optionally start with **one** flag, defining how it triggers. Use `serreload` if modifying a flagged script on a live server.

| Flag Type          | Syntax Example                                            | Description                               |
|--------------------|-----------------------------------------------------------|-------------------------------------------|
| **Utility**        | *(No flag)*                                               | Run manually via `serrun` or `RunScript`. |
| **Custom Command** | `!-- CustomCommand heal`<br>`-- availableFor RemoteAdmin` | Binds the script to a custom command.     |
| **Event**          | `!-- OnEvent Dying`<br>`-- require @evPlayer`             | Triggers on a LabAPI game event.          |

* *Event Cancellation:* Use `IsAllowed false` followed by `stop` to cancel the base game event.
* *Event Variables:* Provided via C# reflection, but may not always exist. Use `--require` to validate.

---

## 7. Quick Reference Cheat Sheet

### Top Essential Methods

| Category            | Methods                                                                                                                   |
|---------------------|---------------------------------------------------------------------------------------------------------------------------|
| **Communication**   | `Broadcast`, `Hint`, `Cassie`                                                                                             |
| **Player Control**  | `GiveItem`, `ClearInventory`, `SetRole`, `SetSize`, `GiveEffect`                                                          |
| **Health & Damage** | `Kill`, `Damage`, `Heal`, `SetHealth`, `SetMaxHealth`, `Explode`                                                          |
| **Environment**     | `CloseDoor`, `OpenDoor`, `LockDoor`, `UnlockDoor`                                                                         |
| **Movement**        | `TPPlayer`, `TPPosition`                                                                                                  |
| **Utility**         | `AmountOf`, `Take`, `Random`, `Chance`, `SetRoundLock`, `SetLobbyLock`, `SetPlayerData`, `GetPlayerData`, `HasPlayerData` |

### Top Essential Events

| Event                 | Variables Provided                                         | Common Use Cases                  |
|-----------------------|------------------------------------------------------------|-----------------------------------|
| **RoundStarted**      | None                                                       | Round logic.                      |
| **WaitingForPlayers** | None                                                       | Server systems.                   |
| **Death**             | `@evPlayer`, `@evAttacker`, `$evOldRole`, `*evOldPosition` | Kill streaks, death rewards.      |
| **Hurt**              | `@evPlayer`, `@evAttacker`, `$evDamage`                    | Hit reactions, damage tracking.   |
| **Joined**            | `@evPlayer`                                                | Welcome messages, tutorial hints. |
| **ChangedRole**       | `@evPlayer`, `$evOldRole`, `$evNewRole`                    | Class transitions, spawn effects. |

### Common Scripting Patterns

**Select Random Player:**

```ser
@plr = Take @all 1
```

**Check % Chance & SCP Team:**

```ser
if {Chance 25%} and {@plr -> team} is "SCPs"
    # 25% chance to run for SCPs
end
```

**Player Data Management:**

```ser
SetPlayerData @plr "kills" 5
$kills = GetPlayerData @plr "kills"
```