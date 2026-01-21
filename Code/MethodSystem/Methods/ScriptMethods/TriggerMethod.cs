using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class TriggerMethod : SynchronousMethod
{
    public override string Description => "Fires a given trigger, executing scripts which are attached to it.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("trigger name")
    ];
    
    public override void Execute()
    {
        if (!OnCustomTriggerFlag.ScriptsBoundToTrigger.TryGetValue(Args.GetText("trigger name"), out var scripts))
        {
            return;
        }

        foreach (var scriptName in scripts)
        {
            if (Script.CreateByScriptName(scriptName, ScriptExecutor.Get()).HasErrored(out var error, out var script))
            {
                throw new ScriptRuntimeError(this, error);
            }
            
            script.Run(RunContext.Script, Script);
        }
    }
}