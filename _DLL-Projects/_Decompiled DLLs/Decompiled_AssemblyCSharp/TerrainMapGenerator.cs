using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainMapGenerator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int xs;

	[PublicizedFrom(EAccessModifier.Private)]
	public int zs;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int w = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int h = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] colArray = new Color[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] normalArray = new Color[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] heights = new int[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] densitySub = new float[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] biomeIntens = new float[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues = new BlockValue[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition[] biomeDefs = new BiomeDefinition[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] subbiome = new int[1050625];

	public void GenerateTerrain(IChunkProvider _chunkProvider)
	{
		World world = GameManager.Instance.World;
		xs = world.GetPrimaryPlayer().GetBlockPosition().x - 512;
		zs = world.GetPrimaryPlayer().GetBlockPosition().z - 512;
		MicroStopwatch microStopwatch = new MicroStopwatch();
		long elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		int num = 1;
		HashSetLong hashSetLong = new HashSetLong();
		for (int i = 0; i < 1025; i++)
		{
			for (int j = 0; j < 1025; j++)
			{
				int num2 = (xs + i) * num;
				int num3 = (zs + j) * num;
				long item = WorldChunkCache.MakeChunkKey(num2 / 16, num3 / 16);
				if (!hashSetLong.Contains(item))
				{
					hashSetLong.Add(item);
					_chunkProvider.RequestChunk(num2 / 16, num3 / 16);
				}
			}
		}
		Log.Out("Request chunks: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds));
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		bool flag = false;
		do
		{
			flag = false;
			foreach (long item2 in hashSetLong)
			{
				if (GameManager.Instance.World.GetChunkSync(item2) == null)
				{
					flag = true;
					break;
				}
			}
			Thread.Sleep(300);
		}
		while (flag);
		Log.Out("Generate chunks: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds));
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		for (int k = 0; k < 1025; k++)
		{
			for (int l = 0; l < 1025; l++)
			{
				int num4 = (xs + k) * num;
				int num5 = (zs + l) * num;
				long key = WorldChunkCache.MakeChunkKey(num4 / 16, num5 / 16);
				Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(key);
				int x = World.toBlockXZ(num4);
				int z = World.toBlockXZ(num5);
				BiomeDefinition biome = world.Biomes.GetBiome(chunk.GetBiomeId(x, z));
				biomeDefs[k + l * 1025] = biome;
				heights[k + l * 1025] = chunk.GetHeight(x, z);
				blockValues[k + l * 1025] = chunk.GetBlock(x, heights[k + l * 1025], z);
				sbyte density = chunk.GetDensity(x, heights[k + l * 1025], z);
				densitySub[k + l * 1025] = 1f - (float)density / -128f;
				subbiome[k + l * 1025] = biome.m_Id;
			}
		}
		Log.Out("Cache data: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds));
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		for (int m = 0; m < 1024; m++)
		{
			for (int n = 0; n < 1024; n++)
			{
				BiomeDefinition biomeDefinition = biomeDefs[m + n * 1025];
				if (biomeDefinition == null || biomeDefinition.m_Layers.Count == 0)
				{
					continue;
				}
				int num6 = heights[m + n * 1025];
				_ = subbiome[m + n * 1025];
				_ = biomeIntens[m + n * 1025];
				BlockValue blockValue = blockValues[m + n * 1025];
				Vector3 vector = Vector3.up;
				Color color = BlockLiquidv2.Color;
				bool num7 = world.IsWater(m, num6, n);
				if (!num7)
				{
					vector = calcNormal(m, n, 0, 1, 1, 0);
					if (m > 0 && n > 0)
					{
						Vector3 vector2 = calcNormal(m, n, 0, -1, -1, 0);
						Vector3 vector3 = calcNormal(m, n, 1, 1, 1, -1);
						Vector3 vector4 = calcNormal(m, n, -1, -1, -1, 1);
						vector = ((vector + vector2 + vector3 + vector4) * 0.25f).normalized;
					}
				}
				vector = new Vector3(vector.z, vector.x, vector.y);
				normalArray[m + n * 1024] = new Color((vector.x + 1f) / 2f, (vector.y + 1f) / 2f, (vector.z + 1f) / 2f, 1f);
				if (!num7)
				{
					color = blockValue.Block.GetMapColor(blockValue, vector, num6);
				}
				colArray[m + n * 1024] = color;
				colArray[m + n * 1024] = new Color(colArray[m + n * 1024].r, colArray[m + n * 1024].g, colArray[m + n * 1024].b, 1f);
			}
		}
		Log.Out("Create image: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds));
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		Texture2D texture2D = new Texture2D(1024, 1024);
		texture2D.SetPixels(colArray);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, "Map.png");
		Object.Destroy(texture2D);
		GCUtils.UnloadAndCollectStart();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 calcNormal(int x, int z, int _xAdd1, int _zAdd1, int _xAdd2, int _zAdd2)
	{
		Vector3 up = Vector3.up;
		float y = (float)heights[x + z * 1025] - densitySub[x + z * 1025];
		float y2 = (float)heights[x + _xAdd1 + (z + _zAdd1) * 1025] - densitySub[x + _xAdd1 + (z + _zAdd1) * 1025];
		float y3 = (float)heights[x + _xAdd2 + (z + _zAdd2) * 1025] - densitySub[x + _xAdd2 + (z + _zAdd2) * 1025];
		Vector3 lhs = new Vector3(_xAdd1, y3, _zAdd1) - new Vector3(0f, y, 0f);
		Vector3 rhs = new Vector3(_xAdd2, y2, _zAdd2) - new Vector3(0f, y, 0f);
		return Vector3.Cross(lhs, rhs).normalized;
	}
}
