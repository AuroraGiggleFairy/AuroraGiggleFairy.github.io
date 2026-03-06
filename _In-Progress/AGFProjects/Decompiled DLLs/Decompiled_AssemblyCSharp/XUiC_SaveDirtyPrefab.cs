using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveDirtyPrefab : XUiController
{
	public enum EMode
	{
		AskSaveIfDirty,
		ForceSave
	}

	public enum ESelectedAction
	{
		Save,
		Cancel,
		DontSave
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSaveName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameInvalid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameExists;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<ESelectedAction> onCloseAction;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		((XUiC_SimpleButton)GetChildById("btnSave")).OnPressed += BtnSave_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnDontSave")).OnPressed += BtnDontSave_OnPressed;
		txtSaveName = (XUiC_TextInput)GetChildById("txtSaveName");
		txtSaveName.OnChangeHandler += TxtSaveNameOnOnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSaveNameOnOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (nameRequired)
		{
			nameInvalid = _text.Length <= 0 || _text.Contains(" ") || !GameUtils.ValidateGameName(_text);
			nameExists = !nameInvalid && Prefab.LocationForNewPrefab(_text).Exists();
		}
		else
		{
			nameInvalid = false;
			nameExists = false;
		}
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		CloseWith(ESelectedAction.Save);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		CloseWith(ESelectedAction.Cancel);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDontSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		CloseWith(ESelectedAction.DontSave);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseWith(ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		if (_action == ESelectedAction.Save)
		{
			if (nameRequired)
			{
				string text = txtSaveName.Text;
				PrefabEditModeManager.Instance.VoxelPrefab.location = Prefab.LocationForNewPrefab(text);
			}
			if (PrefabEditModeManager.Instance.SaveVoxelPrefab())
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, string.Format(Localization.Get("xuiPrefabsPrefabSaved"), PrefabEditModeManager.Instance.LoadedPrefab.Name));
			}
			else
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, Localization.Get("xuiPrefabsPrefabSavingError"));
				_action = ESelectedAction.Cancel;
			}
		}
		ThreadManager.StartCoroutine(delayCallback(_action));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayCallback(ESelectedAction _action)
	{
		yield return new WaitForSeconds(0.1f);
		onCloseAction?.Invoke(_action);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtSaveName.Text = "";
		TxtSaveNameOnOnChangeHandler(this, "", _changeFromCode: true);
		IsDirty = true;
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
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		switch (_bindingName)
		{
		case "is_save_new":
			_value = (loadedPrefab.Type == PathAbstractions.EAbstractedLocationType.None).ToString();
			return true;
		case "current_prefab_name":
			_value = ((loadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None) ? loadedPrefab.Name : "");
			return true;
		case "request_name":
			_value = nameRequired.ToString();
			return true;
		case "prefab_name_exists":
			_value = nameExists.ToString();
			return true;
		case "prefab_name_invalid":
			_value = nameInvalid.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public static void Show(XUi _xui, Action<ESelectedAction> _onCloseAction, EMode _mode = EMode.AskSaveIfDirty)
	{
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		switch (_mode)
		{
		case EMode.AskSaveIfDirty:
			if (PrefabEditModeManager.Instance.VoxelPrefab == null || !PrefabEditModeManager.Instance.NeedsSaving)
			{
				_onCloseAction?.Invoke(ESelectedAction.DontSave);
				return;
			}
			break;
		case EMode.ForceSave:
			if (loadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None && Prefab.CanSaveIn(loadedPrefab))
			{
				if (PrefabEditModeManager.Instance.SaveVoxelPrefab())
				{
					GameManager.ShowTooltip(_xui.playerUI.entityPlayer, string.Format(Localization.Get("xuiPrefabsPrefabSaved"), loadedPrefab.Name));
					_onCloseAction?.Invoke(ESelectedAction.Save);
				}
				else
				{
					GameManager.ShowTooltip(_xui.playerUI.entityPlayer, Localization.Get("xuiPrefabsPrefabSavingError"));
					_onCloseAction?.Invoke(ESelectedAction.Cancel);
				}
				return;
			}
			break;
		}
		XUiC_SaveDirtyPrefab childByType = ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID)).Controller.GetChildByType<XUiC_SaveDirtyPrefab>();
		childByType.nameRequired = loadedPrefab.Type == PathAbstractions.EAbstractedLocationType.None || !Prefab.CanSaveIn(loadedPrefab);
		childByType.onCloseAction = _onCloseAction;
		_xui.playerUI.windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true);
	}
}
