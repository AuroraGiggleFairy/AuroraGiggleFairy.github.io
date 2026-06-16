using System.Collections.Generic;

public static class ChunkResetCommandHelpers
{
	public static bool TryParseProtectionMode(List<string> _params, out ChunkProtectionLevel _protection, out int _pmode)
	{
		_protection = ChunkProtectionLevel.All;
		_pmode = 0;
		int num = FindFlag(_params, "-p", "-pmode");
		if (num < 0)
		{
			return true;
		}
		if (num + 1 >= _params.Count || !int.TryParse(_params[num + 1], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid argument: '" + _params[num] + "' requires an integer value.");
			return false;
		}
		switch (result)
		{
		case 0:
			_protection = ChunkProtectionLevel.All;
			break;
		case 1:
			_protection = ~ChunkProtectionLevel.CurrentlySynced;
			break;
		case 2:
			_protection = ChunkProtectionLevel.None;
			break;
		case 3:
			_protection = ChunkProtectionLevel.CurrentlySynced;
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Invalid argument: '{result}' is not a supported protection mode value.");
			return false;
		}
		_pmode = result;
		return true;
	}

	public static bool TryParseGroupingMode(List<string> _params, out EnumResetUnprotectedChunksGroupingMode _grouping)
	{
		_grouping = EnumResetUnprotectedChunksGroupingMode.GroupedPOIs;
		int num = FindFlag(_params, "-g", "-gmode");
		if (num < 0)
		{
			return true;
		}
		if (num + 1 >= _params.Count || !int.TryParse(_params[num + 1], out var result) || result < 1 || result > 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid argument: '" + _params[num] + "' requires an integer value of 1, 2, or 3.");
			return false;
		}
		_grouping = (EnumResetUnprotectedChunksGroupingMode)result;
		return true;
	}

	public static bool TryParseRegion(List<string> _params, out int? _regionX, out int? _regionZ)
	{
		_regionX = null;
		_regionZ = null;
		int num = FindFlag(_params, "-r", "-region");
		if (num < 0)
		{
			return true;
		}
		if (num + 2 >= _params.Count || !int.TryParse(_params[num + 1], out var result) || !int.TryParse(_params[num + 2], out var result2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid argument: '" + _params[num] + "' requires two integer coordinates (x, z).");
			return false;
		}
		_regionX = result;
		_regionZ = result2;
		return true;
	}

	public static void ExecuteReset(ChunkProtectionLevel _protectionMask, EnumResetUnprotectedChunksGroupingMode _groupingMode, int? _regionX, int? _regionZ, string _opName)
	{
		World world = GameManager.Instance.World;
		ChunkCluster chunkCache = world.ChunkCache;
		if (!(chunkCache.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_opName + " failed: ChunkProviderGenerateWorld could not be found for current world instance.");
			return;
		}
		chunkProviderGenerateWorld.MainThreadCacheProtectedPositions();
		HashSetLong hashSetLong = ((_regionX.HasValue && _regionZ.HasValue) ? chunkProviderGenerateWorld.ResetRegion(_regionX.Value, _regionZ.Value, _protectionMask, _groupingMode) : chunkProviderGenerateWorld.ResetAllChunks(_protectionMask, _groupingMode));
		if ((_protectionMask & ChunkProtectionLevel.CurrentlySynced) == 0)
		{
			HashSetLong hashSetLong2 = new HashSetLong();
			HashSetLong hashSetLong3 = new HashSetLong();
			foreach (long item in hashSetLong)
			{
				if (chunkCache.ContainsChunkSync(item))
				{
					hashSetLong2.Add(item);
				}
			}
			if (hashSetLong2.Count > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Regenerating {hashSetLong2.Count} synced chunks.");
				foreach (long item2 in hashSetLong2)
				{
					if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, item2, _forceRebuild: true))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_opName} failed regenerating chunk at world XZ position: {WorldChunkCache.extractX(item2) << 4}, {WorldChunkCache.extractZ(item2) << 4}");
					}
					else
					{
						hashSetLong3.Add(item2);
					}
				}
				world.m_ChunkManager.ResendChunksToClients(hashSetLong3);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Regeneration complete.");
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_opName} complete. Reset {hashSetLong.Count} chunks.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int FindFlag(List<string> _params, params string[] _flags)
	{
		for (int i = 0; i < _params.Count; i++)
		{
			string text = _params[i].ToLowerInvariant();
			for (int j = 0; j < _flags.Length; j++)
			{
				if (text == _flags[j])
				{
					return i;
				}
			}
		}
		return -1;
	}
}
