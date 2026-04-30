using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch(typeof(AIHordeSpawner), "Tick")]
public class AIHordeSpawnerTickPatch
{
    static void Postfix(AIHordeSpawner __instance, double _dt)
    {
        if (ScreamerAlertManager.Instance == null)
            return;

        var hordeListField = __instance.GetType().GetField("hordeList", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hordeList = hordeListField?.GetValue(__instance) as System.Collections.IList;
        int count = 0, added = 0, screamers = 0;
        if (hordeList != null)
        {
            foreach (var entityObj in hordeList)
            {
                var entity = entityObj as EntityEnemy;
                if (entity != null && entity.EntityClass != null)
                {
                    string className = entity.EntityClass.entityClassName.ToLower();
                    count++;
                    if (className.Contains("screamer"))
                    {
                        screamers++;
                        // Track scout screamers robustly by entityId
                        var alive = entity as EntityAlive;
                        if (alive != null && alive.IsScoutZombie && !entity.IsDead())
                        {
                            ScreamerAlertManager.Instance.persistentScreamerIds.Add(entity.entityId);
                        }
                    }
                    else
                    {
                        // Track horde zombies
                        if (ScreamerAlertManager.Instance.persistentHordeZombieIds.Add(entity.entityId))
                        {
                            added++;
                            ScreamerAlertHybridRouting.NotifyVanillaPlayersOnHordeSpawn(entity as EntityAlive);
                        }
                    }
                }
            }
        }
    }
}
