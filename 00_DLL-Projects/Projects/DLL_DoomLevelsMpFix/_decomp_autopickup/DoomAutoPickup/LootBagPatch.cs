using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomAutoPickup;

[HarmonyPatch(typeof(EntityLootContainer), "Start")]
public static class LootBagPatch
{
	private const int TriggerLayer = 29;

	private const float TriggerRadius = 0.8f;

	private static readonly HashSet<string> AutoPickupClasses = new HashSet<string> { "EntityLootContainerFormerSoldierBM", "EntityLootContainerFormerSergeantBM", "EntityLootContainerFormerCommandoBM" };

	private static void Postfix(EntityLootContainer __instance)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)__instance == (Object)null) && AutoPickupClasses.Contains(ClassName(__instance)))
		{
			AutoPickup autoPickup = ((Component)__instance).gameObject.GetComponent<AutoPickup>();
			if ((Object)(object)autoPickup == (Object)null)
			{
				autoPickup = ((Component)__instance).gameObject.AddComponent<AutoPickup>();
			}
			autoPickup.Configure(__instance);
			GameObject val = new GameObject("DoomAutoPickupTrigger");
			val.transform.SetParent(((Component)__instance).transform, false);
			val.layer = 29;
			SphereCollider obj = val.AddComponent<SphereCollider>();
			((Collider)obj).isTrigger = true;
			obj.radius = 0.8f;
			val.AddComponent<AutoPickupRelay>().Owner = autoPickup;
		}
	}

	private static string ClassName(EntityLootContainer bag)
	{
		return EntityClass.list[((Entity)bag).entityClass]?.entityClassName;
	}
}
