using RueI.API;
using RueI.API.Elements;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using YamlDotNet.Core.Tokens;
using Tag = RueI.API.Elements.Tag;

namespace SER.Code.MethodSystem.Methods.RueiMethods;

public class RueHintMethod : SynchronousMethod, IDependOnFramework
{
    public override string Description { get; } = "Sends or removes hints (in Rue library) of players";
    public override Argument[] ExpectedArguments { get; } = 
        [
            new PlayersArgument("players")
            {
                Description = "The players that will have hint shown/removed",
            },
            new OptionsArgument("Modified option", 
                new Option("Show", "Shows the player hint with certain tag text and position"), 
                new Option("Remove", "Removes hint with certain id, doesn't require arguments after id.")
                )
            {
                Description  = "Main option argument required."
            },
            new TextArgument("Id")
            {
                Description = "Id required for the hint (if same id will be shown again it will override the last hint)", 
            },
            new TextArgument("Message")
            {
                DefaultValue = new(""),
                Description = "The message of hint shown, optional in case Option is set to Remove.",
            },
            new FloatArgument("Position", 0, 1000)
            {
                DefaultValue  = new(0f),
                Description = "The position of hint (Y position), optional in case Option is set to Remove."
            },
            new DurationArgument("Duration")
            {
                DefaultValue = new(TimeSpan.MaxValue),
                Description = "The duration of hint, optional in case Option is set to Remove."
            },
            new BoolArgument("UseResolution")
            {
                DefaultValue = new(false),
                Description = "Automatically formats hint on the Y position to use resolution player sets (option) (X resolution is unable to be calculated)",
            },
        ];
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var option = Args.GetOption("Modified option");
        var id = Args.GetText("Id");
        var tag = new Tag(id);
        var message = Args.GetText("Message");
        var position = Args.GetFloat("Position");
        var duration = Args.GetDuration("Duration");
        var resolution = Args.GetBool("UseResolution");
        
        foreach (var plr in players)
        {
            var display = RueDisplay.Get(plr);
            if (option == "remove")
            {
                display.Remove(tag);
                continue;
            }

            if (option == "show")
            {
                display.Show(tag, new BasicElement(position, message) {ResolutionBasedAlign = resolution}, duration);
            }
        }
    }
    public IDependOnFramework.Type DependsOn { get; } = IDependOnFramework.Type.Ruei;
}

