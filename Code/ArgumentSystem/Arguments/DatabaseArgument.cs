using JetBrains.Annotations;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FileSystem.Structures;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class DatabaseArgument(string name) : Argument(name)
{
    public override string InputDescription => "Database name";
    
    [UsedImplicitly]
    public DynamicTryGet<Database> GetConvertSolution(BaseToken token)
    {
        return new(() => Database.TryGet(token.GetBestTextRepresentation(Script)));
    }
}