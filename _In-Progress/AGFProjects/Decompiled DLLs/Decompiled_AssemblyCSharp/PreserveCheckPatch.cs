using System.Diagnostics;

public static class PreserveCheckPatch
{
	[Conditional("ENABLE_MONO")]
	public static void Enable()
	{
	}

	[Conditional("ENABLE_MONO")]
	public static void Disable()
	{
	}
}
