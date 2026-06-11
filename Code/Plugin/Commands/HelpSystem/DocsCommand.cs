using System.Text;
using CommandSystem;
using LabApi.Features.Permissions;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem;
using SER.Code.Plugin.Commands.Interfaces;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.Plugin.Commands.HelpSystem;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
[UsedImplicitly]
public class DocsCommand : ICommand, IUsePermissions
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission(Permission))
        {
            response = "You do not have permission to create documentation.";
            return false;
        }
        
        var methodSb = GetBuilder();
        methodSb.AppendLine(DocsProvider.GetMethodList());
        methodSb.AppendLine();
        
        foreach (var method in MethodIndex.GetMethods())
        {
            methodSb.AppendLine(DocsProvider.GetMethodHelp(method));
            methodSb.AppendLine();
        }

        MakeDocFile("methods", methodSb);
        
        var variableSb = GetBuilder();
        variableSb.Append(DocsProvider.GetVariableList());
        
        MakeDocFile("variables", variableSb);
        
        var eventSb = GetBuilder();
        foreach (var @event in EventSystem.EventHandler.AvailableEvents)
        {
            eventSb.AppendLine(DocsProvider.GetEventInfo(@event));
            eventSb.AppendLine();
        }
        
        MakeDocFile("events", eventSb);
        
        var keywordSb = GetBuilder();
        keywordSb.AppendLine(DocsProvider.GetKeywordHelpPage());
        keywordSb.AppendLine();
        
        foreach (var keyword in ContextableKeywordToken.KeywordContextTypes)
        {
            keywordSb.AppendLine(DocsProvider.GetKeywordInfo(keyword.CreateInstance<IKeywordContext>()));
            keywordSb.AppendLine();
        }
        
        MakeDocFile("keywords", keywordSb);
        
        var enumSb = GetBuilder();
        enumSb.AppendLine(DocsProvider.GetEnumHelpPage());
        enumSb.AppendLine();
        
        foreach (var @enum in EnumIndex.GetNonReflectedEnums())
        {
            enumSb.AppendLine(DocsProvider.GetEnum(@enum));
            enumSb.AppendLine();
        }
        
        MakeDocFile("enums", enumSb);
        
        var propertySb = GetBuilder();
        foreach (var type in ReferencePropertyRegistry.GetRegisteredTypes())
        {
            if (DocsProvider.GetPropertiesForType(type.AccurateName, out var resp))
            {
                propertySb.AppendLine(resp);
            }
        }
        
        MakeDocFile("properties", propertySb);
        
        var sb = GetBuilder();
        foreach (var helpOption in Enum.GetValues(typeof(HelpOption)).Cast<HelpOption>())
        {
            if (!DocsProvider.GeneralOptions.TryGetValue(helpOption, out var generalOption))
            {
                throw new AndrzejFuckedUpException(
                    $"Option {helpOption} is not registered in the help command.");
            }
            
            sb.AppendLine($"===== {helpOption} =====");
            sb.AppendLine(generalOption());
            sb.AppendLine();
        }

        MakeDocFile("general", sb);
        
        response = "Documentation successfully generated! Check the 'Documentation' folder in the SER directory.";
        return true;
    }

    public string Command => "serdocs";
    public string[] Aliases => [];
    public string Description => "Generates documentation for the plugin.";
    public string Permission => "ser.docs";

    private static StringBuilder GetBuilder()
    {
        return new StringBuilder($"Genrated on [{DateTime.Today.ToLongDateString()}] with SER version [{MainPlugin.Instance.Version}]\n\n");
    }

    private static void MakeDocFile(string type, StringBuilder content)
    {
        var folder = Path.Combine(FileSystem.FileSystem.MainDirPath, "Documentation");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        
        using var sw = File.CreateText(Path.Combine(folder, $"#{type}.txt"));
        sw.Write(content.ToString());
    }
}