using LabApi.Features.Wrappers;
using SER.Code.ValueSystem;
using SER.Code.ValueSystem.Other;
using SER.Code.VariableSystem.Bases;

namespace SER.Code.VariableSystem.Variables;

public class PlayerVariable(string name, Value value) : Variable<PlayerValue>
{
    public override string Name => name;
    public override string FriendlyName => "player variable";
    public override Value BaseValue => value;
    
    [UsedImplicitly]
    public PlayerVariable() : this("temp", null!) {}
}