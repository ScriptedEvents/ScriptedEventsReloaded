using System.Collections;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.Serializable.Schematics;
using ProjectMER.Features.ToolGun;
using UnityEngine;
using SerFileSystem = SER.Code.FileSystem.FileSystem;

namespace SER.Code.Integrations.Mer;

/// <summary>
/// Late-bound boundary for the optional ProjectMER dependency.
/// Keep all ProjectMER types inside synchronous method bodies and never add ProjectMER-typed fields.
/// </summary>
internal static class MerBridge
{
    internal static void LoadMap(string mapName)
    {
        MapUtils.LoadMap(mapName);
    }

    internal static bool UnloadMap(string mapName)
    {
        return MapUtils.UnloadMap(mapName);
    }

    internal static void SaveMap(string mapName)
    {
        MapUtils.SaveMap(mapName);
    }

    internal static void MergeMaps(string outputMapName, string[] inputMapNames)
    {
        if (inputMapNames.Length < 2)
            throw new InvalidOperationException("At least two input maps are required.");

        MapSchematic output = new(outputMapName);
        foreach (string inputMapName in inputMapNames)
        {
            MapSchematic input = MapUtils.GetMapData(inputMapName);
            output.Merge(input);
        }

        if (SerFileSystem.GetContainedPath(ProjectMER.ProjectMER.MapsDir, outputMapName, ".yml")
            .HasErrored(out var pathError, out var path))
        {
            throw new ArgumentException(pathError, nameof(outputMapName));
        }

        File.WriteAllText(path, YamlParser.Serializer.Serialize(output));
    }

    internal static string[] GetAvailableMapNames()
    {
        List<string> names = [];
        string[] files = Directory.GetFiles(ProjectMER.ProjectMER.MapsDir, "*.yml", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
            names.Add(Path.GetFileNameWithoutExtension(file));

        return names.ToArray();
    }

    internal static string[] GetAvailableSchematicNames()
    {
        return MapUtils.GetAvailableSchematicNames();
    }

    internal static MERMap[] GetLoadedMaps()
    {
        List<MERMap> maps = [];
        foreach (KeyValuePair<string, MapSchematic> entry in MapUtils.LoadedMaps)
            maps.Add(new MERMap(entry.Key, entry.Value));

        return maps.ToArray();
    }

    internal static MERMap GetLoadedMap(string mapName)
    {
        if (!MapUtils.LoadedMaps.TryGetValue(mapName, out MapSchematic map))
            throw new KeyNotFoundException($"MER map '{mapName}' is not loaded.");

        return new MERMap(mapName, map);
    }

    internal static MERMap ReadMapData(string mapName)
    {
        return new MERMap(mapName, MapUtils.GetMapData(mapName));
    }

    internal static MERObject[] GetObjects(string mapName, string id, string type)
    {
        MapSchematic map = (MapSchematic)GetLoadedMapObject(mapName);
        List<MERObject> objects = [];
        foreach (MapEditorObject mapObject in map.SpawnedObjects)
        {
            if (!string.IsNullOrEmpty(id) && !mapObject.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                continue;

            string objectType = GetObjectTypeName(mapObject.Base);
            if (!string.IsNullOrEmpty(type) &&
                !type.Equals("any", StringComparison.OrdinalIgnoreCase) &&
                !objectType.Equals(type, StringComparison.OrdinalIgnoreCase))
                continue;

            objects.Add(WrapMapObject(mapObject));
        }

        return objects.ToArray();
    }

    internal static MERObjectDefinition GetObjectDefinition(string mapName, string id)
    {
        MapSchematic map = (MapSchematic)GetLoadedMapObject(mapName);
        SerializableObject definition = (SerializableObject)GetDefinition(map, id, out string canonicalId);
        return WrapDefinition(mapName, canonicalId, definition);
    }

    internal static MERObjectDefinition CreateObject(
        string objectType,
        string mapName,
        string id,
        float positionX,
        float positionY,
        float positionZ,
        string schematicName)
    {
        if (!Enum.TryParse(objectType, true, out ToolGunObjectType parsedType) ||
            !Enum.IsDefined(typeof(ToolGunObjectType), parsedType))
        {
            throw new ArgumentException($"'{objectType}' is not a valid MER object type.");
        }

        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N")[..8];

        MapSchematic map = (MapSchematic)GetOrCreateLoadedMap(mapName);
        if (ContainsDefinition(map, id))
            throw new InvalidOperationException($"MER map '{mapName}' already contains an object with ID '{id}'.");

        Vector3 absolutePosition = new(positionX, positionY, positionZ);
        Room room = RoomExtensions.GetRoomAtPosition(absolutePosition);
        Vector3 relativePosition = room.Name == MapGeneration.RoomName.Outside
            ? absolutePosition
            : room.Transform.InverseTransformPoint(absolutePosition);

        Type definitionType = ToolGunItem.TypesDictionary[parsedType];
        if (Activator.CreateInstance(definitionType) is not SerializableObject definition)
            throw new InvalidOperationException($"MER could not create an object of type '{objectType}'.");

        definition.Room = room.GetRoomStringId();
        definition.Index = room.GetRoomIndex();
        definition.Position = relativePosition;

        if (definition is SerializablePlayerSpawnpoint)
            definition.Position += Vector3.up * 0.01f;
        else if (definition is SerializableTeleport)
            definition.Position += Vector3.up;
        else if (definition is SerializableSchematic schematic)
            schematic.SchematicName = schematicName;

        if (!map.TryAddElement(id, definition))
            throw new InvalidOperationException($"MER could not add '{id}' to map '{mapName}'.");

        map.SpawnObject(id, definition);
        return WrapDefinition(mapName, id, definition);
    }

    internal static void DeleteObject(object reference)
    {
        GetReferenceIdentity(reference, out string mapName, out string id);
        MapSchematic map = (MapSchematic)GetLoadedMapObject(mapName);
        _ = GetDefinition(map, id, out string canonicalId);
        if (!map.TryRemoveElement(canonicalId))
            throw new KeyNotFoundException($"MER object '{id}' does not exist in map '{mapName}'.");

        map.DestroyObject(canonicalId);
    }

    internal static MERObjectDefinition RenameObject(object reference, string newId)
    {
        GetReferenceIdentity(reference, out string mapName, out string oldId);
        MapSchematic map = (MapSchematic)GetLoadedMapObject(mapName);
        if (ContainsDefinition(map, newId))
            throw new InvalidOperationException($"MER map '{mapName}' already contains an object with ID '{newId}'.");

        SerializableObject definition = (SerializableObject)GetDefinition(map, oldId, out string canonicalOldId);
        if (!map.TryAddElement(newId, definition))
            throw new InvalidOperationException($"MER could not assign ID '{newId}'.");

        if (!map.TryRemoveElement(canonicalOldId))
        {
            map.TryRemoveElement(newId);
            throw new InvalidOperationException($"MER could not remove the previous ID '{canonicalOldId}'.");
        }

        map.Reload();
        return WrapDefinition(mapName, newId, definition);
    }

    internal static MERObjectDefinition MoveObjectToMap(object reference, string newMapName, string newId)
    {
        GetReferenceIdentity(reference, out string oldMapName, out string oldId);
        if (string.IsNullOrWhiteSpace(newId))
            newId = oldId;

        MapSchematic oldMap = (MapSchematic)GetLoadedMapObject(oldMapName);
        MapSchematic newMap = (MapSchematic)GetOrCreateLoadedMap(newMapName);
        if (ContainsDefinition(newMap, newId))
            throw new InvalidOperationException($"MER map '{newMapName}' already contains an object with ID '{newId}'.");

        SerializableObject definition =
            (SerializableObject)GetDefinition(oldMap, oldId, out string canonicalOldId);
        if (!newMap.TryAddElement(newId, definition))
            throw new InvalidOperationException($"MER could not move '{oldId}' to map '{newMapName}'.");

        if (!oldMap.TryRemoveElement(canonicalOldId))
        {
            newMap.TryRemoveElement(newId);
            throw new InvalidOperationException($"MER could not remove '{canonicalOldId}' from map '{oldMapName}'.");
        }

        oldMap.Reload();
        if (!ReferenceEquals(oldMap, newMap))
            newMap.Reload();

        return WrapDefinition(newMapName, newId, definition);
    }

    internal static void RefreshObject(object reference)
    {
        if (TryUnwrapMapObject(reference, out object rawMapObject) && rawMapObject is MapEditorObject mapObject)
        {
            mapObject.UpdateObjectAndCopies();
            return;
        }

        GetReferenceIdentity(reference, out string mapName, out string id);
        MapSchematic map = (MapSchematic)GetLoadedMapObject(mapName);
        foreach (MapEditorObject spawned in map.SpawnedObjects)
        {
            if (!spawned.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                continue;

            spawned.UpdateObjectAndCopies();
            return;
        }

        SerializableObject definition = (SerializableObject)GetDefinition(map, id, out string canonicalId);
        map.SpawnObject(canonicalId, definition);
    }

    internal static void SetPosition(object reference, string mode, float x, float y, float z)
    {
        Vector3 value = new(x, y, z);
        if (TryGetSchematic(reference, out object rawSchematic) && rawSchematic is SchematicObject schematic)
        {
            schematic.Position = mode.Equals("add", StringComparison.OrdinalIgnoreCase)
                ? schematic.Position + value
                : value;
            return;
        }

        SerializableObject definition = (SerializableObject)GetDefinitionFromReference(reference);
        definition.Position = mode.Equals("add", StringComparison.OrdinalIgnoreCase)
            ? definition.Position + value
            : value;
        RefreshObject(reference);
    }

    internal static void SetRotation(object reference, string mode, float x, float y, float z)
    {
        Vector3 value = new(x, y, z);
        if (TryGetSchematic(reference, out object rawSchematic) && rawSchematic is SchematicObject schematic)
        {
            schematic.EulerAngles = mode.Equals("add", StringComparison.OrdinalIgnoreCase)
                ? schematic.EulerAngles + value
                : value;
            return;
        }

        SerializableObject definition = (SerializableObject)GetDefinitionFromReference(reference);
        definition.Rotation = mode.Equals("add", StringComparison.OrdinalIgnoreCase)
            ? definition.Rotation + value
            : value;
        RefreshObject(reference);
    }

    internal static void SetScale(object reference, string mode, float x, float y, float z)
    {
        Vector3 value = new(x, y, z);
        if (TryGetSchematic(reference, out object rawSchematic) && rawSchematic is SchematicObject schematic)
        {
            schematic.Scale = mode.Equals("add", StringComparison.OrdinalIgnoreCase)
                ? schematic.Scale + value
                : value;
            return;
        }

        SerializableObject definition = (SerializableObject)GetDefinitionFromReference(reference);
        definition.Scale = mode.Equals("add", StringComparison.OrdinalIgnoreCase)
            ? definition.Scale + value
            : value;
        RefreshObject(reference);
    }

    internal static MERSchematic SpawnSchematic(
        string schematicName,
        float positionX,
        float positionY,
        float positionZ,
        float rotationX,
        float rotationY,
        float rotationZ,
        float scaleX,
        float scaleY,
        float scaleZ)
    {
        SchematicObject schematic = ObjectSpawner.SpawnSchematic(
            schematicName,
            new Vector3(positionX, positionY, positionZ),
            new Vector3(rotationX, rotationY, rotationZ),
            new Vector3(scaleX, scaleY, scaleZ));

        if (schematic == null)
            throw new InvalidOperationException($"MER could not spawn schematic '{schematicName}'.");

        return new MERSchematic(schematic.Name, schematic);
    }

    internal static void DestroySchematic(object reference)
    {
        SchematicObject schematic = (SchematicObject)GetSchematic(reference);
        schematic.Destroy();
    }

    internal static MERSchematicBlock[] GetSchematicBlocks(object reference)
    {
        SchematicObject schematic = (SchematicObject)GetSchematic(reference);
        List<MERSchematicBlock> blocks = [];
        int index = 1;
        foreach (GameObject block in schematic.AttachedBlocks)
        {
            blocks.Add(new MERSchematicBlock(schematic.Name, index, block.name, block));
            index++;
        }

        return blocks.ToArray();
    }

    internal static MERAnimator[] GetAnimators(object reference)
    {
        SchematicObject schematic = (SchematicObject)GetSchematic(reference);
        IReadOnlyList<Animator> animators = schematic.AnimationController.Animators;
        List<MERAnimator> result = [];
        for (int index = 0; index < animators.Count; index++)
        {
            Animator animator = animators[index];
            result.Add(new MERAnimator(schematic.Name, index + 1, animator.name, animator));
        }

        return result.ToArray();
    }

    internal static void PlayAnimation(object reference, string stateName, int animatorIndex, string animatorName)
    {
        SchematicObject schematic = (SchematicObject)GetSchematic(reference);
        if (!string.IsNullOrWhiteSpace(animatorName))
        {
            schematic.AnimationController.Play(stateName, animatorName);
            return;
        }

        schematic.AnimationController.Play(stateName, ToZeroBasedAnimatorIndex(animatorIndex));
    }

    internal static void SetAnimationBool(
        object reference,
        string parameterName,
        bool state,
        int animatorIndex)
    {
        SchematicObject schematic = (SchematicObject)GetSchematic(reference);
        schematic.AnimationController.Play(parameterName, state, ToZeroBasedAnimatorIndex(animatorIndex));
    }

    internal static void StopAnimation(object reference, int animatorIndex, string animatorName)
    {
        SchematicObject schematic = (SchematicObject)GetSchematic(reference);
        if (!string.IsNullOrWhiteSpace(animatorName))
        {
            schematic.AnimationController.Stop(animatorName);
            return;
        }

        schematic.AnimationController.Stop(ToZeroBasedAnimatorIndex(animatorIndex));
    }

    internal static void SetSchematicVisibility(Player[] players, object reference, bool visible)
    {
        SchematicObject schematic = (SchematicObject)GetSchematic(reference);
        foreach (Player player in players)
        {
            if (visible)
                player.SpawnSchematic(schematic);
            else
                player.DestroySchematic(schematic);
        }
    }

    private static object GetLoadedMapObject(string mapName)
    {
        if (!MapUtils.LoadedMaps.TryGetValue(mapName, out MapSchematic map))
            throw new KeyNotFoundException($"MER map '{mapName}' is not loaded.");

        return map;
    }

    private static object GetOrCreateLoadedMap(string mapName)
    {
        if (MapUtils.LoadedMaps.TryGetValue(mapName, out MapSchematic loaded))
            return loaded;

        MapSchematic map;
        if (!MapUtils.TryGetMapData(mapName, out map))
            map = new MapSchematic(mapName);

        MapUtils.LoadedMaps.Add(mapName, map);
        return map;
    }

    private static bool ContainsDefinition(object map, string id)
    {
        return TryGetDefinition(map, id, out _, out _);
    }

    private static object GetDefinition(object rawMap, string id)
    {
        return GetDefinition(rawMap, id, out _);
    }

    private static object GetDefinition(object rawMap, string id, out string canonicalId)
    {
        MapSchematic map = (MapSchematic)rawMap;
        if (TryGetDefinition(map, id, out object definition, out canonicalId))
            return definition;

        throw new KeyNotFoundException($"MER object '{id}' does not exist in map '{map.Name}'.");
    }

    private static bool TryGetDefinition(
        object rawMap,
        string id,
        out object definition,
        out string canonicalId)
    {
        MapSchematic map = (MapSchematic)rawMap;
        if (TryGetDictionaryValue(map.Primitives, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Lights, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Doors, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Workstations, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.ItemSpawnpoints, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.PlayerSpawnpoints, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Capybaras, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Texts, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Interactables, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Schematics, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Scp079Cameras, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.ShootingTargets, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Teleports, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Lockers, id, out definition, out canonicalId)) return true;
        if (TryGetDictionaryValue(map.Waypoints, id, out definition, out canonicalId)) return true;

        definition = null!;
        canonicalId = string.Empty;
        return false;
    }

    private static bool TryGetDictionaryValue(
        IDictionary dictionary,
        string id,
        out object definition,
        out string canonicalId)
    {
        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key is not string key
                || !key.Equals(id, StringComparison.OrdinalIgnoreCase)
                || entry.Value is null)
            {
                continue;
            }

            definition = entry.Value;
            canonicalId = key;
            return true;
        }

        definition = null!;
        canonicalId = string.Empty;
        return false;
    }

    private static MERObject WrapMapObject(object rawMapObject)
    {
        MapEditorObject mapObject = (MapEditorObject)rawMapObject;
        return new MERObject(
            mapObject.MapName,
            mapObject.Id,
            GetObjectTypeName(mapObject.Base),
            mapObject,
            mapObject.Base);
    }

    private static MERObjectDefinition WrapDefinition(
        string mapName,
        string id,
        object definition)
    {
        return new MERObjectDefinition(mapName, id, GetObjectTypeName(definition), definition);
    }

    private static string GetObjectTypeName(object definition)
    {
        string name = definition.GetType().Name;
        const string prefix = "Serializable";
        return name.StartsWith(prefix, StringComparison.Ordinal) ? name[prefix.Length..] : name;
    }

    private static void GetReferenceIdentity(object reference, out string mapName, out string id)
    {
        if (reference is MERObject mapObject)
        {
            mapName = mapObject.MapName;
            id = mapObject.Id;
            return;
        }

        if (reference is MERObjectDefinition definition)
        {
            mapName = definition.MapName;
            id = definition.Id;
            return;
        }

        if (reference is MapEditorObject rawMapObject)
        {
            mapName = rawMapObject.MapName;
            id = rawMapObject.Id;
            return;
        }

        throw new ArgumentException("The reference is not a MER map object or object definition reference.");
    }

    private static object GetDefinitionFromReference(object reference)
    {
        if (reference is MERObject mapObject && mapObject.Definition is SerializableObject mapDefinition)
            return mapDefinition;

        if (reference is MERObjectDefinition definition && definition.Object is SerializableObject rawDefinition)
            return rawDefinition;

        if (reference is MapEditorObject rawMapObject)
            return rawMapObject.Base;

        if (reference is SerializableObject serializableObject)
            return serializableObject;

        throw new ArgumentException("The reference is not a MER map object or object definition reference.");
    }

    private static bool TryUnwrapMapObject(object reference, out object rawMapObject)
    {
        if (reference is MERObject mapObject)
        {
            rawMapObject = mapObject.Object;
            return true;
        }

        if (reference is MapEditorObject)
        {
            rawMapObject = reference;
            return true;
        }

        rawMapObject = null!;
        return false;
    }

    private static object GetSchematic(object reference)
    {
        if (TryGetSchematic(reference, out object rawSchematic) && rawSchematic is SchematicObject schematic)
            return schematic;

        throw new ArgumentException("The reference is not a MER schematic reference.");
    }

    private static bool TryGetSchematic(object reference, out object rawSchematic)
    {
        if (reference is MERSchematic schematic)
        {
            rawSchematic = schematic.Object;
            return true;
        }

        if (reference is SchematicObject)
        {
            rawSchematic = reference;
            return true;
        }

        rawSchematic = null!;
        return false;
    }

    private static int ToZeroBasedAnimatorIndex(int oneBasedIndex)
    {
        if (oneBasedIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(oneBasedIndex), "Animator index starts at 1.");

        return oneBasedIndex - 1;
    }
}
