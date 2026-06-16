using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnersList : XUiC_List<XUiC_SpawnersList.SpawnerEntry>
{
	[Preserve]
	public class SpawnerEntry : XUiListEntry<SpawnerEntry>
	{
		public readonly string name;

		public readonly string displayName;

		public SpawnerEntry(string _name)
		{
			name = _name;
			displayName = GameStageGroup.MakeDisplayName(_name);
		}

		public override int CompareTo(SpawnerEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(name, _otherEntry.name, StringComparison.OrdinalIgnoreCase);
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
			return entryData?.displayName ?? "";
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (KeyValuePair<string, GameStageGroup> group in GameStageGroup.Groups)
		{
			string key = group.Key;
			allEntries.Add(new SpawnerEntry(key));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
