using LabApi.Features.Wrappers;
using SER.Code.ValueSystem;

namespace SER.Code.VariableSystem.Variables;

public class PredefinedPlayerVariable(string name, Func<List<Player>> value, string category) 
    : PlayerVariable(name, null!)
{
    public override PlayerValue Value => new(value());
    public string Category => category;
    
    [UsedImplicitly]
    public PredefinedPlayerVariable() : this("temp", null!, "temp") {}
}