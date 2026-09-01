using System;
using System.Collections.Generic;

namespace VisualEntityTrackerAddon
{
    public static class ChatCmdVisualEntityTrackerAddon
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
            if (cmd != "agf-vet" && cmd != "agfvet")
            {
                return ModEvents.EModEventResult.Continue;
            }

            int senderEntityId = data.SenderEntityId;
            if (senderEntityId < 0)
            {
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (IsDuplicateCommand(senderEntityId, text))
            {
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (parts.Length == 1
                || string.Equals(parts[1], "status", StringComparison.OrdinalIgnoreCase)
                || string.Equals(parts[1], "help", StringComparison.OrdinalIgnoreCase)
                || parts[1] == "?")
            {
                SendStatusHelpLines(senderEntityId);
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (!VisualEntityTrackerModeSettings.TryParseCommandMode(parts[1], out VisualEntityTrackerMode nextMode))
            {
                WhisperToSender(senderEntityId, Localize("VisualEntityTracker_Chat_Invalid", "[ERROR][Use /agfvet]"));
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (!VisualEntityTrackerModeSettings.SetModeForEntityId(senderEntityId, nextMode))
            {
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            VisualEntityTrackerAddonService.ApplyServerSideMode(senderEntityId, nextMode == VisualEntityTrackerMode.On);
            WhisperToSender(senderEntityId, nextMode == VisualEntityTrackerMode.Off
                ? Localize("VisualEntityTracker_Chat_SetOff", "[Visual Entity Tracker = OFF]")
                : Localize("VisualEntityTracker_Chat_SetOn", "[Visual Entity Tracker = ON]"));
            return ModEvents.EModEventResult.StopHandlersAndVanilla;
        }

        private static int lastHandledEntityId = int.MinValue;
        private static string lastHandledText = string.Empty;
        private static int lastHandledTick;

        private static bool IsDuplicateCommand(int senderEntityId, string text)
        {
            int now = Environment.TickCount;
            if (senderEntityId == lastHandledEntityId
                && string.Equals(text, lastHandledText, StringComparison.Ordinal)
                && unchecked((uint)(now - lastHandledTick)) < 250)
            {
                return true;
            }

            lastHandledEntityId = senderEntityId;
            lastHandledText = text;
            lastHandledTick = now;
            return false;
        }

        private static void SendStatusHelpLines(int senderEntityId)
        {
            VisualEntityTrackerMode current = VisualEntityTrackerModeSettings.GetModeForEntityId(
                senderEntityId,
                VisualEntityTrackerModeSettings.GetServerDefaultMode());

            WhisperToSender(senderEntityId, Localize(
                "VisualEntityTracker_Chat_Status",
                "[Visual Entity Tracker = {0}]",
                VisualEntityTrackerModeSettings.GetModeToken(current)));
            WhisperToSender(senderEntityId, Localize(
                "VisualEntityTracker_Chat_Options",
                "[Options: /agfvet off, on]"));
        }

        private static void WhisperToSender(int senderEntityId, string message)
        {
            if (senderEntityId < 0 || string.IsNullOrEmpty(message))
            {
                return;
            }

            GameManager.Instance?.ChatMessageServer(null, EChatType.Whisper, -1, message, new List<int> { senderEntityId }, EMessageSender.Server);
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
    }
}
