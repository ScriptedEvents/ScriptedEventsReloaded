![VERSION](https://img.shields.io/github/v/release/ScriptedEvents/ScriptedEventsReloaded?include_prereleases&logo=gitbook&style=for-the-badge)
![COMMITS](https://img.shields.io/github/commit-activity/m/ScriptedEvents/ScriptedEventsReloaded?logo=git&style=for-the-badge)
[![DISCORD](https://img.shields.io/discord/1060274824330620979?label=Discord&logo=discord&style=for-the-badge)](https://discord.gg/3j54zBnbbD)
<img alt="scriptedeventslogo.png" height="300" src="scriptedeventslogo.png"/>


# What is `Scripted Events Reloaded`?
**Scripted Events Reloaded (SER)** is an SCP:SL plugin that adds a custom scripting language for server-side events.

# Main goal
Making plugins with C# is NOT an easy thing, especially for beginners.
If you want to get started with SCP:SL server-side scripting, SER is a great way to begin.

SER simplifies the most essential plugin features into a friendly package.
All you need to get started is a text editor and a server!

# Nice-to-Haves of SER
- **Simplification** of the most essential features like commands, events, and player management.
- **No compilation required**, while C# plugins require a full development environment, compilation, and DLL management.
- **Lots of built-in features** like custom roles, audio, map editor, databases, Discord webhooks, HTTP, and more!
- **Extendable** with integrations such as UCR, EXILED, Callvote, and ProjectMER,
  but only when you need it - no unnecessary dependencies!
- **Plugin docs** are available directly on the server using the `serhelp` command.
- **Helpful community** available to help you with any questions you may have.

# SER Tutorials

Want to make something useful before studying the language? Follow the
outcome-first tutorial. Need an exact rule or edge case? Jump to the technical
reference. Both are versioned with the plugin:

> [SER 1.0 documentation](./docs/README.md)

The previous GitBook documents older releases and is not the source of truth
for the current 1.0 branch.

# SER editing tools

SER ships one synchronized editing system in two forms:

- **SER Blocks**, a standalone beginner editor generated as `SER Visual Editor.html`;
- a VS Code extension with completions, hovers, shared diagnostics, and the same
  visual editor available through **SER: Open Blocks Editor**.

# Quick start

1. Install `SER.dll` as a LabAPI plugin and start the server once.
2. Open the script directory shown by `serhelp start` or `serstatus`.
3. Create `hello.ser` containing:

   ```ser
   Print "Hello from SER!"
   ```

4. Run `serrun hello`. SER discovers or reloads the requested file immediately
   before creating the runnable script.
5. Use `serreload` when you want to refresh the complete directory at once, and
   `serstatus` to see accepted, failed, disabled, excluded, linked, and conflicting paths.

`.ser` is the preferred extension. `.txt` is an identical compatibility format
for server hosts whose file managers do not allow users to open unknown file
types. A script name is global: two files with the same base name cannot coexist,
even in different folders.

Run `serexamples` to generate validated examples. Their names begin with `#`, so
they remain disabled until you copy one or remove the leading `#` and run
`serreload`. A folder whose name begins with `#` is also disabled: SER skips the
folder and its entire contents during script discovery.
Linked folders (symbolic links and directory junctions) are also skipped so
discovery cannot leave the SER directory or loop indefinitely.

# Examples

The build validates every script in the
[`Example Scripts`](./Example%20Scripts) directory. The short examples below use
the same current syntax.

One `.ser` or `.txt` file may contain multiple independent handlers. Each `!--`
flag starts a new section that ends immediately before the next flag.
Multi-section files can be addressed as `filename:1`, `filename:2`, and so on.

SER reloads the complete script directory when the plugin initializes and whenever
the round restarts. Every file-backed script is also checked and reloaded immediately
before execution, whether requested by `serrun`, an event, a callback, or another
script. Use the permission-protected `serreload` command to refresh everything on
demand, especially when adding or removing bindings that cannot trigger yet. Reloads
are transactional: a changed file is compiled in full before its flags are replaced,
and if validation fails, the last known-good version stays active.

```ser
!-- OnEvent RoundStarted
Print "Round started"

!-- OnEvent Death
Print "A player died"

!-- CustomCommand status
-- requireSender
Reply "Hello {@sender -> name}! The server is online"
```

ProjectMER integrations can use the dedicated optional event entry point:

```ser
!-- OnPMER SchematicSpawned
-- require *evSchematic

Print "Spawned {$evName}"
MER.PlayAnimation *evSchematic "Open"
```

### Welcome message
```
!-- OnEvent Joined
-- require @evPlayer

Broadcast @evPlayer 10s "Welcome to the server {@evPlayer -> name}!"
```
### Coin on kill
```
!-- OnEvent Death
-- require @evAttacker

# give the attacker a coin
GiveItem @evAttacker Coin
```
### VIP broadcast command
```
# define the command with custom attributes
!-- CustomCommand vipbc
-- description "broadcasts a message to all players - VIP only"
-- neededRank vip svip mvip
-- arguments message
-- availableFor player
-- requireSender
-- cooldown 2m

# send the broadcast to all players
Broadcast @all 10s "{@sender -> name} used VIP broadcast<br>{$message}"
```
### Heal random SCP
```
!-- CustomCommand healscp
-- description "heals a random SCP"
-- availableFor Player
-- requireSender
-- cooldown 10s

# dont allow SCPs to use this command
if {@sender -> team} is "SCPs"
    stop
end

# get a random SCP that is not a SCP-079
@randomScp = Take {Except @scpPlayers @scp079Players} 1

# get 5% of the SCP's max health
$healAmount = Round ({@randomScp -> maxHealth} * 0.05)

Heal @randomScp $healAmount
Broadcast @randomScp 4s "{@sender -> name} healed you with {$healAmount} HP!"
```
For a complete event script, see
[`hotPotato.ser`](./Example%20Scripts/hotPotato.ser), which is validated during
every build.

# Are you an LLM?
Check the [syntax definition](https://raw.githubusercontent.com/ScriptedEvents/ScriptedEventsReloaded/refs/heads/main/language_specification.md) for guidance about SER script-making.

# Building release artifacts

Run `tools/package-release.ps1` from PowerShell. It rebuilds and verifies both
plugin hosts and the editor tooling, then creates minimal LabAPI and EXILED
plugin bundles, the examples/documentation bundle, the standalone editor, the
VS Code extension, a complete bundle, and `SHA256SUMS.txt` under `artifacts/`.
Release maintainers can pass `-UcrReferencePath <path>` to compile against the
exact official UCR DLL selected for that compatibility baseline.

# Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for a fresh-clone setup, the normal
verification commands, generated-file rules, and pull-request checklist.
