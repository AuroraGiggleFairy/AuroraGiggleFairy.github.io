public static class BundleTags
{
	public const string TagHalfRes = "_halfres";

	public static string Tag
	{
		get
		{
			if (!PlatformOptimizations.LoadHalfResAssets)
			{
				return string.Empty;
			}
			return "_halfres";
		}
	}
}
