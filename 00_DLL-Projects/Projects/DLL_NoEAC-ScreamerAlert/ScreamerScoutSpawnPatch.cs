using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch(typeof(AIDirectorChunkEventComponent), "SpawnScouts")]
public class ScreamerScoutSpawnPatch
{
    private static void Postfix(Vector3 targetPos)
    {
        // No-op: screamer tracking is now handled by persistentScreamerIds
    }
}
