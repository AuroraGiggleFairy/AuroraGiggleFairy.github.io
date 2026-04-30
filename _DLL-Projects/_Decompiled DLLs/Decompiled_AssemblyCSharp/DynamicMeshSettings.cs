using System;

public class DynamicMeshSettings
{
	public static int MaxRegionMeshData = 1;

	public static int MaxRegionLoadMsPerFrame = 2;

	public static int MaxDyMeshData = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _maxViewDistance = 1000;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool UseImposterValues { get; set; } = true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool OnlyPlayerAreas { get; set; } = false;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static int PlayerAreaChunkBuffer { get; set; } = 3;

	public static int MaxViewDistance
	{
		get
		{
			return _maxViewDistance;
		}
		set
		{
			_maxViewDistance = Math.Min(3000, value);
			PrefabLODManager.lodPoiDistance = _maxViewDistance;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool NewWorldFullRegen { get; set; } = false;

	public static void LogSettings()
	{
		Log.Out("Dynamic Mesh Settings");
		Log.Out("Use Imposter Values: " + UseImposterValues);
		Log.Out("Only Player Areas: " + OnlyPlayerAreas);
		Log.Out("Player Area Buffer: " + PlayerAreaChunkBuffer);
		Log.Out("Max View Distance: " + MaxViewDistance);
		Log.Out("Regen all on new world: " + NewWorldFullRegen);
	}

	public static void Validate()
	{
	}
}
