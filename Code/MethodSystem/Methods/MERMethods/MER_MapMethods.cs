using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.Integrations.Mer;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;
// ReSharper disable InconsistentNaming

namespace SER.Code.MethodSystem.Methods.MERMethods;

[UsedImplicitly]
public class MER_LoadMapMethod : SynchronousMethod, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Loads a MER map.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("map name")
    ];

    public override void Execute()
    {
        MerBridge.LoadMap(Args.GetText("map name"));
    }

    public string[] ErrorReasons => ["The map does not exist, is invalid, or ProjectMER failed to spawn it."];
}

[UsedImplicitly]
public class MER_UnloadMapMethod : ReturningMethod<BoolValue>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Unloads a MER map.";
    public string AdditionalDescription => "Returns true when the map was loaded.";
    public override Argument[] ExpectedArguments { get; } = [new TextArgument("map name")];
    public override void Execute() => ReturnValue = MerBridge.UnloadMap(Args.GetText("map name"));
}

[UsedImplicitly]
public class MER_SaveMapMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Saves a MER map.";
    public string AdditionalDescription => "MER merges Untitled into the target, reloads it, and clears Untitled.";
    public override Argument[] ExpectedArguments { get; } = [new TextArgument("map name")];
    public string[] ErrorReasons => ["The map could not be serialized or written.", "The reserved Untitled name was provided."];
    public override void Execute() => MerBridge.SaveMap(Args.GetText("map name"));
}

[UsedImplicitly]
public class MER_MergeMapsMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Merges MER maps.";
    public string AdditionalDescription => "Provide an output name followed by at least two input map names.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("output map name"),
        new TextArgument("input map names") { ConsumesRemainingValues = true }
    ];
    public string[] ErrorReasons => ["Fewer than two input maps were provided.", "An input map does not exist or is invalid.", "The output map could not be written."];

    public override void Execute()
    {
        MerBridge.MergeMaps(
            Args.GetText("output map name"),
            Args.GetRemainingArguments<string, TextArgument>("input map names"));
    }
}

[UsedImplicitly]
public class MER_GetAvailableMapsMethod : ReturningMethod<CollectionValue<StaticTextValue>>, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Lists available MER maps.";
    public override Argument[] ExpectedArguments { get; } = [];
    public override void Execute() => ReturnValue = new CollectionValue<StaticTextValue>(MerBridge.GetAvailableMapNames());
}

[UsedImplicitly]
public class MER_GetAvailableSchematicsMethod : ReturningMethod<CollectionValue<StaticTextValue>>, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Lists available MER schematics.";
    public override Argument[] ExpectedArguments { get; } = [];
    public override void Execute() => ReturnValue = new CollectionValue<StaticTextValue>(MerBridge.GetAvailableSchematicNames());
}

[UsedImplicitly]
public class MER_GetLoadedMapsMethod : ReturningMethod<CollectionValue<ReferenceValue<MERMap>>>, IAdditionalDescription, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Lists loaded MER maps.";
    public string AdditionalDescription => "Each reference exposes name and object; object is the actual MER map.";
    public override Argument[] ExpectedArguments { get; } = [];
    public override void Execute() => ReturnValue = new CollectionValue<ReferenceValue<MERMap>>(MerBridge.GetLoadedMaps());
}

[UsedImplicitly]
public class MER_GetLoadedMapMethod : ReferenceReturningMethod<MERMap>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Gets a loaded MER map.";
    public string AdditionalDescription => "The returned reference exposes name and object; object is the actual MER map.";
    public override Argument[] ExpectedArguments { get; } = [new TextArgument("map name")];
    public string[] ErrorReasons => ["The requested map is not loaded."];
    public override void Execute() => ReturnValue = MerBridge.GetLoadedMap(Args.GetText("map name"));
}

[UsedImplicitly]
public class MER_ReadMapDataMethod : ReferenceReturningMethod<MERMap>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Reads a MER map file.";
    public string AdditionalDescription => "Returns a detached map reference; it is not spawned or added to loaded maps.";
    public override Argument[] ExpectedArguments { get; } = [new TextArgument("map name")];
    public string[] ErrorReasons => ["The requested map file does not exist or is invalid."];
    public override void Execute() => ReturnValue = MerBridge.ReadMapData(Args.GetText("map name"));
}
