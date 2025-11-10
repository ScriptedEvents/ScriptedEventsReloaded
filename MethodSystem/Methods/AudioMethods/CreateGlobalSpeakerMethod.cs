using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.AudioMethods;

public class CreateGlobalSpeakerMethod : SynchronousMethod
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
        new PlayersArgument("target players")
        {
            DefaultValue = new(null, "all"),
            Description = "If specified, only the provided players will hear the audio."
        }
    ];

    public override void Execute()
    {
        var targetPlayers = Args.GetPlayers("target players");
        
        AudioPlayer.Create(
            Args.GetText("speaker name"), 
            condition: hub =>
            {
                if (targetPlayers is null) return true;
                
                return targetPlayers.Any(p => p.ReferenceHub == hub);
            },
            onIntialCreation: p =>
            {
                p.AddSpeaker("Main", Args.GetFloat("volume"), isSpatial: false, maxDistance: 5000f);
            }
        );
    }
}