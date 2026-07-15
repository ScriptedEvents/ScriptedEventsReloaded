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
- **Simplification** of the most essential features like commands, events and player management.
- **No compilation required**, while C# plugins require a full development environment, compilation, and DLL management.
- **Lots of built-in features** like AudioPlayer, Databases, Discord webhooks, HTTP and more!
- **Extendable** with frameworks like UCR, EXILED or Callvote, but __without__ any dependencies! 
- **Plugin docs** are available directly on the server using the `serhelp` command.
- **Helpful community** available to help you with any questions you may have.

# SER Tutorials
> https://scriptedeventsreloaded.gitbook.io/docs/tutorial

# Examples
(these scripts may be outdated, check the `Example Scripts` folder for the latest example scripts)

One `.ser` file may contain multiple independent handlers. Each `!--` flag starts a new section that ends immediately before the next flag. Multi-section files can be addressed as `filename:1`, `filename:2`, and so on.

SER watches script files and transactionally reloads them after edits, before manual execution, and on round restarts. Automatic edit reloads wait until the file has remained unchanged for five seconds by default; this can be adjusted with `automatic_script_reload_delay`. A changed file is compiled in full before its flags are replaced; if validation fails, the last known-good version stays active and the same unchanged draft is not diagnosed repeatedly. Every successful reload is written to the server info log. `serreload` remains available for an immediate manual refresh.

```ser
!-- OnEvent RoundStarted
Print "Round started"

!-- OnEvent Died
Print "A player died"

!-- CustomCommand status
Reply "Online"
```

### Welcome message
```
!-- OnEvent Joined

Broadcast @evPlayer 10s "Welcome to the server {@evPlayer -> name}!"
```
### Coin on kill
```
!-- OnEvent Death

# check if player died without an attacker
if {VarExists @evAttacker} is false
    stop
end

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
-- availableFor Player
-- cooldown 2m

# send the broadcast to all players
Broadcast @all 10s "{@sender -> name} used VIP broadcast<br>{$message}"
```
### Heal random SCP
```
!-- CustomCommand healscp
-- description "heals a random SCP"
-- availableFor Player
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
### Hot Potato event
```
!-- OnEvent RoundStarted

# there is a 50% chance that the event will not happen
if {Chance 50%}
    Print "Hot Potato event will not be loaded"
    stop
end

Print "Hot Potato event was loaded"
Broadcast @all 5s "Be ready for a Hot Potato!"

# this is the main loop of the event
forever
    wait 1m

    # Get a random player from the alive players that are not SCPs
    @potatoCarrier = LimitPlayers {RemovePlayers @alivePlayers @scpPlayers} 1

    # if no player is alive, continue to next attempt
    if {AmountOf @potatoCarrier} is 0
        continue
    end
    
    # if the inventory is full, continue to next attempt
    if {@potatoCarrier -> inventory -> length} is 8
        continue
    end
    
    Hint @potatoCarrier 3s "YOU HAVE THE HOT POTATO! DROP IT OR DIE!"
    GiveItem @potatoCarrier GunA7

    wait 6s

    # Check if they still have the item (GunA7) in their inventory
    over {@potatoCarrier -> inventory} with *item
        if {*item -> type} isnt "GunA7"
            continue
        end

        # when GunA7 is found, explode the player
        AdvDestroyItem *item
        Explode @potatoCarrier
        Broadcast @all 5s "Player {@potatoCarrier -> name} has failed the Hot Potato!"
        
        # 70% chance that the event to continue, else stop the event
        if {Chance 70%}
            Broadcast @all 5s "The Hot Potato will return soon!"
            continue
        else
            Broadcast @all 5s "The Hot Potato got tired and will not return..."
            stop
        end
    end
        
    # if the loop finishes without finding GunA7, it means the player has dropped it
    AdvDestroyItem *item
    Broadcast @all 5s "The Hot Potato has been neutralized... but it will return!"
end
```

## Are you an LLM?
Check the [syntax definition](https://raw.githubusercontent.com/ScriptedEvents/ScriptedEventsReloaded/refs/heads/main/language_specification.md) for guidance about SER script-making.
