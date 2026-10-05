using System;
using System.Collections.Generic;
using VisualEntityTrackerAddon;

public class ConsoleCmdVisualEntityTrackerAddon : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return new[] { "agf-et", "agfet", "agf-vet", "agfvet" };
    }

    public override string getDescription()
    {
        return "Admin controls for Visual Entity Tracker default, per-player, and list operations.";
    }

    public override string getHelp()
    {
        return "Usage:\n"
            + "  agf-et\n"
            + "  agf-et help\n"
            + "  agf-et default <off|on>\n"
            + "  agf-et set <entityId|all> <off|on|default>\n"
            + "  agf-et list";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        int senderEntityId = -1;
        TryGetSenderEntityId(_senderInfo, out senderEntityId);

        if (!IsSenderAdmin(_senderInfo, senderEntityId))
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] admin permission required.");
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
                SdtdConsole.Instance.Output("[VisualEntityTracker] invalid option. Use: agf-et help.");
                return;
        }
    }

    private static void OutputUsageAndDefault()
    {
        SdtdConsole.Instance.Output("[VisualEntityTracker] Usage:");
        SdtdConsole.Instance.Output("[VisualEntityTracker]   agf-et default <off|on>");
        SdtdConsole.Instance.Output("[VisualEntityTracker]   agf-et set <entityId|all> <off|on|default>");
        SdtdConsole.Instance.Output("[VisualEntityTracker]   agf-et list");
        SdtdConsole.Instance.Output("[VisualEntityTracker] default currently set to " + VisualEntityTrackerModeSettings.GetModeToken(VisualEntityTrackerModeSettings.GetServerDefaultMode()));
    }

    private static void HandleDefault(List<string> args)
    {
        if (args.Count < 2)
        {
            OutputUsageAndDefault();
            return;
        }

        if (!VisualEntityTrackerModeSettings.TryParseCommandMode(args[1], out VisualEntityTrackerMode next))
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] invalid option. Use: agf-et default <off|on>.");
            return;
        }

        VisualEntityTrackerModeSettings.SetServerDefaultMode(next);
        if (next == VisualEntityTrackerMode.Off)
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] default set to OFF. New joining players will start with Visual Entity Tracker set to OFF.");
        }
        else
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] default set to ON. New joining players will start with Visual Entity Tracker set to ON.");
        }
    }

    private static void HandleSet(List<string> args)
    {
        if (args.Count < 3)
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] invalid option. Use: agf-et help.");
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
            SdtdConsole.Instance.Output("[VisualEntityTracker] invalid entityId. Use numeric entityId or all.");
            return;
        }

        if (!TryParseModeOrDefaultKeyword(modeToken, out VisualEntityTrackerMode requestedMode, out bool useDefault))
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] invalid option. Use: agf-et help.");
            return;
        }

        EntityPlayer targetPlayer = GameManager.Instance?.World?.GetEntity(targetEntityId) as EntityPlayer;
        if (targetPlayer == null)
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] invalid entityId. Use numeric entityId or all.");
            return;
        }

        VisualEntityTrackerMode baseDefault = VisualEntityTrackerModeSettings.GetServerDefaultMode();
        VisualEntityTrackerMode effectiveMode = useDefault ? baseDefault : requestedMode;

        if (!VisualEntityTrackerModeSettings.SetModeForEntityId(targetEntityId, effectiveMode))
        {
            return;
        }

        VisualEntityTrackerAddonService.ApplyServerSideMode(targetEntityId, effectiveMode == VisualEntityTrackerMode.On);

        string playerName = SafePlayerName(targetPlayer);
        if (useDefault)
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] entity=" + targetEntityId + " (" + playerName + ") set to current default=" + VisualEntityTrackerModeSettings.GetModeToken(baseDefault) + ".");
        }
        else
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] entity=" + targetEntityId + " (" + playerName + ") mode set to " + VisualEntityTrackerModeSettings.GetModeToken(effectiveMode) + ".");
        }
    }

    private static void HandleSetAll(string modeToken)
    {
        if (!TryParseModeOrDefaultKeyword(modeToken, out VisualEntityTrackerMode requestedMode, out bool useDefault))
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] invalid mode. Use: agf-et set all <off|on|default>.");
            return;
        }

        VisualEntityTrackerMode baseDefault = VisualEntityTrackerModeSettings.GetServerDefaultMode();
        ICollection<EntityPlayer> players = GameManager.Instance?.World?.Players?.dict?.Values;
        if (players != null)
        {
            foreach (EntityPlayer player in players)
            {
                if (player == null || player.IsDead())
                {
                    continue;
                }

                VisualEntityTrackerMode effectiveMode = useDefault ? baseDefault : requestedMode;
                VisualEntityTrackerModeSettings.SetModeForEntityId(player.entityId, effectiveMode);
                VisualEntityTrackerAddonService.ApplyServerSideMode(player.entityId, effectiveMode == VisualEntityTrackerMode.On);
            }
        }

        if (useDefault)
        {
            SdtdConsole.Instance.Output("[VisualEntityTracker] all online players set to current default=" + VisualEntityTrackerModeSettings.GetModeToken(baseDefault) + ".");
            return;
        }

        SdtdConsole.Instance.Output("[VisualEntityTracker] all online players set to " + VisualEntityTrackerModeSettings.GetModeToken(requestedMode) + ". default unchanged.");
    }

    private static void HandleList()
    {
        int total = 0;
        int index = 0;
        ICollection<EntityPlayer> players = GameManager.Instance?.World?.Players?.dict?.Values;
        if (players != null)
        {
            foreach (EntityPlayer player in players)
            {
                if (player == null || player.IsDead())
                {
                    continue;
                }

                total++;
                VisualEntityTrackerMode mode = VisualEntityTrackerModeSettings.GetModeForEntityId(
                    player.entityId,
                    VisualEntityTrackerModeSettings.GetServerDefaultMode());
                SdtdConsole.Instance.Output("[VisualEntityTracker] " + index + ". id=" + player.entityId + ", " + SafePlayerName(player) + ", et=" + VisualEntityTrackerModeSettings.GetModeToken(mode));
                index++;
            }
        }

        SdtdConsole.Instance.Output("[VisualEntityTracker] total online=" + total);
    }

    private static bool TryParseModeOrDefaultKeyword(string text, out VisualEntityTrackerMode mode, out bool useDefault)
    {
        useDefault = false;
        mode = VisualEntityTrackerMode.On;
        if (text != null && text.Trim().Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            mode = VisualEntityTrackerModeSettings.GetServerDefaultMode();
            useDefault = true;
            return true;
        }

        return VisualEntityTrackerModeSettings.TryParseCommandMode(text, out mode);
    }

    private static bool IsSenderAdmin(CommandSenderInfo senderInfo, int senderEntityId)
    {
        if (senderInfo.IsLocalGame)
        {
            return true;
        }

        if (senderInfo.RemoteClientInfo == null && senderEntityId < 0)
        {
            return true;
        }

        EntityPlayer senderPlayer = GameManager.Instance?.World?.GetEntity(senderEntityId) as EntityPlayer;
        return senderPlayer != null && senderPlayer.IsAdmin;
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

    private static bool TryGetSenderEntityId(CommandSenderInfo senderInfo, out int entityId)
    {
        entityId = -1;
        if (senderInfo.RemoteClientInfo != null)
        {
            entityId = senderInfo.RemoteClientInfo.entityId;
            return true;
        }

        EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (localPlayer != null)
        {
            entityId = localPlayer.entityId;
            return true;
        }

        return true;
    }
}
