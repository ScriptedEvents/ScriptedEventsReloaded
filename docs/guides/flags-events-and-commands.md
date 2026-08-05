# Flags, events, and commands

A flag binds a script section to a trigger. Put the declaration first, followed
by any `--` options and then executable instructions.

One file may contain multiple sections. Each new `!--` declaration starts the
next section, and every section may have one major behavior flag.

Discover the current flags with:

```text
serhelp flags
serhelp CustomCommand
serhelp OnEvent
```

## Custom commands

```ser
!-- CustomCommand healme
-- availableFor Player RemoteAdmin
-- requireSender
-- description "Heals the command sender."
-- cooldown 30s

Heal @sender 25
Reply "You have been healed."
```

`-- requireSender` prevents execution when no player-backed `@sender` exists.
Use it whenever the script depends on a player sender. It cannot be combined
with the `Server` console.

Command arguments become local literal variables:

```ser
!-- CustomCommand announce
-- availableFor Player RemoteAdmin
-- requireSender
-- arguments message

Broadcast @all 8s "{$message}<br> - {@sender -> name}"
```

An argument ending with `?` is optional. Required arguments must come first.
Permissions, ranks, cooldowns, and use limits are documented by
`serhelp CustomCommand`.

## Game events

```ser
!-- OnEvent Joined
-- require @evPlayer

Broadcast @evPlayer 8s "Welcome to the server!"
```

Use `serhelp events` to browse categories and `serhelp EventName` to inspect
injected variables and cancellability.

`-- require` silently skips the section when a required event variable is
absent:

```ser
!-- OnEvent Death
-- require @evPlayer @evAttacker

Print "{@evAttacker -> name} killed {@evPlayer -> name}"
```

If absence is a case the script should handle, omit that variable from
`-- require` and test it with `VarExists`.

Only an event reported as cancellable can be cancelled:

```ser
!-- OnEvent Dying
-- require @evPlayer

if {@evPlayer -> role} is "Tutorial"
    IsAllowed false
    stop
end
```

`IsAllowed false` changes the game event. `stop` prevents later SER
instructions in this section from running.

## ProjectMER events

When ProjectMER is installed:

```ser
# requires ProjectMER
!-- OnPMER SchematicSpawned
-- require *evSchematic

Print "Spawned {$evName}"
MER.PlayAnimation *evSchematic "Open"
```

Run `serhelp pmerevents` to list events exposed by the installed ProjectMER
version.

## Other flags

| Flag | Purpose |
|---|---|
| `OnCRole` | React to a SER custom role being assigned or removed |
| `OnCustomTrigger` | React when the `Trigger` method fires a matching name |
| `InteractableToyEvent` | React to a SER interactable toy |
| `Function` | Legacy cross-file function script called by `RunFunc` |

Prefer inline functions for new logic contained in one file.

Name each legacy function and call it by that name, including when the file
contains other sections:

```ser
!-- Function HandleDoor
-- argument *door

LockDoor *door NoPower
```

Use `RunFunc HandleDoor *door`. Function names are globally unique. Unnamed
legacy functions remain callable by their file or section name.

Existing bindings reload their script before the next execution. After adding or
removing bindings which cannot trigger yet, run `serreload` (or restart the round)
and inspect `serstatus`.

Next reference: [functions, variable lifetimes, and errors](../language/functions-scopes-and-errors.md).
