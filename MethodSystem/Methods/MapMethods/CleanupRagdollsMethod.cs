using LabApi.Features.Wrappers;
using PlayerRoles;
using SER.ArgumentSystem.Arguments;
using SER.ArgumentSystem.BaseArguments;
using SER.Helpers.Extensions;
using SER.MethodSystem.BaseMethods;

namespace SER.MethodSystem.Methods.MapMethods;

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