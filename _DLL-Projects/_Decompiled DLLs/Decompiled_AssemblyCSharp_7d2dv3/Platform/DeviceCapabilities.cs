namespace Platform;

public static class DeviceCapabilities
{
	public static bool CanUserAccessFilesystem()
	{
		if (!Submission.Enabled)
		{
			return true;
		}
		return (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
	}
}
