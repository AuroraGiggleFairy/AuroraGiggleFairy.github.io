using HarmonyLib;

public static class ScreamerAlertEnhancedGate
{
    private static bool _serverScreamerDetected;

    public static bool IsScreamerPresentLocally()
    {
        return AccessTools.TypeByName("ScreamerAlertsController") != null
            && AccessTools.TypeByName("ScreamerAlertManager") != null;
    }

    public static bool IsServerScreamerDetected()
    {
        return _serverScreamerDetected;
    }

    public static void MarkServerScreamerDetected()
    {
        if (_serverScreamerDetected)
        {
            return;
        }

        _serverScreamerDetected = true;
        Logging.Inform("ScreamerAlertEnhancedGate", "Detected ScreamerAlert server activity; enhanced client mode unlocked.");
    }

    public static bool IsScreamerInPlay()
    {
        return IsScreamerPresentLocally() || IsServerScreamerDetected();
    }

    public static bool ShouldProcessClientHooks()
    {
        return !GameManager.IsDedicatedServer;
    }

    public static bool ShouldApplyRuntimeBehavior()
    {
        if (!ShouldProcessClientHooks())
        {
            return false;
        }

        return IsScreamerInPlay();
    }
}