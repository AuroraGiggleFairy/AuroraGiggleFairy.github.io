using UnityEngine;
using System.Collections.Generic;

public class ScreamerAlertManager : MonoBehaviour
{
    // Track screamer positions for sync
    public Dictionary<int, Vector3> screamerPositions = new Dictionary<int, Vector3>();
    // Synced from server (on clients)
    public Dictionary<int, Vector3> syncedScreamerPositions = new Dictionary<int, Vector3>();
    // Synced screamer IDs from server (for clients)
    public HashSet<int> syncedScreamerIds = new HashSet<int>();
    public static ScreamerAlertManager Instance;
    // Deprecated: position-based tracking
    // public Dictionary<Vector3, List<int>> targetScreamerIds = new Dictionary<Vector3, List<int>>();
    // Robust: track scout screamer entity IDs
    public HashSet<int> persistentScreamerIds = new HashSet<int>();
    // Horde tracking is now isolated; do not mix with screamer tracking
    public HashSet<int> persistentHordeZombieIds = new HashSet<int>();
    public static Dictionary<Vector3, List<int>> ClientTargetScreamerIds = new Dictionary<Vector3, List<int>>();
    private float syncTimer = 0f;
    private float serverScanTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        var worldEntities = GameManager.Instance.World?.Entities;
        if (worldEntities == null)
        {
            return;
        }
        // Server owns authoritative tracking data. Clients should consume synced state.
        if (ConnectionManager.Instance.IsServer)
        {
            serverScanTimer += Time.deltaTime;
            if (serverScanTimer >= 0.2f)
            {
                serverScanTimer = 0f;

                // Rebuild scout screamer list from world entities.
                var newScreamerIds = new HashSet<int>();
                screamerPositions.Clear();
                foreach (Entity entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity is EntityAlive alive && alive.IsScoutZombie && !entity.IsDead())
                    {
                        newScreamerIds.Add(entity.entityId);
                        screamerPositions[entity.entityId] = entity.position;
                    }
                }
                persistentScreamerIds = newScreamerIds;

                // persistentHordeZombieIds is managed by patches; remove dead/missing entries.
                var worldEntitiesForHorde = GameManager.Instance.World?.Entities;
                var toRemoveHorde = new List<int>();
                foreach (int entityId in persistentHordeZombieIds)
                {
                    if (worldEntitiesForHorde == null || !worldEntitiesForHorde.dict.TryGetValue(entityId, out var entity) || entity == null || entity.IsDead())
                    {
                        toRemoveHorde.Add(entityId);
                    }
                }
                foreach (var id in toRemoveHorde)
                    persistentHordeZombieIds.Remove(id);
            }
        }
        // Server-side: log screamer and horde counts for admin verification
        if (ScreamerAlertsController.Instance != null)
        {
            ScreamerAlertsController.Instance.UpdateAlertMessage();
            if (ConnectionManager.Instance.IsServer)
            {
                // Count all scout zombies
                int scoutCount = 0;
                int screamerCount = 0;
                var entitiesList = GameManager.Instance.World?.Entities;
                if (entitiesList != null)
                {
                    foreach (Entity entity in entitiesList.list)
                    {
                        if (entity != null && entity.GetType().Name == "EntityZombie" && entity.EntityClass != null)
                        {
                            var alive = entity as EntityAlive;
                            string className = entity.EntityClass.entityClassName.ToLower();
                            if (alive != null && alive.IsScoutZombie && !entity.IsDead())
                                scoutCount++;
                            if (className.Contains("screamer") && !entity.IsDead())
                                screamerCount++;
                        }
                    }
                }
                int hordeCount = persistentHordeZombieIds.Count;
                // (Removed all logs as requested)
            }
        }
        // Multiplayer sync and horde alert logic remain separate
        if (!ConnectionManager.Instance.IsServer || ScreamerAlertsController.Instance == null)
        {
            return;
        }
        syncTimer += Time.deltaTime;
        if (syncTimer >= 0.5f)
        {
            syncTimer = 0f;
            if (ScreamerAlertsController.Instance.hordeAlertEndTime > 0f && Time.time > ScreamerAlertsController.Instance.hordeAlertEndTime)
            {
                ScreamerAlertsController.Instance.hordeAlertPosition = Vector3.zero;
                ScreamerAlertsController.Instance.hordeAlertEndTime = 0f;
            }
            // Generate screamer alert message for sync (server-side)
            string screamerMsg = "";
            var screamerIds = persistentScreamerIds;
            var worldEntitiesForProx = GameManager.Instance.World?.Entities;
            EntityPlayer entityPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
            bool playerNearScout = false;
            if (entityPlayer != null && !entityPlayer.IsDead() && screamerIds != null)
            {
                foreach (int entityId in screamerIds)
                {
                    if (worldEntitiesForProx != null && worldEntitiesForProx.dict.TryGetValue(entityId, out var entity) && entity != null && !entity.IsDead())
                    {
                        float dist = Vector3.Distance(entityPlayer.position, entity.position);
                        if (dist <= 120f)
                        {
                            playerNearScout = true;
                        }
                    }
                }
            }
            screamerMsg = playerNearScout ? Localization.Get("ScreamerAlert_Scout") : "";
            var pkg = new NetPackageScreamerAlertSync(
                persistentScreamerIds,
                persistentHordeZombieIds,
                screamerMsg,
                ScreamerAlertsController.Instance.screamerHordeAlertMessage,
                ScreamerAlertsController.Instance.hordeAlertPosition,
                ScreamerAlertsController.Instance.hordeAlertEndTime
            );
            // Send custom sync only to clients that explicitly handshook support for this mod.
            foreach (ClientInfo ci in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
            {
                if (ScreamerAlertHybridRouting.HasClientCapability(ci))
                {
                    ci.SendPackage(pkg);
                }
            }
        }
    }

    public void RemoveScoutScreamer(int entityId, Vector3 position)
    {
        // No-op: screamer removal is now handled by persistentScreamerIds update logic
    }
}
