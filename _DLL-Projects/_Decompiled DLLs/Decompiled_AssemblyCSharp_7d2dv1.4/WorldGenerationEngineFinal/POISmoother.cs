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
		float width = worldBuilder.StreetTileMap.GetLength(0);
		float height = worldBuilder.StreetTileMap.GetLength(1);
		float current = 0f;
		float total = width * height;
		for (int x = 0; (float)x < width; x++)
		{
			for (int i = 0; (float)i < height; i++)
			{
				current += 1f;
				worldBuilder.StreetTileMap[x, i].SmoothTerrainPost();
				worldBuilder.StreetTileMap[x, i].UpdateValidity();
			}
			if (worldBuilder.IsMessageElapsed())
			{
				yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgSmoothingStreetTiles"), Mathf.RoundToInt(current / total * 100f)));
			}
		}
		Log.Out("POISmoother SmoothStreetTiles in {0}", (float)ms.ElapsedMilliseconds * 0.001f);
	}
}
