using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.PlayerVariableMethods;

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
        new EnumArgument<PlayerExpressionToken.PlayerProperty>("property")
        {
            DefaultValue = new(PlayerExpressionToken.PlayerProperty.Name, "name"),
            Description = "The property which will be displayed."
        }
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var property = Args.GetEnum<PlayerExpressionToken.PlayerProperty>("property");

        if (!PlayerExpressionToken.PropertyInfoMap.TryGetValue(property, out var propInfo) ||
            propInfo is not { ReturnType: var type, Handler: var handler } ||
            !typeof(LiteralValue).IsAssignableFrom(type.Type)) 
        {
            throw new ScriptRuntimeError(this, ErrorReasons[0]);
        }

        ReturnValue = players
            .Select(handler)
            .OfType<LiteralValue>()
            .Select(lv => lv.StringRep)
            .JoinStrings(", ");
    }
}