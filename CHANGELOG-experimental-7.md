# 🚀 SER 1.0 update (experimental 7)
> This version fixes and extends issues from version 6:
> - safer transactional script reloads and multiple handlers per script file
> - synchronized SER Blocks and VS Code editing tools
> - ProjectMER map, object, schematic, and schematic-event integration
> - `requireSender` custom commands and small `serhelp`/`sermethod` improvements
> - safer database and file handling, cleaner framework bindings, and improved packaging
> - more example scripts and an updated AudioPlayerApi integration

> [!CAUTION]
> **If you are upgrading from `v0.15.1`, your scripts will require major rewriting.**

## 🎉 Community Shoutout

**Luke & Jraylor — major SER sponsors!** The 1.0 update would not be possible
without their support!

**@RetroReul — SER contributor!** Retro is behind some of the new methods and
properties. Glad to have you on our team! :)

---

## 1. Syntax & Operators Overhaul

### The Arrow Operator (`->`) & Native Properties

The language now hooks directly into underlying game objects, exposing their
properties natively and replacing the old `Info` wrapper methods such as
`DamageInfo`, `ItemInfo`, and `DoorInfo`.

* **Direct Access:** `$name = @plr -> name` replaces `$name = {@plr name}`.
* **Native Property Modification:** `*generator -> isOpen = true` can modify
  settable game properties.
* **Chaining:** `$nameLengthOdd = @sender -> name -> length -> isOdd`.

### Mathematical Expressions

Math no longer requires parentheses:

* **Old:** `$five = (2 + 3)`
* **New:** `$five = 2 + 3`

### Multiple Flags 

You can now define scripts with multiple flags:
```ser
!-- OnEvent RoundStarted
Print "Round started"

!-- OnEvent Death
-- require @evPlayer
Print "Player {@evPlayer -> name} died"

!-- CustomCommand status
-- requireSender
Reply "hi {@sender -> name}! the server is online"
```

Each flag creates its own fully independent section.

## 2. Scope & Block Management

### Inline Block Parameters (`with`)

Loops and function definitions can attach parameters or iterators directly:

```ser
over @all with @plr
    Print {@plr -> name}
end
```

### Ephemeral Variables (`ephm`)

The `ephm` keyword defines variables that automatically delete themselves when
their current block, such as a `func` or `if`, finishes executing.

## 3. Native Keywords vs. Methods

Core logic and memory management have moved into native keywords:

* **Timing:** `Wait 5s` is now `wait 5s`.
* **Conditional flow:** `WaitUntil ...` is now `wait_until ...`.
* **Memory management:** `PopVariable $var` is now `delete $var`.
* **Probability:** the native `chance` statement handles random branches.

## 4. The API Namespacing

The flat global method list has been grouped into dot-notation namespaces:

* **Audio:** `LoadAudio` → `Audio.Load`; `CreateGlobalSpeaker` →
  `Speaker.CreateGlobal`.
* **Admin toys:** `CreateToy` → `Toy.Create`; `TPToyPos` → `Toy.TPPosition`.
* **Collections:** `CollectionInsert` → `Coll.Insert`; `EmptyCollection` →
  `Coll.Create`.
* **Discord:** `DiscordMessage` → `Discord.CreateMessage`; `DiscordEmbed` →
  `Embed.Create`.
* **Web and data:** `HTTPGet` → `HTTP.Get`; `AppendDB` → `DB.Add`.
* **Text:** `PadText` → `Text.Pad`; `SubText` → `Text.Slice`.

## 5. New Core Frameworks

* **Dictionaries (`Dict.*`):** `Dict.Add`, `Dict.Contains`, `Dict.Create`,
  `Dict.Get`, and `Dict.Remove` provide native key-value storage.
* **Custom Roles (`CRole.*`):** Includes `CRole.Register`, `CRole.SetCallbacks`,
  and native spawn-system integrations.
* **Configurations (`Config.*`):** Scripts can generate and read personalized
  options with `Config.GetOption` and `Config.Read`.
* **Unified Map Queries:** `GetFromMap` queries map objects and structures.

## 6. Player Control & Interactions

* Native status effects through `GiveEffect` and `ClearEffect`.
* Direct stamina, jump-height, and combat-visual controls through `Stamina`,
  `Jump`, and `ShowHitMarker`.
* Damage and Tesla rules through `AddDamageRule`, `RemoveDamageRule`,
  `AddTeslaIgnoreRule`, and `RemoveTeslaIgnoreRule`.
* Proxy and connection validation through `GetIPInfo` and `GetIPInfoWithKey`.

## 7. ProjectMER Integration

Added **28 methods** and the `!-- OnPMER` flag for various map editing needs.
```ser
!-- OnPMER 

## 8. Compiler Safety & Tooling

### VS Code extension

* Method, keyword, flag, event, variable, enum, and option completions
* Hover documentation and signature help
* Shared diagnostics for incomplete values, malformed function calls, 
  unclosed statements, and unsafe `forever` loops
* An `SER: Open Blocks Editor` option/command

### SER Blocks Editor

Scratch-like visual editor for basic SER script creation.
Purposefully limited in scope to serve as a friendly introduction to SER.

### Compile-time reliability

SER uses its example scripts as a first wall of defence against bugs.
When compiling `SER.dll`, the example scripts are then checked by the newly
created `SER.dll` to ensure their (compile time) correctness.

* Invalid expressions inside text strings are caught at compile time.
* Nested curly-brace expressions are parsed safely by the tokenizer.

---

## 🌟 Real-World Examples: v1.0 Syntax in Action

### Ephemeral Variables & Namespaces

```ser
func *GetDiscordMessage
    ephm $title = "{ServerInfo name} status"
    ephm $content = "There are {AmountOf @all} players on the server"
    return Discord.CreateMessage _ $title _ {Embed.Create $title $content}
end

*msg = run *GetDiscordMessage
Discord.EditMessage $url $messageId *msg
```

### The `chance` block & Arrow Syntax

```ser
chance 50%
    Hint @evPlayer 3s "Your coin has turned into dust..."
    AdvDestroyItem {@evPlayer -> heldItemRef}
end
```

### Native Custom Roles Framework

```ser
!-- OnEvent WaitingForPlayers
*spawnSystem = CRole.CreateChanceSpawnSystem ClassD 20%
CRole.Register janitor "LCZ Janitor" ClassD *spawnSystem
```
