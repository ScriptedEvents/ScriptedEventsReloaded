using LabApi.Features.Wrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SER.Code.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using SER.Code.ValueSystem;
using Db = System.Collections.Generic.Dictionary<string, SER.Code.FileSystem.Structures.Database.DatabaseValue>;

namespace SER.Code.FileSystem.Structures;

public class Database
{
    public readonly struct DatabaseValue(string type, object value)
    {
        public string Type { get; } = type;
        public object Value { get; } = value;
    }
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        SerializationBinder = new DatabaseSerializationBinder()
    };
    
    private readonly string _path;
    private readonly string _name;
    private readonly Db _db;

    protected Database(string path, Db db)
    {
        _path = path;
        _db = db;
        _name = Path.GetFileNameWithoutExtension(path);
        AllDatabases.Add(this);
    }

    private static readonly List<Database> AllDatabases = [];

    public static Result Create(string name)
    {
        if (FileSystem.GetContainedPath(FileSystem.DbDirPath, name, ".json")
            .HasErrored(out var error, out var path))
        {
            return error;
        }

        Directory.CreateDirectory(FileSystem.DbDirPath);
        if (File.Exists(path)) return true;

        using var file = File.CreateText(path);
        file.Write("{}");
        file.Close();
        return true;
    }
    
    public static TryGet<Database> TryGet(string name)
    {
        if (AllDatabases.FirstOrDefault(d => d._name == name) is { } foundDb)
        {
            return foundDb;
        }
        
        if (FileSystem.GetContainedPath(FileSystem.DbDirPath, name, ".json")
            .HasErrored(out var pathError, out var path))
        {
            return pathError;
        }
        if (!File.Exists(path))
        {
            return $"There is no database called '{name}'";
        }
        
        string content = File.ReadAllText(path);
        try
        {
            if (JsonConvert.DeserializeObject<Db>(content, Settings) is not { } db)
            {
                return $"Database '{name}' is corrupted!";
            }
            
            return new Database(path, db);
        }
        catch (Exception ex)
        {
            return $"Database '{name}' is corrupted! {ex.Message}";
        }
    }

    public Result TrySet(string key, Value value, bool save = true)
    {
        object saveVal;
        switch (value)
        {
            case ColorValue colorValue:
                // UnityEngine.Color exposes computed properties such as linear/gamma,
                // which recurse during JSON serialization. Persist its stable SER form.
                saveVal = colorValue.StringRep;
                break;
            case LiteralValue literalValue:
                saveVal = literalValue.Value;
                break;
            case PlayerValue playerValue:
                saveVal = playerValue.Players.Select(p => p.UserId).ToArray();
                break;
            default:
                return $"Value '{value}' cannot be stored in databases";
        }
        
        var hadPreviousValue = _db.TryGetValue(key, out var previousValue);
        _db[key] = new(
            value is DynamicTextValue
                ? typeof(StaticTextValue).AccurateName
                : value.GetType().AccurateName, saveVal
            );

        if (save && Save().HasErrored(out var error))
        {
            if (hadPreviousValue)
                _db[key] = previousValue;
            else
                _db.Remove(key);

            return error;
        }

        return true;
    }

    public Result RemoveKey(string key, bool save = true)
    {
        if (!_db.TryGetValue(key, out var removedValue))
            return true;

        _db.Remove(key);

        if (save && Save().HasErrored(out var error))
        {
            _db[key] = removedValue;
            return error;
        }

        return true;
    }

    public TryGet<DatabaseValue> HasKey(string key)
    {
        if (!_db.TryGetValue(key, out var val))
        {
            return $"There is no key called '{key}' in the '{_name}' database.";
        }

        return val;
    }

    public TryGet<Value> Get(string key)
    {
        if (HasKey(key).HasErrored(out var err, out var val))
        {
            return err;
        } 
        
        if (val.Value is null)
        {
            return $"Value of key '{key}' cannot be read.";
        }

        if (val.Type == typeof(PlayerValue).AccurateName)
        {
            if (val.Value is not string[] playerIds)
            {
                return $"Value for key '{key}' is corrupted";
            }

            return Value.Parse(Player.ReadyList.Where(p => playerIds.Contains(p.UserId)));
        }

        if (val.Type == typeof(ColorValue).AccurateName)
        {
            if (val.Value is not string colorText ||
                ColorToken.TryParseColor(colorText).HasErrored(out _, out var color))
            {
                return $"Value for key '{key}' is corrupted";
            }

            return new ColorValue(color);
        }

        if (Value.Parse(val.Value) is { } value && value.GetType().AccurateName == val.Type)
        {
            return value;
        }

        return $"Value for key '{key}' is corrupted";
    }

    public Result Save()
    {
        try
        {
            string json = JsonConvert.SerializeObject(_db, Formatting.Indented, Settings);
            File.WriteAllText(_path, json);
            return true;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return $"Failed to save database '{_name}': {ex.Message}";
        }
    }

    private sealed class DatabaseSerializationBinder : ISerializationBinder
    {
        public Type BindToType(string? assemblyName, string typeName)
        {
            var qualifiedName = string.IsNullOrEmpty(assemblyName)
                ? typeName
                : $"{typeName}, {assemblyName}";
            var type = Type.GetType(qualifiedName, throwOnError: false);

            if (type is null || !IsAllowed(type))
            {
                throw new JsonSerializationException($"Type '{typeName}' is not allowed in a SER database.");
            }

            return type;
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            if (!IsAllowed(serializedType))
            {
                throw new JsonSerializationException(
                    $"Type '{serializedType.FullName}' is not allowed in a SER database.");
            }

            assemblyName = serializedType.Assembly.FullName;
            typeName = serializedType.FullName;
        }

        private static bool IsAllowed(Type type)
        {
            if (type == typeof(DatabaseValue) ||
                type == typeof(Dictionary<string, DatabaseValue>) ||
                type == typeof(string) || type == typeof(decimal) || type == typeof(bool) ||
                type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) ||
                type == typeof(ushort) || type == typeof(int) || type == typeof(uint) ||
                type == typeof(long) || type == typeof(ulong) || type == typeof(float) ||
                type == typeof(double) || type == typeof(TimeSpan))
            {
                return true;
            }

            if (type.IsEnum)
                return true;

            return type.IsArray && IsAllowed(type.GetElementType()!);
        }
    }
}
