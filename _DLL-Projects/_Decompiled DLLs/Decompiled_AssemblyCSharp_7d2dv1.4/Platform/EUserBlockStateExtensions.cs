using System;

namespace Platform;

public static class EUserBlockStateExtensions
{
	public static bool IsBlocked(this EUserBlockState blockState)
	{
		switch (blockState)
		{
		case EUserBlockState.InGame:
		case EUserBlockState.ByPlatform:
			return true;
		case EUserBlockState.NotBlocked:
			return false;
		default:
			throw new ArgumentOutOfRangeException("blockState", blockState, string.Format("{0} not implemented for {1}.{2}", "IsBlocked", "EUserBlockState", blockState));
		}
	}
}
