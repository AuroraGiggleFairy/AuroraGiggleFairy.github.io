using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabMarkerList : XUiC_List<XUiC_PrefabMarkerList.PrefabMarkerEntry>
{
	[Preserve]
	public class PrefabMarkerEntry : XUiListEntry<PrefabMarkerEntry>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_PrefabMarkerList parentList;

		public readonly Prefab.Marker marker;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly CachedStringFormatterXuiRgbaColor colorFormatter = new CachedStringFormatterXuiRgbaColor();

		public PrefabMarkerEntry(XUiC_PrefabMarkerList _parentList, Prefab.Marker _marker)
		{
			parentList = _parentList;
			marker = _marker;
		}

		public override int CompareTo(PrefabMarkerEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(marker.GroupName, _otherEntry.marker.GroupName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "groupname":
				_value = marker.GroupName;
				return true;
			case "groupcolor":
				_value = colorFormatter.Format(new Color(marker.GroupColor.r, marker.GroupColor.g, marker.GroupColor.b, 1f));
				return true;
			case "markertype":
				_value = marker.MarkerType.ToString();
				return true;
			case "markersize":
				if (Prefab.Marker.MarkerSizes.Contains(marker.Size))
				{
					_value = ((Prefab.Marker.MarkerSize)Prefab.Marker.MarkerSizes.IndexOf(marker.Size)/*cast due to .constrained prefix*/).ToString();
				}
				else
				{
					_value = Prefab.Marker.MarkerSize.Custom.ToString();
				}
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return marker.GroupName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "groupname":
				_value = string.Empty;
				return true;
			case "markertype":
				_value = string.Empty;
				return true;
			case "groupcolor":
				_value = colorFormatter.Format(Color.clear).ToString();
				return true;
			case "markersize":
				_value = string.Empty;
				return true;
			default:
				return false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> groupsResult = new List<string>();

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return;
		}
		allEntries.Clear();
		foreach (Prefab.Marker pOIMarker in PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers)
		{
			allEntries.Add(new PrefabMarkerEntry(this, pOIMarker));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
