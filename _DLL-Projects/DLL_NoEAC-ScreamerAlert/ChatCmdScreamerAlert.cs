using System;
using System.Collections.Generic;
using Platform;

namespace ScreamerAlert
{
    public static class ChatCmdScreamerAlert
    {
        public static ModEvents.EModEventResult OnChatMessage(ref ModEvents.SChatMessageData data)
        {
            string raw = data.Message;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return ModEvents.EModEventResult.Continue;
            }

            string text = raw.Trim();
            if (!text.StartsWith("/", StringComparison.Ordinal))
            {
                return ModEvents.EModEventResult.Continue;
            }

            string withoutSlash = text.Substring(1).Trim();
            if (string.IsNullOrEmpty(withoutSlash))
            {
                return ModEvents.EModEventResult.Continue;
            }

            string[] parts = withoutSlash.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return ModEvents.EModEventResult.Continue;
            }

            string cmd = parts[0].ToLowerInvariant();
            if (cmd != "agf-sa" && cmd != "agf-screameralert")
            {
                return ModEvents.EModEventResult.Continue;
            }

            int senderEntityId = data.SenderEntityId;
            if (senderEntityId < 0)
            {
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            ClientInfo senderClientInfo = data.ClientInfo;
            bool allowExtendedModes = SupportsExtendedModes(senderEntityId, senderClientInfo);

            if (parts.Length == 1 || string.Equals(parts[1], "status", StringComparison.OrdinalIgnoreCase))
            {
                ScreamerAlertMode current = ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, ScreamerAlertMode.OnWithNumbers);
                if (!allowExtendedModes)
                {
                    current = NormalizeServerSideOnlyMode(current);
                }

                WhisperToSender(senderEntityId, "[ScreamerAlert] mode=" + ScreamerAlertModeSettings.GetModeLabel(current));
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            string arg = parts[1].ToLowerInvariant();
            if (arg == "help" || arg == "?")
            {
                if (allowExtendedModes)
                {
                    WhisperToSender(senderEntityId, "[ScreamerAlert] Usage: /agf-sa <off|on|counts|cycle|status>");
                }
                else
                {
                    WhisperToSender(senderEntityId, "[ScreamerAlert] Usage: /agf-sa <off|on|status>");
                }

                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            ScreamerAlertMode currentMode = ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, ScreamerAlertMode.OnWithNumbers);
            if (!TryParseMode(arg, currentMode, allowExtendedModes, out ScreamerAlertMode nextMode))
            {
                if (allowExtendedModes)
                {
                    WhisperToSender(senderEntityId, "[ScreamerAlert] Invalid mode. Use: off | on | counts | cycle");
                }
                else
                {
                    WhisperToSender(senderEntityId, "[ScreamerAlert] Invalid mode for server-side-only users. Use: off | on");
                }

                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (!ScreamerAlertModeSettings.SetModeForEntityId(senderEntityId, nextMode))
            {
                WhisperToSender(senderEntityId, "[ScreamerAlert] Could not save mode.");
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            WhisperToSender(senderEntityId, "[ScreamerAlert] mode set to " + ScreamerAlertModeSettings.GetModeLabel(nextMode));
            return ModEvents.EModEventResult.StopHandlersAndVanilla;
        }

        private static void WhisperToSender(int senderEntityId, string message)
        {
            if (senderEntityId < 0 || string.IsNullOrEmpty(message))
            {
                return;
            }

            GameManager.Instance?.ChatMessageServer(null, EChatType.Whisper, -1, message, new List<int> { senderEntityId }, EMessageSender.Server);
        }

        private static bool SupportsExtendedModes(int entityId, ClientInfo clientInfo)
        {
            if (entityId < 0)
            {
                return false;
            }

            if (ScreamerAlertHybridRouting.HasClientCapability(clientInfo))
            {
                return true;
            }

            EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (localPlayer != null && localPlayer.entityId == entityId && XUiC_ScreamerAlerts.Instance != null)
            {
                return true;
            }

            return false;
        }

        private static ScreamerAlertMode NormalizeServerSideOnlyMode(ScreamerAlertMode mode)
        {
            return mode == ScreamerAlertMode.Off ? ScreamerAlertMode.Off : ScreamerAlertMode.On;
        }

        private static bool TryParseMode(string text, ScreamerAlertMode current, bool allowExtendedModes, out ScreamerAlertMode mode)
        {
            mode = allowExtendedModes ? current : NormalizeServerSideOnlyMode(current);
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            switch (text.Trim().ToLowerInvariant())
            {
                case "0":
                case "off":
                    mode = ScreamerAlertMode.Off;
                    return true;
                case "1":
                case "on":
                    mode = ScreamerAlertMode.On;
                    return true;
                case "2":
                case "counts":
                case "numbers":
                case "onwithnumbers":
                case "on+#":
                    if (!allowExtendedModes)
                    {
                        return false;
                    }

                    mode = ScreamerAlertMode.OnWithNumbers;
                    return true;
                case "cycle":
                    if (!allowExtendedModes)
                    {
                        return false;
                    }

                    mode = ScreamerAlertModeSettings.Cycle(current);
                    return true;
                default:
                    return false;
            }
        }
    }
}
