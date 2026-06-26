using System;
using System.Collections.Generic;
using System.Reflection;

public class ConsoleCmdScreamerAlert : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return new[] { "agf-sa" };
    }

    public override string getDescription()
    {
        return "Admin controls for Screamer Alert default, per-player, and list operations.";
    }

    public override string getHelp()
    {
        return "Usage:\n"
            + "  agf-sa\n"
            + "  agf-sa help\n"
            + "  agf-sa default <off|on|count>\n"
            + "  agf-sa set <entityId|all> <off|on|count|default>\n"
            + "  agf-sa list";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        int senderEntityId = -1;
        TryGetSenderEntityId(_senderInfo, out senderEntityId);

        if (!IsSenderAdmin(senderEntityId))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] admin permission required.");
            return;
        }

        if (_params == null || _params.Count == 0 || string.Equals(_params[0], "help", StringComparison.OrdinalIgnoreCase))
        {
            OutputUsageAndDefault();
            return;
        }

        string sub = _params[0].Trim().ToLowerInvariant();
        switch (sub)
        {
            case "default":
                HandleDefault(_params);
                return;
            case "set":
                HandleSet(_params);
                return;
            case "list":
                HandleList();
                return;
            default:
                SdtdConsole.Instance.Output("[ScreamerAlert] invalid option. Use: agf-sa help.");
                return;
        }
    }

    private static void OutputUsageAndDefault()
    {
        SdtdConsole.Instance.Output("[ScreamerAlert] Usage:");
        SdtdConsole.Instance.Output("[ScreamerAlert]   agf-sa default <off|on|count>");
        SdtdConsole.Instance.Output("[ScreamerAlert]   agf-sa set <entityId|all> <off|on|count|default>");
        SdtdConsole.Instance.Output("[ScreamerAlert]   agf-sa list");

        ScreamerAlertMode currentDefault = NormalizeForOutput(ScreamerAlertModeSettings.GetServerDefaultMode(), enhancedAvailable: true);
        SdtdConsole.Instance.Output("[ScreamerAlert] default currently set to " + ModeToken(currentDefault));
        SdtdConsole.Instance.Output("[ScreamerAlert] If target is set to COUNT and EnhancedAGF=NO, will use ON instead.");
    }

    private static void HandleDefault(List<string> args)
    {
        if (args.Count < 2)
        {
            OutputUsageAndDefault();
            return;
        }

        if (!TryParseModeOrDefaultKeyword(args[1], false, out ScreamerAlertMode next, out bool _))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] invalid option. Use: agf-sa default <off|on|count>.");
            return;
        }

        ScreamerAlertMode normalized = NormalizeForOutput(next, enhancedAvailable: true);
        ScreamerAlertModeSettings.SetServerDefaultMode(normalized);

        switch (normalized)
        {
            case ScreamerAlertMode.Off:
                SdtdConsole.Instance.Output("[ScreamerAlert] default set to OFF. New joining players will start with Screamer Alert set to OFF.");
                break;
            case ScreamerAlertMode.On:
                SdtdConsole.Instance.Output("[ScreamerAlert] default set to ON. New joining players will start with Screamer Alert set to ON.");
                break;
            case ScreamerAlertMode.OnWithNumbers:
                SdtdConsole.Instance.Output("[ScreamerAlert] default set to COUNT. New joining players will start with Screamer Alert set to COUNT when EnhancedAGF is available, otherwise ON.");
                break;
        }
    }

    private static void HandleSet(List<string> args)
    {
        if (args.Count < 3)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] invalid option. Use: agf-sa help.");
            return;
        }

        string targetToken = args[1].Trim();
        string modeToken = args[2].Trim();

        if (string.Equals(targetToken, "all", StringComparison.OrdinalIgnoreCase))
        {
            HandleSetAll(modeToken);
            return;
        }

        if (!int.TryParse(targetToken, out int targetEntityId) || targetEntityId < 0)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] invalid entityId. Use numeric entityId or all.");
            return;
        }

        if (!TryParseModeOrDefaultKeyword(modeToken, true, out ScreamerAlertMode requestedMode, out bool useDefault))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] invalid option. Use: agf-sa help.");
            return;
        }

        EntityPlayer targetPlayer = GameManager.Instance?.World?.GetEntity(targetEntityId) as EntityPlayer;
        if (targetPlayer == null)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] invalid entityId. Use numeric entityId or all.");
            return;
        }

        bool enhanced = IsEnhancedAvailableForEntity(targetEntityId);
        ScreamerAlertMode baseDefault = ScreamerAlertModeSettings.GetServerDefaultMode();
        ScreamerAlertMode defaultForTarget = NormalizeForOutput(baseDefault, enhancedAvailable: enhanced);
        ScreamerAlertMode effectiveMode = useDefault ? defaultForTarget : NormalizeForOutput(requestedMode, enhanced);

        if (!ScreamerAlertModeSettings.SetModeForEntityId(targetEntityId, effectiveMode))
        {
            return;
        }

        string playerName = SafePlayerName(targetPlayer);
        if (useDefault)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] entity=" + targetEntityId + " (" + playerName + ") set to current default=" + ModeToken(baseDefault) + ".");
        }
        else if (requestedMode == ScreamerAlertMode.OnWithNumbers && !enhanced)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] entity=" + targetEntityId + " (" + playerName + ") mode set to ON. EnhancedAGF=NO. COUNT is unavailable.");
        }
        else
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] entity=" + targetEntityId + " (" + playerName + ") mode set to " + ModeToken(effectiveMode) + ".");
        }

        if (useDefault && baseDefault == ScreamerAlertMode.OnWithNumbers && !enhanced)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] non-EnhancedAGF players use ON.");
        }
    }

    private static void HandleSetAll(string modeToken)
    {
        if (!TryParseModeOrDefaultKeyword(modeToken, true, out ScreamerAlertMode requestedMode, out bool useDefault))
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] invalid mode. Use: agf-sa set all <off|on|count|default>.");
            return;
        }

        ICollection<EntityPlayer> players = GameManager.Instance?.World?.Players?.dict?.Values;

        ScreamerAlertMode baseDefault = ScreamerAlertModeSettings.GetServerDefaultMode();
        if (players != null)
        {
            foreach (EntityPlayer player in players)
            {
                if (player == null || player.IsDead())
                {
                    continue;
                }

                bool enhanced = IsEnhancedAvailableForEntity(player.entityId);
                ScreamerAlertMode resolvedDefault = NormalizeForOutput(baseDefault, enhanced);
                ScreamerAlertMode effectiveMode = useDefault ? resolvedDefault : NormalizeForOutput(requestedMode, enhanced);

                ScreamerAlertModeSettings.SetModeForEntityId(player.entityId, effectiveMode);
            }
        }

        if (useDefault)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] all online players set to current default=" + ModeToken(baseDefault) + ".");
            if (baseDefault == ScreamerAlertMode.OnWithNumbers)
            {
                SdtdConsole.Instance.Output("[ScreamerAlert] if current default=COUNT, non-EnhancedAGF players use ON.");
            }
            return;
        }

        if (requestedMode == ScreamerAlertMode.OnWithNumbers)
        {
            SdtdConsole.Instance.Output("[ScreamerAlert] all online players set to COUNT where EnhancedAGF is available, otherwise ON. default unchanged.");
            return;
        }

        SdtdConsole.Instance.Output("[ScreamerAlert] all online players set to " + ModeToken(requestedMode) + ". default unchanged.");
    }

    private static void HandleList()
    {
        ICollection<EntityPlayer> players = GameManager.Instance?.World?.Players?.dict?.Values;
        SdtdConsole.Instance.Output("[ScreamerAlert] playername,id,screamer_alert,enhancedagf");

        int total = 0;
        if (players != null)
        {
            foreach (EntityPlayer player in players)
            {
                if (player == null || player.IsDead())
                {
                    continue;
                }

                total++;
                string capabilityState = GetCapabilityState(player.entityId, out bool enhanced);
                ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForEntityId(player.entityId, ScreamerAlertModeSettings.GetServerDefaultMode());
                ScreamerAlertMode normalized = NormalizeForOutput(mode, enhanced);

                SdtdConsole.Instance.Output("[ScreamerAlert] " + SafePlayerName(player) + "," + player.entityId + "," + ModeToken(normalized) + "," + capabilityState);
            }
        }

        SdtdConsole.Instance.Output("[ScreamerAlert] total online=" + total);
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

    private static bool TryParseModeOrDefaultKeyword(string text, bool allowDefaultKeyword, out ScreamerAlertMode mode, out bool useDefault)
    {
        useDefault = false;
        if (allowDefaultKeyword && text != null && text.Trim().Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            mode = ScreamerAlertModeSettings.GetServerDefaultMode();
            useDefault = true;
            return true;
        }

        if (TryParseMode(text, out mode))
        {
            return true;
        }

        if (text != null)
        {
            string lowered = text.Trim().ToLowerInvariant();
            if (lowered == "count")
            {
                mode = ScreamerAlertMode.OnWithNumbers;
                return true;
            }
        }

        return false;
    }

    private static ScreamerAlertMode NormalizeForOutput(ScreamerAlertMode mode, bool enhancedAvailable)
    {
        if (!enhancedAvailable && mode == ScreamerAlertMode.OnWithNumbers)
        {
            return ScreamerAlertMode.On;
        }

        return mode;
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

    private static string SafePlayerName(EntityPlayer player)
    {
        if (player == null)
        {
            return "Unknown";
        }

        string n = player.EntityName;
        if (string.IsNullOrEmpty(n))
        {
            n = "Unknown";
        }

        return n.Replace(",", " ");
    }

    private static bool IsEnhancedAvailableForEntity(int entityId)
    {
        ClientInfo client = SingletonMonoBehaviour<ConnectionManager>.Instance?.Clients?.ForEntityId(entityId);
        return ScreamerAlertHybridRouting.HasClientCapability(client);
    }

    private static string GetCapabilityState(int entityId, out bool enhanced)
    {
        ClientInfo client = SingletonMonoBehaviour<ConnectionManager>.Instance?.Clients?.ForEntityId(entityId);
        if (client == null)
        {
            enhanced = false;
            return "UNKNOWN";
        }

        enhanced = ScreamerAlertHybridRouting.HasClientCapability(client);
        return enhanced ? "YES" : "NO";
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
