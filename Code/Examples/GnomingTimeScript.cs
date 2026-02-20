namespace SER.Code.Examples;

public class GnomingTimeScript : Example
{
    public override string Name => "gnomingTime";

    public override string Content =>
        """
        !-- OnEvent Death

        if {VarExists @evAttacker} is false
            stop
        end

        @plr = @evAttacker

        # lower scale by .1 when killing someone
        SetSize @plr ({@plr scaleX} - .1) ({@plr scaleY} - .1) ({@plr scaleZ} - .1)
        Hint @plr 5s "KILLED PLAYER - IT'S GNOMING TIME!"

        Wait 15s

        # return them to normal
        SetSize @plr ({@plr scaleX} + .1) ({@plr scaleY} + .1) ({@plr scaleZ} + .1)
        Hint @plr 5s "Gnoming potential depleted :("
        """;
}