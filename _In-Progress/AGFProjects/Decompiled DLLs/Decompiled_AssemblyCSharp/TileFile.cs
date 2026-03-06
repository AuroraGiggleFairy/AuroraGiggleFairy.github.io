using System;

public class TileFile<T> : IDisposable where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedArray<T> fba;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int tileWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int tileCountWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int tileCountHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int dataLength;

	public TileFile(FileBackedArray<T> _fileBackedArray, int _tileWidth, int _tileCountWidth, int _tileCountHeight)
	{
		fba = _fileBackedArray;
		tileWidth = _tileWidth;
		tileCountWidth = _tileCountWidth;
		tileCountHeight = _tileCountHeight;
		dataLength = tileWidth * tileWidth;
	}

	public bool IsInDatabase(int _tileX, int _tileZ)
	{
		if (_tileX >= 0 && _tileX < tileCountWidth && _tileZ >= 0)
		{
			return _tileZ < tileCountHeight;
		}
		return false;
	}

	public unsafe void LoadTile(int _tileX, int _tileZ, ref T[,] _tile)
	{
		if (_tile == null)
		{
			_tile = new T[tileWidth, tileWidth];
		}
		int start = _tileZ * tileCountWidth * dataLength + _tileX * dataLength;
		ReadOnlySpan<T> span;
		using (fba.GetReadOnlySpan(start, dataLength, out span))
		{
			fixed (T* pointer = _tile)
			{
				Span<T> destination = new Span<T>(pointer, dataLength);
				span.CopyTo(destination);
			}
		}
	}

	public void Dispose()
	{
		fba.Dispose();
	}
}
