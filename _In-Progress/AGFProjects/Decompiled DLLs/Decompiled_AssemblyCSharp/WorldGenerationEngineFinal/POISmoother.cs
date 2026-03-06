using System.Collections;
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

	public IEnumerator SmoothStreetTiles()
	{
		yield return null;
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		int width = worldBuilder.StreetTileMapWidth;
		int height = worldBuilder.StreetTileMapWidth;
		int current = 0;
		int total = width * height;
		for (int y = 0; y < height; y++)
		{
			for (int i = 0; i < width; i++)
			{
				current++;
				StreetTile obj = worldBuilder.StreetTileMap[i + y * width];
				obj.SmoothTownshipTerrain();
				obj.UpdateValidity();
			}
			if (worldBuilder.IsMessageElapsed())
			{
				yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgSmoothingStreetTiles"), Mathf.RoundToInt((float)current / (float)total * 100f)));
			}
		}
		Log.Out("POISmoother SmoothStreetTiles in {0}", (float)ms.ElapsedMilliseconds * 0.001f);
	}
}
