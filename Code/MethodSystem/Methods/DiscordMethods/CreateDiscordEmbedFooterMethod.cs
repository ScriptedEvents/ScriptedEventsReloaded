using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.DiscordMethods.Structures;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

[UsedImplicitly]
public class CreateDiscordEmbedFooterMethod : ReferenceReturningMethod<DEmbedFooter>
{
    public override string Description => "Creates a footer that can be used in discord embeds.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("content"),
        new TextArgument("icon url")
    ];
    
    public override void Execute()
    {
        ReturnValue = new(new()
        {
            ["text"] = Args.GetText("content"),
            ["icon_url"] = Args.GetText("icon url")
        });
    }
}