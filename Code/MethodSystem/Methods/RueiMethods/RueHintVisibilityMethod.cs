using RueI.API;
using RueI.API.Elements;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.RueiMethods;

public class RueHintVisibilityMethod : SynchronousMethod, IDependOnFramework
{
    public override string Description => "Manages the visibility of already created hints of player";
    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Ruei;

    public const string Visibility = "is hint visible";
    
    public override Argument[] ExpectedArguments { get; } = 
    [
        new PlayersArgument(RueHintMethod.Players)
        {
            Description = "The players that will have hint change visibility.",
        },
        new TextArgument(RueHintMethod.Id)
        {
            Description = "Id required for the hint (if same id will be shown again it will override the last hint)", 
        },
        new BoolArgument(Visibility)
        {
            DefaultValue = new (true, "true"),
            Description = "Sets the visibility of hint if Option set to Visibility"
        },
    ];
    public override void Execute()
    {
        var players = Args.GetPlayers(RueHintMethod.Players);
        var id = Args.GetText(RueHintMethod.Id);
        var tag = new Tag(id);
        var isVisible = Args.GetBool(Visibility);

        foreach (var player in players)
        {
            var display = RueDisplay.Get(player);
            display.SetVisible(tag, isVisible);
        }
    }
}