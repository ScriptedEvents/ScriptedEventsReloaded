using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.Helpers;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Variables;
using Utils.NonAllocLINQ;

namespace SER.Code.FlagSystem.Flags;

[UsedImplicitly]
public class InteractableToyEventFlag : Flag
{
    public static readonly List<ScriptName> ScriptsBoundToEvent = [];
    
    public override string Description => 
        $"""
         Triggers whenever an {nameof(InteractableToy)} gets interacted with
         (Only works on SER-created interactables)
         
         Injects the following variables into the script:
           -- @evPlayer (the player who interacted with the {nameof(InteractableToy)})
           -- *evToy (the toy that got interacted with)
        """;
    public override Argument? InlineArgument => null;
    
    public override Argument[] Arguments => [];

    public override void OnParsingComplete()
    {
        ScriptsBoundToEvent.AddIfNotContains(ScriptName);
    }

    public override void Unbind()
    {
        ScriptsBoundToEvent.Remove(ScriptName);
    }

    public static void RunBoundScripts(Player player, InteractableToy interactableToy)
    {
        Variable[] variables =
        [
            new PlayerVariable("evPlayer", new PlayerValue(player)),
            new ReferenceVariable("evToy", new ReferenceValue(interactableToy))
        ];
        
        foreach (var scriptName in ScriptsBoundToEvent)
        {
            if (scriptName.GetScript(null).HasErrored(out var err, out var script))
            {
                Log.Error(err);
                continue;
            }

            script.AddLocalVariables(variables);
            script.Run(RunContext.Event);
        }
    }
}