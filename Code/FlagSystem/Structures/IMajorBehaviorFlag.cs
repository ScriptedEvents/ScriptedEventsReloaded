namespace SER.Code.FlagSystem.Structures;

/// <summary>
/// Defines a flag as majorly modifying behavior, not allowing more than 1 per script.
/// </summary>
// as of now, it is used on every flag lmao
// tho this can change in the future like !-- RunLimit flag or smth
public interface IMajorBehaviorFlag;