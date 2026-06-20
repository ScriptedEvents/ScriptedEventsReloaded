using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.TokenSystem.Slices;
using SER.Code.TokenSystem.Structures;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.ExpressionTokens;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;

namespace SER.Code.TokenSystem;

public static class Tokenizer
{
    public static readonly Type[] OrderedImportanceTokensFromSingleSlices =
    [
        typeof(ContextableKeywordToken),
        typeof(BoolToken),
        typeof(MethodToken),
        typeof(FlagToken),
        typeof(FlagArgumentToken),
        typeof(NumberToken),
        typeof(ColorToken),
        typeof(CommentToken),
        typeof(SymbolToken),
        typeof(PlayerVariableToken),
        typeof(LiteralVariableToken),
        typeof(CollectionVariableToken),
        typeof(ReferenceVariableToken),
        typeof(DurationToken),
        typeof(RunFunctionToken),
        typeof(AllToken)
    ];
    
    public static readonly Type[] OrderedImportanceTokensFromCollectionSlices =
    [
        typeof(ExpressionToken),
        typeof(ParenthesesToken),
        typeof(TextToken)
    ];
    
    public static Line[] GetInfoFromMultipleLines(string content)
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

    public static OldResult SliceLine(Line line)
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
    public static OldTryGet<IEnumerable<Slice>> SliceLine(string line)
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

    public static OldResult TokenizeLine(Line line, Script scr)
    {
        if (TokenizeLine(line.Slices, scr, line.LineNumber).HasErrored(out var err, out var tokens))
        {
            return err;
        }

        line.Tokens = tokens;
        return true;
    }

    public static OldTryGet<BaseToken[]> TokenizeLine(string line, Script scr, uint? lineNum)
    {
        if (SliceLine(line).HasErrored(out var sliceError, out var slices))
        {
            return sliceError;
        }

        return TokenizeLine(slices, scr, lineNum);
    }

    public static OldTryGet<BaseToken[]> TokenizeLine(IEnumerable<Slice> slices, Script scr, uint? lineNum)
    {
        var sliceArray = slices.ToArray();
        List<BaseToken> outList = [];
        foreach (var slice in sliceArray)
        {
            if (GetTokenFromSlice(slice, scr, lineNum).HasErrored(out var error, out var token))
            {
                return $"Value '{slice.RawRep}' is invalid.".AsOldError() + error.AsOldError();
            }
            
            outList.Add(token);
        }
        
        return outList.ToArray();
    }

    public static OldTryGet<BaseToken> GetTokenFromSlice(Slice slice, Script? scr, uint? lineNum)
    {
        var tokenCollection = slice is CollectionSlice 
            ? OrderedImportanceTokensFromCollectionSlices 
            : OrderedImportanceTokensFromSingleSlices;
        
        foreach (var tokenType in tokenCollection)
        {
            var token = tokenType.CreateInstance<BaseToken>();
            switch (token.TryInit(slice, scr!, lineNum))
            {
                case BaseToken.Success: return token;
                case BaseToken.Ignore: continue;
                case BaseToken.Error err: return err.Message;
                default: throw new AndrzejFuckedUpException();
            }
        }

        var unspecified = new BaseToken();
        unspecified.TryInit(slice, scr!, lineNum);
        return unspecified;
    }

    public static OldTryGet<BaseToken> GetTokenFromString(string str, Script? scr, uint? lineNum)
    {
        if (SliceLine(str).HasErrored(out var err, out var slices))
        {
            return err;
        }

        var bettaSlices = slices as Slice[] ?? slices.ToArray();
        if (bettaSlices.Length > 1)
        {
            return $"Value '{str}' contains multiple slices.";
        }
        
        return GetTokenFromSlice(bettaSlices.First(), scr, lineNum);
    }
}