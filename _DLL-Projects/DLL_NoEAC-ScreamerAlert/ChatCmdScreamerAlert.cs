using System;
using System.Collections.Generic;

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
            if (cmd != "agf-sa" && cmd != "agfsa")
            {
                return ModEvents.EModEventResult.Continue;
            }

            int senderEntityId = data.SenderEntityId;
            if (senderEntityId < 0)
            {
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            bool enhanced = IsEnhancedAvailableForEntity(senderEntityId);

            if (parts.Length == 1 || string.Equals(parts[1], "status", StringComparison.OrdinalIgnoreCase))
            {
                ScreamerAlertMode playerDefault = NormalizeForOutput(ScreamerAlertModeSettings.GetServerDefaultMode(), enhanced);
                ScreamerAlertMode current = NormalizeForOutput(ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, playerDefault), enhanced);
                WhisperToSender(senderEntityId, enhanced
                    ? Localize("ScreamerAlert_Chat_Status_Enhanced", "[Screamer Alert = {0}] [Change with /agfsa <off|on|count>.]", ModeToken(current))
                    : Localize("ScreamerAlert_Chat_Status_Baseline", "[Screamer Alert = {0}] [Change with /agfsa <off|on>.]", ModeToken(current)));
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            string arg = parts[1].ToLowerInvariant();
            if (arg == "help")
            {
                ScreamerAlertMode playerDefault = NormalizeForOutput(ScreamerAlertModeSettings.GetServerDefaultMode(), enhanced);
                ScreamerAlertMode current = NormalizeForOutput(ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, playerDefault), enhanced);
                WhisperToSender(senderEntityId, enhanced
                    ? Localize("ScreamerAlert_Chat_Status_Enhanced", "[Screamer Alert = {0}] [Change with /agfsa <off|on|count>.]", ModeToken(current))
                    : Localize("ScreamerAlert_Chat_Status_Baseline", "[Screamer Alert = {0}] [Change with /agfsa <off|on>.]", ModeToken(current)));
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (IsCountAlias(arg))
            {
                ScreamerAlertMode targetMode = enhanced ? ScreamerAlertMode.OnWithNumbers : ScreamerAlertMode.On;
                if (!ScreamerAlertModeSettings.SetModeForEntityId(senderEntityId, targetMode))
                {
                    return ModEvents.EModEventResult.StopHandlersAndVanilla;
                }

                if (enhanced)
                {
                    WhisperToSender(senderEntityId, Localize("ScreamerAlert_Chat_SetCount_Enhanced", "[Screamer Alert = COUNT] [Options: off|on|count.]"));
                }
                else
                {
                    WhisperToSender(senderEntityId, Localize("ScreamerAlert_Chat_SetCount_Baseline", "[Screamer Alert = ON] [COUNT requires EnhancedAGF.]"));
                }
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (!TryParseMode(arg, out ScreamerAlertMode requestedMode))
            {
                WhisperToSender(senderEntityId, enhanced
                    ? Localize("ScreamerAlert_Chat_Invalid_Enhanced", "[Screamer Alert] [Invalid option. Try /agfsa <off|on|count>.]")
                    : Localize("ScreamerAlert_Chat_Invalid_Baseline", "[Screamer Alert] [Invalid option. Try /agfsa <off|on>.]"));
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            ScreamerAlertMode nextMode = NormalizeForOutput(requestedMode, enhanced);

            if (!ScreamerAlertModeSettings.SetModeForEntityId(senderEntityId, nextMode))
            {
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (nextMode == ScreamerAlertMode.Off)
            {
                WhisperToSender(senderEntityId, enhanced
                    ? Localize("ScreamerAlert_Chat_SetOff_Enhanced", "[Screamer Alert = OFF] [Options: off|on|count.]")
                    : Localize("ScreamerAlert_Chat_SetOff_Baseline", "[Screamer Alert = OFF] [Options: off|on.]"));
            }
            else
            {
                WhisperToSender(senderEntityId, enhanced
                    ? Localize("ScreamerAlert_Chat_SetOn_Enhanced", "[Screamer Alert = ON] [Options: off|on|count.]")
                    : Localize("ScreamerAlert_Chat_SetOn_Baseline", "[Screamer Alert = ON] [Options: off|on.]"));
            }

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

        private static ScreamerAlertMode NormalizeForOutput(ScreamerAlertMode mode, bool enhancedAvailable)
        {
            if (!enhancedAvailable && mode == ScreamerAlertMode.OnWithNumbers)
            {
                return ScreamerAlertMode.On;
            }

            return mode;
        }

        private static bool IsEnhancedAvailableForEntity(int entityId)
        {
            return ScreamerAlertHybridRouting.HasClientCapabilityByEntityId(entityId);
        }

        private static bool TryParseMode(string text, out ScreamerAlertMode mode)
        {
            mode = ScreamerAlertMode.Off;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            switch (text.Trim().ToLowerInvariant())
            {
                case "off":
                    mode = ScreamerAlertMode.Off;
                    return true;
                case "on":
                    mode = ScreamerAlertMode.On;
                    return true;
                default:
                    return false;
            }
        }

        private static string ModeToken(ScreamerAlertMode mode)
        {
            switch (mode)
            {
                case ScreamerAlertMode.Off:
                    return "OFF";
                case ScreamerAlertMode.On:
                    return "ON";
                case ScreamerAlertMode.OnWithNumbers:
                    return "COUNT";
                default:
                    return mode.ToString().ToUpperInvariant();
            }
        }

        private static string Localize(string key, string fallback, params object[] args)
        {
            string template = Localization.Get(key);
            if (string.IsNullOrEmpty(template) || string.Equals(template, key, StringComparison.Ordinal))
            {
                template = fallback;
            }

            if (args == null || args.Length == 0)
            {
                return template;
            }

            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return string.Format(fallback, args);
            }
        }

        private static bool IsCountAlias(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            switch (text.Trim().ToLowerInvariant())
            {
                case "count":
                case "counts":
                case "numbers":
                    return true;
                default:
                    return false;
            }
        }
    }
}
