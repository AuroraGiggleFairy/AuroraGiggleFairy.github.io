using System;
using System.Collections.Generic;
using System.Reflection;

public class ConsoleCmdScreamerAlert : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return new[] { "sa", "screameralert" };
    }

    public override string getDescription()
    {
        return "Manage Screamer Alert mode (off/on/numbers).";
    }

    public override string getHelp()
    {
        return "Usage:\n"
            + "  sa\n"
            + "  sa status\n"
            + "  sa <off|on|numbers|cycle>\n"
            + "  sa admin <entityId> <off|on|numbers|cycle>\n"
            + "Notes:\n"
            + "  - off: hide all Screamer Alert UI text\n"
            + "  - on: show text only (no counts)\n"
            + "  - numbers: show text and counts\n"
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
                SdtdConsole.Instance.Output("[ScreamerAlert] Console has no per-player mode. Use: sa admin <entityId> <mode>");
                return;
            }

            ScreamerAlertMode current = ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, ScreamerAlertMode.OnWithNumbers);
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
            SdtdConsole.Instance.Output("[ScreamerAlert] Console must use admin mode: sa admin <entityId> <mode>");
            return;
        }

        ScreamerAlertMode selfCurrent = ScreamerAlertModeSettings.GetModeForEntityId(senderEntityId, ScreamerAlertMode.OnWithNumbers);
        if (!TryParseMode(_params[0], selfCurrent, out ScreamerAlertMode selfNext))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Invalid mode. Use: off | on | numbers | cycle");
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
            SdtdConsole.Instance.Output("[ScreamerAlert] Usage: sa admin <entityId> <off|on|numbers|cycle>");
            return;
        }

        if (!int.TryParse(args[1], out int targetEntityId) || targetEntityId < 0)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Invalid entityId: " + args[1]);
            return;
        }

        ScreamerAlertMode current = ScreamerAlertModeSettings.GetModeForEntityId(targetEntityId, ScreamerAlertMode.OnWithNumbers);
        if (!TryParseMode(args[2], current, out ScreamerAlertMode next))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] Invalid mode. Use: off | on | numbers | cycle");
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

    private static bool TryParseMode(string text, ScreamerAlertMode current, out ScreamerAlertMode mode)
    {
        mode = current;
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
            case "numbers":
            case "onwithnumbers":
            case "on+#":
                mode = ScreamerAlertMode.OnWithNumbers;
                return true;
            case "cycle":
                mode = ScreamerAlertModeSettings.Cycle(current);
                return true;
            default:
                return false;
        }
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
