using System;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(AIDirectorChunkEventComponent), "SpawnScouts")]
public class ScreamerScoutSpawnPatch
{
	private static void Postfix(Vector3 targetPos)
	{
		try
		{
			if (!(ScreamerAlertManager.Instance == null))
			{
				ScreamerAlertManager.Instance.AddScreamerTarget(targetPos);
			}
		}
		catch (Exception)
		{
		}
	}
}
