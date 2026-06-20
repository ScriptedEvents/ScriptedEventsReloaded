using System.Runtime.CompilerServices;

#pragma warning disable CS0169 // Field is never used

namespace SER.Code.ValueSystem;

public struct ValuePrefixes
{
    private char _item0;
    private char _item1;
    private char _item2;
    private char _item3;
    
    private byte _count; 
    
    public readonly int Length => _count;
    
    public void Add(char value)
    {
        if (_count >= 4)
        {
            throw new InvalidOperationException("ValuePrefixes is full! Max capacity is 4.");
        }
        
        Unsafe.Add(ref _item0, _count) = value;
        _count++;
    }
    
    public readonly ReadOnlySpan<char> AsSpan()
    {
        unsafe
        {
            fixed (char* p = &_item0)
            {
                return new ReadOnlySpan<char>(p, _count);
            }
        }
    }
}