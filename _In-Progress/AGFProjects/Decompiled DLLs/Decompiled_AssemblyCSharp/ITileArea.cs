public interface ITileArea<T> where T : class
{
	TileAreaConfig Config { get; }

	T this[int _tileX, int _tileZ] { get; }

	T this[uint _key] { get; }

	void Cleanup()
	{
	}
}
