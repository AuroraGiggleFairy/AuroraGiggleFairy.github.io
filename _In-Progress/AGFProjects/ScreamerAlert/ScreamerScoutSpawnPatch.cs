using System;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(AIDirectorChunkEventComponent), "SpawnScouts")]
public class ScreamerScoutSpawnPatch
{
	private static void Postfix(Vector3 targetPos)
	{
		// ...existing code...
		try
		{
			if (!( (UnityEngine.Object)(object)ScreamerAlertManager.Instance == (UnityEngine.Object)null))
			{
				ScreamerAlertManager.Instance.AddScreamerTarget(targetPos);
			}
		}
		catch (Exception)
		{
		}
	}
}
