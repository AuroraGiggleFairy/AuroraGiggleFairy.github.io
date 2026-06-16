using System;

public static class DynamicMeshSettings
{
	public static int MaxRegionMeshData;

	public static int MaxRegionLoadMsPerFrame;

	public static int MaxDyMeshData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _maxViewDistance;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool UseImposterValues { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool OnlyPlayerAreas { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static int PlayerAreaChunkBuffer { get; set; }

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
	public static bool NewWorldFullRegen { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	static DynamicMeshSettings()
	{
		MaxRegionMeshData = 1;
		MaxRegionLoadMsPerFrame = 2;
		MaxDyMeshData = 3;
		UseImposterValues = true;
		OnlyPlayerAreas = false;
		PlayerAreaChunkBuffer = 3;
		_maxViewDistance = 1000;
		NewWorldFullRegen = false;
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.DynamicMeshDistance)
		{
			MaxViewDistance = GamePrefs.GetInt(_pref);
		}
	}

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
