using System.Collections.Generic;
using System.Text;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxSettingsDisplay : XUiController
{
	[XuiBindComponent("sandboxPresetValues.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView sandboxPresetValuesScroll;

	[XuiBindParent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView parentScroll;

	[XuiBindComponent("btnCopyCode", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCopyCode;

	[XuiBindComponent("btnSaveAsPreset", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSaveAsPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sandboxValuesColumns;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplateDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandboxCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandboxName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valid;

	[XuiXmlAttribute("sandbox_values_columns", false)]
	public int SandboxValuesColumns
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sandboxValuesColumns;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			sandboxValuesColumns = value;
			IsDirty = true;
		}
	}

	[XuiXmlAttribute("sandbox_values_template", false)]
	public string SandBoxValuesTemplate
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sandBoxValuesTemplate;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				sandBoxValuesTemplate = "";
				IsDirty = true;
			}
			else
			{
				sandBoxValuesTemplate = value.Replace("%0", "{0}").Replace("%1", "{1}");
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("sandbox_values_template_defaults", false)]
	public string SandBoxValuesTemplateDefaults
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sandBoxValuesTemplateDefaults;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				sandBoxValuesTemplateDefaults = "";
				IsDirty = true;
			}
			else
			{
				sandBoxValuesTemplateDefaults = value.Replace("%0", "{0}").Replace("%1", "{1}");
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("sandboxcode", false)]
	[XuiXmlBinding("sandboxcode")]
	public string SandboxCode
	{
		get
		{
			return sandboxCode ?? "";
		}
		set
		{
			if (!string.Equals(value, sandboxCode))
			{
				sandboxName = null;
				sandboxCode = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("sandboxname", false)]
	[XuiXmlBinding("sandboxname")]
	public string SandboxName
	{
		get
		{
			return sandboxName ?? "";
		}
		set
		{
			if (!string.Equals(value, sandboxName))
			{
				sandboxCode = null;
				sandboxName = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("descriptiontemplate", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string SandboxDescriptionTemplate { get; set; }

	[XuiXmlBinding("sandboxcodevalid")]
	public bool Valid
	{
		get
		{
			return valid;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (valid != value)
			{
				valid = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("sandboxvalues")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> SandboxValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	} = new Dictionary<string, string>();

	public override void Update(float _dt)
	{
		base.Update(_dt);
		RefreshBindings();
		if (IsDirty)
		{
			IsDirty = false;
			updateSandboxData();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSandboxData()
	{
		SandboxValues.Clear();
		if (sandboxName == null && sandboxCode == null)
		{
			return;
		}
		SandboxOptionPreset sandboxOptionPreset;
		if (sandboxName != null)
		{
			sandboxOptionPreset = SandboxOptionManager.Current.GetPreset(sandboxName);
			sandboxCode = sandboxOptionPreset.SandboxCode;
		}
		else
		{
			sandboxOptionPreset = new SandboxOptionPreset();
			Valid = sandboxManager.LoadOptionsFromCode(SandboxCode, sandboxOptionPreset);
			if (!valid)
			{
				return;
			}
		}
		if (!sandboxManager.GetChangedPresetOptions(sandboxOptionPreset, out List<(string, string, bool)> valuesList))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < SandboxValuesColumns; i++)
		{
			for (int j = 0; j < valuesList.Count; j++)
			{
				int num = i + j * SandboxValuesColumns;
				if (num >= valuesList.Count)
				{
					break;
				}
				stringBuilder.AppendFormat(valuesList[num].Item3 ? SandBoxValuesTemplateDefaults : SandBoxValuesTemplate, valuesList[num].Item1, valuesList[num].Item2);
			}
			SandboxValues[(i + 1).ToString()] = stringBuilder.ToString();
			stringBuilder.Clear();
		}
		ThreadManager.RunTaskAfterFrames([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (sandboxPresetValuesScroll != null)
			{
				sandboxPresetValuesScroll.ResetPosition();
			}
			else if (parentScroll != null)
			{
				parentScroll.ResetPosition();
			}
		}, 3);
	}

	[XuiBindEvent("OnPress", "btnCopyCode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCopyCode_OnPress(XUiController _sender, int _mouseButton)
	{
		GUIUtility.systemCopyBuffer = SandboxCode;
	}

	[XuiBindEvent("OnPress", "btnSaveAsPreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSaveAsPreset_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_SandboxSettingsSaveAsPreset.Open(LocalPlayerUI.primaryUI.xui, SandboxCode, SandboxDescriptionTemplate);
	}
}
