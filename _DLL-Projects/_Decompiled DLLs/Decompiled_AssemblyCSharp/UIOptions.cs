using System;
using Platform;

public static class UIOptions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static OptionsVideoWindowMode optionsVideoWindow;

	public static OptionsVideoWindowMode OptionsVideoWindow
	{
		get
		{
			return optionsVideoWindow;
		}
		set
		{
			optionsVideoWindow = value;
			UIOptions.OnOptionsVideoWindowChanged?.Invoke(value);
		}
	}

	public static event Action<OptionsVideoWindowMode> OnOptionsVideoWindowChanged;

	public static void Init()
	{
		optionsVideoWindow = ((!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent()) ? OptionsVideoWindowMode.Detailed : OptionsVideoWindowMode.Simplified);
	}
}
