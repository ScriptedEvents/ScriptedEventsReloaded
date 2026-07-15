using SER.Code.Extensions;
using SER.Code.FlagSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.MethodSystem.Methods.ScriptMethods;
using SER.Code.ScriptSystem.Structures;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class OnCustomTriggerFlag : Flag, IMajorBehaviorFlag
{
    private string _trigger = null!;
    
    public static readonly Dictionary<string, List<ScriptName>> ScriptsBoundToTrigger = [];
    
    public override string Description =>
        $"Makes a script execute when a trigger with a matching name is fired (done using {nameof(TriggerMethod).Replace("Method", "")} method)";

    public override Argument? InlineArgument => new(
        "triggerName",
        "The name of the trigger to bind to.",
        inlineArgs =>
        {
            switch (inlineArgs.Length)
            {
                case < 1: return "Trigger name is missing";
                case > 1: return "Too many arguments, only trigger name is allowed";
            }

            _trigger = inlineArgs.First();
            return true;
        },
        true,
        "!-- OnCustomTrigger myTrigger"
    );
    
    public override Argument[] Arguments => [];

    public override Result Bind()
    {
        ScriptsBoundToTrigger.AddOrInitListWithKey(_trigger, ScriptName);
        return true;
    }

    public override void Unbind()
    {
        if (ScriptsBoundToTrigger.TryGetValue(_trigger, out var list))
        {
            list.Remove(ScriptName);
        }
    }
}
