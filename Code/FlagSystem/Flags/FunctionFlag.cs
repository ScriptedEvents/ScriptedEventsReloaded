using SER.Code.FlagSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.FlagSystem.Flags;

public class FunctionFlag : Flag, IMajorBehaviorFlag
{
    private readonly List<VariableToken> _expectedVarTokens = [];
    private string? _functionName;

    public IReadOnlyCollection<VariableToken> ExpectedVarTokens => _expectedVarTokens;
    public string? FunctionName => _functionName;
    
    public override string Description =>
        "Requires this script to be executed only when required arguments are supplied.";

    public override Argument? InlineArgument => new(
        "name",
        "The unique name used to call this function with RunFunc.",
        args =>
        {
            if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                return "Function requires exactly one name.";
            }

            _functionName = args[0];
            return true;
        },
        false,
        "!-- Function HandleDoor");

    public override Argument[] Arguments =>
    [
        new(
            "argument", 
            "The variable that has to be present in order for this script to execute.",
            args =>
            {
                switch (args.Length)
                {
                    case < 1: return "Argument requires a variable.";
                    case > 2: return "Argument expects only a single variable.";
                }

                if (BaseToken.TryParse<VariableToken>(args.First(), null!)
                    .HasErrored(out var error, out var token))
                {
                    return error;
                }
                
                _expectedVarTokens.Add(token);
                return true;
            },
            true,
            "-- argument $name"
        )
    ];
    
    public override Result OnScriptRunning(Script scr, out bool mustReport)
    {
        mustReport = true;
        if (base.OnScriptRunning(scr, out _).HasErrored(out var error)) return error;
        
        foreach (var token in _expectedVarTokens)
        {
            // A missing variable comes back as null; report it instead of reading from it.
            if (scr.LocalVariables.FirstOrDefault(var => var.Name == token.Name) is not { } variable)
            {
                return $"Variable '{token.Name}' was not provided to the script.";
            }

            if (!token.ValueType.CanHold(variable.BaseValue.Type))
            {
                return $"Variable '{token.Name}' is not of the correct type. " +
                       $"Expected '{token.ValueType}', got '{variable.BaseValue.Type}'.";
            }
        }

        return true;
    }

    public override void Unbind()
    {
    }
}
