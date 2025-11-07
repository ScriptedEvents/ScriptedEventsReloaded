using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;

namespace SER.ValueSystem;

public class PlayerValue : Value
{
    public PlayerValue(Player plr)
    {
        Players = [plr];
    }

    public PlayerValue(IEnumerable<Player> players)
    {
        Players = players.ToArray();
    }

    public Player[] Players { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not PlayerValue other) return false;
        return Equals(other);
    }

    protected bool Equals(PlayerValue other)
    {
        return Players.Equals(other.Players);
    }

    public override int GetHashCode()
    {
        return Players.GetHashCode();
    }
}