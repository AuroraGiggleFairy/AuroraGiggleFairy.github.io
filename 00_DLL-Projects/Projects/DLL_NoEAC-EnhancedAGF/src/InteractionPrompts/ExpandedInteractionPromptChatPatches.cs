using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ExpandedInteractionPrompts
{
    [HarmonyPatch(typeof(GameManager), "ChatMessageServer")]
    public static class ExpandedInteractionPromptChatPatches
    {
        private const string ColorOk = "B8A3D8";
        private const string ColorOpt = "86A67F";
        private const string ColorErr = "C38FAE";

        public static bool Prefix(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode)
        {
            _ = _cInfo;
            _ = _chatType;
            _ = _recipientEntityIds;
            _ = _bbMode;

            if (_msgSender == EMessageSender.Server)
                return true;

            if (!TryHandle(_msg, _senderEntityId))
                return true;

            return false;
        }

        private static bool TryHandle(string message, int senderEntityId)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            string trimmed = message.Trim();
            if (!trimmed.StartsWith("/", StringComparison.Ordinal))
                return false;

            string[] parts = trimmed.Substring(1).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return false;

            string root = parts[0].Trim().ToLowerInvariant();
            if (!IsEpRoot(root))
                return false;

            string mode = parts.Length > 1 ? parts[1].Trim().ToLowerInvariant() : "status";
            if (mode == "grow")
                return GrowthStagePrompt.HandleServerQuery(senderEntityId, parts);

            switch (mode)
            {
                case "full":
                    PromptHudMode.Set(PromptHudMode.Full);
                    Whisper(senderEntityId, Ok("Interaction Prompts = FULL"));
                    break;
                case "partial":
                    PromptHudMode.Set(PromptHudMode.Partial);
                    Whisper(senderEntityId, Ok("Interaction Prompts = PARTIAL"));
                    break;
                case "off":
                    PromptHudMode.Set(PromptHudMode.Off);
                    Whisper(senderEntityId, Ok("Interaction Prompts = OFF"));
                    break;
                default:
                    Whisper(senderEntityId, Ok("Interaction Prompts = " + PromptHudMode.Name(PromptHudMode.Current).ToUpperInvariant()));
                    Whisper(senderEntityId, Opt("Options: /agfep full, partial, off"));
                    break;
            }

            return true;
        }

        private static bool IsEpRoot(string root)
        {
            return root == "agfep"
                || root == "agf-ep"
                || root == "agfeip"
                || root == "agf-eip";
        }

        private static string Ok(string text)
        {
            return "[" + ColorOk + "][" + text + "][-]";
        }

        private static string Opt(string text)
        {
            return "[" + ColorOpt + "][" + text + "][-]";
        }

        private static void Whisper(int senderEntityId, string message)
        {
            if (senderEntityId < 0)
            {
                EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
                senderEntityId = local != null ? local.entityId : -1;
            }

            if (senderEntityId < 0 || string.IsNullOrEmpty(message))
                return;

            GameManager.Instance?.ChatMessageServer(
                null,
                EChatType.Whisper,
                -1,
                message,
                new List<int> { senderEntityId },
                EMessageSender.Server);
        }
    }
}
