using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

public class WaterDebugManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct InitializedRenderer
	{
		public long chunkKey;

		public WaterDebugRenderer renderer;
	}

	public struct RendererHandle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public WaterDebugManager manager;

		[PublicizedFrom(EAccessModifier.Private)]
		public long? key;

		public bool IsValid
		{
			get
			{
				if (manager != null)
				{
					return key.HasValue;
				}
				return false;
			}
		}

		public RendererHandle(Chunk _chunk, WaterDebugManager _manager)
		{
			manager = _manager;
			key = _chunk.Key;
		}

		[Conditional("UNITY_EDITOR")]
		public void SetChunkOrigin(Vector3i _origin)
		{
			if (IsValid && manager.activeRenderers.TryGetValue(key.Value, out var value))
			{
				value.SetChunkOrigin(_origin);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void SetWater(int _x, int _y, int _z, float mass)
		{
			if (IsValid && manager.activeRenderers.TryGetValue(key.Value, out var value))
			{
				value.SetWater(_x, _y, _z, mass);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void Reset()
		{
			if (IsValid)
			{
				manager.ReturnRenderer(key.Value);
			}
			manager = null;
			key = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<InitializedRenderer> newRenderers = new ConcurrentQueue<InitializedRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, WaterDebugRenderer> activeRenderers = new Dictionary<long, WaterDebugRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<long> renderersToRemove = new ConcurrentQueue<long>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool RenderingEnabled { get; set; } = true;

	public void InitializeDebugRender(Chunk chunk)
	{
		WaterDebugRenderer waterDebugRenderer = WaterDebugPools.rendererPool.AllocSync(_bReset: true);
		waterDebugRenderer.LoadFromChunk(chunk);
		chunk.AssignWaterDebugRenderer(new RendererHandle(chunk, this));
		newRenderers.Enqueue(new InitializedRenderer
		{
			chunkKey = chunk.Key,
			renderer = waterDebugRenderer
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReturnRenderer(long key)
	{
		renderersToRemove.Enqueue(key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRenderers()
	{
		InitializedRenderer result;
		while (newRenderers.TryDequeue(out result))
		{
			if (activeRenderers.TryGetValue(result.chunkKey, out var value))
			{
				WaterDebugPools.rendererPool.FreeSync(value);
				activeRenderers.Remove(result.chunkKey);
			}
			activeRenderers.Add(result.chunkKey, result.renderer);
		}
		long result2;
		while (renderersToRemove.TryDequeue(out result2))
		{
			if (activeRenderers.TryGetValue(result2, out var value2))
			{
				WaterDebugPools.rendererPool.FreeSync(value2);
				activeRenderers.Remove(result2);
			}
		}
	}

	public void DebugDraw()
	{
		UpdateRenderers();
		if (!RenderingEnabled)
		{
			return;
		}
		foreach (WaterDebugRenderer value in activeRenderers.Values)
		{
			value.Draw();
		}
	}

	public void Cleanup()
	{
		InitializedRenderer result;
		while (newRenderers.TryDequeue(out result))
		{
			WaterDebugPools.rendererPool.FreeSync(result.renderer);
		}
		foreach (WaterDebugRenderer value in activeRenderers.Values)
		{
			WaterDebugPools.rendererPool.FreeSync(value);
		}
		activeRenderers.Clear();
	}
}
