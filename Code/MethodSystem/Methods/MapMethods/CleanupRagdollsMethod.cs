using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers.Extensions;
using SER.Code.MethodSystem.BaseMethods;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class CleanupRagdollsMethod : SynchronousMethod
{
    public override string Description => "Destroys ragdolls.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new EnumArgument<RoleTypeId>("roleToRemove")
        {
            DefaultValue = new(RoleTypeId.None, "all"),
            Description = "Do not provide this argument to destroy all ragdolls."
        }
    ];
    
    public override void Execute()
    {
        var roleToRemove = Args.GetEnum<RoleTypeId>("roleToRemove");
        IEnumerable<Ragdoll> ragdolls = Ragdoll.List;
        if (roleToRemove != RoleTypeId.None)
        {
            ragdolls = ragdolls.Where(rd => rd.Role == roleToRemove);
        }
        
        ragdolls.ForEachItem(rd => rd.Destroy());
    }
}