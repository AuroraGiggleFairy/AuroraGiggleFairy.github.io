using System.Collections.Generic;

public class TileArea<T> : ITileArea<T> where T : class
{
	public TileAreaConfig config;

	public Dictionary<uint, T> Data = new Dictionary<uint, T>();

	public TileAreaConfig Config => config;

	public T this[int _tileX, int _tileZ]
	{
		get
		{
			config.checkCoordinates(ref _tileX, ref _tileZ);
			uint key = TileAreaUtils.MakeKey(_tileX, _tileZ);
			if (!Data.TryGetValue(key, out var value))
			{
				return null;
			}
			return value;
		}
		set
		{
			config.checkCoordinates(ref _tileX, ref _tileZ);
			uint key = TileAreaUtils.MakeKey(_tileX, _tileZ);
			Data[key] = value;
		}
	}

	public T this[uint _key]
	{
		get
		{
			if (!Data.TryGetValue(_key, out var value))
			{
				return null;
			}
			return value;
		}
		set
		{
			Data[_key] = value;
		}
	}

	public TileArea(TileAreaConfig _config, T[,] _data = null)
	{
		config = _config;
		if (_data == null)
		{
			return;
		}
		for (int i = 0; i < _data.GetLength(0); i++)
		{
			for (int j = 0; j < _data.GetLength(1); j++)
			{
				int tileX = i + config.tileStart.x;
				int tileZ = j + config.tileStart.y;
				uint key = TileAreaUtils.MakeKey(tileX, tileZ);
				Data[key] = _data[i, j];
			}
		}
	}

	public void Remove(uint _key)
	{
		Data.Remove(_key);
	}
}
