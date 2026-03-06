using UnityEngine.Scripting;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine;

[MovedFrom("UnityEngine.Experimental.TerrainAPI")]
public static class TerrainCallbacks
{
	public delegate void HeightmapChangedCallback(Terrain terrain, RectInt heightRegion, bool synched);

	public delegate void TextureChangedCallback(Terrain terrain, string textureName, RectInt texelRegion, bool synched);

	public static event HeightmapChangedCallback heightmapChanged;

	public static event TextureChangedCallback textureChanged;

	[RequiredByNativeCode]
	internal static void InvokeHeightmapChangedCallback(TerrainData terrainData, RectInt heightRegion, bool synched)
	{
		if (TerrainCallbacks.heightmapChanged != null)
		{
			Terrain[] users = terrainData.users;
			foreach (Terrain terrain in users)
			{
				TerrainCallbacks.heightmapChanged(terrain, heightRegion, synched);
			}
		}
	}

	[RequiredByNativeCode]
	internal static void InvokeTextureChangedCallback(TerrainData terrainData, string textureName, RectInt texelRegion, bool synched)
	{
		if (TerrainCallbacks.textureChanged != null)
		{
			Terrain[] users = terrainData.users;
			foreach (Terrain terrain in users)
			{
				TerrainCallbacks.textureChanged(terrain, textureName, texelRegion, synched);
			}
		}
	}
}
