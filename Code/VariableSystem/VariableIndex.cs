using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using SER.Code.Extensions;
using SER.Code.ScriptSystem;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Structures;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.VariableSystem;

public static class VariableIndex
{
    private static readonly Dictionary<(char, string), Variable> _globalVariables = [];
    public static IEnumerable<Variable> GlobalVariables => _globalVariables.Values;

    /// <summary>Registered names only; unlike Variable.Prefix, this never evaluates a variable.</summary>
    public static IEnumerable<(char Prefix, string Name, string? Category)> GetGlobalVariableInfo() =>
        _globalVariables.Select(pair => (pair.Key.Item1, pair.Key.Item2,
            (pair.Value as PredefinedPlayerVariable)?.Category)).ToArray();

    public static void Initialize()
    {
        _globalVariables.Clear();
        
        List<PredefinedPlayerVariable> allApiVariables =
        [
            new("all", Player.ReadyList.ToList, "Other"),
            new("allPlayers", Player.ReadyList.ToList, "Other"),
            new("alivePlayers", () => Player.ReadyList.Where(plr => plr.IsAlive).ToList(), "Other"),
            new("npcPlayers", () => Player.ReadyList.Where(plr => plr.IsNpc).ToList(), "Other"),
            new("empty", () => [], "Other"),
            new("emptyPlayers", () => [], "Other"),
        ];

        allApiVariables.AddRange(
            Enum.GetValues(typeof(RoleTypeId))
                .Cast<RoleTypeId>()
                .Select(roleType =>
                {
                    return new PredefinedPlayerVariable(
                        roleType.ToString().LowerFirst() + "Players",
                        () => Player.ReadyList.Where(plr => plr.Role == roleType).ToList(),
                        "Role"
                    );
                }));
        
        allApiVariables.AddRange(
            Enum.GetValues(typeof(FacilityZone))
                .Cast<FacilityZone>()
                .Where(zone => zone != FacilityZone.None)
                .Select(zone =>
                {
                    return new PredefinedPlayerVariable(
                        zone.ToString().LowerFirst() + "Players",
                        () => Player.ReadyList.Where(plr => plr.Zone == zone).ToList(),
                        "Facility zone"
                    );
                }));
        
        allApiVariables.AddRange(
            Enum.GetValues(typeof(Team))
                .Cast<Team>()
                .Select(teamType =>
                {
                    string name = teamType.ToString();
                    if (teamType is Team.SCPs)
                    {
                        name = "scp";
                    }
                    else if (name.EndsWith("s"))
                    {
                        name = name[..^1];
                    }

                    name = name.LowerFirst() + "Players";
                    if (allApiVariables.Any(v => v.Name == name))
                    {
                        return null;
                    }

                    return new PredefinedPlayerVariable(
                        name,
                        () => Player.ReadyList.Where(plr => plr.Role.GetTeam() == teamType).ToList(),
                        "Team"
                    );
                })
                .OfType<PredefinedPlayerVariable>());
        
        foreach (var v in allApiVariables)
        {
            // Every predefined selection is a player variable. Do not evaluate it to discover its prefix.
            _globalVariables[('@', v.Name)] = v;
        }
    }

    public static void AddGlobalVariable(Variable variable)
    {
        foreach (var runningScript in Script.RunningScripts)
        { 
            Variable.AssertNoVariableNameCollisions(variable, runningScript.LocalVariables);
        }
        
        _globalVariables[(variable.Prefix, variable.Name)] = variable;
    }

    public static void RemoveGlobalVariable(IVariableRepr variable)
    {
        _globalVariables.Remove((variable.Prefix, variable.Name));
    }

    public static bool TryGetGlobalVariable(char prefix, string name, out Variable variable)
    {
        return _globalVariables.TryGetValue((prefix, name), out variable);
    }
}
