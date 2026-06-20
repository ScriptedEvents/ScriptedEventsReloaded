using SER.Code.FlagSystem.Structures;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.FlagSystem.Flags;

public class FunctionFlag : Flag, IMajorBehaviorFlag
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
            true,
            "-- argument $name"
        )
    ];
    
    public override OldResult OnScriptRunning(Script scr, out bool mustReport)
    {
        mustReport = true;
        if (base.OnScriptRunning(scr, out _).HasErrored(out var error)) return error;
        
        (VariableToken token, Variable var)[] provided = _expectedVarTokens
            .Select(token => (token, scr.LocalVariables.FirstOrDefault(var => var.Name == token.Name)))
            .OfType<(VariableToken, Variable)>()
            .ToArray();

        var missingVariable = _expectedVarTokens
            .FirstOrDefault(token => !provided.Select(r => r.token).Contains(token));
        
        if (missingVariable is not null)
        {
            return $"Variable '{missingVariable.Name}' was not provided to the script.";
        }

        (VariableToken token, Variable var) mismatchedTypeVar = provided
            .Select(x => (x.token, x.var))
            .FirstOrDefault(x => !x.token.ValueType.CanHold(x.var.BaseValue.Type));

        if (mismatchedTypeVar is { token: {} t, var: {} v })
        {
            return $"Variable '{t.Name}' is not of the correct type. " +
                   $"Expected '{v.BaseValue.Type}', got '{t.ValueType}'.";
        }
        
        return true;
    }

    public override void Unbind()
    {
        _expectedVarTokens.Clear();
    }
}