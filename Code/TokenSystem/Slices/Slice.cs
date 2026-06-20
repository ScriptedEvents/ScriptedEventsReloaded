using System.Text;
using SER.Code.Helpers.OldResultSystem;

namespace SER.Code.TokenSystem.Slices;

public abstract class Slice(char startChar)
{
    public string RawRep => PrivateRawRepresentation.ToString();
    protected StringBuilder PrivateRawRepresentation { get; } = new(startChar.ToString());

    public abstract string Value { get; }

    public abstract bool CanContinueAfterAdd(char c);

    public abstract OldResult VerifyState();
}