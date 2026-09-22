using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.PropertySystem;
using SER.Code.VariableSystem;
using Events = SER.Code.EventSystem.EventHandler;

namespace SER.Code.Plugin.Commands.HelpSystem;

public static class SerSymbolCatalogue
{
    private static string TypeId(Type type) => type.Assembly.GetName().Name + ":" + type.FullName;

    /// <summary>Read after host registry initialization, on its registry thread. No getters or handlers are invoked.</summary>
    public static IReadOnlyList<SerSymbol> Capture()
    {
        var symbols = new List<SerSymbol>();
        var available = MethodIndex.GetMethods();
        foreach (var method in available.Concat(MethodIndex.FrameworkDependentMethods.Values.SelectMany(m => m)).Distinct())
        {
            var requirement = method is IDependOnFramework dependency ? dependency.DependsOn.ToString() : null;
            symbols.Add(new SerSymbol("method:" + TypeId(method.GetType()), SerSymbolKind.Method, method.Name,
                method.Description, method.Name, RequiresIntegration: requirement, Available: available.Contains(method),
                Aliases: Array.AsReadOnly(method is IHasAliases aliases ? aliases.Aliases.ToArray() : Array.Empty<string>())));
        }
        foreach (var ev in Events.AvailableEvents.Concat(Events.AvailablePmerEvents).Concat(Events.AvailableUcrEvents).Distinct())
        {
            var requirement = Events.IsPmerEvent(ev) ? FrameworkBridge.Type.ProjectMapEditorReborn
                : Events.IsUcrEvent(ev) ? FrameworkBridge.Type.UncomplicatedCustomRoles : (FrameworkBridge.Type?)null;
            var isAvailable = requirement is null || FrameworkBridge.Found.Any(f => f.Type == requirement);
            var id = "event:" + TypeId(ev.DeclaringType!) + ":" + ev.Name;
            symbols.Add(new SerSymbol(id, SerSymbolKind.Event, ev.Name, Events.GetEventDescription(ev) ?? "",
                ev.Name, RequiresIntegration: requirement?.ToString(), Available: isAvailable));
            foreach (var variable in Events.GetMimicVariableInfo(ev))
                symbols.Add(new SerSymbol(id + ":variable:" + variable.Name, SerSymbolKind.EventVariable,
                    variable.Name, variable.Type + ". " + variable.Description, ev.Name, ev.Name,
                    requirement?.ToString(), isAvailable));
        }
        foreach (var type in EnumIndex.GetAllEnums().Distinct())
            symbols.Add(new SerSymbol("enum:" + TypeId(type), SerSymbolKind.Enum, type.Name,
                "Values: " + string.Join(", ", Enum.GetNames(type)), type.Name));
        foreach (var keyword in ContextableKeywordToken.KeywordContexts)
            symbols.Add(new SerSymbol("keyword:" + keyword.KeywordName, SerSymbolKind.Keyword,
                keyword.KeywordName, keyword.Description, keyword.KeywordName));
        foreach (var pair in Flag.FlagInfos)
        {
            var flag = (Flag)Activator.CreateInstance(pair.Value)!;
            symbols.Add(new SerSymbol("flag:" + pair.Key, SerSymbolKind.Flag, pair.Key, flag.Description, pair.Key));
        }
        foreach (var variable in VariableIndex.GetGlobalVariableInfo())
            symbols.Add(new SerSymbol("variable:" + variable.Prefix + variable.Name, SerSymbolKind.Variable,
                variable.Prefix + variable.Name, variable.Category ?? "Global variable", "variables"));

        var basic = new (string Name, IValueWithProperties Value)[] {
            ("player", new PlayerValue()), ("collection", new CollectionValue()), ("number", new NumberValue()),
            ("text", new StaticTextValue()), ("bool", new BoolValue()), ("color", new ColorValue()), ("duration", new DurationValue()) };
        foreach (var entry in basic) AddProperties(entry.Name, entry.Name, entry.Value.Properties);
        foreach (var type in ReferencePropertyRegistry.GetRegisteredTypes().ToArray())
            AddProperties(TypeId(type), type.Name, ReferencePropertyRegistry.GetProperties(type));

        return Array.AsReadOnly(symbols.OrderBy(s => s.Id, StringComparer.Ordinal).ToArray());

        void AddProperties(string identity, string owner, IReadOnlyDictionary<string, IValueWithProperties.PropInfo> properties)
        {
            foreach (var pair in properties)
                symbols.Add(new SerSymbol("property:" + identity + ":" + pair.Key, SerSymbolKind.Property,
                    pair.Key, pair.Value.Description ?? "Property of " + owner, "properties " + owner, owner));
        }
    }
}
