using LabApi.Features.Wrappers;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class PlayerVariable(string name, PlayerValue value) : Variable<PlayerValue>
{
    private PlayerValue _value = value;

    public override string Name => name;
    public override string FriendlyName => "player variable";
    public override PlayerValue Value
    {
        get
        {
            var sanitizedValue = RetainReadyPlayers(_value);
            if (!ReferenceEquals(sanitizedValue, _value))
            {
                _value = sanitizedValue;
            }

            return _value;
        }
    }

    public Player[] Players => Value.Players;

    protected static PlayerValue RetainReadyPlayers(PlayerValue playerValue)
    {
        var players = playerValue.Players;
        var readyPlayers = Player.ReadyList.ToArray();
        var retainedPlayers = players
            .Where(player => readyPlayers.Any(readyPlayer => ReferenceEquals(readyPlayer, player)))
            .ToArray();

        return retainedPlayers.Length == players.Length
            ? playerValue
            : new PlayerValue(retainedPlayers);
    }
    
    [UsedImplicitly]
    public PlayerVariable() : this("temp", null!) {}
}
