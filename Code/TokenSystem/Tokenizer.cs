using SER.Code.Helpers.Exceptions;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.TokenSystem;

public static class Tokenizer
{
    public static readonly Type[] OrderedImportanceTokensFromSingleSlices =
    [
        typeof(KeywordToken),
        typeof(BoolToken),
        typeof(MethodToken),
        typeof(FlagToken),
        typeof(FlagArgumentToken),
        typeof(CommentToken),
        typeof(SymbolToken),
        typeof(NumberToken),
        typeof(PlayerVariableToken),
        typeof(LiteralVariableToken),
        typeof(CollectionVariableToken),
        typeof(ReferenceVariableToken),
        typeof(DurationToken),
        typeof(RunFunctionToken)
    ];
    
    public static readonly Type[] OrderedImportanceTokensFromCollectionSlices =
    [
        typeof(PlayerExpressionToken),
        typeof(MethodExpressionToken),
        typeof(LiteralVariableExpressionToken),
        typeof(ParenthesesToken),
        typeof(TextToken)
    ];
    
    public static TryGet<Line[]> GetInfoFromMultipleLines(string content)
    {
        List<Line> outList = [];
        
        var lines = content.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        for (uint index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            var info = new Line
            {
                RawRepresentation = line,
                LineNumber = index + 1
            };
            
            outList.Add(info);
        }

        return outList.ToArray();
    }

    public static Result SliceLine(Line line)
    {
        if (SliceLine(line.RawRepresentation).HasErrored(out var err, out var slices))
        {
            return err;
        }
        
        line.Slices = slices.ToArray();
        return true;
    }
    
    /// <summary>
    /// Decides the line if it's collection slice or single slice.
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <returns>The slices for the specificalious line</returns>
    public static TryGet<IEnumerable<Slice>> SliceLine(string line)
    {
        List<Slice> outList = [];
        Slice? currentSlice = null;
        foreach (char currentChar in line)
        {
            if (currentSlice is null)
            {
                if (char.IsWhiteSpace(currentChar))
                {
                    continue;
                }

                currentSlice = CollectionSlice.CollectionStarters.Contains(currentChar) 
                    ? new CollectionSlice(currentChar) 
                    : new SingleSlice(currentChar);
                
                continue;
            }

            if (currentSlice.CanContinueAfterAdd(currentChar))
            {
                continue;
            }

            if (currentSlice.VerifyState().HasErrored(out var error))
            {
                return error;
            }
            
            outList.Add(currentSlice);
            currentSlice = null;
        }

        if (currentSlice is not null)
        {
            if (currentSlice.VerifyState().HasErrored(out var error))
            {
                return error;
            }
            
            outList.Add(currentSlice);
        }

        return outList;
    }

    public static Result TokenizeLine(Line line, Script scr)
    {
        if (TokenizeLine(line.Slices, scr, line.LineNumber).HasErrored(out var err, out var tokens))
        {
            return err;
        }

        line.Tokens = tokens;
        return true;
    }

    public static TryGet<BaseToken[]> TokenizeLine(string line, Script scr, uint? lineNum)
    {
        if (SliceLine(line)
            .HasErrored(out var sliceError, out var slices))
        {
            return sliceError;
        }

        return TokenizeLine(slices, scr, lineNum);
    }

    public static TryGet<BaseToken[]> TokenizeLine(IEnumerable<Slice> slices, Script scr, uint? lineNum)
    {
        var sliceArray = slices.ToArray();
        var tokens = sliceArray.Select(slice => GetTokenFromSlice(slice, scr, lineNum)).ToArray();

        var error = tokens.FirstOrDefault(t => t.HasErrored(out _))?.ErrorMsg;
        if (error is not null)
        {
            return error;
        }
        
        return tokens.Select(t => t.Value!).ToArray();
    }

    public static TryGet<BaseToken> GetTokenFromSlice(Slice slice, Script scr, uint? lineNum)
    {
        var tokenCollection = slice is CollectionSlice 
            ? OrderedImportanceTokensFromCollectionSlices 
            : OrderedImportanceTokensFromSingleSlices;
        
        foreach (var tokenType in tokenCollection)
        {
            var token = tokenType.CreateInstance<BaseToken>();
            switch (token.TryInit(slice, scr, lineNum))
            {
                case BaseToken.Success: return token;
                case BaseToken.Ignore: continue;
                case BaseToken.Error err: return err.Message;
                default: throw new AndrzejFuckedUpException();
            }
        }

        var unspecified = new BaseToken();
        unspecified.TryInit(slice, scr, lineNum);
        return unspecified;
    }
}