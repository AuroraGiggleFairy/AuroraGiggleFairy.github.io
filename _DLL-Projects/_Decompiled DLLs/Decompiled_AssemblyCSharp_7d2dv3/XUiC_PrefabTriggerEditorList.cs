using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabTriggerEditorList : XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry>
{
	[Preserve]
	public class PrefabTriggerEntry : XUiListEntry<PrefabTriggerEntry>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_PrefabTriggerEditorList parentList;

		public readonly string name;

		public readonly byte TriggerLayer;

		public PrefabTriggerEntry(XUiC_PrefabTriggerEditorList _parentList, byte _triggerLayer)
		{
			parentList = _parentList;
			TriggerLayer = _triggerLayer;
			name = _triggerLayer.ToString();
		}

		public override int CompareTo(PrefabTriggerEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			byte triggerLayer = TriggerLayer;
			return triggerLayer.CompareTo(_otherEntry.TriggerLayer);
		}

		public bool isSelected()
		{
			return (parentList.GetCurrentTriggerIndicesList?.Invoke())?.Contains(TriggerLayer) ?? false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			return entryData?.name ?? "";
		}

		[XuiXmlBinding("selected")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingSelected()
		{
			return entryData?.isSelected() ?? false;
		}
	}

	public Func<List<byte>> GetCurrentPrefabTriggerLayers;

	public Func<List<byte>> GetCurrentTriggerIndicesList;

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		List<byte> list = GetCurrentPrefabTriggerLayers?.Invoke();
		if (list != null)
		{
			foreach (byte item in list)
			{
				allEntries.Add(new PrefabTriggerEntry(this, item));
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
