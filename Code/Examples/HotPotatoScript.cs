using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class HotPotatoScript : Example
{
    public override string Name => "hotPotato";

    public override string Content =>
        """
        !-- OnEvent RoundStarted

        forever
            Wait 1m

            # Get a random player from the alive players
            @potatoCarrier = LimitPlayers @alivePlayers 1

            # if no player is alive, continue to next attempt
            if {AmountOf @potatoCarrier} is 0
                continue
            end
            
            Hint @potatoCarrier 3s "YOU HAVE THE HOT POTATO! DROP IT OR DIE!"
            GiveItem @potatoCarrier GunA7

            Wait 3s

            # Check if they still have the item (GunA7) in their inventory
            over {@potatoCarrier inventory}
                with *item

                if {ItemInfo *item type} isnt "GunA7"
                    continue
                end

                AdvDestroyItem *item
                Explode @potatoCarrier
                Broadcast @all 5s "{@potatoCarrier name} failed the Hot Potato!"
                stop
            end
                
            Broadcast @all 5s "The Hot Potato has been neutralized... for now."
        end
        """;
}