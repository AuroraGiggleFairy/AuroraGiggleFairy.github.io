using System.Collections.Generic;
using HarmonyLib;

namespace SortingNearbyPlus
{
	[HarmonyPatch(typeof(GameManager), "ChatMessageServer")]
	internal static class ChatInterceptPatch
	{
		public static bool Prefix(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender)
		{
			_ = _cInfo;
			_ = _chatType;
			_ = _recipientEntityIds;
			_ = _msgSender;

			if (!StorageUtil.IsServer() || !ChatCmdSortingNearbyPlus.IsSortCommand(_msg))
			{
				return true;
			}

			ModEvents.SChatMessageData data = new ModEvents.SChatMessageData(_cInfo, _chatType, _senderEntityId, _msg, null, _recipientEntityIds);
			return ChatCmdSortingNearbyPlus.OnChatMessage(ref data) != ModEvents.EModEventResult.StopHandlersAndVanilla;
		}
	}
}
