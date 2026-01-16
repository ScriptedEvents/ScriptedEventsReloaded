using JetBrains.Annotations;
using RemoteAdmin;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.MethodSystem.MethodDescriptors;
using Console = GameCore.Console;

namespace SER.Code.MethodSystem.Methods.ServerMethods;

[UsedImplicitly]
public class CommandMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Runs a server command with full permission.";

    public string AdditionalDescription
        => "This action executes commands as the server. Therefore, the command needs '/' before it if it's a RA " +
           "command, or '.' before it if it's a console command.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("command"),
        new PlayerArgument("sender")
        {
            DefaultValue = new(null, "unspecified") 
        }
    ];

    public override void Execute()
    {
        var sender = Args.GetPlayer("sender").MaybeNull();
        Console.Singleton.TypeCommand(
            Args.GetText("command"), 
            sender is not null 
                ? new PlayerCommandSender(sender.ReferenceHub) 
                : null);
    }
}