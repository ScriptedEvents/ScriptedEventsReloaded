namespace SER.Code.VariableSystem;

public record struct VariableRepr(char Prefix, string Name)
{
    private int CachedHash
    {
        get
        {
            if (field != 0) return field;
            
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Prefix.GetHashCode();
                hash = hash * 23 + Name.GetHashCode();
            
                return field = hash;
            }
        }
    }

    public override int GetHashCode() => CachedHash;
}