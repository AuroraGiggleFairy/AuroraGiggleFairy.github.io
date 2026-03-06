using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageMapChunks : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ushort[]> mapPieces;

	public override int Channel => 1;

	public override bool Compress => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageMapChunks Setup(int _entityId, List<int> _chunks, List<ushort[]> _mapPieces)
	{
		entityId = _entityId;
		chunks = new List<int>(_chunks);
		mapPieces = new List<ushort[]>(_mapPieces);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		chunks = new List<int>();
		mapPieces = new List<ushort[]>();
		entityId = _reader.ReadInt32();
		int num = _reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			chunks.Add(_reader.ReadInt32());
			ushort[] array = new ushort[256];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = _reader.ReadUInt16();
			}
			mapPieces.Add(array);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		bool flag = true;
		ushort num = (ushort)chunks.Count;
		long position = _writer.BaseStream.Position;
		_writer.Write(num);
		for (int i = 0; i < chunks.Count; i++)
		{
			ushort[] array = mapPieces[i];
			if (array.Length != 256)
			{
				Log.Warning("Player map data for entityid {0} of invalid size {1}", entityId, array.Length);
				num--;
				flag = false;
			}
			else
			{
				_writer.Write(chunks[i]);
				for (int j = 0; j < array.Length; j++)
				{
					_writer.Write(array[j]);
				}
			}
		}
		if (!flag)
		{
			long position2 = _writer.BaseStream.Position;
			_writer.BaseStream.Position = position;
			_writer.Write(num);
			_writer.BaseStream.Position = position2;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityPlayer entityPlayer = _world.GetEntity(entityId) as EntityPlayer;
			if (entityPlayer != null && entityPlayer.ChunkObserver.mapDatabase != null)
			{
				entityPlayer.ChunkObserver.mapDatabase.Add(chunks, mapPieces);
			}
		}
	}

	public override int GetLength()
	{
		return 4 + ((chunks != null) ? (chunks.Count * 8) : 0) + ((mapPieces != null) ? (mapPieces.Count * 512) : 0);
	}
}
