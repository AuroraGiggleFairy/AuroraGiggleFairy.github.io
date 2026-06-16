using System;
using UnityEngine;

public static class DecoUtils
{
	public delegate bool DecoAllowedTest(EnumDecoAllowed decoAllowed);

	public static bool IsBigDeco(BlockValue blockValue, Block block)
	{
		if (block.SmallDecorationRadius <= 0 && block.BigDecorationRadius <= 0)
		{
			return block.isOversized;
		}
		return true;
	}

	public static int GetDecoRadius(BlockValue blockValue, Block block)
	{
		int num = Math.Max(block.SmallDecorationRadius, block.BigDecorationRadius);
		if (block.isOversized)
		{
			Vector3 extents = block.oversizedBounds.extents;
			num = Math.Max(num, Math.Max((int)(extents.x + 0.5f), (int)(extents.z + 0.5f)));
		}
		return num;
	}

	public static bool CanPlaceDeco(Chunk cX0Z0, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, Vector3i blockPos, BlockValue blockValue, DecoAllowedTest additionalTest = null)
	{
		if (CanPlaceDeco(cX0Z0, blockPos, blockValue, additionalTest) && CanPlaceDeco(cX1Z0, blockPos, blockValue, additionalTest) && CanPlaceDeco(cX0Z1, blockPos, blockValue, additionalTest))
		{
			return CanPlaceDeco(cX1Z1, blockPos, blockValue, additionalTest);
		}
		return false;
	}

	public static bool CanPlaceDeco(Chunk chunk, Vector3i blockPos, BlockValue blockValue, DecoAllowedTest additionalTest = null)
	{
		if (blockValue.isair)
		{
			return false;
		}
		Block block = blockValue.Block;
		if (block.isMultiBlock && blockValue.ischild)
		{
			return false;
		}
		int num = chunk.X * 16;
		int num2 = chunk.Z * 16;
		int x = blockPos.x - num;
		int z = blockPos.z - num2;
		if (!IsBigDeco(blockValue, block))
		{
			if (x < 0 || x >= 16 || z < 0 || z >= 16)
			{
				return true;
			}
			EnumDecoAllowed decoAllowedAt = chunk.GetDecoAllowedAt(x, z);
			if (decoAllowedAt.AllowSmallDeco())
			{
				if (additionalTest != null)
				{
					return additionalTest(decoAllowedAt);
				}
				return true;
			}
			return false;
		}
		int cxMax = num + 16 - 1;
		int czMax = num2 + 16 - 1;
		if (CanPlaceBigDecoForBlockPos() && CanPlaceBigDecoForBlockDecorationRadius(chunk, num, num2, cxMax, czMax, blockPos, blockValue, block, additionalTest))
		{
			return CanPlaceBigDecoForBlockOversized(chunk, num, num2, cxMax, czMax, blockPos, blockValue, block, additionalTest);
		}
		return false;
		[PublicizedFrom(EAccessModifier.Internal)]
		bool CanPlaceBigDecoForBlockPos()
		{
			if (x < 0 || x >= 16 || z < 0 || z >= 16)
			{
				return true;
			}
			EnumDecoAllowed decoAllowedAt2 = chunk.GetDecoAllowedAt(x, z);
			if (decoAllowedAt2.AllowBigDeco())
			{
				if (additionalTest != null)
				{
					return additionalTest(decoAllowedAt2);
				}
				return true;
			}
			return false;
		}
	}

	public static bool HasDecoAllowed(BlockValue blockValue)
	{
		if (blockValue.isair)
		{
			return false;
		}
		Block block = blockValue.Block;
		if (block.isMultiBlock && blockValue.ischild)
		{
			return false;
		}
		if (block.SmallDecorationRadius <= 0 && block.BigDecorationRadius <= 0)
		{
			return block.isOversized;
		}
		return true;
	}

	public static void ApplyDecoAllowed(Chunk cX0Z0, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, Vector3i blockPos, BlockValue blockValue)
	{
		ApplyDecoAllowed(cX0Z0, blockPos, blockValue);
		ApplyDecoAllowed(cX1Z0, blockPos, blockValue);
		ApplyDecoAllowed(cX0Z1, blockPos, blockValue);
		ApplyDecoAllowed(cX1Z1, blockPos, blockValue);
	}

	public static void ApplyDecoAllowed(Chunk chunk, Vector3i blockPos, BlockValue blockValue)
	{
		if (HasDecoAllowed(blockValue))
		{
			Block block = blockValue.Block;
			int num = chunk.X * 16;
			int num2 = chunk.Z * 16;
			int cxMax = num + 16 - 1;
			int czMax = num2 + 16 - 1;
			ApplyDecoAllowedForBlockDecorationRadius(chunk, num, num2, cxMax, czMax, blockPos, blockValue, block);
			ApplyDecoAllowedForBlockOversized(chunk, num, num2, cxMax, czMax, blockPos, blockValue, block);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CanPlaceBigDecoForBlockDecorationRadius(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block, DecoAllowedTest additionalTest)
	{
		int num = Math.Max(block.SmallDecorationRadius, block.BigDecorationRadius);
		if (num <= 0)
		{
			return true;
		}
		int num2 = blockPos.x - num;
		int num3 = blockPos.z - num;
		int num4 = blockPos.x + num;
		int num5 = blockPos.z + num;
		if (num2 > cxMax || num3 > czMax || num4 < cxMin || num5 < czMin)
		{
			return true;
		}
		int num6 = Math.Clamp(num2 - cxMin, 0, 15);
		int num7 = Math.Clamp(num3 - czMin, 0, 15);
		int num8 = Math.Clamp(num4 - cxMin, 0, 15);
		int num9 = Math.Clamp(num5 - czMin, 0, 15);
		for (int i = num7; i <= num9; i++)
		{
			for (int j = num6; j <= num8; j++)
			{
				EnumDecoAllowed decoAllowedAt = chunk.GetDecoAllowedAt(j, i);
				if (!decoAllowedAt.AllowBigDeco() && (additionalTest == null || additionalTest(decoAllowedAt)))
				{
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyDecoAllowedForBlockDecorationRadius(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block)
	{
		int smallDecorationRadius = block.SmallDecorationRadius;
		int num = Math.Max(smallDecorationRadius, block.BigDecorationRadius);
		if (num <= 0)
		{
			return;
		}
		int num2 = blockPos.x - num;
		int num3 = blockPos.z - num;
		int num4 = blockPos.x + num;
		int num5 = blockPos.z + num;
		if (num2 > cxMax || num3 > czMax || num4 < cxMin || num5 < czMin)
		{
			return;
		}
		int num6 = Math.Clamp(num2 - cxMin, 0, 15);
		int num7 = Math.Clamp(num3 - czMin, 0, 15);
		int num8 = Math.Clamp(num4 - cxMin, 0, 15);
		int num9 = Math.Clamp(num5 - czMin, 0, 15);
		for (int i = num7; i <= num9; i++)
		{
			for (int j = num6; j <= num8; j++)
			{
				if (smallDecorationRadius == num || (smallDecorationRadius > 0 && Math.Max(Math.Abs(i), Math.Abs(j)) <= smallDecorationRadius))
				{
					chunk.SetDecoAllowedSizeAt(j, i, EnumDecoAllowedSize.None);
				}
				else
				{
					chunk.SetDecoAllowedSizeAt(j, i, EnumDecoAllowedSize.OnlySmall);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CanPlaceBigDecoForBlockOversized(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block, DecoAllowedTest additionalTest)
	{
		Bounds clipBounds = default(Bounds);
		clipBounds.SetMinMax(new Vector3(cxMin, 0f, czMin), new Vector3(cxMax, 0f, czMax));
		foreach (Vector3i item in OversizedBlockUtils.EnumerateOverlappingCells(blockPos, block.oversizedBounds, blockValue.rotation, clipBounds))
		{
			EnumDecoAllowed decoAllowedAt = chunk.GetDecoAllowedAt(item.x - cxMin, item.z - czMin);
			if (!decoAllowedAt.AllowBigDeco() && (additionalTest == null || additionalTest(decoAllowedAt)))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyDecoAllowedForBlockOversized(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block)
	{
		Bounds clipBounds = default(Bounds);
		clipBounds.SetMinMax(new Vector3(cxMin, 0f, czMin), new Vector3(cxMax, 0f, czMax));
		foreach (Vector3i item in OversizedBlockUtils.EnumerateOverlappingCells(blockPos, block.oversizedBounds, blockValue.rotation, clipBounds))
		{
			chunk.SetDecoAllowedSizeAt(item.x - cxMin, item.z - czMin, EnumDecoAllowedSize.None);
		}
	}
}
