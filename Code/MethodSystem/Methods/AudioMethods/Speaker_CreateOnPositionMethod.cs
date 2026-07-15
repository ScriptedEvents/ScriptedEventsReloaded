using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Speaker_CreateOnPositionMethod : SynchronousMethod
{
    public override string Description => "Creates a speaker in a specified XYZ position.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name"),
        new FloatArgument("x"),
        new FloatArgument("y"),
        new FloatArgument("z"),
        new FloatArgument("volume", 0f)
        {
            DefaultValue = new(1f, "100%"),
            Description = "The volume of the audio."
        },
        new FloatArgument("min distance", 0f)
        {
            DefaultValue = new(5f, null),
            Description = "The minimum distance for full-volume audio."
        },
        new FloatArgument("max distance", 0f)
        {
            DefaultValue = new(15f, null),
            Description = "The maximum audible distance for the audio."
        },
        new BoolArgument("is stereo")
        {
            DefaultValue = new(true, null),
            Description = "Whether the audio will be 3D."
        },
        ..Argument.PlayersArgumentUpdating(
            "target players",
            playerDescription: "only these players will hear the audio.",
            playerDefault: new(null, "everyone"),
            updateDefault: new(false, "no update")
        )
    ];

    public override void Execute()
    {
        var speakerName = Args.GetText("speaker name");
        var x = Args.GetFloat("x");
        var y = Args.GetFloat("y");
        var z = Args.GetFloat("z");
        var volume = Args.GetFloat("volume");
        var minDistance = Args.GetFloat("min distance");
        var maxDistance = Args.GetFloat("max distance");
        var isStereo = Args.GetBool("is stereo");
        var targetPlayers = Args.GetPlayers("target players").MaybeNull();
        
        Func<ReferenceHub, bool>? condition;
        if (targetPlayers is not null && Args.GetBool("update target players"))
        {
            var func = Args.GetPlayersFunc("target players");
            condition = p => func().Contains(Player.Get(p));
        }
        else if (targetPlayers is not null)
        {
            condition = plr => targetPlayers.Contains(Player.Get(plr));
        }
        else
        {
            condition = null;
        }
        
        AudioPlayer.Create(
            speakerName, 
            condition: condition,
            onIntialCreation: p =>
            {        
                var speaker = p.AddSpeaker("Main", volume, isStereo, minDistance, maxDistance);
                speaker.Position = new(x, y, z);
            }
        );
    }
}