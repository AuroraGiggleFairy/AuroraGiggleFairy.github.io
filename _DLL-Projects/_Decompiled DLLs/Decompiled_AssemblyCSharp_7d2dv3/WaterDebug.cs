using System.Diagnostics;

public static class WaterDebug
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public static WaterDebugManager Manager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static bool IsAvailable => Manager != null;

	public static bool RenderingEnabled
	{
		get
		{
			return Manager?.RenderingEnabled ?? false;
		}
		set
		{
			if (Manager != null)
			{
				Manager.RenderingEnabled = value;
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void Init()
	{
		if (WaterSimulationNative.Instance.ShouldEnable)
		{
			WaterDebugPools.CreatePools();
			Manager = new WaterDebugManager();
			RenderingEnabled = false;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void InitializeForChunk(Chunk _chunk)
	{
		Manager?.InitializeDebugRender(_chunk);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Draw()
	{
		Manager?.DebugDraw();
	}

	[Conditional("UNITY_EDITOR")]
	public static void Cleanup()
	{
		Manager?.Cleanup();
		WaterDebugPools.Cleanup();
		Manager = null;
	}
}
