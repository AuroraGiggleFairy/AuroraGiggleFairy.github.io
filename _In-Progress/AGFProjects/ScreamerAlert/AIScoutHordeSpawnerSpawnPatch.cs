using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(AIScoutHordeSpawner), "spawnHordeNear")]
public class AIScoutHordeSpawnerSpawnPatch
{
	private static void Postfix(World world, AIScoutHordeSpawner.ZombieCommand command, Vector3 target)
	{
		ScreamerAlertsController.Instance?.TriggerScreamerHordeAlert(target);
	}
}
