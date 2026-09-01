using System;
using System.Collections.Generic;
using HarmonyLib;

[HarmonyPatch(typeof(GameManager), "ChatMessageServer")]
public static class VisualEntityTrackerAddonChatInterceptPatch
{
    static bool Prefix(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode)
    {
        _ = _chatType;
        _ = _msgSender;
        _ = _bbMode;

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (manager == null || !manager.IsServer)
        {
            return true;
        }

        if (!IsVisualEntityTrackerCommand(_msg))
        {
            return true;
        }

        ModEvents.SChatMessageData data = new ModEvents.SChatMessageData(_cInfo, _chatType, _senderEntityId, _msg, null, _recipientEntityIds);
        ModEvents.EModEventResult handled = VisualEntityTrackerAddon.ChatCmdVisualEntityTrackerAddon.OnChatMessage(ref data);
        if (handled == ModEvents.EModEventResult.StopHandlersAndVanilla)
        {
            return false;
        }

        return true;
    }

    private static bool IsVisualEntityTrackerCommand(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        string trimmed = message.Trim();
        if (!trimmed.StartsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        string withoutSlash = trimmed.Substring(1).Trim();
        if (string.IsNullOrEmpty(withoutSlash))
        {
            return false;
        }

        int separatorIndex = withoutSlash.IndexOf(' ');
        string root = separatorIndex >= 0 ? withoutSlash.Substring(0, separatorIndex) : withoutSlash;
        root = root.Trim().ToLowerInvariant();
        return root == "agf-vet" || root == "agfvet";
    }
}
