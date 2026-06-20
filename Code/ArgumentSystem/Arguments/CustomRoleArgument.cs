using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class CustomRoleArgument(string name) : Argument(name)
{
    public override string InputDescription => "Custom role id e.g. myCustomRole";

    [UsedImplicitly]
    public OldDynamicTryGet<CRole> GetConvertSolution(BaseToken token)
    {
        return new(() => Get(token.BestStaticTextRepr()));

        OldTryGet<CRole> Get(string n)
        {
            return CRole.RegisteredRoles.TryGetValue(n, out var cr)
                ? cr
                : $"Provided value '{n}' is not a valid custom role id.".AsOldError();
        }
    }
}