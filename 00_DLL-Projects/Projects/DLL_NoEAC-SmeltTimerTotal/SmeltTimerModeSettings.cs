using UnityEngine;

public static class SmeltTimerModeSettings
{
	public const string PrefKey = "AGF.SmeltTimerTotal.Mode";
	// Matches the one-row mode strip height in windows.xml (content shifts by this amount).
	public const float ModeBarHeight = 32f;

	public enum Mode
	{
		Single = 0,
		Total = 1
	}

	private static Mode cached = Mode.Total;
	private static bool loaded;

	public static Mode Current
	{
		get
		{
			EnsureLoaded();
			return cached;
		}
		set
		{
			cached = value;
			loaded = true;
			PlayerPrefs.SetInt(PrefKey, (int)value);
			PlayerPrefs.Save();
		}
	}

	public static bool IsTotal => Current == Mode.Total;

	private static void EnsureLoaded()
	{
		if (loaded)
		{
			return;
		}

		cached = (Mode)PlayerPrefs.GetInt(PrefKey, (int)Mode.Total);
		loaded = true;
	}
}
