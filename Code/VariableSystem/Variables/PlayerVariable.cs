using LabApi.Features.Wrappers;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class PlayerVariable(string name, PlayerValue value) : Variable<PlayerValue>
{
    public override string Name => name;
    public override char Prefix => '@';
    public override PlayerValue Value => value;
    public Player[] Players => Value.Players;
}