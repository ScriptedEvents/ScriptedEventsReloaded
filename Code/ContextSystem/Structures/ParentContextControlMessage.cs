using SER.Code.ValueSystem;

namespace SER.Code.ContextSystem.Structures;

public abstract record ParentContextControlMessage;

public record Continue : ParentContextControlMessage;

public record Break : ParentContextControlMessage;

public record Return(Value ReturnedValue) : ParentContextControlMessage;