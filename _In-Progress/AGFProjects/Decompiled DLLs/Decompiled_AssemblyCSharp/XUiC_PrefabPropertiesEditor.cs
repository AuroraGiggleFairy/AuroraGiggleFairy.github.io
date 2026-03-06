using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabPropertiesEditor : XUiController
{
	public enum EPropertiesFrom
	{
		LoadedPrefab,
		FileBrowserSelection
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_PrefabFeatureEditorList> featureLists = new List<XUiC_PrefabFeatureEditorList>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxDifficultyTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtThemeRepeatDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtDuplicateRepeatDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public EPropertiesFrom propertiesFrom;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	public Prefab Prefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return prefab;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != prefab)
			{
				prefab = value;
				for (int i = 0; i < featureLists.Count; i++)
				{
					featureLists[i].EditPrefab = value;
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		XUiC_PrefabFeatureEditorList[] childrenByType = GetChildrenByType<XUiC_PrefabFeatureEditorList>();
		foreach (XUiC_PrefabFeatureEditorList xUiC_PrefabFeatureEditorList in childrenByType)
		{
			featureLists.Add(xUiC_PrefabFeatureEditorList);
			xUiC_PrefabFeatureEditorList.FeatureChanged += featureChangedCallback;
		}
		if (GetChildById("cbxDifficultyTier") is XUiC_ComboBoxInt xUiC_ComboBoxInt)
		{
			cbxDifficultyTier = xUiC_ComboBoxInt;
			cbxDifficultyTier.OnValueChanged += CbxDifficultyTier_OnValueChanged;
		}
		if (GetChildById("txtThemeRepeatDistance") is XUiC_TextInput xUiC_TextInput)
		{
			txtThemeRepeatDistance = xUiC_TextInput;
			txtThemeRepeatDistance.OnChangeHandler += TxtThemeRepeatDistance_OnChangeHandler;
		}
		if (GetChildById("txtDuplicateRepeatDistance") is XUiC_TextInput xUiC_TextInput2)
		{
			txtDuplicateRepeatDistance = xUiC_TextInput2;
			txtDuplicateRepeatDistance.OnChangeHandler += TxtDuplicateRepeatDistance_OnChangeHandler;
		}
		((XUiC_SimpleButton)GetChildById("btnSave")).OnPressed += BtnSave_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnOpenInEditor")).OnPressed += BtnOpenInEditor_OnOnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void featureChangedCallback(XUiC_PrefabFeatureEditorList _list, string _featureName, bool _selected)
	{
		if (propertiesFrom == EPropertiesFrom.LoadedPrefab)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOpenInEditor_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		Process.Start(prefab.location.FullPathNoExtension + ".xml");
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (propertiesFrom == EPropertiesFrom.FileBrowserSelection)
		{
			prefab.SaveXMLData(prefab.location);
			PrefabEditModeManager.Instance.LoadXml(prefab.location);
		}
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (cbxDifficultyTier != null)
		{
			cbxDifficultyTier.Value = Prefab.DifficultyTier;
		}
		if (txtThemeRepeatDistance != null)
		{
			txtThemeRepeatDistance.Text = Prefab.ThemeRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		if (txtDuplicateRepeatDistance != null)
		{
			txtDuplicateRepeatDistance.Text = Prefab.DuplicateRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxDifficultyTier_OnValueChanged(XUiController _sender, long _oldvalue, long _newvalue)
	{
		byte b = (byte)_newvalue;
		if (Prefab.DifficultyTier != b)
		{
			Prefab.DifficultyTier = b;
			if (propertiesFrom == EPropertiesFrom.LoadedPrefab)
			{
				PrefabEditModeManager.Instance.NeedsSaving = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtThemeRepeatDistance_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiC_TextInput xUiC_TextInput = (XUiC_TextInput)_sender;
		if (_text.Length < 1)
		{
			xUiC_TextInput.Text = "0";
		}
		else if (_text.Length > 1 && _text[0] == '0')
		{
			xUiC_TextInput.Text = _text.Substring(1);
		}
		if (!int.TryParse(xUiC_TextInput.Text, out var result) || result < 0)
		{
			xUiC_TextInput.Text = Prefab.ThemeRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else if (Prefab.ThemeRepeatDistance != result)
		{
			Prefab.ThemeRepeatDistance = result;
			if (propertiesFrom == EPropertiesFrom.LoadedPrefab)
			{
				PrefabEditModeManager.Instance.NeedsSaving = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtDuplicateRepeatDistance_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiC_TextInput xUiC_TextInput = (XUiC_TextInput)_sender;
		if (_text.Length < 1)
		{
			xUiC_TextInput.Text = "0";
		}
		else if (_text.Length > 1 && _text[0] == '0')
		{
			xUiC_TextInput.Text = _text.Substring(1);
		}
		if (!int.TryParse(xUiC_TextInput.Text, out var result) || result < 0)
		{
			xUiC_TextInput.Text = Prefab.DuplicateRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else if (Prefab.DuplicateRepeatDistance != result)
		{
			Prefab.DuplicateRepeatDistance = result;
			if (propertiesFrom == EPropertiesFrom.LoadedPrefab)
			{
				PrefabEditModeManager.Instance.NeedsSaving = true;
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
		prefab = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "fromprefabbrowser"))
		{
			if (_bindingName == "title")
			{
				_value = Localization.Get("xuiPrefabProperties") + ": " + ((prefab != null) ? prefab.PrefabName : "-");
				return true;
			}
			return false;
		}
		_value = (propertiesFrom == EPropertiesFrom.FileBrowserSelection).ToString();
		return true;
	}

	public static void Show(XUi _xui, EPropertiesFrom _from, PathAbstractions.AbstractedLocation _prefabLocation)
	{
		XUiC_PrefabPropertiesEditor childByType = ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID)).Controller.GetChildByType<XUiC_PrefabPropertiesEditor>();
		childByType.propertiesFrom = _from;
		switch (_from)
		{
		case EPropertiesFrom.FileBrowserSelection:
			childByType.Prefab = new Prefab();
			childByType.Prefab.LoadXMLData(_prefabLocation);
			break;
		case EPropertiesFrom.LoadedPrefab:
			childByType.Prefab = PrefabEditModeManager.Instance.VoxelPrefab;
			break;
		}
		_xui.playerUI.windowManager.Open(ID, _bModal: true);
	}
}
