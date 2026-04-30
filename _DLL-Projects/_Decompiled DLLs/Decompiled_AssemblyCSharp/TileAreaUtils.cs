using System.Runtime.CompilerServices;

public static class TileAreaUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint MakeKey(int _tileX, int _tileZ)
	{
		return (uint)((_tileX << 16) | (_tileZ & 0xFFFF));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetTileXPos(uint _key)
	{
		return (int)_key >> 16;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetTileZPos(uint _key)
	{
		return (short)_key;
	}
}
