using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.Helpers;
using SER.Code.Integrations.Mer;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
// ReSharper disable InconsistentNaming

namespace SER.Code.MethodSystem.Methods.MERMethods;

[UsedImplicitly]
public class MER_GetObjectsMethod : ReturningMethod<CollectionValue<ReferenceValue<MERObject>>>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Gets MER map objects.";
    public string AdditionalDescription => "An empty ID and the any type match every object. Each reference exposes mapName, id, type, object, and definition.";
    public override Argument[] ExpectedArguments { get; } =
    [
        MerObjectTypeArguments.CreateFilter("object type"),
        new TextArgument("map name"),
        new TextArgument("object ID") { DefaultValue = new("", "all IDs") }
    ];

    public override void Execute()
    {
        ReturnValue = new CollectionValue<ReferenceValue<MERObject>>(MerBridge.GetObjects(
            Args.GetText("map name"),
            Args.GetText("object ID"),
            Args.GetOption("object type")));
    }
}

[UsedImplicitly]
public class MER_GetObjectDefinitionMethod : ReferenceReturningMethod<MERObjectDefinition>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Gets a MER object definition.";
    public string AdditionalDescription => "The reference exposes mapName, id, type, and object. Definition properties are forwarded and refreshed after writes.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("object ID"),
        new TextArgument("map name")
    ];
    public string[] ErrorReasons => ["The map is not loaded.", "The object ID does not exist in the map."];

    public override void Execute()
    {
        ReturnValue = MerBridge.GetObjectDefinition(Args.GetText("map name"), Args.GetText("object ID"));
    }
}

[UsedImplicitly]
public class MER_CreateObjectMethod : ReferenceReturningMethod<MERObjectDefinition>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Creates a MER map object.";
    public string AdditionalDescription => "The position is absolute. Definition properties are forwarded and refreshed after writes.";
    public override Argument[] ExpectedArguments { get; } =
    [
        MerObjectTypeArguments.Create("object type"),
        new FloatArgument("x position"),
        new FloatArgument("y position"),
        new FloatArgument("z position"),
        new TextArgument("map name") { DefaultValue = new("Untitled", "Untitled") },
        new TextArgument("object ID") { DefaultValue = new("", "automatically generated") },
        new TextArgument("schematic name")
        {
            Description = "Used only when object type is schematic.",
            DefaultValue = new("", "none")
        }
    ];
    public string[] ErrorReasons => ["The object type or schematic name is invalid.", "The object ID already exists.", "ProjectMER failed to spawn the object."];

    public override void Execute()
    {
        ReturnValue = MerBridge.CreateObject(
            Args.GetOption("object type"),
            Args.GetText("map name"),
            Args.GetText("object ID"),
            Args.GetFloat("x position"),
            Args.GetFloat("y position"),
            Args.GetFloat("z position"),
            Args.GetText("schematic name"));
    }
}

[UsedImplicitly]
public class MER_DeleteObjectMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Deletes a MER map object.";
    public string AdditionalDescription => "Deletes the definition and every spawned copy.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<IMerObjectReference>("MER object reference")];
    public string[] ErrorReasons => ["The referenced map or object no longer exists."];
    public override void Execute() => MerBridge.DeleteObject(Args.GetReference<IMerObjectReference>("MER object reference"));
}

[UsedImplicitly]
public class MER_RenameObjectMethod : ReferenceReturningMethod<MERObjectDefinition>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Renames a MER map object.";
    public string AdditionalDescription => "Reloads the map and returns a replacement definition reference.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<IMerObjectReference>("MER object reference"),
        new TextArgument("new object ID")
    ];
    public string[] ErrorReasons => ["The old object no longer exists.", "The new object ID is already used."];

    public override void Execute()
    {
        ReturnValue = MerBridge.RenameObject(
            Args.GetReference<IMerObjectReference>("MER object reference"),
            Args.GetText("new object ID"));
    }
}

[UsedImplicitly]
public class MER_MoveObjectToMapMethod : ReferenceReturningMethod<MERObjectDefinition>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Moves a MER map object.";
    public string AdditionalDescription => "Reloads affected maps and returns a replacement definition reference.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<IMerObjectReference>("MER object reference"),
        new TextArgument("new map name"),
        new TextArgument("new object ID") { DefaultValue = new("", "keep current ID") }
    ];
    public string[] ErrorReasons => ["The source object no longer exists.", "The destination already contains the requested ID."];

    public override void Execute()
    {
        ReturnValue = MerBridge.MoveObjectToMap(
            Args.GetReference<IMerObjectReference>("MER object reference"),
            Args.GetText("new map name"),
            Args.GetText("new object ID"));
    }
}

[UsedImplicitly]
public class MER_RefreshObjectMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Refreshes a MER map object.";
    public string AdditionalDescription => "Properties that require reloading destroy and recreate the runtime object after a short MER delay. Fetch a new object reference afterward.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<IMerObjectReference>("MER object reference")];
    public string[] ErrorReasons => ["The referenced map or object no longer exists."];
    public override void Execute() => MerBridge.RefreshObject(Args.GetReference<IMerObjectReference>("MER object reference"));
}

[UsedImplicitly]
public class MER_SetPositionMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Sets a MER object position.";
    public string AdditionalDescription => "Map object changes are refreshed automatically. Some changes can invalidate the old runtime reference.";
    public override Argument[] ExpectedArguments { get; } = MerTransformArguments.Create("position");
    public string[] ErrorReasons => MerTransformArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.SetPosition(
            Args.GetReference<IMerReference>("MER reference"),
            Args.GetOption("mode"),
            Args.GetFloat("x position"),
            Args.GetFloat("y position"),
            Args.GetFloat("z position"));
    }
}

[UsedImplicitly]
public class MER_SetRotationMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Sets a MER object rotation.";
    public string AdditionalDescription => "Map object changes are refreshed automatically. Some changes can invalidate the old runtime reference.";
    public override Argument[] ExpectedArguments { get; } = MerTransformArguments.Create("rotation");
    public string[] ErrorReasons => MerTransformArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.SetRotation(
            Args.GetReference<IMerReference>("MER reference"),
            Args.GetOption("mode"),
            Args.GetFloat("x rotation"),
            Args.GetFloat("y rotation"),
            Args.GetFloat("z rotation"));
    }
}

[UsedImplicitly]
public class MER_SetScaleMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Sets a MER object scale.";
    public string AdditionalDescription => "Map object changes are refreshed automatically. Some changes can invalidate the old runtime reference.";
    public override Argument[] ExpectedArguments { get; } = MerTransformArguments.Create("scale");
    public string[] ErrorReasons => MerTransformArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.SetScale(
            Args.GetReference<IMerReference>("MER reference"),
            Args.GetOption("mode"),
            Args.GetFloat("x scale"),
            Args.GetFloat("y scale"),
            Args.GetFloat("z scale"));
    }
}

internal static class MerObjectTypeArguments
{
    private static readonly Option[] ObjectTypes =
    [
        "schematic",
        "primitive",
        "light",
        "door",
        "workstation",
        "itemSpawnpoint",
        "playerSpawnpoint",
        "capybara",
        "text",
        "scp079Camera",
        "shootingTarget",
        "locker",
        "teleport",
        "interactable",
        "waypoint"
    ];

    internal static OptionsArgument Create(string name)
    {
        return new OptionsArgument(name, ObjectTypes);
    }

    internal static OptionsArgument CreateFilter(string name)
    {
        Option[] options = new Option[ObjectTypes.Length + 1];
        options[0] = new Option("any", "Matches every object type");
        Array.Copy(ObjectTypes, 0, options, 1, ObjectTypes.Length);
        return new OptionsArgument(name, options)
        {
            DefaultValue = new Argument.Default("any", "any")
        };
    }
}

internal static class MerTransformArguments
{
    internal static readonly string[] ErrorReasons = ["The reference does not represent a map object, definition, or standalone schematic."];

    internal static Argument[] Create(string component)
    {
        return
        [
            new ReferenceArgument<IMerReference>("MER reference"),
            new OptionsArgument("mode", "set", "add"),
            new FloatArgument($"x {component}"),
            new FloatArgument($"y {component}"),
            new FloatArgument($"z {component}")
        ];
    }
}
