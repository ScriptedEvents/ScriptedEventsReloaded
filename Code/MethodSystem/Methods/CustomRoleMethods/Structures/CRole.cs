using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using Random = System.Random;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

public class CRole : CustomEventsHandler
{
    public CRole()
    {
        CustomHandlersManager.RegisterEventsHandler(this);
    }
    
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
    public static readonly Dictionary<int, CRole> LifeIdAssignedRoles = [];
    public static readonly Dictionary<Player, CRole> LastRoles = [];
    public static readonly List<Player> PlayersInProcessOfReceivingCRole = [];
    public static readonly Dictionary<CustomRoleEvent, HashSet<Handler>> EventHandlers = [];

    public required string Id;
    public required string DisplayName;
    public required RoleTypeId RoleType;
    public CustomRoleSpawnSystem? SpawnSystem;
    public bool RemoveRoleOnDeath = true;

    public static void ResetAll()
    {
        LifeIdAssignedRoles.Clear();
        LastRoles.Clear();
        RegisteredRoles.Clear();
        PlayersInProcessOfReceivingCRole.Clear();
        EventHandlers.Clear();
    }
    
    public void AssignPlayer(Player plr)
    {
        if (LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var previousRole))
        {
            LastRoles[plr] = previousRole;
        }
        
        PlayersInProcessOfReceivingCRole.Add(plr);
        
        plr.SetRole(RoleType);
        LifeIdAssignedRoles[plr.LifeId] = this;
        
        PlayersInProcessOfReceivingCRole.Remove(plr);

        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            plr.InfoArea = PlayerInfoArea.CustomInfo;
            plr.CustomInfo = $"{plr.DisplayName}\n{DisplayName}";
        });
         
        RunHandlers(CustomRoleEvent.Spawned, plr);
    }

    public static void RemoveRoleFrom(Player plr)
    {
        if (!LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var role))
        {
            return;
        }
        
        role.RemovePlayer(plr);
    }

    public void RemovePlayer(Player plr)
    {
        LifeIdAssignedRoles.Remove(plr.LifeId);
        
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

    public override void OnServerRoundStarted()
    {
        Timing.CallDelayed(Timing.WaitForOneFrame, delegate
        {
            foreach (var role in RegisteredRoles.Values)
            {
                HandleRoleRoundStart(role);
            }
        });
    }

    private static void HandleRoleRoundStart(CRole role)
    {
        switch (role.SpawnSystem)
        {
            case null: return;
            case ProceduralSpawn procedural:
            {
                var pool = Player
                    .ReadyList
                    .Where(p => p.Role == procedural.RoleToReplace)
                    .ToArray();
                
                if (pool.Length < procedural.StartSpawningWhen)
                {
                    return;
                }

                var playersToAssign = pool
                    .Where(p => !LifeIdAssignedRoles.ContainsKey(p.LifeId))
                    .Where(_ => procedural.SpawnChance > new Random().NextDouble())
                    .ToList();

                playersToAssign.ShuffleList();
                if (procedural.MaxAmountToSpawn is { } maxAmountToSpawn)
                {
                    playersToAssign = playersToAssign.Take(maxAmountToSpawn).ToList();
                }

                foreach (var plr in playersToAssign)
                {
                    role.AssignPlayer(plr);
                }
                
                return;
            }
            case BracketSpawn bracketSpawn:
            {
                var pool = Player
                    .ReadyList
                    .Where(p => p.Role == bracketSpawn.RoleToReplace)
                    .ToArray();

                foreach (var bracket in bracketSpawn.SpawnBrackets)
                {
                    if (bracket.LowerBound > pool.Length || pool.Length > bracket.UpperBound)
                    {
                        return;
                    }

                    var availablePlayers = pool
                        .Where(p => !LifeIdAssignedRoles.ContainsKey(p.LifeId))
                        .ToList();

                    for (int i = 0; i < bracket.AmountToSpawn; i++)
                    {
                        if (availablePlayers.Count <= 0) return;

                        var plr = availablePlayers.PullRandomItem();
                        role.AssignPlayer(plr);
                    }
                }

                return;
            }
        }
    }

    public static void OnDeath(PlayerDeathEventArgs ev)
    {
        if (ev.Player is not {} plr) return;
        if (!LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var role)) return;
        if (!role.RemoveRoleOnDeath) return;
        
        role.RemovePlayer(plr);
    }

    public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
    {
        if (ev.Player is not {} plr) return;
        
        if (LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var role))
        {
            role.RemovePlayer(plr);
        }
        
        if (PlayersInProcessOfReceivingCRole.Contains(plr)) return;
        if (SpawnSystem is not ChanceSpawn chanceSpawn) return;
        if (ev.NewRole != chanceSpawn.RoleToReplace) return;
        if (chanceSpawn.SpawnChance <= new Random().NextDouble()) return;
        
        ev.IsAllowed = false;
        AssignPlayer(plr);
    }
}