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
public class MER_SpawnSchematicMethod : ReferenceReturningMethod<MERSchematic>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Spawns a MER schematic.";
    public string AdditionalDescription => "The returned reference exposes name and object; object is the actual schematic.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("schematic name"),
        new FloatArgument("x position"),
        new FloatArgument("y position"),
        new FloatArgument("z position"),
        new FloatArgument("x rotation") { DefaultValue = new(0f, "0") },
        new FloatArgument("y rotation") { DefaultValue = new(0f, "0") },
        new FloatArgument("z rotation") { DefaultValue = new(0f, "0") },
        new FloatArgument("x scale") { DefaultValue = new(1f, "1") },
        new FloatArgument("y scale") { DefaultValue = new(1f, "1") },
        new FloatArgument("z scale") { DefaultValue = new(1f, "1") }
    ];
    public string[] ErrorReasons => ["The schematic does not exist, is invalid, or ProjectMER cancelled its spawning event."];

    public override void Execute()
    {
        ReturnValue = MerBridge.SpawnSchematic(
            Args.GetText("schematic name"),
            Args.GetFloat("x position"),
            Args.GetFloat("y position"),
            Args.GetFloat("z position"),
            Args.GetFloat("x rotation"),
            Args.GetFloat("y rotation"),
            Args.GetFloat("z rotation"),
            Args.GetFloat("x scale"),
            Args.GetFloat("y scale"),
            Args.GetFloat("z scale"));
    }
}

[UsedImplicitly]
public class MER_DestroySchematicMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Destroys a MER schematic.";
    public string AdditionalDescription => "Only standalone schematic references are accepted.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<MERSchematic>("schematic reference")];
    public string[] ErrorReasons => ["The schematic was already destroyed or the reference is invalid."];
    public override void Execute() => MerBridge.DestroySchematic(Args.GetReference<MERSchematic>("schematic reference"));
}

[UsedImplicitly]
public class MER_GetSchematicBlocksMethod : ReturningMethod<CollectionValue<ReferenceValue<MERSchematicBlock>>>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Gets MER schematic blocks.";
    public string AdditionalDescription => "Each reference exposes schematicName, index, name, and object; object is the block GameObject.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<MERSchematic>("schematic reference")];
    public string[] ErrorReasons => ["The schematic reference is no longer valid."];

    public override void Execute()
    {
        ReturnValue = new CollectionValue<ReferenceValue<MERSchematicBlock>>(
            MerBridge.GetSchematicBlocks(Args.GetReference<MERSchematic>("schematic reference")));
    }
}

[UsedImplicitly]
public class MER_GetAnimatorsMethod : ReturningMethod<CollectionValue<ReferenceValue<MERAnimator>>>, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Gets MER schematic animators.";
    public string AdditionalDescription => "Each reference exposes schematicName, index, name, and object; object is the Unity animator.";
    public override Argument[] ExpectedArguments { get; } = [new ReferenceArgument<MERSchematic>("schematic reference")];
    public string[] ErrorReasons => ["The schematic reference is no longer valid."];

    public override void Execute()
    {
        ReturnValue = new CollectionValue<ReferenceValue<MERAnimator>>(
            MerBridge.GetAnimators(Args.GetReference<MERSchematic>("schematic reference")));
    }
}

[UsedImplicitly]
public class MER_PlayAnimationMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Plays a MER animation.";
    public string AdditionalDescription => "Select the animator by name or one-based index.";
    public override Argument[] ExpectedArguments { get; } = MerAnimationArguments.Create("state name");
    public string[] ErrorReasons => MerAnimationArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.PlayAnimation(
            Args.GetReference<MERSchematic>("schematic reference"),
            Args.GetText("state name"),
            Args.GetInt("animator index"),
            Args.GetText("animator name"));
    }
}

[UsedImplicitly]
public class MER_SetAnimationStateMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Sets a MER animation bool.";
    public string AdditionalDescription => "The animator index is one-based.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<MERSchematic>("schematic reference"),
        new TextArgument("parameter name"),
        new BoolArgument("state"),
        new IntArgument("animator index", 1) { DefaultValue = new(1, "1") }
    ];
    public string[] ErrorReasons => MerAnimationArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.SetAnimationBool(
            Args.GetReference<MERSchematic>("schematic reference"),
            Args.GetText("parameter name"),
            Args.GetBool("state"),
            Args.GetInt("animator index"));
    }
}

[UsedImplicitly]
public class MER_StopAnimationMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Stops a MER animation.";
    public string AdditionalDescription => "Select the animator by name or one-based index.";
    public override Argument[] ExpectedArguments { get; } =
    [
        new ReferenceArgument<MERSchematic>("schematic reference"),
        new IntArgument("animator index", 1) { DefaultValue = new(1, "1") },
        new TextArgument("animator name") { DefaultValue = new("", "use animator index") }
    ];
    public string[] ErrorReasons => MerAnimationArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.StopAnimation(
            Args.GetReference<MERSchematic>("schematic reference"),
            Args.GetInt("animator index"),
            Args.GetText("animator name"));
    }
}

[UsedImplicitly]
public class MER_ShowSchematicMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Shows a MER schematic.";
    public string AdditionalDescription => "Sends spawn messages only to the specified players; visibility is transient.";
    public override Argument[] ExpectedArguments { get; } = MerVisibilityArguments.Create();
    public string[] ErrorReasons => MerVisibilityArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.SetSchematicVisibility(
            Args.GetPlayers("players"),
            Args.GetReference<MERSchematic>("schematic reference"),
            true);
    }
}

[UsedImplicitly]
public class MER_HideSchematicMethod : SynchronousMethod, IAdditionalDescription, ICanError, IDependOnFramework
{
    public FrameworkBridge.Type DependsOn => FrameworkBridge.Type.ProjectMapEditorReborn;
    public override string Description => "Hides a MER schematic.";
    public string AdditionalDescription => "Sends destroy messages only to the specified players; visibility is transient.";
    public override Argument[] ExpectedArguments { get; } = MerVisibilityArguments.Create();
    public string[] ErrorReasons => MerVisibilityArguments.ErrorReasons;

    public override void Execute()
    {
        MerBridge.SetSchematicVisibility(
            Args.GetPlayers("players"),
            Args.GetReference<MERSchematic>("schematic reference"),
            false);
    }
}

internal static class MerAnimationArguments
{
    internal static readonly string[] ErrorReasons = ["The schematic is invalid.", "The animator index is out of range or its name was not found.", "The requested state or parameter does not exist."];

    internal static Argument[] Create(string valueName)
    {
        return
        [
            new ReferenceArgument<MERSchematic>("schematic reference"),
            new TextArgument(valueName),
            new IntArgument("animator index", 1) { DefaultValue = new(1, "1") },
            new TextArgument("animator name") { DefaultValue = new("", "use animator index") }
        ];
    }
}

internal static class MerVisibilityArguments
{
    internal static readonly string[] ErrorReasons = ["The schematic reference is no longer valid.", "A player's connection is not available."];

    internal static Argument[] Create()
    {
        return
        [
            new PlayersArgument("players"),
            new ReferenceArgument<MERSchematic>("schematic reference")
        ];
    }
}
