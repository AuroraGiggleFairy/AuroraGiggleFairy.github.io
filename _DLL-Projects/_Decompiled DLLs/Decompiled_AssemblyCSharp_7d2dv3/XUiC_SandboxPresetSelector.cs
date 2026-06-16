using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxPresetSelector : XUiController
{
	[XuiBindComponent("cbxPresetGroup", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<SandboxPresetGroupData> cbxPresetGroups;

	[XuiBindComponent("cbxPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<SandboxPresetInfo> cbxPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxPresetDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplate;

	[XuiXmlBinding("description")]
	public string Description
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxPreset?.Value.Description ?? "";
		}
	}

	[XuiXmlBinding("icon")]
	public string Icon
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxPreset?.Value.Icon ?? "";
		}
	}

	[XuiXmlBinding("difficulty")]
	public int Difficulty
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxPreset?.Value.Difficulty ?? 0;
		}
	}

	[XuiXmlBinding("userpreset")]
	public bool IsUserPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxPreset?.Value.IsUserPreset ?? false;
		}
	}

	[XuiXmlBinding("custompreset")]
	public bool IsCustomPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxPreset?.Value.IsCustomPreset ?? false;
		}
	}

	public SandboxPresetInfo SelectedPreset => cbxPreset.Value;

	public event SandboxPresetSelectionChangedDelegate SandboxPresetSelectionChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePresetGroups(bool _allowCustom = false)
	{
		string groupName = cbxPresetGroups.Value.InternalName ?? "";
		cbxPresetGroups.Elements.Clear();
		foreach (string allPresetGroup in sandboxManager.GetAllPresetGroups())
		{
			if (!(allPresetGroup == "Custom") && sandboxManager.GetPresetsForGroup(allPresetGroup))
			{
				SandboxPresetGroupData item = new SandboxPresetGroupData(allPresetGroup, Localization.Get(allPresetGroup));
				cbxPresetGroups.Elements.Add(item);
			}
		}
		if (_allowCustom)
		{
			cbxPresetGroups.Elements.Add(new SandboxPresetGroupData("Custom", Localization.Get("Custom")));
		}
		selectGroupByName(groupName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePresetsForSelectedGroup(string _selectPreset = null, bool _forceChangedCallback = false)
	{
		string text = cbxPresetGroups.Value.InternalName ?? "";
		SandboxPresetInfo oldSelectedPreset = cbxPreset.Value;
		cbxPreset.Elements.Clear();
		foreach (SandboxOptionPreset sandboxPreset in sandboxManager.SandboxPresets)
		{
			if (!(sandboxPreset.Group != text))
			{
				cbxPreset.Elements.Add(new SandboxPresetInfo(sandboxPreset));
			}
		}
		if (_selectPreset != null)
		{
			cbxPreset.SelectedIndex = cbxPreset.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (SandboxPresetInfo _data) => _data.InternalName == _selectPreset);
		}
		else
		{
			cbxPreset.SelectedIndex = cbxPreset.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (SandboxPresetInfo _data) => _data.Equals(oldSelectedPreset));
		}
		if (_forceChangedCallback || !cbxPreset.Value.Equals(oldSelectedPreset))
		{
			CbxPreset_OnValueChanged(this, oldSelectedPreset, cbxPreset.Value);
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxPresetGroups")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPresetGroups_OnValueChanged(XUiController _sender, SandboxPresetGroupData _oldValue, SandboxPresetGroupData _newValue)
	{
		updatePresetsForSelectedGroup();
		IsDirty = true;
	}

	[XuiBindEvent("OnValueChanged", "cbxPreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPreset_OnValueChanged(XUiController _sender, SandboxPresetInfo _oldValue, SandboxPresetInfo _newValue)
	{
		IsDirty = true;
		this.SandboxPresetSelectionChanged?.Invoke(_oldValue, _newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectGroupByName(string _groupName)
	{
		cbxPresetGroups.SelectedIndex = cbxPresetGroups.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (SandboxPresetGroupData _data) => _data.InternalName == _groupName);
	}

	public void SelectPresetByName(string _presetName, string _tryForceGroup = null, bool _forceChangedCallback = false)
	{
		SandboxOptionPreset sandboxOptionPreset = sandboxManager.GetPreset(_presetName);
		string groupName;
		if (sandboxOptionPreset != null)
		{
			groupName = sandboxOptionPreset.Group;
		}
		else
		{
			sandboxOptionPreset = sandboxManager.GetDefaultPreset();
			groupName = _tryForceGroup ?? sandboxOptionPreset.Group;
		}
		updatePresetGroups(sandboxOptionPreset.IsCustomPreset);
		selectGroupByName(groupName);
		updatePresetsForSelectedGroup(sandboxOptionPreset.Name, _forceChangedCallback);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updatePresetGroups();
		updatePresetsForSelectedGroup();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}
}
