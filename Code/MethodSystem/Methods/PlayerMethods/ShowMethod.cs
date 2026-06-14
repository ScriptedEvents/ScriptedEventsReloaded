using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem.PropertySystem;

namespace SER.Code.MethodSystem.Methods.PlayerMethods;

[UsedImplicitly]
public class ShowMethod : ReturningMethod<TextValue>, ICanError
{
    public override string Description => "Formats provided players into a nice text representation.";

    public string[] ErrorReasons =>
    [
        "Provided property returned a value which cannot be displayed using text."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new EnumArgument<PlayerValue.PlayerProperty>("property")
        {
            DefaultValue = new(PlayerValue.PlayerProperty.Name, "name"),
            Description = "The property which will be displayed."
        },
        new TextArgument("separator")
        {
            DefaultValue = new(", ", "\", \"")
        },
        new BoolArgument("separator on beginning")
        {
            Description = "Whether to add the separator at the beginning of the returned text",
            DefaultValue = new(false, null)
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var property = Args.GetEnum<PlayerValue.PlayerProperty>("property");

        if (!PlayerValue.PropertyInfoMap.TryGetValue(property, out var propInfo) ||
            propInfo is not IValueWithProperties.PropInfo<Player> { ReturnType: var type, Func: var handler } ||
            !typeof(LiteralValue).IsAssignableFrom(type.Type)) 
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }

        var separator = Args.GetText("separator");
        var value = players
            .Select(handler)
            .OfType<LiteralValue>()
            .Select(lv => lv.StringRep)
            .JoinStrings(separator);

        if (Args.GetBool("separator on beginning") && value.Length > 0)
        {
            value = separator + value;
        }
        
        ReturnValue = value.ToStaticTextValue();
    }
}