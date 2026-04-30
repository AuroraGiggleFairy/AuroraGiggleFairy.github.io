using HarmonyLib;
using UnityEngine;
using System.Reflection;

// Patch EntityFactory.CreateEntity to track all scout screamer spawns
[HarmonyPatch]
public class EntityFactoryCreateEntityPatch
{
    static MethodBase TargetMethod()
    {
        // Find the correct overload for CreateEntity(EntityCreationData)
        return AccessTools.Method(
            typeof(EntityFactory),
            "CreateEntity",
            new[] { typeof(EntityCreationData) }
        );
    }

    static void Postfix(Entity __result)
    {
        // Only track scout screamers
        if (__result != null && __result.GetType().Name == "EntityZombie")
        {
            var alive = __result as EntityAlive;
            if (alive != null && alive.IsScoutZombie && !__result.IsDead())
            {
                // Add to persistentScreamerIds
                if (ScreamerAlertManager.Instance != null)
                {
                    if (ScreamerAlertManager.Instance.persistentScreamerIds.Add(__result.entityId))
                    {
                        ScreamerAlertHybridRouting.NotifyVanillaPlayersOnScoutSpawn(alive);
                    }
                }
            }
        }
    }
}
