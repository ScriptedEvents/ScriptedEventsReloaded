# Event Pack: Deathmatch and Duel

The build-validated `EventPack` examples provide whole-round events operated
through one Remote Admin command. The first version contains Deathmatch and
Duel and uses only built-in SER/LabAPI features. It does not require
Callvote, EXILED, ProjectMER, UCR, or another optional integration.

## Enable the pack

1. Run `serexamples` after installing the current `SER.dll`.
2. Open the generated `Example Scripts` directory.
3. Rename the `#EventPack` directory to `EventPack`. Renaming the directory
   enables the manager and both workers together.
4. Run `serreload` and check `serstatus`.
5. Grant event organizers the `serverevent.manage` permission.

Do not enable only `eventManager.ser`. The manager deliberately refuses to
start an event when its worker is unavailable.

## Administrator commands

Use these commands in Remote Admin or the server console:

```text
event list
event status
event stop confirm
```

`event stop confirm` announces the cancellation and restarts the round. The
confirmation word protects against an accidental restart.

Events can start only in the pre-round lobby. A player-backed administrator who
runs the command is treated as the organizer and excluded from the participant
list. A command run from the server console includes every eligible player.
Overwatch players, NPCs, and dummies are always excluded. At least two
participants are required. Each worker checks the connected participant count
again after starting the round. If fewer than two remain, the event announces
the cancellation and safely restarts the round before assigning match roles or
loadouts.

## Deathmatch

The complete syntax is:

```text
event start deathmatch [ffa/team] [lcz/hcz/entrance] [weapon]
```

All options are optional. The safe default is `ffa hcz GunAK`. Examples:

```text
event start deathmatch
event start deathmatch ffa lcz Shotgun
event start deathmatch team entrance GunE11SR
event start dm team hcz SCP127
```

Supported weapon names and short forms are: AK, E11, Crossvec, FSP9, Logicer,
Shotgun, Revolver, COM15, COM18, COM45, FRMG0, A7, Particle, Micro, Jailbird,
SCP127, and SCP1509. The manager validates every option before changing the
round.

FFA uses one human role and enables friendly fire. Team mode balances players
between NTF and Chaos and keeps friendly fire disabled. When one team wins,
its survivors are moved to a smaller room, changed to a common role, and fight
a final FFA as required by the event rules.

## Duel

Use one of the following:

```text
event start duel
event start duel Jailbird
event start duel SCP1509
```

Jailbird is the default. The script randomizes the queue. The winner of each
90-second duel stays in the arena and meets the next challenger. A timeout is
resolved by remaining health; equal health is resolved randomly. Disconnecting
players are skipped or forfeit their current duel.

## Safety behavior

Both events lock the round, disable normal respawn waves and LCZ
decontamination, lock the warhead, remove unrelated pickups, and force late or
excluded players into Spectator. Every event has time limits and a top-level
error handler. Normal completion, administrator cancellation, and runtime
errors all end with a round restart so temporary roles, door locks, friendly
fire, items, and global event state cannot leak into the next round.

The build compiler validates script syntax and method contracts, but it cannot
simulate a live SCP:SL map. Before public use, run one short server smoke test
for each zone and verify the two relative spawn points inside `Lcz173` against
the exact game/map version deployed on the server.
