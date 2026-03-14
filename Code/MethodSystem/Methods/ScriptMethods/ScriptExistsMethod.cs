using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class ScriptExistsMethod : ReturningMethod
{
    public override string Description => "Returns true or false indicating if a script with the provided name exists.";

    public override Argument[] ExpectedArguments =>
    [
        new TextArgument("script name")
    ];
    
    public override void Execute()
    {
        var scriptName = Args.GetText("script name");
        ReturnValue = new BoolValue(FileSystem.FileSystem.RegisteredScriptPaths.Any(p => Path.GetFileNameWithoutExtension(p) == scriptName));
    }

    public override TypeOfValue Returns => new SingleTypeOfValue(typeof(BoolValue));
}