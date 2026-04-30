using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GameEventsList : XUiC_List<XUiC_GameEventsList.GameEventEntry>
{
	[Preserve]
	public class GameEventEntry : XUiListEntry<GameEventEntry>
	{
		public readonly string name;

		public readonly string camelCase;

		public GameEventEntry(string _name)
		{
			name = _name;
		}

		public override int CompareTo(GameEventEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(name, _otherEntry.name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = name;
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "";

	public string Category
	{
		get
		{
			return category;
		}
		set
		{
			category = value;
			RebuildList();
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (KeyValuePair<string, GameEventActionSequence> gameEventSequence in GameEventManager.GameEventSequences)
		{
			if (gameEventSequence.Value.AllowUserTrigger && (category == "" || (gameEventSequence.Value.CategoryNames != null && gameEventSequence.Value.CategoryNames.ContainsCaseInsensitive(category))))
			{
				allEntries.Add(new GameEventEntry(gameEventSequence.Key));
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (allEntries.Count == 0)
		{
			RebuildList();
		}
	}
}
