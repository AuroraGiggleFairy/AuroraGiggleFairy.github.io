using System.Collections.Generic;

public class TileAreaCache<T> : ITileArea<T[,]> where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileFile<T> tilesDatabase;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileAreaConfig config;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, T[,]> cache = new Dictionary<uint, T[,]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<uint> cacheQueue = new LinkedList<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheMax;

	public TileAreaConfig Config => config;

	public T[,] this[uint _key]
	{
		get
		{
			if (cache.TryGetValue(_key, out var value))
			{
				PromoteEntry(_key);
				return value;
			}
			int tileXPos = TileAreaUtils.GetTileXPos(_key);
			int tileZPos = TileAreaUtils.GetTileZPos(_key);
			return Cache(_key, tileXPos, tileZPos);
		}
	}

	public T[,] this[int _tileX, int _tileZ]
	{
		get
		{
			config.checkCoordinates(ref _tileX, ref _tileZ);
			uint key = TileAreaUtils.MakeKey(_tileX, _tileZ);
			if (cache.TryGetValue(key, out var value))
			{
				PromoteEntry(key);
				return value;
			}
			return Cache(key, _tileX, _tileZ);
		}
	}

	public TileAreaCache(TileAreaConfig _config, TileFile<T> _tileFile, int _cacheMax)
	{
		tilesDatabase = _tileFile;
		config = _config;
		cacheMax = _cacheMax;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PromoteEntry(uint _key)
	{
		for (LinkedListNode<uint> linkedListNode = cacheQueue.First; linkedListNode != cacheQueue.Last; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value == _key)
			{
				cacheQueue.Remove(linkedListNode);
				cacheQueue.AddFirst(linkedListNode);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T[,] Cache(uint _key, int _tileX, int _tileZ)
	{
		int tileX = _tileX - config.tileStart.x;
		int tileZ = _tileZ - config.tileStart.y;
		if (!tilesDatabase.IsInDatabase(tileX, tileZ))
		{
			return null;
		}
		LinkedListNode<uint> linkedListNode = null;
		T[,] _tile = null;
		if (cacheQueue.Count >= cacheMax)
		{
			linkedListNode = cacheQueue.Last;
			cacheQueue.Remove(linkedListNode);
			_tile = cache[linkedListNode.Value];
			cache.Remove(linkedListNode.Value);
		}
		tilesDatabase.LoadTile(tileX, tileZ, ref _tile);
		cache.Add(_key, _tile);
		if (linkedListNode != null)
		{
			linkedListNode.Value = _key;
			cacheQueue.AddFirst(linkedListNode);
		}
		else
		{
			cacheQueue.AddFirst(_key);
		}
		return _tile;
	}

	public void Cleanup()
	{
		if (tilesDatabase != null)
		{
			tilesDatabase.Dispose();
			tilesDatabase = null;
		}
	}
}
