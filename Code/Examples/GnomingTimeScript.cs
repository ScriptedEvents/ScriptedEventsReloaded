using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
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

        # lower size by .1 when killing someone
        SetSize @plr ({@plr sizeX} - .1) ({@plr sizeY} - .1) ({@plr sizeZ} - .1)
        Hint @plr 5s "KILLED PLAYER - IT'S GNOMING TIME!"

        Wait 15s

        # return them to normal
        SetSize @plr ({@plr sizeX} + .1) ({@plr sizeY} + .1) ({@plr sizeZ} + .1)
        Hint @plr 5s "Gnoming potential depleted :("
        """;
}