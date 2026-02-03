using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.FlagSystem.Flags;

public class FunctionFlag : Flag
{
    private readonly List<VariableToken> _expectedVarTokens = [];
    public IReadOnlyCollection<VariableToken> ExpectedVarTokens => _expectedVarTokens;
    
    public override string Description =>
        "Requires this script to be executed only when required arguments are supplied.";

    public override Argument? InlineArgument => null;

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
            true
        )
    ];
    
    public override Result OnScriptRunning(Script scr)
    {
        (VariableToken token, Variable var)[] provided = _expectedVarTokens
            .Select(token => (token, scr.Variables.FirstOrDefault(var => var.Name == token.Name)))
            .OfType<(VariableToken, Variable)>()
            .ToArray();

        var missingVariable = _expectedVarTokens
            .FirstOrDefault(token => !provided.Select(r => r.token).Contains(token));
        
        if (missingVariable is not null)
        {
            return $"Variable '{missingVariable.Name}' was not provided to the script.";
        }

        (VariableToken, Type) mismatchedTypeVar = provided
            .Select(x => (x.token, x.var.GetType()))
            .FirstOrDefault(x => x.token.VariableType != x.Item2);

        if (mismatchedTypeVar is { Item1: {} varToken, Item2: {} type })
        {
            return $"Variable '{varToken.Name}' is not of the correct type. " +
                   $"Expected '{type.FriendlyTypeName()}', got '{varToken.ValueType.FriendlyTypeName()}'.";
        }
        
        return true;
    }

    public override void Unbind()
    {
        _expectedVarTokens.Clear();
    }
}