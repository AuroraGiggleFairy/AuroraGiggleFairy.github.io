using System;
using System.Collections.Generic;
using System.Reflection;

public class ConsoleCmdScreamerAlert : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return new[] { "agf-screameralert", "agf-sa" };
    }

    public override string getDescription()
    {
        return "Allows toggling screamer alert function.";
    }

    public override string getHelp()
    {
        return "Usage:\n"
            + "  agf-screameralert\n"
            + "  agf-sa\n"
            + "  agf-screameralert status\n"
            + "  agf-screameralert <off|on|counts|cycle>\n"
            + "  agf-screameralert admin <entityId> <off|on|counts|cycle>\n"
            + "Modes:\n"
            + "  1) off: hide all Screamer Alert UI text\n"
            + "  2) on: show text only (no counts)\n"
            + "  3) counts: show text and counts\n"
            + "  4) cycle: rotate mode in order Off -> On -> On + # -> Off\n"
            + "Notes:\n"
            + "  - admin subcommand requires admin privileges.";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (!TryGetSenderEntityId(_senderInfo, out int senderEntityId))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Unable to resolve sender context.");
            return;
        }

        if (_params == null || _params.Count == 0 || string.Equals(_params[0], "status", StringComparison.OrdinalIgnoreCase))
        {
            if (senderEntityId < 0)
            {
                SdtdConsole.Instance.Output("[ScreamerAlert] Console has no per-player mode. Use: agf-screameralert admin <entityId> <mode>");
                return;
            }

            ScreamerAlertMode current = ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, ScreamerAlertMode.OnWithNumbers);
            bool senderHasClientModeSupport = SupportsExtendedModes(senderEntityId);
            if (!senderHasClientModeSupport)
            {
                current = NormalizeServerSideOnlyMode(current);
            }

            SdtdConsole.Instance.Output("[ScreamerAlert] mode=" + ScreamerAlertModeSettings.GetModeLabel(current));
            return;
        }

        if (string.Equals(_params[0], "admin", StringComparison.OrdinalIgnoreCase))
        {
            HandleAdmin(_params, senderEntityId);
            return;
        }

        if (senderEntityId < 0)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Console must use admin mode: agf-screameralert admin <entityId> <mode>");
            return;
        }

        ScreamerAlertMode selfCurrent = ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, ScreamerAlertMode.OnWithNumbers);
        bool selfHasClientModeSupport = SupportsExtendedModes(senderEntityId);
        if (!TryParseMode(_params[0], selfCurrent, selfHasClientModeSupport, out ScreamerAlertMode selfNext))
        {
            SdtdConsole.Instance.Output(selfHasClientModeSupport
                ? "[ScreamerAlert] Invalid mode. Use: off | on | counts | cycle"
                : "[ScreamerAlert] Invalid mode for server-side-only users. Use: off | on");
            return;
        }

        if (!ScreamerAlertModeSettings.SetModeForEntityId(senderEntityId, selfNext))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Could not save mode for sender.");
            return;
        }

        SdtdConsole.Instance.Output("[ScreamerAlert] mode set to " + ScreamerAlertModeSettings.GetModeLabel(selfNext));
        if (XUiC_ScreamerAlerts.Instance != null)
        {
            XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
        }
    }

    private static void HandleAdmin(List<string> args, int senderEntityId)
    {
        if (!IsSenderAdmin(senderEntityId))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Admin permission required.");
            return;
        }

        if (args.Count < 3)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Usage: agf-screameralert admin <entityId> <off|on|counts|cycle>");
            return;
        }

        if (!int.TryParse(args[1], out int targetEntityId) || targetEntityId < 0)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Invalid entityId: " + args[1]);
            return;
        }

        ScreamerAlertMode current = ScreamerAlertModeSettings.GetModeForEntityId(targetEntityId, ScreamerAlertMode.OnWithNumbers);
        bool targetHasClientModeSupport = SupportsExtendedModes(targetEntityId);
        if (!TryParseMode(args[2], current, targetHasClientModeSupport, out ScreamerAlertMode next))
        {
            SdtdConsole.Instance.Output(targetHasClientModeSupport
                ? "[ScreamerAlert] Invalid mode. Use: off | on | counts | cycle"
                : "[ScreamerAlert] Invalid mode for server-side-only target. Use: off | on");
            return;
        }

        if (!ScreamerAlertModeSettings.SetModeForEntityId(targetEntityId, next))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Could not save mode for target entityId " + targetEntityId + ".");
            return;
        }

        SdtdConsole.Instance.Output("[ScreamerAlert] entity " + targetEntityId + " mode set to " + ScreamerAlertModeSettings.GetModeLabel(next));
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

    private static ScreamerAlertMode NormalizeServerSideOnlyMode(ScreamerAlertMode mode)
    {
        return mode == ScreamerAlertMode.Off ? ScreamerAlertMode.Off : ScreamerAlertMode.On;
    }

    private static bool SupportsExtendedModes(int entityId)
    {
        if (entityId < 0)
        {
            return false;
        }

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        ClientInfo clientInfo = manager?.Clients?.ForEntityId(entityId);
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
