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

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = Name;
				return true;
			case "selected":
			{
				bool flag = false;
				if (parentList.EditPrefab != null)
				{
					flag = parentList.FeatureEnabled(Name);
				}
				_value = (flag ? "true" : "false");
				return true;
			}
			case "assigned":
				_value = "true";
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
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
	public void FeatureListSelectionChanged(XUiC_ListEntry<FeatureEntry> _previousEntry, XUiC_ListEntry<FeatureEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			string name = _newEntry.GetEntry().Name;
			ToggleFeature(name);
			_newEntry.IsDirty = true;
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
