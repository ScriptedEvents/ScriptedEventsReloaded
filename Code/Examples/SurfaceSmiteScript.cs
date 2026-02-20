using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class SurfaceSmiteScript : Example
{
    public override string Name => "surfaceSmite";

    public override string Content =>
        """
        !-- CustomCommand surfacesmite
        -- availableFor RemoteAdmin
        -- description "Kills a random player currently in the Surface Zone."

        # 1. Get all players on the surface and limit the selection to 1 random person.
        @target = LimitPlayers @surfacePlayers 1

        # 2. Check if anyone was actually found to avoid a runtime error.
        if {AmountOf @target} > 0
            # 3. Execute the kill with a custom reason.
            Kill @target "The surface is no longer safe."
            Broadcast @sender 5s "Eliminated {@target nickname} from the surface."
        else
            Broadcast @sender 5s "No players found in the Surface Zone."
        end
        """;
}