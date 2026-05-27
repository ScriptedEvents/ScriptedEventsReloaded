using YamlDotNet.Serialization;

namespace SER.Code.MethodSystem.Methods.ConfigMethods.Structures;

public class CustomConfig
{
    private readonly Dictionary<object, object>? _rootData;

    public CustomConfig(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            _rootData = new Dictionary<object, object>();
            return;
        }

        var deserializer = new DeserializerBuilder().Build();
        
        // Deserialize as a generic object, then safely cast to a dictionary
        var rawResult = deserializer.Deserialize<object>(yamlContent);
        _rootData = rawResult as Dictionary<object, object>;
    }

    /// <summary>
    /// Navigates the YAML structure using a dynamic path and returns the value.
    /// </summary>
    /// <param name="path">The sequence of keys to navigate down the YAML tree.</param>
    /// <returns>The object value if found; otherwise, null.</returns>
    public object? GetValue(params string[] path)
    {
        if (_rootData == null || path == null || path.Length == 0)
        {
            return null;
        }

        object current = _rootData;

        foreach (var key in path)
        {
            // If the current node is a dictionary, try to fetch the next nested object
            if (current is Dictionary<object, object> dict)
            {
                if (dict.TryGetValue(key, out var nextNode))
                {
                    current = nextNode;
                }
                else
                {
                    // Key wasn't found in the dictionary
                    return null;
                }
            }
            else
            {
                // We are trying to navigate deeper, but the current node isn't a container
                return null;
            }
        }

        return current;
    }
}