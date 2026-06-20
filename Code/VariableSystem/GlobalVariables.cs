using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using SER.Code.Extensions;
using SER.Code.ValueSystem;

namespace SER.Code.VariableSystem;

public static class GlobalVariables
{
    private static readonly Dictionary<VariableRepr, Value> _variables = [];
    private static DynamicGlobalPlayerVariable[] _dynamicGlobalPlayerVariables = [];
    private static readonly Dictionary<VariableRepr, Func<Value>> _dynamicGlobalPlayerVariableDict = [];
    
    public static IEnumerable<VariableRepr> Variables => _variables.Keys;

    public static void Initialize()
    {
        _variables.Clear();
        
        List<DynamicGlobalPlayerVariable> vars =
        [
            new("all", Player.ReadyList.ToArray, "Other"),
            new("allPlayers", Player.ReadyList.ToArray, "Other"),
            new("alivePlayers", () => Player.ReadyList.Where(plr => plr.IsAlive).ToArray(), "Other"),
            new("npcPlayers", () => Player.ReadyList.Where(plr => plr.IsNpc).ToArray(), "Other"),
            new("empty", () => [], "Other"),
            new("emptyPlayers", () => [], "Other"),
        ];

        vars.AddRange(
            Enum.GetValues(typeof(RoleTypeId))
            .Cast<RoleTypeId>()
            .Select(roleType =>
            {
                return new DynamicGlobalPlayerVariable(
                    roleType.ToString().LowerFirst() + "Players",
                    () => Player.ReadyList.Where(plr => plr.Role == roleType).ToArray(),
                    "Role"
                );
            }));
        
        vars.AddRange(
            Enum.GetValues(typeof(FacilityZone))
            .Cast<FacilityZone>()
            .Where(zone => zone != FacilityZone.None)
            .Select(zone =>
            {
                return new DynamicGlobalPlayerVariable(
                    zone.ToString().LowerFirst() + "Players",
                    () => Player.ReadyList.Where(plr => plr.Zone == zone).ToArray(),
                    "Facility zone"
                );
            }));
        
        vars.AddRange(
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
                if (vars.Any(v => v.Name == name))
                {
                    return null;
                }

                return new DynamicGlobalPlayerVariable(
                    name,
                    () => Player.ReadyList.Where(plr => plr.Role.GetTeam() == teamType).ToArray(),
                    "Team"
                );
            })
            .OfType<DynamicGlobalPlayerVariable>());
        
        _dynamicGlobalPlayerVariables = vars.ToArray();
        
        foreach (var v in vars)
        {
            _dynamicGlobalPlayerVariableDict[new('@', v.Name)] = () => Value.Player(v.Getter());
        }
    }

    public static void AddGlobalVariable(VariableRepr repr, Value value)
    {
        _variables[repr] = value;
    }

    public static void RemoveGlobalVariable(VariableRepr repr)
    {
        _variables.Remove(repr);
    }

    public static bool TryGetGlobalVariable(VariableRepr repr, out Value value)
    {
        return _variables.TryGetValue(repr, out value);
    }
}