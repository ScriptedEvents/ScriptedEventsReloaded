using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class BreachScript : Example
{
    public override string Name => "breach";

    public override string Content =>
        """
        !-- CustomCommand breach
        -- availableFor Server RemoteAdmin

        Cassie jingle "Containment breach detected . All heavy containment doors locked ." ""

        CloseDoor HeavyContainment
        LockDoor HeavyContainment

        Wait 30s

        Cassie jingle "Lockdown lifted ." ""
        UnlockDoor HeavyContainment
        """;
}