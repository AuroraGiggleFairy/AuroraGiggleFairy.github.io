using HarmonyLib;

[HarmonyPatch(typeof(EntityHuman), "OnEntityDeath")]
public class EntityHumanDeathPatch
{
	private static void Postfix(EntityHuman __instance)
	{
		EntityZombie entityZombie = __instance as EntityZombie;
		if (entityZombie != null && entityZombie.EntityName == "zombieScreamer" && (object)ScreamerAlertManager.Instance != null)
		{
			ScreamerAlertManager.Instance.RemoveScoutScreamer(entityZombie.entityId, entityZombie.position);
		}
	}
}
