using MEC;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.DiscordMethods.Structures;
using SER.Code.MethodSystem.Methods.HTTPMethods;

namespace SER.Code.MethodSystem.Methods.DiscordMethods;

public class SendDiscordEmbedsMethod : SynchronousMethod
{
    public override string Description => "tes";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("url"),
        new ReferenceArgument<DEmbed>("embed")
    ];
    
    public override void Execute()
    {
        var url = Args.GetText("url");
        var embed = Args.GetReference<DEmbed>("embed");
        
        HTTPPostMethod.SendPost(this, url, embed.ToString()).RunCoroutine();
    }
}