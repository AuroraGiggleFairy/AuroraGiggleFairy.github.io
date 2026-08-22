using HarmonyLib;

namespace CombatGlitchMitigations
{
	[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.ProcessDamageResponseLocal))]
	public static class Patch_EntityAlive_ProcessDamageResponseLocal
	{
		public static void Postfix(EntityAlive __instance, DamageResponse _dmResponse)
		{
			try
			{
			if (!CombatGlitchMitigationsLogic.IsServerContext())
			{
				return;
			}

			if (__instance == null || __instance is EntityPlayer || !_dmResponse.PainHit)
			{
				return;
			}

			if ((_dmResponse.HitBodyPart & EnumBodyPartHit.Head) == 0)
			{
				return;
			}

			if (_dmResponse.Source == null)
			{
				return;
			}

			EntityAlive attacker = __instance.world?.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive;
			if (!(attacker is EntityPlayer))
			{
				return;
			}

			CombatGlitchMitigationsLogic.StartFaceHold(__instance, attacker);
			}
			catch
			{
			}
		}
	}
}
