using System.Collections.Generic;

public class BlockTracker
{
	public int limit;

	public List<Vector3i> blockLocations;

	public int clientAmount;

	public BlockTracker(int _limit)
	{
		limit = _limit;
		blockLocations = new List<Vector3i>();
	}

	public bool TryAddBlock(Vector3i _position)
	{
		if (blockLocations.Contains(_position))
		{
			return true;
		}
		if (blockLocations.Count >= limit)
		{
			return false;
		}
		blockLocations.Add(_position);
		return true;
	}

	public bool RemoveBlock(Vector3i _position)
	{
		if (blockLocations.Contains(_position))
		{
			blockLocations.Remove(_position);
			return true;
		}
		return false;
	}

	public bool CanAdd(Vector3i _position)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (blockLocations.Count >= limit)
			{
				return blockLocations.Contains(_position);
			}
			return true;
		}
		return clientAmount < limit;
	}

	public void Clear()
	{
		blockLocations.Clear();
	}

	public void Read(PooledBinaryReader _reader)
	{
		blockLocations = new List<Vector3i>();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			blockLocations.Add(new Vector3i(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32()));
		}
	}

	public void Write(PooledBinaryWriter _writer)
	{
		_writer.Write(blockLocations.Count);
		foreach (Vector3i blockLocation in blockLocations)
		{
			_writer.Write(blockLocation.x);
			_writer.Write(blockLocation.y);
			_writer.Write(blockLocation.z);
		}
	}
}
