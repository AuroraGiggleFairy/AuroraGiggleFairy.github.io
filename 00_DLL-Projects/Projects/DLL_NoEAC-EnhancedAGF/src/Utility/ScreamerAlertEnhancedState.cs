using System;

public static class ScreamerAlertEnhancedState
{
    private const string Protocol = ".agfSAProtocol";
    private const string Scouts = ".agfSAScoutCount";
    private const string Hordes = ".agfSAHordeCount";
    private const string ModeName = ".agfSAMode";
    public static bool Available { get; private set; }
    public static int ScoutCount { get; private set; }
    public static int HordeCount { get; private set; }
    public static ScreamerAlertMode Mode { get; private set; } = ScreamerAlertMode.On;
    public static event Action Changed;

    public static void Tick()
    {
        EntityPlayer p = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (p?.Buffs == null) return;
        bool available = p.Buffs.HasCustomVar(Protocol) && p.Buffs.GetCustomVar(Protocol) >= 2f;
        int scouts = Read(p, Scouts);
        int hordes = Read(p, Hordes);
        ScreamerAlertMode mode = ReadMode(p);
        if (available == Available && scouts == ScoutCount && hordes == HordeCount && mode == Mode) return;
        Available = available;
        ScoutCount = scouts;
        HordeCount = hordes;
        Mode = mode;
        ScreamerAlertModeSettings.SetModeForLocalPlayer(mode);
        Changed?.Invoke();
    }

    public static string ScoutText() => Format(Localize("ScreamerAlert_Scout", "Screamer Alert"), ScoutCount);
    public static string HordeText() => Format(Localize("ScreamerAlert_Horde", "Horde Incoming"), HordeCount);

    private static string Format(string text, int count)
    {
        if (!Available || Mode == ScreamerAlertMode.Off || count <= 0) return string.Empty;
        return Mode == ScreamerAlertMode.OnWithNumbers ? text + " [FFFFFF](" + count + ")[-]" : text;
    }

    private static int Read(EntityPlayer p, string name) => p.Buffs.HasCustomVar(name) ? Math.Max(0, (int)p.Buffs.GetCustomVar(name)) : 0;
    private static ScreamerAlertMode ReadMode(EntityPlayer p)
    {
        if (!p.Buffs.HasCustomVar(ModeName)) return ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.On);
        int value = (int)p.Buffs.GetCustomVar(ModeName);
        return value == 0 ? ScreamerAlertMode.Off : value == 2 ? ScreamerAlertMode.OnWithNumbers : ScreamerAlertMode.On;
    }
    private static string Localize(string key, string fallback)
    {
        string value = Localization.Get(key);
        return string.IsNullOrEmpty(value) || value == key ? fallback : value;
    }
}