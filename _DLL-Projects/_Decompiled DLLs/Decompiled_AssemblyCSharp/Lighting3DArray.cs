public class Lighting3DArray : Array3DWithOffset<Lighting>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSize = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSize2d = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSize3d = 27;

	[PublicizedFrom(EAccessModifier.Private)]
	public int available;

	[PublicizedFrom(EAccessModifier.Private)]
	public INeighborBlockCache nBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte stab;

	public override Lighting this[int _x, int _y, int _z]
	{
		get
		{
			int index = GetIndex(_x, _y, _z);
			int num = 1 << index;
			if ((available & num) == 0)
			{
				data[index] = GetLight(nBlocks.GetChunk(_x, _z), blockPos.x + _x, blockPos.y + _y, blockPos.z + _z);
				available |= num;
			}
			return data[index];
		}
		set
		{
			int index = GetIndex(_x, _y, _z);
			data[index] = value;
			available |= 1 << index;
		}
	}

	public Lighting3DArray()
		: base(3, 3, 3)
	{
	}

	public void SetBlockCache(INeighborBlockCache _nBlocks)
	{
		nBlocks = _nBlocks;
	}

	public void SetPosition(Vector3i _blockPos)
	{
		stab = nBlocks.GetChunk(0, 0).GetStability(_blockPos.x, _blockPos.y, _blockPos.z);
		if (blockPos.x != _blockPos.x || blockPos.z != _blockPos.z || blockPos.y != _blockPos.y + 1)
		{
			available = 0;
		}
		else
		{
			int num = 26;
			for (int i = 0; i < 18; i++)
			{
				data[num] = data[num - 9];
				num--;
			}
			available <<= 9;
		}
		blockPos = _blockPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting GetLight(IChunk _c, int _x, int _y, int _z)
	{
		if (_c == null)
		{
			return Lighting.one;
		}
		_x &= 0xF;
		_z &= 0xF;
		return new Lighting(_c.GetLight(_x, _y, _z, Chunk.LIGHT_TYPE.SUN), 0, stab);
	}
}
