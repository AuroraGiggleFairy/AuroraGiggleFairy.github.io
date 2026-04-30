namespace System.Numerics.Hashing;

internal static class _003C7b849468_002Dff53_002D41a4_002D939b_002Db0f040936437_003EHashHelpers
{
	public static readonly int RandomSeed = Guid.NewGuid().GetHashCode();

	public static int Combine(int h1, int h2)
	{
		uint num = (uint)((h1 << 5) | (h1 >>> 27));
		return ((int)num + h1) ^ h2;
	}
}
