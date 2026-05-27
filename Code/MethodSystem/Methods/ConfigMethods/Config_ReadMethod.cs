using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Methods.ConfigMethods.Structures;

namespace SER.Code.MethodSystem.Methods.ConfigMethods;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Config_ReadMethod : ReferenceReturningMethod<CustomConfig?>, IAdditionalDescription
{
    public override string Description => "Reads and returns a config.";

    public string AdditionalDescription => 
        $"This method will attempt to read basic '.yaml' files from '{FileSystem.FileSystem.ConfigsDirPath}' folder. " +
        $"If the folder doesnt exist, you can safely make one and create '.yaml' files there. " +
        $"The '.yml' extension is not supported, use '.yaml' only. " +
        $"If a file with the config name is not found, an invalid reference will be returned. " +
        $"Learn more about YAML files: https://www.cloudbees.com/blog/yaml-tutorial-everything-you-need-get-started ";

    public override Argument[] ExpectedArguments { get; } =
    [
        new TextArgument("config name")
    ];

    public override void Execute()
    {
        var path = Path.Combine(FileSystem.FileSystem.ConfigsDirPath, Args.GetText("config name") + ".yaml");
        if (!File.Exists(path))
        {
            ReturnValue = null;
            return;
        }

        ReturnValue = new CustomConfig(File.ReadAllText(path));
    }
}