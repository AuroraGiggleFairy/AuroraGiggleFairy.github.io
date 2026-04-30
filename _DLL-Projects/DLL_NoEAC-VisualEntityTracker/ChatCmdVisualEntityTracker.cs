using System;
using System.Collections.Generic;

namespace NoEACVisualEntityTracker
{
    public static class ChatCmdVisualEntityTracker
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
            if (cmd != "vet" && cmd != "visualentitytracker" && cmd != "agf-vet" && cmd != "agf-visualentitytracker")
            {
                return ModEvents.EModEventResult.Continue;
            }

            int senderEntityId = data.SenderEntityId;
            if (senderEntityId < 0)
            {
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (parts.Length == 1 || string.Equals(parts[1], "status", StringComparison.OrdinalIgnoreCase))
            {
                VisualEntityTrackerMode current = VisualEntityTrackerModeSettings.GetModeForEntityId(senderEntityId, VisualEntityTrackerMode.On);
                WhisperToSender(senderEntityId, "[NoEACVisualEntityTracker] mode=" + VisualEntityTrackerModeSettings.GetModeLabel(current));
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            string arg = parts[1].ToLowerInvariant();
            if (arg == "help" || arg == "?")
            {
                WhisperToSender(senderEntityId, "[NoEACVisualEntityTracker] Usage: /vet <off|on|status>");
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (!VisualEntityTrackerModeSettings.TryParseMode(arg, out VisualEntityTrackerMode nextMode))
            {
                WhisperToSender(senderEntityId, "[NoEACVisualEntityTracker] Invalid mode. Use: off | on");
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            if (!VisualEntityTrackerModeSettings.SetModeForEntityId(senderEntityId, nextMode))
            {
                WhisperToSender(senderEntityId, "[NoEACVisualEntityTracker] Could not save mode.");
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            VisualEntityTrackerService.ApplyServerSideMode(senderEntityId, nextMode == VisualEntityTrackerMode.On);
            VisualEntityTrackerService.RefreshTrackerMode();
            XUiC_VisualEntityTrackerOptions.MarkAllDirty();
            WhisperToSender(senderEntityId, "[NoEACVisualEntityTracker] mode set to " + VisualEntityTrackerModeSettings.GetModeLabel(nextMode));
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
    }
}