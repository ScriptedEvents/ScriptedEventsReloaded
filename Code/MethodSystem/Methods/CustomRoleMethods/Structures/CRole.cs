using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using SER.Code.Extensions;
using SER.Code.Helpers;
using Random = System.Random;

namespace SER.Code.MethodSystem.Methods.CustomRoleMethods.Structures;

public class CRole
{
    private static readonly Random SpawnRandom = new();

    public CRole()
    {
        PlayerEvents.Death += OnDeath;
        PlayerEvents.ChangingRole += OnPlayerChangingRole;
        ServerEvents.RoundStarted += OnServerRoundStarted;
    }

    public void Unload()
    {
        PlayerEvents.Death -= OnDeath;
        PlayerEvents.ChangingRole -= OnPlayerChangingRole;
        ServerEvents.RoundStarted -= OnServerRoundStarted;
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
    private static readonly HashSet<Player> PlayersQueuedForCRole = [];
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
        PlayersInProcessOfReceivingCRole.Clear();
        PlayersQueuedForCRole.Clear();
        EventHandlers.Clear();
        RegisteredRoles.Values.ForEachItem(role => role.Unload());
        RegisteredRoles.Clear();
    }

    public static void Register(CRole role)
    {
        if (RegisteredRoles.TryGetValue(role.Id, out var previousRole))
        {
            Unregister(previousRole);
        }

        RegisteredRoles[role.Id] = role;
    }

    public static void Unregister(CRole role)
    {
        if (RegisteredRoles.TryGetValue(role.Id, out var registeredRole)
            && ReferenceEquals(registeredRole, role))
        {
            RegisteredRoles.Remove(role.Id);
        }

        foreach (var player in Player.ReadyList
                     .Where(player => LifeIdAssignedRoles.TryGetValue(player.LifeId, out var assignedRole)
                                      && ReferenceEquals(assignedRole, role))
                     .ToArray())
        {
            role.RemovePlayer(player);
        }

        role.Unload();
    }
    
    public void AssignPlayer(Player plr)
    {
        if (LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var previousRole))
        {
            if (ReferenceEquals(previousRole, this))
            {
                return;
            }

            LastRoles[plr] = previousRole;
            previousRole.RemovePlayer(plr);
        }

        if (PlayersInProcessOfReceivingCRole.Contains(plr))
        {
            return;
        }

        PlayersInProcessOfReceivingCRole.Add(plr);
        try
        {
            if (plr.Role != RoleType)
            {
                plr.SetRole(RoleType);
            }

            LifeIdAssignedRoles[plr.LifeId] = this;
        }
        finally
        {
            PlayersInProcessOfReceivingCRole.Remove(plr);
        }

        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            if (!LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var currentRole)
                || !ReferenceEquals(currentRole, this))
            {
                return;
            }

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
        if (!LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var currentRole)
            || !ReferenceEquals(currentRole, this))
        {
            return;
        }

        LifeIdAssignedRoles.Remove(plr.LifeId);
        
        plr.CustomInfo = "";
        plr.InfoArea = (PlayerInfoArea)~0;
        
        RunHandlers(CustomRoleEvent.Removed, plr);
    }

    private void RunHandlers(CustomRoleEvent @event, Player plr)
    {
        if (!EventHandlers.TryGetValue(@event, out var handlers)) return;
        
        foreach (var handler in handlers)
        {
            if (handler.ForRoles is not { } roles || roles.Contains(Id))
            {
                handler.Action(plr, this);
            }
        }
    }

    public void OnServerRoundStarted()
    {
        Timing.CallDelayed(Timing.WaitForOneFrame, delegate
        {
            if (RegisteredRoles.TryGetValue(Id, out var registeredRole)
                && ReferenceEquals(registeredRole, this))
            {
                HandleRoleRoundStart(this);
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
                    .Where(p => !LifeIdAssignedRoles.ContainsKey(p.LifeId))
                    .ToArray();
                
                if (pool.Length < procedural.StartSpawningWhen)
                {
                    return;
                }

                var playersToAssign = pool
                    .Where(_ => PassesSpawnChance(procedural.SpawnChance))
                    .ToList();

                playersToAssign.ShuffleList();
                playersToAssign = playersToAssign
                    .Take(GetCappedSpawnCount(playersToAssign.Count, procedural.MaxAmountToSpawn))
                    .ToList();

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
                    .Where(p => !LifeIdAssignedRoles.ContainsKey(p.LifeId))
                    .ToArray();

                var bracket = bracketSpawn.SpawnBrackets.FirstOrDefault(candidate =>
                    candidate.LowerBound <= pool.Length && pool.Length <= candidate.UpperBound);
                if (bracket is null)
                {
                    return;
                }

                var availablePlayers = pool.ToList();
                for (var index = 0; index < bracket.AmountToSpawn && availablePlayers.Count > 0; index++)
                {
                    var plr = availablePlayers.PullRandomItem();
                    role.AssignPlayer(plr);
                }

                return;
            }
        }
    }

    public void OnDeath(PlayerDeathEventArgs ev)
    {
        if (ev.Player is not {} plr) return;
        if (!LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var role)) return;
        if (!role.RemoveRoleOnDeath) return;
        
        role.RemovePlayer(plr);
    }

    public void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
    {
        if (ev.Player is not {} plr) return;

        if (PlayersInProcessOfReceivingCRole.Contains(plr))
        {
            Log.Debug($"player {plr.DisplayName} is already in process of receiving a custom role");
            return;
        }

        if (LifeIdAssignedRoles.TryGetValue(plr.LifeId, out var role))
        {
            role.RemovePlayer(plr);
            Log.Debug($"removing role {role.Id} from {plr.DisplayName}");
        }

        if (PlayersQueuedForCRole.Contains(plr))
        {
            return;
        }

        if (SpawnSystem is not ChanceSpawn chanceSpawn)
        {
            Log.Debug($"{Id} is not a chance spawn system");
            return;
        }
        
        if (ev.NewRole != chanceSpawn.RoleToReplace)
        {
            Log.Debug($"{Id} role cannot work for {plr.DisplayName}, invalid role");
            return;
        }
        if (!PassesSpawnChance(chanceSpawn.SpawnChance))
        {
            Log.Debug($"player {plr.DisplayName} failed chance spawn");
            return;
        }
        
        PlayersQueuedForCRole.Add(plr);
        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            try
            {
                if (!RegisteredRoles.TryGetValue(Id, out var registeredRole)
                    || !ReferenceEquals(registeredRole, this)
                    || plr.Role != chanceSpawn.RoleToReplace
                    || LifeIdAssignedRoles.ContainsKey(plr.LifeId))
                {
                    return;
                }

                AssignPlayer(plr);
                Log.Debug($"player {plr.DisplayName} successfully received role {Id}");
            }
            finally
            {
                PlayersQueuedForCRole.Remove(plr);
            }
        });
    }

    internal static bool PassesSpawnChance(float spawnChance, double randomValue)
    {
        return spawnChance >= 1f || spawnChance > 0f && randomValue < spawnChance;
    }

    internal static int GetCappedSpawnCount(int candidateCount, int? maximum)
    {
        return maximum is { } limit
            ? Math.Min(candidateCount, Math.Max(0, limit))
            : candidateCount;
    }

    private static bool PassesSpawnChance(float spawnChance)
    {
        lock (SpawnRandom)
        {
            return PassesSpawnChance(spawnChance, SpawnRandom.NextDouble());
        }
    }
}
