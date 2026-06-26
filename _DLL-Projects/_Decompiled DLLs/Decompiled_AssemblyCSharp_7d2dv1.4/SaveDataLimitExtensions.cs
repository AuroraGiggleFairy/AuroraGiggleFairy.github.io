using System;

public static class SaveDataLimitExtensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const long MB = 1048576L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long FLAT_OVERHEAD = 104857600L;

	public static bool IsSupported(this SaveDataLimitType saveDataLimitType)
	{
		if (PlatformOptimizations.LimitedSaveData)
		{
			return saveDataLimitType != SaveDataLimitType.Unlimited;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long GetRegionSizeLimit(this SaveDataLimitType saveDataLimitType)
	{
		if (!saveDataLimitType.IsSupported())
		{
			throw new ArgumentException(string.Format("Unexpected usage of {0}.{1}.{2}() when not supported by the current device.", "SaveDataLimitType", saveDataLimitType, "GetRegionSizeLimit"), "saveDataLimitType");
		}
		return saveDataLimitType switch
		{
			SaveDataLimitType.Unlimited => -1L, 
			SaveDataLimitType.Short => 33554432L, 
			SaveDataLimitType.Medium => 67108864L, 
			SaveDataLimitType.Long => 134217728L, 
			SaveDataLimitType.VeryLong => 268435456L, 
			_ => throw new ArgumentOutOfRangeException("saveDataLimitType", saveDataLimitType, null), 
		};
	}

	public static long CalculateTotalSize(this SaveDataLimitType saveDataLimitType, Vector2i worldSize)
	{
		if (!saveDataLimitType.IsSupported())
		{
			throw new ArgumentException(string.Format("Unexpected usage of {0}.{1}.{2}() when not supported by the current device.", "SaveDataLimitType", saveDataLimitType, "CalculateTotalSize"), "saveDataLimitType");
		}
		long regionSizeLimit = saveDataLimitType.GetRegionSizeLimit();
		if (regionSizeLimit <= 0)
		{
			return -1L;
		}
		long num = SaveDataLimitUtils.CalculatePlayerMapSize(worldSize);
		return 104857600 + num + regionSizeLimit;
	}
}
