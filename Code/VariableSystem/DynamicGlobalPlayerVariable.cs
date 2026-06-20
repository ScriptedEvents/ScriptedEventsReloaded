using LabApi.Features.Wrappers;

namespace SER.Code.VariableSystem;

public record DynamicGlobalPlayerVariable(string Name, Func<Player[]> Getter, string Category);