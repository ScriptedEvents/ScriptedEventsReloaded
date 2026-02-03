using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;

namespace SER.Code.MethodSystem.Methods.AdminToysMethods;

[UsedImplicitly]
public class SetToyParentMethod : SynchronousMethod, ICanError
{
    public override string Description => "Sets the parent of a toy (So that the toy follows it).";

    public string[] ErrorReasons =>
    [
        "No player has been given when needed.",
        "No Admin Toy has been given when needed."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<AdminToy>("toy reference"),
        new OptionsArgument("parent type",
            "playerCamera",
            "playerBody",
            "toy"),
        new PlayerArgument("player parent")
        {
            DefaultValue = new(null, "none (if type is toy)")
        },
        new ReferenceArgument<AdminToy>("toy parent")
        {
            DefaultValue = new(null, "none (if type is player)")
        },
    ];
    
    public override void Execute()
    {
        var toy = Args.GetReference<AdminToy>("toy reference");
        var parentType = Args.GetOption("parent type");
        var playerParent = Args.GetPlayer("player parent").MaybeNull();
        var toyParent = Args.GetReference<AdminToy>("toy parent").MaybeNull();

        toy.Transform.SetParent(parentType switch
        {
            "playerbody" => playerParent?.ReferenceHub.transform
                            ?? throw new ScriptRuntimeError(this, ErrorReasons[0]),
            "playercamera" => playerParent?.Camera
                              ?? throw new ScriptRuntimeError(this, ErrorReasons[0]),
            "toy" => toyParent?.Transform
                     ?? throw new ScriptRuntimeError(this, ErrorReasons[1]),
            _ => throw new TosoksFuckedUpException("out of order")
        }, false);
    }

}