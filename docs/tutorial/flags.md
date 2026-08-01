# Commands and events with flags

A method says **what** should happen. A flag says **when** it should happen.

With one line at the top of a script, SER can turn ordinary methods into a
player command or connect them to a game event.

## Give players a `.medic` command

Create `medic.ser`:

```ser
!-- CustomCommand medic
-- availableFor player
-- requireSender
-- description "Restores 35 health"
-- cooldown 30s

Heal @sender 35
Hint @sender 4s "<color=#73ff73>Medic charge used: +35 HP</color>"
Reply "Medic charge used."
```

Reload once so the new command is registered:

```text
serreload
```

A player can now enter `.medic` in their client console.

The script has three parts:

1. `!-- CustomCommand medic` binds this script to the command.
2. The `--` lines configure who may use it and how often.
3. The ordinary methods perform the useful work.

`@sender` is a value SER provides when a player uses the command. You do not
need to understand all variable rules yet; for now, read it as **the player who
ran this command**. `-- requireSender` prevents the script from starting when no
player-backed sender exists.

## Welcome players automatically

Create `welcome.ser`:

```ser
!-- OnEvent Joined
-- require @evPlayer

Broadcast @evPlayer 10s "Welcome {@evPlayer -> name}!"
Hint @evPlayer 6s "Have fun—and read the rules."
```

This time there is no command to run. `OnEvent Joined` starts the script when a
player joins. SER supplies `@evPlayer`, meaning **the player involved in this
event**.

The `-- require` line says the script is useful only when that event value is
available. The `-> name` part reads the player's name; properties get their own
tutorial later.

## Discover flags and events

Use the server rather than guessing names:

```text
serhelp flags
serhelp CustomCommand
serhelp events
serhelp Joined
```

Specific event help lists the values it provides and whether the event can be
cancelled.

## Try one change

Change the welcome duration, colors, or wording. Then try another event from
`serhelp events`. Keep the first version small: one event, one visible result.

You already know enough to create useful commands and reactions. The next step
is learning the player shortcuts which make those methods powerful.

Next: [choose which players are affected](player-targets.md).
