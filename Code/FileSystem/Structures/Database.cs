using LabApi.Features.Wrappers;
using Newtonsoft.Json;
using SER.Code.Helpers.Extensions;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ValueSystem;
using Db = System.Collections.Generic.Dictionary<string, SER.Code.FileSystem.Structures.Database.DatabaseValue>;

namespace SER.Code.FileSystem.Structures;

public class Database
{
    public readonly struct DatabaseValue(Type originalType, object value)
    {
        public string Type { get; } = originalType.GetAccurateName();
        public object Value { get; } = value;
    }
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto
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

    public static void Create(string name)
    {
        Directory.CreateDirectory(FileSystem.DbDirPath);
        var path = Path.Combine(FileSystem.DbDirPath, $"{name}.json");
        if (File.Exists(path)) return;

        using var file = File.CreateText(path);
        file.Write("{}");
        file.Close();
    }
    
    public static TryGet<Database> TryGet(string name)
    {
        if (AllDatabases.FirstOrDefault(d => d._name == name) is { } foundDb)
        {
            return foundDb;
        }
        
        var path = Path.Combine(FileSystem.DbDirPath, $"{name}.json");
        if (!File.Exists(path))
        {
            return $"There is no database called '{name}'";
        }
        
        string content = File.ReadAllText(path);
        if (JsonConvert.DeserializeObject<Db>(content, Settings) is not { } db)
        {
            return $"Database '{name}' is corrupted!";
        }
        
        return new Database(path, db);
    }

    public Result TrySet(string key, Value value, bool save = true)
    {
        object saveVal;
        switch (value)
        {
            case LiteralValue literalValue:
                saveVal = literalValue.Value;
                break;
            case PlayerValue playerValue:
                saveVal = playerValue.Players.Select(p => p.UserId).ToArray();
                break;
            default:
                return $"Value '{value}' cannot be stored in databases";
        }
        
        _db[key] = new(value.GetType(), saveVal);
        if (save) Save();
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

        if (val.Type == typeof(PlayerValue).GetAccurateName())
        {
            if (val.Value is not string[] playerIds)
            {
                return $"Value for key '{key}' is corrupted";
            }

            return Value.Parse(Player.List.Where(p => playerIds.Contains(p.UserId)));
        }

        if (Value.Parse(val.Value) is { } value && value.GetType().GetAccurateName() == val.Type)
        {
            return value;
        }

        return $"Value for key '{key}' is corrupted";
    }

    public void Save()
    {
        string json = JsonConvert.SerializeObject(_db, Formatting.Indented, Settings);
        File.WriteAllText(_path, json);
    }
}