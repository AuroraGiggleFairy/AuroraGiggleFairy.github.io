using System;
using System.Collections.Generic;
using System.Reflection;

namespace NoEACVisualEntityTracker
{
    public class ConsoleCmdVisualEntityTracker : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "agf-visualentitytracker", "agf-vet" };
        }

        public override string getDescription()
        {
            return "Allows toggling Visual Entity Tracker mode.";
        }

        public override string getHelp()
        {
            return "Usage:\n"
                + "  agf-visualentitytracker\n"
                + "  agf-vet\n"
                + "  agf-visualentitytracker status\n"
                + "  agf-visualentitytracker <off|on>\n"
                + "  agf-visualentitytracker admin <entityId> <off|on>\n"
                + "Notes:\n"
                + "  - admin subcommand requires admin privileges.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (!TryGetSenderEntityId(_senderInfo, out int senderEntityId))
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Unable to resolve sender context.");
                return;
            }

            if (_params == null || _params.Count == 0 || string.Equals(_params[0], "status", StringComparison.OrdinalIgnoreCase))
            {
                if (senderEntityId < 0)
                {
                    SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Console has no per-player mode. Use: agf-vet admin <entityId> <off|on>");
                    return;
                }

                VisualEntityTrackerMode current = VisualEntityTrackerModeSettings.GetModeForEntityId(senderEntityId, VisualEntityTrackerMode.On);
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] mode=" + VisualEntityTrackerModeSettings.GetModeLabel(current));
                return;
            }

            if (string.Equals(_params[0], "admin", StringComparison.OrdinalIgnoreCase))
            {
                HandleAdmin(_params, senderEntityId);
                return;
            }

            if (senderEntityId < 0)
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Console must use admin mode: agf-vet admin <entityId> <off|on>");
                return;
            }

            if (!VisualEntityTrackerModeSettings.TryParseMode(_params[0], out VisualEntityTrackerMode nextMode))
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Invalid mode. Use: off | on");
                return;
            }

            if (!VisualEntityTrackerModeSettings.SetModeForEntityId(senderEntityId, nextMode))
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Could not save mode for sender.");
                return;
            }

            VisualEntityTrackerService.ApplyServerSideMode(senderEntityId, nextMode == VisualEntityTrackerMode.On);
            VisualEntityTrackerService.RefreshTrackerMode();
            XUiC_VisualEntityTrackerOptions.MarkAllDirty();
            SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] mode set to " + VisualEntityTrackerModeSettings.GetModeLabel(nextMode));
        }

        private static void HandleAdmin(List<string> args, int senderEntityId)
        {
            if (!IsSenderAdmin(senderEntityId))
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Admin permission required.");
                return;
            }

            if (args.Count < 3)
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Usage: agf-vet admin <entityId> <off|on>");
                return;
            }

            if (!int.TryParse(args[1], out int targetEntityId) || targetEntityId < 0)
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Invalid entityId: " + args[1]);
                return;
            }

            if (!VisualEntityTrackerModeSettings.TryParseMode(args[2], out VisualEntityTrackerMode nextMode))
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Invalid mode. Use: off | on");
                return;
            }

            if (!VisualEntityTrackerModeSettings.SetModeForEntityId(targetEntityId, nextMode))
            {
                SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] Could not save mode for target entityId " + targetEntityId + ".");
                return;
            }

            VisualEntityTrackerService.ApplyServerSideMode(targetEntityId, nextMode == VisualEntityTrackerMode.On);
            SdtdConsole.Instance.Output("[NoEACVisualEntityTracker] entity " + targetEntityId + " mode set to " + VisualEntityTrackerModeSettings.GetModeLabel(nextMode));
        }

        private static bool IsSenderAdmin(int senderEntityId)
        {
            if (senderEntityId < 0)
            {
                return true;
            }

            EntityPlayer senderPlayer = GameManager.Instance?.World?.GetEntity(senderEntityId) as EntityPlayer;
            return senderPlayer != null && senderPlayer.IsAdmin;
        }

        private static bool TryGetSenderEntityId(CommandSenderInfo senderInfo, out int entityId)
        {
            entityId = -1;
            Type senderType = senderInfo.GetType();

            if (TryReadInt(senderType, senderInfo, "entityId", out int senderEntityId)
                || TryReadInt(senderType, senderInfo, "EntityId", out senderEntityId))
            {
                entityId = senderEntityId;
                return true;
            }

            object remoteClientInfo = null;
            if (TryReadObject(senderType, senderInfo, "RemoteClientInfo", out remoteClientInfo)
                || TryReadObject(senderType, senderInfo, "remoteClientInfo", out remoteClientInfo)
                || TryReadObject(senderType, senderInfo, "ClientInfo", out remoteClientInfo)
                || TryReadObject(senderType, senderInfo, "clientInfo", out remoteClientInfo))
            {
                if (remoteClientInfo is ClientInfo clientInfo)
                {
                    entityId = clientInfo.entityId;
                    return true;
                }

                if (remoteClientInfo != null)
                {
                    Type remoteType = remoteClientInfo.GetType();
                    if (TryReadInt(remoteType, remoteClientInfo, "entityId", out int remoteEntityId)
                        || TryReadInt(remoteType, remoteClientInfo, "EntityId", out remoteEntityId))
                    {
                        entityId = remoteEntityId;
                        return true;
                    }
                }
            }

            EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (localPlayer != null)
            {
                entityId = localPlayer.entityId;
                return true;
            }

            return true;
        }

        private static bool TryReadInt(Type type, object instance, string memberName, out int value)
        {
            value = 0;
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(int))
            {
                value = (int)property.GetValue(instance, null);
                return true;
            }

            FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(int))
            {
                value = (int)field.GetValue(instance);
                return true;
            }

            return false;
        }

        private static bool TryReadObject(Type type, object instance, string memberName, out object value)
        {
            value = null;
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                value = property.GetValue(instance, null);
                return true;
            }

            FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                value = field.GetValue(instance);
                return true;
            }

            return false;
        }
    }
}