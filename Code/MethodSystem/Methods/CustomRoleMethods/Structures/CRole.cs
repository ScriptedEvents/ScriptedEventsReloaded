using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

public class CRole
{
    public enum CustomRoleEvent
    {
        Spawned,
        Removed
    }

    public class Handler : IEquatable<Handler>
    {
        public required Action<Player, CRole> Action { get; init; }
        public required string Id { get; init; }
        public string[]? ForRoles { get; init; }

        public bool Equals(Handler? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Handler)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
    
    public static readonly Dictionary<string, CRole> RegisteredRoles = [];
    public static readonly Dictionary<Player, CRole> AssignedRoles = [];
    public static readonly Dictionary<Player, CRole> LastRoles = [];
    
    public static readonly Dictionary<CustomRoleEvent, List<Handler>> EventHandlers = [];

    public required string Id;
    public required string DisplayName;
    public required RoleTypeId RoleType;
    public CustomRoleSpawnSystem? SpawnInfo;
    public bool RemoveRoleOnDeath = true;

    public static void ResetAll()
    {
        AssignedRoles.Clear();
        LastRoles.Clear();
        RegisteredRoles.Clear();
    }
    
    public void AssignPlayer(Player plr)
    {
        if (AssignedRoles.TryGetValue(plr, out var previousRole))
        {
            LastRoles[plr] = previousRole;
            AssignedRoles.Remove(plr);
        }
        
        AssignedRoles.Add(plr, this);
        
        plr.SetRole(RoleType);
        
        plr.InfoArea |= PlayerInfoArea.CustomInfo;
        plr.InfoArea &= ~PlayerInfoArea.Role;
        plr.InfoArea &= ~PlayerInfoArea.Nickname;
        plr.InfoArea &= ~PlayerInfoArea.UnitName;

        plr.CustomInfo = $"{plr.DisplayName}\n{DisplayName}";
         
        RunHandlers(CustomRoleEvent.Spawned, plr);
    }

    public static void RemoveRoleFrom(Player plr)
    {
        if (!AssignedRoles.TryGetValue(plr, out var role))
        {
            return;
        }
        
        role.RemovePlayer(plr);
    }

    public void RemovePlayer(Player plr)
    {
        AssignedRoles.Remove(plr);
        
        plr.CustomInfo = "";
        plr.InfoArea = (PlayerInfoArea)~0;
        
        RunHandlers(CustomRoleEvent.Removed, plr);
    }

    private void RunHandlers(CustomRoleEvent @event, Player plr)
    {
        if (EventHandlers.TryGetValue(@event, out var handlers))
        {
            foreach (var handler in handlers)
            {
                if (handler.ForRoles is not { } roles || roles.Contains(Id))
                {
                    handler.Action(plr, this);
                }
            }
        }
    }
    
    public static void RegisterEvents()
    {
        PlayerEvents.Death += OnDeath;
        ServerEvents.RoundStarted += () => Timing.CallDelayed(Timing.WaitForOneFrame, OnRoundStarted);
    }

    private static void OnRoundStarted()
    {
        foreach (var role in RegisteredRoles.Values)
        {
            switch (role.SpawnInfo)
            {
                case null:
                    continue;
                case ProceduralSpawn procedural:
                {
                    var pool = Player.ReadyList.Where(p => p.Role == procedural.RoleToReplace).ToArray();
                    if (pool.Length < procedural.StartSpawningWhen)
                    {
                        continue;
                    }
                    
                    var playersToAssign = pool
                        .Where(p => !AssignedRoles.ContainsKey(p))
                        .Where(_ => procedural.SpawnChance > new Random().NextDouble())
                        .ToList();
                
                    playersToAssign.ShuffleList();
                    if (procedural.MaxAmountToSpawn is { } maxAmountToSpawn)
                    {
                        playersToAssign = playersToAssign.Take(maxAmountToSpawn).ToList();
                    }

                    playersToAssign.ForEach(p => role.AssignPlayer(p));
                    break;
                }
                case BracketSpawn bracketSpawn:
                {
                    var pool = Player.List.Where(p => p.Role == bracketSpawn.RoleToReplace).ToArray();
                
                    foreach (var bracket in bracketSpawn.SpawnBrackets)
                    {
                        if (bracket.LowerBound > pool.Length || pool.Length > bracket.UpperBound)
                        {
                            continue;
                        }

                        var availablePlayers = pool
                            .Where(p => !AssignedRoles.ContainsKey(p))
                            .ToList();

                        for (int i = 0; i < bracket.AmountToSpawn; i++)
                        {
                            if (availablePlayers.Count <= 0) return;
                        
                            var plr = availablePlayers.PullRandomItem();
                            role.AssignPlayer(plr);
                        }
                    }

                    break;
                }
            }
        }
    }

    public static void OnDeath(PlayerDeathEventArgs ev)
    {
        if (ev.Player is not {} plr) return;
        if (!AssignedRoles.TryGetValue(plr, out var role)) return;
        if (!role.RemoveRoleOnDeath) return;
        
        role.RemovePlayer(plr);
    }
}