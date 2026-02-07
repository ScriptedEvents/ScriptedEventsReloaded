using SER.Code.ContextSystem.BaseContexts;
using SER.Code.Exceptions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens.Interfaces;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.TokenSystem.Tokens.VariableTokens;

public abstract class VariableToken : BaseToken, IContextableToken
{
    public abstract string Name { get; protected set; }
    
    public abstract Type VariableType { get; }
    
    public abstract Type ValueType { get; }

    public abstract Context? GetContext(Script? scr);

    public static readonly (char prefix, Type varTypeToken)[] VariablePrefixes =
    [
        ('$', typeof(LiteralVariableToken)),
        ('@', typeof(PlayerVariableToken)),
        ('*', typeof(ReferenceVariableToken)),
        ('&', typeof(CollectionVariableToken))
    ];
    
    public char Prefix => VariablePrefixes.First(pair => pair.varTypeToken == GetType()).prefix;

    public static string Verified<TVariable>(string rep) where TVariable : VariableToken, new()
    {
        if (new TVariable().AnonymousInit(rep) is not Success)
        {
            throw new Exception($"Documentation tried using variable '{rep}' which has invalid syntax.");
        }
        
        return rep;
    }

    public TryGet<Variable> TryGetVariable()
    {
        return Script?.TryGetVariable<Variable>(this)
               ?? throw new AndrzejFuckedUpException("Tried to get variable from a anonymous variable token.");
    }
}

public abstract class VariableToken<TVariable, TValue> : VariableToken, IValueToken
    where TVariable : Variable<TValue>
    where TValue : Value
{
    public override string Name { get; protected set; } = null!;

    public override Type VariableType => typeof(TVariable);
    public override Type ValueType => typeof(TValue);

    public new TryGet<TVariable> TryGetVariable()
    {
        return Script?.TryGetVariable<TVariable>(this) 
               ?? throw new AndrzejFuckedUpException("Tried to get variable from a anonymous variable token.");
    }

    public TryGet<TValue> ExactValue => TryGetVariable().OnSuccess(variable => variable.Value, null);

    protected override IParseResult InternalParse()
    {
        if (((BaseToken)this).RawRep.Length < 2 || ((BaseToken)this).RawRep.FirstOrDefault() != Prefix)
        {
            return new Ignore();
        }

        Name = ((BaseToken)this).RawRep[1..];
        if (Name.Any(c => !char.IsLetter(c) && !char.IsDigit(c) && c != '_'))
        {
            return new Ignore();
        }
        
        return new Success();
    }

    public TryGet<Value> Value()
    {
        return TryGetVariable().OnSuccess(Value (variable) => variable.Value, null);
    }

    public TypeOfValue PossibleValues => new TypeOfValue<TValue>();
    
    public bool IsConstant => false;
}