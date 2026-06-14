using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class PlayerArgument(string name) : Argument(name)
{
    public override string InputDescription => "Player value e.g. @player, with EXACTLY 1 player.";

    [UsedImplicitly]
    public DynamicTryGet<Player> GetConvertSolution(BaseToken token)
    {
        if (token.CanReturn<PlayerValue>(out var value))
        {
            return new(() => value().OnSuccess(DynamicSolver));
        }

        return GenericError(token);
        
        TryGet<Player> DynamicSolver(PlayerValue playerValue)
        {
            var plrs = playerValue.Players;
            if (plrs.Len != 1)
            {
                return $"The player value under '{token.RawRep}' must have exactly 1 player, but has {plrs.Len} instead.";
            }

            return plrs[0];
        }
    }
}