namespace SER.Code.Examples;

public class KillStreakScript : Example
{
    public override string Name => "killStreak";

    public override string Content =>
        """
        !-- OnEvent Death

        if {VarExists @evAttacker} is true
            # increment killer's streak
            if {HasPlayerData @evAttacker "streak"}
                $new = ({GetPlayerData @evAttacker "streak"} + 1)
            else
                $new = 1
            end

            # save the new kill streak
            SetPlayerData @evAttacker "streak" $new

            # announce milestone
            if $new is 5
                Broadcast @all 5s "{@evAttacker name} is on a KILLING SPREE (5 Kills)!"
            elif $new is 10
                Broadcast @all 5s "{@evAttacker name} is UNSTOPPABLE (10 Kills)!"
            end
        end

        if {VarExists @evPlayer} is true
            # reset the victim's streak
            SetPlayerData @evPlayer "streak" 0
        end
        """;
}