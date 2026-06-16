using UnityEngine;

namespace MapRendering;

public static class Constants
{
	public static readonly TextureFormat DefaultTextureFormat = TextureFormat.ARGB32;

	public static int MapBlockSize = 128;

	public const int MapChunkSize = 16;

	public const int MapRegionSize = 512;

	public static int Zoomlevels = 5;

	public static string MapDirectory = string.Empty;

	public static int MAP_BLOCK_TO_CHUNK_DIV => MapBlockSize / 16;

	public static int MAP_REGION_TO_CHUNK_DIV => 32;
}
