using System.Collections.Generic;
using System.Linq;
using SER.FlagSystem.Structures;
using SER.TokenSystem;
using SER.TokenSystem.Tokens;
using SER.TokenSystem.Tokens.VariableTokens;

namespace SER.FlagSystem.Flags;

public class FunctionFlag : Flag
{
    private List<VariableToken> _variables = [];
    
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
                    case < 1: return "Argument requires the variable name,";
                    case > 2: return "Argument expects only a single variable.";
                }

                if (BaseToken.TryParse<VariableToken>(args.First(), null!).HasErrored(out var error, out var token))
                {
                    return error;
                }
                
                _variables.Add(token);
                return true;
            },
            true,
            true
        )
    ];
    
    public override void FinalizeFlag()
    {
        throw new System.NotImplementedException();
    }

    public override void Unbind()
    {
        throw new System.NotImplementedException();
    }
}