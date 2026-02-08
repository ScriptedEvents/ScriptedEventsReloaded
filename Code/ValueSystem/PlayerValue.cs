using LabApi.Features.Wrappers;
using SER.Code.Exceptions;
using SER.Code.Extensions;

namespace SER.Code.ValueSystem;

public class PlayerValue : TraversableValue
{
    public PlayerValue(Player? plr)
    {
        Players = plr is not null
            ? [plr]
            : [];
    }

    public PlayerValue(IEnumerable<Player> players)
    {
        Players = players.ToArray();
    }

    public Player[] Players { get; }

    public override bool EqualCondition(Value other) => other is PlayerValue otherP && Players.SequenceEqual(otherP.Players);
    
    public override int HashCode =>
        Players.Select(plr => plr.UserId).GetEnumerableHashCode().HasErrored(out var error, out var val)
        ? throw new TosoksFuckedUpException(error)
        : val;

    public override Value[] TraversableValues => Players.Select(plr => new PlayerValue(plr)).ToArray();
}