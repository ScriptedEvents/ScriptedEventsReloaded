# Build-validated examples

The scripts in [`Example Scripts`](../../Example%20Scripts) are embedded in the
plugin and compiled during every build. Prefer them over copied snippets from
old releases.

## Suggested learning order

| Goal | Example |
|---|---|
| Welcome a joining player | [`welcome.ser`](../../Example%20Scripts/welcome.ser) |
| Create a player command | [`selfHealCommand.ser`](../../Example%20Scripts/selfHealCommand.ser) |
| Put several handlers in one file | [`multiSectionHandlers.ser`](../../Example%20Scripts/multiSectionHandlers.ser) |
| Track round state | [`killStreak.ser`](../../Example%20Scripts/killStreak.ser) |
| Block damage between teammates | [`friendlyFireGuard.ser`](../../Example%20Scripts/friendlyFireGuard.ser) |
| Send a round result to Discord | [`roundReport.ser`](../../Example%20Scripts/roundReport.ser) |
| Build a timed event | [`hotPotato.ser`](../../Example%20Scripts/hotPotato.ser) |
| Store persistent-style data | [`coinTracker.ser`](../../Example%20Scripts/coinTracker.ser) |
| Create custom roles | [`customRoles.ser`](../../Example%20Scripts/customRoles.ser) |
| Build a larger multi-event system | [`zombieInfection.ser`](../../Example%20Scripts/zombieInfection.ser) |
| Run whole-round administrator events | [`EventPack`](../../Example%20Scripts/EventPack) |

Generate disabled copies on a server with:

```text
serexamples
```

Copy or rename one so its filename no longer starts with `#`, ensure its base
name is globally unique, and run `serreload`.

Examples demonstrate current syntax, but they are not configuration templates
for every server. Review permissions, ranks, external URLs, audio paths,
cooldowns, and gameplay effects before enabling one.
