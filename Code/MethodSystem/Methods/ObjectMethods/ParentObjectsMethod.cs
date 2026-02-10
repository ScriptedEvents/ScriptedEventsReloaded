using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.ObjectMethods;

public class ParentObjectsMethod : SynchronousMethod, ICanError
{
    public override string Description { get; } = "Parents child to parent";

    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<GameObject>("child")
        {
            Description = "The child that will follor parents position",
        },
        new ReferenceArgument<GameObject>("parent")
        {
            Description = "The parent that will have object under itself."
        },
    ];
    public override void Execute()
    {
        var child = Args.GetReference<GameObject>("child");
        var parent = Args.GetReference<GameObject>("parent");

        if (child == null)
        {
            Script.Error(ErrorReasons[0]);
            return;
        }
        
        if (parent == null)
        {
            Script.Error(ErrorReasons[1]);
            return;
        }
        
        child.transform.parent = parent.transform;
    }

    public string[] ErrorReasons { get; } = 
        [
            "Child is null (not existent)",
            "Parent is null (not existent)",
        ];
}