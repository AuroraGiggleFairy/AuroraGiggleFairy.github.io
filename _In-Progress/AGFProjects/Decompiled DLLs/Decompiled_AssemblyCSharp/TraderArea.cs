using System.Collections.Generic;
using UnityEngine;

public class TraderArea
{
	public Vector3i Position;

	public Vector3i PrefabSize;

	public Vector3i ProtectPosition;

	public Vector3i ProtectSize;

	public BoundsInt ProtectBounds;

	public bool IsClosed = true;

	public List<Prefab.PrefabTeleportVolume> TeleportVolumes;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPadXZ = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityTrader owningTrader;

	public bool IsInitialized => owningTrader != null;

	public TraderArea(Vector3i _pos, Vector3i _size, Vector3i _protectPadding, List<Prefab.PrefabTeleportVolume> _teleportVolumes)
	{
		Position = _pos;
		PrefabSize = _size;
		ProtectSize = _size + _protectPadding;
		ProtectSize.x += 2;
		ProtectSize.z += 2;
		ProtectPosition = _pos + _size / 2 - ProtectSize / 2;
		ProtectBounds = new BoundsInt(ProtectPosition, ProtectSize);
		TeleportVolumes = _teleportVolumes;
	}

	public Vector3i GetProtectPadding()
	{
		Vector3i result = ProtectSize - PrefabSize;
		result.x -= 2;
		result.z -= 2;
		return result;
	}

	public bool IsWithinProtectArea(Vector3 _pos)
	{
		if ((float)ProtectBounds.xMin <= _pos.x && _pos.x <= (float)ProtectBounds.xMax && (float)ProtectBounds.yMin <= _pos.y && _pos.y <= (float)ProtectBounds.yMax && (float)ProtectBounds.zMin <= _pos.z)
		{
			return _pos.z <= (float)ProtectBounds.zMax;
		}
		return false;
	}

	public bool IsWithinTeleportArea(Vector3 _pos, out Prefab.PrefabTeleportVolume tpVolume)
	{
		foreach (Prefab.PrefabTeleportVolume teleportVolume in TeleportVolumes)
		{
			Vector3i vector3i = Position + teleportVolume.startPos;
			if ((float)vector3i.x <= _pos.x && _pos.x <= (float)(vector3i.x + teleportVolume.size.x) && (float)vector3i.y <= _pos.y && _pos.y <= (float)(vector3i.y + teleportVolume.size.y) && (float)vector3i.z <= _pos.z && _pos.z <= (float)(vector3i.z + teleportVolume.size.z))
			{
				tpVolume = teleportVolume;
				return true;
			}
		}
		tpVolume = null;
		return false;
	}

	public bool SetClosed(World _world, bool _bClosed, EntityTrader _trader, bool playSound = false)
	{
		owningTrader = _trader;
		IsClosed = _bClosed;
		int num = World.toChunkXZ(Position.x - 1);
		int num2 = World.toChunkXZ(Position.x + PrefabSize.x + 1);
		int num3 = World.toChunkXZ(Position.z - 1);
		int num4 = World.toChunkXZ(Position.z + PrefabSize.z + 1);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				if (!(_world.GetChunkSync(j, i) is Chunk))
				{
					return false;
				}
			}
		}
		for (int k = num3; k <= num4; k++)
		{
			for (int l = num; l <= num2; l++)
			{
				Chunk chunk = _world.GetChunkSync(l, k) as Chunk;
				List<Vector3i> list = chunk.IndexedBlocks["TraderOnOff"];
				if (list == null)
				{
					continue;
				}
				for (int m = 0; m < list.Count; m++)
				{
					BlockValue block = chunk.GetBlock(list[m]);
					if (block.ischild)
					{
						continue;
					}
					Vector3i vector3i = chunk.ToWorldPos(list[m]);
					if (!ProtectBounds.Contains(vector3i))
					{
						continue;
					}
					Block block2 = block.Block;
					if (block2 is BlockDoor)
					{
						if (_bClosed && BlockDoor.IsDoorOpen(block.meta))
						{
							block2.OnBlockActivated(_world, 0, vector3i, block, null);
						}
						if (!(block2 is BlockDoorSecure blockDoorSecure))
						{
							continue;
						}
						if (_bClosed)
						{
							if (!blockDoorSecure.IsDoorLocked(_world, vector3i))
							{
								block2.OnBlockActivated("lock", _world, 0, vector3i, block, null);
							}
						}
						else if (blockDoorSecure.IsDoorLocked(_world, vector3i))
						{
							block2.OnBlockActivated("unlock", _world, 0, vector3i, block, null);
						}
					}
					else if (block2 is BlockLight)
					{
						block.meta = (byte)((!_bClosed) ? (block.meta | 2) : (block.meta & -3));
						_world.SetBlockRPC(vector3i, block);
					}
					else if (block2 is BlockSpeakerTrader && playSound)
					{
						BlockSpeakerTrader blockSpeakerTrader = block2 as BlockSpeakerTrader;
						if (_bClosed)
						{
							blockSpeakerTrader.PlayClose(vector3i, _trader);
						}
						else
						{
							blockSpeakerTrader.PlayOpen(vector3i, _trader);
						}
					}
				}
			}
		}
		return true;
	}

	public void HandleWarning(World _world, EntityTrader _trader)
	{
		int num = World.toChunkXZ(Position.x - 1);
		int num2 = World.toChunkXZ(Position.x + PrefabSize.x + 1);
		int num3 = World.toChunkXZ(Position.z - 1);
		int num4 = World.toChunkXZ(Position.z + PrefabSize.z + 1);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!(_world.GetChunkSync(i, j) is Chunk chunk))
				{
					continue;
				}
				List<Vector3i> list = chunk.IndexedBlocks["TraderOnOff"];
				if (list == null)
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					BlockValue block = chunk.GetBlock(list[k]);
					if (!block.ischild && block.Block is BlockSpeakerTrader blockSpeakerTrader)
					{
						Vector3i blockPos = chunk.ToWorldPos(list[k]);
						blockSpeakerTrader.PlayWarning(blockPos, _trader);
					}
				}
			}
		}
	}

	public bool Overlaps(Vector3i _min, Vector3i _max)
	{
		if (_max.x < ProtectPosition.x || _min.x >= ProtectPosition.x + ProtectSize.x)
		{
			return false;
		}
		if (_max.z < ProtectPosition.z || _min.z >= ProtectPosition.z + ProtectSize.z)
		{
			return false;
		}
		return true;
	}

	public int GetReadWriteSize()
	{
		return 21 + (1 + TeleportVolumes.Count * 6);
	}

	public static TraderArea Read(PooledBinaryReader _reader)
	{
		Vector3i pos = default(Vector3i);
		pos.x = _reader.ReadInt32();
		pos.y = _reader.ReadInt32();
		pos.z = _reader.ReadInt32();
		Vector3i size = default(Vector3i);
		size.x = _reader.ReadInt16();
		size.y = _reader.ReadInt16();
		size.z = _reader.ReadInt16();
		Vector3i protectPadding = default(Vector3i);
		protectPadding.x = _reader.ReadSByte();
		protectPadding.y = _reader.ReadSByte();
		protectPadding.z = _reader.ReadSByte();
		int num = _reader.ReadByte();
		List<Prefab.PrefabTeleportVolume> list = new List<Prefab.PrefabTeleportVolume>();
		for (int i = 0; i < num; i++)
		{
			Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume();
			prefabTeleportVolume.startPos.x = _reader.ReadSByte();
			prefabTeleportVolume.startPos.y = _reader.ReadSByte();
			prefabTeleportVolume.startPos.z = _reader.ReadSByte();
			prefabTeleportVolume.size.x = _reader.ReadByte();
			prefabTeleportVolume.size.y = _reader.ReadByte();
			prefabTeleportVolume.size.z = _reader.ReadByte();
			list.Add(prefabTeleportVolume);
		}
		return new TraderArea(pos, size, protectPadding, list);
	}

	public void Write(PooledBinaryWriter _writer)
	{
		_writer.Write(Position.x);
		_writer.Write(Position.y);
		_writer.Write(Position.z);
		_writer.Write((short)PrefabSize.x);
		_writer.Write((short)PrefabSize.y);
		_writer.Write((short)PrefabSize.z);
		Vector3i protectPadding = GetProtectPadding();
		_writer.Write((sbyte)protectPadding.x);
		_writer.Write((sbyte)protectPadding.y);
		_writer.Write((sbyte)protectPadding.z);
		_writer.Write((byte)TeleportVolumes.Count);
		for (int i = 0; i < TeleportVolumes.Count; i++)
		{
			Prefab.PrefabTeleportVolume prefabTeleportVolume = TeleportVolumes[i];
			_writer.Write((sbyte)prefabTeleportVolume.startPos.x);
			_writer.Write((sbyte)prefabTeleportVolume.startPos.y);
			_writer.Write((sbyte)prefabTeleportVolume.startPos.z);
			_writer.Write((byte)prefabTeleportVolume.size.x);
			_writer.Write((byte)prefabTeleportVolume.size.y);
			_writer.Write((byte)prefabTeleportVolume.size.z);
		}
	}
}
