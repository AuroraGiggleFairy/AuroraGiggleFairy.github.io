using System.Collections.Generic;
using UnityEngine;

public class SpawnPointList : List<SpawnPoint>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static uint CurrentSaveVersion = 2u;

	public SpawnPoint Find(Vector3i _blockPos)
	{
		for (int i = 0; i < base.Count; i++)
		{
			SpawnPoint spawnPoint = base[i];
			if (spawnPoint.spawnPosition.ToBlockPos().Equals(_blockPos))
			{
				return spawnPoint;
			}
		}
		return null;
	}

	public virtual SpawnPosition GetRandomSpawnPosition(World _world, Vector3? _refPosition = null, int _minDistance = 0, int _maxDistance = 0)
	{
		if (base.Count > 0)
		{
			GameRandom gameRandom = _world.GetGameRandom();
			if (!_refPosition.HasValue)
			{
				return base[gameRandom.RandomRange(base.Count)].spawnPosition;
			}
			Vector3 value = _refPosition.Value;
			for (int i = 0; i < 100; i++)
			{
				int index = gameRandom.RandomRange(base.Count);
				SpawnPosition spawnPosition = base[index].spawnPosition;
				float magnitude = (spawnPosition.position - value).magnitude;
				if (magnitude >= (float)_minDistance && magnitude <= (float)_maxDistance)
				{
					return spawnPosition;
				}
			}
			float num = float.MaxValue;
			int num2 = -1;
			for (int j = 0; j < base.Count; j++)
			{
				float sqrMagnitude = (base[j].spawnPosition.position - value).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = j;
				}
			}
			if (num2 != -1)
			{
				return base[num2].spawnPosition;
			}
		}
		return SpawnPosition.Undef;
	}

	public void Read(IBinaryReaderOrWriter _readerOrWriter)
	{
		if (_readerOrWriter != null)
		{
			Clear();
			uint version = _readerOrWriter.ReadWrite((byte)0);
			int num = _readerOrWriter.ReadWrite(0);
			for (int i = 0; i < num; i++)
			{
				SpawnPoint spawnPoint = new SpawnPoint();
				spawnPoint.Read(_readerOrWriter, version);
				Add(spawnPoint);
			}
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		if (_br != null)
		{
			Clear();
			uint version = _br.ReadByte();
			int num = _br.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				SpawnPoint spawnPoint = new SpawnPoint();
				spawnPoint.Read(_br, version);
				Add(spawnPoint);
			}
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write((byte)CurrentSaveVersion);
		_bw.Write(base.Count);
		for (int i = 0; i < base.Count; i++)
		{
			base[i].Write(_bw);
		}
	}
}
