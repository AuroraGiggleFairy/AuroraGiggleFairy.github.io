using System.Collections.Generic;

public class PrefabCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab> prefabCache = new Dictionary<string, Prefab>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab[]> prefabCacheRotations = new Dictionary<string, Prefab[]>();

	public Prefab GetPrefabRotated(string _name, int _rotation, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipBlockData = false)
	{
		_rotation &= 3;
		lock (prefabCache)
		{
			if (prefabCacheRotations.TryGetValue(_name, out var value))
			{
				if (value[_rotation] != null)
				{
					return value[_rotation];
				}
			}
			else
			{
				value = new Prefab[4];
				prefabCacheRotations[_name] = value;
			}
			Prefab prefab = GetPrefab(_name, _applyMapping, _fixChildblocks && _rotation == 0, _allowMissingBlocks, _skipBlockData);
			if (prefab == null)
			{
				return null;
			}
			if (_rotation > 0)
			{
				prefab = prefab.Clone(_sharedData: true);
				prefab.RotateY(_bLeft: true, _rotation);
			}
			value[_rotation] = prefab;
			return prefab;
		}
	}

	public Prefab GetPrefab(string _name, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipBlockData = false)
	{
		lock (prefabCache)
		{
			if (prefabCache.ContainsKey(_name))
			{
				return prefabCache[_name];
			}
			Prefab prefab = new Prefab();
			if (prefab.Load(_name, _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipBlockData))
			{
				prefabCache[_name] = prefab;
				return prefab;
			}
			return null;
		}
	}

	public void Clear()
	{
		lock (prefabCache)
		{
			prefabCache.Clear();
			prefabCacheRotations.Clear();
		}
	}

	public void CalculateStats(out int basePrefabCount, out int rotatedPrefabsCount, out int basePrefabBytes, out int rotatedPrefabBytes)
	{
		lock (prefabCache)
		{
			basePrefabCount = prefabCache.Count;
			basePrefabBytes = 0;
			foreach (Prefab value in prefabCache.Values)
			{
				basePrefabBytes += value.EstimateOwnedBytes();
			}
			rotatedPrefabsCount = 0;
			rotatedPrefabBytes = 0;
			foreach (Prefab[] value2 in prefabCacheRotations.Values)
			{
				for (int i = 1; i < value2.Length; i++)
				{
					Prefab prefab = value2[i];
					if (prefab != null)
					{
						rotatedPrefabsCount++;
						rotatedPrefabBytes += prefab.EstimateOwnedBytes();
					}
				}
			}
		}
	}
}
