/// <summary>
/// Dan's Doom HUD is a separate mod. EnhancedAGF checks for it once at startup.
/// </summary>
public static class DoomHudGate
{
    public const string ModName = "FranticDansCleanerHUD1080";

    public static bool IsLoaded { get; private set; }

    public static void Resolve()
    {
        IsLoaded = ModManager.GetMod(ModName) != null;
    }
}
