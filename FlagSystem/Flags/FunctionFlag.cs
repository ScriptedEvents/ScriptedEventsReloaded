using System.Collections.Generic;
using System.Linq;
using SER.FlagSystem.Structures;
using SER.Helpers.Extensions;
using SER.Helpers.ResultSystem;
using SER.ScriptSystem;
using SER.TokenSystem.Tokens;
using SER.TokenSystem.Tokens.VariableTokens;

namespace SER.FlagSystem.Flags;

public class FunctionFlag : Flag
{
    private List<VariableToken> _expectedVariables = [];
    
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
                
                _expectedVariables.Add(token);
                return true;
            },
            true,
            true
        )
    ];
    
    public override Result OnScriptRunning(Script scr)
    {
        var notIncluded = _expectedVariables.FirstOrDefault(
            token => scr.Variables.All(variable => variable.Name != token.Name && variable.GetType() != token.VariableType));

        if (notIncluded is not null)
        {
            return $"{notIncluded.VariableType.FriendlyTypeName()} '{notIncluded.Name}' was not provided to the script.";
        }
        
        return true;
    }

    public override void Unbind()
    {
        _expectedVariables.Clear();
    }
}