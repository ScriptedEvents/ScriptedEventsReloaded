using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using SER.Code.Extensions;
using SER.Code.VariableSystem.Bases;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.VariableSystem;

public static class VariableIndex
{
    public static readonly List<Variable> GlobalVariables = [];

    /// <summary>
    /// A method used for documentation to verify if the variable exists.
    /// </summary>
    /// <param name="name">The name to verify.</param>
    /// <returns>A formatted player variable</returns>
    public static string DocsGet(string name)
    {
        if (name.StartsWith("@")) name = name[1..];
        
        if (GlobalVariables.FirstOrDefault(v => v.Name == name) is { } variable)
        {
            return $"@{variable.Name}";
        }
        
        throw new Exception($"Documentation tried to use variable '@{name}' which does not exist.");
    }
    
    public static void Initialize()
    {
        GlobalVariables.Clear();
        
        List<PredefinedPlayerVariable> allApiVariables =
        [
            new("all", Player.ReadyList.ToList, "Other"),
            new("allPlayers", Player.ReadyList.ToList, "Other"),
            new("alivePlayers", () => Player.ReadyList.Where(plr => plr.IsAlive).ToList(), "Other"),
            new("npcPlayers", () => Player.ReadyList.Where(plr => plr.IsNpc).ToList(), "Other"),
            new("empty", () => [], "Other")
        ];

        allApiVariables.AddRange(
            Enum.GetValues(typeof(RoleTypeId))
                .Cast<RoleTypeId>()
                .Select(roleType =>
                {
                    return new PredefinedPlayerVariable(roleType.ToString().LowerFirst() + "Players",
                        () => Player.ReadyList.Where(plr => plr.Role == roleType).ToList(),
                        "Role");
                }));
        
        allApiVariables.AddRange(
            Enum.GetValues(typeof(FacilityZone))
                .Cast<FacilityZone>()
                .Where(zone => zone != FacilityZone.None)
                .Select(zone =>
                {
                    return new PredefinedPlayerVariable(zone.ToString().LowerFirst() + "Players",
                        () => Player.ReadyList.Where(plr => plr.Zone == zone).ToList(),
                        "Facility zone");
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

                    return new PredefinedPlayerVariable(name,
                        () => Player.ReadyList.Where(plr => plr.Role.GetTeam() == teamType).ToList(),
                        "Team");
                })
                .OfType<PredefinedPlayerVariable>());
        
        GlobalVariables.AddRange(allApiVariables);
    }
}