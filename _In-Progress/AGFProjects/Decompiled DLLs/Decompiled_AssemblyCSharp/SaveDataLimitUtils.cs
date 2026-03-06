using System;

public static class SaveDataLimitUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const long CHUNK_SIZE = 16L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long CHUNK_AREA = 256L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PLAYER_MAP_OVERHEAD_PER_CHUNK = 516L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PLAYER_MAP_MAX_OVERHEAD = 270532608L;

	public static long CalculatePlayerMapSize(Vector2i worldSize)
	{
		int num = worldSize.x * worldSize.y;
		if (num <= 0)
		{
			throw new ArgumentException($"Expected a positive value for the world area, but was: {num}", "worldSize");
		}
		return Math.Min((long)num / 256L * 516, 270532608L);
	}
}
