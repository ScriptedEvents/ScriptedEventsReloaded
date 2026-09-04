using LabApi.Features.Wrappers;
using SER.Code.ValueSystem.PropertySystem;

// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedPositionalProperty.Global
namespace SER.Code.Integrations.Ucr;

/// <summary>
/// A named reference to a UCR role definition. Object contains the actual UCR role.
/// </summary>
public sealed record UCRRole(int Id, string Name, object Object) : IFrameworkTypeShell;

/// <summary>
/// A reference to one active UCR role instance. Object contains the actual UCR instance.
/// </summary>
public sealed record UCRRoleInstance(
    string Id,
    Player Player,
    UCRRole Role,
    object Object) : IFrameworkTypeShell
{
    /// <summary>
    /// Whether UCR still considers this spawned role active.
    /// </summary>
    public bool IsActive => Object.GetType().GetProperty("IsValid")?.GetValue(Object) is true;
}
