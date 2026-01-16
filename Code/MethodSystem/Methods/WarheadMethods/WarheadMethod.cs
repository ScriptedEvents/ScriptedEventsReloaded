using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.WarheadMethods;

[UsedImplicitly]
public class WarheadMethod : SynchronousMethod
{
    public override string Description => "Manages alpha warhead.";
    
    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("action", 
            "open",
            "close",
            "arm",
            "disarm",
            "lock",
            "unlock",
            "start",
            "stop",
            "detonate",
            "shake"
        )
    ];
    
    public override void Execute()
    {
        switch (Args.GetOption("action"))
        {
            case "open": 
                Warhead.IsAuthorized = true; 
                break;

            case "close":
                Warhead.IsAuthorized = false;
                break;

            case "lock":
                Warhead.IsLocked = true;
                break;

            case "unlock":
                Warhead.IsLocked = false;
                break;

            case "arm":
                Warhead.BaseNukesitePanel?.Networkenabled = true;
                break;

            case "disarm":
                Warhead.BaseNukesitePanel?.Networkenabled = false;
                break;

            case "start":
                Warhead.Start();
                break;

            case "stop":
                Warhead.Stop();
                break;

            case "detonate":
                Warhead.Detonate();
                break;

            case "shake":
                Warhead.Shake();
                break;

            default: 
                throw new KrzysiuFuckedUpException("out of range");
        }
    }
}