using System;
using PrefabVolumes;
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

		public readonly SelectionBox Box;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly CachedStringFormatterXuiRgbaColor colorFormatter = new CachedStringFormatterXuiRgbaColor();

		public Marker Marker => Box.UserData as Marker;

		public PrefabMarkerEntry(XUiC_PrefabMarkerList _parentList, SelectionBox _box)
		{
			parentList = _parentList;
			Box = _box;
		}

		public override int CompareTo(PrefabMarkerEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			int num = string.Compare(Marker.GroupName, _otherEntry.Marker.GroupName, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(Box.name, _otherEntry.Box.name, StringComparison.Ordinal);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Marker.GroupName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[XuiXmlBinding("groupname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingGroupName()
		{
			return entryData?.Marker.GroupName ?? "";
		}

		[XuiXmlBinding("markertype")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingMarkerType()
		{
			return entryData?.Marker.MarkerType.ToString() ?? "";
		}

		[XuiXmlBinding("groupcolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingGroupColor()
		{
			if (entryData == null)
			{
				return "0,0,0,0";
			}
			Color groupColor = entryData.Marker.GroupColor;
			groupColor.a = 1f;
			return XUiUtils.ToXuiColorString(groupColor);
		}

		[XuiXmlBinding("markersize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingMarkerSize()
		{
			if (entryData == null)
			{
				return "";
			}
			int num = Marker.MarkerSizes.IndexOf(entryData.Marker.size);
			if (num >= 0)
			{
				return ((Marker.MarkerSize)num/*cast due to .constrained prefix*/).ToString();
			}
			return Marker.MarkerSize.Custom.ToString();
		}
	}

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
		foreach (SelectionBox registeredBox in POIMarkerToolManager.GetRegisteredBoxes())
		{
			allEntries.Add(new PrefabMarkerEntry(this, registeredBox));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public bool SelectByMarker(Marker _marker)
	{
		if (_marker == null)
		{
			return false;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i].Marker == _marker)
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}
}
