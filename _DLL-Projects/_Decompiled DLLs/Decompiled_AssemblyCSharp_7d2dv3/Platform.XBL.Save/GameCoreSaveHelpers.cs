using System.Diagnostics;

namespace Platform.XBL.Save;

public static class GameCoreSaveHelpers
{
	[Conditional("NEVER_DEFINED")]
	public static void TraceLogHR(int hr, string identifier)
	{
		XblHelpers.LogHR(hr, identifier);
	}

	public static void NonTraceLogHR(int hr, string identifier)
	{
		XblHelpers.LogHR(hr, identifier);
	}
}
