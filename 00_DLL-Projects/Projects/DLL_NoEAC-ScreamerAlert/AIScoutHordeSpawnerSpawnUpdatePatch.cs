using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch(typeof(AIScoutHordeSpawner), "SpawnUpdate")]
public class AIScoutHordeSpawnerSpawnUpdatePatch
{
    static void Postfix(AIScoutHordeSpawner __instance)
    {
        if (ScreamerAlertManager.Instance == null)
            return;

        var hordeListField = __instance.GetType().GetField("hordeList", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hordeList = hordeListField?.GetValue(__instance) as IList<AIScoutHordeSpawner.ZombieCommand>;
        if (hordeList == null)
        {
            return;
        }
        int i = 0;
        foreach (var zombieCmd in hordeList)
        {
            if (zombieCmd == null || zombieCmd.Zombie == null || zombieCmd.Zombie.EntityClass == null)
            {
            }
            else
            {
                string className = zombieCmd.Zombie.EntityClass.entityClassName.ToLower();
                if (!className.Contains("screamer"))
                {
                    if (ScreamerAlertManager.Instance.persistentHordeZombieIds.Add(zombieCmd.Zombie.entityId))
                    {
                        ScreamerAlertHybridRouting.NotifyVanillaPlayersOnHordeSpawn(zombieCmd.Zombie as EntityAlive);
                    }
                }
            }
            i++;
        }
    }
}
