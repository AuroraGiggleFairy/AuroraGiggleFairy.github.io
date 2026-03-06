using System;
using System.Collections;
using Backtrace.Unity.Model;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportWindow : XUiController
{
	public static float lastSubmissionTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAttachScreenshot;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAttachSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BugReportSaveSelect saveSelectWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblAttachSaveDescInGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblAttachSaveDescMenu;

	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool fromMainMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool inGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool attachScreenshot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool attachSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.SaveEntryInfo selectedSaveInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uploading;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		windowGroup.isEscClosable = false;
		btnSubmit = GetChildById("btnSubmit").GetChildByType<XUiC_SimpleButton>();
		btnSubmit.OnPressed += BtnSubmitOnPressed;
		GetChildById("btnCancel").GetChildByType<XUiC_SimpleButton>().OnPressed += BtnCancelOnPressed;
		txtDescription = GetChildById("txtDescription") as XUiC_TextInput;
		txtDescription.OnChangeHandler += TxtDescriptionOnChanged;
		comboAttachScreenshot = GetChildById("comboAttachScreenshot") as XUiC_ComboBoxBool;
		comboAttachScreenshot.OnValueChanged += ComboAttachScreenshot_OnValueChanged;
		comboAttachSave = GetChildById("comboAttachSave") as XUiC_ComboBoxBool;
		comboAttachSave.OnValueChanged += ComboAttachSave_OnValueChanged;
		saveSelectWindow = windowGroup.Controller.GetChildByType<XUiC_BugReportSaveSelect>();
		if (saveSelectWindow != null)
		{
			saveSelectWindow.GetChildByType<XUiC_BugReportSavesList>().SelectionChanged += List_SelectionChanged;
		}
		lblAttachSaveDescInGame = GetChildById("lblAttachSaveDescInGame");
		lblAttachSaveDescMenu = GetChildById("lblAttachSaveDescMenu");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void List_SelectionChanged(XUiC_ListEntry<XUiC_BugReportSavesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_BugReportSavesList.ListEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			selectedSaveInfo = _newEntry.GetEntry().saveEntryInfo;
		}
		else
		{
			selectedSaveInfo = null;
		}
		CheckCanSubmit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAttachSave_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		attachSave = _newValue;
		if (!inGame)
		{
			saveSelectWindow.ViewComponent.IsVisible = attachSave;
		}
		if (!attachSave && !inGame)
		{
			selectedSaveInfo = null;
			if (saveSelectWindow != null)
			{
				saveSelectWindow.list.SelectedEntry = null;
			}
		}
		CheckCanSubmit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAttachScreenshot_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		attachScreenshot = _newValue;
	}

	public static void Open(XUi _xui, bool _fromMainMenu)
	{
		_xui.playerUI.windowManager.Open(ID, _bModal: true);
		fromMainMenu = _fromMainMenu;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		inGame = GameManager.Instance.World != null;
		txtDescription.Text = "";
		attachSave = false;
		attachScreenshot = false;
		selectedSaveInfo = null;
		uploading = false;
		comboAttachSave.Value = false;
		comboAttachScreenshot.Value = false;
		lblAttachSaveDescMenu.ViewComponent.IsVisible = BacktraceUtils.BugReportAttachSaveFeature && !inGame;
		lblAttachSaveDescInGame.ViewComponent.IsVisible = BacktraceUtils.BugReportAttachSaveFeature && inGame;
		if (inGame)
		{
			GameManager.Instance.Pause(_bOn: true);
		}
		RefreshBindings();
		CheckCanSubmit();
	}

	public override void OnClose()
	{
		base.OnClose();
		uploading = false;
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = true;
		if (fromMainMenu)
		{
			base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, _bModal: true);
		}
		if (inGame && !fromMainMenu)
		{
			GameManager.Instance.Pause(_bOn: false);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!inGame && saveSelectWindow != null && saveSelectWindow.ViewComponent.IsVisible != attachSave)
		{
			saveSelectWindow.ViewComponent.IsVisible = attachSave;
		}
		else if (inGame && saveSelectWindow != null && saveSelectWindow.ViewComponent.IsVisible)
		{
			saveSelectWindow.ViewComponent.IsVisible = false;
		}
		if (!uploading && (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased || base.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased))
		{
			base.xui.playerUI.windowManager.Close(ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtDescriptionOnChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		CheckCanSubmit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckCanSubmit()
	{
		if (inGame)
		{
			btnSubmit.Enabled = canSubmit && txtDescription.Text.Length > 0;
		}
		else
		{
			btnSubmit.Enabled = canSubmit && txtDescription.Text.Length > 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmitOnPressed(XUiController _sender, int _mouseButton)
	{
		if (!canSubmit || txtDescription.Text.Length <= 0)
		{
			return;
		}
		if (inGame)
		{
			SaveInfoProvider.SaveEntryInfo saveEntryInfo2;
			if (GameManager.Instance.World.IsRemote())
			{
				string guid = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
				if (SaveInfoProvider.Instance.TryGetRemoteSaveEntry(guid, out var saveEntryInfo))
				{
					selectedSaveInfo = saveEntryInfo;
				}
				else
				{
					Log.Error("Could not get save info entry for remote world");
				}
			}
			else if (SaveInfoProvider.Instance.TryGetLocalSaveEntry(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), out saveEntryInfo2))
			{
				selectedSaveInfo = saveEntryInfo2;
			}
			else
			{
				Log.Error("Could not get save info entry for local world");
			}
		}
		lastSubmissionTime = Time.time;
		ThreadManager.StartCoroutine(SubmitRoutine());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SubmitRoutine()
	{
		uploading = true;
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = false;
		string screenshotPath = null;
		if (attachScreenshot)
		{
			base.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			GameManager.Instance.Pause(_bOn: false);
			for (int i = 0; i < 10; i++)
			{
				yield return null;
			}
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(GameUtils.TakeScreenshotEnum(GameUtils.EScreenshotMode.File, PlatformApplicationManager.Application.temporaryCachePath + "/" + Application.productName), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
			{
				Log.Exception(_exception);
			});
			GameManager.Instance.Pause(_bOn: true);
			yield return null;
			base.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			screenshotPath = GameUtils.lastSavedScreenshotFilename;
		}
		yield return null;
		XUiC_ProgressWindow.Open(base.xui.playerUI, Localization.Get("xuiBugReportUploading"), null, _modal: true, _escClosable: false, _closeOpenWindows: false, _useShadow: true);
		yield return new WaitForSecondsRealtime(0.5f);
		BacktraceUtils.SendBugReport(txtDescription.Text, screenshotPath, selectedSaveInfo, attachSave, BugReportCallBack);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BugReportCallBack(BacktraceResult _result)
	{
		Log.Out("Bug Report Send callback: {0}", (_result == null) ? "null" : _result.message);
		HandlePostSubmissionClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePostSubmissionClose()
	{
		XUiC_ProgressWindow.Close(base.xui.playerUI);
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiBugReportHeader"), Localization.Get("xuiBugReportSubmitted"), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (fromMainMenu)
			{
				base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, _bModal: true);
			}
		}, fromMainMenu);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "can_submit":
			canSubmit = Time.time - lastSubmissionTime >= 600f || lastSubmissionTime < 0f;
			_value = canSubmit.ToString();
			return true;
		case "in_game":
			_value = inGame.ToString();
			return true;
		case "attach_saves_enabled":
			_value = BacktraceUtils.BugReportAttachSaveFeature.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}
}
