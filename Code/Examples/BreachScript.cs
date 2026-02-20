namespace SER.Code.Examples;

public class BreachScript : Example
{
    public override string Name => "breach";

    public override string Content =>
        """
        !-- CustomCommand breach

        Cassie jingle "Containment breach detected . All heavy containment doors locked ." ""

        CloseDoor HeavyContainment
        LockDoor HeavyContainment 30s

        Wait 30s

        Cassie jingle "Lockdown lifted ." ""
        UnlockDoor HeavyContainment
        """;
}