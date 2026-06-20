using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class OnErrorStatement : StatementContext, IStatementExtender, IKeywordContext, IAcceptOptionalVariableDefinitionsContext
{
    private VariableToken? _messageVariableToken;
    private VariableToken? _stackTraceVariableToken;
    private VariableToken? _typeVariableToken;
    public override string FriendlyName => "'on_error' statement";

    public Exception? Exception
    {
        get;
        set
        {
            if (field is not null)
                return;
            field = value;
        }
    }

    public OldResult SetOptionalVariables(params VariableToken[] variableTokens)
    {
        if (variableTokens.Length > 3)
            return $"Too many arguments provided for {FriendlyName}, only up to 3 are allowed.";

        var errorMsg = "Provided variable '{0}' cannot be used for an " + FriendlyName +
                       $", as it's not a {typeof(LiteralVariable).FriendlyTypeName()}";

        var messageToken = variableTokens.FirstOrDefault();
        switch (messageToken)
        {
            case null:
                return true;
            case LiteralVariableToken:
                _messageVariableToken = messageToken;
                break;
            default:
                return string.Format(errorMsg, messageToken.RawRepr);
        }

        var typeToken = variableTokens.Skip(1).FirstOrDefault();
        switch (typeToken)
        {
            case null:
                return true;
            case LiteralVariableToken:
                _typeVariableToken = typeToken;
                break;
            default:
                return string.Format(errorMsg, typeToken.RawRepr);
        }

        var stackTraceToken = variableTokens.Skip(2).FirstOrDefault();
        switch (stackTraceToken)
        {
            case null:
                break; // it's gonna return true either way
            case LiteralVariableToken:
                _stackTraceVariableToken = stackTraceToken;
                break;
            default:
                return string.Format(errorMsg, stackTraceToken.RawRepr);
        }

        return true;
    }
    public string KeywordName => "on_error";
    public string Description => "Catches an exception thrown inside of an 'attempt' statement.";

    public string[] Arguments => [];
    public string Example =>
        """
        &collection = Coll.Empty
        attempt
            Print {Coll.Fetch &collection 2}
            # ERROR: there's nothing at index 2
            
            Print "Hello, world!"
            # ^ won't get executed because 'attempt' skips the remaining code
            # inside of it if an error was made
            
        on_error with $message $type $stackTrace
            # this will print the error message
            Print "Error: {$message}"
            
            # In 90% of situations $type will be ScriptRuntimeError
            Print "Type of error: {$type}"
            
            # This just shows where in the internal code (not the script) the error was made
            # (basically to allow devs to know where in the code they may have fucked up)
            Print "Stack trace: {$stackTrace}"
        end
        """;

    public IExtendableStatement.Signal Extends => IExtendableStatement.Signal.ThrewException;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error($"A {FriendlyName} does not expect any arguments.");
    }

    public override OldResult VerifyCurrentState()
    {
        return true;
    }

    protected override IEnumerator<float> Execute()
    {
        Variable? messageVariable = null;
        Variable? typeVariable = null;
        Variable? stackTraceVariable = null;

        if (_messageVariableToken is not null)
        {
            messageVariable = Variable.Create(_messageVariableToken.Name, Value.Parse(Exception!.Message));
            Script.AddLocalVariable(messageVariable);
        }
        if (_typeVariableToken is not null)
        {
            typeVariable = Variable.Create(_typeVariableToken.Name, Value.Parse(Exception!.GetType().AccurateName));
            Script.AddLocalVariable(typeVariable);
        }
        if (_stackTraceVariableToken is not null)
        {
            stackTraceVariable = Variable.Create(_stackTraceVariableToken.Name, Value.Parse(Exception!.StackTrace));
            Script.AddLocalVariable(stackTraceVariable);
        }

        using var coro = RunChildren();
        while (coro.MoveNext())
            yield return coro.Current;

        if (messageVariable is not null) Script.RemoveLocalVariable(messageVariable);
        if (typeVariable is not null) Script.RemoveLocalVariable(typeVariable);
        if (stackTraceVariable is not null) Script.RemoveLocalVariable(stackTraceVariable);
    }
}