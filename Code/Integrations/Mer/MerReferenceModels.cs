using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem.PropertySystem;

// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedPositionalProperty.Global
namespace SER.Code.Integrations.Mer;

/// <summary>
/// Common shape for named SER references that contain an actual ProjectMER or Unity object.
/// </summary>
public interface IMerReference : IFrameworkTypeShell
{
}

/// <summary>
/// Common shape for references that identify an object stored in a ProjectMER map.
/// </summary>
public interface IMerObjectReference : IMerReference
{
    string MapName { get; }
    string Id { get; }
    string Type { get; }
}

/// <summary>
/// A named reference to a ProjectMER map. Object contains the actual ProjectMER map instance.
/// </summary>
public sealed record MERMap(string Name, object Object) : IMerReference;

/// <summary>
/// A named reference to one spawned copy of a ProjectMER map object.
/// Object contains the actual spawned map object and Definition contains its serializable definition.
/// </summary>
public sealed record MERObject(
    string MapName,
    string Id,
    string Type,
    object Object,
    object Definition) : IMerObjectReference;

/// <summary>
/// A named reference to a ProjectMER serializable object definition.
/// Object contains the actual ProjectMER serializable object.
/// </summary>
public sealed record MERObjectDefinition(
    string MapName,
    string Id,
    string Type,
    object Object) : IMerObjectReference, IFrameworkTypeShellWriteHandler
{
    public Result OnObjectPropertySet(string propertyName)
    {
        try
        {
            MerBridge.RefreshObject(this);
            return true;
        }
        catch (Exception exception)
        {
            return $"Property was changed, but the MER object could not be refreshed: {exception.Message}";
        }
    }
}

/// <summary>
/// A named reference to a spawned standalone ProjectMER schematic.
/// Object contains the actual ProjectMER schematic instance.
/// </summary>
public sealed record MERSchematic(string Name, object Object) : IMerReference;

/// <summary>
/// A named reference to a block belonging to a ProjectMER schematic.
/// Object contains the actual Unity game object.
/// </summary>
public sealed record MERSchematicBlock(
    string SchematicName,
    int Index,
    string Name,
    object Object) : IMerReference;

/// <summary>
/// A named reference to an animator belonging to a ProjectMER schematic.
/// Object contains the actual Unity animator.
/// </summary>
public sealed record MERAnimator(
    string SchematicName,
    int Index,
    string Name,
    object Object) : IMerReference;
