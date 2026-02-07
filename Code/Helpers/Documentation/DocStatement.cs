using SER.Code.TokenSystem.Tokens;

namespace SER.Code.Helpers.Documentation;

public class DocStatement : DocComponent
{
    private readonly List<DocComponent> _children = [];

    public DocStatement AddRangeIf(Func<DocComponent[]?> children)
    {
        if (children.Invoke() is {} items)
            _children.AddRange(items);
        return this;
    }
    
    public DocStatement AddIf(Func<DocComponent?> child)
    {
        if (child.Invoke() is {} item)
            _children.Add(item);
        return this;
    }
    
    public DocStatement Add(DocComponent child)
    {
        _children.Add(child);
        return this;
    }
    
    public DocStatement AddRange(DocComponent[] children)
    {
        _children.AddRange(children);
        return this;
    }
    
    public DocStatement(string keyword, params BaseToken[] init)
    {
        
    }
}