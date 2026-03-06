using System;
using UnityEngine;

public class WaterDebugRenderer : IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int numLayers = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 chunkOrigin = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterDebugRendererLayer[] layers = new WaterDebugRendererLayer[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] activeLayers = new int[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public int numActiveLayers;

	public void SetChunkOrigin(Vector3 _origin)
	{
		chunkOrigin = _origin;
		for (int i = 0; i < numActiveLayers; i++)
		{
			int num = activeLayers[i];
			Vector3 layerOrigin = chunkOrigin + Vector3.up * num * 16f;
			layers[num].SetLayerOrigin(layerOrigin);
		}
	}

	public void SetWater(int _x, int _y, int _z, float mass)
	{
		int layerIndex = _y / 16;
		int y = _y % 16;
		GetOrCreateLayer(layerIndex).SetWater(_x, y, _z, mass);
	}

	public void LoadFromChunk(Chunk chunk)
	{
		SetChunkOrigin(chunk.GetWorldPos());
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					float num = chunk.GetWater(i, k, j).GetMass();
					if (num > 195f)
					{
						SetWater(i, k, j, num);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterDebugRendererLayer GetOrCreateLayer(int layerIndex)
	{
		WaterDebugRendererLayer waterDebugRendererLayer = layers[layerIndex];
		if (waterDebugRendererLayer == null)
		{
			waterDebugRendererLayer = WaterDebugPools.layerPool.AllocSync(_bReset: false);
			Vector3 layerOrigin = chunkOrigin + Vector3.up * layerIndex * 16f;
			waterDebugRendererLayer.SetLayerOrigin(layerOrigin);
			layers[layerIndex] = waterDebugRendererLayer;
			activeLayers[numActiveLayers] = layerIndex;
			numActiveLayers++;
			Array.Sort(activeLayers, 0, numActiveLayers);
		}
		return waterDebugRendererLayer;
	}

	public void Draw()
	{
		for (int i = 0; i < numActiveLayers; i++)
		{
			int num = activeLayers[i];
			layers[num].Draw();
		}
	}

	public void Clear()
	{
		for (int i = 0; i < numActiveLayers; i++)
		{
			int num = activeLayers[i];
			WaterDebugRendererLayer t = layers[num];
			WaterDebugPools.layerPool.FreeSync(t);
			layers[num] = null;
			activeLayers[i] = 0;
		}
		numActiveLayers = 0;
	}

	public void Cleanup()
	{
		Clear();
	}

	public void Reset()
	{
		Clear();
	}
}
