using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoiList : XUiC_List<XUiC_PoiList.PoiListEntry>
{
	[Preserve]
	public class PoiListEntry : XUiListEntry<PoiListEntry>
	{
		public readonly PrefabInstance prefabInstance;

		public PoiListEntry(PrefabInstance _prefabInstance)
		{
			prefabInstance = _prefabInstance;
		}

		public override int CompareTo(PoiListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			int num = string.Compare(prefabInstance.name, _otherEntry.prefabInstance.name, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			num = prefabInstance.boundingBoxPosition.x - _otherEntry.prefabInstance.boundingBoxPosition.x;
			if (num != 0)
			{
				return num;
			}
			return prefabInstance.boundingBoxPosition.z - _otherEntry.prefabInstance.boundingBoxPosition.z;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = prefabInstance.prefab.PrefabName;
				return true;
			case "localizedname":
				_value = prefabInstance.prefab.LocalizedName;
				return true;
			case "coords":
			{
				Vector3i boundingBoxPosition = prefabInstance.boundingBoxPosition;
				BiomeDefinition biomeInWorld = GameManager.Instance.World.GetBiomeInWorld(boundingBoxPosition.x, boundingBoxPosition.z);
				_value = "(" + ValueDisplayFormatters.WorldPos(boundingBoxPosition.ToVector3()) + ")\n" + biomeInWorld?.LocalizedName;
				return true;
			}
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			Prefab prefab = prefabInstance.prefab;
			if (prefab.PrefabName.ContainsCaseInsensitive(_searchString) || prefab.LocalizedName.ContainsCaseInsensitive(_searchString))
			{
				return true;
			}
			Vector3i boundingBoxPosition = prefabInstance.boundingBoxPosition;
			return GameManager.Instance.World.GetBiomeInWorld(boundingBoxPosition.x, boundingBoxPosition.z)?.LocalizedName.ContainsCaseInsensitive(_searchString) ?? false;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = string.Empty;
				return true;
			case "localizedname":
				_value = string.Empty;
				return true;
			case "coords":
				_value = string.Empty;
				return true;
			default:
				return false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SmallPoiVolumeLimit = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filterSmallPois = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int filterTier = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	public bool FilterSmallPois
	{
		get
		{
			return filterSmallPois;
		}
		set
		{
			if (filterSmallPois != value)
			{
				filterSmallPois = value;
				RebuildList();
			}
		}
	}

	public int FilterTier
	{
		get
		{
			return filterTier;
		}
		set
		{
			if (filterTier != value)
			{
				filterTier = value;
				RebuildList();
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MinTier
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MaxTier
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		if (GameManager.Instance != null)
		{
			List<PrefabInstance> list = GameManager.Instance.GetDynamicPrefabDecorator()?.GetDynamicPrefabs();
			if (list != null)
			{
				foreach (PrefabInstance item in list)
				{
					if (!openedBefore)
					{
						MinTier = Mathf.Min(item.prefab.DifficultyTier, MinTier);
						MaxTier = Mathf.Max(item.prefab.DifficultyTier, MaxTier);
					}
					if ((!filterSmallPois || item.boundingBoxSize.Volume() >= 100) && (filterTier < 0 || item.prefab.DifficultyTier == filterTier))
					{
						allEntries.Add(new PoiListEntry(item));
					}
				}
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!openedBefore || allEntries.Count == 0)
		{
			RebuildList();
		}
		openedBefore = true;
	}
}
