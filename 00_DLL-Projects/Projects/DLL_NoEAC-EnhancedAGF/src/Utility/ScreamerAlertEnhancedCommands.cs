using System;

public static class ScreamerAlertEnhancedCommands
{
    private const string CommandRoot = "agf-sa";
    private const string CommandAlias = "agfsa";

    public static bool TryHandleOutgoingChatMessage(string message, out bool consumeVanilla)
    {
        consumeVanilla = false;

        try
        {
            if (!ScreamerAlertEnhancedGate.ShouldProcessClientHooks())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            if (!TryParseCommand(message, out string[] parts))
            {
                return false;
            }

            return ProcessCommand(parts, out consumeVanilla);
        }
        catch (Exception ex)
        {
            Logging.Warning("ScreamerAlertEnhancedCommands", "Failed to handle outgoing command: " + ex.Message);
            return false;
        }
    }

    private static bool TryParseCommand(string message, out string[] parts)
    {
        parts = null;

        string trimmed = message.Trim();
        if (!trimmed.StartsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        string withoutSlash = trimmed.Substring(1).Trim();
        if (string.IsNullOrEmpty(withoutSlash))
        {
            return false;
        }

        parts = withoutSlash.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        string command = parts[0].Trim().ToLowerInvariant();
        return command == CommandRoot || command == CommandAlias;
    }

    private static bool ProcessCommand(string[] parts, out bool consumeVanilla)
    {
        consumeVanilla = false;

        if (parts.Length <= 1)
        {
            return true;
        }

        TrySendCapabilityHelloForCommand();

        string action = parts[1].Trim().ToLowerInvariant();
        switch (action)
        {
            case "off":
                TrySetLocalMode(ScreamerAlertMode.Off);
                return true;
            case "on":
                TrySetLocalMode(ScreamerAlertMode.On);
                return true;
            case "count":
            case "counts":
            case "numbers":
                TrySetLocalMode(ScreamerAlertMode.OnWithNumbers);
                return true;
            default:
                return true;
        }
    }

    private static void TrySetLocalMode(ScreamerAlertMode mode)
    {
        try
        {
            ScreamerAlertModeSettings.SetModeForLocalPlayer(mode);
        }
        catch
        {
        }
    }

    private static void TrySendCapabilityHelloForCommand()
    {
        try
        {
            int localEntityId = GameManager.Instance?.World?.GetPrimaryPlayer()?.entityId ?? -1;
            ScreamerAlertEnhancedCapabilityHello.TrySendFromCommand(localEntityId);
        }
        catch
        {
        }
    }
}
