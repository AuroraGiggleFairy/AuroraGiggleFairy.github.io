using UnityEngine;
using System.Collections.Generic;

public class ScreamerAlertManager : MonoBehaviour
{
    private const float ServerScanIntervalSeconds = 0.5f;
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
    private readonly HashSet<int> scanScreamerIdsBuffer = new HashSet<int>();
    private readonly List<int> hordeCleanupBuffer = new List<int>();
    private float syncTimer = 0f;
    private float serverScanTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        GameManager gameManager = GameManager.Instance;
        World world = gameManager?.World;
        var worldEntities = world?.Entities;
        if (worldEntities == null)
        {
            return;
        }

        ConnectionManager connectionManager = ConnectionManager.Instance;
        // Server owns authoritative tracking data. Clients should consume synced state.
        if (connectionManager != null && connectionManager.IsServer)
        {
            serverScanTimer += Time.deltaTime;
            if (serverScanTimer >= ServerScanIntervalSeconds)
            {
                serverScanTimer = 0f;

                // Rebuild scout screamer list from world entities.
                scanScreamerIdsBuffer.Clear();
                screamerPositions.Clear();

                List<Entity> entities = worldEntities.list;
                for (int i = 0; i < entities.Count; i++)
                {
                    Entity entity = entities[i];
                    if (entity is EntityAlive alive && alive.IsScoutZombie && !entity.IsDead())
                    {
                        bool wasTracked = persistentScreamerIds.Contains(entity.entityId);
                        scanScreamerIdsBuffer.Add(entity.entityId);
                        screamerPositions[entity.entityId] = entity.position;

                        // Scan-based detection keeps scout whisper routing reliable even when
                        // some spawn hooks are bypassed by engine path changes.
                        if (!wasTracked)
                        {
                            ScreamerAlertHybridRouting.NotifyVanillaPlayersOnScoutSpawn(alive);
                        }
                    }
                }

                persistentScreamerIds.Clear();
                persistentScreamerIds.UnionWith(scanScreamerIdsBuffer);

                // persistentHordeZombieIds is managed by patches; remove dead/missing entries.
                hordeCleanupBuffer.Clear();
                foreach (int entityId in persistentHordeZombieIds)
                {
                    if (!worldEntities.dict.TryGetValue(entityId, out var entity) || entity == null || entity.IsDead())
                    {
                        hordeCleanupBuffer.Add(entityId);
                    }
                }

                for (int i = 0; i < hordeCleanupBuffer.Count; i++)
                {
                    persistentHordeZombieIds.Remove(hordeCleanupBuffer[i]);
                }
            }

            // Flush queued follow-up incidents (3s merge, 7s cooldown) on server.
            ScreamerAlertHybridRouting.TickQueuedIncidents();
        }

        // Multiplayer sync and horde alert logic remain separate
        if (connectionManager == null || !connectionManager.IsServer || ScreamerAlertsController.Instance == null)
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

            // Preserve exact server-side ID attribution and publish only the
            // player-specific results through a built-in game package.
            ScreamerAlertVanillaProtocol.PublishToEnhancedClients();
        }
    }

    public void RemoveScoutScreamer(int entityId, Vector3 position)
    {
        // No-op: screamer removal is now handled by persistentScreamerIds update logic
    }
}
