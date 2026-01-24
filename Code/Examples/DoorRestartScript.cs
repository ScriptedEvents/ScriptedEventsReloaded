using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class DoorRestartScript : Example
{
    public override string Name => "doorRestart";

    public override string Content =>
        """
        # initial cassie announcement
        Cassie jingle "ATTENTIONALLPERSONNEL . DOOR CONTROL CONSOLE MALFUNCTION DETECTED . INITIALIZING REACTIVATION SEQUENCE . ATTEMPTING FULL SYSTEM REACTIVATION IN . 3 . 2 . 1" "Attention all personnel. Door control console malfunction detected.<split>Initializing reactivation sequence. Attempting full system reactivation in..."

        # wait for cassie to finish before restarting doors
        Wait 1s
        WaitUntil ({IsCassieSpeaking} is false)

        # restart effects
        LightsOut * 15s
        CloseDoor *
        LockDoor * NoPower

        # cassie for spooky effect
        repeat 2
            Cassie noJingle "pitch_{RandomNum 0.15 0.25 real} .g{RandomNum 1 6 int}"
        end

        # duration of the restart
        Wait 15s

        # revert to unlocked
        UnlockDoor *

        # cassie information that restart was successful
        ClearCassie
        Cassie jingle "DOOR CONTROL CONSOLE SUCCESSFULLY ACTIVATED . SYSTEM IS BACK IN OPERATIONAL MODE" "Door control console successfully activated.<split>System is back in operational mode."
        """;
}