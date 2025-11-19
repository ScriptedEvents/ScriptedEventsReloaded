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
}