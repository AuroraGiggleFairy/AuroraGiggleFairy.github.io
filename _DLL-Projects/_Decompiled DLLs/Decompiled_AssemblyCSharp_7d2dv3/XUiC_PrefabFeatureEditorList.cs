using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_PrefabFeatureEditorList : XUiC_List<XUiC_PrefabFeatureEditorList.FeatureEntry>
{
	public delegate void FeatureChangedDelegate(XUiC_PrefabFeatureEditorList _list, string _featureName, bool _selected);

	[Preserve]
	public class FeatureEntry : XUiListEntry<FeatureEntry>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_PrefabFeatureEditorList parentList;

		public readonly string Name;

		public FeatureEntry(XUiC_PrefabFeatureEditorList _parentList, string _name)
		{
			parentList = _parentList;
			Name = _name;
		}

		public override int CompareTo(FeatureEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(Name, _otherEntry.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_PrefabFeatureEditorList parentList;

		public override void Init()
		{
			base.Init();
			parentList = (XUiC_PrefabFeatureEditorList)list;
		}

		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			return entryData?.Name ?? "";
		}

		[XuiXmlBinding("selected")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingSelected()
		{
			if (entryData == null)
			{
				return false;
			}
			if (parentList.EditPrefab == null)
			{
				return false;
			}
			return parentList.FeatureEnabled(entryData.Name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput addInput;

	public Prefab EditPrefab;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<string> groupsResult = new List<string>();

	public event FeatureChangedDelegate FeatureChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool FeatureEnabled(string _featureName);

	public override void Init()
	{
		base.Init();
		base.SelectionChanged += FeatureListSelectionChanged;
		addInput = GetChildById("addInput") as XUiC_TextInput;
		if (addInput != null)
		{
			addInput.OnSubmitHandler += OnAddInputSubmit;
		}
		XUiController childById = GetChildById("addButton");
		if (childById != null)
		{
			childById.OnPress += HandleAddEntry;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddEntry(XUiController _sender, int _mouseButton)
	{
		OnAddFeaturePressed(addInput.Text);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnAddInputSubmit(XUiController _sender, string _text)
	{
		OnAddFeaturePressed(addInput.Text);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void AddNewFeature(string _featureName);

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAddFeaturePressed(string _s)
	{
		_s = _s.Trim();
		if (validGroupName(_s))
		{
			AddNewFeature(_s);
			RebuildList();
			addInput.Text = string.Empty;
			this.FeatureChanged?.Invoke(this, _s, _selected: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validGroupName(string _s)
	{
		_s = _s.Trim();
		if (_s.Length > 0 && _s.IndexOf(",", StringComparison.OrdinalIgnoreCase) < 0)
		{
			return !groupsResult.ContainsCaseInsensitive(_s);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ToggleFeature(string _featureName);

	[PublicizedFrom(EAccessModifier.Private)]
	public void FeatureListSelectionChanged(XUiC_List<FeatureEntry> _list, FeatureEntry _previousEntry, FeatureEntry _newEntry)
	{
		if (_newEntry != null)
		{
			string name = _newEntry.Name;
			ToggleFeature(name);
			_newEntry.UiDirty = true;
			this.FeatureChanged?.Invoke(this, name, FeatureEnabled(name));
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void GetSupportedFeatures();

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		groupsResult.Clear();
		GetSupportedFeatures();
		foreach (string item in groupsResult)
		{
			allEntries.Add(new FeatureEntry(this, item));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_PrefabFeatureEditorList()
	{
	}
}
