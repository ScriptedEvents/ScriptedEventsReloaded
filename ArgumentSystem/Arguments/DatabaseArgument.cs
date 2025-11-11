using JetBrains.Annotations;
using SER.ArgumentSystem.BaseArguments;
using SER.FileSystem.Structures;
using SER.Helpers.ResultSystem;
using SER.TokenSystem.Tokens;

namespace SER.ArgumentSystem.Arguments;

public class DatabaseArgument(string name) : Argument(name)
{
    public override string InputDescription => "Database name";
    
    [UsedImplicitly]
    public DynamicTryGet<Database> GetConvertSolution(BaseToken token)
    {
        return new(() => Database.TryGet(token.GetBestTextRepresentation(Script)));
    }
}