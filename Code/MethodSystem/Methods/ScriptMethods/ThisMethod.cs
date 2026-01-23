using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.FlagSystem;
using SER.Code.Helpers.Exceptions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.ScriptSystem;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.ScriptMethods;

[UsedImplicitly]
public class ThisMethod : ReturningMethod
{
    public override string Description => "Returns info about the current script";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("info to receive",
            "flags",
            new("caller","The name of the script that called this script"),
            Option.Enum<RunContext>("context"),
            new("duration","The amount of time the script's been running for"),
            "name",
            new("path", "The path to the script on the local directory of the server"),
            new("variables",$"Returns a {nameof(CollectionValue)} containing the names of all the variables in the script"))
    ];

    public override TypeOfValue Returns => new TypesOfValue([
        typeof(CollectionValue),
        typeof(TextValue),
        typeof(DurationValue),
    ]);

    public override void Execute()
    {
        ReturnValue = Args.GetOption("info to receive") switch
        {
            "flags" => new CollectionValue(ScriptFlagHandler.GetScriptFlags(Script.Name)
                .Select(f => f.GetType().Name.Replace("Flag", ""))),
            "caller" => new TextValue(Script.Caller?.Name ?? "none"),
            "context" => new TextValue(Script.Context.ToString()),
            "duration" => new DurationValue(Script.TimeRunning),
            "name" => new TextValue(Script.Name),
            "path" => new TextValue(FileSystem.FileSystem.GetScriptPath(Script)),
            "variables" => new CollectionValue(Script.Variables.Select(v => v.Prefix + v.Name)),
            _ => throw new TosoksFuckedUpException("out of order")
        };
    }
}