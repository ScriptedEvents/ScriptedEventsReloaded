using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Methods.ConfigMethods.Structures;
using SER.Code.MethodSystem.Structures;

namespace SER.Code.MethodSystem.Methods.ConfigMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Config_ReadMethod : ReferenceReturningMethod<CustomConfig?>, IAdditionalDescription, ICanError
{
    public override string Description => "Reads a YAML file from the Custom Configs folder.";

    public string AdditionalDescription =>
        "Add a '.yml' or '.yaml' file to the Custom Configs folder inside the SER config directory. " +
        "Pass the file name without its extension. If no matching file exists, this method returns an invalid " +
        "reference. Learn more about YAML: https://www.cloudbees.com/blog/yaml-tutorial-everything-you-need-get-started";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("config name")
    ];

    public override void Execute()
    {
        if (FileSystem.FileSystem.GetContainedPath(
                FileSystem.FileSystem.ConfigsDirPath, Args.GetText("config name"), ".yml")
            .HasErrored(out var error, out var ymlPath))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (File.Exists(ymlPath))
        {
            ReturnValue = new CustomConfig(File.ReadAllText(ymlPath));
            return;
        }

        if (FileSystem.FileSystem.GetContainedPath(
                FileSystem.FileSystem.ConfigsDirPath, Args.GetText("config name"), ".yaml")
            .HasErrored(out error, out var yamlPath))
        {
            throw new ScriptRuntimeError(this, error);
        }

        if (File.Exists(yamlPath))
        {
            ReturnValue = new CustomConfig(File.ReadAllText(yamlPath));
            return;
        }
        
        ReturnValue = null;
    }

    public string[] ErrorReasons => ["The config name resolves outside the SER custom config directory."];
}
