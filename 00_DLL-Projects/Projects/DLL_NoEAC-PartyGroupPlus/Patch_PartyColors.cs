using HarmonyLib;

namespace PartyGroupPlus
{
	[HarmonyPatch(typeof(Party), nameof(Party.AddPlayer))]
	public static class Patch_Party_AddPlayer_Color
	{
		public static void Postfix(Party __instance, EntityPlayer player, bool __result)
		{
			if (!__result || player == null)
			{
				return;
			}

			if (PartyGroupPlusColorStore.IsServer)
			{
				PartyGroupPlusColorStore.ServerAssignDefault(player);
				PartyGroupPlusColorStore.ServerSendPartySync(player);
			}
			else
			{
				EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
				if (local != null && local.entityId == player.entityId)
				{
					PartyGroupPlusColorStore.RequestSync();
				}
			}

			PartyGroupPlusColorStore.ApplyToParty(__instance);
		}
	}

	[HarmonyPatch(typeof(Party), "UpdateMemberList")]
	public static class Patch_Party_UpdateMemberList_Color
	{
		public static void Postfix(Party __instance)
		{
			PartyGroupPlusColorStore.ApplyToParty(__instance);
		}
	}

	[HarmonyPatch(typeof(XUiC_PartyEntry), "GetBindingValueInternal")]
	public static class Patch_PartyEntry_ArrowColor
	{
		public static void Postfix(XUiC_PartyEntry __instance, ref string value, string bindingName, ref bool __result)
		{
			if (bindingName != "arrowcolor" || __instance.Player == null)
			{
				return;
			}

			if (!PartyGroupPlusColorStore.TryGet(__instance.Player.entityId, out int color))
			{
				EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
				if (local == null || local.entityId != __instance.Player.entityId)
				{
					return;
				}

				color = PartyGroupPlusColorStore.GetLocalDisplayColor();
			}

			value = PartyGroupPlusPalette.ToBinding(color);
			__result = true;
		}
	}
}
