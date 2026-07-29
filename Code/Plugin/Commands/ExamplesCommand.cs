using CommandSystem;

namespace SER.Code.Plugin.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
public class ExamplesCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var summary = FileSystem.FileSystem.GenerateExamples();
        response =
            $"Example generation finished: {summary.Created} created, {summary.AlreadyExisted} already existed.\n" +
            $"Directory: {summary.DirectoryPath}\n" +
            "Examples start with '#', so they are disabled. Copy one or remove the leading '#', " +
            "then run 'serreload'. Both .ser and .txt files are supported.";
        return true;
    }

    public string Command => "serexamples";
    public string[] Aliases => [];
    public string Description => "Generates disabled example scripts and explains how to enable them.";
}
