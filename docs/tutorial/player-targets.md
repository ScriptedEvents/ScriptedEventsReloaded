# Choose which players are affected

Many SER methods begin with a player argument:

```ser
Broadcast @all 5s "Message"
Heal @sender 20
SetRole @classDPlayers Scientist
```

The `@` value answers the most important question: **which players?**

## Useful player shortcuts

SER keeps common groups ready for you:

| Value | Who it means |
|---|---|
| `@all` | every connected player |
| `@alivePlayers` | every living player |
| `@scpPlayers` | living SCP players |
| `@classDPlayers` | Class-D players |
| `@foundationForcePlayers` | Foundation forces |
| `@surfacePlayers` | players currently on Surface |
| `@sender` | the player who used a command |
| `@evPlayer` | the main player supplied by many events |

Run `serhelp variables` for the list provided by your installed version. There
are groups for roles, teams, zones, living players, spectators, NPCs, and more.

## Practical script: discourage Surface camping

Create `surfacewarning.ser`:

```ser
!-- CustomCommand surfacewarning
-- availableFor RemoteAdmin Server
-- description "Warns and damages players camping on Surface"

Broadcast @surfacePlayers 8s "<b><color=red>Leave the Surface Zone!</color></b>"
Damage @surfacePlayers 20 "Surface camping"
```

An administrator now has one command which affects only players in that zone.
There is no need to look up names or player IDs. The value of
`@surfacePlayers` is resolved when the method runs.

## A player value can contain many players

This is why the same method can target one person or a whole group:

```ser
Heal @sender 20
Heal @classDPlayers 20
Heal @all 20
```

Combine player groups with `Join`, `Except`, and `Intersect`. `Intersect` keeps
only players that appear in every supplied group while preserving the order of
the first group:

```ser
@connectedParticipants = Intersect @eventParticipants @all
```

This is useful when a stored group may contain players who have since
disconnected. Additional players in `@all` are not added because they were not
in `@eventParticipants`.

Be careful when reading a property such as a name or health value. Those
questions normally make sense for exactly one player, not a group. We will
select one player and inspect them in the next lesson.

## Try one change

Turn `surfacewarning.ser` into a zone-specific announcement. Replace
`@surfacePlayers` with another predefined zone group from `serhelp variables`,
remove the `Damage` line, and change the message.

You now have methods, triggers, and useful targets—the core of many SER
scripts. Variables become worthwhile only when you need to remember a result.

Next: [remember values and inspect players](variables-and-properties.md).
