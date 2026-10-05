using System.Reflection;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// When a party member logs out from inside a level, the remaining client often no longer
	/// has that player's entity. Vanilla party data then reads PlayerDisplayName on null.
	/// Show the leave line from the saved name, and skip that null read.
	/// </summary>
	[HarmonyPatch(typeof(NetPackagePartyData), nameof(NetPackagePartyData.ProcessPackage))]
	internal static class Patch_PartyDisconnect
	{
		private static readonly FieldInfo ActionField = AccessTools.Field(typeof(NetPackagePartyData), "partyAction");
		private static readonly FieldInfo ChangedField = AccessTools.Field(typeof(NetPackagePartyData), "changedEntityID");
		private static readonly FieldInfo PartyField = AccessTools.Field(typeof(NetPackagePartyData), "PartyID");

		private static void Prefix(NetPackagePartyData __instance, World _world)
		{
			if (__instance == null || _world == null || ActionField == null || ChangedField == null)
			{
				return;
			}

			ConnectionManager connection = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (connection == null || connection.IsServer)
			{
				return;
			}

			int changedId = (int)ChangedField.GetValue(__instance);
			Party party = PartyManager.Current != null && PartyField != null
				? PartyManager.Current.GetParty((int)PartyField.GetValue(__instance))
				: null;
			if (party?.MemberList != null)
			{
				party.MemberList.RemoveAll(member => member == null);
			}

			if (changedId == -1)
			{
				return;
			}

			EntityPlayer entity = _world.GetEntity(changedId) as EntityPlayer;
			if (entity != null)
			{
				return;
			}

			var action = (NetPackagePartyData.PartyActions)ActionField.GetValue(__instance);
			EntityPlayerLocal primary = GameManager.Instance?.World?.GetPrimaryPlayer();
			bool ours = primary?.Party != null && party != null && primary.Party == party;
			if (ours && primary.entityId != changedId &&
				(action == NetPackagePartyData.PartyActions.Disconnected ||
				 action == NetPackagePartyData.PartyActions.LeaveParty ||
				 action == NetPackagePartyData.PartyActions.KickFromParty))
			{
				string name = PartyIds.NameOf(changedId);
				if (string.IsNullOrEmpty(name))
				{
					name = "Player";
				}

				string key = action == NetPackagePartyData.PartyActions.KickFromParty
					? "ttPartyOtherKickedFromParty"
					: action == NetPackagePartyData.PartyActions.LeaveParty
						? "ttPartyOtherLeftParty"
						: "ttPartyDisconnectedFromParty";
				Audio.Manager.PlayInsidePlayerHead("party_member_leave");
				GameManager.ShowTooltip(primary, string.Format(Localization.Get(key), name));
			}

			ChangedField.SetValue(__instance, -1);
		}
	}
}
