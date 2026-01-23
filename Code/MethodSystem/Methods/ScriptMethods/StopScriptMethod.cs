using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class StopScriptMethod : SynchronousMethod
{
    public override string Description => "Stops a given script.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new RunningScriptArgument("running script")
    ];
    
    public override void Execute()
    {
        Args.GetRunningScript("running script").Stop();
    }
}