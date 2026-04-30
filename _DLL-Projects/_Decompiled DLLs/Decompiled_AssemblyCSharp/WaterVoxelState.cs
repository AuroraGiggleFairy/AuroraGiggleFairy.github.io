using System;
using Unity.Mathematics;

public struct WaterVoxelState : IEquatable<WaterVoxelState>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte stateBits;

	public WaterVoxelState(byte stateBits)
	{
		this.stateBits = stateBits;
	}

	public WaterVoxelState(WaterVoxelState other)
	{
		stateBits = other.stateBits;
	}

	public bool IsDefault()
	{
		return stateBits == 0;
	}

	public bool IsSolidYPos()
	{
		return (stateBits & 1) != 0;
	}

	public bool IsSolidYNeg()
	{
		return (stateBits & 2) != 0;
	}

	public bool IsSolidXPos()
	{
		return (stateBits & 0x20) != 0;
	}

	public bool IsSolidXNeg()
	{
		return (stateBits & 8) != 0;
	}

	public bool IsSolidZPos()
	{
		return (stateBits & 4) != 0;
	}

	public bool IsSolidZNeg()
	{
		return (stateBits & 0x10) != 0;
	}

	public bool IsSolidXZ(int2 side)
	{
		if (side.x > 0)
		{
			return IsSolidXPos();
		}
		if (side.x < 0)
		{
			return IsSolidXNeg();
		}
		if (side.y > 0)
		{
			return IsSolidZPos();
		}
		if (side.y < 0)
		{
			return IsSolidZNeg();
		}
		return IsSolid();
	}

	public bool IsSolid()
	{
		if (stateBits != 0)
		{
			return (~stateBits & 0x3F) == 0;
		}
		return false;
	}

	public void SetSolid(BlockFaceFlag flags)
	{
		stateBits = (byte)flags;
	}

	public void SetSolidMask(BlockFaceFlag mask, bool value)
	{
		if (value)
		{
			stateBits |= (byte)mask;
		}
		else
		{
			stateBits &= (byte)(~mask);
		}
	}

	public bool Equals(WaterVoxelState other)
	{
		return stateBits == other.stateBits;
	}
}
