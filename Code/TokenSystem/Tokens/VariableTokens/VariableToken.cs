using SER.Code.ContextSystem.BaseContexts;
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

    public abstract Context GetContext(Script scr);

    public static readonly (char prefix, Type varTypeToken)[] VariablePrefixes =
    [
        ('$', typeof(LiteralVariableToken)),
        ('@', typeof(PlayerVariableToken)),
        ('*', typeof(ReferenceVariableToken)),
        ('&', typeof(CollectionVariableToken))
    ];
    
    public char Prefix => VariablePrefixes.First(pair => pair.varTypeToken == GetType()).prefix;
    
    public TryGet<Variable> TryGetVariable()
    {
        return Script.TryGetVariable<Variable>(this);
    }
    
    public string RawRepr => $"{Prefix}{Name}";
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
        return Script.TryGetVariable<TVariable>(this);
    }

    public TryGet<TValue> ExactValue => TryGetVariable().OnSuccess(variable => variable.Value, null);

    protected override IParseResult InternalParse(Script scr)
    {
        if (RawRep.Length < 2 || RawRep.FirstOrDefault() != Prefix)
        {
            return new Ignore();
        }

        Name = RawRep.Substring(1);
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