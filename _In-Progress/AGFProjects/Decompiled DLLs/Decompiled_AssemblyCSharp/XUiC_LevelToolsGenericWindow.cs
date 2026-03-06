using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelToolsGenericWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<(XUiC_ToggleButton toggle, NGuiAction action)> toggleList = new List<(XUiC_ToggleButton, NGuiAction)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<(XUiC_SimpleButton button, NGuiAction action)> buttonList = new List<(XUiC_SimpleButton, NGuiAction)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool blockListsInitDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown dropdownHighlightBlockName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtOldBlockId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtNewBlockId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnReplaceBlockIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtOldShapeMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtNewShapeMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnReplaceShapeMaterials;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		initGenericButtons();
		initSpecialFeatures();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		onOpenSpecialFeatures();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		updateGenericButtons();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initGenericButtons()
	{
		XUiC_ToggleButton[] childrenByType = GetChildrenByType<XUiC_ToggleButton>();
		foreach (XUiC_ToggleButton xUiC_ToggleButton in childrenByType)
		{
			string iD = xUiC_ToggleButton.ViewComponent.ID;
			string label = xUiC_ToggleButton.Label;
			NGuiAction nGuiAction = XUiC_LevelToolsHelpers.BuildAction(iD, label, _forToggle: true);
			if (nGuiAction != null)
			{
				setToggle(xUiC_ToggleButton, nGuiAction);
			}
		}
		XUiC_SimpleButton[] childrenByType2 = GetChildrenByType<XUiC_SimpleButton>();
		foreach (XUiC_SimpleButton xUiC_SimpleButton in childrenByType2)
		{
			string iD2 = xUiC_SimpleButton.ViewComponent.ID;
			string text = xUiC_SimpleButton.Text;
			NGuiAction nGuiAction2 = XUiC_LevelToolsHelpers.BuildAction(iD2, text, _forToggle: false);
			if (nGuiAction2 != null)
			{
				setButton(xUiC_SimpleButton, nGuiAction2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGenericButtons()
	{
		foreach (var toggle in toggleList)
		{
			XUiC_ToggleButton item = toggle.toggle;
			NGuiAction item2 = toggle.action;
			item.Value = item2.IsChecked();
			item.Enabled = item2.IsEnabled();
		}
		foreach (var button in buttonList)
		{
			XUiC_SimpleButton item3 = button.button;
			NGuiAction item4 = button.action;
			item3.Enabled = item4.IsEnabled();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToggle(XUiC_ToggleButton _toggle, NGuiAction _action)
	{
		_toggle.Label = buildCaption(_action);
		_toggle.OnValueChanged += [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ToggleButton _, bool _) =>
		{
			_action.OnClick();
		};
		_toggle.Tooltip = _action.GetTooltip();
		toggleList.Add((_toggle, _action));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setButton(XUiC_SimpleButton _button, NGuiAction _action)
	{
		_button.Text = buildCaption(_action);
		_button.OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
		{
			_action.OnClick();
		};
		_button.Tooltip = _action.GetTooltip();
		buttonList.Add((_button, _action));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string buildCaption(NGuiAction _action)
	{
		return _action.GetText() + " " + _action.GetHotkey().GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.KeyboardWithParentheses);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initSpecialFeatures()
	{
		initBlockHighlighter();
		initBlockReplacer();
		initShapeMaterialReplacer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onOpenSpecialFeatures()
	{
		List<string> list = null;
		if (!blockListsInitDone)
		{
			list = new List<string>();
			Block[] list2 = Block.list;
			foreach (Block block in list2)
			{
				if (block != null)
				{
					list.Add(block.GetBlockName());
				}
			}
			blockListsInitDone = true;
		}
		onOpenBlockHighlighter(list);
		onOpenBlockReplacer(list);
		onOpenShapeMaterialReplacer(list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initBlockHighlighter()
	{
		dropdownHighlightBlockName = GetChildById("txtHighlightBlockName") as XUiC_DropDown;
		if (dropdownHighlightBlockName != null)
		{
			blockListsInitDone = false;
			dropdownHighlightBlockName.OnChangeHandler += HighlightBlock_OnChangeHandler;
			dropdownHighlightBlockName.OnSubmitHandler += HighlightBlock_OnSubmitHandler;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onOpenBlockHighlighter(List<string> _allBlockNames)
	{
		if (_allBlockNames != null && dropdownHighlightBlockName != null)
		{
			dropdownHighlightBlockName.AllEntries.AddRange(_allBlockNames);
			dropdownHighlightBlockName.UpdateFilteredList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HighlightBlock_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		bool flag = Block.GetBlockByName(dropdownHighlightBlockName.Text, _caseInsensitive: true) != null;
		dropdownHighlightBlockName.TextInput.ActiveTextColor = (flag ? Color.white : Color.red);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HighlightBlock_OnSubmitHandler(XUiController _sender, string _text)
	{
		Block blockByName = Block.GetBlockByName(dropdownHighlightBlockName.Text, _caseInsensitive: true);
		if (blockByName != null)
		{
			PrefabEditModeManager.Instance.HighlightBlocks(blockByName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initBlockReplacer()
	{
		txtOldBlockId = GetChildById("txtOldBlockId") as XUiC_DropDown;
		txtNewBlockId = GetChildById("txtNewBlockId") as XUiC_DropDown;
		if (txtOldBlockId != null)
		{
			blockListsInitDone = false;
			txtOldBlockId.OnChangeHandler += ReplaceBlockIds_OnChangeHandler;
			txtOldBlockId.OnSubmitHandler += ReplaceBlockIds_OnSubmitHandler;
			txtOldBlockId.TextInput.SelectOnTab = txtNewBlockId?.TextInput;
		}
		if (txtNewBlockId != null)
		{
			blockListsInitDone = false;
			txtNewBlockId.OnChangeHandler += ReplaceBlockIds_OnChangeHandler;
			txtNewBlockId.OnSubmitHandler += ReplaceBlockIds_OnSubmitHandler;
			txtNewBlockId.TextInput.SelectOnTab = txtOldBlockId?.TextInput;
		}
		btnReplaceBlockIds = GetChildById("btnReplaceBlockIds") as XUiC_SimpleButton;
		if (btnReplaceBlockIds != null)
		{
			btnReplaceBlockIds.OnPressed += BtnReplaceBlockIds_OnPressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onOpenBlockReplacer(List<string> _allBlockNames)
	{
		ReplaceBlockIds_OnChangeHandler(this, null, _changefromcode: true);
		if (_allBlockNames != null)
		{
			if (txtOldBlockId != null)
			{
				txtOldBlockId.AllEntries.AddRange(_allBlockNames);
				txtOldBlockId.UpdateFilteredList();
			}
			if (txtNewBlockId != null)
			{
				txtNewBlockId.AllEntries.AddRange(_allBlockNames);
				txtNewBlockId.UpdateFilteredList();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceBlockIds_OnChangeHandler(XUiController _sender, string _text, bool _changefromcode)
	{
		if (txtOldBlockId != null && txtNewBlockId != null && btnReplaceBlockIds != null)
		{
			bool flag = Block.GetBlockByName(txtOldBlockId.Text, _caseInsensitive: true) != null;
			bool flag2 = Block.GetBlockByName(txtNewBlockId.Text, _caseInsensitive: true) != null;
			txtOldBlockId.TextInput.ActiveTextColor = (flag ? Color.white : Color.red);
			txtNewBlockId.TextInput.ActiveTextColor = (flag2 ? Color.white : Color.red);
			btnReplaceBlockIds.Enabled = flag && flag2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceBlockIds_OnSubmitHandler(XUiController _sender, string _text)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnReplaceBlockIds_OnPressed(XUiController _sender, int _mouseButton)
	{
		Block blockByName = Block.GetBlockByName(txtOldBlockId.Text, _caseInsensitive: true);
		Block blockByName2 = Block.GetBlockByName(txtNewBlockId.Text, _caseInsensitive: true);
		if (blockByName != null && blockByName2 != null)
		{
			XUiC_LevelToolsHelpers.ReplaceBlockId(blockByName, blockByName2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initShapeMaterialReplacer()
	{
		txtOldShapeMaterial = GetChildById("txtOldShapeMaterial") as XUiC_DropDown;
		txtNewShapeMaterial = GetChildById("txtNewShapeMaterial") as XUiC_DropDown;
		if (txtOldShapeMaterial != null)
		{
			blockListsInitDone = false;
			txtOldShapeMaterial.OnChangeHandler += ReplaceShapeMaterial_OnChangeHandler;
			txtOldShapeMaterial.OnSubmitHandler += ReplaceShapeMaterial_OnSubmitHandler;
			txtOldShapeMaterial.TextInput.SelectOnTab = txtNewShapeMaterial?.TextInput;
		}
		if (txtNewShapeMaterial != null)
		{
			blockListsInitDone = false;
			txtNewShapeMaterial.OnChangeHandler += ReplaceShapeMaterial_OnChangeHandler;
			txtNewShapeMaterial.OnSubmitHandler += ReplaceShapeMaterial_OnSubmitHandler;
			txtNewShapeMaterial.TextInput.SelectOnTab = txtOldShapeMaterial?.TextInput;
		}
		btnReplaceShapeMaterials = GetChildById("btnReplaceShapeMaterials") as XUiC_SimpleButton;
		if (btnReplaceShapeMaterials != null)
		{
			btnReplaceShapeMaterials.OnPressed += BtnReplaceShapeMaterial_OnPressed;
		}
		if (!(GetChildById("btnReplaceShapeMaterialSwitchInOut")?.ViewComponent is XUiV_Button xUiV_Button))
		{
			return;
		}
		xUiV_Button.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _button) =>
		{
			if (txtOldShapeMaterial != null && txtNewShapeMaterial != null)
			{
				XUiC_DropDown xUiC_DropDown = txtOldShapeMaterial;
				XUiC_DropDown xUiC_DropDown2 = txtNewShapeMaterial;
				string text = txtNewShapeMaterial.Text;
				string text2 = txtOldShapeMaterial.Text;
				string text3 = (xUiC_DropDown.Text = text);
				text3 = (xUiC_DropDown2.Text = text2);
				ReplaceShapeMaterial_OnChangeHandler(this, null, _changefromcode: true);
			}
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onOpenShapeMaterialReplacer(List<string> _allBlockNames)
	{
		ReplaceShapeMaterial_OnChangeHandler(this, null, _changefromcode: true);
		if (_allBlockNames != null)
		{
			HashSet<string> autoShapeMaterials = Block.GetAutoShapeMaterials();
			if (txtOldShapeMaterial != null)
			{
				txtOldShapeMaterial.AllEntries.AddRange(autoShapeMaterials);
				txtOldShapeMaterial.UpdateFilteredList();
			}
			if (txtNewShapeMaterial != null)
			{
				txtNewShapeMaterial.AllEntries.AddRange(autoShapeMaterials);
				txtNewShapeMaterial.UpdateFilteredList();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceShapeMaterial_OnChangeHandler(XUiController _sender, string _text, bool _changefromcode)
	{
		if (txtOldShapeMaterial != null && txtNewShapeMaterial != null && btnReplaceShapeMaterials != null)
		{
			HashSet<string> autoShapeMaterials = Block.GetAutoShapeMaterials();
			bool flag = autoShapeMaterials.Contains(txtOldShapeMaterial.Text);
			bool flag2 = autoShapeMaterials.Contains(txtNewShapeMaterial.Text);
			txtOldShapeMaterial.TextInput.ActiveTextColor = (flag ? Color.white : Color.red);
			txtNewShapeMaterial.TextInput.ActiveTextColor = (flag2 ? Color.white : Color.red);
			btnReplaceShapeMaterials.Enabled = flag && flag2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceShapeMaterial_OnSubmitHandler(XUiController _sender, string _text)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnReplaceShapeMaterial_OnPressed(XUiController _sender, int _mouseButton)
	{
		HashSet<string> autoShapeMaterials = Block.GetAutoShapeMaterials();
		string actualValue = txtOldShapeMaterial.Text;
		string actualValue2 = txtNewShapeMaterial.Text;
		if (autoShapeMaterials.TryGetValue(actualValue, out actualValue) && autoShapeMaterials.TryGetValue(actualValue2, out actualValue2))
		{
			XUiC_LevelToolsHelpers.ReplaceBlockShapeMaterials(actualValue, actualValue2);
		}
	}
}
