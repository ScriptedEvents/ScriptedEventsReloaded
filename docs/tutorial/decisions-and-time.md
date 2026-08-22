# Decisions, chance, and timing

Your scripts become much more interesting when they can wait, check the game,
and choose what happens next.

Let us turn the Surface warning into a dramatic—but safe—admin event.

## Practical script: Surface Smite

Create `surfacesmite.ser`:

```ser
!-- CustomCommand surfacesmite
-- description "Kills a random player currently in the Surface Zone."

Countdown @all 10s "A random person on surface is going to be killed in %seconds% seconds!"
wait 11s

@target = Take @surfacePlayers 1

if {AmountOf @target} isnt 1
    Broadcast @all 5s "No players found in the Surface Zone!"
    stop
end

Kill @target "The surface is no longer safe."
Broadcast @all 5s "Eliminated {@target -> name} from the surface."
```

This is the build-validated [`surfaceSmite.ser`](../../Example%20Scripts/surfaceSmite.ser)
example. Test it away from a public round: its successful path really kills a
player.

## What the new pieces do

`wait 11s` pauses this script only. The server and every other script continue
running.

`Take @surfacePlayers 1` selects at most one player. If Surface is empty, it
returns an empty player value.

The `if` statement protects the property and `Kill` calls:

```ser
if {AmountOf @target} isnt 1
    Broadcast @all 5s "Nobody was selected."
    stop
end
```

`stop` ends this script immediately. `end` closes the decision.

## Add chance without doing mathematics

The statement form reads naturally:

```ser
chance 25%
    Broadcast @all 5s "The rare outcome happened!"
else
    Print "No rare outcome this time."
end
```

Use the returning form when chance is part of a larger condition:

```ser
if {Chance 25%}
    GiveItem @all Coin
end
```

## Wait for the game, not a fixed clock

`wait_until` pauses until a condition becomes true:

```ser
Print "Waiting for every SCP to leave play..."
wait_until {AmountOf @scpPlayers} is 0
Broadcast @all 5s "No living SCPs remain."
```

After a long wait, remember that players may disconnect and item, room, or any 
other references may become invalid.

## Try one change

Turn Surface Smite into Surface Mercy: keep the countdown and safe empty check,
but replace `Kill` with `TPRoom @target LczToilets` and change the messages.

You now know enough control flow to build a real repeating event.

Next: [build Hot Potato](hot-potato.md).
