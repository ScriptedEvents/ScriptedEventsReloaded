using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class ChaosCoinScript : Example
{
    public override string Name => "chaosCoin";

    public override string Content =>
        """
        !-- OnEvent FlippingCoin

        # formats of broadcasts, used to not repeat the same things in different broadcasts
        $hintInfo = "<color=#ff5555><size=30><b>"
        $baseText = "<size=20><b><color=green>the holy coin has been used<br><size=35></color>"
        $afterText = "<color=white><size=25><b>"

        # "coin locked" is a property of this player, storing whether a coin can be used.
        # this is because some effects of the coin take time, and we DO NOT want to
        # have the coin be used again WHILE a different effect is still ongoing
        if {HasPlayerData @evPlayer "coin locked"}
            if {GetPlayerData @evPlayer "coin locked"}
                Hint @evPlayer 5s "{$hintInfo}You can't use the coin for now!<br><size=20>Come back when the current effect has finished."
                IsAllowed false
                stop
            end
        end

        # 50% chance to lose the coin
        if {Chance 50%}
            Hint @evPlayer 3s "{$hintInfo}Your coin has turned into dust..."
            AdvDestroyItem {@evPlayer heldItemRef}
        end

        # select a random effect of the coin, from 9 available
        $effect = RandomNum 1 9 int

        # russian rulette with a full gun :trollface:
        if $effect == 1
            GiveItem @evPlayer GunRevolver
            Broadcast @evPlayer 5s "{$baseText}Let's play russian rulette!"
            stop
        end

        # HP change 
        if $effect == 2
            $newHP = RandomNum 1 150 int
            SetHealth @evPlayer $newHP
            SetMaxHealth @evPlayer $newHP

            Broadcast @evPlayer 5s "{$baseText}You now have {$newHP} HP!"
            stop
        end

        # rave 
        if $effect == 3
            *room = {@evPlayer roomRef}

            # explode player if he isnt in a room
            if not {ValidRef *room}
                Explode @evPlayer
                stop
            end

            # we are blocking the use of the coin for the player
            # until the rave is over
            SetPlayerData @evPlayer "coin locked" true

            Broadcast @evPlayer 5s "{$baseText}Time for a rave!"

            CloseDoor *room
            LockDoor *room AdminCommand
            SetLightColor *room #000000

            repeat 5
                TransitionLightColor *room #ff0000ff .5s
                Wait .5s
                TransitionLightColor *room #00ff00ff .5s
                Wait .5s
                TransitionLightColor *room #0000ffff .5s
                Wait .5s
            end

            Wait .1s
            UnlockDoor *room
            ResetLightColor *room

            SetPlayerData @evPlayer "coin locked" false
            stop
        end

        # bomb 
        if $effect == 4
            SetPlayerData @evPlayer "coin locked" true
            $initRole = {@evPlayer role}

            Countdown @evPlayer 15s "{$baseText}<color=red>You have %seconds% seconds left to live!"

            # waiting 15 seconds here
            repeat 15
                Wait 1s

                # every second we are checking if the role of the player changed
                # if so, we remove the countdown and unlock the coin
                if {@evPlayer role} != $initRole
                    ClearCountdown @evPlayer
                    SetPlayerData @evPlayer "coin locked" false
                    stop
                end
            end

            Explode @evPlayer
            SetPlayerData @evPlayer "coin locked" false
            stop
        end

        # bypass
        if $effect == 5
            $initRole = {@evPlayer role}
            SetPlayerData @evPlayer "coin locked" true

            Countdown @evPlayer 15s "{$baseText}You can now open any keycard locked thing! (for %seconds% seconds)"
            Bypass @evPlayer true

            repeat 15
                Wait 1s

                if {@evPlayer role} != $initRole
                    ClearCountdown @evPlayer
                    break
                end
            end

            Bypass @evPlayer false
            SetPlayerData @evPlayer "coin locked" false
            stop
        end

        # role downgrade 
        if $effect == 6
            if {@evPlayer role} != "ClassD"
                SetRole @evPlayer ClassD None
            elif {@evPlayer role} != "Scp0492"
                SetRole @evPlayer Scp0492
            else
                SetRole @evPlayer Spectator
            end

            Broadcast @evPlayer 5s "{$baseText}Your role got downgraded!"
            stop
        end

        # funny cassie
        if $effect == 7
            SetPlayerData @evPlayer "coin locked" true

            Cassie noJingle "pitch_0.7 warning . pitch_3 XMAS_JINGLEBELLS" ""
            Broadcast @evPlayer 5s "{$baseText}Most useful cassie message sent!"

            Wait 7s
            SetPlayerData @evPlayer "coin locked" false
            stop
        end

        # change size
        if $effect == 8
            # set player size in every direction to a random number between 10% and 100%
            SetSize @evPlayer {RandomNum 0.1 1 real} {RandomNum 0.1 1 real} {RandomNum 0.1 1 real}

            Broadcast @evPlayer 5s "{$baseText}Your size has changed a little!"
            stop
        end

        # swap places 
        if $effect == 9
            # cant swap places if there arent at least 2 players
            if {AmountOf @alivePlayers} < 2
                Explode @evPlayer
                stop
            end

            # gets a random player that is not @evPlayer
            @swapPlayer = LimitPlayers {RemovePlayers * @evPlayer} 1
            $swapX = {@swapPlayer positionX}
            $swapY = {@swapPlayer positionY}
            $swapZ = {@swapPlayer positionZ}

            # we can teleport @swapPlayer directly to @evPlayer
            TPPlayer @swapPlayer @evPlayer 
            Broadcast @swapPlayer 5s "{$baseText}You have swapped places with {@evPlayer name}"

            # because @swapPlayer is in the same place as @evPlayer, we need to use the saved values to teleport
            TPPosition @evPlayer $swapX $swapY $swapZ
            Broadcast @evPlayer 5s "{$baseText}You have swapped places with {@swapPlayer name}"
            stop
        end
        """;
}