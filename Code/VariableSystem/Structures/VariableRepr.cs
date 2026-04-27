namespace SER.Code.VariableSystem.Structures;

public readonly struct VariableRepr(string name, char prefix) : IVariableRepr
{
    public string Name => name;
    public char Prefix => prefix;
}