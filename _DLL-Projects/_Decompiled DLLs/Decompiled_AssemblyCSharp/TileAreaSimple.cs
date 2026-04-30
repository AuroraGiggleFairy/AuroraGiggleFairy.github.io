using System;

public class TileAreaSimple<T> : ITileArea<T> where T : class
{
	[PublicizedFrom(EAccessModifier.Private)]
	public T[,] data;

	public T this[uint _key] => data[0, 0];

	public T this[int _tileX, int _tileZ] => data[0, 0];

	public TileAreaConfig Config
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public TileAreaSimple(T[,] _data = null)
	{
		data = _data;
	}
}
