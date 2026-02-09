using RueI.API;
using RueI.API.Elements;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.RueiMethods;

public class RueHintVisibility : SynchronousMethod, IDependOnFramework
{
    public override string Description { get; } = "Manages the visibility of already created hints of player";
    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument("players")
        {
            Description = "The players that will have hint shown/removed",
        },
        new TextArgument("Id")
        {
            Description = "Id required for the hint (if same id will be shown again it will override the last hint)", 
        },
        new BoolArgument("IsVisible")
        {
            DefaultValue = new (true, null),
            Description = "Sets the visibility of hint if Option set to Visibility"
        },
    ];
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var id = Args.GetText("Id");
        var tag = new Tag(id);
        var isVisible = Args.GetBool("IsVisible");

        foreach (var player in players)
        {
            var display = RueDisplay.Get(player);
            display.SetVisible(tag, isVisible);
        }
    }

    public IDependOnFramework.Type DependsOn { get; } = IDependOnFramework.Type.Ruei;
}