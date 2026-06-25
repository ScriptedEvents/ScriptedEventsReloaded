using System.Globalization;
using System.Text;
using NCalc;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.Interfaces;

namespace SER.Code.Helpers;

public static class NumericExpressionReslover
{
    public static OldResult IsValidForExpression(BaseToken token)
    {
        if (ParseToken(token, new(), new(), "Expression is invalid.", 0)
            .HasErrored(out var err))
        {
            return err;
        }
        
        return true;
    }
    
    public static OldTryGet<CompiledExpression> CompileExpression(string expression, Script scr)
    {
        OldResult mainErr = $"Condition '{expression}' is invalid.";
        
        if (Tokenizer.TokenizeLine(expression, scr, null)
            .HasErrored(out var err, out var tokens))
        {
            return mainErr + err;
        }
        
        return CompileExpression(tokens.ToArray());
    }

    public static OldTryGet<CompiledExpression> CompileExpression(BaseToken[] tokens)
    {
        var initial = tokens.Select(t => t.RawRep).JoinStrings(" ");
        OldResult mainErr = $"Expression '{initial}' is invalid.";

        var variables = new Dictionary<string, (OldDynamicTryGet<object>, string)>(StringComparer.OrdinalIgnoreCase);
        var sb = new StringBuilder();
        uint tempId = 0;

        foreach (var token in tokens)
        {
            tempId++;
            Log.D($"parsing token {token.RawRep} ({token.GetType().AccurateName})");
            
            if (ParseToken(token, variables, sb, mainErr, tempId).HasErrored(out var err))
            {
                return mainErr + err;
            }
        }
        
        var expression = new Expression(sb.ToString());

        // Now we have the expression string and a variables dictionary.
        return new CompiledExpression(
            expression, 
            variables, 
            initial
        );
    }

    public class CompiledExpression
    {
        private readonly Expression _expression;
        private readonly Dictionary<string, (OldDynamicTryGet<object> value, string repr)> _parameters;
        private readonly string _rawRepresentation;
        private readonly Dictionary<string, object> _values = [];

        public CompiledExpression(
            Expression expression, 
            Dictionary<string, (OldDynamicTryGet<object>, string)> parameters,
            string rawRepresentation
        )
        {
            _expression = expression;
            _parameters = parameters;
            _rawRepresentation = rawRepresentation;
            _expression.Parameters = _values;
        }

        public OldTryGet<object> Evaluate()
        {
            _values.Clear();
            foreach (var (key, (getter, _)) in _parameters)
            {
                if (getter.Invoke().HasErrored(out var err, out var value))
                {
                    return err;
                }
                
                _values[key] = value;
            }
            
            try
            {
                return _expression.Evaluate() ?? throw new Exception();
            }
            catch (Exception)
            {
                if (_values.Count <= 0)
                {
                    return $"Expression '{_rawRepresentation}' is invalid.";
                }
                
                return $"Expression '{_rawRepresentation}' is invalid. Values used:\n" +
                       _values.Select((kvp, _) => $"- {_parameters[kvp.Key].repr} = {kvp.Value}")
                           .JoinStrings("\n");
            }
        }
    }

    private static OldResult ParseToken(
        BaseToken token,
        Dictionary<string, (OldDynamicTryGet<object> value, string serRepr)> variables,
        StringBuilder sb, 
        OldResult mainErr,
        uint tempId)
    {
        switch (token)
        {
            case IValueToken valueToken:
            {
                var tmp = MakeTempName();
                
                if (valueToken.IsConstant)
                {
                    variables[tmp] = (
                        valueToken
                            .TryGetValue()
                            .OnSuccess(s => s.ToCSharpObject(null), mainErr),
                        token.RawRep
                    );
                }
                else
                {
                    variables[tmp] = (
                        new(() => valueToken
                            .TryGetValue()
                            .OnSuccess(s => s.ToCSharpObject(null), mainErr)),
                        token.RawRep
                    );
                }
                
                AppendRaw(tmp);
                return true;
            }
            case
            {
                RawRep: 
                    "==" or "!=" or ">" or ">=" or "<" or "<=" 
                    or "+" or "-" or "*" or "/" or "%" 
                    or "&&" or "||" 
                    or "invalid"
            }:
            {
                AppendRaw(token.RawRep);
                return true;
            }
            case { RawRep: "=" or "is" }:
            {
                AppendRaw("==");
                return true;
            }
            case { RawRep: "isnt" or "isn't" or "isnot" }:
            {
                AppendRaw("!=");
                return true;
            }
            case { RawRep: "and" }:
            {
                AppendRaw("&&");
                return true;
            }
            case { RawRep: "or" }:
            {
                AppendRaw("||");
                return true;
            }
            default:
            {
                return mainErr + $"{token} cannot be used in an expression. Maybe you made a typo?";
            }
        }
        
        string MakeTempName()
        {
            return "value" + tempId.ToString(CultureInfo.InvariantCulture);
        }
        
        void AppendRaw(string value)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(value);
        }
    }
}