using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public sealed class SignTextureStore
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Pool
	{
		public int Resolution;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LinkedList<RenderTexture> Free = new LinkedList<RenderTexture>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<RenderTexture, LinkedListNode<RenderTexture>> FreeNodes = new Dictionary<RenderTexture, LinkedListNode<RenderTexture>>(128);

		public int Count => FreeNodes.Count;

		public void DestroyTexturesAndClear()
		{
			for (LinkedListNode<RenderTexture> linkedListNode = Free.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				RenderTexture value = linkedListNode.Value;
				if (value != null)
				{
					value.Release();
					Object.DestroyImmediate(value);
				}
			}
			Free.Clear();
			FreeNodes.Clear();
		}

		public bool TryRemove(RenderTexture rt)
		{
			if (FreeNodes.TryGetValue(rt, out var value))
			{
				Free.Remove(value);
				FreeNodes.Remove(rt);
				return true;
			}
			return false;
		}

		public void Add(RenderTexture rt)
		{
			if (!FreeNodes.ContainsKey(rt))
			{
				LinkedListNode<RenderTexture> value = Free.AddLast(rt);
				FreeNodes.Add(rt, value);
			}
		}

		public bool TryUnpool(out RenderTexture rt)
		{
			LinkedListNode<RenderTexture> first = Free.First;
			if (first == null)
			{
				rt = null;
				return false;
			}
			rt = first.Value;
			TryRemove(rt);
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Entry
	{
		public readonly Dictionary<int, RenderTexture> TierToRT = new Dictionary<int, RenderTexture>(8);

		public readonly HashSet<int> InUse = new HashSet<int>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct RtOwner(GlobalSignId signId, int tier)
	{
		public readonly GlobalSignId SignId = signId;

		public readonly int Tier = tier;
	}

	public static readonly ProfilerMarker s_SignTextureManagerRebuildPools = new ProfilerMarker("SignTextureManager.SignTextureStore.RebuildPools");

	public static readonly ProfilerMarker s_SignTextureManagerMarkUnused = new ProfilerMarker("SignTextureManager.SignTextureStore.MarkUnused");

	public static readonly ProfilerMarker s_SignTextureManagerMarkUsed = new ProfilerMarker("SignTextureManager.SignTextureStore.MarkUsed");

	public static readonly ProfilerMarker s_SignTextureManagerHas = new ProfilerMarker("SignTextureManager.SignTextureStore.Has");

	public static readonly ProfilerMarker s_SignTextureManagerGet = new ProfilerMarker("SignTextureManager.SignTextureStore.Get");

	public static readonly ProfilerMarker s_SignTextureManagerGetBestUpToTier = new ProfilerMarker("SignTextureManager.SignTextureStore.GetBestUpToTier");

	public static readonly ProfilerMarker s_SignTextureManagerAcquireForBake = new ProfilerMarker("SignTextureManager.SignTextureStore.AcquireForBake");

	public static readonly ProfilerMarker s_SignTextureManagerSetBaked = new ProfilerMarker("SignTextureManager.SignTextureStore.SetBaked");

	public static readonly ProfilerMarker s_SignTextureManagerInvalidateSign = new ProfilerMarker("SignTextureManager.SignTextureStore.InvalidateSign");

	public static readonly ProfilerMarker s_SignTextureManagerEvictUnused = new ProfilerMarker("SignTextureManager.SignTextureStore.EvictUnused");

	public static readonly ProfilerMarker s_SignTextureManagerClear = new ProfilerMarker("SignTextureManager.SignTextureStore.Clear");

	public static readonly ProfilerMarker s_SignTextureManagerReleaseToPool = new ProfilerMarker("SignTextureManager.SignTextureStore.ReleaseToPool");

	public static readonly ProfilerMarker s_SignTextureManagerEnqueueFreeRT = new ProfilerMarker("SignTextureManager.SignTextureStore.EnqueueFreeRT");

	public static readonly ProfilerMarker s_SignTextureManagerCreateRT = new ProfilerMarker("SignTextureManager.SignTextureStore.CreateRT");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<long> s_TotalTextureMemory = new ProfilerCounterValue<long>(ProfilerCategory.Scripts, "STM Texture Memory", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Pool> _pools = new List<Pool>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GlobalSignId, Entry> _entries = new Dictionary<GlobalSignId, Entry>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<RenderTexture, RtOwner> _ownerByRT = new Dictionary<RenderTexture, RtOwner>(512);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _tiersToRemove = new List<int>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GlobalSignId> _signsToRemove = new List<GlobalSignId>(16);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _bestTierScratch = new List<int>(8);

	public void RebuildPools(IReadOnlyList<SignTextureManager.TierSpec> tiers)
	{
		using (s_SignTextureManagerRebuildPools.Auto())
		{
			Clear();
			_pools.Clear();
			long num = 0L;
			for (int i = 0; i < tiers.Count; i++)
			{
				SignTextureManager.TierSpec tierSpec = tiers[i];
				Pool pool = new Pool();
				pool.Resolution = tierSpec.Resolution;
				_pools.Add(pool);
				int totalCount = tierSpec.TotalCount;
				for (int j = 0; j < totalCount; j++)
				{
					RenderTexture rt = CreateRT(tierSpec.Resolution, tierSpec.Resolution, "SignTech_Pool");
					EnqueueFreeRT(i, rt);
					num += tierSpec.Resolution * tierSpec.Resolution * 4;
				}
			}
			s_TotalTextureMemory.Value = num;
		}
	}

	public void BeginFrameMarkAllNotInUse()
	{
		using (s_SignTextureManagerMarkUnused.Auto())
		{
			foreach (KeyValuePair<GlobalSignId, Entry> entry in _entries)
			{
				entry.Value.InUse.Clear();
			}
		}
	}

	public bool Has(GlobalSignId signId, int tier)
	{
		using (s_SignTextureManagerHas.Auto())
		{
			if (_entries.TryGetValue(signId, out var value))
			{
				return value.TierToRT.ContainsKey(tier);
			}
			return false;
		}
	}

	public void MarkInUse(GlobalSignId signId, int tier)
	{
		using (s_SignTextureManagerMarkUsed.Auto())
		{
			if (_entries.TryGetValue(signId, out var value))
			{
				value.InUse.Add(tier);
				if (value.TierToRT.TryGetValue(tier, out var value2) && !(value2 == null) && (uint)tier < (uint)_pools.Count)
				{
					_pools[tier].TryRemove(value2);
				}
			}
		}
	}

	public RenderTexture Get(GlobalSignId signId, int tier)
	{
		using (s_SignTextureManagerGet.Auto())
		{
			if (_entries.TryGetValue(signId, out var value) && value.TierToRT.TryGetValue(tier, out var value2))
			{
				return value2;
			}
			return null;
		}
	}

	public RenderTexture GetBestUpToTier(GlobalSignId signId, int maxTier)
	{
		using (s_SignTextureManagerGetBestUpToTier.Auto())
		{
			if (!_entries.TryGetValue(signId, out var value))
			{
				return null;
			}
			int num = -1;
			RenderTexture result = null;
			foreach (KeyValuePair<int, RenderTexture> item in value.TierToRT)
			{
				int key = item.Key;
				if (key <= maxTier && key > num)
				{
					num = key;
					result = item.Value;
				}
			}
			return result;
		}
	}

	public RenderTexture AcquireForBake(int tier)
	{
		using (s_SignTextureManagerAcquireForBake.Auto())
		{
			if ((uint)tier >= (uint)_pools.Count)
			{
				return null;
			}
			if (!_pools[tier].TryUnpool(out var rt))
			{
				return null;
			}
			if (_ownerByRT.TryGetValue(rt, out var value))
			{
				if (_entries.TryGetValue(value.SignId, out var value2) && value2.TierToRT.TryGetValue(value.Tier, out var value3) && (object)value3 == rt)
				{
					value2.TierToRT.Remove(value.Tier);
				}
				_ownerByRT.Remove(rt);
			}
			return rt;
		}
	}

	public void SetBaked(GlobalSignId signId, int tier, RenderTexture rt)
	{
		using (s_SignTextureManagerSetBaked.Auto())
		{
			if (!(rt == null))
			{
				if (!_entries.TryGetValue(signId, out var value))
				{
					value = new Entry();
					_entries.Add(signId, value);
				}
				value.TierToRT[tier] = rt;
				value.InUse.Add(tier);
				_ownerByRT[rt] = new RtOwner(signId, tier);
			}
		}
	}

	public void InvalidateSign(GlobalSignId signId)
	{
		using (s_SignTextureManagerInvalidateSign.Auto())
		{
			if (!_entries.TryGetValue(signId, out var value))
			{
				return;
			}
			foreach (KeyValuePair<int, RenderTexture> item in value.TierToRT)
			{
				int key = item.Key;
				RenderTexture value2 = item.Value;
				if (!(value2 == null))
				{
					if ((uint)key < (uint)_pools.Count)
					{
						_pools[key].TryRemove(value2);
					}
					_ownerByRT.Remove(value2);
					EnqueueFreeRT(key, value2);
				}
			}
			_entries.Remove(signId);
		}
	}

	public void EvictUnused()
	{
		using (s_SignTextureManagerEvictUnused.Auto())
		{
			foreach (KeyValuePair<GlobalSignId, Entry> entry in _entries)
			{
				Entry value = entry.Value;
				foreach (KeyValuePair<int, RenderTexture> item in value.TierToRT)
				{
					int key = item.Key;
					RenderTexture value2 = item.Value;
					if (value2 != null && !value.InUse.Contains(key))
					{
						EnqueueFreeRT(key, value2);
					}
				}
			}
		}
	}

	public void Clear()
	{
		using (s_SignTextureManagerClear.Auto())
		{
			foreach (KeyValuePair<GlobalSignId, Entry> entry in _entries)
			{
				foreach (KeyValuePair<int, RenderTexture> item in entry.Value.TierToRT)
				{
					ReleaseToPool(item.Key, item.Value);
				}
			}
			_entries.Clear();
			_ownerByRT.Clear();
			for (int i = 0; i < _pools.Count; i++)
			{
				_pools[i].DestroyTexturesAndClear();
			}
			_pools.Clear();
			_tiersToRemove.Clear();
			_signsToRemove.Clear();
			_bestTierScratch.Clear();
			s_TotalTextureMemory.Value = 0L;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReleaseToPool(int tier, RenderTexture rt)
	{
		using (s_SignTextureManagerReleaseToPool.Auto())
		{
			if (!(rt == null))
			{
				if ((uint)tier >= (uint)_pools.Count)
				{
					rt.Release();
					Object.DestroyImmediate(rt);
				}
				else
				{
					EnqueueFreeRT(tier, rt);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnqueueFreeRT(int tier, RenderTexture rt)
	{
		using (s_SignTextureManagerEnqueueFreeRT.Auto())
		{
			if (!(rt == null))
			{
				if ((uint)tier >= (uint)_pools.Count)
				{
					rt.Release();
					Object.DestroyImmediate(rt);
				}
				else
				{
					_pools[tier].Add(rt);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static RenderTexture CreateRT(int w, int h, string name)
	{
		using (s_SignTextureManagerCreateRT.Auto())
		{
			RenderTexture renderTexture = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			renderTexture.name = name;
			renderTexture.useMipMap = false;
			renderTexture.autoGenerateMips = false;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.filterMode = FilterMode.Bilinear;
			renderTexture.antiAliasing = 1;
			renderTexture.Create();
			return renderTexture;
		}
	}

	public void WriteTextureStoreInfo(StringBuilder sb)
	{
		sb.AppendLine("**************************************************");
		sb.AppendLine($"TextureStore: Pools ({_pools.Count})");
		sb.AppendLine("**************************************************");
		if (_pools.Count > 0)
		{
			for (int i = 0; i < _pools.Count; i++)
			{
				sb.AppendLine();
				sb.AppendLine($"Pool Tier {i}");
				Pool pool = _pools[i];
				if (pool == null)
				{
					sb.AppendLine("\tNULL");
					continue;
				}
				sb.AppendLine($"\tResolution: {pool.Resolution}");
				sb.AppendLine($"\tFree Count: {pool.Count}");
			}
		}
		else
		{
			sb.AppendLine();
			sb.AppendLine("\tNONE");
		}
		sb.AppendLine();
		sb.AppendLine("**************************************************");
		sb.AppendLine($"TextureStore: Entries ({_entries.Count})");
		sb.AppendLine("**************************************************");
		Dictionary<int, List<GlobalSignId>> dictionary = new Dictionary<int, List<GlobalSignId>>();
		Dictionary<int, List<GlobalSignId>> dictionary2 = new Dictionary<int, List<GlobalSignId>>();
		foreach (KeyValuePair<GlobalSignId, Entry> entry in _entries)
		{
			Entry value = entry.Value;
			sb.AppendLine();
			sb.AppendLine($"{entry.Key}");
			bool flag = false;
			for (int j = 0; j < _pools.Count; j++)
			{
				if (!value.TierToRT.ContainsKey(j))
				{
					continue;
				}
				flag = true;
				if (value.InUse.Contains(j))
				{
					sb.AppendLine($"\t{j} (in use)");
					if (!dictionary.TryGetValue(j, out var value2))
					{
						value2 = (dictionary[j] = new List<GlobalSignId>());
					}
					value2.Add(entry.Key);
				}
				else
				{
					sb.AppendLine($"\t{j} (in reserve)");
					if (!dictionary2.TryGetValue(j, out var value3))
					{
						value3 = (dictionary2[j] = new List<GlobalSignId>());
					}
					value3.Add(entry.Key);
				}
			}
			if (!flag)
			{
				sb.AppendLine("\tNONE");
			}
		}
		sb.AppendLine();
		sb.AppendLine("**************************************************");
		sb.AppendLine("TextureStore: IDs By Tier");
		sb.AppendLine("**************************************************");
		for (int k = 0; k < _pools.Count; k++)
		{
			sb.AppendLine();
			sb.AppendLine($"Tier {k}:");
			if (!dictionary.TryGetValue(k, out var value4))
			{
				sb.AppendLine("\t--- In-Use (Count: 0) ---");
				sb.AppendLine("\tNONE");
			}
			else
			{
				sb.AppendLine($"\t--- In-Use (Count: {value4.Count}) ---");
				foreach (GlobalSignId item in value4)
				{
					sb.AppendLine($"\t{item}");
				}
			}
			sb.AppendLine();
			if (!dictionary2.TryGetValue(k, out var value5))
			{
				sb.AppendLine("\t--- Pooled (Count: 0) ---");
				sb.AppendLine("\tNONE");
				continue;
			}
			sb.AppendLine($"\t--- Pooled (Count: {value5.Count}) ---");
			foreach (GlobalSignId item2 in value5)
			{
				sb.AppendLine($"\t{item2}");
			}
		}
		sb.AppendLine();
	}
}
