using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

// Patch for AIDirectorChunkEventComponent.Horde.SpawnMore to track screamer horde zombies
[HarmonyPatch]
public class HordeSpawnMorePatch
{
	static HordeSpawnMorePatch()
	{
	}

	static System.Reflection.MethodBase TargetMethod()
	{
		var outerType = AccessTools.TypeByName("AIDirectorChunkEventComponent");
		if (outerType == null)
		{
			return null;
		}
		var hordeType = outerType.GetNestedType("Horde", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
		if (hordeType == null)
		{
			return null;
		}
		var method = AccessTools.Method(hordeType, "SpawnMore");
		if (method == null)
		{
		}
		else
		{
		}
		return method;
	}

	static void Postfix(object __instance)
	{
		try
		{
			var hordeField = AccessTools.Field(__instance.GetType(), "_horde");
			var hordeObj = hordeField?.GetValue(__instance);
			if (hordeObj == null)
			{
				return;
			}
			var hordeListField = AccessTools.Field(hordeObj.GetType(), "hordeList");
			var hordeListObj = hordeListField?.GetValue(hordeObj);
			if (hordeListObj == null)
			{
				var fields = hordeObj.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				foreach (var field in fields)
					{
						try { field.GetValue(hordeObj); } catch { /* handle unreadable case */ }
				}
				return;
			}
			var hordeList = hordeListObj as System.Collections.IEnumerable;
			if (hordeList == null)
			{
				return;
			}
			int count = 0, added = 0;
			foreach (var entityObj in hordeList)
			{
				count++;
				var entity = entityObj as EntityEnemy;
				if (entity != null && entity.EntityClass != null)
				{
					string className = entity.EntityClass.entityClassName.ToLower();
					if (!className.Contains("screamer"))
					{
						if (ScreamerAlertManager.Instance.persistentHordeZombieIds.Add(entity.entityId))
						{
							added++;
							ScreamerAlertHybridRouting.NotifyVanillaPlayersOnHordeSpawn(entity as EntityAlive);
						}
					}
				}
			}
		}
		catch (System.Exception)
		{
		}
	}
}

