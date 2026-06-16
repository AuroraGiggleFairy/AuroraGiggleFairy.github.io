using UnityEngine;

namespace WorldGenerationEngineFinal;

public class POISmoother
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public POISmoother(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public void SmoothStreetTiles()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		int streetTileMapWidth = worldBuilder.StreetTileMapWidth;
		int streetTileMapWidth2 = worldBuilder.StreetTileMapWidth;
		int num = 0;
		int num2 = streetTileMapWidth * streetTileMapWidth2;
		for (int i = 0; i < streetTileMapWidth2; i++)
		{
			for (int j = 0; j < streetTileMapWidth; j++)
			{
				num++;
				StreetTile obj = worldBuilder.StreetTileMap[j + i * streetTileMapWidth];
				obj.SmoothTownshipTerrain();
				obj.UpdateValidity();
			}
			worldBuilder.SetTaskMessage(string.Format(worldBuilder.messageSmoothingStreetTiles, Mathf.RoundToInt((float)num / (float)num2 * 100f)));
		}
		Log.Out("POISmoother SmoothStreetTiles in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}
}
