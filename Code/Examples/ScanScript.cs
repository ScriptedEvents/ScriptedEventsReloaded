using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class ScanScript : Example
{
    public override string Name => "scan";

    public override string Content =>
        """
        # scan beginning announcement
        Cassie jingle "a facility scan is commencing" "A facility scan is commencing..."

        Wait 1s
        WaitUntil ({IsCassieSpeaking} is false)

        repeat 3
            Wait 10s
            
            # scan sound
            Cassie noJingle "pitch_0.3 .G1 .G4" ""
        end

        # wait until cassie finishes
        Wait 10s

        # results
        $cassie = "scan complete . {AmountOf @scpPlayers} scpsubjects remaining . {AmountOf @classDPlayers} class d personnel remaining . {AmountOf @scientistPlayers} scientist personnel remaining . {AmountOf @foundationForcePlayers} foundation forces remaining . {AmountOf @chaosInsurgencyPlayers} hostileforces remaining . . . . ."
        $translation = "<br><size=30><color=red>[{AmountOf @scpPlayers}] SCP subjects</color><br><color=#FF8E00>[{AmountOf @classDPlayers}] Class-D personnel</color><br><color=#FFFF7C>[{AmountOf @scientistPlayers}] Scientist personnel</color><br><color=#70C3FF>[{AmountOf @foundationForcePlayers}] Foundation forces</color><br><color=#15853D>[{AmountOf @chaosInsurgencyPlayers}] Hostile forces</color>"
        Cassie jingle $cassie $translation
        """;
}