using System;
using System.IO;

public sealed class BlockChangeInfo
{
	[Flags]
	[PublicizedFrom(EAccessModifier.Private)]
	public enum Flags : byte
	{
		ChangeBlockValue = 1,
		ChangeDamage = 2,
		ChangeDensity = 4,
		ForceDensity = 8,
		UpdateLight = 0x10,
		ChangeTexture = 0x20
	}

	public static BlockChangeInfo Empty = new BlockChangeInfo
	{
		blockValueRef = BlockValueRef.None
	};

	public BlockValueRef blockValueRef;

	public bool bChangeBlockValue;

	public bool bChangeDamage;

	public BlockValue blockValue;

	public bool bChangeDensity;

	public bool bForceDensity;

	public sbyte density;

	public bool bUpdateLight;

	public bool bChangeTexture;

	public TextureFullArray textureFull;

	public int changedByEntityId = -1;

	public BlockChangeInfo()
	{
		blockValueRef = BlockValueRef.None;
		blockValue = BlockValue.Air;
		density = MarchingCubes.DensityAir;
		changedByEntityId = -1;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue)
	{
		blockValueRef = _bvRef;
		blockValue = _blockValue;
		bChangeBlockValue = true;
		bUpdateLight = false;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, bool _updateLight, bool _bOnlyDamage = false)
		: this(_bvRef, _blockValue, _updateLight)
	{
		bChangeDamage = _bOnlyDamage;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, int _changedEntityId)
		: this(_bvRef, _blockValue)
	{
		changedByEntityId = _changedEntityId;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, bool _updateLight)
		: this(_bvRef, _blockValue)
	{
		bUpdateLight = _updateLight;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, bool _updateLight, int _changingEntityId)
		: this(_bvRef, _blockValue, _updateLight)
	{
		changedByEntityId = _changingEntityId;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, sbyte _density, bool _bForceDensityChange = false)
	{
		blockValueRef = _bvRef;
		density = _density;
		bChangeDensity = true;
		bForceDensity = _bForceDensityChange;
		changedByEntityId = -1;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density)
	{
		blockValueRef = _bvRef;
		blockValue = _blockValue;
		bChangeBlockValue = true;
		density = _density;
		bChangeDensity = true;
		bUpdateLight = true;
		changedByEntityId = -1;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density, int _changedByEntityId)
		: this(_bvRef, _blockValue, _density)
	{
		changedByEntityId = _changedByEntityId;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density, TextureFullArray _tex)
		: this(_bvRef, _blockValue, _density)
	{
		bChangeTexture = true;
		textureFull = _tex;
		changedByEntityId = -1;
	}

	public void Read(BinaryReader _br)
	{
		blockValueRef = BlockValueRef.Read(_br);
		changedByEntityId = _br.ReadInt32();
		Flags flags = (Flags)_br.ReadByte();
		bChangeBlockValue = (flags & Flags.ChangeBlockValue) != 0;
		bChangeDensity = (flags & Flags.ChangeDensity) != 0;
		bForceDensity = (flags & Flags.ForceDensity) != 0;
		bUpdateLight = (flags & Flags.UpdateLight) != 0;
		bChangeDamage = (flags & Flags.ChangeDamage) != 0;
		bChangeTexture = (flags & Flags.ChangeTexture) != 0;
		if (bChangeBlockValue)
		{
			blockValue = BlockValue.Read(_br);
		}
		if (bChangeDensity)
		{
			density = _br.ReadSByte();
		}
		if (bChangeTexture)
		{
			textureFull.Read(_br);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		blockValueRef.Write(_bw);
		_bw.Write(changedByEntityId);
		Flags flags = (Flags)0;
		flags = (Flags)((uint)flags | (uint)(bChangeBlockValue ? 1 : 0));
		flags = (Flags)((uint)flags | (uint)(bChangeDensity ? 4 : 0));
		flags = (Flags)((uint)flags | (uint)(bForceDensity ? 8 : 0));
		flags = (Flags)((uint)flags | (uint)(bUpdateLight ? 16 : 0));
		flags = (Flags)((uint)flags | (uint)(bChangeDamage ? 2 : 0));
		flags = (Flags)((uint)flags | (uint)(bChangeTexture ? 32 : 0));
		_bw.Write((byte)flags);
		if (bChangeBlockValue)
		{
			blockValue.Write(_bw);
		}
		if (bChangeDensity)
		{
			_bw.Write(density);
		}
		if (bChangeTexture)
		{
			textureFull.Write(_bw);
		}
	}
}
