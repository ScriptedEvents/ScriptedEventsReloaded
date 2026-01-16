using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
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