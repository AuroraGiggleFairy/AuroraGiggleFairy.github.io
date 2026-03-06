using HarmonyLib;

[HarmonyPatch(typeof(EntityHuman), "OnEntityDeath")]
public class EntityHumanDeathPatch
{
	private static void Postfix(EntityHuman __instance)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		var entityZombie = __instance as EntityZombie;
		if (entityZombie != null && entityZombie.EntityName == "zombieScreamer")
		{
			ScreamerAlertManager.Instance?.RemoveScoutScreamer(entityZombie.entityId, entityZombie.position);
		}
	}
}
