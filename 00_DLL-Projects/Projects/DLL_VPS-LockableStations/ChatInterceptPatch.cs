using System.Collections.Generic;
using HarmonyLib;

namespace LockableWorkstations
{
	[HarmonyPatch(typeof(GameManager), "ChatMessageServer")]
	public static class ChatInterceptPatch
	{
		public static bool Prefix(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode)
		{
			_ = _chatType;
			_ = _recipientEntityIds;
			_ = _bbMode;

			if (_msgSender == EMessageSender.Server)
				return true;

			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null || !manager.IsServer)
				return true;

			if (!ConsoleCmdLockableWorkstations.TryHandleChatMessage(_msg, _senderEntityId, _cInfo))
				return true;

			return false;
		}
	}
}
