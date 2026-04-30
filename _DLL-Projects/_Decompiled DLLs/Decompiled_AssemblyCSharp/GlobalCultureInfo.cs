using System.Globalization;
using System.Threading;

public class GlobalCultureInfo
{
	public static bool SetDefaultCulture(CultureInfo _culture)
	{
		Thread.CurrentThread.CurrentCulture = _culture;
		CultureInfo.DefaultThreadCurrentCulture = _culture;
		return true;
	}
}
