using System.IO;

public class WorldBlockTickerEntry
{
	public readonly Vector3i worldPos;

	public readonly int blockID;

	public readonly ulong scheduledTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long nextTickEntryID;

	public readonly long tickEntryID;

	public WorldBlockTickerEntry(Vector3i _pos, int _id, ulong _scheduledTime)
	{
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
		_br.ReadUInt16();
		return new WorldBlockTickerEntry(pos, id, num);
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)World.toBlockXZ(worldPos.x));
		_bw.Write((byte)worldPos.y);
		_bw.Write((byte)World.toBlockXZ(worldPos.z));
		_bw.Write((ushort)blockID);
		_bw.Write(scheduledTime);
		_bw.Write((ushort)0);
	}

	public override bool Equals(object _obj)
	{
		if (_obj is WorldBlockTickerEntry worldBlockTickerEntry)
		{
			if (worldPos.Equals(worldBlockTickerEntry.worldPos) && blockID == worldBlockTickerEntry.blockID)
			{
				return worldBlockTickerEntry.tickEntryID == tickEntryID;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToHashCode(worldPos, blockID);
	}

	public static int ToHashCode(Vector3i _pos, int _blockID)
	{
		return (_pos.GetHashCode() * 397) ^ _blockID;
	}

	public long GetChunkKey()
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
	}
}
