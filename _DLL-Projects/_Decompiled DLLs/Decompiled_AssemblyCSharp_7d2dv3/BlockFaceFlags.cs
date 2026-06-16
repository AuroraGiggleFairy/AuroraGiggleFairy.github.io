using System.Runtime.CompilerServices;
using UnityEngine;

public static class BlockFaceFlags
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int[] faceRotShiftValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly BlockFaceFlag[] cubeSideFaceFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] cubeSideFaceChars;

	[PublicizedFrom(EAccessModifier.Private)]
	static BlockFaceFlags()
	{
		faceRotShiftValues = new int[144];
		cubeSideFaceFlags = new BlockFaceFlag[6]
		{
			BlockFaceFlag.Top,
			BlockFaceFlag.Bottom,
			BlockFaceFlag.North,
			BlockFaceFlag.West,
			BlockFaceFlag.South,
			BlockFaceFlag.East
		};
		cubeSideFaceChars = new char[6] { 'T', 'B', 'N', 'W', 'S', 'E' };
		for (int i = 0; i < 24; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				faceRotShiftValues[i * 6 + j] = (int)((BlockFace)j).RotateFace(i) - j;
			}
		}
	}

	public static BlockFaceFlag RotateFlags(BlockFaceFlag mask, byte blockRotation)
	{
		if (mask == BlockFaceFlag.None || mask == BlockFaceFlag.All || blockRotation > 23)
		{
			return mask;
		}
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			int num2 = (int)mask & (1 << i);
			if (num2 != 0)
			{
				int num3 = faceRotShiftValues[blockRotation * 6 + i];
				num2 = ((num3 <= 0) ? (num2 >> -num3) : (num2 << num3));
				num |= num2;
			}
		}
		return (BlockFaceFlag)num;
	}

	public static BlockFace ToBlockFace(BlockFaceFlag flags)
	{
		if ((flags & BlockFaceFlag.Top) != BlockFaceFlag.None)
		{
			return BlockFace.Top;
		}
		if ((flags & BlockFaceFlag.Bottom) != BlockFaceFlag.None)
		{
			return BlockFace.Bottom;
		}
		if ((flags & BlockFaceFlag.North) != BlockFaceFlag.None)
		{
			return BlockFace.North;
		}
		if ((flags & BlockFaceFlag.South) != BlockFaceFlag.None)
		{
			return BlockFace.South;
		}
		if ((flags & BlockFaceFlag.East) != BlockFaceFlag.None)
		{
			return BlockFace.East;
		}
		if ((flags & BlockFaceFlag.West) != BlockFaceFlag.None)
		{
			return BlockFace.West;
		}
		return BlockFace.None;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlockFaceFlag FromBlockFace(BlockFace face)
	{
		if (face == BlockFace.None)
		{
			return BlockFaceFlag.None;
		}
		return (BlockFaceFlag)(1 << (int)face);
	}

	public static BlockFace OppositeFace(BlockFace face)
	{
		return face switch
		{
			BlockFace.Top => BlockFace.Bottom, 
			BlockFace.Bottom => BlockFace.Top, 
			BlockFace.North => BlockFace.South, 
			BlockFace.South => BlockFace.North, 
			BlockFace.East => BlockFace.West, 
			BlockFace.West => BlockFace.East, 
			_ => BlockFace.None, 
		};
	}

	public static BlockFace NearestFaceForDirection(Vector3 direction, float dotTolerance = 0.8f)
	{
		float num = -2f;
		BlockFace result = BlockFace.None;
		BlockFace blockFace = BlockFace.Top;
		while ((int)blockFace <= 5)
		{
			float num2 = Vector3.Dot(OffsetForFace(blockFace), direction);
			if (num2 > num)
			{
				num = num2;
				result = blockFace;
			}
			blockFace++;
		}
		if (!(num >= dotTolerance))
		{
			return BlockFace.None;
		}
		return result;
	}

	public static Vector3 OffsetForFace(BlockFace face)
	{
		return face switch
		{
			BlockFace.Top => Vector3.up, 
			BlockFace.Bottom => Vector3.down, 
			BlockFace.North => Vector3.forward, 
			BlockFace.South => Vector3.back, 
			BlockFace.East => Vector3.right, 
			BlockFace.West => Vector3.left, 
			_ => Vector3.zero, 
		};
	}

	public static Vector3i OffsetIForFace(BlockFace face)
	{
		return face switch
		{
			BlockFace.Top => Vector3i.up, 
			BlockFace.Bottom => Vector3i.down, 
			BlockFace.North => Vector3i.forward, 
			BlockFace.South => Vector3i.back, 
			BlockFace.East => Vector3i.right, 
			BlockFace.West => Vector3i.left, 
			_ => Vector3i.zero, 
		};
	}

	public static BlockFaceFlag OppositeFaceFlag(BlockFace face)
	{
		return FromBlockFace(OppositeFace(face));
	}

	public static float YawForDirection(BlockFace face)
	{
		return face switch
		{
			BlockFace.South => 180f, 
			BlockFace.East => 90f, 
			BlockFace.West => 270f, 
			_ => 0f, 
		};
	}

	public static BlockFaceFlag FrontSidesFromPosition(Vector3i blockPos, Vector3 entityPos)
	{
		BlockFaceFlag blockFaceFlag = BlockFaceFlag.None;
		if (entityPos.x < (float)blockPos.x)
		{
			blockFaceFlag |= BlockFaceFlag.West;
		}
		if (entityPos.x >= (float)(blockPos.x + 1))
		{
			blockFaceFlag |= BlockFaceFlag.East;
		}
		if (entityPos.y < (float)blockPos.y)
		{
			blockFaceFlag |= BlockFaceFlag.Bottom;
		}
		if (entityPos.y >= (float)(blockPos.y + 1))
		{
			blockFaceFlag |= BlockFaceFlag.Top;
		}
		if (entityPos.z < (float)blockPos.z)
		{
			blockFaceFlag |= BlockFaceFlag.South;
		}
		if (entityPos.z >= (float)(blockPos.z + 1))
		{
			blockFaceFlag |= BlockFaceFlag.North;
		}
		return blockFaceFlag;
	}

	public static string SerializeFaceFlags(BlockFaceFlag coverFaceMask)
	{
		string text = string.Empty;
		bool flag = true;
		for (int i = 0; i < cubeSideFaceFlags.Length; i++)
		{
			if ((coverFaceMask & cubeSideFaceFlags[i]) != BlockFaceFlag.None)
			{
				string arg = (flag ? "" : ",");
				text += $"{arg}{cubeSideFaceChars[i]}";
				flag = false;
			}
		}
		return text;
	}
}
