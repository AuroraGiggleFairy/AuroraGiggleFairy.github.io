using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace PartyGroupPlus
{
	[HarmonyPatch(typeof(Party), nameof(Party.IsFull))]
	public static class Patch_Party_IsFull
	{
		public static bool Prefix(ref bool __result)
		{
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(Party), nameof(Party.AddPlayer))]
	public static class Patch_Party_AddPlayer
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			int replaced = 0;
			foreach (CodeInstruction instruction in instructions)
			{
				if (replaced == 0 && instruction.opcode == OpCodes.Ldc_I4_8)
				{
					replaced++;
					yield return new CodeInstruction(OpCodes.Ldc_I4, int.MaxValue);
					continue;
				}

				yield return instruction;
			}

			if (replaced == 0)
			{
				Debug.LogWarning("PartyGroupPlus: AddPlayer cap (8) was not found; party size may still be 8.");
			}
		}
	}

	[HarmonyPatch(typeof(Party), nameof(Party.GetPartyXP))]
	public static class Patch_Party_GetPartyXP
	{
		public static bool Prefix(EntityPlayer player, int startingXP, ref int __result)
		{
			__result = PartyGroupPlusXP.SplitKillXP(player, startingXP);
			return false;
		}
	}

	[HarmonyPatch(typeof(GameManager), nameof(GameManager.SharedKillServer))]
	public static class Patch_GameManager_SharedKillServer
	{
		public static bool Prefix(GameManager __instance, int _entityID, int _killerID, float _xpModifier)
		{
			try
			{
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
						NetPackageManager.GetPackage<NetPackageSharedPartyKill>().Setup(_entityID, _killerID));
					return false;
				}

				World world = __instance.World;
				EntityPlayer killer = world.GetEntity(_killerID) as EntityPlayer;
				EntityAlive killed = world.GetEntity(_entityID) as EntityAlive;
				if (killer == null || killed == null || killer.Party == null)
				{
					return false;
				}

				int experienceValue = EntityClass.list[killed.entityClass].ExperienceValue;
				experienceValue = (int)EffectManager.GetValue(
					PassiveEffects.ExperienceGain,
					killed.inventory.holdingItemItemValue,
					experienceValue,
					killed);
				if (_xpModifier != 1f)
				{
					experienceValue = (int)((float)experienceValue * _xpModifier + 0.5f);
				}

				experienceValue = PartyGroupPlusXP.SplitKillXP(killer, experienceValue);
				int range = GameStats.GetInt(EnumGameStats.PartySharedKillRange);
				for (int i = 0; i < killer.Party.MemberList.Count; i++)
				{
					EntityPlayer member = killer.Party.MemberList[i];
					if (member == null || member == killer
						|| Vector3.Distance(killer.position, member.position) >= range)
					{
						continue;
					}

					if (world.IsLocalPlayer(member.entityId))
					{
						__instance.SharedKillClient(killed.entityClass, experienceValue, null, killed.entityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
							NetPackageManager.GetPackage<NetPackageSharedPartyKill>()
								.Setup(killed.entityClass, experienceValue, _killerID, killed.entityId),
							_onlyClientsAttachedToAnEntity: false,
							member.entityId);
					}
				}

				return false;
			}
			catch
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_PartyEntryList), nameof(XUiC_PartyEntryList.RefreshPartyList))]
	public static class Patch_PartyEntryList_Refresh
	{
		public static bool Prefix(XUiC_PartyEntryList __instance)
		{
			PartyGroupPlusHud.ApplySortedList(__instance);
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_PartyWindow), nameof(XUiC_PartyWindow.Update))]
	public static class Patch_PartyWindow_SlowResort
	{
		public static void Postfix(XUiC_PartyWindow __instance)
		{
			PartyGroupPlusHud.TrySlowRefresh(__instance);
		}
	}

	[HarmonyPatch(typeof(XUiC_PartyWindow), "GetBindingValueInternal")]
	public static class Patch_PartyWindow_PartyCount
	{
		public static void Postfix(XUiC_PartyWindow __instance, ref string _value, string _bindingName, ref bool __result)
		{
			if (_bindingName != "partycount" && _bindingName != "partycountvisible")
			{
				return;
			}

			try
			{
				EntityPlayer localPlayer = XUiC_PartyGroupPlusColorChoice.SafeLocal(__instance.xui);
				bool show = PartyGroupPlusHud.TryGetPartyCount(localPlayer, out int count);
				if (_bindingName == "partycountvisible")
				{
					_value = show ? "true" : "false";
					__result = true;
					return;
				}

				_value = show ? count.ToString() : string.Empty;
				__result = true;
			}
			catch
			{
				_value = _bindingName == "partycountvisible" ? "false" : string.Empty;
				__result = true;
			}
		}
	}
}
