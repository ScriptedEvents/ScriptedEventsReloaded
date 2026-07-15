# SCRIPTED EVENTS RELOADED (SER) - Language Spec v1.0.0

SER scripts are written in .ser files (for compatibility, .txt is also allowed) 

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

### Expression Braces

Curly braces group an expression when it is used directly as part of another line. They tell the tokenizer exactly where a multi-token expression starts and ends.

* **Variables do not need braces.** Their prefix (`$`, `@`, `*`, or `&`) identifies them as complete value tokens, so these forms are valid:

  ```ser
  if $var is "value"
  Reply $var
  ```

  Braces are still required when the variable is interpolated inside text, for example `Reply "Value: {$var}"`.
* **Method calls require braces when used inline.** A method name is an ordinary word and may or may not be followed by arguments, so the tokenizer cannot reliably determine where the expression ends. Group the complete call:

  ```ser
  if {RetMethod arg} is "value"
  Reply {ServerInfo name}
  ```
* **Property chains require braces when used inline.** The chain contains multiple tokens, so group it in the same way:

  ```ser
  if {@plr -> name} is "Elektryk_Andrzej"
  Reply {@plr -> name}
  ```
* **Variable definitions already provide an expression boundary.** Do not add braces around the right-hand side:

  ```ser
  $var = "value"
  $serverName = ServerInfo name
  $playerName = @plr -> name
  ```

In short: use braces when an inline expression could otherwise be mistaken for separate line arguments; omit them for a standalone assignment or a prefixed variable whose token boundary is unambiguous.

### Math Expressions

* **Operators:** `+`, `-`, `*`, `/`, `%` (Handled via NCalc 1.3.8).
* **Negative Values:** Script parsing relies on whitespace, so negatives don't need parentheses (e.g., `TPPosition @plr -37 313 -140`).
* **Percent Sign (`%`):** Divides a number by 100 (`50%` becomes `0.5`).

---

## 3. Methods & Properties

### Methods (Commands)

Methods perform actions and are written in `PascalCase`.

* **Syntax:** `MethodName Arg1 Arg2` (space separated) (e.g., `Broadcast @all 5s "Hello"`)
* **Return Values:** Can be stored directly (`$name = ServerInfo name`)
* **The Wildcard (`*`):** Targets ALL of something, excluding players (`CloseDoor *`)
* **Omit Arguments (`_`):** Skip optional arguments (`*embed = Embed.Create "Title" _ _ "Author"`)

### Properties (`->`)

Access internal data of values. Can be chained (e.g., `@plr -> name -> length -> isOdd`).

* **Player Properties:** MUST strictly be one player. Use `{AmountOf @all}` if you need a length count.
* **Reference Validity:** C# objects can become null. Always validate before use: `if {*room -> isInvalid}`.
* **Context Rules:**
* *Variable definitions:* The right-hand side is already a complete expression, so no braces are needed (`$name = @plr -> name`).
* *Inline conditions and method arguments:* Use braces for method calls and property chains (`if {RetMethod arg} is "value"`, `if {@plr -> role} is "ClassD"`). A prefixed variable can remain unbraced (`if $role is "ClassD"`).


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

A file can contain multiple flagged script sections. Every `!--` declaration starts a new independent script, and its section continues up to (but not including) the next `!--` declaration. Named `--` arguments belong to the nearest flag above them.

SER automatically checks for changes after a file edit, before a script is run, and on round restarts. Watcher-based reloads wait for five seconds of file inactivity by default (`automatic_script_reload_delay`), giving editors and authors time to finish a save. The complete physical file must compile and register successfully before any of its active sections are replaced. If it does not, SER reports the error once for that file revision and keeps the last known-good version active. Successful reloads produce a server info log. `serreload` can still force an immediate refresh.

```ser
!-- OnEvent RoundStarted
Print "The round started"

!-- OnEvent Died
Print "A player died"

!-- CustomCommand status
Reply "The server is online"
```

The sections of a multi-section file named `roundHandlers.ser` can be addressed manually as `roundHandlers:1`, `roundHandlers:2`, and `roundHandlers:3`. A bare name is accepted only for flagless and single-section files. Only blank lines and comments may appear before the first flag in a multi-section file.

| Flag Type          | Syntax Example                                            | Description                               |
|--------------------|-----------------------------------------------------------|-------------------------------------------|
| **Utility**        | *(No flags in the file)*                                  | Run manually via `serrun` or `RunScript`. |
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

**Event Cancellation:**

```ser
!-- ChangingRole
-- require @evPlayer $evNewRole

if $evNewRole is "ClassD"
    # Do not allow ClassD to change roles
    IsAllowed false
    stop
end
```