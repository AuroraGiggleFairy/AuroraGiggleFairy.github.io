using System;
using System.Reflection;

public static class ScreamerAlertEnhancedGate
{
    private static readonly string[] ScreamerModNames =
    {
        "AGF-ScreamerAlert",
        "AGF-NoEAC-ScreamerAlert"
    };
    private const string ScreamerManagerTypeName = "ScreamerAlertManager";

    private static bool _localPresenceResolved;
    private static bool _screamerPresentLocally;
    private static bool _serverScreamerDetected;

    public static bool IsScreamerPresentLocally()
    {
        if (_localPresenceResolved)
        {
            return _screamerPresentLocally;
        }

        _screamerPresentLocally = DetectLocalScreamerAlert();
        _localPresenceResolved = true;
        return _screamerPresentLocally;
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

    public static void ResetServerDetection()
    {
        _serverScreamerDetected = false;
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

    /// <summary>
    /// Host with no local ScreamerAlert must never handshake. Remote clients still
    /// watch for Protocol CVar in case the server has ScreamerAlert and this PC does not.
    /// </summary>
    public static bool ShouldRunCapabilityHandshake()
    {
        if (GameManager.IsDedicatedServer)
        {
            return false;
        }

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (manager != null && manager.IsServer)
        {
            return IsScreamerPresentLocally();
        }

        return true;
    }

    public static Type FindLoadedType(string simpleName)
    {
        if (string.IsNullOrEmpty(simpleName))
        {
            return null;
        }

        AppDomain domain = AppDomain.CurrentDomain;
        if (domain == null)
        {
            return null;
        }

        Assembly[] assemblies;
        try
        {
            assemblies = domain.GetAssemblies();
        }
        catch
        {
            return null;
        }

        for (int i = 0; i < assemblies.Length; i++)
        {
            Assembly assembly = assemblies[i];
            if (assembly == null)
            {
                continue;
            }

            try
            {
                Type type = assembly.GetType(simpleName, false);
                if (type != null)
                {
                    return type;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static bool DetectLocalScreamerAlert()
    {
        try
        {
            for (int i = 0; i < ScreamerModNames.Length; i++)
            {
                if (ModManager.GetMod(ScreamerModNames[i]) != null)
                {
                    return true;
                }
            }
        }
        catch
        {
        }

        return FindLoadedType(ScreamerManagerTypeName) != null;
    }
}
