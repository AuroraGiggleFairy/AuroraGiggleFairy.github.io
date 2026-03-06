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

		public byte TriggerLayer;

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
			return TriggerLayer.CompareTo(_otherEntry.TriggerLayer);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GetSelected()
		{
			bool result = false;
			if (parentList.Owner != null)
			{
				if (parentList.Owner.blockTrigger != null || parentList.Owner.TriggerVolume != null)
				{
					if (parentList.IsTriggers)
					{
						if (parentList.Owner.TriggersIndices != null)
						{
							result = parentList.Owner.TriggersIndices.Contains(StringParsers.ParseUInt8(name));
						}
					}
					else if (parentList.Owner != null)
					{
						if (parentList.Owner.TriggeredByIndices != null)
						{
							result = parentList.Owner.TriggeredByIndices.Contains(StringParsers.ParseUInt8(name));
						}
					}
					else if (parentList.SleeperOwner != null && parentList.SleeperOwner.TriggeredByIndices != null)
					{
						result = parentList.SleeperOwner.TriggeredByIndices.Contains(StringParsers.ParseUInt8(name));
					}
				}
			}
			else if (parentList.SleeperOwner != null && !parentList.IsTriggers && parentList.SleeperOwner != null && parentList.SleeperOwner.TriggeredByIndices != null)
			{
				result = parentList.SleeperOwner.TriggeredByIndices.Contains(StringParsers.ParseUInt8(name));
			}
			return result;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = name;
				return true;
			case "selected":
				_value = (GetSelected() ? "true" : "false");
				return true;
			case "assigned":
				_value = "true";
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = string.Empty;
				return true;
			case "selected":
				_value = "false";
				return true;
			case "assigned":
				_value = "false";
				return true;
			default:
				return false;
			}
		}
	}

	public Prefab EditPrefab;

	public XUiC_TriggerProperties Owner;

	public XUiC_WoPropsSleeperVolume SleeperOwner;

	public bool IsTriggers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> groupsResult = new List<string>();

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		groupsResult.Clear();
		if (EditPrefab != null)
		{
			List<byte> triggerLayers = EditPrefab.TriggerLayers;
			for (int i = 0; i < triggerLayers.Count; i++)
			{
				allEntries.Add(new PrefabTriggerEntry(this, triggerLayers[i]));
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
