using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.FileSystem.Structures;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ArgumentSystem.Arguments;

public class DatabaseArgument(string name) : Argument(name)
{
    public override string InputDescription => "Database name";

    [UsedImplicitly]
    public OldDynamicTryGet<Database> GetConvertSolution(BaseToken token)
    {
        return new(() => Database.TryGet(token.BestStaticTextRepr()));
    }
}