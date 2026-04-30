using System.IO;

public class WorldBlockTickerEntry
{
	public readonly Vector3i worldPos;

	public readonly int blockID;

	public readonly ulong scheduledTime;

	public readonly int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long nextTickEntryID;

	public readonly long tickEntryID;

	public WorldBlockTickerEntry(int _clrIdx, Vector3i _pos, int _id, ulong _scheduledTime)
	{
		clrIdx = _clrIdx;
		tickEntryID = nextTickEntryID++;
		worldPos = _pos;
		blockID = _id;
		scheduledTime = _scheduledTime;
	}

	public static WorldBlockTickerEntry Read(BinaryReader _br, int _chunkX, int _chunkZ, int _version)
	{
		Vector3i pos = new Vector3i(_br.ReadByte() + _chunkX * 16, _br.ReadByte(), _br.ReadByte() + _chunkZ * 16);
		int id = _br.ReadUInt16();
		ulong num = _br.ReadUInt64();
		return new WorldBlockTickerEntry(_br.ReadUInt16(), pos, id, num);
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)World.toBlockXZ(worldPos.x));
		_bw.Write((byte)worldPos.y);
		_bw.Write((byte)World.toBlockXZ(worldPos.z));
		_bw.Write((ushort)blockID);
		_bw.Write(scheduledTime);
		_bw.Write((ushort)clrIdx);
	}

	public override bool Equals(object _obj)
	{
		if (_obj is WorldBlockTickerEntry worldBlockTickerEntry)
		{
			if (worldPos.Equals(worldBlockTickerEntry.worldPos) && blockID == worldBlockTickerEntry.blockID && clrIdx == worldBlockTickerEntry.clrIdx)
			{
				return worldBlockTickerEntry.tickEntryID == tickEntryID;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToHashCode(clrIdx, worldPos, blockID);
	}

	public static int ToHashCode(int _clrIdx, Vector3i _pos, int _blockID)
	{
		return (((_pos.GetHashCode() * 397) ^ _blockID) * 397) ^ _clrIdx;
	}

	public long GetChunkKey()
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z), clrIdx);
	}
}
