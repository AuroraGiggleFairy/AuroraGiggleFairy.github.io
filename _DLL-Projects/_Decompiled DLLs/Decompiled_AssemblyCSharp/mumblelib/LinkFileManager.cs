using System;

namespace mumblelib;

public static class LinkFileManager
{
	public static ILinkFile Open()
	{
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			Log.Out("[MumbleLF] Loading Windows Mumble Link");
			return new WindowsLinkFile();
		}
		Log.Out("[MumbleLF] Loading Unix Mumble Link");
		return new UnixLinkFile();
	}
}
