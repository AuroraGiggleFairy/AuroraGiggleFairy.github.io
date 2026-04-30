using System.IO;

public class BlockChangeInfo
{
	public static BlockChangeInfo Empty = new BlockChangeInfo();

	public int clrIdx;

	public Vector3i pos;

	public bool bChangeBlockValue;

	public bool bChangeDamage;

	public BlockValue blockValue;

	public bool bChangeDensity;

	public bool bForceDensityChange;

	public sbyte density;

	public bool bUpdateLight;

	public bool bChangeTexture;

	public TextureFullArray textureFull;

	public int changedByEntityId = -1;

	public BlockChangeInfo()
	{
		pos = Vector3i.zero;
		blockValue = BlockValue.Air;
		density = MarchingCubes.DensityAir;
		changedByEntityId = -1;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _pos, BlockValue _blockValue)
	{
		pos = _pos;
		blockValue = _blockValue;
		bChangeBlockValue = true;
		bUpdateLight = false;
		clrIdx = _clrIdx;
	}

	public BlockChangeInfo(Vector3i _blockPos, BlockValue _blockValue, bool _updateLight, bool _bOnlyDamage = false)
		: this(0, _blockPos, _blockValue, _updateLight)
	{
		bChangeDamage = _bOnlyDamage;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _pos, BlockValue _blockValue, int _changedEntityId)
		: this(_clrIdx, _pos, _blockValue)
	{
		changedByEntityId = _changedEntityId;
	}

	public BlockChangeInfo(Vector3i _blockPos, BlockValue _blockValue, sbyte _density)
		: this(0, _blockPos, _blockValue, _density)
	{
	}

	public BlockChangeInfo(int _x, int _y, int _z, BlockValue _blockValue, bool _updateLight)
		: this(0, new Vector3i(_x, _y, _z), _blockValue, _updateLight)
	{
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _updateLight)
		: this(_clrIdx, _blockPos, _blockValue)
	{
		bUpdateLight = _updateLight;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _updateLight, int _changingEntityId)
		: this(_clrIdx, _blockPos, _blockValue, _updateLight)
	{
		changedByEntityId = _changingEntityId;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, sbyte _density, bool _bForceDensityChange = false)
	{
		clrIdx = _clrIdx;
		pos = _blockPos;
		density = _density;
		bChangeDensity = true;
		bForceDensityChange = _bForceDensityChange;
		changedByEntityId = -1;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density)
	{
		pos = _blockPos;
		blockValue = _blockValue;
		bChangeBlockValue = true;
		density = _density;
		bChangeDensity = true;
		bUpdateLight = true;
		clrIdx = _clrIdx;
		changedByEntityId = -1;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density, int _changedByEntityId)
		: this(_clrIdx, _blockPos, _blockValue, _density)
	{
		changedByEntityId = _changedByEntityId;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density, TextureFullArray _tex)
		: this(_clrIdx, _blockPos, _blockValue, _density)
	{
		bChangeTexture = true;
		textureFull = _tex;
		changedByEntityId = -1;
	}

	public override bool Equals(object other)
	{
		if (!(other is BlockChangeInfo blockChangeInfo))
		{
			return false;
		}
		if (pos.Equals(blockChangeInfo.pos) && blockValue.type == blockChangeInfo.blockValue.type)
		{
			return density == blockChangeInfo.density;
		}
		return false;
	}

	public void Read(BinaryReader _br)
	{
		clrIdx = _br.ReadByte();
		pos = StreamUtils.ReadVector3i(_br);
		changedByEntityId = _br.ReadInt32();
		int num = _br.ReadByte();
		bChangeBlockValue = (num & 1) != 0;
		bChangeDensity = (num & 2) != 0;
		bUpdateLight = (num & 4) != 0;
		bChangeDamage = (num & 8) != 0;
		bChangeTexture = (num & 0x10) != 0;
		if (bChangeBlockValue)
		{
			blockValue.rawData = _br.ReadUInt32();
			blockValue.damage = _br.ReadUInt16();
		}
		if (bChangeDensity)
		{
			density = _br.ReadSByte();
			bForceDensityChange = _br.ReadBoolean();
		}
		if (bChangeTexture)
		{
			textureFull.Read(_br);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)clrIdx);
		StreamUtils.Write(_bw, pos);
		_bw.Write(changedByEntityId);
		int num = (bChangeBlockValue ? 1 : 0);
		num |= (bChangeDensity ? 2 : 0);
		num |= (bUpdateLight ? 4 : 0);
		num |= (bChangeDamage ? 8 : 0);
		num |= (bChangeTexture ? 16 : 0);
		_bw.Write((byte)num);
		if (bChangeBlockValue)
		{
			_bw.Write(blockValue.rawData);
			_bw.Write((ushort)blockValue.damage);
		}
		if (bChangeDensity)
		{
			_bw.Write(density);
			_bw.Write(bForceDensityChange);
		}
		if (bChangeTexture)
		{
			textureFull.Write(_bw);
		}
	}

	public override int GetHashCode()
	{
		return pos.GetHashCode();
	}

	public static bool operator ==(BlockChangeInfo point1, BlockChangeInfo point2)
	{
		if ((object)point1 != null || (object)point2 != null)
		{
			return point1.Equals(point2);
		}
		return true;
	}

	public static bool operator !=(BlockChangeInfo point1, BlockChangeInfo point2)
	{
		return !(point1 == point2);
	}
}
