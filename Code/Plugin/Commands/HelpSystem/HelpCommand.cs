using CommandSystem;

namespace SER.Code.Plugin.Commands.HelpSystem;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class HelpCommand : ICommand
{
    public string Command => MainPlugin.HelpCommandName;
    public string[] Aliases => [];
    public string Description => "The help command of SER.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count > 0)
        {
            return DocsProvider.GetGeneralOutput(arguments, sender, out response);
        }

        response = DocsProvider.GetOptionsList();
        return true;
    }
}











