using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class SignPrioritizer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct Group
	{
		public GlobalSignId SignId;

		public float MinDistanceSquared;

		public int TargetTier;

		public int CanvasStart;

		public int CanvasCount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct CanvasRef
	{
		public SignCanvas Canvas;

		public float DistanceSquared;

		public int GroupIndex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Group> _groups = new List<Group>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<CanvasRef> _canvasRefs = new List<CanvasRef>(1024);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<CanvasRef> _canvasRefsPacked = new List<CanvasRef>(1024);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GlobalSignId, int> _groupIndexById = new Dictionary<GlobalSignId, int>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _groupOrder = new List<int>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _availableSlots = new List<int>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _groupCounts = new List<int>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> _groupOffsets = new List<int>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Comparison<int> _compareGroupOrder;

	public SignPrioritizer()
	{
		_compareGroupOrder = CompareGroupOrder;
	}

	public void Clear()
	{
		_groups.Clear();
		_canvasRefs.Clear();
		_canvasRefsPacked.Clear();
		_groupIndexById.Clear();
		_groupOrder.Clear();
		_availableSlots.Clear();
		_groupCounts.Clear();
		_groupOffsets.Clear();
	}

	public void RebuildRequests(HashSet<SignCanvas> canvases, Vector3 playerPos, SignTextureManager.TierSpec[] tiers, SignTextureStore store, List<SignBakeRequest> outRequests)
	{
		outRequests.Clear();
		if (tiers.Length == 0)
		{
			Clear();
			return;
		}
		BuildGroups(canvases, playerPos);
		PackCanvasRefs();
		SortGroupsByDistance();
		AssignTiersAndEmitRequests(tiers, store, outRequests);
		outRequests.Sort();
	}

	public GlobalSignId GetGroupSignId(int groupIndex)
	{
		return _groups[groupIndex].SignId;
	}

	public void ApplyToGroupCanvases(int groupIndex, RenderTexture rt)
	{
		ApplyToCanvases(groupIndex, rt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildGroups(HashSet<SignCanvas> canvases, Vector3 playerPos)
	{
		_groups.Clear();
		_canvasRefs.Clear();
		_groupIndexById.Clear();
		_groupOrder.Clear();
		foreach (SignCanvas canvase in canvases)
		{
			if (canvase == null)
			{
				continue;
			}
			GlobalSignId displaySignId = canvase.DisplaySignId;
			if (!displaySignId.IsValid)
			{
				continue;
			}
			float sqrMagnitude = (canvase.GetWorldPosition() - playerPos).sqrMagnitude;
			sqrMagnitude /= canvase.SizeSquared;
			if (!_groupIndexById.TryGetValue(displaySignId, out var value))
			{
				value = _groups.Count;
				_groupIndexById.Add(displaySignId, value);
				Group item = new Group
				{
					SignId = displaySignId,
					MinDistanceSquared = sqrMagnitude,
					TargetTier = -1,
					CanvasStart = 0,
					CanvasCount = 0
				};
				_groups.Add(item);
				_groupOrder.Add(value);
			}
			else
			{
				Group value2 = _groups[value];
				if (sqrMagnitude < value2.MinDistanceSquared)
				{
					value2.MinDistanceSquared = sqrMagnitude;
				}
				_groups[value] = value2;
			}
			_canvasRefs.Add(new CanvasRef
			{
				Canvas = canvase,
				DistanceSquared = sqrMagnitude,
				GroupIndex = value
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PackCanvasRefs()
	{
		int count = _groups.Count;
		EnsureListSize(_groupCounts, count);
		EnsureListSize(_groupOffsets, count);
		for (int i = 0; i < count; i++)
		{
			_groupCounts[i] = 0;
		}
		for (int j = 0; j < _canvasRefs.Count; j++)
		{
			_groupCounts[_canvasRefs[j].GroupIndex]++;
		}
		int num = 0;
		for (int k = 0; k < count; k++)
		{
			_groupOffsets[k] = num;
			Group value = _groups[k];
			value.CanvasStart = num;
			value.CanvasCount = _groupCounts[k];
			_groups[k] = value;
			num += _groupCounts[k];
		}
		EnsureListSize(_canvasRefsPacked, num);
		for (int l = 0; l < _canvasRefs.Count; l++)
		{
			CanvasRef value2 = _canvasRefs[l];
			int index = _groupOffsets[value2.GroupIndex]++;
			_canvasRefsPacked[index] = value2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CompareGroupOrder(int a, int b)
	{
		return _groups[a].MinDistanceSquared.CompareTo(_groups[b].MinDistanceSquared);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortGroupsByDistance()
	{
		_groupOrder.Sort(_compareGroupOrder);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AssignTiersAndEmitRequests(SignTextureManager.TierSpec[] tiers, SignTextureStore store, List<SignBakeRequest> outRequests)
	{
		_availableSlots.Clear();
		for (int i = 0; i < tiers.Length; i++)
		{
			_availableSlots.Add(tiers[i].ActiveCount);
		}
		int num = _availableSlots.Count - 1;
		for (int j = 0; j < _groupOrder.Count; j++)
		{
			int num2 = _groupOrder[j];
			Group value = _groups[num2];
			value.TargetTier = num;
			_groups[num2] = value;
			for (int k = 0; k <= num; k++)
			{
				if (store.Has(value.SignId, k))
				{
					store.MarkInUse(value.SignId, k);
				}
				else
				{
					outRequests.Add(new SignBakeRequest(num2, k, value.MinDistanceSquared));
				}
				_availableSlots[k]--;
			}
			RenderTexture bestUpToTier = store.GetBestUpToTier(value.SignId, num);
			ApplyToCanvases(num2, bestUpToTier);
			if (num >= 0 && _availableSlots[num] <= 0)
			{
				num--;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyToCanvases(int groupIndex, RenderTexture rt)
	{
		Group obj = _groups[groupIndex];
		int canvasStart = obj.CanvasStart;
		int num = canvasStart + obj.CanvasCount;
		for (int i = canvasStart; i < num; i++)
		{
			_canvasRefsPacked[i].Canvas.SetTexture(rt);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void EnsureListSize<T>(List<T> list, int size)
	{
		while (list.Count < size)
		{
			list.Add(default(T));
		}
	}

	public void WriteGroupInfo(StringBuilder sb, Vector3 playerPos, SignTextureManager.TierSpec[] activeTiers)
	{
		int count = _groups.Count;
		sb.AppendLine("**************************************************");
		sb.AppendLine($"GroupInfo ({count} groups)");
		sb.AppendLine("**************************************************");
		int num = 0;
		Dictionary<int, HashSet<Texture>> dictionary = new Dictionary<int, HashSet<Texture>>();
		for (int i = 0; i < _groupOrder.Count; i++)
		{
			int num2 = _groupOrder[i];
			Group obj = _groups[num2];
			int canvasStart = obj.CanvasStart;
			int num3 = canvasStart + obj.CanvasCount;
			sb.AppendLine();
			sb.AppendLine($"--- Group {num2} ---");
			sb.AppendLine($"Sign ID: {obj.SignId}");
			if (SignDataManager.Instance.TryGetSignData(obj.SignId, out var signData))
			{
				sb.AppendLine("Sign Name: " + signData.name);
			}
			sb.AppendLine($"MinDistanceSquared: {obj.MinDistanceSquared}");
			sb.AppendLine($"TargetTier: {obj.TargetTier}");
			sb.AppendLine($"CanvasCount: {obj.CanvasCount}");
			sb.AppendLine($"CanvasStart: {obj.CanvasStart}");
			for (int j = canvasStart; j < num3; j++)
			{
				sb.AppendLine();
				sb.AppendLine($"\t--- Canvas {j - canvasStart} (index {j}) ---");
				CanvasRef canvasRef = _canvasRefsPacked[j];
				SignCanvas canvas = canvasRef.Canvas;
				if (canvas == null)
				{
					sb.AppendLine("\tNULL");
					continue;
				}
				float sqrMagnitude = (canvas.GetWorldPosition() - playerPos).sqrMagnitude;
				sb.AppendLine($"\tDistanceSquared (recorded): {canvasRef.DistanceSquared}");
				sb.AppendLine($"\tDistanceSquared (current): {sqrMagnitude / canvas.SizeSquared}");
				sb.AppendLine($"\t\tRaw: {sqrMagnitude}");
				sb.AppendLine($"\t\tSizeSquared: {canvas.SizeSquared}");
				Texture bakedTexture = canvas.BakedTexture;
				if (bakedTexture != null)
				{
					int width = bakedTexture.width;
					int resolution = activeTiers[obj.TargetTier].Resolution;
					if (!dictionary.TryGetValue(width, out var value))
					{
						value = (dictionary[width] = new HashSet<Texture>());
					}
					value.Add(bakedTexture);
					if (width < resolution)
					{
						sb.AppendLine($"\tTexture: {width}px [BELOW TARGET: {resolution}px]");
					}
					else
					{
						sb.AppendLine($"\tTexture: {width}px");
					}
				}
				else
				{
					sb.AppendLine("\tTexture: NULL");
				}
				num++;
			}
		}
		sb.AppendLine();
		sb.AppendLine("**************************************************");
		sb.AppendLine("GroupInfo: Canvas-Assigned Texture Counts By Size");
		sb.AppendLine("**************************************************");
		sb.AppendLine();
		foreach (KeyValuePair<int, HashSet<Texture>> item in dictionary)
		{
			int key = item.Key;
			HashSet<Texture> value2 = item.Value;
			sb.AppendLine($"Size {key}: {value2.Count}");
		}
		sb.AppendLine();
		sb.AppendLine("**************************************************");
		sb.AppendLine($"Finished LogGroupInfo ({num} active canvases)");
		sb.AppendLine("**************************************************");
	}
}
