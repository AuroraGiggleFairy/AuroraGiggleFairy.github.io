using System.Diagnostics;

public static class RegionFileLog
{
	[Conditional("DEBUG_REGIONLOG")]
	public static void Region(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} Region {_format}";
		Log.Warning(_format, _args);
	}
}
