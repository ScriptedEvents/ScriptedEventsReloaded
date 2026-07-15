using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Speaker_CreateGlobalMethod : SynchronousMethod
{
    public override string Description => "Creates a speaker to play audio through.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("speaker name"),
        new FloatArgument("volume", 0f)
        {
            DefaultValue = new(1f, "100%"),
            Description = "The volume of the audio."
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
            Args.GetText("speaker name"), 
            condition: condition,
            onIntialCreation: p =>
            {
                p.AddSpeaker("Main", Args.GetFloat("volume"), isSpatial: false, maxDistance: 5000f);
            }
        );
    }
}