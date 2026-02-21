namespace SER.Code.Examples;

public class PinataLootScript : Example
{
    public override string Name => "pinataLoot";

    public override string Content =>
        """
        !-- OnEvent Hurt

        # Only trigger if the victim exists
        if {VarExists @evPlayer} is false
            stop
        end
        
        # Only trigger if the victim is SCP-173
        if {@evPlayer role} isnt "Scp173"
            stop
        end

        # Trigger only with 5% chance
        if {Chance 5%} is false
            stop
        end

        # 80% to get pills, 20% to get medkit
        if {Chance 80%} is true
            *pickup = CreatePickup Painkillers
        else
            *pickup = CreatePickup Medkit
        end

        SpawnPickupPlayer *pickup @evPlayer
        
        if {VarExists @evAttacker} is true
            Hint @evAttacker 3s "You hit the piñata!"
        end
        """;
}