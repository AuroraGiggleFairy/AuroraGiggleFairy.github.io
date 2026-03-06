using System;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabFolderList : XUiC_List<XUiC_PrefabFolderList.PrefabFolderEntry>
{
	public class PrefabFolderEntry : XUiListEntry<PrefabFolderEntry>
	{
		public readonly string Name;

		public readonly string RelativePath;

		public readonly string AbsolutePath;

		public PrefabFolderEntry(string _name, string _relativePath, string _absolutePath)
		{
			Name = _name;
			RelativePath = _relativePath;
			AbsolutePath = _absolutePath;
		}

		public override int CompareTo(PrefabFolderEntry _otherEntry)
		{
			return string.Compare(Name, _otherEntry.Name, StringComparison.Ordinal);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = Name;
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Name.ContainsCaseInsensitive(_searchString);
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
	public PathAbstractions.EAbstractedLocationType locationType = PathAbstractions.EAbstractedLocationType.GameData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mod mod;

	public PathAbstractions.EAbstractedLocationType LocationType
	{
		get
		{
			return locationType;
		}
		set
		{
			if (value != locationType)
			{
				locationType = value;
				RebuildList();
			}
		}
	}

	public Mod Mod
	{
		get
		{
			return mod;
		}
		set
		{
			if (value != mod)
			{
				mod = value;
				RebuildList();
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		string basePath = PathAbstractions.PrefabsSearchPaths.GetBasePath(locationType, mod);
		if (SdDirectory.Exists(basePath))
		{
			string[] directories = SdDirectory.GetDirectories(basePath);
			foreach (string text in directories)
			{
				string fileName = Path.GetFileName(text);
				allEntries.Add(new PrefabFolderEntry(fileName, fileName, text));
			}
			allEntries.Sort();
		}
		allEntries.Insert(0, new PrefabFolderEntry("<Root>", "", basePath));
		base.RebuildList(_resetFilter);
		base.SelectedEntryIndex = 0;
	}

	public bool SelectByName(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return false;
		}
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i].Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}
}
