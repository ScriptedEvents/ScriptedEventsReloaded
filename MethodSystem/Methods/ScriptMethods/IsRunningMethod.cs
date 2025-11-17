using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.MethodSystem.BaseMethods;
using SER.ValueSystem;
using SER.ScriptSystem;

namespace SER.MethodSystem.Methods.ScriptMethods;

public class IsRunningMethod : ReturningMethod<BoolValue>
{
    public override string Description => "Returns true if given script is running";

    public override Argument[] ExpectedArguments { get; } = 
    [
        new TextArgument("script name")
    ];

    public override void Execute()
    {
        var name = Args.GetText("script name");
        ReturnValue = Script.RunningScripts.Any(scr => scr.Name == name);
    }
}