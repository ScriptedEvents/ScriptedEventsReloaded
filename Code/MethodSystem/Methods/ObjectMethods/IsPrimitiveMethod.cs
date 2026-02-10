using AdminToys;
using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SER.Code.MethodSystem.Methods.ObjectMethods;

public class IsPrimitiveMethod : ReturningMethod
{
    public override string Description { get; } = "Checks if GameObject is primitive, also checks if GameObject is null or not (can be pared with raycast).";
    public override Argument[] ExpectedArguments { get; } = 
    [
        new ReferenceArgument<GameObject>("object"),
    ];
    public override void Execute()
    {
        var obj = Args.GetReference<GameObject>("object");

        if (obj == null)
        {
            ReturnValue = new BoolValue(false);
            return;
        }
        
        var adminToy = AdminToy.Get(obj.GetComponent<AdminToyBase>());
        if (adminToy is not PrimitiveObjectToy primitiveObjectToy)
        {
            ReturnValue = new BoolValue(false);
            return;
        }
        
        ReturnValue = new BoolValue(true);
    }
    
    public override TypeOfValue Returns { get; } = typeof(bool);
}